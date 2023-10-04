﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TCC.Data.Chat;
using TCC.Settings.WindowSettings;
using TCC.Utils;
using TCC.ViewModels;

namespace TCC.UI.Controls.Chat;

public partial class TabSettingControl
{
    Tab? _dc;
    public TabSettingControl()
    {
        InitializeComponent();
    }

    void TabSettingControl_OnLoaded(object sender, RoutedEventArgs e)
    {
        _dc = DataContext as Tab;
    }

    void RemoveAuthor(object sender, RoutedEventArgs e)
    {
        var author = (string)((FrameworkElement)sender).DataContext;
        _dc?.TabInfoVM.Authors.Remove(author);
        _dc?.ApplyFilter();

    }

    void RemoveChannel(object sender, RoutedEventArgs e)
    {
        _dc?.TabInfoVM.ShowedChannels.Remove((ChatChannel)((FrameworkElement)sender).DataContext);
        _dc?.ApplyFilter();

    }

    void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.Down) || Keyboard.IsKeyDown(Key.Up))
            return;

        e.Handled = true;
    }

    void RemoveExAuthor(object sender, RoutedEventArgs e)
    {
        _dc?.TabInfoVM.ExcludedAuthors.Remove((string)((FrameworkElement)sender).DataContext);
        _dc?.ApplyFilter();

    }

    void RemoveExChannel(object sender, RoutedEventArgs e)
    {
        _dc?.TabInfoVM.ExcludedChannels.Remove((ChatChannel)((FrameworkElement)sender).DataContext);
        _dc?.ApplyFilter();

    }

    void NewChannelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (_dc == null) return;
            if (e.AddedItems.Count == 0) return;
            if (e.AddedItems[0] is ChatChannelOnOff i)
            {
                var ch = i.Channel;
                if (!_dc.TabInfoVM.ShowedChannels.Contains(ch))
                {
                    _dc.TabInfoVM.ShowedChannels.Add(ch);
                    _dc.ApplyFilter();
                }
            }

            if (sender is ComboBox s) s.SelectedIndex = -1;
        }
        catch (Exception)
        {
            // ignored
        }
    }

    void NewExChannelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (_dc == null) return;

            if (e.AddedItems.Count == 0) return;
            if (e.AddedItems[0] is ChatChannelOnOff i)
            {
                var ch = i.Channel;
                if (!_dc.TabInfoVM.ExcludedChannels.Contains(ch))
                {
                    _dc.TabInfoVM.ExcludedChannels.Add(ch);
                    _dc.ApplyFilter();
                }
            }

            if (sender is ComboBox s) s.SelectedIndex = -1;
        }
        catch (Exception)
        {
            // ignored
        }
    }

    void NewAuthorTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (_dc == null) return;
        var s = sender as TextBox;
        if (string.IsNullOrEmpty(s?.Text) || string.Equals(s.Text, "New author...")) return;
        if (_dc.TabInfoVM.Authors.Contains(s.Text)) return;
        _dc.TabInfoVM.Authors.Add(s.Text);
        _dc.ApplyFilter();

    }

    void NewAuthorTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        ((TextBox)sender).Text = "New author...";
    }

    void NewExAuthorTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (_dc == null) return;
        if (sender is not TextBox s || string.IsNullOrEmpty(s.Text) || string.Equals(s.Text, "New author...")) return;
        if (_dc.TabInfoVM.ExcludedAuthors.Contains(s.Text)) return;
        _dc.TabInfoVM.ExcludedAuthors.Add(s.Text);
        _dc.ApplyFilter();
    }

    void NewExAuthorTextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if ((sender as TextBox)?.Text != "New author...") return;
        ((TextBox)sender).Text = "";
    }

    void DeleteTab(object sender, RoutedEventArgs e)
    {
        if (_dc == null) return;

        var win = ChatManager.Instance.ChatWindows.FirstOrDefault(w => w.VM.Tabs.Contains(_dc));
        win?.VM.RemoveTab(_dc);
        win?.UpdateSettings();

        if (win?.VM.TabVMs.Count == 0)
        {
            win.Close();
            App.Settings.ChatWindowsSettings.Remove((ChatWindowSettings?)win.WindowSettings ?? throw new NullReferenceException("WindowSettings is null!"));
        }
        Window.GetWindow(this)?.Close();
    }

    void NewExKeywordTextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if ((sender as TextBox)?.Text != "New keyword...") return;
        ((TextBox)sender).Text = "";
    }

    void NewKeywordTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        ((TextBox)sender).Text = "New keyword...";
    }

    void NewKeywordTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (_dc == null) return;
        var s = sender as TextBox;
        if (string.IsNullOrEmpty(s?.Text) || string.Equals(s.Text, "New keyword...")) return;
        if (_dc.TabInfoVM.Keywords.Contains(s.Text)) return;
        _dc.TabInfoVM.Keywords.Add(s.Text);
        _dc.ApplyFilter();
    }

    void NewExKeywordTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (_dc == null) return;
        if (sender is not TextBox s || string.IsNullOrEmpty(s.Text) || string.Equals(s.Text, "New keyword...")) return;
        if (_dc.TabInfoVM.ExcludedKeywords.Contains(s.Text)) return;
        _dc.TabInfoVM.ExcludedKeywords.Add(s.Text);
        _dc.ApplyFilter();


    }

    void RemoveKeyword(object sender, RoutedEventArgs e)
    {
        _dc?.TabInfoVM.Keywords.Remove((string)((FrameworkElement)sender).DataContext);
        _dc?.ApplyFilter();
    }

    void RemoveExKeyword(object sender, RoutedEventArgs e)
    {
        _dc?.TabInfoVM.ExcludedKeywords.Remove((string)((FrameworkElement)sender).DataContext);
        _dc?.ApplyFilter();
    }
}