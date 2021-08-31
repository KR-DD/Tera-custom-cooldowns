﻿using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Nostrum.WPF.Factories;
using Nostrum.WinAPI;
using TCC.ViewModels;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Drawing.Point;

namespace TCC.UI.Controls.Chat
{
    public partial class PlayerMenuWindow
    {
        private readonly DoubleAnimation _openAnim;
        private readonly DoubleAnimation _closeAnim;

        public PlayerMenuWindow([NotNull] PlayerMenuViewModel vm)
        {
            _openAnim = AnimationFactory.CreateDoubleAnimation(150, 1);
            _closeAnim = AnimationFactory.CreateDoubleAnimation(150, 0, completed: (_, _) =>
            {
                UnfriendConfirmRipple.Reset();
                BlockConfirmRipple.Reset();
                KickConfirmRipple.Reset();
                GkickConfirmRipple.Reset();
                vm.Reset();
                Hide();
            });

            vm.UnfriendConfirmationRequested += () => UnfriendConfirmRipple.Trigger();
            vm.BlockConfirmationRequested += () => BlockConfirmRipple.Trigger();
            vm.KickConfirmationRequested += () => KickConfirmRipple.Trigger();
            vm.GKickConfirmationRequested += () => GkickConfirmRipple.Trigger();

            DataContext = vm;

            Loaded += (_, _) =>
            {
                var handle = new WindowInteropHelper(this).Handle;
                FocusManager.MakeUnfocusable(handle);
                FocusManager.HideFromToolBar(handle);
            };

            InitializeComponent();
        }

        public void AnimateOpening()
        {
            Topmost = false;
            Topmost = true;
            RootBorder.BeginAnimation(OpacityProperty, _openAnim);
        }

        public void ShowAndPosition()
        {
            Dispatcher?.InvokeAsync(() =>
            {
                FocusManager.PauseTopmost = true;
                var prevLeft = Left;
                var prevTop = Top;
                User32.GetCursorPos(out var pos);
                Show();

                var currScreen = Screen.FromPoint(new Point(Convert.ToInt32(Left), Convert.ToInt32(Top)));
                double top = pos.Y;
                double left = pos.X + 20;
                if (top > currScreen.Bounds.Height / 2D) top -= ActualHeight;

                if (double.IsNaN(prevLeft)) prevLeft = left;
                if (double.IsNaN(prevTop)) prevTop = top;

                BeginAnimation(TopProperty, AnimationFactory.CreateDoubleAnimation(200, top, prevTop, true));
                BeginAnimation(LeftProperty, AnimationFactory.CreateDoubleAnimation(200, left, prevLeft, true));
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            if (MgPopup.IsMouseOver) return;
            if (FpsUtilsPopup.IsMouseOver) return;
            var popupsOpen = MgPopup.IsOpen || FpsUtilsPopup.IsOpen;
            MgPopup.IsOpen = false;
            FpsUtilsPopup.IsOpen = false;
            if (IsMouseOver && popupsOpen) return;

            FocusManager.PauseTopmost = false;
            RootBorder.BeginAnimation(OpacityProperty, _closeAnim);
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            Close();
        }

        private void InspectClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PartyInviteClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GuildInviteClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WhisperClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GrantInviteClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DelegateLeaderClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MakeGuildMasterClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MoongourdClick(object sender, RoutedEventArgs routedEventArgs)
        {
            //var p = MgPopup.Child as MoongourdPopup;
            //p?.SetInfo(_vm.Name, App.Settings.LastLanguage);
            MgPopup.IsOpen = true;
        }

        private void FpsUtilsClick(object sender, RoutedEventArgs routedEventArgs)
        {
            FpsUtilsPopup.IsOpen = true;
        }
    }
}