using System;
using System.Collections.Generic;
using System.Numerics;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Spawn;
using MyGame.Utility;
namespace MyGame.Systems;

public class AnimationSystem : MoonTools.ECS.System
{
    private Filter IdleFilter;
    private Filter WalkFilter;
    private Filter WalkSpeedFilter;
    private Filter RiseFallFilter;
    private Filter AirFilter;
    private Filter PlayerFilter;
    private Filter HurtFilter;
    private Filter SequenceFilter;
    private HashSet<Entity> ProcessedEntities = new HashSet<Entity>();


    public AnimationSystem(World world) : base(world)
    {
        IdleFilter = FilterBuilder.Include<IdleAnimation>().Build();
        SequenceFilter = FilterBuilder.Include<SequenceAnimation>().Build();
        HurtFilter = FilterBuilder.Include<HurtAnimation>().Build();
        WalkFilter = FilterBuilder.Include<WalkAnimation>().Build();
        WalkSpeedFilter = FilterBuilder.Include<WalkSpeedModAnimation>().Build();
        RiseFallFilter = FilterBuilder.Include<RiseFallAnimation>().Build();
        PlayerFilter = FilterBuilder.Include<PlayerAnimation>().Build();
        AirFilter = FilterOne<AirAnimation>();
    }

    public override void Update(TimeSpan delta)
    {
        ProcessedEntities.Clear();
        foreach (var entity in HurtFilter.Entities)
        {
            if (TryGet<EnemyState>(entity, out EnemyState state) && state == EnemyState.Hurt)
            {
                SetAnimation(entity, Get<HurtAnimation>(entity).ID);
            }
        }
        // foreach(var entity in SequenceFilter.Entities) {
        //     if(ProcessedEntities.Contains(entity)) continue;
        //     if(Has<PerformAttackSequence>(entity)) {
        //         SetAnimation(entity, Get<SequenceAnimation>(entity).Animation);
        //     }
        // }
        foreach (var entity in PlayerFilter.Entities)
        {
            if (ProcessedEntities.Contains(entity)) continue;
            var animData = Get<PlayerAnimation>(entity);
            if (TryGet<AimAngle>(entity, out AimAngle angle) && angle.Angle.Y != 0)
            {
                SetPlayerAnimation(entity, animData.UpIdle, animData.UpWalk, animData.UpWalkBack, animData.UpJump, animData.UpFall, animData.AnimSpeedMult);
            }
            else
            {
                SetPlayerAnimation(entity, animData.Idle, animData.Walk, animData.WalkBack, animData.Jump, animData.Fall, animData.AnimSpeedMult);
            }
        }
        foreach (var entity in RiseFallFilter.Entities)
        {
            if (ProcessedEntities.Contains(entity)) continue;
            if (!Has<Grounded>(entity))
            {
                var verticalVelocity = Get<Velocity>(entity).Y;
                if (verticalVelocity > 0) SetAnimation(entity, Get<RiseFallAnimation>(entity).Fall);
                else SetAnimation(entity, Get<RiseFallAnimation>(entity).Rise);
            }
        }
        foreach (var entity in AirFilter.Entities)
        {
            if (ProcessedEntities.Contains(entity)) continue;
            if (!Has<Grounded>(entity)) SetAnimation(entity, Get<AirAnimation>(entity).ID);
        }
        foreach (var entity in WalkSpeedFilter.Entities)
        {
            if (ProcessedEntities.Contains(entity)) continue;
            var horizontalVelocity = Get<Velocity>(entity).X;
            if (horizontalVelocity != 0)
            {
                (var anim, float animSpeedMult) = Get<WalkSpeedModAnimation>(entity);
                SetAnimation(entity, ComponentUtils.SetAnimSpeed(anim, animSpeedMult, horizontalVelocity));
            }
        }
        foreach (var entity in WalkFilter.Entities)
        {
            if (ProcessedEntities.Contains(entity)) continue;
            var horizontalVelocity = Get<Velocity>(entity).X;
            if (horizontalVelocity != 0)
            {
                SetAnimation(entity, Get<WalkAnimation>(entity).Walk);
            }
        }
        foreach (var entity in IdleFilter.Entities)
        {
            if (ProcessedEntities.Contains(entity)) continue;
            SetAnimation(entity, Get<IdleAnimation>(entity).Idle);
        }
    }
    void SetAnimation(Entity entity, SpriteAnimationInfoID animID)
    {
        Set(entity, new SetAnimation(animID));
        ProcessedEntities.Add(entity);
    }
    void SetAnimation(Entity entity, SpriteAnimation anim)
    {
        Set(entity, new SetAnimation(anim));
        ProcessedEntities.Add(entity);
    }
    void SetPlayerAnimation(Entity entity, SpriteAnimationInfoID idle, SpriteAnimationInfoID walk, SpriteAnimationInfoID walkBack, SpriteAnimationInfoID jump, SpriteAnimationInfoID fall, float animSpeedMult)
    {
        if (Has<Grounded>(entity))
        {
            if (TryGet<Velocity>(entity, out Velocity velocity) && velocity.X != 0)
            {
                if (EntityPrefabs.GetFacing(entity) == (velocity.X > 0))
                {
                    SetAnimation(entity, ComponentUtils.SetAnimSpeed(walk, animSpeedMult, velocity.X));
                }
                else
                {
                    SetAnimation(entity, ComponentUtils.SetAnimSpeed(walkBack, animSpeedMult, velocity.X));
                }
            }
            else
            {
                SetAnimation(entity, idle);
            }
        }
        else if (TryGet(entity, out Velocity velocity) && velocity.Y > 0)
        {
            SetAnimation(entity, fall);
        }
        else
        {
            SetAnimation(entity, jump);
        }
    }
}
