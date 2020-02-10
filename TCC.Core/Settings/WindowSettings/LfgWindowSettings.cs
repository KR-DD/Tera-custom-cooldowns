﻿using System;
using TCC.Interop.Proxy;

namespace TCC.Settings.WindowSettings
{
    public class LfgWindowSettings : WindowSettingsBase
    {
        private bool _hideTradeListings;
        public event Action HideTradeListingsChangedEvent;
        private int _minLevel;

        public int MinLevel
        {
            get => _minLevel;
            set
            {
                if (_minLevel == value) return;
                if (value < 1) value = 1;
                if (value > 70) value = 70;
                _minLevel = value;
                if (value > _maxLevel) MaxLevel = value;
            }
        }
        private int _maxLevel;

        public int MaxLevel
        {
            get => _maxLevel;
            set
            {
                if (_maxLevel == value) return;
                if (value < 1) value = 1;
                if (value > 70) value = 70;
                _maxLevel = value;
                if (value < _minLevel) MinLevel = value;
            }
        }

        public bool HideTradeListings
        {
            get => _hideTradeListings;
            set
            {
                if (_hideTradeListings == value) return;
                _hideTradeListings = value;
                N();
                HideTradeListingsChangedEvent?.Invoke();
            }
        }

        public LfgWindowSettings()
        {
            HideTradeListings = true;
            MinLevel = 60;
            MaxLevel = 70;

            GpkNames.Add("PartyBoard");
            GpkNames.Add("PartyBoardMemberInfo");
        }

        protected override void OnEnabledChanged(bool enabled)
        {
            ProxyInterface.Instance.Stub.UpdateSetting("useLfg", enabled);
            // do nothing
        }
    }
}