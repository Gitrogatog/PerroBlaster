using System;
using System.Collections.Generic;
using System.IO;

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
    Dictionary<int, Dictionary<string, HashSet<int>>> tilesetIdToEnum = new Dictionary<int, Dictionary<string, HashSet<int>>>();
    Dictionary<int, Dictionary<int, string>> tilesetIdToCustomData = new Dictionary<int, Dictionary<int, string>>();
    Dictionary<string, int> levelIds = new Dictionary<string, int>();
    // Dictionary<int, string[]> tileIdToCollisionType = new Dictionary<int, string[]>();


    public LoadLevelJSON(MoonTools.ECS.World world, Vector2I tileSize, int tileMult) : base(world)
    {
        this.tileSize = tileSize;
        this.tileMult = tileMult;
    }
    public void ReadFile(string filePath)
    {
        filePath = Path.Combine(AppContext.BaseDirectory, filePath);

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
        foreach(var tileset in jsonObject.defs.tilesets) {

            var enumSet = new Dictionary<string, HashSet<int>>();
            foreach (var enumTagData in tileset.enumTags)
            {
                string tag = enumTagData.enumValueId;
                HashSet<int> set= new HashSet<int>(enumTagData.tileIds.Length);
                foreach (int? tileId in enumTagData.tileIds)
                {
                    if(tileId.HasValue) {
                        set.Add(tileId.Value);
                    }
                    
                }
                enumSet[tag] = set;
            }
            tilesetIdToEnum[tileset.uid] = enumSet;

            var customDataSet = new Dictionary<int, string>();
            foreach(var customData in tileset.customData) {
                if(customData.data != "" && customData.data != null) {
                    customDataSet[customData.tileId] = customData.data;
                }
            }
            tilesetIdToCustomData[tileset.uid] = customDataSet;
        }
        for(int i = 0; i < jsonObject.levels.Length; i++) {
            levelIds[jsonObject.levels[i].iid] = i;
        }
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
        int levelWidth = level.pxWid / Dimensions.TILE_SIZE;
        int levelHeight = level.pxHei / Dimensions.TILE_SIZE;
        Console.WriteLine($"level width:{levelWidth} height:{levelHeight}");
        GlobalTilemap.Tilemap = new CustomTilemap.Tilemap(World, levelWidth, levelHeight);
        Globals.CameraMaxX = Math.Max(level.pxWid - Dimensions.GAME_W, 0);
        Globals.CameraMaxY = Math.Max(level.pxHei - Dimensions.GAME_H, 0);
        // TopTiles
        // TopButBelowPlayerTiles
        // MiniTiles
        // Tiles16
        
        // read through 
        Sprite tileGridSprite = SpriteAnimations.Tiles.Frames[0];
        ReadVisualTiles(level, tileGridSprite, "Tiles16", 16, 0.9f);
        ReadVisualTiles(level, tileGridSprite, "MiniTiles", 8, 0.8f);
        ReadVisualTiles(level, tileGridSprite, "TopButBelowPlayerTiles", 16, 0.7f);
        ReadVisualTiles(level, tileGridSprite, "TopTiles", 16, 0.2f);
        
        Layerinstance layerInstance = GetLayerInstanceByName(level, "Entities");
        if(layerInstance.__type == "Entities") {
            foreach (var entityInstance in layerInstance.entityInstances)
            {
                ReadEntity(entityInstance);
            }
        }
        
    }
    void ReadVisualTiles(Level level, Sprite tileGridSprite, string layerName, int localTileSize, float depth) {
        Layerinstance layerInstance = GetLayerInstanceByName(level, layerName);
        var tileEnumSet = tilesetIdToEnum[layerInstance.__tilesetDefUid.Value];
        var customDataSet = tilesetIdToCustomData[layerInstance.__tilesetDefUid.Value];
        foreach (var tile in layerInstance.gridTiles)
        {
            (int levelX, int levelY) = ((int)tile.px[0], (int)tile.px[1]); // position of tile in the ldtk world grid
            (int spriteX, int spriteY) = ((int)tile.src[0], (int)tile.src[1]);
            Sprite tileSprite = tileGridSprite.Slice(spriteX, spriteY, localTileSize, localTileSize);
            int tileId = (int)tile.t;
            // bool isAnimated = tilesetIdToEnum["Animated"].Contains(tileId);
            bool hasCollision = tileEnumSet["Solid"].Contains(tileId);
            MoonTools.ECS.Entity entity;
            if(customDataSet.TryGetValue(tileId, out string animName)) {
                SpriteAnimationInfo tileAnim = SpriteAnimations.Lookup(animName);
                if(tileAnim == null) {
                    Console.WriteLine($"couldnt find animation for {animName}");

                }
                entity = EntityPrefabs.CreateAnimatedTile(levelX + localTileSize / 2, levelY + localTileSize / 2, tileAnim, depth);
            } else {
                entity = EntityPrefabs.CreateTile(levelX + localTileSize / 2, levelY + localTileSize / 2, tileSprite, depth);
            }
            if(hasCollision) {
                EntityPrefabs.AddSolidCollision(entity, new Rectangle(16, 16, EffectorFlags.None, EffectedFlags.IsWall));
                EntityPrefabs.AddSolidTileCollision(entity, levelX / 16, levelY / 16);
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
        int tileX = entityInstance.__grid[0];
        int tileY = entityInstance.__grid[1];
        // Console.WriteLine($"read entity at {position}");
        JsonElement field;
        Console.WriteLine($"entity isntance: {entityInstance.__identifier}");
        switch (entityInstance.__identifier)
        {

            case "Player":
                {
                    var entity = CreateEntity();
                    Set(entity, new DestroyOnLoad());
                    Set(entity, new InitialPlayerSpawn(tileX, tileY));
                    
                    // EntityPrefabs.CreatePlayer(tileX, tileY);
                    break;
                }
            case "Player_nontile": {
                var entity = CreateEntity();
                Set(entity, new DestroyOnLoad());
                Set(entity, new InitialPlayerSpawn(tileX, tileY));
                Set(entity, new SpawnNonTilePlayer());
                break;
            }
            // case "Text": {
            //     var entity = EntityPrefabs.CreateText(position.X, position.Y, 12, Fonts.PixeltypeID, GetStringField(entityInstance, "String"));
            //     if(GetBoolField(entityInstance, "Black")) {
            //         Set(entity, new ColorBlend(Color.Black));
            //     }
            //     break;
            // }
            case "LevelTransition": {
                var entity = EntityPrefabs.CreateEntityOnTileGrid(tileX, tileY);
                // Set(entity, new DestroyOnLoad());
                // Set(entity, new TilePosition(tileX, tileY));
                Set(entity, new UUID(TextStorage.GetID(entityInstance.iid)));
                string levelTarget = GetLevelRef(entityInstance, "Location");
                string exitTarget = GetEntityRef(entityInstance, "Location");
                Set(entity, new ChangeLevelOnInteract(levelIds[levelTarget], TextStorage.GetID(exitTarget)));
                AddStepTalkInteract(entity, entityInstance);
                string soundID = GetStringField(entityInstance, "SoundID");
                switch(soundID) {
                    case "Pass":{
                        Console.WriteLine("Pass sound!");
                        Set(entity, new PlaySFXOnInteract(StaticAudio.Move));
                        break;
                    }
                    case "Door": {
                        Console.WriteLine("open sound!");
                        Set(entity, new PlaySFXOnInteract(StaticAudio.Open1));
                        break;
                    }
                    default:{
                        Console.WriteLine("no id found for: " + soundID);
                        break;
                    }
                }
                break;
            }
            case "LevelExit": {
                var entity = EntityPrefabs.CreateEntityOnTileGrid(tileX, tileY);
                // Set(entity, new DestroyOnLoad());
                // Set(entity, new TilePosition(tileX, tileY));
                Set(entity, new UUID(TextStorage.GetID(entityInstance.iid)));
                break;
            }
            case "PisonVisual": {
                var pison = CreateEntity();
                Set(pison, new Position(position.X - 8, position.Y - 24));
                Set(pison, new SpriteAnimation(SpriteAnimations.pison_map));
                Set(pison, new Depth(0.5f));
                Set(pison, new DestroyOnLoad());
                break;
            }
            case "Dialog": {
                Console.WriteLine("creating dialog");
                var entity = EntityPrefabs.CreateEntityOnTileGrid(tileX, tileY);
                string dialog = GetStringField(entityInstance, "Dialog");
                
                CloseDialogAction action = ConvertStringEnum<CloseDialogAction>.ToEnum(GetStringField(entityInstance, "EventOnFinish"));
                Console.WriteLine("dialog action " + action);
                Set(entity, new DisplayDialogOnInteract(TextStorage.GetID(dialog), action));
                Set(entity, new CanBeTalked());
                
                break;
            }
            case "DialogOnEnter": {
                string dialog = GetStringField(entityInstance, "Dialog");
                EntityPrefabs.CreateTimedMessage(new DisplayDialog(TextStorage.GetID(dialog), CloseDialogAction.None), 0.6f);
                break;
            }
            case "PlayMusic": {
                string musicId = GetStringField(entityInstance, "MusicID");
                StreamingSoundID id = musicId switch {
                    "Castle" => StreamingAudio.castle,
                    "Dungeon" => StreamingAudio.dungeon,
                    "Overworld" => StreamingAudio.overworld1,
                    "Town" => StreamingAudio.fortress_city,
                    "Hell" => StreamingAudio.castle,
                    _ => StreamingAudio.rm_open01
                };
                Set(CreateEntity(), new StopMusicUnless(id));
                Set(CreateEntity(), new AddAfterTime<PlayMusic>(0.5f, new PlayMusic(id)));
                break;
            }
        }
    }
    void AddStepTalkInteract(MoonTools.ECS.Entity entity, Entityinstance entityInstance) {
        if(GetBoolField(entityInstance, "StepInteract")) {
            Set(entity, new CanBeStepped());
        }
        if(GetBoolField(entityInstance, "TalkInteract")) {
            Set(entity, new CanBeTalked());
        }
    }
    JsonElement GetFieldValue(Entityinstance entity, string name) => (JsonElement)GetFieldFromName(entity, name).__value;
    bool GetBoolField(Entityinstance entity, string name) => GetFieldValue(entity, name).GetBoolean();
    int GetIntField(Entityinstance entity, string name) => GetFieldValue(entity, name).GetInt32();
    float GetFloatField(Entityinstance entity, string name) => GetFieldValue(entity, name).GetSingle();
    string GetStringField(Entityinstance entity, string name) => GetFieldValue(entity, name).GetString();
    string GetEntityRef(Entityinstance entity, string name) => GetFieldValue(entity, name).GetProperty("entityIid").GetString();
    string GetLevelRef(Entityinstance entity, string name) => GetFieldValue(entity, name).GetProperty("levelIid").GetString();

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
