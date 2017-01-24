using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class TileMapOutlineRenderer : VoBehavior
{
    public enum TilingMethod
    {
        CPU,
        GPU
    }

    public int Width = 100;
    public int Height = 50;
    public int TileRenderSize = 64;
    public bool OffMapIsFilled = true;
    public bool OffsetTilesToCenter = true;
    public Texture2D Atlas = null;

	void Start()
    {
        //this.CreateMap();
    }

    public void CreateEmptyMap()
    {
        this.CreateMapWithGrid(new int[this.Width, this.Height]);
    }

    public void CreateMapWithGrid(int [,] grid)
    {
        this.Clear();
        
        _sprites = this.Atlas.GetSprites();
        this.Width = grid.GetLength(0);
        this.Height = grid.GetLength(1);

        createMapUsingMesh(grid);

        if (this.OffsetTilesToCenter)
        {
            this.transform.position = new Vector3(this.transform.position.x - this.TileRenderSize * this.Width / 2, this.transform.position.y - this.TileRenderSize * this.Height / 2, this.transform.position.z);
        }
    }

    public void Clear()
    {
        _sprites = null;
        this.GetComponent<MeshFilter>().mesh = null;
        
        if (this.OffsetTilesToCenter)
            this.transform.position = new Vector3(0, 0, this.transform.position.z);
    }

    public IntegerVector PositionForTile(int x, int y)
    {
        int halfTileSize = this.TileRenderSize / 2;
        return new IntegerVector(x * this.TileRenderSize + halfTileSize, y * this.TileRenderSize + halfTileSize);
    }


    /**
     * Private
     */
    private Dictionary<string, Sprite> _sprites;

    private void createMapUsingMesh(int[,] grid)
    {
        float originX = this.transform.position.x;
        float originY = this.transform.position.y;
        float originZ = this.transform.position.z;

        int numTiles = this.Width * this.Height;
        int numTriangles = numTiles * 2;
        
        // Generate mesh data
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        int[] triangles = new int[numTriangles * 3]; // Clockwise order of vertices within triangles (for correct render direction)

        for (int y = 0; y < this.Height; ++y)
        {
            for (int x = 0; x < this.Width; ++x)
            {
                int tileIndex = this.Width * y + x;
                int triangleIndex = tileIndex * 2 * 3;

                // Create 4 verts
                Vector3 bottomLeft = new Vector3(originX + x * this.TileRenderSize, originY + y * this.TileRenderSize, originZ);
                Vector3 bottomRight = new Vector3(bottomLeft.x + this.TileRenderSize, bottomLeft.y, originZ);
                Vector3 topLeft = new Vector3(bottomLeft.x, bottomLeft.y + this.TileRenderSize, originZ);
                Vector3 topRight = new Vector3(bottomRight.x, topLeft.y, originZ);

                // Indices of verts
                int bottomLeftVert = vertices.Count;
                int bottomRightVert = bottomLeftVert + 1;
                int topLeftVert = bottomRightVert + 1;
                int topRightVert = topLeftVert + 1;

                // Assign vert indices to triangles
                triangles[triangleIndex] = topLeftVert;
                triangles[triangleIndex + 1] = bottomRightVert;
                triangles[triangleIndex + 2] = bottomLeftVert;

                triangles[triangleIndex + 3] = topLeftVert;
                triangles[triangleIndex + 4] = topRightVert;
                triangles[triangleIndex + 5] = bottomRightVert;

                // Handle UVs
                Vector2[] spriteUVs = _sprites[TilingHelper.GetTileType(TilingHelper.GetNeighbors(grid, x, y, this.OffMapIsFilled))].uv;

                Vector2 bottomLeftUV = spriteUVs[0];
                Vector2 bottomRightUV = spriteUVs[1];
                Vector2 topLeftUV = spriteUVs[2];
                Vector2 topRightUV = spriteUVs[3];

                // Add vertices and vertex data to mesh data
                vertices.Add(bottomLeft);
                vertices.Add(bottomRight);
                vertices.Add(topLeft);
                vertices.Add(topRight);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                uvs.Add(bottomLeftUV);
                uvs.Add(bottomRightUV);
                uvs.Add(topLeftUV);
                uvs.Add(topRightUV);
            }
        }

        // Populate a mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles;

        // Assign mesh to behaviors
        this.GetComponent<MeshFilter>().mesh = mesh;
        this.renderer.material.mainTexture = this.Atlas;
    }
}
