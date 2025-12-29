using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Content;
using MyGame.Relations;
using MyGame.Spawn;
using MyGame.Utility;
namespace MyGame.Systems;

public class Collision : MoonTools.ECS.System
{
    private Filter EntityFilter;

    public Collision(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach ((var entityA, var entityB) in Relations<Colliding>())
        {

        }

        foreach ((var entityA, var entityB) in Relations<TouchingSolid>())
        {
        }
    }
}