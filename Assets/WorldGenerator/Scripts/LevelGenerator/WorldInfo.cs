using UnityEngine;

public class WorldInfo : ScriptableObject
{
    public int QuadSize;
    public int SeaLevel;
    public IntegerVector BeachHeightRange;
    public IntegerVector PlainsHeightRange;
    public IntegerVector DesertHeightRange;
    public IntegerVector HillsHeightRange;
    public IntegerVector MountainsHeightRange;
    //TODO - method for distributing height for each type
    public float BeachPerlinFrequency;
    public float PlainsPerlinFrequency;
    public float DesertPerlinFrequency;
    public float HillsPerlinFrequency;
    public float MountainsPerlinFrequency;
    public int BeachPerlinPower;
    public int PlainsPerlinPower;
    public int DesertPerlinPower;
    public int HillsPerlinPower;
    public int MountainsPerlinPower;
    public float FrequencyRange;
    public float BorderPercentage;
    public int BorderRange;
    public float BorderFrequency;
    public float KeepCornerChance;
}
