﻿using System;
using System.Globalization;
using System.Windows.Data;
using TCC.Utilities;
using TCC.Utils;

namespace TCC.UI.Converters;

public class ChatChannelToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ChatChannel ch) ch = ChatChannel.Say;

        return TccUtils.ChatChannelToBrush(ch);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}