using System.Numerics;
using MyGame.Utility;

namespace MyGame;

public static class TileConsts
{
    public const int TILE_SIZE = 10;
    public const int TILE_MULT = 2;
}

public static class Dimensions
{
    public const int GAME_W = 640;
    public const int GAME_H = 480;
    public const int CARD_WIDTH = 120;

}

public static class Globals
{
    public static int PlayerX;
    public static int PlayerY;
}

public static class FontSizes
{
    public const int SCORE = 12;
    public const int ORDER = 10;
    public const int SMALL_ORDER = 8;
    public const int DIALOGUE = 10;
    public const int INSPECT = 10;
    public const int INGREDIENT = 8;
    public const int HOLDING = 10;
    public const int SCORE_STRING = 20;
}

public static class Time
{
    public const float ROUND_TIME = 90.0f;
    public const float CATEGORY_ORDER_TIME = 20.0f;
    public const float INGREDIENT_ORDER_TIME = 30.0f;
}

public static class MoveConsts
{
    public static float MAX_FALL_SPEED => 250f;
    public static float MIN_JUMP_POWER;
    public static float MAX_JUMP_POWER => 250;
    public static float GRAVITY => 600;
    public static float MOVE_SPEED => 105;
    public static float CANCEL_JUMP_SPEED => 75;
    public static float GROUND_MOVE_SPEED;
    public static float GROUND_ACCEL;
    public static float GROUND_STOP_ACCEL;
    public static float AIR_MOVE_SPEED;
    public static float AIR_ACCEL;
    public static float AIR_STOP_ACCEL;
    public static float THROW_SPEED_X => 120;
    public static float THROW_SPEED_Y => 250;
    public static float WALLJUMP_SPEED_X => 140;
    public static float WALLJUMP_SPEED_Y => 210;
    public static float WALL_GRAVITY => 200;
    public static float WALL_MAX_FALL_SPEED => 70f;
    public static float WALL_STICKINESS => 0.3f;

    public static Vector2 SCREEN_SIZE = new Vector2(640, 360);
    public static Vector2 ROOM_SIZE = new Vector2(20, 20);
    public static Vector2 TILE_SIZE = new Vector2(16, 16);
    static Vector2 INV_MULT = Vector2.One / (ROOM_SIZE * TILE_SIZE);
    public static Vector2I PosToScreen(Vector2 position)
    {
        Vector2 screenFloat = position * INV_MULT;
        return Vector2I.Floor(screenFloat);
    }

}

public static class TextConsts
{
    public static float TEXT_ADVANCE_CHAR_TIME => 1;
}