using System;
using MoonTools.ECS;
using MyGame.Components;
namespace MyGame.Systems;

public class MiscSpriteUpdateSystem : MoonTools.ECS.System
{
    private Filter RotateAimAngleFilter;

    public MiscSpriteUpdateSystem(World world) : base(world)
    {
        RotateAimAngleFilter = FilterBuilder
            .Include<AimAngle>()
            .Include<RotateSpriteToAimAngle>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in RotateAimAngleFilter.Entities)
        {
            var aimAngle = Get<AimAngle>(entity).Angle;
            Set(entity, new Rotation(MathF.Atan2(aimAngle.Y, aimAngle.X) + MathF.PI * 0.5f));
        }
    }
}
