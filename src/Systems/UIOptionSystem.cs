using System;
using MoonTools.ECS;
using MyGame.Utility;
using MyGame.Components;
using MyGame.Relations;
using MyGame.Spawn;
using MyGame.Content;
namespace MyGame.Systems;

public class UIOptionSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;
    int currentIndex = 0;

    public UIOptionSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<UIOption>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        if(EntityFilter.Count == 0){
            currentIndex = 0;
            return;
        }
        // bool hasCurrentOption = AttemptInitCurrentOption();
        if(Some<PreventInput>()) {
            Console.WriteLine("disable all ui select");
            return;
        }
        // if(!hasCurrentOption) {
        //     Console.WriteLine("no current option");
        //     return;
        // }
        // Console.WriteLine("we have something selecteed!");
        // Console.WriteLine(EntityFilter.Count);
        int change = InputUtils.InputToAxis(GlobalInput.Current.Up.IsPressed, GlobalInput.Current.Down.IsPressed);
        if(change != 0) {
            // Console.WriteLine(change);
            // int currentIndex = GetCurrentIndex();
            currentIndex += change;
            Console.WriteLine(currentIndex);
            if(currentIndex < 0) {
                Console.WriteLine("loop to max");
                currentIndex = EntityFilter.Count - 1;
            }
            else if (currentIndex >= EntityFilter.Count) {
                Console.WriteLine("loop to 0");
                currentIndex = 0;
            }
            Console.WriteLine(currentIndex);
            EntityPrefabs.PlaySFX(StaticAudio.Cursor1);
            // RemoveAll<CurrentOption>();
            
            if(Some<SelectHighlight>()) {
                var optionEntity = EntityFilter.NthEntity(currentIndex);
                
                var position = Get<Position>(optionEntity);
                var selectEntity = GetSingletonEntity<SelectHighlight>();
                (int xOffset, int yOffset) = Get<SelectHighlight>(selectEntity);
                Set(selectEntity, position + new Position(xOffset, yOffset));
            }
            // Set(optionEntity, new CurrentOption());
        }
        // Console.WriteLine(GetSingletonEntity<CurrentOption>());
        if(GlobalInput.Current.Interact.IsPressed) {
            PerformSelect(EntityFilter.NthEntity(currentIndex));
        }
    }
    void PerformSelect(Entity entity) {
        Console.WriteLine("performing seect");
        if(Has<PlaySFXOnSelect>(entity)) {
            EntityPrefabs.PlaySFX(Get<PlaySFXOnSelect>(entity).ID);
        }
        if(Has<ChangeSceneOnSelect>(entity)) {
            GameSceneType scene = Get<ChangeSceneOnSelect>(entity).Scene;
            Console.WriteLine("changing scene to: " + scene);
            EntityPrefabs.ChangeSceneFadeout(scene);
            // Set(CreateEntity(), new ChangeGameScene(scene));
        }
        // else if (Has<SetCharacterTypeOnSelect>(entity)) {
        //     DestroyAll<CharacterType>();
        //     Set(CreateEntity(), Get<SetCharacterTypeOnSelect>(entity).Value);
        // }
        else if (Has<CloseWindowOnSelect>(entity)) {
            Set(CreateEntity(), new CloseGameWindow());
        }
    }
    int GetCurrentIndex() {
        int index = 0;
        Entity singleton = GetSingletonEntity<CurrentOption>();
        foreach(Entity entity in EntityFilter.Entities) {
            if(entity == singleton) {
                return index;
            }
            index++;
        }
        return 0;
    }
    // bool AttemptInitCurrentOption() {
    //     if(Some<CurrentOption>()) {
    //         return true;
    //     }
    //     foreach(Entity entity in EntityFilter.Entities) {
    //         if(!HasInRelation<DontDraw>(entity)) {
    //             Set(entity, new CurrentOption());
    //             return true;
    //         }
    //     }
    //     return false;
    // }
}
