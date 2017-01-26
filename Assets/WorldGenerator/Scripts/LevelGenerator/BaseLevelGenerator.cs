using UnityEngine;
using System.Collections.Generic;

public class BaseLevelGenerator : MonoBehaviour
{
	public string GeneratorName = "Basic Generator";
    public IntegerRect Bounds;
    public LevelGenMap.TileType OpenTileType = LevelGenMap.TileType.B;

    public LevelGenMap OutputMap { get; private set; }
    public LevelGenMap InputMap { get { return _inputMap; } }
    public LevelGenPhase CurrentPhase { get { return _phases[_currentPhase]; } }
	public bool IsFinished { get { return _currentPhase >= _phases.Count; } }

    public void Start()
    {
    }

	/**
	 * Virtual
	 */
	public virtual void SetupGeneration(LevelGenMap inputMap, LevelGenMap outputMap, IntegerRect bounds)
	{
		_phases = null;
		_currentPhase = 0;
        _inputMap = inputMap;
        this.OutputMap = outputMap;
        this.Bounds = bounds;
    }

	/**
	 * Public
	 */
	public void GenerateEntireMap(LevelGenMap inputMap, LevelGenMap outputMap, IntegerRect bounds)
	{
		this.SetupGeneration(inputMap, outputMap, bounds);

		while (!this.IsFinished)
			this.RunGenerationFrames(int.MaxValue);
	}

	public void RunGenerationFrames(int frames)
	{
		this.CurrentPhase.RunFrames(frames);
	}

	public void AddPhase(LevelGenPhase.PhaseUpdate phaseCallback)
	{
		if (_phases == null)
			_phases = new List<LevelGenPhase>();
		_phases.Add(new LevelGenPhase(phaseCallback));
	}

	public void NextPhase()
	{
		++_currentPhase;
	}

    public virtual LevelGenOutput GetOutput()
    {
        LevelGenOutput output = new LevelGenOutput();
        output.Grid = this.OutputMap.Grid;
        output.MapInfo = new Dictionary<string, LevelGenMapInfo>();
        output.OpenTiles = this.OutputMap.ListOfCoordinatesOfType(this.Bounds, this.OpenTileType);
        return output;
    }

	/**
	 * Private
	 */
	private int _currentPhase;
	private List<LevelGenPhase> _phases;
    private LevelGenMap _inputMap;
}
