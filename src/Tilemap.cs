namespace CustomTilemap;
using System;
using System.Collections;
using System.Collections.Generic;
using MoonTools.ECS;
using MyGame.Components;

public enum TileType {
    None, Wall
}

public class Tilemap : Manipulator
{
    public List<TileType> tiles;
    public List<List<Entity>> tileContents;
    public int xTiles;
    public int yTiles;
    public int xy_id(int x, int y)
    {
        return (y * xTiles) + x;
    }
    public Vector2I id_to_xy(int idx)
    {
        return new Vector2I(idx % xTiles, idx / xTiles);
    }
    public bool IndexInRange(int index) => index >= 0 && index < xTiles * yTiles;
    public bool TileInBounds(Vector2I tile) => tile.X >= 0 && tile.X < xTiles && tile.Y >= 0 && tile.Y < yTiles;

    public static int xy_id(int x, int y, int xTiles)
    {
        return (y * xTiles) + x;
    }
    public Tilemap(World world, int x, int y) : base(world)
    {
        xTiles = x;
        yTiles = y;
        int totalTiles = xTiles * yTiles;
        tiles = new List<TileType>(new TileType[totalTiles]);
        tileContents = new List<List<Entity>>(new List<Entity>[totalTiles]);

        for (int i = 0; i < totalTiles; i++)
        {
            tileContents[i] = new List<Entity>();
        }
    }
    public Tilemap(World world, int x, int y, TileType[] tileList) : this(world, x, y)
    {
        // xTiles = x;
        // yTiles = y;
        // tiles = new List<TileType>();
        // revealedTiles = new List<bool>(new bool[xTiles * yTiles]);
        // visibleTiles = new List<bool>(new bool[xTiles * yTiles]);
        for (int i = 0; i < tileList.Length; i++)
        {
            TileType tile = tileList[i];
            tiles[i] = tile;
        }
        // revealedTiles = new List<bool>(new bool[tileList.Length]);
    }

    // public void PopulateBlocked()
    // {
    //     for (int i = 0; i < tiles.Count; i++)
    //     {
    //         blocked[i] = tiles[i] == TileType.Wall;
    //     }
    // }
    public void ClearTileContents()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            tileContents[i].Clear();
        }
    }

    public bool IsOpaque(int idx)
    {
        return tiles[idx] == TileType.Wall;
    }

    public bool IsBlocked(int idx)
    {
        if(tiles[idx] == TileType.Wall) {
            return true;
        }
        foreach (var entity in tileContents[idx]) {
            if(Has<Solid>(entity)) {
                return true;
            }
        }
        return false;
    }

    public bool IsBlocked(int x, int y)
    {
        if(x < 0 || x >= xTiles || y < 0 || y >= yTiles) return true;
        return IsBlocked(xy_id(x, y));
    }
    public void ChangePosition(Entity entity, int x, int y) {
        if(TryGet<TilePosition>(entity, out TilePosition pos)) {
            tileContents[xy_id(pos.X, pos.Y)].Remove(entity);
        } 
        tileContents[xy_id(x, y)].Add(entity);
        Set(entity, new TilePosition(x, y));
    }
    public void LogTiles()
    {
        Console.WriteLine("tiles:");
        for (int y = 0; y < yTiles; y++)
        {
            string listOfTiles = "";
            for (int x = 0; x < xTiles; x++)
            {
                int index = xy_id(x, y);
                TileType tileType = tiles[index];
                if (tileType == TileType.None)
                {
                    listOfTiles += ".";
                }
                else
                {
                    listOfTiles += "X";
                }
            }
            Console.WriteLine(listOfTiles);
        }
    }

}