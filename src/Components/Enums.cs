using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml;

namespace MyGame.Components;

public enum GameSceneType {
    StartMenu, Level, EndMenu,
}
public enum AxeState {
    Held, Thrown, Stuck, Recalled
}
public enum ClickableState
{
    Idle, Hover, Click
}
public enum EnemyType {
    Square, Diamond, Circle, Triangle, Pentagon
}
public enum BulletPattern {
    Single, Triple
}
public enum ThingType
{
    Player, Bunny, RushBunny, TumbleBunny, ClimbBunny, FallBunny, Explosion
}
public enum TileType
{
    Solid, Fake, Invisible, Throwable, Spike
}
public enum RectThingType {
    EnterFence, ExitFence,
}

public enum Cardinal
{
    None = 0, Up = 1, Down = 2, Left = 4, Right = 8,
    Vertical = Up | Down,
    Horizontal = Left | Right,
    Negative = Up | Left,
    Positive = Down | Right
}
[Flags]
public enum EffectorFlags {
    None,
    CanDamage = 1,
    CanTriggerEvent = 1 << 1,
    CanTouchWall = 1 << 2,
    CanTouchPit = 1 << 3,
}
[Flags]
public enum EffectedFlags {
    None = 0,
    CanTakeDamage = 1,
    CanPerformEvent = 1 << 1,
    IsWall = 1 << 2,
    IsPit = 1 << 3,
}
public enum AmbushTrigger
{
    EqualX, EqualY, EqualXGreaterThanY, EqualXLessThanY, GreaterThanXEqualY, LessThanXEqualY
}

public static class MyConvertEnum
{
    public static Cardinal ToCardinal(string s) =>
        s switch
        {
            "Left" => Cardinal.Left,
            "Right" => Cardinal.Right,
            "Up" => Cardinal.Up,
            "Down" => Cardinal.Down,
            _ => Cardinal.Left
        };
    public static Vector2 CardinalToVec(Cardinal input) =>
        input switch
        {
            Cardinal.Left => new Vector2(-1, 0),
            Cardinal.Right => new Vector2(1, 0),
            Cardinal.Up => new Vector2(0, -1),
            Cardinal.Down => new Vector2(0, 1),
            _ => default
        };
    // public static ThingType StringToThing(string s) =>
    // s switch
    // {
    //     "Player" => ThingType.Player,
    //     "Bunny" => ThingType.Bunny,
    //     "RushBunny" => ThingType.RushBunny,
    //     "ClimbBunny" => ThingType.ClimbBunny,
    //     "FallBunny" => ThingType.FallBunny,
    //     "TumbleBunny" => ThingType.TumbleBunny,
    //     _ => ThingType.Explosion
    // };

}
public static class ConvertStringEnum<T> where T : struct, System.Enum
{
    static Dictionary<string, T> StringToEnum;
    static Dictionary<T, string> EnumToString;
    static bool init;
    static void Init()
    {
        init = true;
        var strings = Enum.GetNames<T>();
        var enums = Enum.GetValues<T>();
        int length = enums.Length;
        StringToEnum = new Dictionary<string, T>(length);
        EnumToString = new Dictionary<T, string>(length);
        for (int i = 0; i < length; i++)
        {
            T e = enums[i];
            string s = strings[i];
            EnumToString[e] = s;
            StringToEnum[s] = e;
        }
    }
    public static T ToEnum(string s)
    {
        if (!init)
        {
            Init();
        }
        return StringToEnum[s];
    }
    public static string ToString(T e)
    {
        if (!init)
        {
            Init();
        }
        return EnumToString[e];
    }
}