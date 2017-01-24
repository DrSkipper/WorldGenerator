using UnityEngine;
using System.Collections.Generic;

public class BSPGenerator : BaseLevelGenerator
{
	//TODO - fcole - Allow custom bounds here?
	[System.Serializable]
	public struct BSPGenerationParams
	{
        public LevelGenMap.TileType FillTileType;
        public LevelGenMap.TileType DebugSecondFillTileType;
		public int RoomMinSize;
		public int RoomMaxSize;
        public int MinLeafSize;
		public float MinNodeWHRatio;
		public float RoomToLeafRatio;
		public bool EnableCorridors;
		public float ExtraCorridorsPerRoom;
        public bool VisualizeSplitting;
	}

    public LevelGenMap.TileType FillTileType = LevelGenMap.TileType.B;
    public LevelGenMap.TileType DebugSecondFillTileType = LevelGenMap.TileType.B;
	public int RoomMinSize = 4;
	public int RoomMaxSize = 6;
    public int MinLeafSize = 8;
    public float MinNodeWHRatio = 0.25f;
	public float RoomToLeafRatio = 1.0f;
	public bool EnableCorridors = true;
	public float ExtraCorridorsPerRoom = 0.0f;
    public bool VisualizeSplitting = true;

	void Reset()
	{
		this.GeneratorName = "BSP Generator";
	}

	public void ApplyParams(BSPGenerationParams generationParams)
	{
        this.FillTileType = generationParams.FillTileType;
        this.DebugSecondFillTileType = generationParams.DebugSecondFillTileType;
		this.RoomMinSize = generationParams.RoomMinSize;
		this.RoomMaxSize = generationParams.RoomMaxSize;
        this.MinLeafSize = generationParams.MinLeafSize;
        this.MinNodeWHRatio = generationParams.MinNodeWHRatio;
		this.RoomToLeafRatio = generationParams.RoomToLeafRatio;
		this.EnableCorridors = generationParams.EnableCorridors;
		this.ExtraCorridorsPerRoom = generationParams.ExtraCorridorsPerRoom;
        this.VisualizeSplitting = generationParams.VisualizeSplitting;
	}

	public override void SetupGeneration()
	{
		base.SetupGeneration();
        _root = createNode(this.Bounds);
        _nodeQueue = new List<BSPNode>();
		_nodeList = new List<BSPNode>();
		_leaves = new List<BSPNode>();
		_nodeQueue.Add(_root);
        _originalMap = this.Map.CopyOfGridRect(this.Bounds);
		_rooms = new List<Rect>();
		_corridorTiles = new List<List<LevelGenMap.Coordinate>>();
        _numExtraCorridors = 0;

        this.AddPhase(this.DivisionPhase);
		if (this.VisualizeSplitting)
			this.AddPhase(this.PostDivisionVisualizationPhase);
        this.AddPhase(this.RoomCreationPhase);
		if (this.VisualizeSplitting)
			this.AddPhase(this.PostRoomCreationVisualizationPhase);
		if (this.EnableCorridors)
		{
			this.AddPhase(this.RoomConnectionSetupPhase);
			this.AddPhase(this.RoomConnectionPhase);
		}
		if (this.ExtraCorridorsPerRoom > 0)
			this.AddPhase(this.ExtraRoomConnectionPhase);
	}

    public override LevelGenOutput GetOutput()
    {
        LevelGenOutput output = base.GetOutput();
        output.AddMapInfo(new LevelGenRoomInfo(SimpleRect.RectListToSimpleRectList(_rooms)));
        output.AddMapInfo(new LevelGenCorridorInfo(_corridorTiles));
        return output;
    }

    public void DivisionPhase(int frames)
    {
        for (int i = 0; i < frames; ++i)
        {
			if (_nodeQueue.Count == 0)
			{
				_numRoomsToMake = Mathf.Max(Mathf.RoundToInt(this.RoomToLeafRatio * _leaves.Count), 1);
                this.NextPhase();
                break;
            }

			BSPNode node = _nodeQueue[0];
			_nodeQueue.RemoveAt(0);
            node.Children = splitNode(node);

            if (node.Children != null)
			{
				_nodeQueue.Add(node.Children[0]);
				_nodeQueue.Add(node.Children[1]);
            }
			else
			{
				_leaves.Add(node);
			}
        }

        if (this.VisualizeSplitting)
        {
            applyOriginalMap();
            visualizeNodes(true);
        }
    }

