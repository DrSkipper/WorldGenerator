using UnityEngine;
using System.Collections;

public class MiniMapCursor : VoBehavior
{
    public Transform BaseTransform;
    public TerrainManager Manager;
    public int TileRenderSize;

    void Update()
    {
        this.transform.SetPosition2D(this.BaseTransform.position.x + this.Manager.CurrentCenter.X * this.TileRenderSize - this.TileRenderSize / 2, this.BaseTransform.position.y + this.Manager.CurrentCenter.Y * this.TileRenderSize - this.TileRenderSize / 2);
    }
}
