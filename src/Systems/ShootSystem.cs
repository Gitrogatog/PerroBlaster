using System;
using System.Numerics;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Relations;
using MyGame.Spawn;
using MyGame.Utility;
namespace MyGame.Systems;

public class ShootSystem : MoonTools.ECS.System
{
    private Filter CanShootFilter;
    private Filter ShootThisFrameFilter;

    public ShootSystem(World world) : base(world)
    {
        CanShootFilter = FilterBuilder
            .Include<CanShoot>()
            // .Include<AttemptShootThisFrame>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in CanShootFilter.Entities)
        {
            if(HasInRelation<DontShoot>(entity) || !(Has<AttemptShootThisFrame>(entity) || Has<ContinuousAttemptShoot>(entity))) continue;
            Remove<AttemptShootThisFrame>(entity);
            (ShotType shotType, int offsetX, int offsetY, float cooldown, AimType aimType) = Get<CanShoot>(entity);
            EntityPrefabs.CreateTimer(entity, cooldown, new DontShoot());
            EntityPrefabs.PerformShot(entity, shotType, aimType, offsetX, offsetY);
        }
        // while(Some<ShootThisFrame>()) {
        //     (ShotType shotType, AimType aimType) = GetSingleton<ShootThisFrame>();
        //     Entity entity = GetSingletonEntity<ShootThisFrame>();
        //     Remove<ShootThisFrame>(entity);
        //     if(HasInRelation<DontShoot>(entity)) {
        //         continue;
        //     }
        //     if(Has<ShotOffset>(entity)) {
        //         (int offsetX, int offsetY) = Get<ShotOffset>(entity);
        //         PerformShot(entity, shotType, aimType, offsetX, offsetY);
        //     }
        //     else {
        //         PerformShot(entity, shotType, aimType, 0, 0);
        //     }
        // }
    }

    // void PerformShot(Entity entity, ShotType shotType, AimType aimType, int offsetX, int offsetY) {
    //     Vector2 shotDir;
    //         switch(aimType) {
    //             case AimType.Facing: {
    //                 if(Has<Facing>(entity) && !Get<Facing>(entity).Right) {
    //                     shotDir = new Vector2(1, 0);
    //                     offsetX = -offsetX;
    //                 }
    //                 else {
    //                     shotDir = new Vector2(-1, 0);
    //                 }
    //                 break;
    //             }
    //             case AimType.AimAngle: {
    //                 shotDir = Has<AimAngle>(entity) ? Get<AimAngle>(entity).Angle : Vector2.Zero;
    //                 break;
    //             }
    //             case AimType.AimAndFacing: {
    //                 bool facing = EntityPrefabs.GetFacing(entity);
    //                 // Console.WriteLine($"facing: {Get<Facing>(entity).Right}");
    //                 if(!facing) {
    //                     offsetX = -offsetX;
    //                 }
    //                 if(TryGet(entity, out AimAngle angle) && angle.Angle.Y != 0) {
    //                     shotDir = angle.Angle;
    //                 }
    //                 else if(facing) {
    //                     shotDir = new Vector2(1, 0);
    //                 }
    //                 else {
    //                     shotDir = new Vector2(-1, 0);
    //                 }
    //                 break;
    //             }
    //             case AimType.PlayerArc: {
    //                 var start = Get<Position>(entity);
    //                 float height = 20;
    //                 Vector2 velocity = MathUtils.InitialProjectileVelocity(start.X, start.Y, Globals.PlayerX, Globals.PlayerY, height, MoveConsts.GRAVITY);
    //                 shotDir = velocity;
    //                 break;
    //             }
    //             default: {
    //                 shotDir = Vector2.Zero;
    //                 break;
    //             }
    //         }
    //         switch(shotType) {
    //             case ShotType.Player: {
    //                 var shot = MakeBullet(entity, offsetX, offsetY, shotDir, 300, 8, 8, 0.5f);
    //                 Set(shot, new SpriteAnimation(SpriteAnimations.yellow_shot));
    //                 Set(shot, new DeathAnimation(SpriteAnimations.yellow_shot_dead, Time.SINGLE_FRAME * 4));
    //                 if(shotDir.X != 0) {
    //                     Set(shot, new Facing(shotDir.X > 0));
    //                 }
    //                 else {
    //                     Set(shot, new Rotation(MathF.PI * 0.5f));
    //                 }
    //                 break;
    //             }
    //             case ShotType.Flower: {
    //                 var shot = MakeBullet(entity, offsetX, offsetY, shotDir, 1, 8, 8, 5f);
    //                 Set(shot, new SpriteAnimation(SpriteAnimations.jump_flower_bullet));
    //                 break;
    //             }
    //             case ShotType.Fireball: {
    //                 var shot = MakeBullet(entity, offsetX, offsetY, shotDir, 100, 8, 8, 10f);
    //                 Set(shot, new SpriteAnimation(SpriteAnimations.flower_fireball));
    //                 break;
    //             }
    //         }
    // }

    // Entity MakeBullet(Entity source, int offsetX, int offsetY, Vector2 direction, float speed, int width, int height, float lifetime = 0) {
    //     var entity = CreateEntity("bullet");
    //     Set(entity, new DestroyOnLoad());
    //     var pos = Get<Position>(source);
    //     Set(entity, new Position(pos.X + offsetX, pos.Y + offsetY));
    //     Set(entity, new Velocity(direction * speed));
    //     Set(entity, new Rectangle(width, height));
    //     Set(entity, EffectorFlags.CanDamage | EffectorFlags.CanTouchWall);
    //     Set(entity, EffectedFlags.None);
    //     if(Has<OwnedByEnemy>(source)) Set(entity, new OwnedByEnemy());
    //     else if (Has<OwnedByPlayer>(source)) Set(entity, new OwnedByPlayer());
    //     Set(entity, new DestroyOnContact());
    //     Set(entity, new DamageOnContact());
    //     Set(entity, new CanInteract());
    //     if(lifetime != 0) Set(entity, new Timer(lifetime));
    //     // Set(entity, new DrawAsRectangle());
    //     // Set(entity, new ColorBlend(new MoonWorks.Graphics.Color(0, 255, 0)));
    //     return entity;
    // }
}
