﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Colors = TCC.R.Colors;

namespace TCC.UI.Controls.Abnormalities;

public class TooltipParser
{
    const string GoodMarker = "H_W_GOOD";
    const string BadMarker = "H_W_BAD";
    const string CustomMarker = "H_W_CSTM";
    const string EndMarker = "COLOR_END";

    const string CarriageReturn1 = "$BR";
    const string CarriageReturn2 = "<br>";

    string _t;

    readonly List<Inline> _ret;

    public TooltipParser(string s)
    {
        _ret = new List<Inline>();
        _t = s;
        Clean();
        CorrectOrder();
        ReplaceHTML();
    }

    public List<Inline> Parse()
    {
        var pieces = SplitOn(_t, EndMarker);

        for (var i = 0; i < pieces.Length; i++)
        {
            var piece = pieces[i];

            if (i == pieces.Length - 1)
            {
                Add(piece);
            }
            else
            {
                if (ParseGood(piece)) continue;
                if (ParseBad(piece)) continue;
                if (ParseCustom(piece)) continue;
                System.Diagnostics.Debug.WriteLine("Failed to parse piece");
            }
        }

        return _ret;
    }

    void Clean()
    {
        _t = _t.Replace(CarriageReturn1, "\n")
            .Replace(CarriageReturn2, "\n")
            .Replace("color = ", "color=")
            .Replace("=\"", "='")
            .Replace("\">", "'>");
    }

    void CorrectOrder()
    {
        var correctionSplit = _t.Split('$');
        var swapped = false;
        for (var i = 0; i < correctionSplit.Length - 1; i++)
        {
            if (!correctionSplit[i].StartsWith(EndMarker)
                || correctionSplit[i - 1].StartsWith(GoodMarker)
                || correctionSplit[i - 1].StartsWith(BadMarker)) continue;

            if (correctionSplit[i + 1].StartsWith(GoodMarker))
            {
                correctionSplit[i] = correctionSplit[i].Replace(EndMarker, $"${GoodMarker}");
                correctionSplit[i + 1] = correctionSplit[i + 1].Replace(GoodMarker, $"${EndMarker}");
                swapped = true;
            }
            else if (correctionSplit[i + 1].StartsWith(BadMarker))
            {
                correctionSplit[i] = correctionSplit[i].Replace(EndMarker, $"${BadMarker}");
                correctionSplit[i + 1] = correctionSplit[i + 1].Replace(BadMarker, $"${EndMarker}");
                swapped = true;
            }
        }

        if (!swapped) return;
        _t = "";
        foreach (var s1 in correctionSplit) _t += s1;
    }

    void ReplaceHTML()
    {
        while (_t.Contains("<font"))
            _t = _t.Replace("<font color='", $"${CustomMarker}")
                .Replace("'>", "")
                .Replace("</font>", $"${EndMarker}");
    }

    bool ParseGood(string piece)
    {
        if (!IsGood(piece)) return false;
        var d = SplitOn(piece, GoodMarker);

        Add(d[0]);
        AddFormatted(d[1], Colors.TooltipGoodColor);
        return true;
    }

    bool ParseBad(string piece)
    {
        if (!IsBad(piece)) return false;
        var d = SplitOn(piece, BadMarker);

        Add(d[0]);
        AddFormatted(d[1], Colors.TooltipBadColor);
        return true;
    }

    bool ParseCustom(string piece)
    {
        if (!IsCustom(piece)) return false;
        var d = SplitOn(piece, CustomMarker);

        var txt = d[1].Substring(7);
        var col = d[1].Substring(0, 7);
        var cstm = new {Text = txt, Color = col};
        Add(d[0]);
        AddFormatted(cstm.Text, Nostrum.WPF.MiscUtils.ParseColor(cstm.Color));

        return true;
    }

    void Add(string content)
    {
        _ret.Add(new Run(content));
    }

    void AddFormatted(string content, Color color)
    {
        _ret.Add(new Run(content) {Foreground = new SolidColorBrush(color), FontWeight = FontWeights.DemiBold});
    }

    static string[] SplitOn(string input, string marker)
    {
        return input.Split(new[] {$"${marker}"}, StringSplitOptions.None);
    }

    static bool IsGood(string input)
    {
        return input.Contains($"${GoodMarker}");
    }

    static bool IsBad(string input)
    {
        return input.Contains($"${BadMarker}");
    }

    static bool IsCustom(string input)
    {
        return input.Contains($"${CustomMarker}");
    }
}