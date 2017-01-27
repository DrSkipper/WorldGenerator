using UnityEngine;

public class WorldGenSpecs : ScriptableObject
{
    [System.Serializable]
    public enum GenerationStageType
    {
        CA,
        BSP,
        Room,
        Fill
    }

    [System.Serializable]
    public struct GenerationParam
    {
        public string Name;
        public float Value;

        /**
         * Valid Parameters
         *
         * Universal:
         * input_layer
         * 
         * CA:
         * max_caves
         * initial_chance
         * death_limit
         * birth_limit
         * number_of_steps
         * 
         * BSP:
         * room_min_size
         * room_max_size
         * min_leaf_size
         * min_node_wh_ratio
         * room_to_leaf_ratio
         * enable_corridors
         * extra_corridors_per_room
         *
         * Room:
         * num_rooms
         * room_min_size
         * room_max_size
         * max_retries
         */
    }

    [System.Serializable]
    public struct GenerationStage
    {
        public IntegerVector Min;
        public IntegerVector Size;
        public GenerationStageType Type;
        [BitMask(typeof(LevelGenMap.TileType))]
        public LevelGenMap.TileType ValidInputTiles;
        public LevelGenMap.TileType OutputTileType;
        public GenerationParam[] Parameters;
    }

    [System.Serializable]
    public struct GenerationLayer
    {
        public GenerationStage[] Stages;
    }

    public GenerationLayer[] Layers;
    public IntegerVector MapSize;
}