	public void PostDivisionVisualizationPhase(int frames)
	{
		this.NextPhase();
		applyOriginalMap();
		visualizeNodes(true);
	}

    public void RoomCreationPhase(int frames)
	{
		for (int i = 0; i < frames; ++i)
		{
			if (_leaves.Count == 0 || _rooms.Count >= _numRoomsToMake)
			{
				this.NextPhase();
				break;
			}

			int leafIndex = Random.Range(_rooms.Count, _leaves.Count);
			BSPNode leaf = _leaves[leafIndex];
			_leaves.RemoveAt(leafIndex);
			_leaves.Insert(0, leaf);
			createRoomInLeaf(leaf);
		}

        if (this.VisualizeSplitting)
        {
            applyOriginalMap();
            visualizeNodes(true);
        }
    }

	public void PostRoomCreationVisualizationPhase(int frames)
	{
		this.NextPhase();
		applyOriginalMap();
		visualizeNodes(false);
	}

	public void RoomConnectionSetupPhase(int frames)
	{
		_nodeQueue.Add(_root);

		while (_nodeQueue.Count > 0)
		{
			BSPNode node = _nodeQueue[0];
			_nodeQueue.RemoveAt(0);

			if (node.Children != null)
			{
				_nodeQueue.Add(node.Children[0]);
				_nodeQueue.Add(node.Children[1]);
				_nodeList.Add(node.Children[0]);
				_nodeList.Add(node.Children[1]);
			}
		}

		this.NextPhase();
	}

    public void RoomConnectionPhase(int frames)
    {
		for (int i = 0; i < frames; ++i)
		{
			if (_nodeList.Count <= 1)
            {
                _numExtraCorridorsToMake = Mathf.RoundToInt(_rooms.Count * this.ExtraCorridorsPerRoom);
                _leaves.Shuffle();
				this.NextPhase();
				break;
			}

			BSPNode node1 = _nodeList[_nodeList.Count - 1];
			BSPNode node2 = _nodeList[_nodeList.Count - 2];
			_nodeList.RemoveRange(_nodeList.Count - 2, 2);
			
			if (Random.Range(0, 2) == 0)
				joinNodes(node1, node2, true);
			else
				joinNodes(node2, node1, true);
		}
    }

	public void ExtraRoomConnectionPhase(int frames)
	{
        for (int i = 0; i < frames; ++i)
        {
            if (_numExtraCorridors >= _numExtraCorridorsToMake || _numExtraCorridors * 2 + 1 >= _leaves.Count)
            {
                this.NextPhase();
                break;
            }

            // Grab two random leaves (TODO - fcole - that aren't siblings)
            BSPNode leaf1 = _leaves[_numExtraCorridors * 2];
            BSPNode leaf2 = _leaves[_numExtraCorridors * 2 + 1];
            joinNodes(leaf1, leaf2, false);
            ++_numExtraCorridors;
        }
	}

	/**
	 * Private
	 */
    private BSPNode _root;
	private List<BSPNode> _nodeQueue;
	private List<BSPNode> _nodeList;
	private List<BSPNode> _leaves;
    private LevelGenMap.TileType[,] _originalMap;
	private List<Rect> _rooms;
	private List<List<LevelGenMap.Coordinate>> _corridorTiles;
	private int _numRoomsToMake;
    private int _numExtraCorridorsToMake;
    private int _numExtraCorridors;

	private class BSPNode
	{
		public Rect Bounds;
		public BSPNode[] Children;
        public List<LevelGenMap.Coordinate> CarvedArea;

		public List<LevelGenMap.Coordinate> GetTotalCarvedArea()
		{
			if (this.Children != null)
			{
				List<LevelGenMap.Coordinate> totalCarvedArea = new List<LevelGenMap.Coordinate>();
				totalCarvedArea.AddRange(this.CarvedArea);
                totalCarvedArea.AddRange(this.Children[0].GetTotalCarvedArea());
                totalCarvedArea.AddRange(this.Children[1].GetTotalCarvedArea());
				return totalCarvedArea;
			}
			else
			{
				return this.CarvedArea;
			}
		}
	}

