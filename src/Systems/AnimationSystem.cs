using System;
using System.Numerics;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Content;
namespace MyGame.Systems;

public class AnimationSystem : MoonTools.ECS.System
{
    private Filter PlayerFilter;

    public AnimationSystem(World world) : base(world)
    {
        PlayerFilter = FilterBuilder
            .Include<PlayerAnimationSet>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in PlayerFilter.Entities)
        {
            var animSet = Get<PlayerAnimationSet>(entity);
            // if (Has<IsPivoting>(entity))
            // {
            //     Set(entity, new SetAnimation(SpriteAnimations.Skeleton_Pivot));
            // }
            var velocity = Has<Velocity>(entity) ? Get<Velocity>(entity).Value : Vector2.Zero;
            if (!Has<Grounded>(entity))
            {
                Set(entity, new SetAnimation(animSet.Jump));
            }
            else if (MathF.Abs(velocity.X) > 1)
            {
                if (Has<Facing>(entity) && Get<Facing>(entity).Right != (velocity.X > 0))
                {
                    Set(entity, new SetAnimation(animSet.Pivot));
                }
                else
                {
                    Set(entity, new SetAnimation(animSet.Walk));
                }
            }
            else
            {
                Set(entity, new SetAnimation(animSet.Idle));
            }
        }
    }
}
