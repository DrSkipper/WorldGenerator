using UnityEngine;
using System.Collections.Generic;

public class LevelGenStarter : LevelGenBehavior
{
    [System.Serializable]
    public struct InputTable
    {
        public List<LevelGenInput> PossibleInputs;
    }

    public GameObject Tiles;
    public List<InputTable> InputsByDifficulty;
    public List<InputTable> MiniBossInputs;
    public LevelGenInput InputToOverride;
    public bool OverrideInput = false;
    
    void Start()
    {
        this.BeginGeneration();
    }

    public void BeginGeneration()
    {
        /*if (!this.OverrideInput)
        {
            int difficulty = ProgressData.GetCurrentDifficulty();
            List<LevelGenInput> possibleInputs = !ProgressData.IsMiniBoss(ProgressData.MostRecentTile) ? this.InputsByDifficulty[difficulty].PossibleInputs : this.MiniBossInputs[difficulty].PossibleInputs;
            this.Manager.InitiateGeneration(possibleInputs[Random.Range(0, possibleInputs.Count)]);
        }
        else
        {*/
            this.Manager.InitiateGeneration(this.InputToOverride);
        //}
        _beganGeneration = true;
    }

    void Update()
    {
        if (_beganGeneration && this.Manager.Finished)
        {
            _beganGeneration = false;

            // Dump json of level gen output
            /*LevelGenOutput output = this.Manager.GetOutput();
            string json = JsonConvert.SerializeObject(output, Formatting.None);
            Debug.Log("json of level gen output:\n" + json);*/
            Debug.Log("level generation complete, json output not enabled");

            int[,] grid = this.tileTypeMapToSpriteIndexMap();
            //this.Tiles.GetComponent<TileMapOutlineRenderer>().CreateMapWithGrid(grid);
            //this.Tiles.GetComponent<TileGeometryCreator>().CreateMapWithGrid(grid);
            this.Manager.Cleanup();
        }
    }

    /**
	 * Private
	 */
    private bool _beganGeneration;

    private int[,] tileTypeMapToSpriteIndexMap()
    {
        LevelGenMap.TileType[,] grid = this.Map.Grid;
        int[,] spriteIndices = new int[this.Map.Width, this.Map.Height];

        for (int x = 0; x < this.Map.Width; ++x)
        {
            for (int y = 0; y < this.Map.Height; ++y)
            {
                spriteIndices[x, y] = tileSetIndexForTile(grid[x, y]);
            }
        }

        return spriteIndices;
    }

    //TODO - fcole - Data-drive this
    private int tileSetIndexForTile(LevelGenMap.TileType tile)
    {
        switch (tile)
        {
            default:
            case LevelGenMap.TileType.A:
                return 1;
            case LevelGenMap.TileType.B:
                return 0;
            case LevelGenMap.TileType.C:
                return 2;
            case LevelGenMap.TileType.D:
                return 3;
            case LevelGenMap.TileType.E:
                return 4;
            case LevelGenMap.TileType.F:
                return 5;
        }
    }
}
