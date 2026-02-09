using System;
using System.Numerics;
using MoonTools.ECS;
using MyGame.Components;
namespace MyGame.Systems;

public class UpdateRotationSystem : MoonTools.ECS.System
{
    private Filter RotateSpeedFilter;
    private Filter FacePlayerFilter;

    public UpdateRotationSystem(World world) : base(world)
    {
        RotateSpeedFilter = FilterBuilder
            .Include<RotateSpeed>()
            .Build();
        FacePlayerFilter = FilterBuilder.Include<AimAtPlayer>().Build();

    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in RotateSpeedFilter.Entities)
        {
            float rotationDelta = Get<RotateSpeed>(entity).Value * (float)delta.TotalSeconds;
            if(!Has<Rotation>(entity)) {
                Set(entity, new Rotation(rotationDelta));
            }
            else {
                Set(entity, new Rotation(Get<Rotation>(entity).Value + rotationDelta));

            }
        }
        foreach(var entity in FacePlayerFilter.Entities){
            var position = Get<Position>(entity);
            Set(entity, new AimAngle(Globals.PlayerX - position.X, Globals.PlayerY - position.Y));
        }
    }
}
