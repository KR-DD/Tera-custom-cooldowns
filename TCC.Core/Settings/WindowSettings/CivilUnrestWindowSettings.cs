﻿using TCC.Data;
using TCC.UI.Windows.Widgets;

namespace TCC.Settings.WindowSettings;

public class CivilUnrestWindowSettings : WindowSettingsBase
{
    public override bool Enabled { get => false; set { } }

    public CivilUnrestWindowSettings()
    {
        _visible = true;
        _clickThruMode = ClickThruMode.Never;
        _scale = 1;
        _autoDim = true;
        _dimOpacity = .5;
        _showAlways = false;
        _enabled = false;
        _allowOffScreen = false;
        Positions = new ClassPositions(1, .45, ButtonsPosition.Above);

        UndimOnFlyingGuardian = false;

    }
}