using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.IO;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
// using ldtk;
using ldtk;
using MoonTools.ECS;
using MoonWorks.Graphics;
using MyGame;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Spawn;
using MyGame.Systems;
using MyGame.Utility;
public class LoadLevelJSON : MoonTools.ECS.Manipulator
{
    // LDTKJsonObject jsonObject;
    Rootobject jsonObject;
    Vector2I tileSize;
    int tileMult;
    Dictionary<(int, int), (int, int)> tileSliceDict = new Dictionary<(int, int), (int, int)>(300);
    Dictionary<(int, int), Color> colorsDict = new Dictionary<(int, int), Color>(300);
    Dictionary<string, Dictionary<int, Color>> intGridToColorDict = new Dictionary<string, Dictionary<int, Color>>();
    Dictionary<int, string> tileIdToCollisionType = new Dictionary<int, string>();


    public LoadLevelJSON(MoonTools.ECS.World world, Vector2I tileSize, int tileMult) : base(world)
    {
        this.tileSize = tileSize;
        this.tileMult = tileMult;
    }
    public void ReadFile(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
        {
            Console.WriteLine("No such file: " + filePath);
            return;
        }

        // string contents = File.ReadAllText(filePath);

        // jsonObject = LdtkJson.FromJson(contents);
        jsonObject = RootobjectReader.ReadJson(filePath);
        // PreloadColorsFromDefs();
        Console.WriteLine($"READING JSON at {Path.GetFullPath(filePath)} : levels count: {jsonObject.levels.Length}");
        var tileset = jsonObject.defs.tilesets[0];
        foreach (var enumTagData in tileset.enumTags)
        {
            string tag = enumTagData.enumValueId;
            foreach (int tileId in enumTagData.tileIds)
            {
                tileIdToCollisionType[tileId] = tag;
            }
        }
        // InitTilemap();

    }
    public bool ReadLevel(int id)
    {
        if (id < 0 || jsonObject.levels.Length <= id)
        {
            Console.WriteLine($"ERROR: level id {id} is out of bounds!");
            return false;
        }
        ReadLevel(jsonObject, jsonObject.levels[id]);
        // EntityPrefabs.CreateMessage(new StartMusicAndSetVolume(0.5f, 0));
        // EntityPrefabs.CreateMessage(new StopMusic(0));
        return true;
    }
    void ReadLevel(Rootobject rootobject, Level level)
    {
        shouldIgnore = false;
        foreach (var fieldInstance in level.fieldInstances)
        {

        }
        Layerinstance layerInstance = GetLayerInstanceByName(level, "Tiles");
        // read through 
        Sprite tileGridSprite = SpriteAnimations.TilesAbstract.Frames[0];

        foreach (var tile in layerInstance.gridTiles)
        {
            (int levelX, int levelY) = ((int)tile.px[0], (int)tile.px[1]); // position of tile in the ldtk world grid
            (int spriteX, int spriteY) = ((int)tile.src[0], (int)tile.src[1]);
            Sprite tileSprite = tileGridSprite.Slice(spriteX, spriteY, tileSize.X, tileSize.Y);
            var entity = EntityPrefabs.CreateTile(LevelToWorld(levelX), LevelToWorld(levelY), tileSprite);
            int tileId = (int)tile.t;
            if (tileIdToCollisionType.TryGetValue(tileId, out string enumId))
            {
                switch (enumId)
                {
                    case "SolidFull":
                        {
                            EntityPrefabs.AddSolidCollision(entity, new Rectangle(16, 16, EffectorFlags.None, EffectedFlags.IsWall));
                            break;
                        }
                    case "RoofTop":
                        {
                            // EntityPrefabs.AddSolidCollision(entity, new Rectangle(-8, -6, 16, 4));
                            break;
                        }
                }
            }
        }
        Stores.PathStorage.Reset();
        if(true) { // Stores.PathStorage.IsEmpty
            foreach(Toc toc in rootobject.toc) {
                if(toc.identifier == "Path") {
                    foreach(var instanceData in toc.instancesData) {
                        Stores.PathStorage.Register(instanceData.iids.entityIid, new List<Vector2>());
                    }
                    
                }
            }
        }
        
        layerInstance = GetLayerInstanceByName(level, "Entities");
        if(layerInstance.__type == "Entities"){
            foreach (var entityInstance in layerInstance.entityInstances)
            {
                ReadEntity(entityInstance);
            }
        }
        
    }
    int LevelToWorld(int levelPos) => (levelPos + tileSize.X / 2) * tileMult;
    int TileToWorld(int tilePos) => LevelToWorld(tilePos * 16);
    Layerinstance GetLayerInstanceByName(Level level, string name)
    {
        foreach (Layerinstance layerInstance in level.layerInstances)
        {
            if (layerInstance.__identifier == name)
            {
                return layerInstance;
            }
        }
        return default;
    }
    Layer GetLayerDefByName(string name)
    {
        foreach (var layerDef in jsonObject.defs.layers)
        {
            if (layerDef.identifier == name)
            {
                return layerDef;
            }
        }
        return default;
    }
    bool shouldIgnore = false;
    // void ReadTocEntity(Entityinstance entityInstance) {
    //     Vector2I position = new Vector2I((int)entityInstance.px[0] * tileMult, (int)entityInstance.px[1] * tileMult); //+ tileSize / 2;
    //     switch(entityInstance.__identifier) {
    //         case "Path": {
    //             break;
    //         }
    //     }
    // }
    void ReadEntity(Entityinstance entityInstance)
    {
        Vector2I position = new Vector2I(LevelToWorld(entityInstance.px[0]), LevelToWorld(entityInstance.px[1])); //+ tileSize / 2;
        // Console.WriteLine($"read entity at {position}");
        JsonElement field;
        Console.WriteLine($"entity isntance: {entityInstance.__identifier}");
        int roomX = position.X / Dimensions.ROOM_X;
        int roomY = position.Y / Dimensions.ROOM_Y;
        switch (entityInstance.__identifier)
        {

            case "Player":
                {
                    EntityPrefabs.CreatePlayer(position.X, position.Y);
                    Globals.CheckpointX = position.X;
                    Globals.CheckpointY = position.Y;
                    Globals.CameraX = position.X;
                    Globals.CameraY = position.Y;
                    break;
                }
            case "EnterFence": {
                var entity = CreateEntity();
                Set(entity, new DestroyOnLoad());
                Set(entity, new RoomID(roomX, roomY));

                Set(entity, new CollisionForceMoveForOneFrame(MyConvertEnum.CardinalToVec(MyConvertEnum.ToCardinal(GetStringField(entityInstance, "Cardinal")))));
                Set(entity, new RectangleSpawnPoint(entityInstance.px[0], entityInstance.px[1], entityInstance.width, entityInstance.height, RectThingType.EnterFence));
                break;
            }
            case "ExitFence": {
                var entity = CreateEntity();
                Set(entity, new DestroyOnLoad());
                Set(entity, new RoomID(roomX, roomY));
                Set(entity, new RectangleSpawnPoint(entityInstance.px[0], entityInstance.px[1], entityInstance.width, entityInstance.height, RectThingType.ExitFence));
                // var entity = CreateEntity();
                // Set(entity, new Position(position.X, position.Y));
                // Set(entity, new Solid());
                // Set(entity, new CanInteract());
                // Set(entity, new Rectangle(0, 0, entityInstance.width, entityInstance.height, EffectorFlags.None, EffectedFlags.IsWall));
                // Set(entity, new DrawAsRectangle());
                // Set(entity, new DestroyOnLoad());
            
                break;
            }
            case "PitTest": {
                var entity = CreateEntity();
                Set(entity, new Position(position.X, position.Y));
                // Set(entity, new Solid());
                Set(entity, new CanInteract());
                Set(entity, new Rectangle(0, 0, entityInstance.width, entityInstance.height, EffectorFlags.None, EffectedFlags.IsPit));
                Set(entity, new DrawAsRectangle());
                Set(entity, new ColorBlend(Color.Blue));
                Set(entity, new DestroyOnLoad());
                break;
            }
            case "Chaser": {
                EntityPrefabs.CreateEnemySpawnPoint(position.X, position.Y, EnemyType.Circle);
                Console.WriteLine("Creating chaser!");
                break;
            }
            case "Shooter": {
                // var entity = EntityPrefabs.CreateBaseEnemy(position.X, position.Y, 16, 16);
                EntityPrefabs.CreateEnemySpawnPoint(position.X, position.Y, EnemyType.Triangle);
                break;
            }
            case "ShooterPath": {
                // var entity = EntityPrefabs.CreateBaseEnemy(position.X, position.Y, 16, 16);
                var entity = EntityPrefabs.CreateEnemySpawnPoint(position.X, position.Y, EnemyType.Triangle);
                var pathRef = GetEntityRef(entityInstance, "PathRef");
                Set(entity, new FollowPath(Stores.PathStorage.GetID(pathRef), GetIntField(entityInstance, "StartPoint")));
                if(GetBoolField(entityInstance, "InvertPathFollow")){
                    Set(entity, new InvertPath());
                }
                break;
            }
            case "Path": {
                List<Vector2> points = Stores.PathStorage.Get(Stores.PathStorage.GetID(entityInstance.iid));
                points.Add(new Vector2(position.X, position.Y));
                foreach(var jsonElement in GetFieldValue(entityInstance, "Points").EnumerateArray()) {
                    Console.WriteLine($"point element: {jsonElement}");
                    var pointDict = jsonElement.Deserialize<Dictionary<string, int>>();
                    points.Add(new Vector2(TileToWorld(pointDict["cx"]), TileToWorld(pointDict["cy"])));
                }
                foreach(Vector2 point in points){
                    Console.WriteLine($"added point: {point}");
                }

                break;
            }
            case "Text": {
                var entity = EntityPrefabs.CreateText(position.X, position.Y, 12, Fonts.PixeltypeID, GetStringField(entityInstance, "String"));
                if(GetBoolField(entityInstance, "Black")) {
                    Set(entity, new ColorBlend(Color.Black));
                }
                break;
            }
        }
    }
    JsonElement GetFieldValue(Entityinstance entity, string name) => (JsonElement)GetFieldFromName(entity, name).__value;
    bool GetBoolField(Entityinstance entity, string name) => GetFieldValue(entity, name).GetBoolean();
    int GetIntField(Entityinstance entity, string name) => GetFieldValue(entity, name).GetInt32();
    float GetFloatField(Entityinstance entity, string name) => GetFieldValue(entity, name).GetSingle();
    string GetStringField(Entityinstance entity, string name) => GetFieldValue(entity, name).GetString();
    string GetEntityRef(Entityinstance entity, string name) => GetFieldValue(entity, name).GetProperty("entityIid").GetString();

