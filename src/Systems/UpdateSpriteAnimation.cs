using System;
using MoonTools.ECS;
using MoonWorks.Graphics.Font;
using MoonWorks.Math;
using MyGame.Components;
using MyGame.Content;
using MyGame.Relations;
using MyGame.Utility;

namespace MyGame.Systems;

public class UpdateSpriteAnimationSystem : MoonTools.ECS.System
{
    Filter SpriteAnimationFilter;
    Filter SlowDownAnimationFilter;
    Filter FlickerFilter;
    // Filter TextFilter;

    public UpdateSpriteAnimationSystem(World world) : base(world)
    {
        SpriteAnimationFilter = FilterBuilder
            .Include<SpriteAnimation>()
            .Include<Position>()
            .Build();
        FlickerFilter = FilterBuilder.Include<ColorFlicker>().Build();
        // TextFilter = FilterBuilder.Include<Text>().Build();
        SlowDownAnimationFilter = FilterBuilder.Include<SlowDownAnimation>().Include<Position>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in SpriteAnimationFilter.Entities)
        {
            UpdateSpriteAnimation(entity, (float)delta.TotalSeconds);
        }


        // Slows down item animation
        foreach (var entity in SlowDownAnimationFilter.Entities)
        {
            var c = Get<SlowDownAnimation>(entity);
            var goal = c.BaseSpeed;
            var step = c.step;
            var currentAnimation = Get<SpriteAnimation>(entity);
            var frameRate = currentAnimation.FrameRate;
            frameRate = Math.Max(frameRate - step, goal);
            Set(entity, currentAnimation.ChangeFramerate(frameRate));
        }

        // Flicker
        foreach (var entity in FlickerFilter.Entities)
        {
            var flicker = Get<ColorFlicker>(entity);
            var frames = flicker.ElapsedFrames + 1;
            Set(entity, new ColorFlicker(frames, flicker.Color));
        }
    }

    public void UpdateSpriteAnimation(Entity entity, float dt)
    {
        var spriteAnimation = Get<SpriteAnimation>(entity).Update(dt);
        Set(entity, spriteAnimation);

        if (spriteAnimation.Finished)
        {
            if (Has<DestroyOnAnimationFinish>(entity))
            {
                Destroy(entity);
            }

        }
    }
}