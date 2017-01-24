using UnityEngine;

public static class TilingHelper
{
    public static string[,] GetTileTypesForGrid(int[,] grid, bool offMapIsFilled = true)
    {
        string[,] tileTypes = new string[grid.GetLength(0), grid.GetLength(1)];

        for (int x = 0; x < tileTypes.GetLength(0); ++x)
        {
            for (int y = 0; y < tileTypes.GetLength(1); ++y)
            {
                tileTypes[x, y] = GetTileType(GetNeighbors(grid, x, y, offMapIsFilled));
            }
        }

        return tileTypes;
    }
    
    public static int[,] GetNeighbors(int[,] grid, int x, int y, bool offMapIsFilled = true)
    {
        int[,] neighbors = new int[3, 3];
        int offscreenValue = offMapIsFilled ? 1 : 0;

        int maxX = grid.GetLength(0) - 1;
        int maxY = grid.GetLength(1) - 1;

        neighbors[0, 0] = x > 0 && y > 0 ? grid[x - 1, y - 1] : offscreenValue; // bottom left
        neighbors[1, 0] = y > 0 ? grid[x, y - 1] : offscreenValue; // bottom
        neighbors[2, 0] = x < maxX && y > 0 ? grid[x + 1, y - 1] : offscreenValue; // bottom right
        neighbors[0, 1] = x > 0 ? grid[x - 1, y] : offscreenValue; // left
        neighbors[1, 1] = grid[x, y]; // center
        neighbors[2, 1] = x < maxX ? grid[x + 1, y] : offscreenValue; // right
        neighbors[0, 2] = x > 0 && y < maxY ? grid[x - 1, y + 1] : offscreenValue; // top left
        neighbors[1, 2] = y < maxY ? grid[x, y + 1] : offscreenValue; // top
        neighbors[2, 2] = x < maxX && y < maxY ? grid[x + 1, y + 1] : offscreenValue; // top right

        return neighbors;
    }

    public static string GetTileType(int[,] neighbors)
    {
        // Empty tile
        if (neighbors[1, 1] == 0)
            return "empty";

        bool bottom = neighbors[1, 0] != 0;
        bool left = neighbors[0, 1] != 0;
        bool top = neighbors[1, 2] != 0;
        bool right = neighbors[2, 1] != 0;
        bool anyCornerEmpty = neighbors[0, 0] == 0 || neighbors[0, 2] == 0 || neighbors[2, 0] == 0 || neighbors[2, 2] == 0;

        // Completely surrounded
        if (!anyCornerEmpty && bottom && left && top && right)
            return "filled";

        // Cross intersection
        if (anyCornerEmpty && bottom && top && left && right)
        {
            if ((neighbors[0, 0] == 0 && neighbors[2, 2] == 0) || (neighbors[2, 0] == 0 && neighbors[0, 2] == 0))
                return "cross";
        }

        // Corners and T-intersections
        if (bottom && left)
        {
            if (!top && !right && neighbors[0, 0] != 0)
            {
                return "corner_bottom_right";
            }
            else if (!top && right && (neighbors[0, 0] == 0 || neighbors[2, 0] == 0))
            {
                return "t_up";
            }
            else if (!right && top && (neighbors[0, 0] == 0 || neighbors[0, 2] == 0))
            {
                return "t_left";
            }
            else
            {
                if (right && neighbors[0, 0] == 0 && neighbors[2, 0] == 0)
                    return "t_up";
                if (top && neighbors[0, 0] == 0 && neighbors[0, 2] == 0)
                    return "t_left";
                if (neighbors[0, 0] == 0)
                    return "corner_bottom_right";
            }
        }

        if (bottom && right)
        {
            if (!top && !left && neighbors[2, 0] != 0)
            {
                return "corner_bottom_left";
            }
            else if (!top && left && (neighbors[0, 0] == 0 || neighbors[2, 0] == 0))
            {
                return "t_up";
            }
            else if (!left && top && (neighbors[2, 0] == 0 || neighbors[2, 2] == 0))
            {
                return "t_right";
            }
            else
            {
                if (left && neighbors[0, 0] == 0 && neighbors[2, 0] == 0)
                    return "t_up";
                if (top && neighbors[2, 0] == 0 && neighbors[2, 2] == 0)
                    return "t_right";
                if (neighbors[2, 0] == 0)
                    return "corner_bottom_left";
            }
        }
        if (top && left)
        {
            if (!bottom && !right && neighbors[0, 2] != 0)
            {
                return "corner_top_right";
            }
            else if (!bottom && right && (neighbors[0, 2] == 0 || neighbors[2, 2] == 0))
            {
                return "t_down";
            }
            else if (!right && bottom && (neighbors[0, 0] == 0 || neighbors[0, 2] == 0))
            {
                return "t_left";
            }
            else
            {
                if (right && neighbors[0, 2] == 0 && neighbors[2, 2] == 0)
                    return "t_down";
                if (bottom && neighbors[0, 0] == 0 && neighbors[0, 2] == 0)
                    return "t_left";
                if (neighbors[0, 2] == 0)
                    return "corner_top_right";
            }
        }
        if (top && right)
        {
            if (!bottom && !left && neighbors[2, 2] != 0)
            {
                return "corner_top_left";
            }
            else if (!bottom && left && (neighbors[0, 2] == 0 || neighbors[2, 2] == 0))
            {
                return "t_down";
            }
            else if (!left && bottom && (neighbors[2, 0] == 0 || neighbors[2, 2] == 0))
            {
                return "t_right";
            }
            else
            {
                if (left && neighbors[0, 2] == 0 && neighbors[2, 2] == 0)
                    return "t_down";
                if (bottom && neighbors[2, 0] == 0 && neighbors[2, 2] == 0)
                    return "t_right";
                if (neighbors[2, 2] == 0)
                    return "corner_top_left";
            }
        }

        // Sides
        if (bottom && top)
            return "side_vertical";
        if (left && right)
            return "side_horizontal";

        // Tips
        if (bottom && !top && !left && !right)
            return "tip_bottom";
        if (!bottom && top && !left && !right)
            return "tip_top";
        if (!bottom && !top && left && !right)
            return "tip_right";
        if (!bottom && !top && !left && right)
            return "tip_left";

        // Lone tile
        //if (!bottom && !left && !top && !right)
        return "lone";
    }
}
