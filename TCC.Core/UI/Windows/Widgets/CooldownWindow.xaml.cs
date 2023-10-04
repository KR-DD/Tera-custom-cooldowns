﻿using TCC.ViewModels.Widgets;

namespace TCC.UI.Windows.Widgets;

public partial class CooldownWindow 
{
    public CooldownWindow(CooldownWindowViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;
        ButtonsRef = Buttons;
        BoundaryRef = Boundary;
        MainContent = WindowContent;
        Init(App.Settings.CooldownWindowSettings);
    }
}