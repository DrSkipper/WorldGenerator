using UnityEngine;
using System.Collections.Generic;

public class TerrainManager : MonoBehaviour
{
    public WorldInfo WorldInfo;
    public WorldGenManager WorldGenManager;
    //public TerrainQuadRenderer QuadPrefab;
    public TerrainQuadRenderer MainQuad;
    public TerrainQuadRenderer LeftQuad;
    public TerrainQuadRenderer UpQuad;
    public TerrainQuadRenderer RightQuad;
    public TerrainQuadRenderer DownQuad;

    public TerrainQuadRenderer UpLeftQuad;
    public TerrainQuadRenderer UpRightQuad;
    public TerrainQuadRenderer DownRightQuad;
    public TerrainQuadRenderer DownLeftQuad;

    public IntegerVector SpawnPoint; //TODO: Should be data driven
    public Transform Tracker;
    public int ExtraBounds = 10;

    void Start()
    {
        _halfQuadSize = this.WorldInfo.QuadSize * this.MainQuad.TileRenderSize / 2;
        this.WorldGenManager.AddUpdateDelegate(onWorldGenUpdate);
    }

    public void BeginManagingWorld(IntegerVector startingPos)
    {
        WorldTileInfo me = _world[startingPos.X, startingPos.Y];
        int leftX = startingPos.X > 0 ? startingPos.X - 1 : _world.GetLength(0) - 1;
        int rightX = startingPos.X < _world.GetLength(0) - 1 ? startingPos.X + 1 : 0;
        int downY = startingPos.Y > 0 ? startingPos.Y - 1 : _world.GetLength(1) - 1;
        int upY = startingPos.Y < _world.GetLength(1) - 1 ? startingPos.Y + 1 : 0;

        WorldTileInfo leftNeighbor = _world[leftX, startingPos.Y];
        WorldTileInfo upNeighbor = _world[startingPos.X, upY];
        WorldTileInfo rightNeighbor = _world[rightX, startingPos.Y];
        WorldTileInfo downNeighbor = _world[startingPos.X, downY];

        WorldTileInfo upperLeftNeighbor = _world[leftX, upY];
        WorldTileInfo upperRightNeighbor = _world[rightX, upY];
        WorldTileInfo downRightNeighbor = _world[rightX, downY];
        WorldTileInfo downLeftNeighbor = _world[leftX, downY];

        if (!me.TerrainInitialized)
            me.InitializeTerrain(this.WorldInfo, leftNeighbor, upNeighbor, rightNeighbor, downNeighbor);

        if (!leftNeighbor.TerrainInitialized)
            leftNeighbor.InitializeTerrain(this.WorldInfo, null, null, me, null);
        if (!upNeighbor.TerrainInitialized)
            upNeighbor.InitializeTerrain(this.WorldInfo, null, null, null, me);
        if (!rightNeighbor.TerrainInitialized)
            rightNeighbor.InitializeTerrain(this.WorldInfo, me, null, null, null);
        if (!downNeighbor.TerrainInitialized)
            downNeighbor.InitializeTerrain(this.WorldInfo, null, me, null, null);

        if (!upperLeftNeighbor.TerrainInitialized)
            upperLeftNeighbor.InitializeTerrain(this.WorldInfo, null, null, upNeighbor, leftNeighbor);
        if (!upperRightNeighbor.TerrainInitialized)
            upperRightNeighbor.InitializeTerrain(this.WorldInfo, upNeighbor, null, null, rightNeighbor);
        if (!downRightNeighbor.TerrainInitialized)
            downRightNeighbor.InitializeTerrain(this.WorldInfo, downNeighbor, rightNeighbor, null, null);
        if (!downLeftNeighbor.TerrainInitialized)
            downLeftNeighbor.InitializeTerrain(this.WorldInfo, null, leftNeighbor, downNeighbor, null);

        this.MainQuad.CreateTerrainWithHeightMap(me, leftNeighbor, upNeighbor, rightNeighbor, downNeighbor);
        this.LeftQuad.CreateTerrainWithHeightMap(leftNeighbor, null, null, me, null);
        this.UpQuad.CreateTerrainWithHeightMap(upNeighbor, null, null, null, me);
        this.RightQuad.CreateTerrainWithHeightMap(rightNeighbor, me, null, null, null);
        this.DownQuad.CreateTerrainWithHeightMap(downNeighbor, null, me, null, null);

        this.UpLeftQuad.CreateTerrainWithHeightMap(upperLeftNeighbor, null, null, upNeighbor, leftNeighbor);
        this.UpRightQuad.CreateTerrainWithHeightMap(upperRightNeighbor, upNeighbor, null, null, rightNeighbor);
        this.DownRightQuad.CreateTerrainWithHeightMap(downRightNeighbor, downNeighbor, rightNeighbor, null, null);
        this.DownLeftQuad.CreateTerrainWithHeightMap(downLeftNeighbor, null, leftNeighbor, downNeighbor, null);
    }

