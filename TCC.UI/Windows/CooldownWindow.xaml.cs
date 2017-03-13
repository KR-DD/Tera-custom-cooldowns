﻿using DamageMeter.Sniffing;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;


namespace TCC
{


    //public class SkillsModel
    //{
    //    public ObservableCollection<SkillIndicator> SkillIndicators;
    //    public SkillsModel()
    //    {
    //        SkillIndicators = new ObservableCollection<SkillIndicator>();
    //    }
    //}

    public class SkillCooldownNew
    {
        public Skill Skill { get; set; }
        public int Cooldown { get; set; }
        public CooldownType Type { get; set; }
        public System.Timers.Timer Timer { get; set; }

        public SkillCooldownNew(Skill sk, int cd, CooldownType t)
        {
            Skill = sk;
            Cooldown = cd;
            if (t == CooldownType.Item)
            {
                Cooldown = Cooldown * 1000;
            }

            if (cd != 0)
            {
                Timer = new System.Timers.Timer(Cooldown);
            }

        }
    }

    public partial class CooldownWindow : Window
    {
        public static CooldownWindow Instance;
        public CooldownWindow()
        {
            Instance = this;

            InitializeComponent();

            SkillsDatabase.Progress += UpdateLoadGauge;
            SkillIconControl.SkillEnded += SkillIconControl_SkillEnded;

            NormalSkillsPanel.ItemsSource = SkillManager.NormalSkillsQueue;
            LongSkillsPanel.ItemsSource = SkillManager.LongSkillsQueue;

            NormalSkillsPanel.DataContext = SkillManager.NormalSkillsQueue;
            LongSkillsPanel.DataContext = SkillManager.LongSkillsQueue;
        }

        private void SkillIconControl_SkillEnded(Skill sk, int cd)
        {
            if(cd < SkillManager.LongSkillTreshold)
            {
                if(SkillManager.NormalSkillsQueue.Where(x => x.Skill == sk).Count() > 0)
                SkillManager.NormalSkillsQueue.Remove(SkillManager.NormalSkillsQueue.Where(x => x.Skill == sk).Single());
            }
            else
            {
                if (SkillManager.LongSkillsQueue.Where(x => x.Skill == sk).Count() > 0)
                    SkillManager.LongSkillsQueue.Remove(SkillManager.LongSkillsQueue.Where(x => x.Skill == sk).Single());
            }
            Console.WriteLine("{0} - {1} removed.", sk.Id, sk.Name);
        }

        //public static void ClearSkills()
        //{
        //    Instance.Dispatcher.Invoke(() =>
        //    {
        //        Instance.NormalSkillsModel.SkillIndicators.Clear();
        //        Instance.LongSkillsModel.SkillIndicators.Clear();
        //    });
        //}

        //public static void AddNormalSkill(SkillCooldownOld sk)
        //{
        //    Instance.Dispatcher.Invoke(() =>
        //    {
        //        Instance.NormalSkillsModel.SkillIndicators.Add(new SkillCooldownNew(SkillsDatabase.GetSkill(sk.Id, SessionManager.CurrentClass), (int)sk.Cooldown));
        //        //Console.WriteLine("Added {0} indicator.", SkillsDatabase.GetSkill(sk.Id, PacketParser.CurrentClass).Name);
        //    });
        //}
        //public static void AddLongSkill(SkillCooldownOld sk)
        //{
        //    Instance.Dispatcher.Invoke(() =>
        //    {
        //        switch (sk.Type)
        //        {
        //            case CooldownType.Skill:
        //                Instance.LongSkillsModel.SkillIndicators.Add(new SkillCooldownNew(SkillsDatabase.GetSkill(sk.Id, SessionManager.CurrentClass), (int)sk.Cooldown));
        //                //Console.WriteLine("Added {0} indicator.", SkillsDatabase.GetSkill(sk.Id, PacketParser.CurrentClass).Name);

        //                break;
        //            case CooldownType.Item:
        //                Instance.LongSkillsModel.SkillIndicators.Add(new SkillCooldownNew(BroochesDatabase.GetBrooch(sk.Id), (int)sk.Cooldown));
        //                //Console.WriteLine("Added {0} indicator.", BroochesDatabase.GetBrooch(sk.Id).Name);

