using System;
using System.Collections.Generic;
using System.Net;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Spawn;
namespace MyGame.Systems;

public class TileCollision : MoonTools.ECS.System
{
    private Filter EntityFilter;
    // private Filter TalkFilter;

    public TileCollision(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<TilePosition>()
            .Include<CanInteract>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        if(!Some<ControlledByPlayer>()) return;
        var entity = GetSingletonEntity<ControlledByPlayer>();
        if(Has<MoveToTile>(entity)) return;
        (int x, int y) = Get<TilePosition>(entity);
        if(Has<FinishStepThisFrame>(entity)) {
            Console.WriteLine("running finish step check!");
            List<Entity> targets = GlobalTilemap.GetEntities(x, y);
            foreach(var target in targets) {
               
                if(Some<StopInteract>()) break;
                if(target == entity || !Has<CanBeStepped>(target)) continue;
                Console.WriteLine($"found entity {target}");
                RunInteract(entity, target);
            }
        }
        if(Has<AttemptTalkThisFrame>(entity)) {
            Console.WriteLine("attemping to talk!");
            (int faceX, int faceY) = Get<FacingDirection>(entity);
            
            x += faceX;
            y += faceY;
            Console.WriteLine($"talk pos: {x} {y}");
            if(!GlobalTilemap.IsWithinBounds(x, y)) return;
            List<Entity> targets = GlobalTilemap.GetEntities(x, y);
            foreach(var target in targets) {
                Console.WriteLine("talk interacting");
                if(Some<StopInteract>()) break;
                if(target == entity || !Has<CanBeTalked>(target)) continue;
                Console.WriteLine("found target!");
                RunInteract(entity, target);
            }
        }
    }
    void RunInteract(Entity entity, Entity target) {
        if(Has<ChangeLevelOnInteract>(target)) {
            (int levelID, int entityUUID) = Get<ChangeLevelOnInteract>(target);
            // Set(CreateEntity(), new ChangeLevel(levelID, entityUUID));
            EntityPrefabs.ChangeLevelFadeout(levelID, entityUUID);
            Set(EntityPrefabs.CreateDestroyOnLoad(), new StopInteract());
            Console.WriteLine("attempt change level interact!");
        }
        // talk
        if(Has<DisplayDialogOnInteract>(target)) {
            Console.WriteLine("dialog interacting!");
            (int textId, CloseDialogAction closeDialogAction) = Get<DisplayDialogOnInteract>(target);
            Set(CreateEntity(), new DisplayDialog(textId, closeDialogAction));
        }
    }
}
