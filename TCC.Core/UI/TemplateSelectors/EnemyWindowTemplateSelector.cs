﻿using System.Windows;
using System.Windows.Controls;
using TCC.Data;

namespace TCC.UI.TemplateSelectors;

public class EnemyWindowTemplateSelector : DataTemplateSelector
{
    public DataTemplate? Default { get; set; }
    public DataTemplate? Phase1 { get; set; }
    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item == null) return null;
        //if (((ViewModels.BossGageWindowViewModel)item).CurrentHHphase == HarrowholdPhase.Phase1) return Phase1;
        if ((HarrowholdPhase)item == HarrowholdPhase.Phase1) return Phase1;
        else return Default;
    }
}