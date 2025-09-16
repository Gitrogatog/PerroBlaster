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
        return new Vector2(MoveTowards(current.X, target.X, changeVector.X), MoveTowards(current.Y, target.Y, changeVector.Y));
    }
}
