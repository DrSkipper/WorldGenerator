using UnityEngine;
using UnityEditor;

public class WorldInfo : ScriptableObject
{
    public int QuadSize;
    public int SeaLevel;
    public IntegerVector PlainsHeightRange;
    public IntegerVector DesertHeightRange;
    public IntegerVector HillsHeightRange;
    public IntegerVector MountainsHeightRange;
    //TODO - method for distributing height for each type
}
