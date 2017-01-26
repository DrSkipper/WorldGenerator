using UnityEngine;
using System.Collections.Generic;

public class WorldGenManager : MonoBehaviour
{
    public int FramesBetweenUpdates = 0;
    public int StepsRunEachUpdate = int.MaxValue;
    public bool Finished { get { return !_generating; } }
    public CAGenerator.CAGenerationParams DefaultCAParams;
    public BSPGenerator.BSPGenerationParams DefaultBSPParams;
    public RoomGenerator.RoomGenerationParams DefaultRoomParams;
    public LevelGenMap LayerPrefab;
    public CAGenerator CAGenerator;
    public BSPGenerator BSPGenerator;
    public RoomGenerator RoomGenerator;

    public delegate void WorldGenerationUpdateDelegate(bool finished);

    public void InitiateGeneration(WorldGenSpecs specs)
    {
        _generating = true;
        _generator = null;
        _framesSinceUpdate = 0;
        _specs = specs;
        _currentLayer = 0;
        _currentStageInLayer = 0;

        if (_layers == null)
        {
            _layers = new List<LevelGenMap>();
        }
        else
        {
            for (int i = 0; i < _layers.Count; ++i)
            {
                _layers[i].Reset();
                _layers[i].Width = specs.MapSize.X;
                _layers[i].Height = specs.MapSize.Y;
                _layers[i].FillCompletely(LevelGenMap.TileType.A);
            }
        }

        if (_specs.Layers == null || _specs.Layers.Length == 0)
        {
            _generating = false;
        }
        else
        {
            configureLayer();
            if (_specs.Layers[_currentLayer].Stages.Length > 0)
                configureGenerator();
        }

        notifyUpdateDelegates();
    }

    public void FixedUpdate()
    {
        if (_generating)
        {
            ++_framesSinceUpdate;
            if (_framesSinceUpdate > this.FramesBetweenUpdates)
            {
                _framesSinceUpdate = 0;
                if (_generator == null || _generator.IsFinished)
                {
                    ++_currentStageInLayer;
                    if (_specs.Layers[_currentLayer].Stages == null || _currentStageInLayer >= _specs.Layers[_currentLayer].Stages.Length)
                    {
                        ++_currentLayer;
                        _currentStageInLayer = 0;
                        if (_currentLayer >= _specs.Layers.Length)
                        {
                            _generating = false;
                            _generator = null;
                        }
                        else
                        {
                            configureLayer();
                            configureGenerator();
                        }
                    }
                    else
                    {
                        configureGenerator();
                    }
                }
                else
                {
                    _generator.RunGenerationFrames(this.StepsRunEachUpdate);
                }

                notifyUpdateDelegates();
            }
        }
    }

    public void AddUpdateDelegate(WorldGenerationUpdateDelegate callback)
    {
        if (_updateDelegates == null)
            _updateDelegates = new List<WorldGenerationUpdateDelegate>();
        _updateDelegates.Add(callback);
    }

    public void RemoveUpdateDelegate(WorldGenerationUpdateDelegate callback)
    {
        _updateDelegates.Remove(callback);
    }

    /**
	 * Private
	 */
    private bool _generating;
    private float _framesSinceUpdate;
    private BaseLevelGenerator _generator;
    private WorldGenSpecs _specs;
    private int _currentLayer;
    private int _currentStageInLayer;
    private List<LevelGenMap> _layers;
    private List<WorldGenerationUpdateDelegate> _updateDelegates;

    private void configureLayer()
    {
        while (_layers.Count <= _currentLayer)
        {
            LevelGenMap map = Instantiate<LevelGenMap>(this.LayerPrefab);
            map.Reset();
            map.Width = _specs.MapSize.X;
            map.Height = _specs.MapSize.Y;
            map.FillCompletely(LevelGenMap.TileType.A);
            map.transform.SetZ(-_currentLayer);
            this.AddUpdateDelegate(map.GetComponent<LevelGenTestRenderer>().MapWasUpdated);
            _layers.Add(map);
        }
    }

