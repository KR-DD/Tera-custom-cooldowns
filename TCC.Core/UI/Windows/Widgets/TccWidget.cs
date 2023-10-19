﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Nostrum.WPF.Extensions;
using Nostrum.WPF.Factories;
using TCC.Data;
using TCC.Settings.WindowSettings;
using TCC.UI.Controls;
using Size = System.Drawing.Size;

namespace TCC.UI.Windows.Widgets;

public class TccWidget : Window
{
    static bool _showBoundaries;
    static bool _hidden;
    static event Action? ShowBoundariesToggled;
    static event Action? HiddenToggled;
    static readonly List<TccWidget> _activeWidgets = new();

    readonly DoubleAnimation _opacityAnimation = AnimationFactory.CreateDoubleAnimation(100, 0);
    readonly DoubleAnimation _hideButtonsAnimation = AnimationFactory.CreateDoubleAnimation(1000, 0);
    readonly DoubleAnimation _showButtonsAnimation = AnimationFactory.CreateDoubleAnimation(150, 1);
    readonly DispatcherTimer _buttonsTimer;
    protected bool _canMove = true;
    Point WindowCenter => new(Left + ActualWidth / 2, Top + ActualHeight / 2);

    protected WindowButtons? ButtonsRef;
    protected UIElement? MainContent;
    protected UIElement? BoundaryRef;
    public WindowSettingsBase? WindowSettings { get; private set; }
    public IntPtr Handle { get; private set; }

    protected TccWidget()
    {
        _buttonsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _activeWidgets.Add(this);
    }


    protected void Init(WindowSettingsBase settings)
    {

        WindowSettings = settings;
        if (MainContent != null) MainContent.Opacity = 0;
        if (BoundaryRef != null) BoundaryRef.Opacity = 0;
        Topmost = true;
        Left = WindowSettings.X * WindowManager.ScreenSize.Width;
        Top = WindowSettings.Y * WindowManager.ScreenSize.Height;
        if (!WindowSettings.IgnoreSize)
        {
            if (WindowSettings.H != 0) Height = WindowSettings.H;
            if (WindowSettings.W != 0) Width = WindowSettings.W;
        }

        CheckBounds();

        WindowSettings.EnabledChanged += OnEnabledChanged;
        WindowSettings.ClickThruModeChanged += OnClickThruModeChanged;
        WindowSettings.VisibilityChanged += OnWindowVisibilityChanged;
        WindowSettings.ResetToCenter += ResetToCenter;

        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
        HiddenToggled += OnHiddenToggled;

        WindowManager.VisibilityManager.VisibilityChanged += OnVisibilityChanged;
        WindowManager.VisibilityManager.DimChanged += OnDimChanged;
        WindowManager.VisibilityManager.ClickThruChanged += OnClickThruModeChanged;
        WindowManager.RepositionRequestedEvent += ReloadPosition;
        WindowManager.ResetToCenterEvent += ResetToCenter;
        WindowManager.DisposeEvent += CloseWindowSafe;
        WindowManager.MakeGlobalEvent += WindowSettings.MakePositionsGlobal;

        FocusManager.TeraScreenChanged += OnTeraScreenChanged;
        FocusManager.FocusTick += OnFocusTick;

        OnClickThruModeChanged();
        OnVisibilityChanged();
        OnWindowVisibilityChanged(WindowSettings.Visible);

        FocusManager.MakeUnfocusable(Handle);

        if (BoundaryRef != null)
        {
            ShowBoundariesToggled += ShowHideBoundaries;
            if (_canMove)
                BoundaryRef.MouseLeftButtonDown += Drag;
        }
        if (ButtonsRef == null)
        {
            if (_canMove) MouseLeftButtonDown += Drag;
        }
        else
        {
            ButtonsRef.Opacity = 0;
            _buttonsTimer.Tick += OnButtonsTimerTick;

            MouseEnter += (_, _) =>
            {
                if (!App.Settings.HideHandles) ButtonsRef.BeginAnimation(OpacityProperty, _showButtonsAnimation);
            };
            MouseLeave += (_, _) => _buttonsTimer.Start();
            if (_canMove) ButtonsRef.MouseLeftButtonDown += Drag;
        }
    }

    void OnHiddenToggled()
    {
        OnVisibilityChanged();
    }

    void OnTeraScreenChanged(System.Drawing.Point oldPos, System.Drawing.Point newPos, Size size)
    {
        var op = new Point(oldPos.X, oldPos.Y); //sigh
        var np = new Point(newPos.X, newPos.Y); //sigh
        var s = new System.Windows.Size(size.Width, size.Height); //sigh

        WindowSettings?.ApplyScreenOffset(op, np, s);
        ReloadPosition();
    }

