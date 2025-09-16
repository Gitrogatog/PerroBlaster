using System;
using MoonTools.ECS;
using MyGame.Components;

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
                else
                {
                    Set(entity, message.Animation);
                }
            }
            else
            {
                Set(entity, message.Animation);
            }
            Remove<SetAnimation>(entity);
        }
    }
}