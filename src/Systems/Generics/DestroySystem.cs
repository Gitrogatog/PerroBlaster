using System;
using MoonTools.ECS;

namespace MyGame.Systems;

public class DestroySystem<T> : MoonTools.ECS.System where T : unmanaged
{
    private Filter EntityFilter;

    public DestroySystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<T>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        EntityFilter.DestroyAllEntities();
    }
}