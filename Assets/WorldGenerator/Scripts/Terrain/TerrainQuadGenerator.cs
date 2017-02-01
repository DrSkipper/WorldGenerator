using UnityEngine;

public class TerrainQuadGenerator : MonoBehaviour
{
    public TerrainQuadRenderer QuadRenderer;
    public int Width;
    public int Height;

    void Start()
    {
        //int[,] heightMap = new int[this.Width, this.Height];
        //randomFill(heightMap, 0, 2);
        //this.QuadRenderer.CreateTerrainWithHeightMap(heightMap);
    }
}
