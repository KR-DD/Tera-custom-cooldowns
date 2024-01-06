﻿/* Add My Abnormals Setting by HQ
GroupAbnormalConfigWindow   -> MyAbnormalConfigWindow
GroupConfigVM               -> MyAbnormalConfigVM
GroupAbnormalityViewModel          -> MyAbnormalityVM
GroupAbnormals              -> MyAbnormals

ClassToggle                 -> MyClassToggle
ToggleCommand               -> MyToggleCommand
*/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Nostrum.WPF;
using Nostrum.WPF.Extensions;
using Nostrum.WPF.Factories;
using Nostrum.WPF.ThreadSafe;
using TCC.ViewModels;
using TeraDataLite;

namespace TCC.UI.Windows;

public partial class MyAbnormalConfigWindow
{
    static MyAbnormalConfigWindow? _instance;
    public static MyAbnormalConfigWindow Instance => _instance ?? new MyAbnormalConfigWindow();
    Class _currentFilter;

    public MyAbnormalConfigVM DC { get; private set; }


    readonly DispatcherTimer _searchCooldown;
    string _searchText = "";

    public MyAbnormalConfigWindow() : base(true)
    {
        _instance = this;
        InitializeComponent();
        DC = new MyAbnormalConfigVM();
        DataContext = DC;
        _searchCooldown = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Background, OnSearchTriggered, Dispatcher ?? throw new InvalidOperationException());
    }

    void OnSearchTriggered(object? sender, EventArgs e)
    {
        _searchCooldown.Stop();
        if (string.IsNullOrWhiteSpace(_searchText)) return;
        var view = DC.AbnormalitiesView;
        view.Filter = o => ((MyAbnormalityVM)o).Abnormality.Name.IndexOf(_searchText, StringComparison.InvariantCultureIgnoreCase) != -1;
        view.Refresh();
    }


    void PassivitySearch_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = ((TextBox)sender).Text;
        if (string.IsNullOrWhiteSpace(_searchText)) return;
        _searchCooldown.Refresh();
    }

    void Close(object sender, RoutedEventArgs e)
    {
        App.Settings.Save();
        var an = AnimationFactory.CreateDoubleAnimation(200, 0, completed: (_, _) =>
        {
            DC.Dispose();
            _instance = null;
            Close();
            if (App.Settings.ForceSoftwareRendering)
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
        });
        BeginAnimation(OpacityProperty, an);
    }

    void FilterByClass(object sender, RoutedEventArgs e)
    {
        var c = (Class)((FrameworkElement)sender).DataContext;
        var view = DC.AbnormalitiesView;
        if (SearchBox.Text.Length > 0)
        {
            SearchBox.Clear();
            view.Filter = null;
        }

        if (view.Filter == null || c != _currentFilter)
        {
            view.Filter = o => ((MyAbnormalityVM)o).Classes.Any(x => x.Class == c && x.Selected);
            _currentFilter = c;
        }
        else
        {
            view.Filter = null;
            _currentFilter = Class.None;
        }

        view.Refresh();

        //TODO: this is bad and i should feel bad
        foreach (var x in ClassesButtons.Items)
        {
            var cp = (ContentPresenter)ClassesButtons.ItemContainerGenerator.ContainerFromItem(x);
            var btn = cp.ContentTemplate.FindName("Btn", cp) as Button;
            if (btn?.DataContext == null) continue;
            var dc = (Class)btn.DataContext;
            if (dc == _currentFilter) btn.Opacity = 1;
            else btn.Opacity = .3;
        }
    }
}

public class MyClassToggle : ThreadSafeObservableObject
{
    bool _selected;
    public bool Selected
    {
        get => _selected;
        set => RaiseAndSetIfChanged(value, ref _selected);
    }
    public Class Class { get; }
    public ICommand MyToggleCommand { get; }
    public uint AbnormalityId { get; }
    public MyClassToggle(Class c, uint abId)
    {
        Class = c;
        AbnormalityId = abId;
        MyToggleCommand = new RelayCommand(_ =>
        {
            Selected = !Selected;
            if (Selected) App.Settings.BuffWindowSettings.MyAbnormals[Class].Add(AbnormalityId);
            else App.Settings.BuffWindowSettings.MyAbnormals[Class].Remove(AbnormalityId);
        });
    }
}