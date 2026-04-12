using System;
using System.Collections.Generic;
using System.Numerics;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
namespace MyGame.Systems;

public class AnimationSystem : MoonTools.ECS.System
{
    private Filter IdleFilter;
    private Filter WalkFilter;
    private Filter WalkSpeedFilter;
    private Filter RiseFallFilter;
    private HashSet<Entity> ProcessedEntities = new HashSet<Entity>();

    public AnimationSystem(World world) : base(world)
    {
        IdleFilter = FilterBuilder
            .Include<IdleAnimation>()
            .Build();
        WalkFilter = FilterBuilder
            .Include<WalkAnimation>()
            .Build();
        WalkSpeedFilter = FilterBuilder
            .Include<WalkSpeedModAnimation>()
            .Build();
        RiseFallFilter = FilterBuilder
            .Include<RiseFallAnimation>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        ProcessedEntities.Clear();
        foreach(var entity in RiseFallFilter.Entities) {
            if(ProcessedEntities.Contains(entity)) continue;
            if(!Has<Grounded>(entity)) {
                var verticalVelocity = Get<Velocity>(entity).Y;
                if(verticalVelocity > 0) SetAnimation(entity, Get<RiseFallAnimation>(entity).Fall);
                else SetAnimation(entity, Get<RiseFallAnimation>(entity).Rise);
            }
        }
        foreach(var entity in WalkSpeedFilter.Entities) {
            if(ProcessedEntities.Contains(entity)) continue;
            var horizontalVelocity = Get<Velocity>(entity).X;
            if(horizontalVelocity != 0) {
                (var anim, float animSpeedMult) = Get<WalkSpeedModAnimation>(entity);
                Set(entity, new SetAnimation(new SpriteAnimation(SpriteAnimationInfo.FromID(anim), animSpeedMult * MathF.Abs(horizontalVelocity))));
                ProcessedEntities.Add(entity);
            }
        }
        foreach(var entity in WalkFilter.Entities) {
            if(ProcessedEntities.Contains(entity)) continue;
            var horizontalVelocity = Get<Velocity>(entity).X;
            if(horizontalVelocity != 0) {
                SetAnimation(entity, Get<WalkAnimation>(entity).Walk);
            }
        }
        foreach(var entity in IdleFilter.Entities) {
            if(ProcessedEntities.Contains(entity)) continue;
            SetAnimation(entity, Get<IdleAnimation>(entity).Idle);
        }
    }
    void SetAnimation(Entity entity, SpriteAnimationInfoID animID) {
        Set(entity, new SetAnimation(animID));
        ProcessedEntities.Add(entity);
    }
}