    // Point GetPointField(Entityinstance entity, string name) => (Point)(GetFieldFromName(entity, name) as PointField).Value[0];
    Fieldinstance GetFieldFromName(Entityinstance entity, string name)
    {
        foreach (Fieldinstance fieldInstance in entity.fieldInstances)
        {
            if (fieldInstance.__identifier == name)
            {
                return fieldInstance;
            }
        }
        Console.WriteLine($"Couldn't find field instance of name {name}");
        return default;
    }
    bool TryGetFieldFromName(Entityinstance entity, string name, out Fieldinstance value)
    {
        foreach (Fieldinstance fieldInstance in entity.fieldInstances)
        {
            if (fieldInstance.__identifier == name)
            {
                value = fieldInstance;
                return true;
            }
        }
        value = default;
        return false;
    }
    // void PreloadColorsFromDefs()
    // {
    //     foreach (var layerDef in jsonObject.defs.layers)
    //     {
    //         if (layerDef.type == "IntGrid")
    //         {
    //             var colorArr = layerDef.intGridValues;
    //             Dictionary<int, Color> idToColor = new Dictionary<int, Color>(colorArr.Length);
    //             foreach (var entry in colorArr)
    //             {
    //                 idToColor[(int)entry.value] = ColorUtils.HexStringToColor(entry.color.Substring(1));
    //                 Console.WriteLine(idToColor[(int)entry.value]);
    //             }
    //             intGridToColorDict[layerDef.identifier] = idToColor;
    //         }
            
