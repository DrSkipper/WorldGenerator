using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : BaseLevelGenerator
{
    //TODO - fcole - Allow custom bounds here?
    [System.Serializable]
    public struct RoomGenerationParams
    {
        public LevelGenMap.TileType FillTileType;
        public int NumberOfRooms;
        public int RoomMinSize;
        public int RoomMaxSize;
        public int MaxRetries;
    }

	public LevelGenMap.TileType FillTileType = LevelGenMap.TileType.B;
	public int NumberOfRooms = 3;
	public int RoomMinSize = 4;
	public int RoomMaxSize = 6;
	public List<Rect> Rooms;
	public int MaxRetries = 20;

	void Reset()
	{
		this.GeneratorName = "Room Generator";
	}

    public void ApplyParams(RoomGenerationParams generationParams)
    {
        this.FillTileType = generationParams.FillTileType;
        this.NumberOfRooms = generationParams.NumberOfRooms;
        this.RoomMinSize = generationParams.RoomMinSize;
        this.RoomMaxSize = generationParams.RoomMaxSize;
        this.MaxRetries = generationParams.MaxRetries;
        this.Rooms = new List<Rect>();
    }

    public override void SetupGeneration(LevelGenMap inputMap)
    {
        base.SetupGeneration(inputMap);
        this.Rooms.Clear();
		_retries = 0;
        _corridorTiles = new List<List<LevelGenMap.Coordinate>>();
        this.AddPhase(this.AddRoomsPhase);
	}

    public override LevelGenOutput GetOutput()
    {
        LevelGenOutput output = base.GetOutput();
        output.AddMapInfo(new LevelGenRoomInfo(SimpleRect.RectListToSimpleRectList(this.Rooms)));
        output.AddMapInfo(new LevelGenCorridorInfo(_corridorTiles));
        return output;
    }

	/**
	 * Phases
	 */
	public void AddRoomsPhase(int frames)
	{
		int room = this.CurrentPhase.FramesElapsed;

		int finalRoomForStep = room + frames;
		if (finalRoomForStep > this.NumberOfRooms)
			finalRoomForStep = this.NumberOfRooms;

		while (room < finalRoomForStep && _retries < this.MaxRetries)
		{
			int w = Random.Range(this.RoomMinSize, this.RoomMaxSize + 1);
			int h = Random.Range(this.RoomMinSize, this.RoomMaxSize + 1);
			int x = Random.Range(0, this.Map.Width - w);
			int y = Random.Range(0, this.Map.Height - h);
			Rect newRoom = new Rect(x, y, w, h);
			bool failed = false;

			foreach (Rect otherRoom in this.Rooms)
			{
				if (newRoom.Intersects(otherRoom))
				{
					failed = true;
					++_retries;
					break;
				}
			}

			if (!failed)
			{
				createRoom(newRoom);

				if (this.Rooms.Count > 0) // First room has no previous room to connect to
				{
					Vector2 prevRoomCenter = this.Rooms[this.Rooms.Count - 1].center;
                    List<LevelGenMap.Coordinate> corridor = new List<LevelGenMap.Coordinate>();
					if (Random.Range(0, 2) == 1)
					{
						createHTunnel((int)prevRoomCenter.x, (int)newRoom.center.x, (int)prevRoomCenter.y, corridor);
						createVTunnel((int)prevRoomCenter.y, (int)newRoom.center.y, (int)newRoom.center.x, corridor);
					}
					else
					{
						createVTunnel((int)prevRoomCenter.y, (int)newRoom.center.y, (int)newRoom.center.x, corridor);
						createHTunnel((int)prevRoomCenter.x, (int)newRoom.center.x, (int)prevRoomCenter.y, corridor);
					}
                    _corridorTiles.Add(corridor);
				}

				this.Rooms.Add(newRoom);
				++room;
			}
		}

		if (finalRoomForStep == this.NumberOfRooms)
			this.NextPhase();
	}

	/**
	 * Public
	 */

	// Can be used to place player
	public Rect FirstRoom()
	{
		return this.Rooms[0];
	}

	// Can be used to place stairs
	public Rect LastRoom()
	{
		return this.Rooms[this.Rooms.Count - 1];
	}

	// Can be used to populate rooms with items, monsters etc
	public List<Rect> AllRooms()
	{
		return this.Rooms;
	}

	/**
	 * Private
	 */
	private int _retries;
    private List<List<LevelGenMap.Coordinate>> _corridorTiles;

    private void createHTunnel(int x1, int x2, int y, List<LevelGenMap.Coordinate> corridor)
	{
		for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); ++x)
		{
            if (this.Map.Grid[x, y] != this.FillTileType)
                corridor.Add(this.Map.ConstructValidCoordinate(x, y, false).Value);
			this.Map.Grid[x, y] = this.FillTileType;
		}
	}

	private void createVTunnel(int y1, int y2, int x, List<LevelGenMap.Coordinate> corridor)
	{
		for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); ++y)
        {
            if (this.Map.Grid[x, y] != this.FillTileType)
                corridor.Add(this.Map.ConstructValidCoordinate(x, y, false).Value);
            this.Map.Grid[x, y] = this.FillTileType;
		}
	}

	private void createRoom(Rect room)
	{
		this.Map.FillRect(room, this.FillTileType);
	}
}
