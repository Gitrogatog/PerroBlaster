using System;
using System.Collections.Generic;
using MoonTools.ECS;
namespace MyGame.Systems;
public class SystemGroup : MoonTools.ECS.System
{
    public List<MoonTools.ECS.System> Systems;
    public SystemGroup(World World) : base(World)
    {
        Systems = new List<MoonTools.ECS.System>();
    }
    public SystemGroup Add(MoonTools.ECS.System system)
    {
        Systems.Add(system);
        return this;
    }
    public override void Update(TimeSpan delta)
    {
        foreach (var system in Systems)
        {
            system.Update(delta);
        }
    }
}