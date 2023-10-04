﻿using System.Windows;
using TCC.Data;

namespace TCC.UI.Windows.Widgets;

public class ClassPositionsData
{
    public Point Position { get; set; }
    public ButtonsPosition Buttons { get; set; }

    public ClassPositionsData()
    {

    }
    public ClassPositionsData(ClassPositionsData origin)
    {
        Position = new Point(origin.Position.X, origin.Position.Y);
        Buttons = origin.Buttons;
    }

    public ClassPositionsData(double x, double y, ButtonsPosition buttons)
    {
        Position = new Point(x, y);
        Buttons = buttons;
    }

    public void ApplyCorrection(Size sc)
    {
        Position = new Point(sc.Width * Position.X, sc.Height * Position.Y);
    }

    public void ApplyOffset(Point oldPos, Point newPos, Size size)
    {
        var offsetW =  (newPos.X - oldPos.X )/ size.Width;
        var offsetH =  (newPos.Y - oldPos.Y)/ size.Height;

        var p = new Point {X = Position.X + offsetW, Y = Position.Y + offsetH};
        Position = p;
        //Log.CW($"Moving window to {Position.X},{Position.Y} by {offsetW},{offsetH}");
    }
}