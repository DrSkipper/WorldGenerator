using UnityEngine;
using System.Collections.Generic;

public class WorldTileInfo
{
    public enum TileType
    {
        Water = 0,
        Plains = 1,
        Desert = 2,
        Hills = 3,
        Mountains = 4
    }

    public enum TileTrait
    {
        Forest,
        MineralVein,
        City
    }

    public class TerrainInfo
    {
        public enum TerrainType
        {
            Plains,
            Hills,
            Mountains,
            Water,
            Desert,
            Beach
        }

        public TerrainType Type;
        public int Height;
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


        //TODO: Don't always fill with default terrain type, handle borders between regions, beaches around water, oasis tiles, etc
        simpleTypeSet(this.Terrain, this.GetTileType());
        //TODO: Add doodads like trees

        fillHeightMap(this.GetTileType(), this.Terrain, worldInfo);
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
    private static void fillHeightMap(TileType tileType, TerrainInfo[,] heightMap, WorldInfo worldInfo)
    {
        int low = worldInfo.SeaLevel;
        int high = worldInfo.SeaLevel;
        float freq = 1.0f;
        int pow = 1;
        TerrainInfo.TerrainType terrainType = TerrainInfo.TerrainType.Water;

        switch (tileType)
        {
            default:
            case TileType.Water:
                break;
            case TileType.Plains:
                low = worldInfo.PlainsHeightRange.X;
                high = worldInfo.PlainsHeightRange.Y;
                freq = worldInfo.PlainsPerlinFrequency;
                pow = worldInfo.PlainsPerlinPower;
                terrainType = TerrainInfo.TerrainType.Plains;
                break;
            case TileType.Desert:
                low = worldInfo.DesertHeightRange.X;
                high = worldInfo.DesertHeightRange.Y;
                freq = worldInfo.DesertPerlinFrequency;
                pow = worldInfo.DesertPerlinPower;
                terrainType = TerrainInfo.TerrainType.Desert;
                break;
            case TileType.Hills:
                low = worldInfo.HillsHeightRange.X;
                high = worldInfo.HillsHeightRange.Y;
                freq = worldInfo.HillsPerlinFrequency;
                pow = worldInfo.HillsPerlinPower;
                terrainType = TerrainInfo.TerrainType.Hills;
                break;
            case TileType.Mountains:
                low = worldInfo.MountainsHeightRange.X;
                high = worldInfo.MountainsHeightRange.Y;
                freq = worldInfo.MountainsPerlinFrequency;
                pow = worldInfo.MountainsPerlinPower;
                terrainType = TerrainInfo.TerrainType.Mountains;
                break;
        }

        //TODO: handle different methods for distributing height
        perlinFill(heightMap, terrainType, low, high, freq, worldInfo.FrequencyRange, pow);
    }

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

    private static void perlinFill(TerrainInfo[,] heightMap, TerrainInfo.TerrainType affectedType, int low, int high, float freq, float freqRange, int pow)
    {
        float rx = Random.Range(-heightMap.GetLength(0) / 4.0f, heightMap.GetLength(0) / 4.0f);
        float ry = Random.Range(-heightMap.GetLength(1) / 4.0f, heightMap.GetLength(1) / 4.0f);
        bool flipX = Random.value > 0.5f;
        bool flipY = Random.value > 0.5f;
        freq = Random.Range(freq - freq * freqRange, freq + freq * freqRange);

        for (int x = 0; x < heightMap.GetLength(0); ++x)
        {
            for (int y = 0; y < heightMap.GetLength(1); ++y)
            {
                if (heightMap[x, y].Type != affectedType)
                    continue;

                float nx = flipX ? heightMap.GetLength(0) - x : x;
                float ny = flipY ? heightMap.GetLength(1) - y : y;
                float p = Mathf.Pow(Mathf.PerlinNoise(freq * (nx + rx) / (float)heightMap.GetLength(0), freq * (ny + ry) / (float)heightMap.GetLength(1)), pow);
                //if (Random.value < 0.33f) Debug.Log("p = " + p);
                float h = p * (high - low) + low;
                heightMap[x, y].Height = Mathf.RoundToInt(h);
            }
        }
    }

    private static void simpleTypeSet(TerrainInfo[,] heightMap, TileType tileType)
    {
        TerrainInfo.TerrainType type = TerrainInfo.TerrainType.Plains;

        switch (tileType)
        {
            default:
            case TileType.Plains:
                break;
            case TileType.Hills:
                type = TerrainInfo.TerrainType.Hills;
                break;
            case TileType.Mountains:
                type = TerrainInfo.TerrainType.Mountains;
                break;
            case TileType.Water:
                type = TerrainInfo.TerrainType.Water;
                break;
            case TileType.Desert:
                type = TerrainInfo.TerrainType.Desert;
                break;
        }

        for (int x = 0; x < heightMap.GetLength(0); ++x)
        {
            for (int y = 0; y < heightMap.GetLength(1); ++y)
            {
                heightMap[x, y].Type = type;
            }
        }
    }
}
