using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Content;
using MyGame.Relations;
using MyGame.Spawn;
using MyGame.Utility;
namespace MyGame.Systems;

public class Collision : MoonTools.ECS.System
{
    private Filter EntityFilter;

    public Collision(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach ((var entityA, var entityB) in Relations<Colliding>())
        {
            if (Has<CanPerformThrow>(entityA) && Has<CanBeThrown>(entityB) && !HasOutRelation<Throwing>(entityA) && !HasInRelation<Throwing>(entityB))
            {
                Set(entityA, new IgnoreCollision());
                Set(entityB, new IgnoreCollision());
                Set(entityA, new Velocity(EntityUtils.FacingMult(World, entityA, MoveConsts.THROW_SPEED_X), -MoveConsts.THROW_SPEED_Y));
                Set(entityA, new Gravity());
                Set(entityB, new Velocity());
                Remove<CanInteract>(entityB);
                Relate(entityA, entityB, new Throwing());
                Console.WriteLine("Starting throw!");
            }

            else if (Has<Kissable>(entityA) && Has<Kissable>(entityB))
            {
                var posA = Get<Position>(entityA);
                var posB = Get<Position>(entityB);
                var kiss = EntityPrefabs.CreateKiss((posA.X + posB.X) / 2, (posA.Y + posB.Y) / 2);
                Remove<Kissable>(entityA);
                Remove<Kissable>(entityB);
                Set(entityA, new DestroyAtEndOfFrame());
                Set(entityB, new DestroyAtEndOfFrame());
            }

        }

        foreach ((var entityA, var entityB) in Relations<TouchingSolid>())
        {
            if (Has<CanPerformThrow>(entityA) && !Has<IgnoreCollision>(entityA) && HasOutRelation<Throwing>(entityA))
            {
                var throwTarget = OutRelationSingleton<Throwing>(entityA);
                UnrelateAll<Offset>(throwTarget);
                Unrelate<Throwing>(entityA, throwTarget);
                Set(entityA, new Velocity());
                // bool facing = Has<Facing>(entityA) ? Get<Facing>(entityA).Right : false;
                Console.WriteLine($"thrower is facing to the: " + (Get<Facing>(entityA).Right ? "right" : "left"));
                Set(throwTarget, new Velocity(EntityUtils.FacingMult(World, entityA, MoveConsts.THROW_SPEED_X), -MoveConsts.THROW_SPEED_Y));
                // Set(throwTarget, new ); spawn timer
                Set(throwTarget, new Timer(1f));
                Set(throwTarget, new PreventInput());
                // Set(throwTarget, new SpawnOnTimerEnd(ThingType.Explosion));
                Set(throwTarget, new CreateAnimationEntityOnTimerEnd(SpriteAnimations.Explosion, false));
                Remove<AttemptJumpThisFrame>(throwTarget);
                Remove<IntendedMove>(throwTarget);
                Set(CreateEntity(), new CauseOfDeath(ThingType.Bunny));
                Console.WriteLine("ending throw!");
            }
        }
    }
}