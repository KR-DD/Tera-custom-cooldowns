﻿using System.Windows;
using TCC.Data;

namespace TCC.UI.Controls.Skills;

public partial class RoundSkillControl
{
    public RoundSkillControl()
    {
        InitializeComponent();
        MainArcRef = Arc;
        PreArcRef = PreArc;
        HideButtonRef = HideButton;
    }

    protected override void OnLoaded(object sender, RoutedEventArgs e)
    {
        base.OnLoaded(sender, e);
        if (Context?.Duration != 0) return;

        OnCooldownEnded(Context.Mode);
    }

    protected override void OnCooldownEnded(CooldownMode mode)
    {
        base.OnCooldownEnded(mode);
        if (mode != CooldownMode.Normal) return;
        if (Context == null) return;
        WindowManager.ViewModels.CooldownsVM.Remove(Context.Skill);
    }
}