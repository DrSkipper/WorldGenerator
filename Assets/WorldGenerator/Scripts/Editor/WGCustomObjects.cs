using UnityEngine;
using UnityEditor;

public class WGCustomObjects : MonoBehaviour
{
    private const string PATH = "Assets/WorldGenerator/CustomObjects/";
    private const string LEVELGENINPUT_PATH = "LevelGenInputs/NewLevelGenInput.asset";
    private const string WORLDGENSPECS_PATH = "WorldGenSpecs/NewWorldGenSpecs.asset";
    private const string WORLDINFO_PATH = "WorldInfo/NewWorldInfo.asset";

    [MenuItem("Custom Objects/Create Level Gen Input")]
    public static void CreateLevelGenInupt()
    {
        SaveAsset(new LevelGenInput(), PATH + LEVELGENINPUT_PATH);
    }

    [MenuItem("Custom Objects/Create World Gen Specs")]
    public static void CreateWorldGenSpecs()
    {
        SaveAsset(new WorldGenSpecs(), PATH + WORLDGENSPECS_PATH);
    }

    [MenuItem("Custom Objects/Create World Info")]
    public static void CreateWorldInfo()
    {
        SaveAsset(new WorldInfo(), PATH + WORLDINFO_PATH);
    }

    public static void SaveAsset(ScriptableObject asset, string path)
    {
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
