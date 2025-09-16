namespace MyGame.Systems;

using System;
using MoonTools.ECS;
using MyGame.Components;
public sealed class AddAfterTimeSystem<T> : System where T : unmanaged
{
    public Filter TimerFilter;
    bool markForDestroy = false;
    public AddAfterTimeSystem(World world, bool markForDestroy = false) : base(world)
    {
        this.markForDestroy = markForDestroy;
        TimerFilter = FilterBuilder
                        .Include<AddAfterTime<T>>()
                        .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in TimerFilter.Entities)
        {
            var timer = Get<AddAfterTime<T>>(entity);
            var t = timer.Time - (float)delta.TotalSeconds;

            if (t <= 0.0f)
            {
                Set(entity, timer.Component);
                if (markForDestroy)
                {
                    Set(entity, new DestroyAtEndOfFrame());
                }
                Remove<AddAfterTime<T>>(entity);
            }
            else
            {
                Set(entity, timer.Update(t));
            }

        }
    }
}