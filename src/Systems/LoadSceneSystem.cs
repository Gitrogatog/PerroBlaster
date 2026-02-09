using System;
using MoonTools.ECS;
using MyGame.Components;
namespace MyGame.Systems;

public class LoadSceneSystem : MoonTools.ECS.System
{
    public LoadSceneSystem(World world) : base(world)
    {
        
    }

    public override void Update(TimeSpan delta)
    {
        while (Some<DestroyOnLoad>())
        {
            Destroy(GetSingletonEntity<DestroyOnLoad>());
        }
        while (Some<DestroyOnPlayerRespawn>()){
            Destroy(GetSingletonEntity<DestroyOnPlayerRespawn>());
        }
    }
}
