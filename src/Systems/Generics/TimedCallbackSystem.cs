namespace MyGame.Systems;

using System;
using MoonTools.ECS;
using MyGame.Components;
public abstract class TimedCallbackSystem<T> : System where T : unmanaged, TimedComponent<T>
{
    public Filter TimerFilter;

    public TimedCallbackSystem(World world) : base(world)
    {
        TimerFilter = FilterBuilder
                        .Include<T>()
                        .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in TimerFilter.Entities)
        {
            var timer = Get<T>(entity);
            var t = timer.Time - (float)delta.TotalSeconds;

            if (t <= 0.0f)
            {
                Remove<T>(entity);
                OnFinish(entity, timer);
            }
            else
            {
                Set<T>(entity, timer.Update(t));
            }
        }
    }
    protected abstract void OnFinish(Entity entity, T component);
}