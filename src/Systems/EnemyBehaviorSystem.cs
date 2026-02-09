using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Relations;
using MyGame.Spawn;
namespace MyGame.Systems;

public class EnemyBehaviorSystem : MoonTools.ECS.System
{
    private Filter ShootFilter;
    private Filter FollowPathFilter;

    public EnemyBehaviorSystem(World world) : base(world)
    {
        ShootFilter = FilterBuilder.Include<CanShoot>().Exclude<CantShootTimer>().Build();
        FollowPathFilter = FilterBuilder
            .Include<FollowPath>()
            .Include<Position>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        if(!Some<MustBeKilledToProgress>()){
            while(Some<DestroyOnCompleteRoom>()){
                Destroy(GetSingletonEntity<DestroyOnCompleteRoom>());
            }
        }
        foreach (var entity in FollowPathFilter.Entities)
        {
            if(!Has<MoveToPosition>(entity)) {
                UpdateTargetPoint(entity, false);
            }
            else if(Vector2.DistanceSquared(Get<Position>(entity).RawPosition, Get<MoveToPosition>(entity).Position.RawPosition) < (Get<MoveSpeed>(entity).Value * (float)delta.TotalSeconds)) {
                UpdateTargetPoint(entity, true);
            }
        }
        bool hasMadeShootSoundThisFrame = false;
        foreach(var entity in ShootFilter.Entities){
            if(HasInRelation<DontShoot>(entity)) continue;
            (var pattern, float cooldown) = Get<CanShoot>(entity);
            EntityPrefabs.CreateBulletPattern(pattern, Get<Position>(entity), Get<AimAngle>(entity).Angle);
            Set(entity, new CantShootTimer(cooldown));
            if(!hasMadeShootSoundThisFrame) {
                hasMadeShootSoundThisFrame = true;
                EntityPrefabs.PlaySFX(StaticAudio.Shoot_01);
            }
        }
    }
    void UpdateTargetPoint(Entity entity, bool shouldUpdate) {
        (int pathID, int targetPointID) = Get<FollowPath>(entity);
        if(shouldUpdate) {
            targetPointID += Has<InvertPath>(entity) ? -1 : 1;
        }
        var path = Stores.PathStorage.Get(pathID);
        if(targetPointID < 0) {
            targetPointID = path.Count - 1;
        }
        else if(targetPointID >= path.Count) {
            targetPointID = 0;
        }
        Set(entity, new FollowPath(pathID, targetPointID));
        Set(entity, new MoveToPosition(new Position(path[targetPointID])));
    }
}