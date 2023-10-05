﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Threading;
using Nostrum.WinAPI;
using TCC.Data;

namespace TCC.UI;

public sealed class KeyboardHook : IDisposable
{
    static KeyboardHook? _instance;
    public static KeyboardHook Instance => _instance ??= new KeyboardHook();

    readonly Window _window;
    readonly Dictionary<HotKey, Action> _callbacks;
    int _currentId;
    bool _isRegistered;
    bool _isInitialized;

    public event Action<HotKey>? KeyPressed;

    KeyboardHook()
    {
        _callbacks = new Dictionary<HotKey, Action>();
        _window = new Window();
        // register the event of the inner native window.
        _window.KeyPressed += key => KeyPressed?.Invoke(key);
    }

    public void Enable()
    {
        if (_isInitialized) return;
        App.BaseDispatcher.Invoke(() =>
        {
            // register the event that is fired after the key press.
            Instance.KeyPressed += OnKeyPressed;
            Game.ChatModeChanged += BeginCheckEnable;
            FocusManager.ForegroundChanged += BeginCheckEnable;
            _isInitialized = true;
            BeginCheckEnable();

        });
    }

    public void Disable()
    {
        if (!_isInitialized) return;
        App.BaseDispatcher.Invoke(() =>
        {
            if (_isRegistered) ClearHotkeys();
            // register the event that is fired after the key press.
            Instance.KeyPressed -= OnKeyPressed;
            Game.ChatModeChanged -= BeginCheckEnable;
            FocusManager.ForegroundChanged -= BeginCheckEnable;
            _isInitialized = false;
        });
    }

    public void RegisterCallback(HotKey hk, Action callback)
    {
        _callbacks[hk] = callback;
    }

    public void ChangeHotkey(HotKey oldHk, HotKey newHk)
    {
        if (!_callbacks.TryGetValue(oldHk, out var cb)) return;
        Console.WriteLine($"Changing {oldHk} to {newHk}");
        _callbacks[newHk] = cb;
        _callbacks.Remove(oldHk);
        BeginCheckEnable();
    }

    void CheckEnable(bool value)
    {
        if (value && !_isRegistered)
        {
            RegisterHotkeys();
            return;
        }

        if (value || !_isRegistered) return;
        ClearHotkeys();
    }

    void BeginCheckEnable()
    {
        App.BaseDispatcher.InvokeAsync(
            () => { CheckEnable(!Game.InGameChatOpen && FocusManager.IsForeground); },
            DispatcherPriority.Background);
    }

    void RegisterHotkeys()
    {
        //Console.WriteLine("RegisterHotkeys()");
        _callbacks.Keys.ToList().ForEach(RegisterHotKey);
        _isRegistered = true;
    }

    void RegisterHotKey(ModifierKeys modifier, Keys key)
    {
        if (key == Keys.None) return; //allow disable hotkeys using "None" key
        // increment the counter.
        _currentId++;

        // register the hot key.
        User32.RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key);
    }

    void RegisterHotKey(HotKey hk)
    {
        RegisterHotKey(hk.Modifier, hk.Key);
    }

    void OnKeyPressed(HotKey hk)
    {
        if (!_callbacks.TryGetValue(hk, out var cb)) return;
        Console.WriteLine($"Executing callback for {hk}");
        cb.DynamicInvoke();
    }



    #region Window

    /// <summary>
    ///     Represents the window that is used internally to get the messages.
    /// </summary>
    sealed class Window : NativeWindow, IDisposable
    {
        const int WmHotkey = 0x0312;

        public Window()
        {
            // create the handle for the window.
            CreateHandle(new CreateParams());
        }

        #region IDisposable Members

        public void Dispose()
        {
            DestroyHandle();
        }

        #endregion

        /// <summary>
        ///     Overridden to get the notifications.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // check if we got a hot key pressed.
            if (m.Msg != WmHotkey) return;
            // get the keys.
            var key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
            var modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

            // invoke the event to notify the parent.
            KeyPressed?.Invoke(new HotKey(key, modifier));
        }

        public event Action<HotKey>? KeyPressed;
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        // unregister all the registered hot keys.
        ClearHotkeys();

        // dispose the inner native window.
        _window.Dispose();
    }

    void ClearHotkeys()
    {
        //Console.WriteLine("ClearHotkeys()");

        for (var i = _currentId; i > 0; i--) User32.UnregisterHotKey(_window.Handle, i);
        _currentId = 0;
        _isRegistered = false;
    }

    #endregion
}