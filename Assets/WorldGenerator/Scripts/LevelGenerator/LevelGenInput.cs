using UnityEngine;

public class LevelGenInput : ScriptableObject
{
    public enum GenerationType
    {
        CA,
        BSP,
        Room
    }

    public GenerationType Type;
    public IntegerVector[] MapSizes;
    public IntegerVector NumRoomsRange;
}
