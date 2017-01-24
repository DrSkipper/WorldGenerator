using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(LevelGenMap))]
public class LevelGenMapEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

        LevelGenMap t = (LevelGenMap)target;
		if (t.Grid != null)
            EditorGUILayout.IntField("Map Size", t.Grid.Length);
		else
			EditorGUILayout.IntField("Map Size", 0);

		if (GUILayout.Button("Reset"))
		{
			t.Reset();
		}
	}
}
