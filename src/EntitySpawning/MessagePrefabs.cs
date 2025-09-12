using MoonTools.ECS;
using MyGame.Components;

namespace MyGame.Spawn;

public static class MessagePrefabs
{

    public static Entity CreateMessage<T>(World World, T message) where T : unmanaged
    {
        var entity = BaseMessage(World);
        World.Set(entity, message);
        return entity;
    }
    public static Entity CreateRelation<T>(World World, Entity entity, T relation) where T : unmanaged
    {
        var message = BaseMessage(World);
        World.Relate(message, entity, relation);
        return message;
    }
    // public static Entity ActivateEnemyTargetPanels(World World)
    // {
    //     var entity = BaseMessage(World);

    //     return entity;
    // }


    static Entity BaseMessage(World World)
    {
        var entity = World.CreateEntity();
        World.Set(entity, new DestroyAtEndOfFrame());
        return entity;
    }
}