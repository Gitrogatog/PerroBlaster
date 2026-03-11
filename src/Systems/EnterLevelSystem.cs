using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Data;
using MyGame.Spawn;
namespace MyGame.Systems;

public class EnterLevelSystem : MoonTools.ECS.System
{
    private Filter UUIDFilter;

    public EnterLevelSystem(World world) : base(world)
    {
        UUIDFilter = FilterBuilder
            .Include<UUID>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        if(Some<InitialPlayerSpawn>() && !Some<ChangeLevel>()) {
            (int x, int y) = GetSingleton<InitialPlayerSpawn>();
            var entity = EntityPrefabs.CreatePlayer(x, y);
            // Console.WriteLine($"player:{entity}");
            Console.WriteLine("initial spawn");
        }
        else if(Some<ChangeLevel>()) {
            (int levelID, int entityUUID) = GetSingleton<ChangeLevel>();
            var levelEntity = GetUUID(entityUUID);
            (int x, int y) = Get<TilePosition>(levelEntity);
            var entity = EntityPrefabs.CreatePlayer(x, y);
            Console.WriteLine($"levelId:{levelID} entityuuid:{entityUUID}");
        }
        else {
            Console.WriteLine("defaulting to player pos!");
            var entity = EntityPrefabs.CreatePlayer(Globals.DefaultPlayerX, Globals.DefaultPlayerY);
            if(Has<SpawnThingWhen<KillTrigger>>(entity)) {

            }
            Console.WriteLine(entity);
        }
        DestroyAll<InitialPlayerSpawn>();
        DestroyAll<ChangeLevel>();
        EntityPrefabs.EnterLevelFadein();
        
    }
    Entity GetUUID (int uuid) {
        foreach (var entity in UUIDFilter.Entities)
        {
            if(Get<UUID>(entity).ID == uuid) return entity;
        }
        Console.WriteLine($"failed to find for id: {uuid}, string: {TextStorage.GetString(uuid)}");
        return default;
    }
}