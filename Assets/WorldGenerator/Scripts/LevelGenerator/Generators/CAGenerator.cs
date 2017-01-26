using UnityEngine;
using System.Collections.Generic;
 
/**
 * Runs Cellular Automata on the map for generation
 */
public class CAGenerator : BaseLevelGenerator
{
    //TODO - fcole - Allow custom bounds for each CA pass?
    [System.Serializable]
    public struct CAGenerationParams
    {
        public LevelGenMap.TileType ValidBaseTilesForGeneration;
        public LevelGenMap.TileType FillTileType;
        public float InitialChance;
        public int DeathLimit;
        public int BirthLimit;
        public int NumberOfSteps;
        public int MaxCaves;
    }

	public LevelGenMap.TileType ValidBaseTilesForGeneration = LevelGenMap.TileType.MASK_ALL;
	public LevelGenMap.TileType FillTileType = LevelGenMap.TileType.B;
	public float InitialChance = 0.48f;
	public int DeathLimit = 4;
	public int BirthLimit = 4;
	public int NumberOfSteps = 3;
	public int MaxCaves = -1;

	// To be used during generation, once generation is done we align with correct map types
	private const LevelGenMap.TileType BASE_TYPE = LevelGenMap.TileType.A; // "alive"
	private const LevelGenMap.TileType CAVE_TYPE = LevelGenMap.TileType.B; // "dead"

	public void Reset()
	{
		this.GeneratorName = "CA Generator";
	}

    public void ApplyParams(CAGenerationParams generationParams)
    {
        this.ValidBaseTilesForGeneration = generationParams.ValidBaseTilesForGeneration;
        this.FillTileType = generationParams.FillTileType;
        this.InitialChance = generationParams.InitialChance;
        this.DeathLimit = generationParams.DeathLimit;
        this.BirthLimit = generationParams.BirthLimit;
        this.NumberOfSteps = generationParams.NumberOfSteps;
        this.MaxCaves = generationParams.MaxCaves;
    }

    public override void SetupGeneration(LevelGenMap inputMap, LevelGenMap outputMap, IntegerRect bounds)
    {
        base.SetupGeneration(inputMap, outputMap, bounds);
        this.AddPhase(this.InitialPhase);
		this.AddPhase(this.AutomataPhase);
		if (this.MaxCaves >= 0)
			this.AddPhase(this.FillCavesPhase);
		this.AddPhase(this.ApplyCorrectTileTypes);
	}

	/**
	 * Phases
	 */
	public void InitialPhase(int frames)
	{
		_originalMap = this.OutputMap.CopyOfGridRect(this.Bounds);
        _inputMap = this.InputMap.CopyOfGridRect(this.Bounds);
        this.OutputMap.FillMatchingTilesInRect(this.Bounds, CAVE_TYPE, this.ValidBaseTilesForGeneration, this.InputMap);
        this.OutputMap.FillMatchingTilesWithChance(this.Bounds, BASE_TYPE, this.ValidBaseTilesForGeneration, this.InitialChance, this.InputMap);
		this.NextPhase();
	}

	public void AutomataPhase(int frames)
	{
		int finalFrameForStep = this.CurrentPhase.FramesElapsed + frames;
		if (finalFrameForStep > this.NumberOfSteps)
			finalFrameForStep = this.NumberOfSteps;

		runAutomata(finalFrameForStep);

		if (finalFrameForStep == this.NumberOfSteps)
			this.NextPhase();
	}

	public void FillCavesPhase(int frames)
	{
        List<LevelGenMap.Coordinate> coords = this.OutputMap.ListOfCoordinatesOfType(this.Bounds, CAVE_TYPE);
        List<List<LevelGenMap.Coordinate>> caves = new List<List<LevelGenMap.Coordinate>>();

        // Gather all the caves
        while (coords.Count > 0)
        {
            LevelGenMap.Coordinate coord = coords[coords.Count - 1];
            coords.RemoveAt(coords.Count - 1);

            List<LevelGenMap.Coordinate> cave = this.OutputMap.FloodFill(coord, CAVE_TYPE);
            foreach (LevelGenMap.Coordinate caveCoord in cave)
            {
                coords.Remove(caveCoord);
            }

            caves.Add(cave);
        }

        // If there are too many caves, only keep the largest ones
        if (caves.Count > this.MaxCaves)
        {
            caves.Sort(
                delegate(List<LevelGenMap.Coordinate> cave1, List<LevelGenMap.Coordinate> cave2)
                {
                    if (cave1.Count > cave2.Count) return -1;
                    if (cave1.Count < cave2.Count) return 1;
                    return 0;
                });

            for (int i = this.MaxCaves; i < caves.Count; ++i)
            {
                foreach (LevelGenMap.Coordinate caveCoord in caves[i])
                {
                    this.OutputMap.Grid[caveCoord.x, caveCoord.y] = BASE_TYPE;
                }
            }

            caves.RemoveRange(this.MaxCaves, caves.Count - this.MaxCaves);
        }

        _caves = caves;
		this.NextPhase();
	}

