using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileMapOutlineRenderer))]
public class TileMapOutlineRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TileMapOutlineRenderer mapRenderer = (TileMapOutlineRenderer)target;

        if (GUILayout.Button("Create Empty Map"))
        {
            mapRenderer.Clear();
            mapRenderer.CreateEmptyMap();
        }

        if (GUILayout.Button("Clear"))
        {
            mapRenderer.Clear();
        }
    }
}
