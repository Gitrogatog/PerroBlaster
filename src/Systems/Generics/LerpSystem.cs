namespace MyGame.Systems;

using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Utility;

public sealed class LerpSystem<T> : System where T : unmanaged
{
    public Filter LerpFilter;
    private Func<float, T> Action;
    public LerpSystem(World world, Func<float, T> action) : base(world)
    {
        LerpFilter = FilterBuilder
                        .Include<LerpValue<T>>()
                        .Build();
        Action = action;
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in LerpFilter.Entities)
        {
            var timer = Get<LerpValue<T>>(entity);
            var progress = timer.Progress + (float)delta.TotalSeconds / timer.MaxTime;
            Console.WriteLine("progress: " + progress);
            if(progress >= 1f) {
                progress = 1;
                Remove<LerpValue<T>>(entity);
            }
            else {
                Set(entity, new LerpValue<T>(timer.Start, timer.End, timer.MaxTime, progress));
            }
            Set(entity, Action(MathUtils.Lerp(timer.Start, timer.End, progress)));

        }
    }
}