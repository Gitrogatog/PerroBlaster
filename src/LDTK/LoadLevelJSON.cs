using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using ldtk;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Content;
using MyGame.Spawn;
public class LoadLevelJSON
{
    MoonTools.ECS.World World;
    // LDTKJsonObject jsonObject;
    LdtkJson jsonObject;
    Vector2I tileSize;
    int tileMult;
    Dictionary<(int, int), (int, int)> tileSliceDict = new Dictionary<(int, int), (int, int)>(300);

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
        tileSliceDict.Clear();
        LayerInstance layerInstance = GetLayerInstanceByName(level, "IntGrid");
        long[] intGrid = layerInstance.IntGridCsv;
        (int xTiles, int yTiles) = ((int)layerInstance.CWid, (int)layerInstance.CHei);
        layerInstance = GetLayerInstanceByName(level, "AutoLayer");
        foreach (var tile in layerInstance.AutoLayerTiles)
        {
            (int x, int y) = ((int)tile.Px[0], (int)tile.Px[1]); // position of tile in the ldtk world grid
            (int spriteX, int spriteY) = ((int)tile.Src[0], (int)tile.Src[1]);
            tileSliceDict[(x, y)] = (spriteX, spriteY);
            // EntityPrefabs.CreateTile(x * tileMult, y * tileMult);
        }
        layerInstance = GetLayerInstanceByName(level, "Ground");
        foreach (var tile in layerInstance.GridTiles)
        {
            (int x, int y) = ((int)tile.Px[0], (int)tile.Px[1]); // position of tile in the ldtk world grid
            (int spriteX, int spriteY) = ((int)tile.Src[0], (int)tile.Src[1]);
            tileSliceDict[(x, y)] = (spriteX, spriteY);
        }
        Sprite tileGridSprite = SpriteAnimations.Tiles.Frames[0];
        foreach ((int worldX, int worldY) in tileSliceDict.Keys)
        {
            (int spriteX, int spriteY) = tileSliceDict[(worldX, worldY)];
            int gridIndex = (worldY / tileSize.Y * xTiles) + worldX / tileSize.X;
            if (gridIndex >= 0 && gridIndex < intGrid.Length)
            {
                TileType tileType = (int)intGrid[gridIndex] switch
                {
                    1 => TileType.Solid,
                    2 => TileType.Spike,
                    3 => TileType.Throwable,
                    4 => TileType.Invisible,
                    _ => TileType.Fake
                };
                Sprite tileSprite = tileGridSprite.Slice(spriteX, spriteY, tileSize.X, tileSize.Y);
                EntityPrefabs.CreateTile(worldX * tileMult, worldY * tileMult, tileSprite, tileType);
            }
        }
        // LoadSpriteTilesFromTileLayer(layerInstance);
        // LoadTilesFromIntGrid(layerInstance, new Vector2I((int)level.WorldX, (int)level.WorldY) / tileSize);

        layerInstance = GetLayerInstanceByName(level, "Entities");
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

    void ReadEntity(EntityInstance entityInstance)
    {
        Vector2I position = new Vector2I((int)entityInstance.Px[0] * tileMult, (int)entityInstance.Px[1] * tileMult); //+ tileSize / 2;
        Console.WriteLine($"read entity at {position}");
        switch (entityInstance.Identifier)
        {
            case "Player":
                {
                    EntityPrefabs.CreatePlayer(position.X, position.Y - (tileSize.Y * tileMult + 6));
                    break;
                }
            case "Bunny":
                {
                    Cardinal direction = ConvertEnum.ToCardinal(entityInstance.FieldInstances[0].Value);
                    EntityPrefabs.CreateBunny(position.X, position.Y + 2, direction);
                    break;
                }
        }
    }
    void LoadSpriteTilesFromTileLayer(LayerInstance layerInstance) // loads in the tile sprite data from the "Visuals" tile layer
    {
        foreach (var tile in layerInstance.GridTiles)
        {
            Vector2I spritePosOnSheet = new Vector2I((int)tile.Src[0], (int)tile.Src[1]);
            Vector2I tilePos = new Vector2I((int)tile.Px[0] / tileSize.X, (int)tile.Px[1] / tileSize.Y);
            // int tileID = tilemap.GetIndex((int)tile.Px[0] / tileSize.X, (int)tile.Px[1] / tileSize.Y);
            // tilemap.Sprites[tileID] = spritePosOnSheet;
            // tilemap.Tiles[tileID] = TileType.Empty;

            // load tile colliders from tile layer
            // int id = (int)tile.T;
            // Vector2I atlasTilePos = tileIDToSlopeTile[id];
            // gdTilemap.SetCell(0, tilePos, slopeTileSourceID, atlasTilePos);

        }
    }
}
