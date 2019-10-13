﻿using System;
using System.Collections.Generic;
using System.Linq;
using TCC.Data;
using TCC.Data.Chat;
using TCC.ViewModels;

namespace TCC.Parsing
{
    public static class SystemMessagesProcessor
    {
        public static void AnalyzeMessage(string srvMsg, SystemMessageData sysMsg, string opcodeName)
        {
            if (!Pass(opcodeName)) return;

            if (!Process(srvMsg, sysMsg, opcodeName))
            {
                ChatWindowManager.Instance.AddSystemMessage(srvMsg, sysMsg);
            }
        }

        private static bool Pass(string opcodeName)
        {
            return !App.Settings.UserExcludedSysMsg.Contains(opcodeName);
        }

        private static void HandleMaxEnchantSucceed(string x)
        {
            var sysMsg = ChatMessage.BuildEnchantSystemMessage(x);
            ChatWindowManager.Instance.AddChatMessage(sysMsg);
        }
        private static void HandleFriendLogin(string friendName, SystemMessageData sysMsg)
        {
            var template = "@0\vUserName\v" + friendName;
            ChatWindowManager.Instance.AddSystemMessage(template, sysMsg,ChatChannel.Friend, friendName);
        }
        private static void HandleClearedGuardianQuestsMessage(string srvMsg, SystemMessageData sysMsg)
        {
            var currChar = WindowManager.ViewModels.DashboardVM.CurrentCharacter;
            var cleared = currChar.GuardianInfo.Cleared;
            var standardCountString = $"<font color =\"#cccccc\">({cleared}/40)</font>";
            var maxedCountString = $"<font color=\"#cccccc\">(</font><font color =\"#ff0000\">{cleared}</font><font color=\"#cccccc\">/40)</font>";
            var newMsg = new SystemMessageData($"{sysMsg.Template} {(cleared == 40 ? maxedCountString : standardCountString)}", sysMsg.ChatChannel);
            var msg = ChatWindowManager.Instance.Factory.CreateSystemMessage(srvMsg, newMsg, ChatChannel.Guardian);
            if (currChar.GuardianInfo.Cleared == 40) msg.ContainsPlayerName = true;
            ChatWindowManager.Instance.AddChatMessage(msg);

        }
        private static void HandleNewGuildMasterMessage(string template, SystemMessageData sysMsg)
        {
            var msg = ChatWindowManager.Instance.Factory.CreateSystemMessage(template, sysMsg, ChatChannel.GuildNotice);
            ChatWindowManager.Instance.AddChatMessage(msg);
            msg.ContainsPlayerName = true;
            WindowManager.ViewModels.NotificationAreaVM.Enqueue("Guild", msg.ToString(), NotificationType.Success);

        }
        private static void HandleGuilBamSpawn(string srvMsg, SystemMessageData sysMsg)
        {
            var msg = ChatWindowManager.Instance.Factory.CreateSystemMessage(srvMsg, sysMsg, (ChatChannel)sysMsg.ChatChannel);
            WindowManager.ViewModels.NotificationAreaVM.Enqueue("Guild BAM", msg.ToString(), NotificationType.Normal);
            ChatWindowManager.Instance.AddChatMessage(msg);

            TimeManager.Instance.UploadGuildBamTimestamp();

            TimeManager.Instance.SetGuildBamTime(true);
            TimeManager.Instance.ExecuteGuildBamWebhook();
        }
        private static void HandleDungeonEngagedMessage(string srvMsg, SystemMessageData sysMsg)
        {
            const string s = "dungeon:";
            var dgId = Convert.ToUInt32(srvMsg.Substring(srvMsg.IndexOf(s, StringComparison.Ordinal) + s.Length));
            WindowManager.ViewModels.DashboardVM.CurrentCharacter.DungeonInfo.Engage(dgId);

            var msg = ChatWindowManager.Instance.Factory.CreateSystemMessage(srvMsg, sysMsg, (ChatChannel)sysMsg.ChatChannel);
            ChatWindowManager.Instance.AddChatMessage(msg);
        }
        private static void HandleFriendInAreaMessage(string srvMsg, SystemMessageData sysMsg)
        {
            var msg = ChatWindowManager.Instance.Factory.CreateSystemMessage(srvMsg, sysMsg, ChatChannel.Friend);
            var start = srvMsg.IndexOf("UserName\v", StringComparison.InvariantCultureIgnoreCase) + "UserName\v".Length;
            var end = srvMsg.IndexOf("\v", start, StringComparison.InvariantCultureIgnoreCase);
            var friendName = srvMsg.Substring(start, end - start);
            msg.Author = friendName;
            ChatWindowManager.Instance.AddChatMessage(msg);
        }
        private static void HandleRessMessage(string srvMsg, SystemMessageData sysMsg)
        {
            var newSysMsg = new SystemMessageData(sysMsg.Template.Replace("{UserName}", "<font color='#cccccc'>{UserName}</font>"), (int)ChatChannel.Ress);
            var msg = ChatWindowManager.Instance.Factory.CreateSystemMessage(srvMsg, newSysMsg, ChatChannel.Ress);
            ChatWindowManager.Instance.AddChatMessage(msg);
            //if (ProxyInterface.Instance.IsStubAvailable) ProxyInterface.Instance.Stub.ForceSystemMessage(srvMsg, "SMT_BATTLE_PARTY_RESURRECT"); //ProxyOld.ForceSystemMessage(srvMsg, "SMT_BATTLE_PARTY_RESURRECT");

        }
        private static void HandleDeathMessage(string srvMsg, SystemMessageData sysMsg)
        {
            var newSysMsg = new SystemMessageData(sysMsg.Template.Replace("{UserName}", "<font color='#cccccc'>{UserName}</font>"), (int)ChatChannel.Death);
            var msg = ChatWindowManager.Instance.Factory.CreateSystemMessage(srvMsg, newSysMsg, ChatChannel.Death);
            ChatWindowManager.Instance.AddChatMessage(msg);
        }
        private static void HandleInvalidLink(string srvMsg, SystemMessageData sysMsg)
        {
            ChatWindowManager.Instance.AddChatMessage(ChatWindowManager.Instance.Factory.CreateSystemMessage(srvMsg, sysMsg, (ChatChannel)sysMsg.ChatChannel));
            ChatWindowManager.Instance.RemoveDeadLfg();
            if (App.Settings.LfgWindowSettings.Enabled) WindowManager.ViewModels.LfgVM.RemoveDeadLfg();
        }
        private static void HandleMerchantSpawn(string srvMsg, SystemMessageData sysMsg)
        {
            var msg = ChatWindowManager.Instance.Factory.CreateSystemMessage(srvMsg, sysMsg, (ChatChannel)sysMsg.ChatChannel);
            ChatWindowManager.Instance.AddChatMessage(msg);
            WindowManager.ViewModels.NotificationAreaVM.Enqueue("TCC", msg.ToString(), NotificationType.Normal, 10000);
        }
        private static void HandleMerchantDespawn(string srvMsg, SystemMessageData sysMsg)
        {
            var msg = ChatWindowManager.Instance.Factory.CreateSystemMessage(srvMsg, sysMsg, (ChatChannel)sysMsg.ChatChannel);
            ChatWindowManager.Instance.AddChatMessage(msg);
            WindowManager.ViewModels.NotificationAreaVM.Enqueue("TCC", msg.ToString(), NotificationType.Normal, 10000);
        }
        private static void HandleLfgNotListed(string srvMsg, SystemMessageData sysMsg)
        {
            ChatWindowManager.Instance.AddSystemMessage(srvMsg, sysMsg);
            WindowManager.ViewModels.LfgVM.ForceStopPublicize();
        }
        private static void Redirect(string srvMsg, SystemMessageData sysMsg, ChatChannel ch)
        {
            ChatWindowManager.Instance.AddSystemMessage(srvMsg, sysMsg, ch);
        }

