using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CustomTilemap;
using MoonTools.ECS;
using MoonWorks.Input;
using MyGame.Components;
using MyGame.Systems;
namespace MyGame;

public static class GlobalInput
{
    public static int MouseX;
    public static int MouseY;
    public static int WorldMouseX => MouseX + Globals.CameraX;
    public static int WorldMouseY => MouseY + Globals.CameraY;
    public static ButtonState Left;
    public static ButtonState Right;
    public static InputState Current;
    public static InputState Prev;
}

public static class GlobalCollision {
    public static EffectorFlags[] effectorFlags = Enum.GetValues<EffectorFlags>();
    public static EffectedFlags[] effectedFlags = Enum.GetValues<EffectedFlags>();
    public static List<(Entity, Entity)>[] Collisions = new List<(Entity, Entity)>[effectorFlags.Length];
    public static void Init() {
        for(int i = 0; i < Collisions.Length; i++){
            Collisions[i] = new List<(Entity, Entity)>();
        }
    }
}

public static class GlobalTilemap {
    public static Tilemap Tilemap;
    public static bool IsWithinBounds(int x, int y) => x >= 0 && y >= 0 && x < Tilemap.xTiles && y < Tilemap.yTiles;
    public static bool IsBlocked(int x, int y) => Tilemap.IsBlocked(x, y);
    public static List<Entity> GetEntities(int x, int y) => Tilemap.tileContents[Tilemap.xy_id(x, y)];
    public static void ChangePosition(Entity entity, int x, int y) => Tilemap.ChangePosition(entity, x, y);
}