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

    public void InitializeTerrain(WorldInfo worldInfo, WorldTileInfo leftNeighbor, WorldTileInfo upNeighbor, WorldTileInfo rightNeighbor, WorldTileInfo downNeighbor, List<TerrainInfo.TerrainType> sharedTerrainList)
    {
        this.Terrain = new TerrainInfo[worldInfo.QuadSize, worldInfo.QuadSize];
        for (int x = 0; x < worldInfo.QuadSize; ++x)
            for (int y = 0; y < worldInfo.QuadSize; ++y)
                this.Terrain[x, y] = new TerrainInfo();

        //TODO: Allow for oasis tiles and stuff.
        simpleTypeSet(this.Terrain, this.GetTileType());
        //TODO: Add doodads like trees

        bool leftBorder = leftNeighbor.Type < this.Type;
        bool upBorder = upNeighbor.Type < this.Type;
        bool rightBorder = rightNeighbor.Type < this.Type;
        bool downBorder = downNeighbor.Type < this.Type;

        int borderIndent = Mathf.RoundToInt(worldInfo.BorderPercentage * worldInfo.QuadSize + .05f) - worldInfo.BorderRange / 2;
        int farBorderIndent = worldInfo.QuadSize - 1 - borderIndent;

        int mainLeftX = leftBorder ? borderIndent + Random.Range(0, worldInfo.BorderRange) : 0;
        int mainUpY = upBorder ? farBorderIndent - Random.Range(0, worldInfo.BorderRange) : worldInfo.QuadSize - 1;
        int mainRightX = rightBorder ? farBorderIndent - Random.Range(0, worldInfo.BorderRange) : worldInfo.QuadSize - 1;
        int mainDownY = downBorder ? borderIndent + Random.Range(0, worldInfo.BorderRange) : 0;
        
        //TODO: Methodical randomness in border line
        sharedTerrainList.Clear();
        sharedTerrainList.Add(simpleTileSwap(this.GetTileType()));
        bool beach = false;

        if (leftBorder)
        {
            TerrainInfo.TerrainType terrain = simpleTileSwap(leftNeighbor.GetTileType());
            sharedTerrainList.Add(terrain);

            // Handle corner artifacts
            int startY = downBorder && leftNeighbor.Type <= downNeighbor.Type && Random.value >= worldInfo.KeepCornerChance ? 0 : mainDownY;
            int endY = upBorder && leftNeighbor.Type <= upNeighbor.Type && Random.value >= worldInfo.KeepCornerChance ? worldInfo.QuadSize - 1 : mainUpY;

            for (int y = startY; y <= endY; ++y)
            {
                // Allow for uneven borders
                int endX = y == startY || y == endY ? mainLeftX : borderIndent + Mathf.RoundToInt(Mathf.PerlinNoise(worldInfo.BorderFrequency * mainLeftX / worldInfo.QuadSize, worldInfo.BorderFrequency * y / worldInfo.QuadSize) * worldInfo.BorderRange);

                for (int x = 0; x <= endX; ++x)
                {
                    this.Terrain[x, y].Type = terrain;
                }

                //TODO: Allow for things like beaches without hardcoded checks here
                if (leftNeighbor.GetTileType() == TileType.Water && this.GetTileType() == TileType.Plains)
                {
                    beach = true;
                    this.Terrain[endX, y].Type = TerrainInfo.TerrainType.Beach;
                }
            }
        }

        if (rightBorder)
        {
            TerrainInfo.TerrainType terrain = simpleTileSwap(rightNeighbor.GetTileType());
            if (!sharedTerrainList.Contains(terrain))
                sharedTerrainList.Add(terrain);

            int startY = downBorder && rightNeighbor.Type <= downNeighbor.Type && Random.value >= worldInfo.KeepCornerChance ? 0 : mainDownY;
            int endY = upBorder && rightNeighbor.Type <= upNeighbor.Type && Random.value >= worldInfo.KeepCornerChance ? worldInfo.QuadSize - 1 : mainUpY;

            for (int y = startY; y <= endY; ++y)
            {
                int startX = y == startY || y == endY ? mainRightX : farBorderIndent - Mathf.RoundToInt(Mathf.PerlinNoise(worldInfo.BorderFrequency * mainRightX / worldInfo.QuadSize, worldInfo.BorderFrequency * y / worldInfo.QuadSize) * worldInfo.BorderRange);

                for (int x = startX; x < worldInfo.QuadSize; ++x)
                {
                    this.Terrain[x, y].Type = terrain;
                }
                
                if (rightNeighbor.GetTileType() == TileType.Water && this.GetTileType() == TileType.Plains)
                {
                    beach = true;
                    this.Terrain[startX, y].Type = TerrainInfo.TerrainType.Beach;
                }
            }
        }

        if (upBorder)
        {
            TerrainInfo.TerrainType terrain = simpleTileSwap(upNeighbor.GetTileType());
            if (!sharedTerrainList.Contains(terrain))
                sharedTerrainList.Add(terrain);

            int startX = leftBorder && upNeighbor.Type < leftNeighbor.Type && Random.value >= worldInfo.KeepCornerChance ? 0 : mainLeftX;
            int endX = rightBorder && upNeighbor.Type < rightNeighbor.Type && Random.value >= worldInfo.KeepCornerChance ? worldInfo.QuadSize - 1 : mainRightX;

            for (int x = startX; x <= endX; ++x)
            {
                int startY = x == startX || x == endX ? mainUpY : farBorderIndent - Mathf.RoundToInt(Mathf.PerlinNoise(worldInfo.BorderFrequency * x / worldInfo.QuadSize, worldInfo.BorderFrequency * mainUpY / worldInfo.QuadSize) * worldInfo.BorderRange);

                for (int y = startY; y < worldInfo.QuadSize; ++y)
                {
                    this.Terrain[x, y].Type = terrain;
                }

                if (upNeighbor.GetTileType() == TileType.Water && this.GetTileType() == TileType.Plains)
                {
                    beach = true;
                    this.Terrain[x, startY].Type = TerrainInfo.TerrainType.Beach;
                }
            }
        }

        if (downBorder)
        {
            TerrainInfo.TerrainType terrain = simpleTileSwap(downNeighbor.GetTileType());
            if (!sharedTerrainList.Contains(terrain))
                sharedTerrainList.Add(terrain);

            int startX = leftBorder && downNeighbor.Type < leftNeighbor.Type && Random.value >= worldInfo.KeepCornerChance ? 0 : mainLeftX;
            int endX = rightBorder && downNeighbor.Type < rightNeighbor.Type && Random.value >= worldInfo.KeepCornerChance ? worldInfo.QuadSize - 1 : mainRightX;

            for (int x = startX; x <= endX; ++x)
            {
                int endY = x == startX || x == endX ? mainDownY : borderIndent + Mathf.RoundToInt(Mathf.PerlinNoise(worldInfo.BorderFrequency * x / worldInfo.QuadSize, worldInfo.BorderFrequency * mainDownY / worldInfo.QuadSize) * worldInfo.BorderRange);

                for (int y = 0; y <= endY; ++y)
                {
                    this.Terrain[x, y].Type = terrain;
                }

                if (downNeighbor.GetTileType() == TileType.Water && this.GetTileType() == TileType.Plains)
                {
                    beach = true;
                    this.Terrain[x, endY].Type = TerrainInfo.TerrainType.Beach;
                }
            }
        }

        if (beach)
            sharedTerrainList.Add(TerrainInfo.TerrainType.Beach);

        for (int i = 0; i < sharedTerrainList.Count; ++i)
        {
            fillHeightMap(sharedTerrainList[i], this.Terrain, worldInfo);
        }
        sharedTerrainList.Clear();

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
    private static void fillHeightMap(TerrainInfo.TerrainType terrainType, TerrainInfo[,] heightMap, WorldInfo worldInfo)
    {
        //TODO: handle different methods for distributing height
        int low, high, pow;
        float freq;
        gatherFillInformation(terrainType, out low, out high, out freq, out pow, worldInfo);
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

    private static void gatherFillInformation(TerrainInfo.TerrainType terrainType, out int low, out int high, out float freq, out int pow, WorldInfo worldInfo)
    {
        switch (terrainType)
        {
            default:
            case TerrainInfo.TerrainType.Water:
                low = worldInfo.SeaLevel;
                high = worldInfo.SeaLevel;
                freq = 1.0f;
                pow = 1;
                break;
            case TerrainInfo.TerrainType.Beach:
                low = worldInfo.BeachHeightRange.X;
                high = worldInfo.BeachHeightRange.Y;
                freq = worldInfo.BeachPerlinFrequency;
                pow = worldInfo.BeachPerlinPower;
                break;
            case TerrainInfo.TerrainType.Plains:
                low = worldInfo.PlainsHeightRange.X;
                high = worldInfo.PlainsHeightRange.Y;
                freq = worldInfo.PlainsPerlinFrequency;
                pow = worldInfo.PlainsPerlinPower;
                break;
            case TerrainInfo.TerrainType.Desert:
                low = worldInfo.DesertHeightRange.X;
                high = worldInfo.DesertHeightRange.Y;
                freq = worldInfo.DesertPerlinFrequency;
                pow = worldInfo.DesertPerlinPower;
                break;
            case TerrainInfo.TerrainType.Hills:
                low = worldInfo.HillsHeightRange.X;
                high = worldInfo.HillsHeightRange.Y;
                freq = worldInfo.HillsPerlinFrequency;
                pow = worldInfo.HillsPerlinPower;
                break;
            case TerrainInfo.TerrainType.Mountains:
                low = worldInfo.MountainsHeightRange.X;
                high = worldInfo.MountainsHeightRange.Y;
                freq = worldInfo.MountainsPerlinFrequency;
                pow = worldInfo.MountainsPerlinPower;
                break;
        }
    }

    private static void simpleTypeSet(TerrainInfo[,] heightMap, TileType tileType)
    {
        TerrainInfo.TerrainType type = simpleTileSwap(tileType);

        for (int x = 0; x < heightMap.GetLength(0); ++x)
        {
            for (int y = 0; y < heightMap.GetLength(1); ++y)
            {
                heightMap[x, y].Type = type;
            }
        }
    }

    private static TerrainInfo.TerrainType simpleTileSwap(TileType tileType)
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
        return type;
    }
}
