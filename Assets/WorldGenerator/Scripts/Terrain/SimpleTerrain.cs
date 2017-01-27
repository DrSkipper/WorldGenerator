using UnityEngine;

public class SimpleTerrain : MonoBehaviour
{
    public TileRenderer TileRenderer;

    void Start()
    {
        this.TileRenderer.CreateEmptyMap(50, 50);

        Vector3[] v = this.TileRenderer.MeshFilter.mesh.vertices;

        for (int i = 0; i < v.Length; ++i)
        {
            v[i] = new Vector3(v[i].x, v[i].y, v[i].z - (int)(i / 50));
        }

        this.TileRenderer.MeshFilter.mesh.vertices = v;
    }
}
