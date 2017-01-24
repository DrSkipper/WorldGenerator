using UnityEngine;

public class LevelGenInput : ScriptableObject
{
    public enum GenerationType
    {
        CA,
        BSP,
        Room,
        CABSPCombo
    }

    public GenerationType Type;
    public IntegerVector NumEnemiesRange;
    public int ExtraEnemiesPerAdditionalPlayer;
    public int[] GuaranteedEnemiesByDifficulty;
    public IntegerVector[] MapSizes;
    public IntegerVector NumRoomsRange;
    public int MinDistanceBetweenSpawns;

    public IntegerVector GetCurrentNumEnemiesRange()
    {
        return IntegerVector.Zero;
        /*int additionalEnemies = (DynamicData.NumJoinedPlayers() - 1) * this.ExtraEnemiesPerAdditionalPlayer; ;
        return new IntegerVector(this.NumEnemiesRange.X + additionalEnemies, this.NumEnemiesRange.Y + additionalEnemies);*/
    }
}
