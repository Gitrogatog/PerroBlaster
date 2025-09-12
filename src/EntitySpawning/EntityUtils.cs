using MoonTools.ECS;
using MyGame.Components;

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
}