using System;
using MoonTools.ECS;
using MoonWorks.Graphics.Font;
using MoonWorks.Math;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Relations;
using MyGame.Spawn;
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
        // if(Has<IsDialogBox>(entity)) Console.WriteLine($"rawframe: {spriteAnimation.RawFrameIndex}");
        Set(entity, spriteAnimation);

        if (spriteAnimation.Finished)
        {
            // Console.WriteLine("finished animaiton");
            if(Has<SpawnOnAnimationFinish>(entity)) {
                Console.WriteLine("spawn on finish~");
                var pos = Has<Position>(entity) ? Get<Position>(entity) : new Position();
                EntityPrefabs.CreateThing(Get<SpawnOnAnimationFinish>(entity).Thing, pos.X, pos.Y);
            }
            if(Has<CreateDialogTextOnAnimFinish>(entity)) {
                int textId = Get<CreateDialogTextOnAnimFinish>(entity).TextID;
                var pos = Get<Position>(entity);
                Console.WriteLine($"creating text entity with text {TextStorage.GetString(textId)}");
                var textEntity = EntityPrefabs.CreateDialogText(textId, pos.X, pos.Y);
                Remove<CreateDialogTextOnAnimFinish>(entity);
            }
            // if(Has<EnableAdvanceCharCount>(entity)) {
            //     Remove<EnableAdvanceCharCount>(entity);
            //     while(Some<SetAdvanceCharCountOnDialogBoxOpen>()) {
            //         var target = GetSingletonEntity<SetAdvanceCharCountOnDialogBoxOpen>();
            //         Set(target, new AdvanceCharCount(Get<SetAdvanceCharCountOnDialogBoxOpen>(entity).CharPerSecond));
            //         Remove<SetAdvanceCharCountOnDialogBoxOpen>(target);
            //     }
            //     //     foreach(var textEntity in AdvanceCharFilter.Entities) {
            //     //         Set(textEntity, new AdvanceCharCount(Get<SetAdvanceCharCountOnDialogBoxOpen>(textEntity).CharPerSecond));
            //     //         Remove<SetAdvanceCharCountOnDialogBoxOpen>(textEntity);
            //     //     }
            // }
            if(Has<PerformDialogBoxFullyClose>(entity) && spriteAnimation.RawFrameIndex < 0) {
                DestroyAll<DestroyOnDialogBoxFullyClose>();
                Destroy(entity);
            }
            if (Has<DestroyOnAnimationFinish>(entity))
            {
                Destroy(entity);
            }
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
        }
    }
}