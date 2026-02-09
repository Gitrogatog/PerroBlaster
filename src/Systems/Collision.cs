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
            if(Has<CanBeHeld>(entityA) && Has<ControlledByPlayer>(entityB)) {
                EntityPrefabs.SetHeldByPlayer(entityB, entityA);
            }
            if(TryGet<AxeState>(entityA, out AxeState axeState) && axeState == AxeState.Thrown && Has<Solid>(entityB)){
                Set(entityA, AxeState.Stuck);
                Set(entityA, new CanBeRecalled());
                Remove<RotateSpeed>(entityA);
                var velocity = Get<Velocity>(entityA);
                Set(entityA, new Rotation(MathF.Atan2(velocity.Y, velocity.X) + MathF.PI));
                Set(entityA, new Velocity());
                Set(entityA, new IgnoreCollision());
                EntityPrefabs.PlaySFX(StaticAudio.Hit_01);
            }
            if(Has<ControlledByPlayer>(entityA) && Has<CollisionForceMoveForOneFrame>(entityB)) {
                Set(entityA, new IntendedMoveOneFrame(Get<CollisionForceMoveForOneFrame>(entityB).Direction));
            }
            if((Has<OwnedByEnemy>(entityA) && Has<OwnedByPlayer>(entityB)) || (Has<OwnedByPlayer>(entityA) && Has<OwnedByEnemy>(entityB))) {
                Console.WriteLine("we are on different teams");
                if (Has<DamageOnContact>(entityA) && Has<Health>(entityB) && !HasInRelation<Invincible>(entityB) && !Related<DontDamage>(entityA, entityB))
                {
                    Console.WriteLine("attempting to deal damage!");
                    DealDamage(entityB, entityA, 1);
                }
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

    void DealDamage(Entity entity, Entity attacker, int damage) {
        var oldHealth = Get<Health>(entity);
        var health = new Health(oldHealth.Current - damage, oldHealth.Max);
        Set(entity, new Health(health.Current, health.Max));
        Console.WriteLine($"health: {health.Current}");
        if(TryGet<InvincibleOnDamage>(entity, out var invincibleOnDamage)){
            EntityPrefabs.CreateTimer(entity, invincibleOnDamage.Time, new Invincible());
        }
        if (health.Current < 1)
        {
            if(Has<OwnedByEnemy>(entity)) {
                Set(entity, new DestroyAtStartOfFrame());
            }
            Set(entity, new IgnoreCollision());
            if(Has<ControlledByPlayer>(entity) && !Some<ReturnToCheckpoint>()){
                var message = CreateEntity();
                Set(message, new Timer(3f));
                Set(message, new ReturnToCheckpoint());
                Set(message, new DestroyOnLoad());
                // Set(entity, new SetAnimation(SpriteAnimations.Hero_Dead));
                Globals.DeathCount += 1;
                var explosion = CreateEntity();
                Set(explosion, new Timer(5f));
                Set(explosion, new SpriteAnimation(SpriteAnimations.Explosion, false));
                Set(explosion, Get<Position>(entity));
                Remove<SpriteAnimation>(entity);
                EntityPrefabs.PlaySFX(StaticAudio.delarune_explosion);
            }
            else {
                var explosion = CreateEntity();
                Set(explosion, new Timer(3f));
                Set(explosion, new SpriteAnimation(SpriteAnimations.Explosion, false));
                Set(explosion, new SpriteScale(0.5f));
                Set(explosion, Get<Position>(entity));
                EntityPrefabs.PlaySFX(StaticAudio.Explosion_02);
            }
            Set(entity, new IsDead());
            Set(entity, new PreventInput());
            Remove<CanInteract>(entity);
        }
        else
        {
            if(Has<DontRepeatDamageUntilStateChange>(attacker)){
                Relate(attacker, entity, new DontDamage());
            }
        }
        if(Has<PlaySFXOnDamage>(entity)){
            EntityPrefabs.PlaySFX(Get<PlaySFXOnDamage>(entity).Sound);
        }
        Set(entity, new TookDamageThisFrame());
        Set(entity, new ColorOverlayTimer(Color.White, 0.1f));
        if(Has<DontRepeatDamageUntilStateChange>(attacker)) {
            Relate(attacker, entity, new DontDamage());
        }
    }
}