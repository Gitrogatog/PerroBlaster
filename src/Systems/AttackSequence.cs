using System;
using MoonTools.ECS;
using MyGame.Components;
namespace MyGame.Systems;

public class EnemyAttackSequence : MoonTools.ECS.System
{
    private Filter EntityFilter;

    public EnemyAttackSequence(World world) : base(world)
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