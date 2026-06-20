using System;
using MoonTools.ECS;
using MyGame.Components;
namespace MyGame.Systems;

public class EnemyBasicBehaviorSystem : MoonTools.ECS.System
{
    private Filter JumpAfterLandingFilter;

    public EnemyBasicBehaviorSystem(World world) : base(world)
    {
        JumpAfterLandingFilter = FilterOne<JumpAfterLanding>();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in JumpAfterLandingFilter.Entities)
        {
            (float time, float maxTime) = Get<JumpAfterLanding>(entity);
            
            if(Has<Grounded>(entity)) {
                time -= (float)delta.TotalSeconds;
                if(time <= 0) {
                    Set(entity, new AttemptJumpThisFrame());
                    Set(entity, new JumpAfterLanding(maxTime));
                }
                else {
                    Set(entity, new JumpAfterLanding(time, maxTime));
                }
            }
            else if(time < maxTime) {
                Set(entity, new JumpAfterLanding(maxTime));
            }
        }
    }
}