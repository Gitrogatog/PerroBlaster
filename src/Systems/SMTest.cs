using System;
using MoonTools.ECS;
using MyGame.Components;
namespace MyGame.Systems;

public class SMTestSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;

    public SMTestSystem(World world) : base(world)
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
    // movement behaviors:
    // walk back and forth
    // jump and shoot
    // dash forward with attack
    // shoot sequence: 
    void Process() {
        
    }
}