	public void ApplyCorrectTileTypes(int frames)
	{
		// Fill the shape of the caves with FillTileType and leave the rest at their original value
		for (int x = 0; x < this.Bounds.Size.X; ++x)
		{
			for (int y = 0; y < this.Bounds.Size.Y; ++y)
			{
				int globalX = this.Bounds.Min.X + x;
				int globalY = this.Bounds.Min.Y + y;

				if (this.OutputMap.Grid[globalX, globalY] == BASE_TYPE || (_inputMap[x, y] | this.ValidBaseTilesForGeneration) != this.ValidBaseTilesForGeneration)
					this.OutputMap.Grid[globalX, globalY] = _originalMap[x, y];
				else
					this.OutputMap.Grid[globalX, globalY] = this.FillTileType;
			}
		}
		this.NextPhase();
	}

    public override LevelGenOutput GetOutput()
    {
        LevelGenOutput output = base.GetOutput();
        output.AddMapInfo(new LevelGenCaveInfo(_caves));
        return output;
    }

    /**
	 * Private
	 */
    private LevelGenMap.TileType[,] _originalMap;
    private LevelGenMap.TileType[,] _inputMap;
    List<List<LevelGenMap.Coordinate>> _caves;

    private void runAutomata(int finalFrame)
	{
		for (int i = this.CurrentPhase.FramesElapsed; i < finalFrame; ++i)
		{
            LevelGenMap.TileType[,] newMap = new LevelGenMap.TileType[this.Bounds.Size.X, this.Bounds.Size.Y];
			doSimulationStep(this.OutputMap.Grid, newMap);
            this.OutputMap.ApplyGridSubset(this.Bounds.Min.X, this.Bounds.Min.Y, newMap);
		}
	}

    private void doSimulationStep(LevelGenMap.TileType[,] prevMap, LevelGenMap.TileType[,] newMap)
	{
		for (int x = this.Bounds.Min.X; x < this.Bounds.Max.X; ++x)
		{
			for (int y = this.Bounds.Min.Y; y < this.Bounds.Max.Y; ++y)
            {
                int inBoundsX = x - this.Bounds.Min.X;
                int inBoundsY = y - this.Bounds.Min.Y;

                // Make sure this tile matches our valid tile types to run CA on
                if ((_inputMap[inBoundsX, inBoundsY] | this.ValidBaseTilesForGeneration) != this.ValidBaseTilesForGeneration)
                    continue;

				int nbs = countAliveNeighbours(prevMap, x, y);

				if (prevMap[x, y] == BASE_TYPE)
				{
					// First, if a cell is alive but has too few neighbours, kill it.
					newMap[inBoundsX, inBoundsY] = nbs < this.DeathLimit ? CAVE_TYPE : BASE_TYPE;
				}
				else
				{
					// Otherwise, if the cell is dead now, check if it has the right number of neighbours to be 'born'
					newMap[inBoundsX, inBoundsY] = nbs > this.BirthLimit ? BASE_TYPE : CAVE_TYPE;
				}
			}
		}
	}

    private int countAliveNeighbours(LevelGenMap.TileType[,] map, int x, int y)
	{
		int count = 0;
		for (int i = -1; i < 2; ++i)
		{
			for (int j = -1; j < 2; ++j)
			{
				int neighbour_x = x + i;
				int neighbour_y = y + j;

				// If we're looking at the middle point
				if (i == 0 && j == 0)
					continue; // Do nothing, we don't want to add ourselves in!

				// In case the index we're looking at it off the edge of the map
				else if (neighbour_x < this.Bounds.Min.X || neighbour_y < this.Bounds.Min.Y || neighbour_x >= this.Bounds.Max.X || neighbour_y >= this.Bounds.Max.Y)
					++count;

				// Otherwise, a normal check of the neighbour
				else if (map[neighbour_x, neighbour_y] == BASE_TYPE)
					++count;
			}
		}
		return count;
	}
}
