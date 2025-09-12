using System;
using MoonTools.ECS;
using MyGame.Relations;
namespace MyGame.Systems;

public class OffsetSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;

    public OffsetSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach ((var parent, var child) in Relations<Offset>())
        {
            var position = Get<Position>(parent);
            position += GetRelationData<Offset>(parent, child).Value;
            Set(child, position);
        }
    }
}


