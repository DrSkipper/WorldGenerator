using System.Collections;
using System.Collections.Generic;

public class LevelGenOutput
{
	public LevelGenMap.TileType[,] Grid;
    public List<LevelGenMap.Coordinate> OpenTiles;
    public Dictionary<string, LevelGenMapInfo> MapInfo;
    public LevelGenInput Input;

    public void AddMapInfo(LevelGenMapInfo info)
    {
        if (this.MapInfo.ContainsKey(info.Name))
        {
            foreach (var entity in info.Data)
            {
                this.MapInfo[info.Name].Data.Add(entity);
            }
        }
        else
        {
            this.MapInfo[info.Name] = info;
        }
    }

    public void AppendOutput(LevelGenOutput output)
    {
        foreach (string key in output.MapInfo.Keys)
        {
            this.AddMapInfo(output.MapInfo[key]);
        }
    }
}

public class LevelGenMapInfo
{
    public string Name;
    public IList Data;
}

public class LevelGenRoomInfo : LevelGenMapInfo
{
    public const string KEY = "rooms";
    public LevelGenRoomInfo(List<IntegerRect> rooms)
    {
        this.Name = KEY;
        this.Data = rooms;
    }
}

public class LevelGenCorridorInfo : LevelGenMapInfo
{
    public const string KEY = "corridors";
    public LevelGenCorridorInfo(List<List<LevelGenMap.Coordinate>> corridors)
    {
        this.Name = KEY;
        this.Data = corridors;
    }
}

public class LevelGenCaveInfo : LevelGenMapInfo
{
    public const string KEY = "caves";
    public LevelGenCaveInfo(List<List<LevelGenMap.Coordinate>> caves)
    {
        this.Name = KEY;
        this.Data = caves;
    }
}