    void ShowHideBoundaries()
    {
        var anim = _showBoundaries ? _showButtonsAnimation : _hideButtonsAnimation;
        Dispatcher?.InvokeAsync(() =>
        {
            BoundaryRef?.BeginAnimation(OpacityProperty, anim);
            ButtonsRef?.BeginAnimation(OpacityProperty, anim);
            OnClickThruModeChanged();
        });
    }

    protected void ReloadPosition()
    {
        if (WindowSettings == null) return;
        Dispatcher?.InvokeAsync(() =>
        {
            var left = WindowSettings.X * WindowManager.ScreenSize.Width;
            Left = left >= int.MaxValue ? 0 : left;

            var top = WindowSettings.Y * WindowManager.ScreenSize.Height;
            Top = top >= int.MaxValue ? 0 : top;

            CheckBounds();
            UpdateButtons();
        });
    }

    public void ResetToCenter()
    {
        Dispatcher?.Invoke(() =>
        {
            var dpi = this.GetDpiScale();

            Left = (FocusManager.TeraScreen.Bounds.X + FocusManager.TeraScreen.Bounds.Width / 2f - ActualWidth / 2) / dpi.DpiScaleX;
            Top = (FocusManager.TeraScreen.Bounds.Y + FocusManager.TeraScreen.Bounds.Height / 2f - ActualHeight / 2) / dpi.DpiScaleY;
            SetRelativeCoordinates();
        });
    }

    void OnFocusTick()
    {
        if (FocusManager.PauseTopmost) return;
        if (WindowSettings?.ShowAlways == true || WindowManager.VisibilityManager.ForceVisible) RefreshTopmost();
        if (WindowManager.VisibilityManager.Visible) RefreshTopmost();
    }

    void OnWindowVisibilityChanged(bool visible)
    {
        SetVisibility(visible);
    }

    void OnButtonsTimerTick(object? sender, EventArgs e)
    {
        _buttonsTimer.Stop();
        if (IsMouseOver || _showBoundaries) return;
        ButtonsRef?.BeginAnimation(OpacityProperty, _hideButtonsAnimation);
    }