        //by HQ 20181224
        private static void HandleFieldBossAppear(string srvMsg, SystemMessageData sysMsg)
        {
            var msg = ChatWindowManager.Instance.Factory.CreateSystemMessage(srvMsg, sysMsg, (ChatChannel)sysMsg.ChatChannel);
            ChatWindowManager.Instance.AddChatMessage(msg);
            WindowManager.ViewModels.NotificationAreaVM.Enqueue("TCC", msg.ToString(), NotificationType.Success, 10000);

            if (!App.Settings.WebhookEnabledFieldBoss) return;

            // @4157 \v
            // regionName \v @rgn:213 \v
            // npcName \v @creature:26#5001

            var monsterName = GetFieldBossName(srvMsg);
            var regName = srvMsg.Split('\v')[2].Replace("@rgn:", "");
            var regId = uint.Parse(regName);

            Game.DB.RegionsDatabase.Names.TryGetValue(regId, out var regionName);

            TimeManager.Instance.ExecuteFieldBossSpawnWebhook(monsterName, regionName, msg.RawMessage);

        }
        private static void HandleFieldBossDie(string srvMsg, SystemMessageData sysMsg)
        {
            var msg = ChatWindowManager.Instance.Factory.CreateSystemMessage(srvMsg, sysMsg, (ChatChannel)sysMsg.ChatChannel);
            ChatWindowManager.Instance.AddChatMessage(msg);
            WindowManager.ViewModels.NotificationAreaVM.Enqueue("TCC", msg.ToString(), NotificationType.Error, 10000);
            if (!App.Settings.WebhookEnabledFieldBoss) return;

            //@4158
            //guildNameWish
            //userName쿤
            //npcname@creature:26#5001

            //@????
            //userName쿤
            //npcname@creature:26#5001

            var monsterName = GetFieldBossName(srvMsg);
            var userName = GetFieldBossKillerName(srvMsg);
            var guildName = GetFieldBossKillerGuild(srvMsg);
            if (string.IsNullOrEmpty(guildName)) guildName = "-no guild-";
            TimeManager.Instance.ExecuteFieldBossDieWebhook(monsterName, msg.RawMessage, userName, guildName);



            //if (srvMsg.Contains("@creature:39#501"))     // Hazar
            //{
            //    TimeManager.Instance.ExecuteFieldBossWebhook(501, 2);
            //    //Log.F("FieldBoss.log", $"[{nameof(HandleFieldBossDie)}] {srvMsg}"); //by HQ 20181228
            //}
            //else if (srvMsg.Contains("@creature:51#4001"))    // Kelos
            //{
            //    TimeManager.Instance.ExecuteFieldBossWebhook(4001, 2);
            //    //Log.F("FieldBoss.log", $"[{nameof(HandleFieldBossDie)}] {srvMsg}"); //by HQ 20181228
            //}
            //else if (srvMsg.Contains("@creature:26#5001"))    // Ortan
            //{
            //    TimeManager.Instance.ExecuteFieldBossWebhook(5001, 2);
            //    //Log.F("FieldBoss.log", $"[{nameof(HandleFieldBossDie)}] {srvMsg}"); //by HQ 20181228
            //}
            //else
            //{
            //    //Log.F("FieldBoss.log", $"[{nameof(HandleFieldBossDie)}] {srvMsg}"); //by HQ 20181228
            //}
        }
        private static string GetFieldBossName(string srvMsg)
        {
            // only for 'SMT_FIELDBOSS_*'
            var srvMsgSplit = srvMsg.Split('\v');
            var npcName = srvMsgSplit.Last().Replace("@creature:", "");
            var zoneId = uint.Parse(npcName.Split('#')[0]);
            var templateId = uint.Parse(npcName.Split('#')[1]);
            Game.DB.MonsterDatabase.TryGetMonster(templateId, zoneId, out var m);
            return m.Name;
        }
        private static string GetFieldBossKillerName(string srvMsg)
        {
            // only for 'SMT_FIELDBOSS_*'
            var ret = "";
            var srvMsgSplit = srvMsg.Split('\v').ToList();
            var idx = srvMsgSplit.IndexOf("userName") + 1;
            if (idx != -1 && idx < srvMsgSplit.Count) ret = srvMsgSplit[idx];
            return ret;
        }
        private static string GetFieldBossKillerGuild(string srvMsg)
        {
            // only for 'SMT_FIELDBOSS_*'
            var ret = "";
            var srvMsgSplit = srvMsg.Split('\v').ToList();
            var idx = srvMsgSplit.IndexOf("guildName") + 1;
            if (idx != -1 && idx < srvMsgSplit.Count) ret = srvMsgSplit[idx];
            return ret;
        }

