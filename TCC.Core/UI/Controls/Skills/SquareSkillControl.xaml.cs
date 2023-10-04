﻿using System.Windows;
using TCC.Data;

namespace TCC.UI.Controls.Skills;

public partial class SquareSkillControl 
{
    public SquareSkillControl()
    {
        InitializeComponent();
        MainArcRef = Arc;
        PreArcRef = PreArc;
        HideButtonRef = HideButton;

    }

    protected override void OnLoaded(object sender, RoutedEventArgs e)
    {
        base.OnLoaded(sender, e);
        if (Context == null) return;

        if (Context.Duration == 0)
        {
            OnCooldownEnded(Context.Mode);
        }
    }

    protected override void OnCooldownEnded(CooldownMode mode)
    {
        base.OnCooldownEnded(mode);
        if (Context == null) return;
        if (mode != CooldownMode.Normal) return;
        WindowManager.ViewModels.CooldownsVM.Remove(Context.Skill);
    }
}