    //     }

    // }
    void CreateColorSpritesFromIntgridLayer(Level level, string name)
    {
        var layer = GetLayerInstanceByName(level, name);
        var idToColor = intGridToColorDict[name];
        var intGrid = layer.intGridCsv;
        (int xTiles, int yTiles) = (layer.__cWid, (int)layer.__cHei);
        int sizeOfTile = (int)layer.__gridSize;
        for (int i = 0; i < intGrid.Length; i++)
        {
            int gridContent = (int)intGrid[i];
            if (gridContent == 0) continue;
            (int tileX, int tileY) = (i % xTiles, i / xTiles);
            var entity = World.CreateEntity();
            World.Set(entity, new Position((tileX - 0.5f) * tileSize.X * tileMult, (tileY - 0.5f) * tileSize.Y * tileMult));
            World.Set(entity, new ColorBlend(idToColor[gridContent]));
            World.Set(entity, new SpriteScale(tileSize * tileMult));
            World.Set(entity, SpriteAnimations.Pixel.Frames[0]);
            World.Set(entity, new Depth(10));
            World.Set(entity, new DestroyOnLoad());
        }
    }
    void LoadSpriteTilesFromTileLayer(Layerinstance layerInstance) // loads in the tile sprite data from the "Visuals" tile layer
    {
        foreach (var tile in layerInstance.gridTiles)
        {
            Vector2I spritePosOnSheet = new Vector2I(tile.src[0], tile.src[1]);
            Vector2I tilePos = new Vector2I(tile.px[0] / tileSize.X, tile.px[1] / tileSize.Y);
        }
    }
}