        #region Factory

        private static readonly Dictionary<string, Delegate> Processor = new Dictionary<string, Delegate>
        {
            { "SMT_MAX_ENCHANT_SUCCEED",                    new Action<string, SystemMessageData>((srvMsg, sysMsg) => HandleMaxEnchantSucceed(srvMsg)) },
            { "SMT_FRIEND_IS_CONNECTED",                    new Action<string, SystemMessageData>(HandleFriendLogin) },
            { "SMT_FRIEND_WALK_INTO_SAME_AREA",             new Action<string, SystemMessageData>(HandleFriendInAreaMessage) },
            { "SMT_CHAT_LINKTEXT_DISCONNECT",               new Action<string, SystemMessageData>(HandleInvalidLink) },

            { "SMT_BATTLE_PARTY_DIE",                       new Action<string, SystemMessageData>(HandleDeathMessage) },
            { "SMT_BATTLE_YOU_DIE",                         new Action<string, SystemMessageData>(HandleDeathMessage) },
            { "SMT_BATTLE_PARTY_RESURRECT",                 new Action<string, SystemMessageData>(HandleRessMessage) },
            { "SMT_BATTLE_RESURRECT",                       new Action<string, SystemMessageData>(HandleRessMessage) },

            { "SMT_ACCEPT_QUEST",                           new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_CANT_START_QUEST",                       new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_COMPLATE_GUILD_QUEST",                   new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_COMPLETE_MISSION",                       new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_COMPLETE_QUEST",                         new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_FAILED_QUEST_COMPENSATION",              new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_FAILED_QUEST",                           new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_FAILED_QUEST_CANCLE",                    new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_QUEST_FAILED_GET_FLAG_PARTY_LEVEL",      new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_QUEST_ITEM_DELETED",                     new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_QUEST_RESET_MESSAGE",                    new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_UPDATE_QUEST_TASK",                      new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_QUEST_SHARE_MESSAGE2",                   new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_QUEST_USE_SKILL",                        new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_QUEST_USE_ITEM",                         new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },

            { "SMT_GRANT_DUNGEON_COOLTIME_AND_COUNT",       new Action<string, SystemMessageData>(HandleDungeonEngagedMessage) },
            { "SMT_GQUEST_URGENT_NOTIFY",                   new Action<string, SystemMessageData>(HandleGuilBamSpawn) },

            { "SMT_PARTY_LOOT_ITEM_PARTYPLAYER",            new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Loot)) },

