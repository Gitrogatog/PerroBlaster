using MoonTools.ECS;
using MyGame;
using MyGame.Components;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Data;
using MyGame.Relations;

namespace MyGame.Spawn;

public static class CombatPrefabs
{
    public static Entity CreateUnitVisual(World World, Entity unitEntity, int X, int Y, SpriteAnimationInfo sprite)
    {
        var entity = World.CreateEntity();
        World.Set(entity, new SpriteAnimation(sprite));
        World.Set(entity, new Position(X, Y));
        var rect = sprite.Frames[0].SliceRect;
        World.Set(entity, new Rectangle(0, 0, rect.W, rect.H));
        World.Relate(entity, unitEntity, new UnitVisual());
        World.Set(entity, new Clickable());
        World.Set(entity, new SelectTargetOnClick());
        return entity;
    }
}