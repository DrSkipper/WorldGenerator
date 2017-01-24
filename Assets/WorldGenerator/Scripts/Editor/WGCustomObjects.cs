using UnityEngine;
using UnityEditor;

public class WGCustomObjects : MonoBehaviour
{
    private const string PATH = "Assets/WorldGenerator/";
    private const string LEVELGENINPUT_PATH = "LevelGenInputs/NewLevelGenInput.asset";

    [MenuItem("Custom Objects/Create Level Gen Input")]
    public static void CreateLevelGenInupt()
    {
        SaveAsset(new LevelGenInput(), PATH + LEVELGENINPUT_PATH);
    }

    public static void SaveAsset(ScriptableObject asset, string path)
    {
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
