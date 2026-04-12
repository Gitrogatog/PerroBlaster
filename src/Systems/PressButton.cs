using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Components;
using MyGame.Relations;
using MyGame.Spawn;

namespace MyGame.Systems;

public class PressButtonSystem : MoonTools.ECS.System
{
    private Filter ClickFilter;

    public PressButtonSystem(World world) : base(world)
    {
        ClickFilter = FilterBuilder
            .Include<Clickable>()
            .Include<PressedThisFrame>()
            .Build();

    }

    public override void Update(TimeSpan delta)
    {

    }
}