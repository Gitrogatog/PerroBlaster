using System;
using MoonTools.ECS;
using MyGame.Components;

namespace MyGame.Systems;

public class DestroyOnMessageSystem<T> : MoonTools.ECS.System where T : unmanaged
{
    private Filter EntityFilter;

    public DestroyOnMessageSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<DestroyOnMessage<T>>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (Some<T>())
        {
            EntityFilter.DestroyAllEntities();
        }
    }
}