            { "SMT_GC_SYSMSG_GUILD_CHIEF_CHANGED",          new Action<string, SystemMessageData>(HandleNewGuildMasterMessage) },

            { "SMT_ACCOMPLISH_ACHIEVEMENT_GRADE_ALL",       new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Laurel)) },

            { "SMT_DROPDMG_DAMAGE",                         new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Damage)) },

            { "SMT_FIELD_EVENT_REWARD_AVAILABLE",           new Action<string, SystemMessageData>(HandleClearedGuardianQuestsMessage) },
            { "SMT_FIELD_EVENT_ENTER",                      new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guardian)) },
            { "SMT_FIELD_EVENT_LEAVE",                      new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guardian)) },
            { "SMT_FIELD_EVENT_COMPLETE",                   new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guardian)) },
            { "SMT_FIELD_EVENT_FAIL_OVERTIME",              new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guardian)) },
            { "SMT_FIELD_EVENT_CLEAR_REWARD_SENT",          new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guardian)) },
            { "SMT_FIELD_EVENT_WORLD_ANNOUNCE",             new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guardian)) },

            { "SMT_ITEM_DECOMPOSE_COMPLETE",                new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Loot)) },
            { "SMT_WAREHOUSE_ITEM_DRAW",                    new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Loot)) },
            { "SMT_WAREHOUSE_ITEM_INSERT",                  new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Loot)) },

            { "SMT_GC_MSGBOX_APPLYLIST_1",                  new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_MSGBOX_APPLYLIST_2",                  new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_MSGBOX_APPLYRESULT_1",                new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_MSGBOX_APPLYRESULT_2",                new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_SYSMSG_ACCEPT_GUILD_APPLY",           new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_SYSMSG_GUILD_MEMBER_BANNED",          new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_SYSMSG_GUILD_MEMBER_BANNED_2",        new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_SYSMSG_LEAVE_GUILD",                  new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_SYSMSG_LEAVE_GUILD_FRIEND",           new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_SYSMSG_REFUSE_GUILD_APPLY",           new Action<string, SystemMessageData>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },

            { "SMT_FIELDBOSS_APPEAR",                       new Action<string, SystemMessageData>(HandleFieldBossAppear) },     //by HQ 20181224
            { "SMT_FIELDBOSS_DIE_GUILD",                    new Action<string, SystemMessageData>(HandleFieldBossDie) },        //by HQ 20181224
            { "SMT_FIELDBOSS_DIE_NOGUILD",                  new Action<string, SystemMessageData>(HandleFieldBossDie) },        //by HQ 20181224

            { "SMT_PARTY_MATCHING_CANT_PR_NO_INFORMATION",  new Action<string, SystemMessageData>(HandleLfgNotListed)},

            { "SMT_WORLDSPAWN_NOTIFY_SPAWN",                new Action<string, SystemMessageData>(HandleMerchantSpawn)},
            { "SMT_WORLDSPAWN_NOTIFY_DESPAWN",              new Action<string, SystemMessageData>(HandleMerchantDespawn)},
        };


        private static bool Process(string serverMsg, SystemMessageData sysMsg, string opcodeName)
        {
            if (!Processor.TryGetValue(opcodeName, out var type) || type == null) return false;
            //TODO: check this and remove when chat will be moved to own thread.
            // BaseDispatcher.InvokeAsync() was added because of a deadlock in AddPiece() called from ChatMessage.ctor().ParseSysHtmlPiece()
            App.BaseDispatcher.InvokeAsync(new Action(() => type.DynamicInvoke(serverMsg, sysMsg)));
            return true;
        }
        #endregion
    }
}