        //                break;
        //            default:
        //                break;
        //        }
        //    });
        //}

        //public static void RemoveNormalSkill(SkillCooldownOld sk)
        //{
        //    Instance.Dispatcher.Invoke(() =>
        //    {
        //        try
        //        {
        //            Instance.NormalSkillsModel.SkillIndicators.Remove(Instance.NormalSkillsModel.SkillIndicators.Where(x => x.Skill.Id == sk.Id).First());
        //            //Console.WriteLine("Removed {0} indicator.", SkillsDatabase.GetSkill(sk.Id, PacketParser.CurrentClass).Name);

        //        }
        //        catch (Exception)
        //        {
        //            //Console.WriteLine("Can't remove {0} indicator.", SkillsDatabase.GetSkill(sk.Id, PacketParser.CurrentClass).Name);
        //        }
        //    });
        //}
        //public static void RemoveLongSkill(SkillCooldownOld sk)
        //{
        //    Instance.Dispatcher.Invoke(() =>
        //    {
        //        try
        //        {
        //            switch (sk.Type)
        //            {
        //                case CooldownType.Skill:
        //                    Instance.LongSkillsModel.SkillIndicators.Remove(Instance.LongSkillsModel.SkillIndicators.Where(x => x.Skill.Id == sk.Id).First());
        //                    //Console.WriteLine("Removed {0} indicator.", SkillsDatabase.SkillIdToName(sk.Id, PacketParser.CurrentClass));
        //                    break;

        //                case CooldownType.Item:
        //                    Instance.LongSkillsModel.SkillIndicators.Remove(Instance.LongSkillsModel.SkillIndicators.Where(x => x.Skill.Id == sk.Id).First());
        //                    //Console.WriteLine("Removed {0} indicator.", BroochesDatabase.GetBrooch(sk.Id).Name);
        //                    break;

        //                default:
        //                    break;
        //            }

        //        }
        //        catch (Exception)
        //        {
        //            Console.WriteLine("Can't remove {0} indicator.", SkillsDatabase.SkillIdToName(sk.Id, SessionManager.CurrentClass));
        //        }
        //    });
        //}

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //SkillManager.AddSkill(new SkillCooldown(100700, 2000, CooldownType.Skill)); //test skill
            this.DragMove();
        }
        private void Window_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //ClearSkills();
        }

        public void UpdateLoadGauge(double val)
        {
            Dispatcher.Invoke(() =>
            {
                var v = val * 359.9 / 100;
                var a = new DoubleAnimation(Instance.loadArc.EndAngle, v, TimeSpan.FromMilliseconds(350))
                {
                    EasingFunction = new QuadraticEase()
                };
                Instance.loadArc.BeginAnimation(Arc.EndAngleProperty, a);
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Top = Properties.Settings.Default.CooldownBarTop;
            this.Left = Properties.Settings.Default.CooldownBarLeft;

            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            FocusManager.MakeUnfocusable(hwnd);
            FocusManager.HideFromToolBar(hwnd);
            if (Properties.Settings.Default.Transparent)
            {
                FocusManager.MakeTransparent(hwnd);
            }


        }
        public void LoadingDone()
        {
            Dispatcher.Invoke(() =>
            {
                var a = new DoubleAnimation(0, 359.9, TimeSpan.FromSeconds(.7))
                {
                    EasingFunction = new QuadraticEase()
                };
                a.Completed += (s, o) =>
                {
                    loadArc.Visibility = Visibility.Hidden;
                    FocusManager.FocusTimer.Start();
                    this.Hide();
                };
                loadArc.Stroke = new SolidColorBrush(Color.FromArgb(255, 100, 255, 100));
                loadArc.BeginAnimation(Arc.StartAngleProperty, a);
            });
        }
    }
}

/*
* C_START_SKILL            0xCBC5
* S_START_COOLTIME_SKILL   0x7BA8
* C_USE_ITEM               0x750F
* S_START_COOLTIME_ITEM    0xAD63
*/