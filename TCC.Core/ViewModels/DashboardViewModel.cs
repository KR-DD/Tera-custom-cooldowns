﻿using FoglioUtils;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using Newtonsoft.Json;
using TCC.Controls;
using TCC.Controls.Dashboard;
using TCC.Data;
using TCC.Data.Abnormalities;
using TCC.Data.Map;
using TCC.Data.Pc;
using TCC.Parsing;
using TCC.Settings;
using TCC.Windows;

using TeraDataLite;
using TeraPacketParser.Messages;
using MessageBoxImage = TCC.Data.MessageBoxImage;

namespace TCC.ViewModels
{

    [TccModule]
    public class DashboardViewModel : TccWindowViewModel
    {
        /* -- Fields ----------------------------------------------- */

        private bool _discardFirstVanguardPacket = true;
        private ICollectionViewLiveShaping _sortedColumns;
        private ObservableCollection<DungeonColumnViewModel> _columns;
        private Character _selectedCharacter;

        /* -- Properties ------------------------------------------- */

        public Character CurrentCharacter => Game.Account.Characters.FirstOrDefault(x => x.Id == Game.Me.PlayerId);
        public Character SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                if (_selectedCharacter == value) return;
                _selectedCharacter = value;
                N();
            }
        }

        public bool ShowElleonMarks => App.Settings.LastLanguage.Contains("EU");


        public ICollectionViewLiveShaping SortedCharacters { get; }
        public ICollectionViewLiveShaping HiddenCharacters { get; }
        public ICollectionViewLiveShaping SortedColumns
        {
            get
            {
                return _sortedColumns ?? (_sortedColumns = CollectionViewUtils.InitLiveView(o => ((DungeonColumnViewModel)o).Dungeon.Show, Columns,
                                            new[] { $"{nameof(Dungeon)}.{nameof(Dungeon.Show)}", $"{nameof(Dungeon)}.{nameof(Dungeon.Index)}" },
                                            new[] { new SortDescription($"{nameof(Dungeon)}.{nameof(Dungeon.Index)}", ListSortDirection.Ascending) }));
            }
        }
        public ICollectionViewLiveShaping SelectedCharacterInventory { get; set; }
        public ICollectionViewLiveShaping CharacterViewModelsView { get; set; }

        public ObservableCollection<InventoryItem> InventoryViewList
        {
            get
            {
                var ret = new ObservableCollection<InventoryItem>();
                Task.Factory.StartNew(() =>
                {
                    if (SelectedCharacter == null) return;
                    SelectedCharacter.Inventory.ToList().ForEach(item =>
                    {
                        App.BaseDispatcher.BeginInvoke(new Action(() =>
                        {
                            ret.Add(item);
                        }), DispatcherPriority.Background);
                    });
                });
                return ret;
            }
        }



        public int TotalElleonMarks
        {
            get
            {
                var ret = 0;
                Game.Account.Characters.ToSyncList().ForEach(c => ret += c.ElleonMarks);
                return ret;
            }
        }
        public int TotalVanguardCredits
        {
            get
            {
                var ret = 0;
                Game.Account.Characters.ToSyncList().ForEach(c => ret += c.VanguardInfo.Credits);
                return ret;
            }
        }
        public int TotalGuardianCredits
        {
            get
            {
                var ret = 0;
                Game.Account.Characters.ToSyncList().ForEach(c => ret += c.GuardianInfo.Credits);
                return ret;
            }
        }

        public ObservableCollection<CharacterViewModel> CharacterViewModels
        {
            get;
            //{
            //    if (_characters == null) _characters = new ObservableCollection<CharacterViewModel>();
            //    _characters.Clear();
            //    foreach (var o in Characters)
            //    {
            //        _characters.Add(new CharacterViewModel { Character = o });
            //    }
            //    return _characters;
            //}
        }

        private void SyncViewModel(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Character item in e.NewItems)
                    {
                        CharacterViewModels.Add(new CharacterViewModel() { Character = item });
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Character item in e.OldItems)
                    {
                        var target = CharacterViewModels.FirstOrDefault(x => x.Character == item);
                        CharacterViewModels.Remove(target);
                    }
                    break;
            }
        }

        public ObservableCollection<DungeonColumnViewModel> Columns
        {
            get
            {
                if (_columns != null) return _columns;
                _columns = new ObservableCollection<DungeonColumnViewModel>();
                return _columns;
            }
        }
        public RelayCommand LoadDungeonsCommand { get; }
        /* -- Constructor ------------------------------------------ */
        bool _loaded;
        public DashboardViewModel(WindowSettings settings) : base(settings)
        {
            CharacterViewModels = new ObservableCollection<CharacterViewModel>();
            EventGroups = new SynchronizedObservableCollection<EventGroup>();
            Markers = new SynchronizedObservableCollection<TimeMarker>();
            SpecialEvents = new SynchronizedObservableCollection<DailyEvent>();
            LoadDungeonsCommand = new RelayCommand(o =>
            {
                if (_loaded) return;

                Task.Factory.StartNew(() =>
                {
                    Game.DB.DungeonDatabase.Dungeons.Values/*.Where(d => d.HasDef)*/.ToList().ForEach(dungeon =>
                    {
                        App.BaseDispatcher.BeginInvoke(new Action(() =>
                        {
                            var dvc = new DungeonColumnViewModel() { Dungeon = dungeon };
                            CharacterViewModels?.ToList().ForEach(charVm =>
                                {
                                    //if (charVm.Character.Hidden) return;
                                    dvc.DungeonsList.Add(
                                          new DungeonCooldownViewModel
                                          {
                                              Owner = charVm.Character,
                                              Cooldown = charVm.Character.DungeonInfo.DungeonList.FirstOrDefault(x =>
                                                  x.Dungeon.Id == dungeon.Id)
                                          });
                                });
                            _columns.Add(dvc);
                        }), DispatcherPriority.Background);
                    });
                });
                _loaded = true;
            }, c => !_loaded);

            Game.Account.Characters.CollectionChanged += SyncViewModel;

            LoadCharacters();

            Game.Account.Characters.ToList().ForEach(c => CharacterViewModels.Add(new CharacterViewModel { Character = c }));

            SortedCharacters = CollectionViewUtils.InitLiveView(o => !((Character)o).Hidden, Game.Account.Characters,
                new[] { nameof(Character.Hidden) },
                new[] { new SortDescription(nameof(Character.Position), ListSortDirection.Ascending) });

            HiddenCharacters = CollectionViewUtils.InitLiveView(o => ((Character)o).Hidden, Game.Account.Characters,
                new[] { nameof(Character.Hidden) },
                new[] { new SortDescription(nameof(Character.Position), ListSortDirection.Ascending) });

            CharacterViewModelsView = CollectionViewUtils.InitLiveView(o => !((CharacterViewModel)o).Character.Hidden, CharacterViewModels,
                new[] { $"{nameof(CharacterViewModel.Character)}.{nameof(Character.Hidden)}" },
                new[] { new SortDescription($"{nameof(CharacterViewModel.Character)}.{nameof(Character.Position)}", ListSortDirection.Ascending) });

        }



        /* -- Methods ---------------------------------------------- */

        public void SaveCharacters()
        {
            //SaveCharDoc(CharactersXmlParser.BuildCharacterFile(Characters));
            var json = JsonConvert.SerializeObject(Game.Account, Formatting.Indented);
            File.WriteAllText(Path.Combine(App.ResourcesPath, "config/characters.json"), json);
            if (File.Exists(Path.Combine(App.ResourcesPath, "config/characters.xml")))
                File.Delete(Path.Combine(App.ResourcesPath, "config/characters.xml"));

        }
        private void LoadCharacters()
        {
            try
            {
                if (File.Exists(Path.Combine(App.ResourcesPath, "config/characters.xml")))
                    new CharactersXmlParser().Read(Game.Account.Characters);
                else
                {
                    if (!File.Exists(Path.Combine(App.ResourcesPath, "config/characters.json"))) return;
                    Game.Account =
                        JsonConvert.DeserializeObject<Account>(
                            File.ReadAllText(Path.Combine(App.ResourcesPath, "config/characters.json")));
                }
            }
            catch (Exception e)
            {
                var res = TccMessageBox.Show("TCC", $"There was an error while reading characters file (more info in error.log). Manually correct the error and press Ok to try again, else press Cancel to delete current data.", MessageBoxButton.OKCancel);
                Log.F($"Cannot read characters file: {e}");
                if (res == MessageBoxResult.OK) LoadCharacters();
                else
                {
                    File.Delete(Path.Combine(App.ResourcesPath, "config/characters.xml"));
                    LoadCharacters();
                }
            }
        }
        public void SetLoggedIn(uint id)
        {
            _discardFirstVanguardPacket = true;
            Game.Account.Characters.ToList().ForEach(x => x.IsLoggedIn = x.Id == id);
        }
        public void SetDungeons(Dictionary<uint, short> dungeonCooldowns)
        {
            CurrentCharacter?.DungeonInfo.UpdateEntries(dungeonCooldowns);

        }
        public void SetDungeons(uint charId, Dictionary<uint, short> dungeonCooldowns)
        {
            Game.Account.Characters.FirstOrDefault(x => x.Id == charId)?.DungeonInfo.UpdateEntries(dungeonCooldowns);

        }
        public void SetVanguard(int weeklyDone, int dailyDone, int vanguardCredits)
        {
            if (_discardFirstVanguardPacket)
            {
                _discardFirstVanguardPacket = false;
                return;
            }

            if (CurrentCharacter == null) return;
            CurrentCharacter.VanguardInfo.WeekliesDone = weeklyDone;
            CurrentCharacter.VanguardInfo.DailiesDone = dailyDone;
            CurrentCharacter.VanguardInfo.Credits = vanguardCredits;
            SaveCharacters();
            N(nameof(TotalVanguardCredits));
        }
        public void SetVanguardCredits(int pCredits)
        {
            CurrentCharacter.VanguardInfo.Credits = pCredits;
            N(nameof(TotalVanguardCredits));
        }
        public void SetGuardianCredits(int pCredits)
        {
            CurrentCharacter.GuardianInfo.Credits = pCredits;
            N(nameof(TotalGuardianCredits));
        }
        public void SetElleonMarks(int val)
        {
            CurrentCharacter.ElleonMarks = val;
            N(nameof(TotalElleonMarks));
        }
        private static void SaveCharDoc(XDocument doc)
        {
            try
            {
                var fs = new FileStream(Path.Combine(App.ResourcesPath, "config/characters.xml"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                fs.SetLength(0);
                using (var sr = new StreamWriter(fs, new UTF8Encoding(true)))
                {
                    sr.Write(doc.Declaration + Environment.NewLine + doc);
                }
                fs.Close();
            }
            catch (Exception)
            {
                var res = TccMessageBox.Show("TCC", "Could not write character data to characters.xml. File is being used by another process. Try again?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes) SaveCharDoc(doc);
            }
        }
        private void GcPls(object sender, EventArgs ev) { }

        public void SelectCharacter(Character character)
        {
            try
            {
                if (SelectedCharacterInventory != null) ((ICollectionView)SelectedCharacterInventory).CollectionChanged -= GcPls;

                SelectedCharacter = character;
                SelectedCharacterInventory = CollectionViewUtils.InitLiveView(o => o != null, character.Inventory, new string[] { }, new[]
                {
                    new SortDescription("Item.Id", ListSortDirection.Ascending),
                });
                ((ICollectionView)SelectedCharacterInventory).CollectionChanged += GcPls;
                WindowManager.DashboardWindow.ShowDetails();
                Task.Delay(300).ContinueWith(t => Task.Factory.StartNew(() => N(nameof(SelectedCharacterInventory))));
            }
            catch (Exception e)
            {
                Log.F($"Failed to select character: {e}");
            }
        }

        protected override void InstallHooks()
        {
            PacketAnalyzer.NewProcessor.Hook<S_UPDATE_NPCGUILD>(OnUpdateNpcGuild);
            PacketAnalyzer.NewProcessor.Hook<S_NPCGUILD_LIST>(OnNpcGuildList);
            PacketAnalyzer.NewProcessor.Hook<S_INVEN>(OnInven);
            PacketAnalyzer.NewProcessor.Hook<S_PLAYER_STAT_UPDATE>(OnPlayerStatUpdate);
            PacketAnalyzer.NewProcessor.Hook<S_GET_USER_LIST>(OnGetUserList);
            PacketAnalyzer.NewProcessor.Hook<S_LOGIN>(OnLogin);
            PacketAnalyzer.NewProcessor.Hook<S_RETURN_TO_LOBBY>(OnReturnToLobby);
            PacketAnalyzer.NewProcessor.Hook<S_DUNGEON_COOL_TIME_LIST>(OnDungeonCoolTimeList);
            PacketAnalyzer.NewProcessor.Hook<S_FIELD_POINT_INFO>(OnFieldPointInfo);
            PacketAnalyzer.NewProcessor.Hook<S_AVAILABLE_EVENT_MATCHING_LIST>(OnAvailableEventMatchingList);
            PacketAnalyzer.NewProcessor.Hook<S_DUNGEON_CLEAR_COUNT_LIST>(OnDungeonClearCountList);
        }

        protected override void RemoveHooks()
        {
            PacketAnalyzer.NewProcessor.Unhook<S_UPDATE_NPCGUILD>(OnUpdateNpcGuild);
            PacketAnalyzer.NewProcessor.Unhook<S_NPCGUILD_LIST>(OnNpcGuildList);
            PacketAnalyzer.NewProcessor.Unhook<S_INVEN>(OnInven);
            PacketAnalyzer.NewProcessor.Unhook<S_PLAYER_STAT_UPDATE>(OnPlayerStatUpdate);
            PacketAnalyzer.NewProcessor.Unhook<S_GET_USER_LIST>(OnGetUserList);
            PacketAnalyzer.NewProcessor.Unhook<S_LOGIN>(OnLogin);
            PacketAnalyzer.NewProcessor.Unhook<S_RETURN_TO_LOBBY>(OnReturnToLobby);
            PacketAnalyzer.NewProcessor.Unhook<S_DUNGEON_COOL_TIME_LIST>(OnDungeonCoolTimeList);
            PacketAnalyzer.NewProcessor.Unhook<S_FIELD_POINT_INFO>(OnFieldPointInfo);
            PacketAnalyzer.NewProcessor.Unhook<S_AVAILABLE_EVENT_MATCHING_LIST>(OnAvailableEventMatchingList);
            PacketAnalyzer.NewProcessor.Unhook<S_DUNGEON_CLEAR_COUNT_LIST>(OnDungeonClearCountList);
        }

        private void OnDungeonClearCountList(S_DUNGEON_CLEAR_COUNT_LIST m)
        {
            if (m.Failed) return;
            if (m.PlayerId != Game.Me.PlayerId) return;
            foreach (var dg in m.DungeonClears)
            {
                CurrentCharacter.DungeonInfo.UpdateClears(dg.Key, dg.Value);
            }
        }
        private void OnAvailableEventMatchingList(S_AVAILABLE_EVENT_MATCHING_LIST m)
        {
            SetVanguard(m.WeeklyDone, m.DailyDone, m.VanguardCredits);
        }
        private void OnFieldPointInfo(S_FIELD_POINT_INFO m)
        {
            if (CurrentCharacter == null) return;
            CurrentCharacter.GuardianInfo.Claimed = m.Claimed;
            CurrentCharacter.GuardianInfo.Cleared = m.Cleared;
        }
        private void OnDungeonCoolTimeList(S_DUNGEON_COOL_TIME_LIST m)
        {
            SetDungeons(m.DungeonCooldowns);
        }
        private void OnReturnToLobby(S_RETURN_TO_LOBBY m)
        {
            UpdateBuffs();
        }
        private void OnLogin(S_LOGIN m)
        {
            SetLoggedIn(m.PlayerId);
        }
        private void OnGetUserList(S_GET_USER_LIST m)
        {
            UpdateBuffs();
            foreach (var item in m.CharacterList)
            {
                var ch = Game.Account.Characters.FirstOrDefault(x => x.Id == item.Id);
                if (ch != null)
                {
                    ch.Name = item.Name;
                    ch.Laurel = item.Laurel;
                    ch.Position = item.Position;
                    ch.GuildName = item.GuildName;
                    ch.Level = item.Level;
                    ch.LastLocation = new Location(item.LastWorldId, item.LastGuardId, item.LastSectionId);
                    ch.LastOnline = item.LastOnline;
                    ch.ServerName = Game.Server.Name;
                }
                else
                {
                    Game.Account.Characters.Add(new Character(item));
                }
            }

            SaveCharacters();
        }
        private void OnPlayerStatUpdate(S_PLAYER_STAT_UPDATE m)
        {
            CurrentCharacter.Coins = m.Coins;
            CurrentCharacter.MaxCoins = m.MaxCoins;
            CurrentCharacter.ItemLevel = m.Ilvl;
            CurrentCharacter.Level = m.Level;
        }
        private void OnInven(S_INVEN m)
        {
            if (m.Failed) return;
            WindowManager.ViewModels.Dashboard.UpdateInventory(m.Items, m.First);
        }
        private void OnNpcGuildList(S_NPCGUILD_LIST m)
        {
            if (!Game.IsMe(m.UserId)) return;
            m.NpcGuildList.Keys.ToList()
                .ForEach(k =>
                {
                    switch (k)
                    {
                        case (int)NpcGuild.Vanguard:
                            SetVanguardCredits(m.NpcGuildList[k]);
                            break;
                        case (int)NpcGuild.Guardian:
                            SetGuardianCredits(m.NpcGuildList[k]);
                            break;
                    }
                });
        }
        private void OnUpdateNpcGuild(S_UPDATE_NPCGUILD m)
        {
            switch (m.Guild)
            {
                case NpcGuild.Vanguard:
                    SetVanguardCredits(m.Credits);
                    break;
                case NpcGuild.Guardian:
                    SetGuardianCredits(m.Credits);
                    break;
            }
        }

        /* -- TODO EVENTS: TO BE REFACTORED ------------------------- */

        public SynchronizedObservableCollection<EventGroup> EventGroups { get; }
        public SynchronizedObservableCollection<TimeMarker> Markers { get; }
        public SynchronizedObservableCollection<DailyEvent> SpecialEvents { get; }

        public void LoadEvents(DayOfWeek today, string region)
        {
            ClearEvents();
            if (region == null)
            {
                WindowManager.FloatingButton.NotifyExtended("Info window", "No region specified; cannot load events.", NotificationType.Error);
                ChatWindowManager.Instance.AddTccMessage("Unable to load events.");
                return;
            }
            LoadEventFile(today, region);
            if (Game.Logged) TimeManager.Instance.SetGuildBamTime(false);

        }
        public void ClearEvents()
        {
            EventGroups.Clear();
            SpecialEvents.Clear();
        }
        private void LoadEventFile(DayOfWeek today, string region)
        {
            var yesterday = today - 1;
            if (region.StartsWith("EU")) region = "EU";
            var path = Path.Combine(App.ResourcesPath, $"config/events/events-{region}.xml");
            if (!File.Exists(path))
            {
                var root = new XElement("Events");
                var eg = new XElement("EventGroup", new XAttribute("name", "Example event group"));
                var ev = new XElement("Event",
                    new XAttribute("name", "Example Event"),
                    new XAttribute("days", "*"),
                    new XAttribute("start", "12:00"),
                    new XAttribute("end", "15:00"),
                    new XAttribute("color", "ff5566"));
                var ev2 = new XElement("Event",
                        new XAttribute("name", "Example event 2"),
                        new XAttribute("days", "*"),
                        new XAttribute("start", "16:00"),
                        new XAttribute("duration", "3:00"),
                        new XAttribute("color", "ff5566"));
                eg.Add(ev);
                eg.Add(ev2);
                root.Add(eg);
                if (!Directory.Exists(Path.Combine(App.ResourcesPath, "config/events")))
                    Directory.CreateDirectory(Path.Combine(App.ResourcesPath, "config/events"));

                //if(!Utils.IsFileLocked(path, FileAccess.ReadWrite))
                root.Save(path);
            }

            try
            {
                var d = XDocument.Load(path);
                foreach (var egElement in d.Descendants().Where(x => x.Name == "EventGroup"))
                {
                    var egName = egElement.Attribute("name").Value;
                    var egRc = egElement.Attribute("remote") != null && bool.Parse(egElement.Attribute("remote").Value);
                    var egStart = egElement.Attribute("start") != null
                        ? DateTime.Parse(egElement.Attribute("start").Value)
                        : DateTime.MinValue;
                    var egEnd = egElement.Attribute("end") != null
                        ? DateTime.Parse(egElement.Attribute("end").Value).AddDays(1)
                        : DateTime.MaxValue;

                    if (TimeManager.Instance.CurrentServerTime < egStart ||
                        TimeManager.Instance.CurrentServerTime > egEnd) continue;

                    var eg = new EventGroup(egName, egStart, egEnd, egRc);
                    foreach (var evElement in egElement.Descendants().Where(x => x.Name == "Event"))
                    {
                        var isYesterday = false;
                        var isToday = false;

                        if (evElement.Attribute("days").Value != "*")
                        {
                            if (evElement.Attribute("days").Value.Contains(','))
                            {
                                var days = evElement.Attribute("days").Value.Split(',');
                                foreach (var dayString in days)
                                {
                                    var day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), dayString);
                                    if (day == today) isToday = true;
                                    if (day == yesterday) isYesterday = true;
                                }
                            }
                            else
                            {
                                var eventDay = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), evElement.Attribute("days").Value);
                                isToday = eventDay == today;
                                isYesterday = eventDay == yesterday;
                            }
                        }
                        else
                        {
                            isToday = true;
                            isYesterday = true;
                        }

                        if (!isToday && !isYesterday) continue;

                        var name = evElement.Attribute("name").Value;
                        var parsedStart = DateTime.Parse(evElement.Attribute("start").Value, CultureInfo.InvariantCulture);
                        var parsedDuration = TimeSpan.Zero;
                        var parsedEnd = DateTime.Now;
                        bool isDuration;
                        if (evElement.Attribute("duration") != null)
                        {
                            parsedDuration = TimeSpan.Parse(evElement.Attribute("duration").Value, CultureInfo.InvariantCulture);
                            isDuration = true;
                        }
                        else if (evElement.Attribute("end") != null)
                        {
                            parsedEnd = DateTime.Parse(evElement.Attribute("end").Value, CultureInfo.InvariantCulture);
                            isDuration = false;
                        }
                        else
                        {
                            parsedDuration = TimeSpan.Zero;
                            parsedEnd = parsedStart;
                            isDuration = true;
                        }

                        var color = "5599ff";

                        var start = parsedStart.Hour + parsedStart.Minute / 60D;
                        var end = isDuration ? parsedDuration.Hours + parsedDuration.Minutes / 60D : parsedEnd.Hour + parsedEnd.Minute / 60D;

                        if (evElement.Attribute("color") != null)
                        {
                            color = evElement.Attribute("color").Value;
                        }
                        if (isYesterday)
                        {
                            if (!EventUtils.EndsToday(start, end, isDuration))
                            {
                                var e1 = new DailyEvent(name, parsedStart.Hour, 24, 0, color, false);
                                end = start + end - 24;
                                var e2 = new DailyEvent(name, parsedStart.Hour, parsedStart.Minute, end, color, isDuration);
                                if (isToday) eg.AddEvent(e1);
                                eg.AddEvent(e2);
                            }
                            else if (isToday)
                            {
                                var ev = new DailyEvent(name, parsedStart.Hour, parsedStart.Minute, end, color, isDuration);
                                eg.AddEvent(ev);
                            }
                        }
                        else
                        {
                            var ev = new DailyEvent(name, parsedStart.Hour, parsedStart.Minute, end, color, isDuration);
                            eg.AddEvent(ev);
                        }
                    }
                    if (eg.Events.Count != 0) AddEventGroup(eg);
                }
                SpecialEvents.Add(new DailyEvent("Reset", TimeManager.Instance.ResetHour, 0, 0, "ff0000"));



            }
            catch (Exception)
            {
                var res = TccMessageBox.Show("TCC", $"There was an error while reading events-{region}.xml. Manually correct the error and and press Ok to try again, else press Cancel to build a default config file.", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                if (res == MessageBoxResult.Cancel) File.Delete(path);
                LoadEventFile(today, region);
            }
        }
        public void AddEventGroup(EventGroup eg)
        {
            var g = EventGroups.ToSyncList().FirstOrDefault(x => x.Name == eg.Name);
            if (g != null)
            {
                foreach (var ev in eg.Events)
                {
                    g.AddEvent(ev);
                }
            }
            else
            {
                EventGroups.Add(eg);
            }
        }

        public void UpdateBuffs()
        {
            if (!Game.Logged) return;
            CurrentCharacter.Buffs.Clear();
            Game.Me.Buffs.ToList().ForEach(b =>
            {
                //var existing = CurrentCharacter.Buffs.FirstOrDefault(x => x.Id == b.Abnormality.Id);
                /*if (existing == null)*/
                CurrentCharacter.Buffs.Add(new AbnormalityData { Id = b.Abnormality.Id, Duration = b.DurationLeft, Stacks = b.Stacks });
                //else
                //{
                //    existing.Id = b.Abnormality.Id;
                //    existing.Duration = b.DurationLeft;
                //    existing.Stacks = b.Stacks;
                //}
            });
            Game.Me.Debuffs.ToList().ForEach(b =>
            {
                //var existing = CurrentCharacter.Buffs.FirstOrDefault(x => x.Id == b.Abnormality.Id);
                /*if (existing == null)*/
                CurrentCharacter.Buffs.Add(new AbnormalityData { Id = b.Abnormality.Id, Duration = b.DurationLeft, Stacks = b.Stacks });
                //else
                //{
                //    existing.Id = b.Abnormality.Id;
                //    existing.Duration = b.DurationLeft;
                //    existing.Stacks = b.Stacks;
                //}
            });
        }

        public void UpdateInventory(Dictionary<uint, ItemAmount> list, bool first)
        {
            var em = list.Values.FirstOrDefault(x => x.Id == 151643);
            var ds = list.Values.FirstOrDefault(x => x.Id == 45474);
            var ps = list.Values.FirstOrDefault(x => x.Id == 45482);

            //TODO: check this
            /*if (em != null)*/
            if (em.Id != 0) SetElleonMarks(em.Amount);
            /*if (ds != null)*/
            if (ds.Id != 0) CurrentCharacter.DragonwingScales = ds.Amount;
            /*if (ps != null)*/
            if (ps.Id != 0) CurrentCharacter.PiecesOfDragonScroll = ps.Amount;
            try
            {
                if (first) CurrentCharacter.Inventory.Clear();

                foreach (var keyVal in list)
                {
                    var existing = CurrentCharacter.Inventory.FirstOrDefault(x => x.Item.Id == keyVal.Value.Id);
                    if (existing != null)
                    {
                        existing.Amount += keyVal.Value.Amount;
                        continue;
                    }
                    CurrentCharacter.Inventory.Add(new InventoryItem(keyVal.Key, keyVal.Value.Id, keyVal.Value.Amount));
                }
            }
            catch (Exception e)
            {
                Log.F($"Error while refreshing inventory: {e}");
            }

            N(nameof(SelectedCharacterInventory));
        }

        public void ResetDailyData()
        {
            Game.Account.Characters.ToList().ForEach(ch => ch.ResetDailyData());
            ChatWindowManager.Instance.AddTccMessage("Daily data has been reset.");
        }

        public void ResetWeeklyDungeons()
        {
            Game.Account.Characters.ToSyncList().ForEach(ch => ch.DungeonInfo.ResetAll(ResetMode.Weekly));
            ChatWindowManager.Instance.AddTccMessage("Weekly dungeon entries have been reset.");
        }

        public void ResetVanguardWeekly()
        {
            Game.Account.Characters.ToList().ForEach(ch => ch.VanguardInfo.WeekliesDone = 0);
            ChatWindowManager.Instance.AddTccMessage("Weekly vanguard data has been reset.");
        }

        public void RefreshDungeons()
        {
            _columns.Clear();
            Task.Factory.StartNew(() =>
            {
                Game.DB.DungeonDatabase.Dungeons.Values.Where(d => d.HasDef).ToList().ForEach(dungeon =>
                {
                    App.BaseDispatcher.BeginInvoke(new Action(() =>
                    {
                        var dvc = new DungeonColumnViewModel() { Dungeon = dungeon };
                        CharacterViewModels?.ToList().ForEach(charVm =>
                        {
                            //if (charVm.Character.Hidden) return;
                            dvc.DungeonsList.Add(
                                new DungeonCooldownViewModel
                                {
                                    Owner = charVm.Character,
                                    Cooldown = charVm.Character.DungeonInfo.DungeonList.FirstOrDefault(x =>
                                        x.Dungeon.Id == dungeon.Id)
                                });
                        });
                        _columns.Add(dvc);
                    }), DispatcherPriority.Background);
                });
            });
        }
    }
}
