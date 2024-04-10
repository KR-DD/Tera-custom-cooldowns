﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Nostrum.WPF.Factories;
using TCC.Utils;
using TCC.ViewModels;

namespace TCC.UI.Windows;

public partial class SettingsWindow
{
    private readonly DoubleAnimation _bigPathSlideAnim;
    private readonly DoubleAnimation _bigPathFadeAnim;

    public SettingsWindow() : base(false)
    {
        DataContext = new SettingsWindowViewModel();
        InitializeComponent();
        _bigPathSlideAnim = AnimationFactory.CreateDoubleAnimation(750, 0, -20, true);
        _bigPathFadeAnim = AnimationFactory.CreateDoubleAnimation(750, 1, 0, true);
    }

    public void ShowDialogAtPage(int idx)
    {
        Dispatcher?.Invoke(() =>
        {
            TabControl.SelectedIndex = idx;
            ShowDialog();
        });
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        HideWindow();
        App.Settings.Save();
    }

    private void OnBigPathLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement t) return;
        t.Opacity = 0;
        t.RenderTransform = new TranslateTransform(-20, 0);
        t.BeginAnimation(OpacityProperty, _bigPathFadeAnim);
        t.RenderTransform.BeginAnimation(TranslateTransform.XProperty, _bigPathSlideAnim);
    }

    private void OnTabBackgroundMouseLeftDown(object sender, MouseButtonEventArgs e)
    {
        Keyboard.ClearFocus();
        ((FrameworkElement)sender).Focus();
    }

    public override void ShowWindow()
    {
        base.ShowWindow();
        Dispatcher.InvokeAsync(() =>
        {
            ((SettingsWindowViewModel) DataContext).ExN(nameof(SettingsWindowViewModel.BlacklistedMonsters));
        });
    }

    // memeing
    private int _testNotifIdx;

    private readonly List<string> _lyrics = new()
    {
        "This was a triumph",
        "I'm making a note here:",
        "Huge success",
        "It's hard to overstate",
        "My satisfaction",
        "Aperture Science",
        "We do what we must",
        "Because we can",
        "For the good of all of us",
        "Except the ones who are dead", //9

        "But there's no sense crying",
        "Over every mistake",
        "You just keep on trying",
        "Till you run out of cake",
        "And the Science gets done",
        "And you make a neat gun",
        "For the people who are",
        "Still alive", //17

        "I'm not even angry",
        "I'm being so sincere right now",
        "Even though you broke my heart",
        "And killed me",
        "And tore me to pieces",
        "And threw every piece into a fire",
        "As they burned it hurt because",
        "I was so happy for you!", //25

        "Now these points of data",
        "Make a beautiful line",
        "And we're out of beta",
        "We're releasing on time",
        "So I'm glad. I got burned",
        "Think of all the things we learned",
        "For the people who are",
        "Still alive", //33

        "Go ahead and leave me",
        "I think I prefer to stay inside",
        "Maybe you'll find someone else",
        "To help you",
        "Maybe Black Mesa...",
        "That was A joke, ha ha, fat chance",
        "Anyway this cake is great",
        "It's so delicious and moist", //41

        "Look at me still talking when there's science to do",
        "When I look out there",
        "It makes me glad I'm not you",
        "I've experiments to be run",
        "There is research to be done",
        "On the people who are",
        "Still alive", //48

        "And believe me I am still alive",
        "I'm doing science and I'm still alive",
        "I feel fantastic and I'm still alive",
        "And while you're dying I'll be still alive",
        "And when you're dead I will be still alive",
        "Still alive",
        "Still alive" //55
    };

    private void TestNotification(object sender, RoutedEventArgs e)
    {
        var msg = _lyrics[_testNotifIdx];

        var type = _testNotifIdx switch
        {
            2 => NotificationType.Success,
            > 9 and <= 17 => NotificationType.Warning,
            > 33 and <= 41 => NotificationType.Error,
            > 48 => NotificationType.Success,
            _ => NotificationType.None
        };

        Log.N("GLaDOS", msg, type);
        _testNotifIdx++;
        if (_testNotifIdx >= _lyrics.Count) _testNotifIdx = 0;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.LeftCtrl) return;
        ((SettingsWindowViewModel) DataContext).ShowDebugSettings = true;
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.LeftCtrl) return;
        ((SettingsWindowViewModel)DataContext).ShowDebugSettings = false;
    }
}