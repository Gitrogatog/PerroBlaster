namespace MyGame.Systems;

using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Utility;

public sealed class LerpSingleSystem<T> : System where T : unmanaged
{
    public Filter LerpFilter;
    private Func<float, T> Action;
    public LerpSingleSystem(World world, Func<float, T> action) : base(world)
    {
        LerpFilter = FilterBuilder
                        .Include<LerpSingle<T>>()
                        .Build();
        Action = action;
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in LerpFilter.Entities)
        {
            var timer = Get<LerpSingle<T>>(entity);
            var progress = timer.Progress + (float)delta.TotalSeconds / timer.MaxTime;
            Console.WriteLine("progress: " + progress);
            if(progress >= 1f) {
                progress = 1;
                Remove<LerpSingle<T>>(entity);
            }
            else {
                Set(entity, new LerpSingle<T>(timer.Start, timer.End, timer.MaxTime, progress));
            }
            Set(entity, Action(MathUtils.Lerp(timer.Start, timer.End, progress)));

        }
    }
}

public sealed class LerpValueSystem<T> : System where T : unmanaged
{
    public Filter LerpFilter;
    private Func<T,T,float, T> Action;
    public LerpValueSystem(World world, Func<T, T,float, T> action) : base(world)
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
            Set(entity, Action(timer.Start, timer.End, progress));

        }
    }
}