    private void configureGenerator()
    {
        WorldGenSpecs.GenerationStage stage = _specs.Layers[_currentLayer].Stages[_currentStageInLayer];
        int inputLayer = _currentLayer;
        IntegerVector layerSize = new IntegerVector(_layers[_currentLayer].Width, _layers[_currentLayer].Height);
        IntegerVector stageMin = IntegerVector.Clamp(stage.Min, IntegerVector.Zero, layerSize - new IntegerVector(1, 1));
        IntegerVector stageSize = stage.Size.X <= 0 || stage.Size.Y <= 0 ? layerSize - stageMin : IntegerVector.Clamp(stage.Size, new IntegerVector(1, 1), layerSize - stageMin);
        IntegerRect bounds = IntegerRect.ConstructRectFromMinAndSize(stageMin, stageSize);
        switch (stage.Type)
        {
            default:
            case WorldGenSpecs.GenerationStageType.CA:
                CAGenerator.CAGenerationParams caParams = this.DefaultCAParams;
                for (int i = 0; i < stage.Parameters.Length; ++i)
                {
                    string p = stage.Parameters[i].Name;
                    float vf = stage.Parameters[i].Value;
                    int vi = Mathf.RoundToInt(vf);

                    if (p == P_INPUT_LAYER)
                        inputLayer = vi;
                    else if (p == P_MAX_CAVES)
                        caParams.MaxCaves = vi;
                    else if (p == P_INITIAL_CHANCE)
                        caParams.InitialChance = vf;
                    else if (p == P_DEATH_LIMIT)
                        caParams.DeathLimit = vi;
                    else if (p == P_BIRTH_LIMIT)
                        caParams.BirthLimit = vi;
                    else if (p == P_NUMBER_OF_STEPS)
                        caParams.NumberOfSteps = vi;
                    else
                        Debug.LogWarning("Invalid parameter name for CA generation: " + p);
                }
                this.CAGenerator.ApplyParams(caParams);
                this.CAGenerator.SetupGeneration(_layers[inputLayer], _layers[_currentLayer], bounds);
                this.CAGenerator.ValidBaseTilesForGeneration = stage.ValidInputTiles;
                this.CAGenerator.FillTileType = stage.OutputTileType;
                _generator = this.CAGenerator;
                break;
            case WorldGenSpecs.GenerationStageType.BSP:
                BSPGenerator.BSPGenerationParams bspParams = this.DefaultBSPParams;
                for (int i = 0; i < stage.Parameters.Length; ++i)
                {
                    string p = stage.Parameters[i].Name;
                    float vf = stage.Parameters[i].Value;
                    int vi = Mathf.RoundToInt(vf);

                    if (p == P_INPUT_LAYER)
                        inputLayer = vi;
                    else if (p == P_ROOM_MIN_SIZE)
                        bspParams.RoomMinSize = vi;
                    else if (p == P_ROOM_MAX_SIZE)
                        bspParams.RoomMaxSize = vi;
                    else if (p == P_MIN_LEAF_SIZE)
                        bspParams.MinLeafSize = vi;
                    else if (p == P_MIN_NODE_WH_RATIO)
                        bspParams.MinNodeWHRatio = vf;
                    else if (p == P_ROOM_TO_LEAF_RATIO)
                        bspParams.RoomToLeafRatio = vf;
                    else if (p == P_ENABLE_CORRIDORS)
                        bspParams.EnableCorridors = vi <= 0 ? false : true;
                    else if (p == P_EXTRA_CORRIDORS_PER_ROOM)
                        bspParams.ExtraCorridorsPerRoom = vi;
                    else
                        Debug.LogWarning("Invalid parameter name for BSP generation: " + p);
                }
                this.BSPGenerator.ApplyParams(bspParams);
                this.BSPGenerator.SetupGeneration(_layers[inputLayer], _layers[_currentLayer], bounds);
                this.BSPGenerator.FillTileType = stage.OutputTileType;
                _generator = this.BSPGenerator;
                //TODO: Have BSPGen recognize ValidBaseTiles
                break;
            case WorldGenSpecs.GenerationStageType.Room:
                RoomGenerator.RoomGenerationParams roomParams = this.DefaultRoomParams;
                for (int i = 0; i < stage.Parameters.Length; ++i)
                {
                    string p = stage.Parameters[i].Name;
                    float vf = stage.Parameters[i].Value;
                    int vi = Mathf.RoundToInt(vf);

                    if (p == P_INPUT_LAYER)
                        inputLayer = vi;
                    else if (p == P_ROOM_MIN_SIZE)
                        roomParams.RoomMinSize = vi;
                    else if (p == P_ROOM_MAX_SIZE)
                        roomParams.RoomMaxSize = vi;
                    else if (p == P_NUM_ROOMS)
                        roomParams.NumberOfRooms = vi;
                    else if (p == P_MAX_RETRIES)
                        roomParams.MaxRetries = vi;
                    else
                        Debug.LogWarning("Invalid parameter name for Room generation: " + p);
                }
                this.RoomGenerator.ApplyParams(roomParams);
                this.RoomGenerator.SetupGeneration(_layers[inputLayer], _layers[_currentLayer], bounds);
                this.RoomGenerator.FillTileType = stage.OutputTileType;
                _generator = this.RoomGenerator;
                //TODO: Have RoomGen recognize ValidBaseTiles
                break;
            case WorldGenSpecs.GenerationStageType.Fill:
                for (int i = 0; i < stage.Parameters.Length; ++i)
                {
                    string p = stage.Parameters[i].Name;
                    float vf = stage.Parameters[i].Value;
                    int vi = Mathf.RoundToInt(vf);

                    if (p == P_INPUT_LAYER)
                        inputLayer = vi;
                    else
                        Debug.LogWarning("Invalid parameter name for Fill generation: " + p);
                }
                //TODO: Fill generator
                break;
        }
    }

    private void notifyUpdateDelegates()
    {
        if (_updateDelegates != null)
        {
            for (int i = 0; i < _updateDelegates.Count; ++i)
                _updateDelegates[i](this.Finished);
        }
    }

    private const string P_INPUT_LAYER = "input_layer";

    private const string P_MAX_CAVES = "max_caves";
    private const string P_INITIAL_CHANCE = "initial_chance";
    private const string P_DEATH_LIMIT = "death_limit";
    private const string P_BIRTH_LIMIT = "birth_limit";
    private const string P_NUMBER_OF_STEPS = "number_of_steps";

    private const string P_ROOM_MIN_SIZE = "room_min_size";
    private const string P_ROOM_MAX_SIZE = "room_max_size";
    private const string P_MIN_LEAF_SIZE = "min_leaf_size";
    private const string P_MIN_NODE_WH_RATIO = "min_node_wh_ratio";
    private const string P_ROOM_TO_LEAF_RATIO = "room_to_leaf_ratio";
    private const string P_ENABLE_CORRIDORS = "enable_corridors";
    private const string P_EXTRA_CORRIDORS_PER_ROOM = "extra_corridors_per_room";

    private const string P_NUM_ROOMS = "num_rooms";
    private const string P_MAX_RETRIES = "max_retries";
}
