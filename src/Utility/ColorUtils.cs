using System;
using System.Globalization;
using MoonTools.ECS;
using MoonWorks.Graphics;
namespace MyGame.Utility;

public static class ColorUtils
{
    public static Color HexStringToColor(string s)
    {
        // int.Parse(colorcode.Substring(0, 2), NumberStyles.HexNumber),
        if (s.Length < 6) return default;
        int r = int.Parse(s.AsSpan(0, 2), NumberStyles.HexNumber);
        int g = int.Parse(s.AsSpan(2, 2), NumberStyles.HexNumber);
        int b = int.Parse(s.AsSpan(4, 2), NumberStyles.HexNumber);
        int a = s.Length >= 8 ? int.Parse(s.AsSpan(4, 2), NumberStyles.HexNumber) : 255;
        return new Color(r, g, b, a);
    }
    // public static Color HexStringToColor(ReadOnlySpan<char> s)
    // {

    // }
}