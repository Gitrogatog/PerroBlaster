using System;
using System.Threading.Tasks;
using MoonTools.ECS;

namespace MyGame.Systems;

public sealed class ReplaceSystem<T1, T2> : MoonTools.ECS.System where T1 : unmanaged where T2 : unmanaged
{
    private Filter EntityFilter;

    public ReplaceSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<T1>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            Remove<T1>(entity);
            Set(entity, new T2());
        }
    }
}