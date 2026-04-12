using MoonTools.ECS;
using MyGame.Components;
using MyGame.Data;

namespace MyGame.Utility;

public static class EntityUtils
{
    public static float FacingMult(World World, Entity entity, float number) => World.Has<Facing>(entity) && World.Get<Facing>(entity).Right ? number : -number;
    public static void RemoveAll<T>(World World) where T : unmanaged
    {
        while (World.Some<T>())
        {
            var entity = World.GetSingletonEntity<T>();
            World.Remove<T>(entity);
        }
    }
    public static Position TileToWorld(float x, float y) =>
        new Position(x * Dimensions.TILE_SIZE + Dimensions.TILE_SIZE / 2, y * Dimensions.TILE_SIZE + Dimensions.TILE_SIZE / 2);
    public static void SetStandAnim(World World, Entity entity, SpriteAnimationInfo animation) {
        int targetFrame = animation.Frames.Length > 1 ? 1 : 0;
        World.Set(entity, new SetAnimation(
            new SpriteAnimation(animation, 0, true, targetFrame),
            ForceUpdate: true
        ));
    }
    // public static T CreateAnimSpeedMult(SpriteAnimationInfo anim, float speed) where T : unmanaged, 
}