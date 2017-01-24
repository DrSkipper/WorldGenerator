using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(TileMapRenderer))]
public class TileMapRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TileMapRenderer mapRenderer = (TileMapRenderer)target;

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
