

using MoonTools.ECS;

public static class ECSUtils
{
    public static bool TryGet<T>(this World world, Entity entity, out T component) where T : unmanaged
    {
        if (!world.Has<T>(entity))
        {
            component = default;
            return false;
        }
        component = world.Get<T>(entity);
        return true;
    }
}