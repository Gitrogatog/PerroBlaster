using System.Numerics;

namespace MyGame.Components;

public enum ClickableState
{
    Idle, Hover, Click
}

public enum ThingType
{
    Player, Bunny, RushBunny, Explosion
}
public enum TileType
{
    Solid, Fake, Invisible, Throwable, Spike
}

public enum Cardinal
{
    None = 0, Up = 1, Down = 2, Left = 4, Right = 8,
    Vertical = Up | Down,
    Horizontal = Left | Right,
    Negative = Up | Left,
    Positive = Down | Right
}

public static class ConvertEnum
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


}