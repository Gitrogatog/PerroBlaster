using System;
using System.Threading.Tasks;
using MoonTools.ECS;
using MyGame.Components;

namespace MyGame.Systems;

public sealed class RequireParentSystem<T> : MoonTools.ECS.System where T : unmanaged
{
    private Filter EntityFilter;

    public RequireParentSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<RequireParent<T>>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            if(!HasInRelation<T>(entity)){
                Destroy(entity);
            }
        }
    }
}