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

    void Start()
    {
        this.WorldGenManager.AddUpdateDelegate(onWorldGenUpdate);
    }

    public void BeginManagingWorld(IntegerVector startingPos)
    {
        WorldTileInfo me = _world[startingPos.X, startingPos.Y];
        WorldTileInfo leftNeighbor = startingPos.X > 0 ? _world[startingPos.X - 1, startingPos.Y] : _world[_world.GetLength(0) - 1, startingPos.Y];
        WorldTileInfo upNeighbor = startingPos.Y < _world.GetLength(1) - 1 ? _world[startingPos.X, startingPos.Y + 1] : _world[startingPos.X, 0];
        WorldTileInfo rightNeighbor = startingPos.X < _world.GetLength(0) - 1 ? _world[startingPos.X + 1, startingPos.Y] : _world[0, startingPos.Y];
        WorldTileInfo downNeighbor = startingPos.Y > 0 ? _world[startingPos.X, startingPos.Y - 1] : _world[startingPos.X, _world.GetLength(1) - 1];

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

        this.MainQuad.CreateTerrainWithHeightMap(me, leftNeighbor, upNeighbor, rightNeighbor, downNeighbor);
        this.LeftQuad.CreateTerrainWithHeightMap(leftNeighbor, null, null, me, null);
        this.UpQuad.CreateTerrainWithHeightMap(upNeighbor, null, null, null, me);
        this.RightQuad.CreateTerrainWithHeightMap(rightNeighbor, me, null, null, null);
        this.DownQuad.CreateTerrainWithHeightMap(downNeighbor, null, me, null, null);
    }

    /**
     * Private
     */
    private WorldTileInfo[,] _world;

    private void onWorldGenUpdate(bool finished)
    {
        if (finished)
        {
            this.MainQuad.transform.SetPosition(0, 0, 0);
            this.LeftQuad.transform.SetPosition(-this.WorldInfo.QuadSize * this.LeftQuad.TileRenderSize, 0, 0);
            this.UpQuad.transform.SetPosition(0, 0, this.WorldInfo.QuadSize * this.UpQuad.TileRenderSize);
            this.RightQuad.transform.SetPosition(this.WorldInfo.QuadSize * this.RightQuad.TileRenderSize, 0, 0);
            this.DownQuad.transform.SetPosition(0, 0, -this.WorldInfo.QuadSize * this.DownQuad.TileRenderSize);

            createWorldTileData(this.WorldGenManager.Specs, this.WorldGenManager.Layers);
            this.BeginManagingWorld(IntegerVector.Zero);
        }
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
