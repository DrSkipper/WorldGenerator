using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TileMapOutlineRenderer))]
[ExecuteInEditMode]
public class TileGeometryCreator : VoBehavior
{
    [System.Serializable]
    public struct TileGeometryPrefabs
    {
        public GameObject Empty;
        public GameObject Filled;
        public GameObject Horizontal;
        public GameObject Vertical;
        public GameObject CornerBottomLeft;
        public GameObject CornerBottomRight;
        public GameObject CornerTopLeft;
        public GameObject CornerTopRight;
        public GameObject TDown;
        public GameObject TLeft;
        public GameObject TUp;
        public GameObject TRight;
        public GameObject Cross;
        public GameObject TipBottom;
        public GameObject TipLeft;
        public GameObject TipTop;
        public GameObject TipRight;
        public GameObject Lone;
    }

    public TileGeometryPrefabs GeometryPrefabs;

    void Awake()
    {
        _tileRenderer = this.GetComponent<TileMapOutlineRenderer>();
        this.Clear();

        if (this.GeometryPrefabs.Empty != null) _geometryPrefabs["empty"] = this.GeometryPrefabs.Empty;
        if (this.GeometryPrefabs.Filled != null) _geometryPrefabs["filled"] = this.GeometryPrefabs.Filled;
        if (this.GeometryPrefabs.Horizontal != null) _geometryPrefabs["side_horizontal"] = this.GeometryPrefabs.Horizontal;
        if (this.GeometryPrefabs.Vertical != null) _geometryPrefabs["side_vertical"] = this.GeometryPrefabs.Vertical;
        if (this.GeometryPrefabs.CornerBottomLeft != null) _geometryPrefabs["corner_top_left"] = this.GeometryPrefabs.CornerBottomLeft;
        if (this.GeometryPrefabs.CornerBottomRight != null) _geometryPrefabs["corner_top_right"] = this.GeometryPrefabs.CornerBottomRight;
        if (this.GeometryPrefabs.CornerTopLeft != null) _geometryPrefabs["corner_bottom_left"] = this.GeometryPrefabs.CornerTopLeft;
        if (this.GeometryPrefabs.CornerTopRight != null) _geometryPrefabs["corner_bottom_right"] = this.GeometryPrefabs.CornerTopRight;
        if (this.GeometryPrefabs.TDown != null) _geometryPrefabs["t_up"] = this.GeometryPrefabs.TDown;
        if (this.GeometryPrefabs.TLeft != null) _geometryPrefabs["t_left"] = this.GeometryPrefabs.TLeft;
        if (this.GeometryPrefabs.TUp != null) _geometryPrefabs["t_down"] = this.GeometryPrefabs.TUp;
        if (this.GeometryPrefabs.TRight != null) _geometryPrefabs["t_right"] = this.GeometryPrefabs.TRight;
        if (this.GeometryPrefabs.Cross != null) _geometryPrefabs["cross"] = this.GeometryPrefabs.Cross;
        if (this.GeometryPrefabs.TipBottom != null) _geometryPrefabs["tip_top"] = this.GeometryPrefabs.TipBottom;
        if (this.GeometryPrefabs.TipLeft != null) _geometryPrefabs["tip_left"] = this.GeometryPrefabs.TipLeft;
        if (this.GeometryPrefabs.TipTop != null) _geometryPrefabs["tip_bottom"] = this.GeometryPrefabs.TipTop;
        if (this.GeometryPrefabs.TipRight != null) _geometryPrefabs["tip_right"] = this.GeometryPrefabs.TipRight;
        if (this.GeometryPrefabs.Lone != null) _geometryPrefabs["lone"] = this.GeometryPrefabs.Lone;
    }

    public void CreateMapWithGrid(int[,] grid)
    {
        if (_tileRenderer == null)
            this.Awake();
        else
            this.Clear();

        for (int x = 0; x < grid.GetLength(0); ++x)
        {
            for (int y = 0; y < grid.GetLength(1); ++y)
            {
                string prefabKey = TilingHelper.GetTileType(TilingHelper.GetNeighbors(grid, x, y, _tileRenderer.OffMapIsFilled));
                if (_geometryPrefabs.ContainsKey(prefabKey))
                {
                    GameObject prefab = _geometryPrefabs[prefabKey];
                    IntegerVector intPosition = _tileRenderer.PositionForTile(x, y);
                    Vector3 position = new Vector3(intPosition.X, intPosition.Y, 0);
                    GameObject geom = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                    geom.transform.parent = this.transform;
                    geom.transform.localPosition = position;
                }
            }
        }
    }

    public void Clear()
    {
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in this.transform)
            toDestroy.Add(child.gameObject);

        if (Application.isPlaying)
        {
            foreach (GameObject child in toDestroy)
                Destroy(child);
        }
        else
        {
            foreach (GameObject child in toDestroy)
                DestroyImmediate(child);
        }
    }

    /**
     * Private
     */
    private TileMapOutlineRenderer _tileRenderer;
    private Dictionary<string, GameObject> _geometryPrefabs = new Dictionary<string, GameObject>();
}
