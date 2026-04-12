using System;
using System.Threading.Tasks;
using MoonTools.ECS;

namespace MyGame.Systems;

public class RemoveSystem<T> : MoonTools.ECS.System where T : unmanaged
{
    private Filter EntityFilter;

    public RemoveSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<T>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            Remove<T>(entity);
        }
    }
}