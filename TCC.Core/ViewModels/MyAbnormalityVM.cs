﻿/* Add My Abnormals Setting by HQ
GroupAbnormalityVM  -> MyAbnormalityVM 
GroupAbnormals      -> MyAbnormals

ClassToggle         -> MyClassToggle
 */

using System.Windows.Input;
using System.Windows.Threading;
using Nostrum;
using Nostrum.WPF;
using Nostrum.WPF.ThreadSafe;
using TCC.Data;
using TCC.Data.Abnormalities;
using TCC.UI.Windows;
using TeraDataLite;

namespace TCC.ViewModels
{
    public class MyAbnormalityVM : ThreadSafeObservableObject   
    {
        private bool _special;
        private bool _hidden;
        public Abnormality Abnormality { get; }
        public ICommand SpecialCommand { get; }
        public ICommand HiddenCommand { get; }

        public bool Special
        {
            get => _special;
            set
            {
                if (_special == value) return;
                _special = value;
                N();
            }
        }
        public bool Hidden
        {
            get => _hidden;
            set
            {
                if (_hidden == value) return;
                _hidden = value;
                N();
            }
        }

        public bool CanBeSpecial => Abnormality.Type == AbnormalityType.Special || Abnormality.Type == AbnormalityType.Buff;

        public ThreadSafeObservableCollection<MyClassToggle> Classes { get; }

        public MyAbnormalityVM(Abnormality ab)
        {
            Abnormality = ab;
            Special = App.Settings.BuffWindowSettings.Specials.Contains(ab.Id);
            Classes = new ThreadSafeObservableCollection<MyClassToggle>(_dispatcher);
            for (var i = 0; i < 13; i++)
            {
                var ct = new MyClassToggle((Class)i, ab.Id);
                if (App.Settings.BuffWindowSettings.MyAbnormals.TryGetValue(ct.Class, out var list)) ct.Selected = list.Contains(ab.Id);
                Classes.Add(ct);
            }
            Classes.Add(new MyClassToggle(Class.Common, ab.Id)
            {
                Selected = App.Settings.BuffWindowSettings.MyAbnormals[Class.Common].Contains(ab.Id)
            });

            SpecialCommand = new RelayCommand(_ =>
            {
                if (!ab.IsBuff) return;
                Special = !Special;
                if (Special)
                {
                    App.Settings.BuffWindowSettings.Specials.Add(Abnormality.Id);
                    ab.Type = AbnormalityType.Special;
                }
                else
                {
                    App.Settings.BuffWindowSettings.Specials.Remove(Abnormality.Id);
                    ab.Type = AbnormalityType.Buff;
                }
            });

            HiddenCommand = new RelayCommand(_ =>
            {
                Hidden = !Hidden;
                if (Hidden)
                {
                    App.Settings.BuffWindowSettings.Hidden.Add(Abnormality.Id);
                }
                else
                {
                    App.Settings.BuffWindowSettings.Hidden.Remove(Abnormality.Id);
                }
            });
        }
    }
}