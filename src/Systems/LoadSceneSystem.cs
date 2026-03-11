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
        if(Some<ControlledByPlayer>()) {
            var entity = GetSingletonEntity<ControlledByPlayer>();
            if(TryGet<SpriteAnimation>(entity, out SpriteAnimation anim)) {
                var data = new LastPlayerData(anim);
                if(Some<LastPlayerData>()) {
                    Set(GetSingletonEntity<LastPlayerData>(), data);
                }
                else {
                    Set(CreateEntity(), data);
                }
            }
        }
        while (Some<DestroyOnLoad>())
        {
            Destroy(GetSingletonEntity<DestroyOnLoad>());
        }
        while (Some<DestroyOnPlayerRespawn>()){
            Destroy(GetSingletonEntity<DestroyOnPlayerRespawn>());
        }
    }
}
