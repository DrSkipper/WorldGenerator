using UnityEngine;
using System.Collections.Generic;

public class LevelGenTestRenderer : MonoBehaviour
{
    public GameObject Tiles;
    public LevelGenMap Map;

    void Awake()
    {
        _tileMapRender = this.Tiles.GetComponent<TileRenderer>();
    }

    public void MapWasUpdated(bool finished)
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

        if (finished)
        {
            _oldTileMap = null;
        }
    }

    /**
     * Private
     */
    private int[,] _oldTileMap;
    private TileRenderer _tileMapRender;

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
                return 0;
            case LevelGenMap.TileType.B:
                return 1;
            case LevelGenMap.TileType.C:
                return 2;
            case LevelGenMap.TileType.D:
                return 3;
            case LevelGenMap.TileType.E:
                return 4;
            case LevelGenMap.TileType.F:
                return 5;
            case LevelGenMap.TileType.G:
                return 6;
            case LevelGenMap.TileType.H:
                return 7;
            case LevelGenMap.TileType.INTERNAL_A:
                return 8;
            case LevelGenMap.TileType.INTERNAL_B:
                return 9;
        }
    }
}
