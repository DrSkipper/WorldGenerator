using UnityEngine;
using System.Collections.Generic;

public class WorldGenManager : MonoBehaviour
{
    public float StepRunInterval = 0.0f;
    public int StepsRunEachUpdate = int.MaxValue;
    public bool Finished { get { return !_generating; } }
    public CAGenerator.CAGenerationParams DefaultCAParams;
    public BSPGenerator.BSPGenerationParams DefaultBSPParams;
    public RoomGenerator.RoomGenerationParams DefaultRoomParams;
    public LevelGenMap LayerPrefab;
    public CAGenerator CAGenerator;
    public BSPGenerator BSPGenerator;
    public RoomGenerator RoomGenerator;

    public void InitiateGeneration(WorldGenSpecs specs)
    {
        _generating = true;
        _timeSinceLastStep = 0.0f;
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
                _layers[i].FillCompletely(LevelGenMap.TileType.A);
        }
    }

    /**
	 * Private
	 */
    private bool _generating;
    private bool _generatorRemainsWhenDone;
    private float _timeSinceLastStep;
    private BaseLevelGenerator _generator;
    private WorldGenSpecs _specs;
    private int _currentLayer;
    private int _currentStageInLayer;
    private List<LevelGenMap> _layers;

    private void configureLayer()
    {
        while (_layers.Count <= _currentLayer)
            _layers.Add(Instantiate<LevelGenMap>(this.LayerPrefab));
    }

    private void configureGenerator()
    {
        WorldGenSpecs.GenerationStage stage = _specs.Layers[_currentLayer].Stages[_currentStageInLayer];
        int inputLayer = _currentLayer;
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
                this.CAGenerator.SetupGeneration(_layers[inputLayer]);
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
                this.BSPGenerator.SetupGeneration(_layers[inputLayer]);
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
                this.RoomGenerator.SetupGeneration(_layers[inputLayer]);
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
