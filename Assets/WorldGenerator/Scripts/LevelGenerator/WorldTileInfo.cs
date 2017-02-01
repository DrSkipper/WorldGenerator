using UnityEngine;
using System.Collections.Generic;

public class WorldTileInfo
{
    public enum TileType
    {
        Plains,
        Hills,
        Mountains,
        Water,
        Desert
    }

    public enum TileTrait
    {
        Forest,
        MineralVein,
        City
    }

    public class TerrainInfo
    {
        public int Height;
        //TODO - terrain type - similar to tile types but includes beach and stuff
        //TODO - doodads - trees etc
    }

    public TileType GetTileType() { return (TileType)this.Type; }
    public void SetTileType(TileType type) { this.Type = (int)type; }
    public int NumTraits { get { return this.Traits == null ? 0 : this.Traits.Count; } }
    public bool TerrainInitialized { get { return this.Terrain != null; } }

    public WorldTileInfo(TileType tileType)
    {
        this.SetTileType(tileType);
    }

    public TileTrait GetTileTrait(int index)
    {
        return (TileTrait)this.Traits[index];
    }

    public bool HasTrait(TileTrait trait)
    {
        for (int i = 0; i < this.NumTraits; ++i)
        {
            if (this.GetTileTrait(i) == trait)
                return true;
        }
        return false;
    }

    public void AddTileTrait(TileTrait trait)
    {
        if (this.Traits == null)
            this.Traits = new List<int>();
        if (!this.HasTrait(trait))
            this.Traits.Add((int)trait);
    }

    public void RemoveTileTrait(TileTrait trait)
    {
        for (int i = 0; i < this.NumTraits; ++i)
        {
            if (this.GetTileTrait(i) == trait)
            {
                this.Traits.RemoveAt(i);
                break;
            }
        }
    }

    public void InitializeTerrain(WorldInfo worldInfo, WorldTileInfo leftNeighbor, WorldTileInfo upNeighbor, WorldTileInfo rightNeighbor, WorldTileInfo downNeighbor)
    {
        this.Terrain = new TerrainInfo[worldInfo.QuadSize, worldInfo.QuadSize];
        for (int x = 0; x < worldInfo.QuadSize; ++x)
            for (int y = 0; y < worldInfo.QuadSize; ++y)
                this.Terrain[x, y] = new TerrainInfo();

        int low = worldInfo.SeaLevel;
        int high = worldInfo.SeaLevel;
        switch (this.GetTileType())
        {
            default:
            case TileType.Water:
                break;
            case TileType.Plains:
                low = worldInfo.PlainsHeightRange.X;
                high = worldInfo.PlainsHeightRange.Y;
                break;
            case TileType.Desert:
                low = worldInfo.DesertHeightRange.X;
                high = worldInfo.DesertHeightRange.Y;
                break;
            case TileType.Hills:
                low = worldInfo.HillsHeightRange.X;
                high = worldInfo.HillsHeightRange.Y;
                break;
            case TileType.Mountains:
                low = worldInfo.MountainsHeightRange.X;
                high = worldInfo.MountainsHeightRange.Y;
                break;
        }

        //TODO: handle different methods for distributing height
        randomFill(this.Terrain, low, high);
    }

    /**
     * Data
     */
    public int Type;
    public List<int> Traits;
    public TerrainInfo[,] Terrain;

    /**
     * Private
     */
    private static void randomFill(TerrainInfo[,] heightMap, int low, int high)
    {
        for (int x = 0; x < heightMap.GetLength(0); ++x)
        {
            for (int y = 0; y < heightMap.GetLength(1); ++y)
            {
                heightMap[x, y].Height = Random.Range(low, high + 1);
            }
        }
    }
}
