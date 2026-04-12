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
}
