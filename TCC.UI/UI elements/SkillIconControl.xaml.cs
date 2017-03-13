﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TCC
{
    public delegate void SkillEndedEventHandler(Skill sk, int cd);
    /// <summary>
    /// Logica di interazione per SkillIconControl.xaml
    /// </summary>
    public partial class SkillIconControl : UserControl
    {
        Timer NumberTimer;
        Timer MainTimer;
        Timer CloseTimer;
        double currentCd;
        int ending = SkillManager.Ending;

        public static event SkillEndedEventHandler SkillEnded;

        public ImageBrush IconBrush
        {
            get { return (ImageBrush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(ImageBrush), typeof(SkillIconControl));

        public uint Id
        {
            get { return (uint)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }
        public static readonly DependencyProperty IdProperty = DependencyProperty.Register("Id", typeof(uint), typeof(SkillIconControl));
        
        public int Cooldown
        {
            get { return (int)GetValue(CooldownProperty); }
            set { SetValue(CooldownProperty, value); }
        }
        public static readonly DependencyProperty CooldownProperty = DependencyProperty.Register("Cooldown", typeof(int), typeof(SkillIconControl));
        
        public string SkillName
        {
            get { return (string)GetValue(SkillNameProperty); }
            set { SetValue(SkillNameProperty, value); }
        }
        public static readonly DependencyProperty SkillNameProperty = DependencyProperty.Register("SkillName", typeof(string), typeof(SkillIconControl));

        public Skill Skill
        {
            get { return (Skill)GetValue(SkillProperty); }
            set { SetValue(SkillProperty, value); }
        }
        public static readonly DependencyProperty SkillProperty =
            DependencyProperty.Register("Skill", typeof(Skill), typeof(SkillIconControl));

        public void Reset(Skill sk)
        {
            Dispatcher.Invoke(() =>
            {
                if (sk == Skill)
                {
                    CloseTimer.Stop();
                    RemoveSkill();
                }
            });
        }



        public SkillIconControl()
        {
            InitializeComponent();
            icon.DataContext = this;
            SkillManager.Changed += ChangeCooldown;
            SkillManager.Reset += Reset;
        }

        private void ChangeCooldown(SkillCooldownNew s)
        {
            Dispatcher.Invoke(() =>
            {
                if (s.Skill.Name == this.Skill.Name)
                {
                    double newAngle = (double)s.Cooldown / (double)Cooldown;
                    currentCd = (double)s.Cooldown / 1000;
                    if (s.Cooldown > ending)
                    {
                        MainTimer.Interval = s.Cooldown - ending;
                    }
                    else
                    {
                        MainTimer.Interval = 1;
                    }
                    if (currentCd > 0)
                    {
                        number.Text = String.Format("{0:N0}", (currentCd));
                    }
                    else
                    {
                        number.Text = 0.ToString();
                    }
                    arc.BeginAnimation(Arc.EndAngleProperty, null);
                    var a = new DoubleAnimation(359.9 * newAngle, 0, TimeSpan.FromMilliseconds(s.Cooldown));
                    var b = new DoubleAnimation(.5, 1, TimeSpan.FromMilliseconds(150));
                    arc.BeginAnimation(Arc.EndAngleProperty, a);
                    this.BeginAnimation(OpacityProperty, b);
                }
            });
        }
        private void ControlLoaded(object sender, RoutedEventArgs e)
        {
            currentCd = (double)Cooldown / 1000;
            ToolTip = SkillName;
            NumberTimer = new Timer(1000);
            MainTimer = new Timer(Cooldown);

            CloseTimer = new Timer(ending);
            CloseTimer.Elapsed += CloseTimer_Elapsed;

            NumberTimer.Elapsed += (s, o) => {
                Dispatcher.Invoke(() => {
                    currentCd --;
                    if(currentCd < 0)
                    {
                        number.Text = "";
                        NumberTimer.Stop();
                    }
                    else
                    {
                        number.Text = String.Format("{0:N0}", currentCd); 
                    }
                });
            };
            MainTimer.Elapsed += (s, o) =>
            {
                Dispatcher.Invoke(() =>
                {
                    var c = new DoubleAnimation(22, 0, TimeSpan.FromMilliseconds(ending))
                    {
                        EasingFunction = new QuadraticEase()
                        {
                            EasingMode = EasingMode.EaseInOut
                        }
                    };
                    var w = new DoubleAnimation(0, TimeSpan.FromMilliseconds(ending))
                    {
                        EasingFunction = new QuadraticEase()
                        {
                            EasingMode = EasingMode.EaseInOut
                        }
                    };
                    var h = new DoubleAnimation(0, TimeSpan.FromMilliseconds(ending))
                    {
                        EasingFunction = new QuadraticEase()
                        {
                            EasingMode = EasingMode.EaseInOut
                        }
                    };
                    var t = new ThicknessAnimation(new Thickness(0), TimeSpan.FromMilliseconds(ending))
                    {
                        EasingFunction = new QuadraticEase()
                        {
                            EasingMode = EasingMode.EaseInOut
                        }
                    };

                    g.BeginAnimation(WidthProperty, c);
                    g.BeginAnimation(HeightProperty, c);
                    g.BeginAnimation(MarginProperty, t);
                    icon.BeginAnimation(WidthProperty, w);
                    icon.BeginAnimation(HeightProperty, h);
                    arc.BeginAnimation(WidthProperty, w);
                    arc.BeginAnimation(HeightProperty, h);
                    this.BeginAnimation(MarginProperty, t);
                    CloseTimer.Enabled = true;

                    MainTimer.Stop();
                });
            };

            number.Text = String.Format("{0:N0}", currentCd);
            AnimateCooldown();
        }

        private void CloseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RemoveSkill();
        }

        private void RemoveSkill()
        {
            this.Dispatcher.Invoke(() =>
            {
                MainTimer.Stop();
                MainTimer.Close();
                NumberTimer.Stop();
                NumberTimer.Close();
                CloseTimer.Stop();
                CloseTimer.Close();
                SkillEnded?.Invoke(Skill, Cooldown);
            });

        }

        void AnimateCooldown()
        {
            var a = new DoubleAnimation(359.9, 0, TimeSpan.FromMilliseconds(Cooldown));
            arc.BeginAnimation(Arc.EndAngleProperty, a);
            NumberTimer.Enabled = true;
            MainTimer.Enabled = true;
        }

    }
}
