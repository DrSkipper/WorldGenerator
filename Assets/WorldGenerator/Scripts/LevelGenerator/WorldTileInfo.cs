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
    }

    public enum TerrainFeature
    {
        Rock,
        Mineral,
        NormalTree,
        PineTree
    }

    public struct FeatureEntry
    {
        //TODO: Id for entry, to make sure don't respawn if destroyed
        public TerrainFeature FeatureType;
        public IntegerVector TilePosition;
    }

    public TileType GetTileType() { return (TileType)this.Type; }
    public void SetTileType(TileType type) { this.Type = (int)type; }
    public int NumTraits { get { return this.Traits == null ? 0 : this.Traits.Count; } }
    public bool TerrainInitialized { get { return this.Terrain != null; } }
    public int NumFeatures { get { return this.Features == null ? 0 : this.Features.Count; } }

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

    public FeatureEntry GetFeature(int index)
    {
        return this.Features[index];
    }

    public void InitializeTerrain(WorldInfo worldInfo, WorldTileInfo leftNeighbor, WorldTileInfo upNeighbor, WorldTileInfo rightNeighbor, WorldTileInfo downNeighbor, List<TerrainInfo.TerrainType> sharedTerrainList, bool heightDebugTerrain = false)
    {
        this.Terrain = new TerrainInfo[worldInfo.QuadSize, worldInfo.QuadSize];
        for (int x = 0; x < worldInfo.QuadSize; ++x)
            for (int y = 0; y < worldInfo.QuadSize; ++y)
                this.Terrain[x, y] = new TerrainInfo();

        //TODO: Allow for oasis tiles and stuff.
        simpleTypeSet(this.Terrain, this.GetTileType());

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

        sharedTerrainList.Clear();
        sharedTerrainList.Add(simpleTileSwap(this.GetTileType()));

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
            }
        }

        //TODO: Allow for things like beaches without hardcoded checks here
        bool beach = false;
        if (this.GetTileType() == TileType.Plains || (this.GetTileType() == TileType.Water && leftBorder || rightBorder || upBorder || downBorder))
        {
            for (int x = 0; x < worldInfo.QuadSize; ++x)
            {
                for (int y = 0; y < worldInfo.QuadSize; ++y)
                {
                    if (this.Terrain[x, y].Type == TerrainInfo.TerrainType.Water)
                    {
                        if ((x > 0 && this.Terrain[x - 1, y].Type == TerrainInfo.TerrainType.Plains) ||
                            (y > 0 && this.Terrain[x, y - 1].Type == TerrainInfo.TerrainType.Plains) ||
                            (x < worldInfo.QuadSize - 1 && this.Terrain[x + 1, y].Type == TerrainInfo.TerrainType.Plains) ||
                            (y < worldInfo.QuadSize - 1 && this.Terrain[x, y + 1].Type == TerrainInfo.TerrainType.Plains) ||
                            (x > 0 && y > 0 && this.Terrain[x - 1, y - 1].Type == TerrainInfo.TerrainType.Plains) ||
                            (x > 0 && y < worldInfo.QuadSize - 1 && this.Terrain[x - 1, y + 1].Type == TerrainInfo.TerrainType.Plains) ||
                            (x < worldInfo.QuadSize - 1 && y > 0 && this.Terrain[x + 1, y - 1].Type == TerrainInfo.TerrainType.Plains) ||
                            (x < worldInfo.QuadSize - 1 && y < worldInfo.QuadSize - 1 && this.Terrain[x + 1, y + 1].Type == TerrainInfo.TerrainType.Plains))
                        {
                            this.Terrain[x, y].Type = TerrainInfo.TerrainType.Beach;
                            beach = true;
                        }
                    }
                }
            }
        }

        if (beach)
            sharedTerrainList.Add(TerrainInfo.TerrainType.Beach);

        for (int i = 0; i < sharedTerrainList.Count; ++i)
        {
            fillHeightMap(sharedTerrainList[i], this.Terrain, worldInfo);
        }

        createFeatureEntries(heightDebugTerrain, leftNeighbor, upNeighbor, rightNeighbor, downNeighbor);
        sharedTerrainList.Clear();

        if (heightDebugTerrain)
            heightDebugTerrainFill(this.Terrain);
    }

    /**
     * Data
     */
    public int Type;
    public List<int> Traits;
    public TerrainInfo[,] Terrain;
    public List<FeatureEntry> Features;

    /**
     * Private
     */
    private const float SQRT_HALF = 0.7071f;

    private void addFeature(TerrainFeature featureType, int x, int y)
    {
        if (this.Features == null)
            this.Features = new List<FeatureEntry>();

        FeatureEntry entry = new FeatureEntry();
        entry.FeatureType = featureType;
        entry.TilePosition = new IntegerVector(x, y);
        this.Features.Add(entry);
    }

    private void createFeatureEntries(bool debugHeight, WorldTileInfo leftNeighbor, WorldTileInfo upNeighbor, WorldTileInfo rightNeighbor, WorldTileInfo downNeighbor)
    {
        //TODO: map features to traits without hardcoding
        //TODO: magic numbers should be world info params
        if (this.HasTrait(TileTrait.Forest))
        {
            bool left = leftNeighbor.HasTrait(TileTrait.Forest);
            bool up = upNeighbor.HasTrait(TileTrait.Forest);
            bool right = rightNeighbor.HasTrait(TileTrait.Forest);
            bool down = downNeighbor.HasTrait(TileTrait.Forest);

            for (int x = 0; x < this.Terrain.GetLength(0); ++x)
            {
                for (int y = 0; y < this.Terrain.GetLength(1); ++y)
                {
                    if (this.Terrain[x, y].Type != TerrainInfo.TerrainType.Water && this.Terrain[x, y].Type != TerrainInfo.TerrainType.Beach && (!debugHeight || this.Terrain[x, y].Height == 2))
                    {
                        float p = Mathf.PerlinNoise(2.0f * x / this.Terrain.GetLength(0), 2.0f * y / this.Terrain.GetLength(1));
                        float r = (0.05f + p * 0.6f) * getFeatureChanceMultiplier(left, up, right, down, (float)x / this.Terrain.GetLength(0), (float)y / this.Terrain.GetLength(1));
                        if (Random.value < r)
                        {
                            addFeature(TerrainFeature.PineTree, x, y);
                        }
                    }
                }
            }
        }
    }

    private static float getFeatureChanceMultiplier(bool leftNeighbor, bool upNeighbor, bool rightNeighbor, bool downNeighbor, float xPercent, float yPercent)
    {
        Vector2 pos = new Vector2(xPercent, yPercent);
        Vector2 mid = new Vector2(0.5f, 0.5f);
        
        {
            if ((!upNeighbor && yPercent > 0.5f) || (!downNeighbor && yPercent < 0.5f))
                return (SQRT_HALF - Vector2.Distance(pos, mid)) / SQRT_HALF;
            else
        }
        else if ((!upNeighbor && yPercent > 0.5f) || (!downNeighbor && yPercent < 0.5f))
        {
        }
        else
        {
            return 1.0f;
        }
    }

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

    private static void heightDebugTerrainFill(TerrainInfo[,] heightMap)
    {
        for (int x = 0; x < heightMap.GetLength(0); ++x)
        {
            for (int y = 0; y < heightMap.GetLength(1); ++y)
            {
                heightMap[x, y].Type = (TerrainInfo.TerrainType)Mathf.Min(heightMap[x, y].Height, 5);
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
