using UnityEngine;
using System.Collections.Generic;

public class TerrainManager : MonoBehaviour
{
    public TerrainQuadGenerator QuadPrefab;

    public void BeginManagingWorld(WorldTileInfo[,] world)
    {
        _world = world;
    }

    /**
     * Private
     */
    private WorldTileInfo[,] _world;
}