    private BSPNode createNode(Rect bounds)
    {
        BSPNode node = new BSPNode();
        node.Bounds = bounds;
        node.Children = null;
        node.CarvedArea = new List<LevelGenMap.Coordinate>();
        return node;
    }

	private void createRoomInLeaf(BSPNode leaf)
	{
		int width = Random.Range(this.RoomMinSize, this.RoomMaxSize + 1);
		int height = Random.Range(this.RoomMinSize, this.RoomMaxSize + 1);
		if (width > leaf.Bounds.IntWidth())
			width = leaf.Bounds.IntWidth();
		if (height > leaf.Bounds.IntHeight())
			height = leaf.Bounds.IntHeight();

		int leftX = Random.Range(leaf.Bounds.IntXMin(), leaf.Bounds.IntXMin() + (leaf.Bounds.IntWidth() - width) + 1);
		int topY = Random.Range(leaf.Bounds.IntYMin(), leaf.Bounds.IntYMin() + (leaf.Bounds.IntHeight() - height) + 1);

		Rect roomRect = new Rect(leftX, topY, width, height);
		this.Map.FillRect(roomRect, this.FillTileType);
		leaf.CarvedArea.AddRange(this.Map.CoordinatesInRect(roomRect));
		_rooms.Add(roomRect);
	}

	private void joinNodes(BSPNode node1, BSPNode node2, bool checkPathFind)
    {
        List<LevelGenMap.Coordinate> carvedArea1 = node1.GetTotalCarvedArea();
        List<LevelGenMap.Coordinate> carvedArea2 = node2.GetTotalCarvedArea();

        if (carvedArea1.Count == 0 || carvedArea2.Count == 0)
            return;

        // Depending on param, only connect nodes at this point if cannot already pathfind between them.
        if (checkPathFind && this.Map.CanPathBetweenCoordinates(carvedArea1[0], carvedArea2[0]))
            return;

        carvedArea1.Shuffle();
        carvedArea2.Shuffle();
		bool leftToRight = node1.Bounds.center.x < node2.Bounds.center.x;
        bool topToBottom = node1.Bounds.center.y < node2.Bounds.center.y;
		LevelGenMap.Coordinate? bestCoord1 = null;
		LevelGenMap.Coordinate? bestCoord2 = null;

        // Greater x distance
        if (Mathf.Abs(node1.Bounds.center.x - node2.Bounds.center.x) > Mathf.Abs(node1.Bounds.center.y - node2.Bounds.center.y))
        {
            // Check if there is a shared y-coordinate in carved area
            foreach (LevelGenMap.Coordinate coord1 in carvedArea1)
            {
                if (bestCoord1.HasValue)
                {
                    if (coord1.y == bestCoord1.Value.y &&
                        ((leftToRight && (coord1.x > bestCoord1.Value.x)) ||
                        (!leftToRight && (coord1.x < bestCoord1.Value.x))))
                    {
                        bestCoord1 = coord1;
                    }
                }
                else if (coord1.y >= node2.Bounds.IntYMin() && coord1.y < node2.Bounds.IntYMax())
                {
                    foreach (LevelGenMap.Coordinate coord2 in carvedArea2)
                    {
                        if (bestCoord2.HasValue)
                        {
                            if (coord2.y == bestCoord2.Value.y &&
                                ((leftToRight && (coord2.x < bestCoord2.Value.x)) ||
                                (!leftToRight && (coord2.x > bestCoord2.Value.x))))
                            {
                                bestCoord2 = coord2;
                            }
                        }
                        else if (coord1.y == coord2.y)
                        {
                            bestCoord1 = coord1;
                            bestCoord2 = coord2;
                        }
                    }
                }
            }
        }

        // Greater y distance
        else
        {
            // Check if there is a shared x-coordinate in carved area
            foreach (LevelGenMap.Coordinate coord1 in carvedArea1)
            {
                if (bestCoord1.HasValue)
                {
                    if (coord1.x == bestCoord1.Value.x &&
                        ((topToBottom && (coord1.y > bestCoord1.Value.y)) ||
                        (!topToBottom && (coord1.y < bestCoord1.Value.y))))
                    {
                        bestCoord1 = coord1;
                    }
                }
                else if (coord1.x >= node2.Bounds.IntXMin() && coord1.x < node2.Bounds.IntXMax())
                {
                    foreach (LevelGenMap.Coordinate coord2 in carvedArea2)
                    {
                        if (bestCoord2.HasValue)
                        {
                            if (coord2.x == bestCoord2.Value.x &&
                                ((topToBottom && (coord2.y < bestCoord2.Value.y)) ||
                                (!topToBottom && (coord2.y > bestCoord2.Value.y))))
                            {
                                bestCoord2 = coord2;
                            }
                        }
                        else if (coord1.x == coord2.x)
                        {
                            bestCoord1 = coord1;
                            bestCoord2 = coord2;
                        }
                    }
                }
            }
        }

        // Did we find an ideal (straight-line) connection?
        if (!bestCoord1.HasValue)
        {
            // Otherwise, pick a random y in node1 and and random x in node2.
            // Pick closest tile in node1 with that y value to left or right (direction of node2).
            // Pick closest tile in node2 with that x value to up or down (direction of node1).
            foreach (LevelGenMap.Coordinate coord in carvedArea1)
            {
                if (!bestCoord1.HasValue || (coord.y == bestCoord1.Value.y &&
                    ((leftToRight && coord.x > bestCoord1.Value.x) ||
                    (!leftToRight && coord.x < bestCoord1.Value.x))))
                {
                    bestCoord1 = coord;
                }
            }

            foreach (LevelGenMap.Coordinate coord in carvedArea2)
            {
                if (!bestCoord2.HasValue || (coord.x == bestCoord2.Value.x &&
                    ((topToBottom && coord.y < bestCoord2.Value.y) ||
                    (!topToBottom && coord.y > bestCoord2.Value.y))))
                {
                    bestCoord2 = coord;
                }
            }
        }

        // Connect the points
        if (bestCoord1.HasValue && bestCoord2.HasValue)
            tracePathBetweenCoordinates(bestCoord1.Value, bestCoord2.Value);
        else
            Debug.Log("Didn't find coords to connect");

	}

