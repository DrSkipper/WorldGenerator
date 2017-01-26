using UnityEngine;
using System.Collections.Generic;

public class LevelGenMap : MonoBehaviour
{
	public int Width = 30;
	public int Height = 20;

    public enum TileType
    {
        A = 0x000002,
        B = 0x000004,
        C = 0x000008,
        D = 0x000010,
        E = 0x000020,
        F = 0x000040,
        G = 0x000080,
        H = 0x000100,
        INTERNAL_A = 0x000200,
        INTERNAL_B = 0x000400,
        MASK_ALL = 0xFFFFFF,
        MASK_NONE = 0x000000
    }

    public struct Coordinate
    {
        public int x;
        public int y;
        public Coordinate(int px, int py)
        {
            x = px;
            y = py;
        }

        public IntegerVector integerVector { get { return new IntegerVector(x, y); } }
    }

    public TileType[,] Grid
	{
		get
		{
			if (_grid == null)
                _grid = new TileType[this.Width, this.Height];
			return _grid;
		}
		set
		{
			_grid = value;
		}
	}

    public void Reset()
    {
        _grid = null;
    }

    public TileType TileTypeAtCoordinate(Coordinate coord)
    {
        return this.Grid[coord.x, coord.y];
    }

    /**
     * 'allowsWrapping' = whether to wrap to other side of map if x or y are out of bounds
     * Returns null if there is no valid coordinate
     */
    public Coordinate? ConstructValidCoordinate(int x, int y, bool allowsWrapping)
    {
        if (allowsWrapping)
        {
            while (x < 0)
                x += this.Width;
            while (x >= this.Width)
                x -= this.Width;
            while (y < 0)
                y += this.Height;
            while (y >= this.Height)
                y -= this.Height;

            return new Coordinate(x, y);
        }

        if (x < 0 || x >= this.Width || y < 0 || y >= this.Height)
            return null;
        return new Coordinate(x, y);
    }

    public void FillCompletely(TileType typeToFill)
    {
        this.FillRect(IntegerRect.ConstructRectFromMinAndSize(0, 0, this.Width, this.Height), typeToFill);
    }
    
    public void FillRect(IntegerRect rect, TileType typeToFill)
    {
        this.FillMatchingTilesInRect(rect, typeToFill, TileType.MASK_ALL);
    }

    public void FillMatchingTilesInRect(IntegerRect rect, TileType typeToFill, TileType match, LevelGenMap mapToCheckAgainst = null)
    {
        if (mapToCheckAgainst == null)
            mapToCheckAgainst = this;

        if (_grid == null)
            _grid = new TileType[this.Width, this.Height];

        for (int x = rect.Min.X; x < rect.Max.X; ++x)
        {
            for (int y = rect.Min.Y; y < rect.Max.Y; ++y)
            {
                if ((mapToCheckAgainst.Grid[x, y] | match) == match)
                    _grid[x, y] = typeToFill;
            }
        }
    }

    public void FillRectWithChance(IntegerRect rect, TileType typeToFill, float chance, LevelGenMap mapToCheckAgainst = null)
    {
        this.FillMatchingTilesWithChance(rect, typeToFill, TileType.MASK_ALL, chance, mapToCheckAgainst);
	}

    public void FillMatchingTilesWithChance(IntegerRect rect, TileType typeToFill, TileType match, float chance, LevelGenMap mapToCheckAgainst = null)
    {
        if (mapToCheckAgainst == null)
            mapToCheckAgainst = this;

        if (_grid == null)
            _grid = new TileType[this.Width, this.Height];

        for (int x = rect.Min.X; x < rect.Max.X; ++x)
        {
            for (int y = rect.Min.Y; y < rect.Max.Y; ++y)
            {
                if ((mapToCheckAgainst.Grid[x, y] | match) == match && Random.Range(0.0f, 1.0f) <= chance)
                    _grid[x, y] = typeToFill;
            }
        }
    }

    public void ApplyGridSubset(int StartX, int StartY, TileType[,] subset)
    {
        for (int x = StartX; x < StartX + subset.GetLength(0); ++x)
        {
            for (int y = StartY; y < StartY + subset.GetLength(1); ++y)
            {
                this.Grid[x, y] = subset[x - StartX, y - StartY];
            }
        }
    }

	public List<Coordinate> CoordinatesInRect(IntegerRect rect, bool allowsWrapping = false)
	{
		List<Coordinate> coords = new List<Coordinate>();
		for (int x = rect.Min.X; x < rect.Max.X; ++x)
		{
			for (int y = rect.Min.Y; y < rect.Max.Y; ++y)
			{
				Coordinate? coord = this.ConstructValidCoordinate(x, y, allowsWrapping);
				if (coord.HasValue)
					coords.Add(coord.Value);
			}
		}
		return coords;
	}

