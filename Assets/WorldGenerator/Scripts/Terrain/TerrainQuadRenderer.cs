using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TerrainQuadRenderer : MonoBehaviour
{
    public MeshFilter MeshFilter;
    public int TileRenderSize = 20;
    public Texture2D Atlas;

    public void CreateTerrainWithHeightMap(WorldTileInfo thisTile, WorldTileInfo leftNeighbor, WorldTileInfo upNeighbor, WorldTileInfo rightNeighbor, WorldTileInfo downNeighbor)
    {
        this.Clear();
        if (_spriteUVs == null)
        {
            Sprite[] sprites = this.Atlas.GetSpritesArray();
            _spriteUVs = new Vector2[sprites.Length][];
            for (int i = 0; i < sprites.Length; ++i)
            {
                _spriteUVs[i] = sprites[i].uv; //NOTE: May have to switch to GetUVs if previously encountered bug with .uv appears
            }
        }
        WorldTileInfo.TerrainInfo[,] terrain = thisTile.Terrain;
        _width = terrain.GetLength(0);
        _height = terrain.GetLength(1);
        _leftNeighbor = leftNeighbor;
        _upNeighbor = upNeighbor;
        _rightNeighbor = rightNeighbor;
        _downNeighbor = downNeighbor;
        createTerrainUsingHeightMap(terrain);
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
    private WorldTileInfo _leftNeighbor;
    private WorldTileInfo _upNeighbor;
    private WorldTileInfo _rightNeighbor;
    private WorldTileInfo _downNeighbor;
    private Vector2[][] _spriteUVs;

    private void createTerrainUsingHeightMap(WorldTileInfo.TerrainInfo[,] terrain)
    {
        int originX = 0; // this.transform.position.x;
        int originY = 0; // this.transform.position.y;
        int originZ = 0; // this.transform.position.z;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>(); // Clockwise order of vertices within triangles (for correct render direction)

        // Generate mesh data
        for (int z = 0; z < _height; ++z)
        {
            for (int x = 0; x < _width; ++x)
            {
                // Get UVs
                Vector2[] spriteUVs = _spriteUVs[(int)terrain[x, z].Type];
                Vector2 bottomLeftUV = spriteUVs[0];
                Vector2 bottomRightUV = spriteUVs[1];
                Vector2 topLeftUV = spriteUVs[2];
                Vector2 topRightUV = spriteUVs[3];

                // Create 4 verts
                int height = originY + terrain[x, z].Height * this.TileRenderSize;
                int smallZ = originZ + z * this.TileRenderSize;
                int bigZ = smallZ + this.TileRenderSize;
                Vector3 bottomLeft = new Vector3(originX + x * this.TileRenderSize, height, smallZ);
                Vector3 bottomRight = new Vector3(bottomLeft.x + this.TileRenderSize, height, bottomLeft.z);
                Vector3 topLeft = new Vector3(bottomLeft.x, height, bigZ);
                Vector3 topRight = new Vector3(bottomRight.x, height, topLeft.z);
                addQuad(bottomLeft, bottomRight, topLeft, topRight, bottomLeftUV, bottomRightUV, topLeftUV, topRightUV, Vector3.up, vertices, normals, uvs, triangles);

                // Side tris
                int rightNeighborHeight = originY + getRightNeighborHeight(terrain, x, z);
                int upNeighborHeight = originY + getUpNeighborHeight(terrain, x, z);
                int leftNeighborHeight = originY + getLeftNeighborHeight(terrain, x, z);
                int downNeighborHeight = originY + getDownNeighborHeight(terrain, x, z);

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

    private int getLeftNeighborHeight(WorldTileInfo.TerrainInfo[,] terrain, int x, int y)
    {
        int heightData = 0;
        if (x - 1 < 0)
        {
            if (_leftNeighbor != null && _leftNeighbor.TerrainInitialized)
                heightData = _leftNeighbor.Terrain[_width - 1, y].Height;
        }
        else
        {
            heightData = terrain[x - 1, y].Height;
        }

        return heightData * this.TileRenderSize;
    }

    private int getUpNeighborHeight(WorldTileInfo.TerrainInfo[,] terrain, int x, int y)
    {
        int heightData = 0;
        if (y + 1 >= _height)
        {
            if (_upNeighbor != null && _upNeighbor.TerrainInitialized)
                heightData = _upNeighbor.Terrain[x, 0].Height;
        }
        else
        {
            heightData = terrain[x, y + 1].Height;
        }

        return heightData * this.TileRenderSize;
    }

    private int getRightNeighborHeight(WorldTileInfo.TerrainInfo[,] terrain, int x, int y)
    {
        int heightData = 0;
        if (x + 1 >= _width)
        {
            if (_rightNeighbor != null && _rightNeighbor.TerrainInitialized)
                heightData = _rightNeighbor.Terrain[0, y].Height;
        }
        else
        {
            heightData = terrain[x + 1, y].Height;
        }

        return heightData * this.TileRenderSize;
    }

    private int getDownNeighborHeight(WorldTileInfo.TerrainInfo[,] terrain, int x, int y)
    {
        int heightData = 0;
        if (y - 1 < 0)
        {
            if (_downNeighbor != null && _downNeighbor.TerrainInitialized)
                heightData = _downNeighbor.Terrain[x, _height - 1].Height;
        }
        else
        {
            heightData = terrain[x, y - 1].Height;
        }

        return heightData * this.TileRenderSize;
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
