using UnityEngine;

public class SimpleTerrain : MonoBehaviour
{
    public TileRenderer TileRenderer;

    void Start()
    {
        this.TileRenderer.CreateEmptyMap(50, 50);
    }
}
