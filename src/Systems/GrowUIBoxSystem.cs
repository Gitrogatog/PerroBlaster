using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Relations;
using MyGame.Utility;
namespace MyGame.Systems;

public class GrowUIBoxSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;
    private Filter AdvanceCharFilter;

    public GrowUIBoxSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<GrowRectToSize>()
            .Build();
        AdvanceCharFilter = FilterBuilder.Include<SetAdvanceCharCountOnDialogBoxOpen>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            (int targetY, int totalFrames, int currentFrame) = Get<GrowRectToSize>(entity);
            var rect = Get<Rectangle>(entity);
            currentFrame++;
            float height = MathUtils.Lerp(rect.Height, targetY, (float)currentFrame / totalFrames);
            Set(entity, rect.Inflate(0, (int)height));
            if (currentFrame < totalFrames)
            {
                Set(entity, new GrowRectToSize(targetY, totalFrames, currentFrame));
            }
            else
            {
                // Remove<GrowRectToSize>(entity);
                // if(Has<EnableAdvanceCharCount>(entity)) {
                //     Remove<EnableAdvanceCharCount>(entity);
                //     foreach(var textEntity in AdvanceCharFilter.Entities) {
                //         Set(textEntity, new AdvanceCharCount(Get<SetAdvanceCharCountOnDialogBoxOpen>(textEntity).CharPerSecond));
                //         Remove<SetAdvanceCharCountOnDialogBoxOpen>(textEntity);
                //     }
                // }
                // if(Has<DestroyOnFinishGrow>(entity)) {
                //     Destroy(entity);
                //     DestroyAll<DestroyOnDialogBoxFullyClose>();
                // }
                
                // foreach (var child in OutRelations<DontDraw>(entity))
                // {
                //     if (Has<DisplayCharCount>(child))
                //     {
                //         Set(child, new AdvanceCharCount(2f));
                //     }
                // }
                // UnrelateAll<DontDraw>(entity);
            }
        }
    }
}