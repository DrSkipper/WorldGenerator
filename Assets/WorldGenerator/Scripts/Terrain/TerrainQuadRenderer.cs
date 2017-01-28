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
        float originX = 0; // this.transform.position.x;
        float originY = 0; // this.transform.position.y;
        float originZ = 0; // this.transform.position.z;

        int numTiles = _width * _height;
        int numTriangles = numTiles * 3 * 2;

        // Generate mesh data
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        int[] triangles = new int[numTriangles * 3]; // Clockwise order of vertices within triangles (for correct render direction)

        float finalZ = originZ + _height * this.TileRenderSize;

        for (int z = 0; z < _height; ++z)
        {
            for (int x = 0; x < _width; ++x)
            {
                int tileIndex = _width * z + x;
                int triangleIndex = tileIndex * 2 * 3;

                // Create 4 verts
                float smallZ = originZ + z * this.TileRenderSize;
                float bigZ = smallZ + this.TileRenderSize;
                Vector3 bottomLeft = new Vector3(originX + x * this.TileRenderSize, originY + heightMap[x, z] * this.TileRenderSize, smallZ);
                Vector3 bottomRight = new Vector3(bottomLeft.x + this.TileRenderSize, originY + heightMap[x, z] * this.TileRenderSize, bottomLeft.z);
                Vector3 topLeft = new Vector3(bottomLeft.x, originY + heightMap[x, z] * this.TileRenderSize, bigZ);
                Vector3 topRight = new Vector3(bottomRight.x, originY + heightMap[x, z] * this.TileRenderSize, topLeft.z);

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

                // Side tris
                int rightNeighborHeight = (x + 1 >= _width ? heightMap[x, z] : heightMap[x + 1, z]) * this.TileRenderSize;
                int upNeighborHeight = (z + 1 >= _height ? heightMap[x, z] : heightMap[x, z + 1]) * this.TileRenderSize;

                // Right side
                Vector3 rightBottomLeft = new Vector3(bottomRight.x, originY + rightNeighborHeight, bottomRight.z);
                Vector3 rightBottomRight = new Vector3(topRight.x, originY + rightNeighborHeight, topRight.z);
                Vector3 rightTopLeft = new Vector3(bottomRight.x, originY + heightMap[x, z] * this.TileRenderSize, bottomRight.z);
                Vector3 rightTopRight = new Vector3(topRight.x, originY + heightMap[x, z] * this.TileRenderSize, topRight.z);
                int rightBottomLeftVert = topRightVert + 1;
                int rightBottomRightVert = topRightVert + 2;
                int rightTopLeftVert = topRightVert + 3;
                int rightTopRightVert = topRightVert + 4;
                triangles[triangleIndex + 6] = rightTopLeftVert;
                triangles[triangleIndex + 7] = rightBottomRightVert;
                triangles[triangleIndex + 8] = rightBottomLeftVert;
                triangles[triangleIndex + 9] = rightTopLeftVert;
                triangles[triangleIndex + 10] = rightTopRightVert;
                triangles[triangleIndex + 11] = rightBottomRightVert;

                // Up side
                Vector3 upBottomLeft = new Vector3(topRight.x, originY + upNeighborHeight, topRight.z);
                Vector3 upBottomRight = new Vector3(topLeft.x, originY + upNeighborHeight, topLeft.z);
                Vector3 upTopLeft = new Vector3(topRight.x, originY + heightMap[x, z] * this.TileRenderSize, topRight.z);
                Vector3 upTopRight = new Vector3(topLeft.x, originY + heightMap[x, z] * this.TileRenderSize, topLeft.z);
                int upBottomLeftVert = rightTopRightVert + 1;
                int upBottomRightVert = rightTopRightVert + 2;
                int upTopLeftVert = rightTopRightVert + 3;
                int upTopRightVert = rightTopRightVert + 4;
                triangles[triangleIndex + 12] = upTopLeftVert;
                triangles[triangleIndex + 13] = upBottomRightVert;
                triangles[triangleIndex + 14] = upBottomLeftVert;
                triangles[triangleIndex + 15] = upTopLeftVert;
                triangles[triangleIndex + 16] = upTopRightVert;
                triangles[triangleIndex + 17] = upBottomRightVert;

                // Handle UVs
                Vector2[] spriteUVs = this.Sprite.uv;
                Vector2 bottomLeftUV = spriteUVs[0];
                Vector2 bottomRightUV = spriteUVs[1];
                Vector2 topLeftUV = spriteUVs[2];
                Vector2 topRightUV = spriteUVs[3];

                // Add vertices and vertex data to mesh data
                vertices.Add(bottomLeft);
                vertices.Add(bottomRight);
                vertices.Add(topLeft);
                vertices.Add(topRight);
                vertices.Add(rightBottomLeft);
                vertices.Add(rightBottomRight);
                vertices.Add(rightTopLeft);
                vertices.Add(rightTopRight);
                vertices.Add(upBottomLeft);
                vertices.Add(upBottomRight);
                vertices.Add(upTopLeft);
                vertices.Add(upTopRight);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                uvs.Add(bottomLeftUV);
                uvs.Add(bottomRightUV);
                uvs.Add(topLeftUV);
                uvs.Add(topRightUV);
                uvs.Add(bottomLeftUV);
                uvs.Add(bottomRightUV);
                uvs.Add(topLeftUV);
                uvs.Add(topRightUV);
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
        this.MeshFilter.mesh = mesh;
    }
}
