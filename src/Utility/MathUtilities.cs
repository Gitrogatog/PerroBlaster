using System;
using System.Numerics;

namespace MyGame.Utility;

public static class MathUtils
{
    public static Vector2 SafeNormalize(Vector2 v)
    {
        if (v.LengthSquared() == 0)
        {
            return Vector2.Zero;
        }

        return Vector2.Normalize(v);
    }

    public static Vector2 Rotate(Vector2 vector, float rotation)
    {
        return Vector2.TransformNormal(vector, Matrix4x4.CreateRotationZ(rotation));
    }
    public static int FloorToInt(float input)
    {
        return (int)MathF.Floor(input);
    }
    public static int CeilToInt(float input)
    {
        return (int)MathF.Ceiling(input);
    }
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
    // the closer the exponent is to 0, the closer math.exp is to 1, and thus the closer the final value is to a
    // a is the start, b is the end
    // the higher the decay the faster we reach b
    public static float LerpDecay(float a, float b, float decay, float dt)
    {
        return b + (a - b) * MathF.Exp(-decay * dt);
    }
    public static Vector2 LerpDecay(Vector2 a, Vector2 b, float decay, float dt)
    {
        return b + (a - b) * MathF.Exp(-decay * dt);
    }
    public static float MoveTowards(float current, float target, float amount)
    {
        if (current < target)
        {
            return Math.Min(current + amount, target);
        }
        return Math.Max(current - amount, target);
    }
    public static Vector2 MoveTowards(Vector2 current, Vector2 target, float amount)
    {
        if (current == target)
        {
            return current;
        }
        Vector2 changeVector = SafeNormalize(target - current) * amount;
        return new Vector2(MoveTowards(current.X, target.X, Math.Abs(changeVector.X)), MoveTowards(current.Y, target.Y, Math.Abs(changeVector.Y)));
        // return new Vector2(MoveTowards(current.X, target.X, changeVector.X), MoveTowards(current.Y, target.Y, changeVector.Y));
    }
    public static float DistanceFromLine(Vector2 angle, Vector2 origin, Vector2 target, float length)
    {
        float a = angle.X;
        float b = angle.Y;
        float c = 0;
        target -= origin;
        if (a == 0 && b == 0) return 0;

        return MathF.Abs(a * target.X + b * target.Y) / MathF.Sqrt(a * a + b * b);
    }
    public static float Deg2Rad(float degrees)
    {
        return MathF.PI * degrees / 180f;
    }
    public static float Rad2Deg(float radians)
    {
        return 180f * radians / MathF.PI;
    }
    public static float Frac(float value)
    {
        return value - (float)Math.Truncate(value);
    }
    public static float Frac01(float value) => value - MathF.Floor(value);
}