    void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (WindowSettings == null) return;
        if (!WindowSettings.AllowOffScreen) CheckBounds();
        if (WindowSettings.IgnoreSize) return;
        if (WindowSettings.W == ActualWidth && WindowSettings.H == ActualHeight) return;
        WindowSettings.W = ActualWidth;
        WindowSettings.H = ActualHeight;
        if (!App.Loading) App.Settings.Save();
    }

    protected virtual void OnLoaded(object sender, RoutedEventArgs e)
    {
        Handle = new WindowInteropHelper(this).Handle;
        FocusManager.MakeUnfocusable(Handle);
        FocusManager.HideFromToolBar(Handle);
        if (WindowSettings?.Enabled == false) Hide();
    }

    void OnDimChanged()
    {
        if (!WindowManager.VisibilityManager.Visible) return;
        if (WindowSettings == null) return;
        if (_hidden)
        {
            AnimateContentOpacity(0);
            return;
        }

        if (!WindowSettings.AutoDim || WindowSettings.ForcedVisible)
        {
            AnimateContentOpacity(WindowSettings.MaxOpacity);
        }
        else
        {
            if (WindowSettings.UndimOnFlyingGuardian)
                AnimateContentOpacity(WindowManager.VisibilityManager.Dim
                    ? WindowSettings.DimOpacity
                    : WindowSettings.MaxOpacity);
            else if (FlyingGuardianDataProvider.IsInProgress) AnimateContentOpacity(WindowSettings.DimOpacity);
            else
                AnimateContentOpacity(WindowManager.VisibilityManager.Dim
                    ? WindowSettings.DimOpacity
                    : WindowSettings.MaxOpacity);
        }

        OnClickThruModeChanged();
    }

    protected virtual void OnVisibilityChanged()
    {
        if (WindowSettings == null) return;

        if (WindowManager.VisibilityManager.Visible && !_hidden || WindowSettings.ShowAlways)
        {

            if (WindowManager.VisibilityManager.Dim && WindowSettings.AutoDim && !WindowSettings.ForcedVisible)
                AnimateContentOpacity(WindowSettings.DimOpacity);
            else
                AnimateContentOpacity(WindowSettings.MaxOpacity);
        }
        else
        {
            if (WindowSettings.ShowAlways && !_hidden) return;
            AnimateContentOpacity(0);
        }

        RefreshTopmost();
    }

    void OnClickThruModeChanged()
    {
        if (_showBoundaries)
        {
            FocusManager.UndoClickThru(Handle);
            return;
        }

        if (WindowSettings == null) return;

        switch (WindowSettings.ClickThruMode)
        {
            case ClickThruMode.Never:
                FocusManager.UndoClickThru(Handle);
                break;
            case ClickThruMode.Always:
                FocusManager.MakeClickThru(Handle);
                break;
            case ClickThruMode.WhenDim:
                if (WindowManager.VisibilityManager.Dim) FocusManager.MakeClickThru(Handle);
                else FocusManager.UndoClickThru(Handle);
                break;
            case ClickThruMode.WhenUndim:
                if (WindowManager.VisibilityManager.Dim) FocusManager.UndoClickThru(Handle);
                else FocusManager.MakeClickThru(Handle);
                break;
            case ClickThruMode.GameDriven:
                if (Game.InGameUiOn) FocusManager.UndoClickThru(Handle);
                else FocusManager.MakeClickThru(Handle);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected virtual void OnEnabledChanged(bool enabled)
    {
        try
        {
            Dispatcher?.Invoke(() =>
            {
                if (WindowSettings!.Enabled) Show();
                else Hide();
            });
        }
        catch
        {
            // ignored
        }
    }

    void AnimateContentOpacity(double opacity)
    {
        if (MainContent == null) return;
        Dispatcher?.InvokeAsync(() =>
            {
                _opacityAnimation.To = opacity;
                MainContent.BeginAnimation(OpacityProperty, _opacityAnimation);
            }
            , DispatcherPriority.DataBind);
    }

    void RefreshTopmost()
    {
        Dispatcher?.InvokeAsync(() =>
        {
            if (FocusManager.PauseTopmost) return;
            Topmost = false;
            Topmost = true;
        }, DispatcherPriority.DataBind);
    }

    void SetVisibility(bool v)
    {
        if (Dispatcher?.Thread.IsAlive == false) return;
        Dispatcher?.Invoke(() =>
        {
            Visibility = !v ? Visibility.Visible : Visibility.Collapsed; // meh ok
            Visibility = v ? Visibility.Visible : Visibility.Collapsed;
        });
    }

    void CheckBounds()
    {
        if (WindowSettings == null) return;

        if (WindowSettings.AllowOffScreen) return;
        if (Left + ActualWidth > SystemParameters.VirtualScreenWidth)
            Left = SystemParameters.VirtualScreenWidth - ActualWidth;
        if (Top + ActualHeight > SystemParameters.VirtualScreenHeight)
            Top = SystemParameters.VirtualScreenHeight - ActualHeight;
        CheckIndividualScreensBounds();
        SetRelativeCoordinates();
    }

    void CheckIndividualScreensBounds()
    {
        if (IsWindowFullyVisible()) return;
        var nearestScreen = FindNearestScreen();

        if (Top + ActualHeight > nearestScreen.Bounds.Y + nearestScreen.Bounds.Height)
            Top = nearestScreen.Bounds.Y + nearestScreen.Bounds.Height - ActualHeight;
        else if (Top < nearestScreen.Bounds.Y) Top = nearestScreen.Bounds.Y;
        if (Left + ActualWidth > nearestScreen.Bounds.X + nearestScreen.Bounds.Width)
            Left = nearestScreen.Bounds.X + nearestScreen.Bounds.Width - ActualWidth;
        else if (Left < nearestScreen.Bounds.X) Left = nearestScreen.Bounds.X;
    }

    Screen FindNearestScreen()
    {
        var screenFromWinCenter = ScreenFromWindowCenter();
        if (screenFromWinCenter != null) return screenFromWinCenter;

        var distances = Screen.AllScreens.Select(screen => new Point(screen.Bounds.X + screen.Bounds.Size.Width / 2f, screen.Bounds.Y + screen.Bounds.Size.Height / 2f))
            .Select(screenCenter => screenCenter - WindowCenter)
            .ToList();

        var min = new Vector(double.MaxValue, double.MaxValue);
        foreach (var distance in distances.Where(distance => distance.Length < min.Length))
        {
            min = distance;
        }
        var index = distances.IndexOf(min);
        return Screen.AllScreens[index != -1 ? index : 0];
    }


    bool IsWindowFullyVisible()
    {
        var tl = false;
        var tr = false;
        var bl = false;
        var br = false;
        foreach (var screen in Screen.AllScreens)
        {
            if (IsTopLeftCornerInScreen(screen)) tl = true;
            if (IsTopRightCornerInScreen(screen)) tr = true;
            if (IsBottomLeftCornerInScreen(screen)) bl = true;
            if (IsBottomRightCornerInScreen(screen)) br = true;
        }

        return tl && tr && bl && br;
    }

    bool IsTopLeftCornerInScreen(Screen screen)
    {
        return screen.Bounds.Contains(Convert.ToInt32(Left), Convert.ToInt32(Top));
    }

    bool IsBottomRightCornerInScreen(Screen screen)
    {
        return screen.Bounds.Contains(Convert.ToInt32(Left + ActualWidth), Convert.ToInt32(Top + ActualHeight));
    }

    bool IsTopRightCornerInScreen(Screen screen)
    {
        return screen.Bounds.Contains(Convert.ToInt32(Left + ActualWidth), Convert.ToInt32(Top));
    }

    bool IsBottomLeftCornerInScreen(Screen screen)
    {
        return screen.Bounds.Contains(Convert.ToInt32(Left), Convert.ToInt32(Top + ActualHeight));
    }

    Screen? ScreenFromWindowCenter()
    {
        return Screen.AllScreens.FirstOrDefault(x =>
            x.Bounds.Contains(Convert.ToInt32(WindowCenter.X), Convert.ToInt32(WindowCenter.Y)));
    }

    void UpdateButtons()
    {
        if (ButtonsRef == null) return;

        var screenMiddle = WindowManager.ScreenSize.Height / 2f;
        var middle = (Top*this.GetDpiScale().DpiScaleY) + Height / 2f;
        var deadzone = WindowManager.ScreenSize.Height / 15f;
        var distance = Math.Abs(screenMiddle - middle);

        if (!(distance > deadzone)) return;
        if (WindowSettings == null) return;

        if (middle >= screenMiddle)
        {

            WindowSettings.ButtonsPosition = ButtonsPosition.Above;
            Grid.SetRow(ButtonsRef, 0);
        }
        else
        {
            WindowSettings.ButtonsPosition = ButtonsPosition.Below;
            Grid.SetRow(ButtonsRef, 2);
        }
    }

    protected void Drag(object sender, MouseButtonEventArgs e)
    {
        var currOp = Opacity;
        if (WindowSettings != null)
        {
            if (!WindowSettings.IgnoreSize) ResizeMode = ResizeMode.NoResize;
            if (!_showBoundaries) BoundaryRef?.BeginAnimation(OpacityProperty, _showButtonsAnimation);
        }
        Opacity = .7;
        this.TryDragMove();
        if (WindowSettings != null)
        {
            if (!_showBoundaries) BoundaryRef?.BeginAnimation(OpacityProperty, _hideButtonsAnimation);
        }
        Opacity = currOp;
        if (WindowSettings == null) return;

        UpdateButtons();
        CheckBounds();
        if (!WindowSettings.IgnoreSize) ResizeMode = ResizeMode.CanResize;
        SetRelativeCoordinates();
        App.Settings.Save();
    }

    void SetRelativeCoordinates()
    {
        if (WindowSettings == null) return;

        WindowSettings.X = Left / WindowManager.ScreenSize.Width;
        WindowSettings.Y = Top / WindowManager.ScreenSize.Height;
    }


    public void CloseWindowSafe()
    {
        Dispatcher?.Invoke(() =>
        {
            WindowManager.VisibilityManager.VisibilityChanged -= OnVisibilityChanged;
            WindowManager.VisibilityManager.DimChanged -= OnDimChanged;
            WindowManager.VisibilityManager.ClickThruChanged -= OnClickThruModeChanged;
            WindowManager.RepositionRequestedEvent -= ReloadPosition;
            WindowManager.ResetToCenterEvent -= ResetToCenter;
            WindowManager.DisposeEvent -= CloseWindowSafe;
            FocusManager.FocusTick -= OnFocusTick;
            if (WindowSettings != null)
            {
                WindowManager.MakeGlobalEvent -= WindowSettings.MakePositionsGlobal;
                WindowSettings.EnabledChanged -= OnEnabledChanged;
                WindowSettings.ClickThruModeChanged -= OnClickThruModeChanged;
                WindowSettings.VisibilityChanged -= OnWindowVisibilityChanged;
                WindowSettings.ResetToCenter -= ResetToCenter;
            }

            Loaded -= OnLoaded;
            SizeChanged -= OnSizeChanged;
            Close();
            _activeWidgets.Remove(this);
        });

        if (Dispatcher == App.BaseDispatcher) return;
        //Log.CW($"[{GetType().Name}] Invoking dispatcher shutdown");
        Dispatcher?.Invoke(() => Thread.Sleep(100)); //uhmmmmm ok
        Dispatcher?.BeginInvokeShutdown(DispatcherPriority.ContextIdle);
    }

    public static void OnShowAllHandlesToggled()
    {
        _showBoundaries = !_showBoundaries;
        ShowBoundariesToggled?.Invoke();
    }

    public static void OnHideAllToggled()
    {
        _hidden = !_hidden;
        HiddenToggled?.Invoke();
    }

    public static bool Exists(IntPtr handle)
    {
        return _activeWidgets.Any(w => w.Handle == handle && w.Handle != IntPtr.Zero);
    }
}