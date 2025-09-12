

using System;
using System.Numerics;
using MoonWorks.Math;
using MyGame.Utility;

public struct Vector2I
{
    public int X;
    public int Y;
    public Vector2I(int a, int b)
    {
        X = a;
        Y = b;
    }
    public Vector2I(int a)
    {
        X = a;
        Y = a;
    }
    public Vector2I()
    {
        X = 0;
        Y = 0;
    }
    public static Vector2I Left => new Vector2I(-1, 0);
    public static Vector2I Right => new Vector2I(1, 0);
    public static Vector2I Up => new Vector2I(0, -1);
    public static Vector2I Down => new Vector2I(0, 1);
    public static Vector2I One => new Vector2I(1, 1);

    public static Vector2I Floor(Vector2 vec) => new Vector2I((int)vec.X, (int)vec.Y);
    public static Vector2I Ceiling(Vector2 vec) => new Vector2I(MathUtils.CeilToInt(vec.X), MathUtils.CeilToInt(vec.Y));

    public static Vector2I operator +(Vector2I a, Vector2I b)
    {
        return new Vector2I(a.X + b.X, a.Y + b.Y);
    }

    public static Vector2I operator -(Vector2I a, Vector2I b)
    {
        return new Vector2I(a.X - b.X, a.Y - b.Y);
    }
    public static Vector2I operator -(Vector2I a)
    {
        return new Vector2I(-a.X, -a.Y);
    }
    public static Vector2I operator *(Vector2I a, int b) => new Vector2I(a.X * b, a.Y * b);
    public static Vector2I operator /(Vector2I a, int b) => new Vector2I(a.X / b, a.Y / b);
    public static implicit operator Vector2(Vector2I vec) => new Vector2(vec.X, vec.Y);
    public override int GetHashCode()
    {
        return X.GetHashCode() + Y.GetHashCode();
    }
}