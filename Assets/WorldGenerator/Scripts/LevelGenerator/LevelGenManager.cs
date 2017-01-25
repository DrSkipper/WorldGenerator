using UnityEngine;
using System.Collections.Generic;

public class LevelGenManager : LevelGenBehavior
{
    public delegate void LevelGenerationUpdateDelegate();

	public float StepRunInterval = 0.0f;
	public int StepsRunEachUpdate = int.MaxValue;
	public bool Finished { get { return !_generating; } }
    public CAGenerator.CAGenerationParams DefaultCAParams;
    public BSPGenerator.BSPGenerationParams DefaultBSPParams;
    public bool RemoveTips = true;
    public int Border = 0;

	public void InitiateGeneration(LevelGenInput input)
	{
		_generating = true;
		_timeSinceLastStep = 0.0f;
        _input = input;

		switch (input.Type)
        {
            default:
            case LevelGenInput.GenerationType.CA:
                _generator = this.gameObject.AddComponent<CAGenerator>();
                ((CAGenerator)_generator).MaxCaves = 1;
                break;
            case LevelGenInput.GenerationType.BSP:
                _generator = this.gameObject.AddComponent<BSPGenerator>();
                ((BSPGenerator)_generator).ApplyParams(this.DefaultBSPParams);
                //((BSPGenerator)_generator).
                break;
            case LevelGenInput.GenerationType.Room:
                _generator = this.gameObject.AddComponent<RoomGenerator>();
                ((RoomGenerator)_generator).NumberOfRooms = Random.Range(input.NumRoomsRange.X, input.NumRoomsRange.Y + 1);
                break;
        }

        IntegerVector size = input.MapSizes[Random.Range(0, input.MapSizes.Length)];
        this.Map.Reset();
        this.Map.Width = size.X;
        this.Map.Height = size.Y;
        this.Map.FillCompletely(LevelGenMap.TileType.A);
        _generator.Bounds = new Rect(this.Border, this.Border, this.Map.Width - this.Border * 2, this.Map.Height - this.Border * 2);
		_generator.SetupGeneration(this.Map);

        if (_updateDelegates != null)
        {
            for (int i = 0; i < _updateDelegates.Count; ++i)
                _updateDelegates[i]();
        }
    }

	public void TestGenerator(BaseLevelGenerator generator)
	{
		_generating = true;
		_timeSinceLastStep = 0.0f;

		_generatorRemainsWhenDone = true;
		_generator = generator;
		this.Map.FillCompletely(LevelGenMap.TileType.A);
		_generator.SetupGeneration(this.Map);

        if (_updateDelegates != null)
        {
            for (int i = 0; i < _updateDelegates.Count; ++i)
                _updateDelegates[i]();
        }
    }

	public void Update()
	{
		if (_generating)
		{
			_timeSinceLastStep += Time.deltaTime;
			if (_timeSinceLastStep > this.StepRunInterval)
			{
				_timeSinceLastStep = 0.0f;
				_generator.RunGenerationFrames(this.StepsRunEachUpdate);

                if (_generator.IsFinished)
                {
                    _generating = false;

                    if (this.RemoveTips)
                        removeTips();
                }

                if (_updateDelegates != null)
                {
                    for (int i = 0; i < _updateDelegates.Count; ++i)
                        _updateDelegates[i]();
                }
			}
		}
	}

	public void HaltGeneration()
	{
		_generating = false;

		if (_generator != null)
		{
			if (!_generatorRemainsWhenDone)
				Destroy(_generator);
			_generator = null;
		}

		_generatorRemainsWhenDone = false;
	}

	public void Cleanup()
	{
		this.HaltGeneration();
	}

	public LevelGenOutput GetOutput()
	{
        LevelGenOutput output = _generator.GetOutput();
        output.Input = _input;
        return output;
	}

    public void AddUpdateDelegate(LevelGenerationUpdateDelegate callback)
    {
        if (_updateDelegates == null)
            _updateDelegates = new List<LevelGenerationUpdateDelegate>();
        _updateDelegates.Add(callback);
    }

    public void RemoveUpdateDelegate(LevelGenerationUpdateDelegate callback)
    {
        _updateDelegates.Remove(callback);
    }

	/**
	 * Private
	 */
	private bool _generating;
	private bool _generatorRemainsWhenDone;
	private float _timeSinceLastStep;
	private BaseLevelGenerator _generator;
    private LevelGenInput _input;
    private List<LevelGenerationUpdateDelegate> _updateDelegates;

    private void removeTips()
    {
        //TODO
    }
}
