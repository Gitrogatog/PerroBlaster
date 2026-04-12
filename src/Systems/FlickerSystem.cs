namespace MyGame.Systems;

using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Relations;

public sealed class FlickerSystem : System
{
    public Filter TimerFilter;

    public FlickerSystem(World world) : base(world)
    {
        TimerFilter = FilterBuilder
                        .Include<FlickerDontDraw>()
                        .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in TimerFilter.Entities)
        {
            var timer = Get<FlickerDontDraw>(entity);
            var t = timer.Time - (float)delta.TotalSeconds;
            // Console.WriteLine(t);
            if (t <= 0.0f){
                if(!Has<DontDraw>(entity)) {
                    Set(entity, new DontDraw());
                }
                else {
                    Remove<DontDraw>(entity);
                }
                // Console.WriteLine(Has<DontDraw>(entity));
                Set(entity, new FlickerDontDraw(timer.MaxTime));
            }
            else
                Set<FlickerDontDraw>(entity, timer.Update(t));
        }
    }
}