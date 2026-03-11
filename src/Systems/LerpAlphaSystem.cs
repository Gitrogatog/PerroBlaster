using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Utility;
namespace MyGame.Systems;

public class LerpAlphaSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;

    public LerpAlphaSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<LerpAlpha>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            (float startAlpha, float endAlpha, float totalTime, float progress) = Get<LerpAlpha>(entity);
            progress += (float)delta.TotalSeconds / totalTime;
            Set(entity, new ColorBlend(new MoonWorks.Graphics.Color(0, 0, 0, MathUtils.Lerp(startAlpha, endAlpha, progress))));
            if(progress >= 1) {
                Remove<LerpAlpha>(entity);
            }
            else {
                Set(entity, new LerpAlpha(startAlpha, endAlpha, totalTime, progress));
            }
            
        }
    }
}
