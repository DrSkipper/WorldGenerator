using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BaseLevelGenerator : LevelGenBehavior
{
	public string GeneratorName = "Basic Generator";
    public Rect Bounds;
    public LevelGenMap.TileType OpenTileType = LevelGenMap.TileType.B;

    public LevelGenMap InputMap { get { return _inputMap; } }
    public LevelGenPhase CurrentPhase { get { return _phases[_currentPhase]; } }
	public bool IsFinished { get { return _currentPhase >= _phases.Count; } }

    public void Start()
    {
        if (this.Bounds.width == 0.0f || this.Bounds.height == 0.0f)
        {
            this.Bounds.x = 0;
            this.Bounds.y = 0;
            this.Bounds.width = this.Map.Width;
            this.Bounds.height = this.Map.Height;
        }
    }

	/**
	 * Virtual
	 */
	public virtual void SetupGeneration(LevelGenMap inputMap)
	{
		_phases = null;
		_currentPhase = 0;
        _inputMap = inputMap;
	}

	/**
	 * Public
	 */
	public void GenerateEntireMap()
	{
		this.SetupGeneration(this.Map);

		while (!this.IsFinished)
		{
			this.RunGenerationFrames(int.MaxValue);
		}
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
        output.Grid = this.Map.Grid;
        output.MapInfo = new Dictionary<string, LevelGenMapInfo>();
        output.OpenTiles = this.Map.ListOfCoordinatesOfType(this.Bounds, this.OpenTileType);
        return output;
    }

	/**
	 * Private
	 */
	private int _currentPhase;
	private List<LevelGenPhase> _phases;
    private LevelGenMap _inputMap;
}
