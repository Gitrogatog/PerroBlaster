using System;
using MoonTools.ECS;
using MoonWorks.Graphics;
using MyGame.Components;
using MyGame.Data;
using MyGame.Spawn;
namespace MyGame.Systems;

public class OnEnterRoomSystem : MoonTools.ECS.System
{
    private MoonTools.ECS.Filter RoomIDFilter;

    public OnEnterRoomSystem(World world) : base(world)
    {
        RoomIDFilter = FilterBuilder
            .Include<RoomID>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        if(!Some<ControlledByPlayer>()) return;
        var position = Get<Position>(GetSingletonEntity<ControlledByPlayer>());
        int roomX = position.X / Dimensions.ROOM_X;
        int roomY = position.Y / Dimensions.ROOM_Y;
        // Console.WriteLine($"roomX{roomX} roomY{roomY}");
        // Console.WriteLine($"posX{position.X} posY{position.Y} drX{Dimensions.ROOM_X} drY{Dimensions.ROOM_Y}");
        // Console.WriteLine($"camX:{Globals.CameraX} camY:{Globals.CameraY}");
        if(roomX != Globals.CurrentRoomX || roomY != Globals.CurrentRoomY) {
            Globals.CurrentRoomX = roomX;
            Globals.CurrentRoomY = roomY;
            Globals.CameraX = Dimensions.ROOM_X * roomX;
            Globals.CameraY = Dimensions.ROOM_Y * roomY;
            
            foreach (var entity in RoomIDFilter.Entities){
                (int x, int y) = Get<RoomID>(entity);
                if(x != roomX || y != roomY) continue;
                if(TryGet<EnemySpawnPoint>(entity, out EnemySpawnPoint enemySpawnPoint)) {
                    Console.WriteLine("creating enemy!");
                    EntityPrefabs.CreateEnemy(enemySpawnPoint, entity);
                }
                else if(Has<IsCheckpoint>(entity)) {
                    var pos = Get<Position>(entity);
                    Globals.CheckpointX = pos.X;
                    Globals.CheckpointY = pos.Y;
                }
                else if(TryGet(entity, out RectangleSpawnPoint rectangleSpawnPoint)) {
                    Console.WriteLine("creating rect thing!");
                    CreateRectThing(rectangleSpawnPoint, entity);
                }
                else {
                    AddOnRoomEnter<CanInteract>(entity);
                    AddOnRoomEnter<Solid>(entity);
                    AddOnRoomEnter<BecomeSolidWhenNotColliding>(entity);
                }
            }
            if(Some<AxeState>() && GetSingleton<AxeState>() != AxeState.Held) {
                EntityPrefabs.RecallAxe(GetSingletonEntity<AxeState>());
            }
        }
    }
    void AddOnRoomEnter<T>(Entity entity) where T : unmanaged {
        if(TryGet<AddOnRoomEnter<T>>(entity, out AddOnRoomEnter<T> stuff)) {
            Set(entity, stuff.Component);
        }
    }
    void CreateRectThing(RectangleSpawnPoint spawnPoint, Entity parent) {
        var entity = CreateEntity();
        Set(entity, new Position(spawnPoint.X, spawnPoint.Y));
        Set(entity, new Rectangle(0, 0, spawnPoint.Width, spawnPoint.Height, EffectorFlags.None, EffectedFlags.IsWall));
        Set(entity, new DestroyOnPlayerRespawn());
        Set(entity, new CanInteract());
        switch(spawnPoint.Type) {
            case RectThingType.EnterFence: {
                Set(entity, new BecomeSolidWhenNotColliding());
                // Set(entity, new CollisionForceMoveForOneFrame());
                Set(entity, new ColorBlend(Colors.SolidWall));
                EntityPrefabs.Mirror<CollisionForceMoveForOneFrame>(parent, entity);
                // Console.WriteLine($"ha0s force move: {Has<CollisionForceMoveForOneFrame>(entity)}");
                break;
            }
            case RectThingType.ExitFence: {
                Set(entity, new Solid());
                Set(entity, new ColorBlend(Colors.SolidWall));
                Set(entity, new DestroyOnCompleteRoom());
                break;
            }
        }
        Set(entity, new DrawAsRectangle());
    }
    void CreateEnterFence(int X, int Y, int Width, int Length){
        var entity = CreateEntity();
        Set(entity, new Position(X, Y));
        Set(entity, new AddOnRoomEnter<BecomeSolidWhenNotColliding>());
        Set(entity, new CanInteract());
        Set(entity, new CollisionForceMoveForOneFrame());
        Set(entity, new DestroyOnPlayerRespawn());
    }
    // void CreateExitFence(int X, int Y) {
    //     var entity = CreateEntity();
    //     Set(entity, new Position(position.X, position.Y));
    //     Set(entity, new Solid());
    //     Set(entity, new CanInteract());
    //     Set(entity, new Rectangle(0, 0, entityInstance.width, entityInstance.height, EffectorFlags.None, EffectedFlags.IsWall));
    //     Set(entity, new DrawAsRectangle());
    //     Set(entity, new DestroyOnLoad());
    // }
}
