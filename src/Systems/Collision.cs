using System;
using MoonTools.ECS;
using MoonWorks.Graphics;
using MyGame.Components;
using MyGame.Content;
using MyGame.Relations;
using MyGame.Spawn;
using MyGame.Utility;
namespace MyGame.Systems;

public class CollisionSystem : MoonTools.ECS.System
{
    private MoonTools.ECS.Filter EntityFilter;

    public CollisionSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach ((var entityA, var entityB) in Relations<Colliding>())
        {
            // Console.WriteLine(entityA);
            // if(Has<IsAxe>(entityA) || Has<IsAxe>(entityB)) {
            //     Console.WriteLine("AaAAA");
            // }
            if(!Has<OwnedByEnemy>(entityB) && Has<DestroyOnContact>(entityA) && !Has<DestroyAtStartOfFrame>(entityA)){
                Set(entityA, new DestroyAtStartOfFrame());
            }
            if(Has<ControlledByPlayer>(entityA) && Has<CollisionForceMoveForOneFrame>(entityB)) {
                Set(entityA, new IntendedMoveOneFrame(Get<CollisionForceMoveForOneFrame>(entityB).Direction));
            }
            // if(Has<CanBeStuck>(entityB) && Has<Solid>(entityA)){
            //     Remove<CanBeStuck>(entityA);
            //     Set(entityA, new CanBeRecalled());
            //     var velocity = Get<Velocity>(entityA);
            //     Set(entityA, new Rotation(MathF.Atan2(velocity.Y, velocity.X)));
            //     Set(entityA, new Velocity());
            //     Set(entityA, new IgnoreCollision());
            // }
        }

        foreach ((var entityA, var entityB) in Relations<TouchingSolid>())
        {
        }
    }
}