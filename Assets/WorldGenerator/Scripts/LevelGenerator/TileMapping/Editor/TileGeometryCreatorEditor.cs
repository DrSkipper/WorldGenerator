using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileGeometryCreator))]
public class TileGeometryCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Clear"))
        {
            ((TileGeometryCreator)target).Clear();
        }
    }
}
