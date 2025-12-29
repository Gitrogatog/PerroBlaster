using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Numerics;
using ldtk;
using MoonTools.ECS;
using MoonWorks.Graphics;
using MyGame.Components;
using MyGame.Content;
using MyGame.Spawn;
using MyGame.Utility;
public class LoadLevelJSON
{
    MoonTools.ECS.World World;
    // LDTKJsonObject jsonObject;
    LdtkJson jsonObject;
    Vector2I tileSize;
    int tileMult;
    Dictionary<(int, int), (int, int)> tileSliceDict = new Dictionary<(int, int), (int, int)>(300);
    Dictionary<(int, int), Color> colorsDict = new Dictionary<(int, int), Color>(300);
    Dictionary<string, Dictionary<int, Color>> intGridToColorDict = new Dictionary<string, Dictionary<int, Color>>();


    public LoadLevelJSON(MoonTools.ECS.World world, Vector2I tileSize, int tileMult)
    {
        World = world;
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

        string contents = File.ReadAllText(filePath);

        jsonObject = LdtkJson.FromJson(contents);
        PreloadColorsFromDefs();
        Console.WriteLine($"READING JSON at {Path.GetFullPath(filePath)} : levels count: {jsonObject.Levels.Length}");
        // InitTilemap();

    }
    public bool ReadLevel(int id)
    {
        if (id < 0 || jsonObject.Levels.Length <= id)
        {
            Console.WriteLine($"ERROR: level id {id} is out of bounds!");
            return false;
        }
        ReadLevel(jsonObject.Levels[id]);
        return true;
    }
    void ReadLevel(Level level)
    {
        foreach (FieldInstance fieldInstance in level.FieldInstances)
        {

        }
        LayerInstance layerInstance = GetLayerInstanceByName(level, "Entities");
        foreach (EntityInstance entityInstance in layerInstance.EntityInstances)
        {
            ReadEntity(entityInstance);
        }
    }
    LayerInstance GetLayerInstanceByName(Level level, string name)
    {
        foreach (LayerInstance layerInstance in level.LayerInstances)
        {
            if (layerInstance.Identifier == name)
            {
                return layerInstance;
            }
        }
        return null;
    }
    LayerDefinition GetLayerDefByName(string name)
    {
        foreach (var layerDef in jsonObject.Defs.Layers)
        {
            if (layerDef.Identifier == name)
            {
                return layerDef;
            }
        }
        return null;
    }

    void ReadEntity(EntityInstance entityInstance)
    {
        Vector2I position = new Vector2I((int)entityInstance.Px[0] * tileMult, (int)entityInstance.Px[1] * tileMult); //+ tileSize / 2;
        Console.WriteLine($"read entity at {position}");
        dynamic field;
        switch (entityInstance.Identifier)
        {

            case "Player":
                {
                    EntityPrefabs.CreatePlayer(position.X, position.Y - (tileSize.Y * tileMult + 6));
                    break;
                }
            case "Bunny":
                {

                    break;
                }
            case "Ambush":
                {
                    break;
                }
        }
    }
    dynamic GetFieldFromName(EntityInstance entity, string name)
    {
        foreach (FieldInstance fieldInstance in entity.FieldInstances)
        {
            if (fieldInstance.Identifier == name)
            {
                return fieldInstance.Value;
            }
        }
        Console.WriteLine($"Couldn't find field instance of name {name}");
        return null;
    }
    bool TryGetFieldFromName(EntityInstance entity, string name, out dynamic value)
    {
        foreach (FieldInstance fieldInstance in entity.FieldInstances)
        {
            if (fieldInstance.Identifier == name)
            {
                value = fieldInstance.Value;
                return true;
            }
        }
        value = null;
        return false;
    }
    void PreloadColorsFromDefs()
    {
        foreach (var layerDef in jsonObject.Defs.Layers)
        {
            if (layerDef.Type != "IntGrid")
            {
                continue;
            }
            var colorArr = layerDef.IntGridValues;
            Dictionary<int, Color> idToColor = new Dictionary<int, Color>(colorArr.Length);
            foreach (var entry in colorArr)
            {
                idToColor[(int)entry.Value] = ColorUtils.HexStringToColor(entry.Color.Substring(1));
                Console.WriteLine(idToColor[(int)entry.Value]);
            }
            intGridToColorDict[layerDef.Identifier] = idToColor;
        }

    }
    void CreateColorSpritesFromIntgridLayer(Level level, string name)
    {
        LayerInstance layer = GetLayerInstanceByName(level, name);
        var idToColor = intGridToColorDict[name];
        long[] intGrid = layer.IntGridCsv;
        (int xTiles, int yTiles) = ((int)layer.CWid, (int)layer.CHei);
        int sizeOfTile = (int)layer.GridSize;
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
    void LoadSpriteTilesFromTileLayer(LayerInstance layerInstance) // loads in the tile sprite data from the "Visuals" tile layer
    {
        foreach (var tile in layerInstance.GridTiles)
        {
            Vector2I spritePosOnSheet = new Vector2I((int)tile.Src[0], (int)tile.Src[1]);
            Vector2I tilePos = new Vector2I((int)tile.Px[0] / tileSize.X, (int)tile.Px[1] / tileSize.Y);
        }
    }
}
