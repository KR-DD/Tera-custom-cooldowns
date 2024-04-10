﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using TCC.ViewModels;

namespace TCC.UI.Controls.Dashboard;

public partial class TimeMarkerControl
{
    private TimeMarker? _dc;

    public TimeMarkerControl()
    {
        InitializeComponent();
    }

    private void TimeMarkerControl_OnLoaded(object sender, RoutedEventArgs e)
    {
        _dc = (TimeMarker)DataContext;
        _dc.PropertyChanged += Dc_PropertyChanged;
    }

    private void Dc_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_dc == null) return;
        if (e.PropertyName != nameof(_dc.TimeFactor)) return;
        var w = TextBorder.ActualWidth;
        Dispatcher?.InvokeAsync(() => TextBorder.LayoutTransform = new TranslateTransform(-w * _dc.TimeFactor, 0));
    }
}