	public LevelGenMap.TileType[,] CopyOfGridRect(IntegerRect rect)
	{
		LevelGenMap.TileType[,] copy = new LevelGenMap.TileType[rect.Size.X, rect.Size.Y];
		for (int x = rect.Min.X; x < rect.Max.X; ++x)
		{
			for (int y = rect.Min.Y; y < rect.Max.Y; ++y)
			{
				copy[x - rect.Min.X, y - rect.Min.Y] = _grid[x, y];
			}
		}
		return copy;
	}

    public List<Coordinate> CoordinatesInManhattanRange(Coordinate centerTile, int range, bool allowsWrapping = false)
    {
        List<Coordinate> coords = new List<Coordinate>();
        for (int x = centerTile.x - range; x <= centerTile.x + range; ++x)
        {
            Coordinate? mid = this.ConstructValidCoordinate(x, centerTile.y, allowsWrapping);
            if (mid.HasValue)
                coords.Add(mid.Value);
            int rangeAtRow = range - (Mathf.Abs(centerTile.x - x));
            for (int i = 1; i <= rangeAtRow; ++i)
            {
                Coordinate? up = this.ConstructValidCoordinate(x, centerTile.y + i, allowsWrapping);
                if (up.HasValue)
                    coords.Add(up.Value);

                Coordinate? down = this.ConstructValidCoordinate(x, centerTile.y - i, allowsWrapping);
                if (down.HasValue)
                    coords.Add(down.Value);
            }
        }
        return coords;
    }

    public List<Coordinate> ListOfCoordinatesOfType(IntegerRect rect, TileType type, bool allowWrapping = false)
    {
        List<Coordinate> coords = new List<Coordinate>();

        for (int x = rect.Min.X; x < rect.Max.X; ++x)
        {
            for (int y = rect.Min.Y; y < rect.Max.Y; ++y)
            {
                if (_grid[x, y] == type)
                {
                    Coordinate? possibleCoord = this.ConstructValidCoordinate(x, y, allowWrapping);
                    if (possibleCoord.HasValue)
                        coords.Add(possibleCoord.Value);
                }
            }
        }

        return coords;
    }

    /*
     * Returns list of tiles with same time as rootTile within reach of flood fill traversal
     * (returns an "island" that includes rootTile)
     * 
     * http://en.wikipedia.org/wiki/Flood_fill
     * 
     * Flood-fill (node, target-color, replacement-color):
     * 1. If target-color is equal to replacement-color, return.
     * 2. Set Q to the empty queue.
     * 3. Add node to the end of Q.
     * 4. While Q is not empty: 
     * 5.     Set n equal to the last element of Q.
     * 6.     Remove last element from Q.
     * 7.     If the color of n is equal to target-color:
     * 8.         Set the color of n to replacement-color and mark "n" as processed.
     * 9.         Add west node to end of Q if west has not been processed yet.
     * 10.        Add east node to end of Q if east has not been processed yet.
     * 11.        Add north node to end of Q if north has not been processed yet.
     * 12.        Add south node to end of Q if south has not been processed yet.
     * 13. Return.
     */
    public List<Coordinate> FloodFill(Coordinate startCoord, TileType tileType, bool allowWrapping = false)
    {
        bool[,] processedFlags = new bool[this.Width, this.Height];
        List<Coordinate> reachedTiles = new List<Coordinate>();
        List<Coordinate> stack = new List<Coordinate>();

        processIntoStack(stack, startCoord, processedFlags);

        while (stack.Count > 0)
        {
            Coordinate currentCoord = stack[0];
            stack.RemoveAt(0);

            //if ((this.TileTypeAtCoordinate(currentCoord) & tileType) != 0x000000) // More lenient/inclusive check
            if (this.TileTypeAtCoordinate(currentCoord) == tileType) // Exact equality
            {
                reachedTiles.Add(currentCoord);
                processIntoStack(stack, this.ConstructValidCoordinate(currentCoord.x - 1, currentCoord.y, allowWrapping), processedFlags);
                processIntoStack(stack, this.ConstructValidCoordinate(currentCoord.x + 1, currentCoord.y, allowWrapping), processedFlags);
                processIntoStack(stack, this.ConstructValidCoordinate(currentCoord.x, currentCoord.y - 1, allowWrapping), processedFlags);
                processIntoStack(stack, this.ConstructValidCoordinate(currentCoord.x, currentCoord.y + 1, allowWrapping), processedFlags);
            }
        }

        return reachedTiles;
    }

    public bool CanPathBetweenCoordinates(Coordinate coord1, Coordinate coord2, bool allowWrapping = false)
    {
        TileType fillType = this.TileTypeAtCoordinate(coord1);
        if (this.TileTypeAtCoordinate(coord2) != fillType)
            return false;

        //TODO - fcole - A* here would be faster
        return this.FloodFill(coord1, fillType, allowWrapping).Contains(coord2);
    }

	/**
	 * Private
	 */
    private TileType[,] _grid;

    private void processIntoStack(List<Coordinate> stack, Coordinate? coord, bool[,] processedFlags)
    {
        if (coord.HasValue && !processedFlags[coord.Value.x, coord.Value.y])
        {
            stack.Insert(0, coord.Value);
            processedFlags[coord.Value.x, coord.Value.y] = true;
        }
    }
}