    private BSPNode[] splitNode(BSPNode parent)
    {
        bool tooThin = parent.Bounds.width <= this.MinLeafSize * 2;
        bool tooShort = parent.Bounds.height <= this.MinLeafSize * 2;

        if (tooThin && tooShort)
            return null;

        // Decide direction split the node
        bool verticalSplit;
        if (tooShort|| (parent.Bounds.width > parent.Bounds.height && parent.Bounds.height / parent.Bounds.width < this.MinNodeWHRatio))
        {
            verticalSplit = true;
        }
        else if (tooThin || (parent.Bounds.height > parent.Bounds.width && parent.Bounds.width / parent.Bounds.height < this.MinNodeWHRatio))
        {
            verticalSplit = false;
        }
        else
        {
            verticalSplit = Random.Range(0, 2) == 0 ? false : true;
        }
        
        // Find the point at which to cut the node and create the children
        BSPNode[] nodes = new BSPNode[2];
        int divider = 0;

        if (verticalSplit)
        {
            divider = Random.Range(parent.Bounds.IntXMin() + this.MinLeafSize, parent.Bounds.IntXMax() - 1 - this.MinLeafSize);

            nodes[0] = createNode(new Rect(parent.Bounds.xMin, parent.Bounds.yMin, divider - parent.Bounds.IntXMin(), parent.Bounds.height));
            nodes[1] = createNode(new Rect(nodes[0].Bounds.xMax, nodes[0].Bounds.yMin, parent.Bounds.IntXMax() - nodes[0].Bounds.xMax, parent.Bounds.height));
        }
        else
        {
            divider = Random.Range(parent.Bounds.IntYMin() + this.MinLeafSize, parent.Bounds.IntYMax() - 1 - this.MinLeafSize);

            nodes[0] = createNode(new Rect(parent.Bounds.xMin, parent.Bounds.yMin, parent.Bounds.width, divider - parent.Bounds.IntYMin()));
            nodes[1] = createNode(new Rect(nodes[0].Bounds.xMin, nodes[0].Bounds.yMax, parent.Bounds.width, parent.Bounds.IntYMax() - nodes[0].Bounds.yMax));
        }
        return nodes;
    }

