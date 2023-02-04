﻿using Nostrum;
using Nostrum.Extensions;
using Nostrum.WPF;
using Nostrum.WPF.Extensions;
using Nostrum.WPF.ThreadSafe;
using System;
using System.Linq;
using System.Windows.Input;
using TCC.Debugging;
using TCC.Interop.Proxy;
using TCC.UI.Windows;
using TCC.Utilities;
using TCC.Utils;
using TCC.ViewModels;

namespace TCC.Data.Chat
{
    public class MessagePieceBase : ThreadSafeObservableObject, IDisposable
    {
        protected bool _customSize;
        protected int _fontSize = 18;
        private bool _isVisible;
        private ChatMessage? _container;

        public string Text { get; set; } = "";
        public int Size
        {
            get => _customSize ? _fontSize : App.Settings.FontSize;
            set
            {
                if (_fontSize == value) return;
                _fontSize = value;
                _customSize = value != App.Settings.FontSize;
                N(nameof(Size));
            }
        }

        public string Color { get; set; } = "";
        public virtual bool IsHovered { get; set; }
        public virtual ICommand ClickCommand { get; }

        public ChatMessage? Container
        {
            protected get => _container;
            set
            {
                if (_container == value) return;
                _container = value;
                if (string.IsNullOrEmpty(Color))
                {
                    Color = TccUtils.ChatChannelToBrush(Container?.Channel ?? ChatChannel.Say).Color.ToHex();
                }
            }
        }
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (value) SettingsWindowViewModel.FontSizeChanged += OnFontSizeChanged;
                else SettingsWindowViewModel.FontSizeChanged -= OnFontSizeChanged;

