using UnityEngine;

public class TerrainQuadGenerator : MonoBehaviour
{
    public TerrainQuadRenderer QuadRenderer;
    public int Width;
    public int Height;

    void Start()
    {
        int[,] heightMap = new int[this.Width, this.Height];
        randomFill(heightMap, 0, 2);
        this.QuadRenderer.CreateTerrainWithHeightMap(heightMap);
    }

    private void randomFill(int[,] heightMap, int low, int high)
    {
        for (int x = 0; x < heightMap.GetLength(0); ++x)
        {
            for (int y = 0; y < heightMap.GetLength(1); ++y)
            {
                heightMap[x, y] = Random.Range(low, high + 1);
            }
        }
    }
}
