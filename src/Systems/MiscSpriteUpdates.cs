using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Utility;
namespace MyGame.Systems;

public class MiscSpriteUpdateSystem : MoonTools.ECS.System
{
    private Filter RotateAimAngleFilter;
    private Filter GrowSpriteScaleFilter;

    public MiscSpriteUpdateSystem(World world) : base(world)
    {
        RotateAimAngleFilter = FilterBuilder
            .Include<AimAngle>()
            .Include<RotateSpriteToAimAngle>()
            .Build();
        GrowSpriteScaleFilter = FilterBuilder.Include<GrowSpriteScale>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in GrowSpriteScaleFilter.Entities) {
            (float start, float end, float maxTime, float progress) = Get<GrowSpriteScale>(entity);
            progress += (float)delta.TotalSeconds / maxTime;
            if(progress >= 1) {
                progress = 1;
                Remove<GrowSpriteScale>(entity);
            }
            Set(entity, new SpriteScale(MathUtils.Lerp(start, end, progress)));
        }
        foreach (var entity in RotateAimAngleFilter.Entities)
        {
            var aimAngle = Get<AimAngle>(entity).Angle;
            Set(entity, new Rotation(MathF.Atan2(aimAngle.Y, aimAngle.X) + MathF.PI * 0.5f));
        }
    }
}
