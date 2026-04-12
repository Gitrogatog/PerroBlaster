using System;
using MoonTools.ECS;
using MyGame.Components;
namespace MyGame.Systems;

public class FacingSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;

    public FacingSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<Facing>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            if(TryGet<IntendedMove>(entity, out IntendedMove move) && move.Value.X != 0) {
                Set(entity, new Facing(move.Value.X < 0));
            }
            else if(TryGet<Velocity>(entity, out Velocity velocity) && velocity.Value.X != 0) {
                Set(entity, new Facing(velocity.Value.X < 0));
            }
        }
    }
}