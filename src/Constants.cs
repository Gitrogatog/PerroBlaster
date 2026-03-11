using System;
using System.Collections.Generic;
using System.Numerics;
using ldtk;
using MyGame.Utility;

namespace MyGame;

public static class TileConsts
{
    public const int TILE_SIZE = 16;
    public const int TILE_MULT = 1;
}

public static class Dimensions
{
    public const int GAME_W = 320;
    public const int GAME_H = 240;
    public const int TILE_SIZE = 16;
    public const float INV_TILE_SIZE = 1f / TILE_SIZE;
    public static int ROOM_X => ROOM_X_TILES * TILE_SIZE;
    public static int ROOM_Y => ROOM_Y_TILES * TILE_SIZE;
    public static int ROOM_X_TILES => 15;
    public static int ROOM_Y_TILES => 15;
}

public static class Globals
{
    public static int PlayerX;
    public static int PlayerY;
    public static bool ShouldExistPlayer = true;
    public static int SignToPlayer(int x) => MathF.Sign(PlayerX - x);
    public static int CameraX;
    public static int CameraY;
    public static int CameraXOffset => -Dimensions.GAME_W / 2;
    public static int CameraYOffset => -Dimensions.GAME_H / 2;
    public static int CheckpointX;
    public static int CheckpointY;
    public static int CameraMinX => 0;
    public static int CameraMaxX;
    public static int CameraMinY => 0;
    public static int CameraMaxY;
    public static int DeathCount = 0;
    public static int CurrentRoomX = -1000;
    public static int CurrentRoomY = -1000;
    public static int DefaultPlayerX => 0;
    public static int DefaultPlayerY => 0;
    public static double CurrentTime = Double.Epsilon;
    public static HashSet<(int, int)> ClearedRooms = new HashSet<(int, int)>();
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
    public static float MOVE_SPEED => 105;
    public static float AXE_THROW_SPEED => 250;
    public static float AXE_RETURN_SPEED => 350;
    public static float AXE_HELD_OFFSET => 9;
    public static float AXE_ROTATION_SPEED => 2;
    public static float ENEMY_INIT_PAUSE_TIME_MOVE => 0.5f;
    public static float ENEMY_INIT_PAUSE_TIME_SHOOT => 0.7f;
    public static float ENEMY_SHOT_SPEED => 100;
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