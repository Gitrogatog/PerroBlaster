using System;
using MoonTools.ECS;
using MyGame.Components;
namespace MyGame.Systems;

public class _DefaultSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;

    public _DefaultSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {

        }
    }
}