    void FixedUpdate()
    {
        if (this.Tracker.position.x < this.MainQuad.transform.position.x - _halfQuadSize - this.ExtraBounds)
        {
            // Center left quad, unload up right, right, and down right - load up left, left and down left
        }
        else if (this.Tracker.position.x > this.MainQuad.transform.position.x + _halfQuadSize + this.ExtraBounds)
        {
            // Center right quad
        }
        else if (this.Tracker.position.z < this.MainQuad.transform.position.z - _halfQuadSize - this.ExtraBounds)
        {
            // Center down quad
        }
        else if (this.Tracker.position.z > this.MainQuad.transform.position.z + _halfQuadSize + this.ExtraBounds)
        {
            // Center up quad
        }
    }

    /**
     * Private
     */
    private WorldTileInfo[,] _world;
    private int _halfQuadSize;

    private void centerLeftQuad()
    {
        // Swap quad references
        TerrainQuadRenderer newUpRight = this.UpQuad;
        TerrainQuadRenderer newRight = this.MainQuad;
        TerrainQuadRenderer newDownRight = this.DownQuad;

        this.UpQuad = this.UpLeftQuad;
        this.MainQuad = this.LeftQuad;
        this.DownQuad = this.DownLeftQuad;
        this.UpLeftQuad = this.UpRightQuad;
        this.LeftQuad = this.RightQuad;
        this.DownLeftQuad = this.DownRightQuad;
        this.UpRightQuad = newUpRight;
        this.RightQuad = newRight;
        this.DownRightQuad = newDownRight;

        // Move left quads into correct position
        assingQuadPositions();

        //TODO: recenter all actors and other objects
        //TODO: Load new data for left quads
    }

    private void onWorldGenUpdate(bool finished)
    {
        if (finished)
        {
            assingQuadPositions();
            createWorldTileData(this.WorldGenManager.Specs, this.WorldGenManager.Layers);
            this.BeginManagingWorld(this.SpawnPoint);
        }
    }

    private void assingQuadPositions()
    {
        int size = _halfQuadSize * 2;
        this.MainQuad.transform.SetPosition(0, 0, 0);
        this.LeftQuad.transform.SetPosition(-size, 0, 0);
        this.UpQuad.transform.SetPosition(0, 0, size);
        this.RightQuad.transform.SetPosition(size, 0, 0);
        this.DownQuad.transform.SetPosition(0, 0, -size);

        this.UpLeftQuad.transform.SetPosition(-size, 0, size);
        this.UpRightQuad.transform.SetPosition(size, 0, size);
        this.DownRightQuad.transform.SetPosition(size, 0, -size);
        this.DownLeftQuad.transform.SetPosition(-size, 0, -size);
    }

    private void createWorldTileData(WorldGenSpecs specs, List<LevelGenMap> layers)
    {
        _world = new WorldTileInfo[specs.MapSize.X, specs.MapSize.Y];
        for (int x = 0; x < specs.MapSize.X; ++x)
        {
            for (int y = 0; y < specs.MapSize.Y; ++y)
            {
                _world[x, y] = new WorldTileInfo(genTileToWorldTileType(layers[0].Grid[x, y]));

                for (int i = 1; i < layers.Count; ++i)
                {
                    WorldTileInfo.TileTrait? trait = genTileToWorldTileTrait(layers[i].Grid[x, y]);
                    if (trait.HasValue)
                        _world[x, y].AddTileTrait(trait.Value);
                }
            }
        }
    }

    private WorldTileInfo.TileType genTileToWorldTileType(LevelGenMap.TileType tile)
    {
        switch (tile)
        {
            default:
            case LevelGenMap.TileType.A:
                return WorldTileInfo.TileType.Plains;
            case LevelGenMap.TileType.B:
                return WorldTileInfo.TileType.Hills;
            case LevelGenMap.TileType.C:
                return WorldTileInfo.TileType.Mountains;
            case LevelGenMap.TileType.D:
                return WorldTileInfo.TileType.Water;
            case LevelGenMap.TileType.E:
                return WorldTileInfo.TileType.Desert;
        }
    }

    private WorldTileInfo.TileTrait? genTileToWorldTileTrait(LevelGenMap.TileType tile)
    {
        switch (tile)
        {

            default:
            case LevelGenMap.TileType.A:
                return null;
            case LevelGenMap.TileType.B:
                return WorldTileInfo.TileTrait.Forest;
            case LevelGenMap.TileType.C:
                return WorldTileInfo.TileTrait.MineralVein;
            case LevelGenMap.TileType.D:
                return WorldTileInfo.TileTrait.City;
        }
    }
}
