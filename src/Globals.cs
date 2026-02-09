using System;
using System.Collections.Generic;
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