	private void tracePathBetweenCoordinates(LevelGenMap.Coordinate coord1, LevelGenMap.Coordinate coord2)
	{
        int modifier = 1;

        List<LevelGenMap.Coordinate> corridor = new List<LevelGenMap.Coordinate>();

        if (coord1.x != coord2.x)
        {
            int startX = coord1.x < coord2.x ? coord1.x + modifier : coord2.x + modifier;
            int endX = coord1.x < coord2.x ? coord2.x : coord1.x;

            for (int x = startX; x < endX; ++x)
            {
                fillCorridorTile(x, coord2.y, corridor);
            }

            modifier = 0;
        }

        if (coord1.y != coord2.y)
        {
            int startY = coord1.y < coord2.y ? coord1.y + modifier : coord2.y + modifier;
            int endY = coord1.y < coord2.y ? coord2.y + (1 - modifier) : coord1.y + (1 - modifier);

            for (int y = startY; y < endY; ++y)
            {
                fillCorridorTile(coord1.x, y, corridor);
            }
        }

        _corridorTiles.Add(corridor);
	}

	private void fillCorridorTile(int x, int y, List<LevelGenMap.Coordinate> cooridor)
	{
		LevelGenMap.Coordinate? coord = this.Map.ConstructValidCoordinate(x, y, false);
		if (!coord.HasValue)
			return;

        bool alreadyContains = false;

		foreach (BSPNode leaf in _leaves)
		{
			if (x >= leaf.Bounds.IntXMin() && x < leaf.Bounds.IntXMax() && y >= leaf.Bounds.IntYMin() && y < leaf.Bounds.IntYMax())
			{
                alreadyContains = leaf.CarvedArea.Contains(coord.Value);
                if (!alreadyContains)
				    leaf.CarvedArea.Add(coord.Value);
				break;
			}
		}

        if (!alreadyContains)
        {
            this.Map.Grid[x, y] = this.FillTileType;
            cooridor.Add(coord.Value);
        }
        else
        {
            this.Map.Grid[coord.Value.x, coord.Value.y] = this.DebugSecondFillTileType;
        }
	}

    private void applyOriginalMap()
    {
        for (int x = 0; x < this.Bounds.IntWidth(); ++x)
        {
            for (int y = 0; y < this.Bounds.IntHeight(); ++y)
            {
                this.Map.Grid[x + this.Bounds.IntXMin(), y + this.Bounds.IntYMin()] = _originalMap[x, y];
            }
        }
    }

    private void visualizeNodes(bool includeLeafOutlines)
    {
        List<BSPNode> nodeStack = new List<BSPNode>();
        nodeStack.Add(_root);
        
        while (nodeStack.Count > 0)
        {
            BSPNode current = nodeStack[0];
            nodeStack.RemoveAt(0);

			visualizeNode(current, includeLeafOutlines);

            if (current.Children != null)
            {
                nodeStack.Add(current.Children[0]);
                nodeStack.Add(current.Children[1]);
            }
        }
    }

	private void visualizeNode(BSPNode node, bool includeLeafOutlines)
    {
		// Visualize the carved area in the node
		foreach (LevelGenMap.Coordinate coord in node.CarvedArea)
		{
			this.Map.Grid[coord.x, coord.y] = this.FillTileType;
		}

		// Visualize leaf outlines if desired
		if (!includeLeafOutlines || node.Children == null)
            return;

        if (node.Children[0].Bounds.width == node.Bounds.width)
        {
            // Horizontal line
            for (int x = node.Children[0].Bounds.IntXMin(); x < node.Children[0].Bounds.IntXMax(); ++x)
            {
                this.Map.Grid[x, node.Children[0].Bounds.IntYMax()] = this.FillTileType;
            }
        }
        else
        {
            // Vertical line
            for (int y = node.Children[0].Bounds.IntYMin(); y < node.Children[0].Bounds.IntYMax(); ++y)
            {
                this.Map.Grid[node.Children[0].Bounds.IntXMax(), y] = this.FillTileType;
            }
        }
    }
}
