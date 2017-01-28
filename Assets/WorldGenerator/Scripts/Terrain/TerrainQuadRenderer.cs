using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TerrainQuadRenderer : MonoBehaviour
{
    public MeshFilter MeshFilter;
    public int TileRenderSize = 20;
    public Sprite Sprite;

    public void CreateTerrainWithHeightMap(int[,] heightMap)
    {
        this.Clear();
        _width = heightMap.GetLength(0);
        _height = heightMap.GetLength(1);
        createTerrainUsingHeightMap(heightMap);
    }

    public void Clear()
    {
        if (!_cleared)
            this.MeshFilter.mesh = null;
    }

    /**
     * Private
     */
    private int _width;
    private int _height;
    private bool _cleared = true;

    private void createTerrainUsingHeightMap(int[,] heightMap)
    {
        int originX = 0; // this.transform.position.x;
        int originY = 0; // this.transform.position.y;
        int originZ = 0; // this.transform.position.z;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>(); // Clockwise order of vertices within triangles (for correct render direction)
        
        Vector2[] spriteUVs = this.Sprite.uv;
        Vector2 bottomLeftUV = spriteUVs[0];
        Vector2 bottomRightUV = spriteUVs[1];
        Vector2 topLeftUV = spriteUVs[2];
        Vector2 topRightUV = spriteUVs[3];

        // Generate mesh data
        for (int z = 0; z < _height; ++z)
        {
            for (int x = 0; x < _width; ++x)
            {
                // Create 4 verts
                int height = originY + heightMap[x, z] * this.TileRenderSize;
                int smallZ = originZ + z * this.TileRenderSize;
                int bigZ = smallZ + this.TileRenderSize;
                Vector3 bottomLeft = new Vector3(originX + x * this.TileRenderSize, height, smallZ);
                Vector3 bottomRight = new Vector3(bottomLeft.x + this.TileRenderSize, height, bottomLeft.z);
                Vector3 topLeft = new Vector3(bottomLeft.x, height, bigZ);
                Vector3 topRight = new Vector3(bottomRight.x, height, topLeft.z);
                addQuad(bottomLeft, bottomRight, topLeft, topRight, bottomLeftUV, bottomRightUV, topLeftUV, topRightUV, Vector3.up, vertices, normals, uvs, triangles);

                // Side tris
                int rightNeighborHeight = x + 1 >= _width ? height : (originY + heightMap[x + 1, z] * this.TileRenderSize);
                int upNeighborHeight = z + 1 >= _height ? height : (originY + heightMap[x, z + 1] * this.TileRenderSize);
                int leftNeighborHeight = x - 1 < 0 ? height : (originY + heightMap[x - 1, z] * this.TileRenderSize);
                int downNeighborHeight = z - 1 < 0 ? height : (originY + heightMap[x, z - 1] * this.TileRenderSize);

                // Right side
                if (rightNeighborHeight < height)
                {
                    Vector3 rightBottomLeft = new Vector3(bottomRight.x, rightNeighborHeight, bottomRight.z);
                    Vector3 rightBottomRight = new Vector3(topRight.x, rightNeighborHeight, topRight.z);
                    addQuad(rightBottomLeft, rightBottomRight, bottomRight, topRight, bottomLeftUV, bottomRightUV, topLeftUV, topRightUV, Vector3.right, vertices, normals, uvs, triangles);
                }

                // Up side
                if (upNeighborHeight < height)
                {
                    Vector3 upBottomLeft = new Vector3(topRight.x, upNeighborHeight, topRight.z);
                    Vector3 upBottomRight = new Vector3(topLeft.x, upNeighborHeight, topLeft.z);
                    addQuad(upBottomLeft, upBottomRight, topRight, topLeft, bottomLeftUV, bottomRightUV, topLeftUV, topRightUV, Vector3.forward, vertices, normals, uvs, triangles);
                }

                // Left side
                if (leftNeighborHeight < height)
                {
                    Vector3 leftBottomLeft = new Vector3(topLeft.x, leftNeighborHeight, topLeft.z);
                    Vector3 leftBottomRight = new Vector3(bottomLeft.x, leftNeighborHeight, bottomLeft.z);
                    addQuad(leftBottomLeft, leftBottomRight, topLeft, bottomLeft, bottomLeftUV, bottomRightUV, topLeftUV, topRightUV, Vector3.left, vertices, normals, uvs, triangles);
                }

                // Down side
                if (downNeighborHeight < height)
                {
                    Vector3 downBottomLeft = new Vector3(bottomLeft.x, downNeighborHeight, bottomLeft.z);
                    Vector3 downBottomRight = new Vector3(bottomRight.x, downNeighborHeight, bottomRight.z);
                    addQuad(downBottomLeft, downBottomRight, bottomLeft, bottomRight, bottomLeftUV, bottomRightUV, topLeftUV, topRightUV, Vector3.back, vertices, normals, uvs, triangles);
                }
            }
        }

        // Populate a mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        this.MeshFilter.mesh = mesh;
    }

    private void addQuad(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight, Vector2 bottomLeftUV, Vector2 bottomRightUV, Vector2 topLeftUV, Vector2 topRightUV, Vector3 normal, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles)
    {
        // Indices of verts
        int bottomLeftVert = vertices.Count;
        int bottomRightVert = bottomLeftVert + 1;
        int topLeftVert = bottomRightVert + 1;
        int topRightVert = topLeftVert + 1;

        // Assign vert indices to triangles
        triangles.Add(topLeftVert);
        triangles.Add(bottomRightVert);
        triangles.Add(bottomLeftVert);

        triangles.Add(topLeftVert);
        triangles.Add(topRightVert);
        triangles.Add(bottomRightVert);

        // Add vertices and vertex data to mesh data
        vertices.Add(bottomLeft);
        vertices.Add(bottomRight);
        vertices.Add(topLeft);
        vertices.Add(topRight);
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);
        uvs.Add(bottomLeftUV);
        uvs.Add(bottomRightUV);
        uvs.Add(topLeftUV);
        uvs.Add(topRightUV);
    }
}
