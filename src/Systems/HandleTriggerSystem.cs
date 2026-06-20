using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Spawn;
namespace MyGame.Systems;

public class HandleTriggerSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;

    public HandleTriggerSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        HandleTrigger<JumpTrigger>();
        HandleTrigger<TouchGroundTrigger>();
        HandleTrigger<BumpCeilingTrigger>();
        HandleTrigger<BumpWallTrigger>();
        HandleTrigger<KillTrigger>();
    }
    void HandleTrigger<T>() where T : unmanaged, ITrigger {
        while(Some<T>()) {
            var entity = GetSingletonEntity<T>();
            Remove<T>(entity);
            EntityPrefabs.HandleTrigger<T>(entity);
        }
    }
}