using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Utility;

namespace RollAndCash.Systems;

public class SetSpriteAnimationSystem : MoonTools.ECS.System
{
    Filter EntityFilter;
    public SetSpriteAnimationSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder.Include<SetAnimation>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            var message = Get<SetAnimation>(entity);
            if (Has<SpriteAnimation>(entity))
            {
                var currentAnimation = Get<SpriteAnimation>(entity);

                if (currentAnimation.SpriteAnimationInfoID ==
                    message.Animation.SpriteAnimationInfoID)
                {
                    if (currentAnimation.FrameRate != message.Animation.FrameRate)
                    {
                        Set(entity, currentAnimation.ChangeFramerate(message.Animation.FrameRate));
                    }
                    else if (message.ForceUpdate)
                    {
                        Set(entity, message.Animation);
                    }
                }
                else if (message.PreserveFrame)
                {
                    Console.WriteLine("preserve frame!");
                    var animation = new SpriteAnimation(message.Animation.SpriteAnimationInfo).ForceRawFrame(MathUtils.Frac01(currentAnimation.RawFrameIndex));
                    Set(entity, animation);
                }
                else {
                    Console.WriteLine("hahahh");
                    Set(entity, message.Animation);
                }
            }
            else
            {
                Set(entity, message.Animation);
            }
            // var superanim = Get<SpriteAnimation>(entity);
            // Console.WriteLine($"anim: {superanim.Loop}");
            Remove<SetAnimation>(entity);
        }
    }
}