                if (_isVisible == value) return;
                _isVisible = value;
                N();
            }
        }

        protected MessagePieceBase()
        {
            ObjectTracker.Register(GetType());
            Dispatcher = ChatManager.Instance.Dispatcher;
            ClickCommand = new RelayCommand(_ => { });
        }

        ~MessagePieceBase()
        {
            ObjectTracker.Unregister(GetType());
        }

        private void OnFontSizeChanged()
        {
            N(nameof(Size));
        }
        public void Dispose()
        {
            _container = null;
            SettingsWindowViewModel.FontSizeChanged -= OnFontSizeChanged;
        }
    }

    public class SimpleMessagePiece : MessagePieceBase
    {
        public SimpleMessagePiece(string text)
        {
            Text = text;
        }
        public SimpleMessagePiece(string text, int fontSize, bool customSize, string col = "") : this(text)
        {
            _fontSize = fontSize;
            _customSize = customSize;
            Color = col;
        }
    }

    public class ActionMessagePiece : SimpleMessagePiece
    {
        private bool _isHovered;
        public override bool IsHovered
        {
            get => _isHovered;
            set
            {
                if (_isHovered == value) return;
                _isHovered = value;
                if (Container != null)
                {
                    var sameType = Container.Pieces.OfType<ActionMessagePiece>().Where(x => x.ChatLinkAction == ChatLinkAction);
                    sameType.ToList().ForEach(x => x.IsHovered = value);
                }
                N();
            }
        }

        public string ChatLinkAction { get; }

        public override ICommand ClickCommand { get; }

        public ActionMessagePiece(string text, string action) : base(text)
        {
            ChatLinkAction = action;
            ClickCommand = new RelayCommand(_ => StubInterface.Instance.StubClient.ChatLinkAction(ChatLinkAction));
        }
    }

    public class UrlMessagePiece : SimpleMessagePiece
    {
        private bool _isHovered;
        public override bool IsHovered
        {
            get => _isHovered;
            set
            {
                if (_isHovered == value) return;
                _isHovered = value;
                N();
            }
        }
        public override ICommand ClickCommand { get; }

        private UrlMessagePiece(string url) : base(url)
        {
            ClickCommand = new RelayCommand(_ =>
            {
                try
                {
                    Utils.Utilities.OpenUrl(Text);
                }
                catch
                {
                    TccMessageBox.Show(SR.UnableToOpenUrl(Text), MessageBoxType.Error);
                }
            });
        }
        public UrlMessagePiece(string text, int fontSize, bool customSize, string col = "") : this(text)
        {
            _fontSize = fontSize;
            _customSize = customSize;
            Color = col;
        }


    }
    public class MoneyMessagePiece : MessagePieceBase
    {
        public Money Money { get; set; }

        public MoneyMessagePiece(Money money)
        {
            Money = money;
            Text = Money.ToString();
        }
    }

    public class IconMessagePiece : MessagePieceBase
    {
        public IconMessagePiece(string source)
        {
            Text = source;
        }

        public IconMessagePiece(string source, int fontSize, bool customSize) : this(source)
        {
            _fontSize = fontSize;
            _customSize = customSize;
        }
    }

    //public class MessagePiece : ThreadSafeObservableObject, IDisposable
    //{
    //    private ChatMessage _container;

    //    public ChatMessage Container
    //    {
    //        private get => _container;
    //        set
    //        {
    //            if (_container == value) return;
    //            _container = value;
    //            if (Color == null)
    //            {
    //                var conv = new ChatChannelToColorConverter();
    //                var col = (SolidColorBrush)conv.Convert(Container.Channel, null, null, null);
    //                Color = col;
    //            }
    //        }
    //    }

    //    public long ItemUid { get; set; }
    //    public uint ItemId { get; set; }
    //    public Location Location { [UsedImplicitly] get; set; }
    //    public string RawLink { get; set; }
    //    public Money Money { get; set; }

    //    public BoundType BoundType { get; set; }

    //    public Thickness Spaces { get; set; }

    //    public string OwnerName { get; set; }

    //    public MessagePieceType Type { get; set; }

    //    public string Text { get; set; }

    //    public SolidColorBrush Color { get; set; }

    //    private bool _customSize;
    //    private bool _isVisible;
    //    private int _size = 18;

    //    public int Size
    //    {
    //        get => _customSize ? _size : App.Settings.FontSize;
    //        set
    //        {
    //            if (_size == value) return;
    //            _size = value;
    //            _customSize = value != App.Settings.FontSize;
    //            N(nameof(Size));
    //        }
    //    }

    //    public bool IsVisible
    //    {
    //        get => _isVisible;
    //        set
    //        {
    //            if (value) SettingsWindowViewModel.FontSizeChanged += OnFontSizeChanged;
    //            else SettingsWindowViewModel.FontSizeChanged -= OnFontSizeChanged;

    //            if (_isVisible == value) return;
    //            _isVisible = value;
    //            N();
    //        }
    //    }

    //    private bool _isHovered;
    //    public bool IsHovered
    //    {
    //        get => _isHovered;
    //        set
    //        {
    //            if (_isHovered == value) return;
    //            _isHovered = value;
    //            if (Container != null)
    //            { 
    //                var sameType = Container.Pieces.Where(x => x.Type == Type && x.RawLink == RawLink);
    //                sameType.ToList().ForEach(x => x.IsHovered = IsHovered);
    //            }
    //            N();
    //        }
    //    }

    //    private Thickness SetThickness(string text)
    //    {
    //        double left = 0;
    //        double right = 0;
    //        if (text.StartsWith(" "))
    //        {
    //            left = 0;
    //            right = -1;
    //        }
    //        if (text.EndsWith(" "))
    //        {
    //            right = 4;
    //            left = -1;
    //        }

    //        return new Thickness(left, 0, right, 0);

    //    }
    //    public void SetColor(string color)
    //    {
    //        if (color == "") return;
    //        Dispatcher.Invoke(() =>
    //        {
    //            //if (color == "")
    //            //{
    //            //    var conv = new ChatChannelToColorConverter();
    //            //    var col = ((SolidColorBrush)conv.Convert(Container.Channel, null, null, null));
    //            //    Color = col;
    //            //}
    //            //else
    //            //{
    //            //try
    //            //{
    //            Color = new SolidColorBrush(MiscUtils.ParseColor(color));
    //            //}
    //            //catch
    //            //{
    //            //    var conv = new ChatChannelToColorConverter();
    //            //    var col = ((SolidColorBrush)conv.Convert(Container.Channel, null, null, null));
    //            //    Color = col;
    //            //}
    //            //}
    //        });
    //    }
    //    public MessagePiece(string text, MessagePieceType type, int size, bool customSize, string col = "") : this(text)
    //    {
    //        if (col != "") SetColor(col);
    //        Type = type;
    //        _size = size;
    //        _customSize = customSize;

    //    }
    //    public MessagePiece(string text)
    //    {
    //        Dispatcher = ChatWindowManager.Instance.Dispatcher;
    //        Text = text;
    //        Spaces = SetThickness(text);
    //        _customSize = false;

    //    }

    //    private void OnFontSizeChanged()
    //    {
    //        N(nameof(Size));
    //    }

    //    public MessagePiece(Money money) : this(text: "")
    //    {
    //        Type = MessagePieceType.Money;
    //        Money = money;
    //        _customSize = false;
    //    }

    //    public void Dispose()
    //    {
    //        Color = null;
    //        _container = null;
    //        SettingsWindowViewModel.FontSizeChanged -= OnFontSizeChanged;
    //    }
    //}
}
