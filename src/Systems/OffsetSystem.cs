using System;
using System.Numerics;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Relations;
using MyGame.Utility;
namespace MyGame.Systems;

public class OffsetSystem : MoonTools.ECS.System
{
    private Filter SpinFilter;

    public OffsetSystem(World world) : base(world)
    {
        SpinFilter = FilterBuilder
            .Include<SpinOffset>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        if(Some<RootUI>()) {
            var entity = GetSingletonEntity<RootUI>();
            MoveChildren(entity, new Position(), false);
        }
        foreach(var entity in SpinFilter.Entities){
            if(!HasOutRelation<Offset>(entity)) continue;
            (float distance, float speed, float progress) = Get<SpinOffset>(entity);
            progress = MathUtils.Frac01(speed * (float)delta.TotalSeconds + progress);
            Set(entity, new SpinOffset(distance, speed, progress));
            foreach(var child in OutRelations<Offset>(entity)){
                Relate(entity, child, new Offset(MathF.Cos(2 * MathF.PI * progress) * distance, MathF.Sin(2 * MathF.PI * progress) * distance));
            }
            
        }
        foreach ((var parent, var child) in Relations<Offset>())
        {
            var position = Get<Position>(parent);
            position += GetRelationData<Offset>(parent, child).Value;
            Set(child, position);
        }
        // foreach ((var parent, var child) in Relations<OffsetSingleFrame>())
        // {
        //     var position = Get<Position>(parent);
        //     position += GetRelationData<OffsetSingleFrame>(parent, child).Value;
        //     Set(child, position);
        //     Unrelate<OffsetSingleFrame>(parent, child);
        // }
        foreach ((var parent, var child) in Relations<OffsetAimAngle>())
        {
            Vector2 angle = Get<AimAngle>(parent).Angle;
            var position = Get<Position>(parent) + GetRelationData<OffsetAimAngle>(parent, child).Offset * angle;
            // Console.WriteLine($"offset amount: {GetRelationData<OffsetAimAngle>(parent, child).Offset * angle} angle: {angle}");
            Set(child, position);
            Set(child, new Rotation(MathF.Atan2(angle.Y, angle.X) + MathF.PI));
            Unrelate<OffsetSingleFrame>(parent, child);

        }
        foreach((var parent, var child) in Relations<FollowYWithOffset>()){
            if(TryGet<Position>(parent, out Position parentPos) && TryGet(child, out Position childPos)){
                float y = parentPos.Y + GetRelationData<FollowYWithOffset>(parent, child).Offset;
                Set(child, new Position(childPos.X, y));
            }
        }
        
    }
    // parent: parent position
    // forceUpdate: parent moved last frame, must update
    // situations where we need to update:
    // - parent updated (forceUpdate = true)
    // - expected final position is different from last position
    void MoveChildren(Entity entity, Position parent, bool forceUpdate) {
        // var localPosition = Has<LocalPosition>(entity) ? Get<LocalPosition>(entity).Value : new Position(0, 0);
        if(forceUpdate || Has<MarkForUpdatePosition>(entity)) { // !Has<LastPosition>(entity) || Get<LastPosition>(entity).Value != localPosition 
            Remove<MarkForUpdatePosition>(entity);
            if(TryGet<LocalPosition>(entity, out LocalPosition localPosition)) {
                parent += localPosition.Value;
            }
            Set(entity, parent);
            forceUpdate = true;
        }
        foreach(var child in OutRelations<Child>(entity)) {
            MoveChildren(child, parent, forceUpdate);
        }
    }
}
