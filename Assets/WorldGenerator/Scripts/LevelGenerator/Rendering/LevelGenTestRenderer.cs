using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LevelGenTestRenderer : LevelGenBehavior
{
    public GameObject Tiles;

    void Awake()
    {
        this.Manager.AddUpdateDelegate(this.MapWasUpdated);
        _tileMapRender = this.Tiles.GetComponent<TileRenderer>();
        //_tileMapOutlineRender = this.Tiles.GetComponent<TileMapOutlineRenderer>();
    }

    public void MapWasUpdated()
    {
        if (this.Tiles == null)
            return;

        int[,] newTileMap = this.tileTypeMapToSpriteIndexMap();

        if (_oldTileMap == null)
        {
            _oldTileMap = newTileMap;
            _tileMapRender.CreateMapWithGrid(newTileMap);
        }
        else
        {
            List<int> changedX = new List<int>();
            List<int> changedY = new List<int>();
            List<int> spriteIndices = new List<int>();

            for (int x = 0; x < this.Map.Width; ++x)
            {
                for (int y = 0; y < this.Map.Height; ++y)
                {
                    if (_oldTileMap[x, y] != newTileMap[x, y])
                    {
                        _oldTileMap[x, y] = newTileMap[x, y];
                        changedX.Add(x);
                        changedY.Add(y);
                        spriteIndices.Add(newTileMap[x, y]);
                    }
                }
            }

            _tileMapRender.SetSpriteIndicesForTiles(changedX.ToArray(), changedY.ToArray(), spriteIndices.ToArray());
        }

        if (this.Manager.Finished)
        {
            _oldTileMap = null;
            //_tileMapRender.Clear();
            //_tileMapOutlineRender.CreateMapWithGrid(newTileMap);
        }
    }

    /**
     * Private
     */
    private int[,] _oldTileMap;
    private TileRenderer _tileMapRender;
    //private TileMapOutlineRenderer _tileMapOutlineRender;

    private int[,] tileTypeMapToSpriteIndexMap()
    {
        LevelGenMap.TileType[,] grid = this.Map.Grid;
        int[,] spriteIndices = new int[this.Map.Width, this.Map.Height];

        for (int x = 0; x < this.Map.Width; ++x)
        {
            for (int y = 0; y < this.Map.Height; ++y)
            {
                spriteIndices[x, y] = tileSetIndexForTile(grid[x, y]);
            }
        }

        return spriteIndices;
    }

    //TODO - fcole - Data-drive this
    private int tileSetIndexForTile(LevelGenMap.TileType tile)
    {
        switch (tile)
        {
            default:
            case LevelGenMap.TileType.A:
                return 1;
            case LevelGenMap.TileType.B:
                return 0;
            case LevelGenMap.TileType.C:
                return 2;
            case LevelGenMap.TileType.D:
                return 3;
            case LevelGenMap.TileType.E:
                return 4;
            case LevelGenMap.TileType.F:
                return 5;
        }
    }
}
