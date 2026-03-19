using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Spawn;
namespace MyGame.Systems;

public class DialogSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;
    int currentDialogID = -1;
    CloseDialogAction closeDialogAction;

    public DialogSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<DisplayDialog>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        // Console.WriteLine(EntityFilter.Count);

        foreach (var entity in EntityFilter.Entities)
        {
            Console.WriteLine("create dialog box");
            (currentDialogID, closeDialogAction) = Get<DisplayDialog>(entity);
            // string text = TextStorage.GetString(currentDialogID);
            EntityPrefabs.CreateTextbox(currentDialogID);
            var preventInput = CreateEntity();
            Set(preventInput, new PreventInput());
            Set(preventInput, new DestroyOnDialogBoxFullyClose());
            Destroy(entity);

        }
        if(Some<CanAdvanceDialog>() && GlobalInput.Current.Interact.IsPressed) {
            DestroyAll<CanAdvanceDialog>();
            DestroyAll<DestroyOnDialogBoxClose>();
            if(Some<IsDialogBox>()) {
                var entity = GetSingletonEntity<IsDialogBox>();
                // Set(entity, new GrowRectToSize(12, 6, 0));
                var animation = Get<SpriteAnimation>(entity);
                Set(entity, 
                    animation
                        .ChangeFramerate(-animation.FrameRate)
                        .ForceRawFrame(animation.SpriteAnimationInfo.Frames.Length - 1));
                Set(entity, new PerformDialogBoxFullyClose());
                Console.WriteLine($"closeDialogAction {closeDialogAction}");
                switch(closeDialogAction) {
                    case CloseDialogAction.StartFakeBattle: {
                        Console.WriteLine("starting fake battle");
                        Set(CreateEntity(), new StartFakeBattle());
                        break;
                    }
                    case CloseDialogAction.PlayGameOverSFX: {
                        EntityPrefabs.PlaySFX(StaticAudio.game_over);
                        Set(CreateEntity(), new StopAllMusic());
                        EntityPrefabs.CreateTimedMessage(new PlayMusic(StreamingAudio.dungeon, false), 6f);
                        break;
                    }
                }
                // Set(entity, new DestroyOnAnimationFinish());
            }
        }
    }
}
