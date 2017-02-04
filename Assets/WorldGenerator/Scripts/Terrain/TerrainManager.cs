﻿using UnityEngine;
using UnityEngine.SceneManagement;
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
    public List<GameObject> IgnoreRecenter;

    void Start()
    {
        _halfQuadSize = this.WorldInfo.QuadSize * this.MainQuad.TileRenderSize / 2;
        _recenterObjects = new List<GameObject>();
        this.WorldGenManager.AddUpdateDelegate(onWorldGenUpdate);
    }

    void FixedUpdate()
    {
        if (this.Tracker.position.x < this.MainQuad.transform.position.x - _halfQuadSize - this.ExtraBounds)
        {
            centerLeftQuad();
        }
        else if (this.Tracker.position.x > this.MainQuad.transform.position.x + _halfQuadSize + this.ExtraBounds)
        {
            centerRightQuad();
        }
        else if (this.Tracker.position.z < this.MainQuad.transform.position.z - _halfQuadSize - this.ExtraBounds)
        {
            centerDownQuad();
        }
        else if (this.Tracker.position.z > this.MainQuad.transform.position.z + _halfQuadSize + this.ExtraBounds)
        {
            centerUpQuad();
        }
    }

    /**
     * Private
     */
    private WorldTileInfo[,] _world;
    private int _halfQuadSize;
    List<GameObject> _recenterObjects;
    IntegerVector _center;

    private enum LoadSection
    {
        AllSides,
        Left,
        Up,
        Right,
        Down
    }

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

        _center.X = _center.X > 0 ? _center.X - 1 : _world.GetLength(0) - 1;
        assingQuadPositions(); // Move left quads into correct position
        recenter(_halfQuadSize * 2, 0); // Recenter all actors and other objects
        loadWorld(LoadSection.Left); // Load new data for left quads
    }

    private void centerRightQuad()
    {
        // Swap quad references
        TerrainQuadRenderer newUpLeft = this.UpQuad;
        TerrainQuadRenderer newLeft = this.MainQuad;
        TerrainQuadRenderer newDownLeft = this.DownQuad;
        this.UpQuad = this.UpRightQuad;
        this.MainQuad = this.RightQuad;
        this.DownQuad = this.DownRightQuad;
        this.UpRightQuad = this.UpLeftQuad;
        this.RightQuad = this.LeftQuad;
        this.DownRightQuad = this.DownLeftQuad;
        this.UpLeftQuad = newUpLeft;
        this.LeftQuad = newLeft;
        this.DownLeftQuad = newDownLeft;

        _center.X = _center.X < _world.GetLength(0) - 1 ? _center.X + 1 : 0;
        assingQuadPositions(); // Move left quads into correct position
        recenter(-_halfQuadSize * 2, 0); // Recenter all actors and other objects
        loadWorld(LoadSection.Right); // Load new data for left quads
    }

    private void centerDownQuad()
    {
        // Swap quad references
        TerrainQuadRenderer newUpLeft = this.LeftQuad;
        TerrainQuadRenderer newUp = this.MainQuad;
        TerrainQuadRenderer newUpRight = this.RightQuad;
        this.LeftQuad = this.DownLeftQuad;
        this.MainQuad = this.DownQuad;
        this.RightQuad = this.DownRightQuad;
        this.DownLeftQuad = this.UpLeftQuad;
        this.DownQuad = this.UpQuad;
        this.DownRightQuad = this.UpRightQuad;
        this.UpLeftQuad = newUpLeft;
        this.UpQuad = newUp;
        this.UpRightQuad = newUpRight;

        _center.Y = _center.Y > 0 ? _center.Y - 1 : _world.GetLength(1) - 1;
        assingQuadPositions(); // Move left quads into correct position
        recenter(0, _halfQuadSize * 2); // Recenter all actors and other objects
        loadWorld(LoadSection.Down); // Load new data for left quads
    }

    private void centerUpQuad()
    {
        // Swap quad references
        TerrainQuadRenderer newDownLeft = this.LeftQuad;
        TerrainQuadRenderer newDown = this.MainQuad;
        TerrainQuadRenderer newDownRight = this.RightQuad;
        this.LeftQuad = this.UpLeftQuad;
        this.MainQuad = this.UpQuad;
        this.RightQuad = this.UpRightQuad;
        this.UpLeftQuad = this.DownLeftQuad;
        this.UpQuad = this.DownQuad;
        this.UpRightQuad = this.DownRightQuad;
        this.DownLeftQuad = newDownLeft;
        this.DownQuad = newDown;
        this.DownRightQuad = newDownRight;

        _center.Y = _center.Y < _world.GetLength(1) - 1 ? _center.Y + 1 : 0;
        assingQuadPositions(); // Move left quads into correct position
        recenter(0, -_halfQuadSize * 2); // Recenter all actors and other objects
        loadWorld(LoadSection.Up); // Load new data for left quads
    }

    private void recenter(float offsetX, float offsetZ)
    {
        _recenterObjects.Clear();
        SceneManager.GetActiveScene().GetRootGameObjects(_recenterObjects);
        _recenterObjects.RemoveAllOptimized(this.IgnoreRecenter);
        for (int i = 0; i < _recenterObjects.Count; ++i)
        {
            _recenterObjects[i].transform.AddPosition(offsetX, 0, offsetZ);
        }
    }

    private void onWorldGenUpdate(bool finished)
    {
        if (finished)
        {
            _center = this.SpawnPoint;
            assingQuadPositions();
            createWorldTileData(this.WorldGenManager.Specs, this.WorldGenManager.Layers);
            loadWorld(LoadSection.AllSides);
        }
    }

    private void loadWorld(LoadSection section)
    {
        WorldTileInfo me = _world[_center.X, _center.Y];
        int leftX = _center.X > 0 ? _center.X - 1 : _world.GetLength(0) - 1;
        int rightX = _center.X < _world.GetLength(0) - 1 ? _center.X + 1 : 0;
        int downY = _center.Y > 0 ? _center.Y - 1 : _world.GetLength(1) - 1;
        int upY = _center.Y < _world.GetLength(1) - 1 ? _center.Y + 1 : 0;

        WorldTileInfo leftNeighbor = _world[leftX, _center.Y];
        WorldTileInfo upNeighbor = _world[_center.X, upY];
        WorldTileInfo rightNeighbor = _world[rightX, _center.Y];
        WorldTileInfo downNeighbor = _world[_center.X, downY];

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

        if (section == LoadSection.AllSides)
            this.MainQuad.CreateTerrainWithHeightMap(me, leftNeighbor, upNeighbor, rightNeighbor, downNeighbor);

        if (section == LoadSection.AllSides || section == LoadSection.Left)
            this.LeftQuad.CreateTerrainWithHeightMap(leftNeighbor, null, null, me, null);

        if (section == LoadSection.AllSides || section == LoadSection.Up)
            this.UpQuad.CreateTerrainWithHeightMap(upNeighbor, null, null, null, me);

        if (section == LoadSection.AllSides || section == LoadSection.Right)
            this.RightQuad.CreateTerrainWithHeightMap(rightNeighbor, me, null, null, null);

        if (section == LoadSection.AllSides || section == LoadSection.Down)
            this.DownQuad.CreateTerrainWithHeightMap(downNeighbor, null, me, null, null);

        if (section == LoadSection.AllSides || section == LoadSection.Left || section == LoadSection.Up)
            this.UpLeftQuad.CreateTerrainWithHeightMap(upperLeftNeighbor, null, null, upNeighbor, leftNeighbor);

        if (section == LoadSection.AllSides || section == LoadSection.Right || section == LoadSection.Up)
            this.UpRightQuad.CreateTerrainWithHeightMap(upperRightNeighbor, upNeighbor, null, null, rightNeighbor);

        if (section == LoadSection.AllSides || section == LoadSection.Right || section == LoadSection.Down)
            this.DownRightQuad.CreateTerrainWithHeightMap(downRightNeighbor, downNeighbor, rightNeighbor, null, null);

        if (section == LoadSection.AllSides || section == LoadSection.Left || section == LoadSection.Down)
            this.DownLeftQuad.CreateTerrainWithHeightMap(downLeftNeighbor, null, leftNeighbor, downNeighbor, null);
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
