using UnityEngine;

public static class VectorExtensions
{
    [System.Serializable]
    public enum Direction16
    {
        None,
        Up,
        UpUpRight,
        UpRight,
        UpRightRight,
        Right,
        DownRightRight,
        DownRight,
        DownDownRight,
        Down,
        DownDownLeft,
        DownLeft,
        DownLeftLeft,
        Left,
        UpLeftLeft,
        UpLeft,
        UpUpLeft
    }

    public static int IntX(this Vector2 self) { return Mathf.RoundToInt(self.x); }
    public static int IntY(this Vector2 self) { return Mathf.RoundToInt(self.y); }

    public static Vector2 VectorAtAngle(this Vector2 self, float angle)
    {
        float ourAngle = Vector2.Angle(Vector2.up, self);
        if (self.x < 0.0f)
            ourAngle = -ourAngle;
        float rad = Mathf.Deg2Rad * (ourAngle + angle);
        return new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)).normalized;
    }

    public static Vector2 EaseTowards(this Vector2 self, Vector2 destination, float t, float duration, Easing.EasingDelegate function)
    {
        float totalDistance = (destination - self).magnitude;
        float newDistance = function(t, 0.0f, totalDistance, duration);
        return Vector2.MoveTowards(self, destination, newDistance);
    }

    public static Vector2 Normalized16(this Vector2 self)
    {
        int xFactor = 1;
        int yFactor = 1;
        float absX = Mathf.Abs(self.x);
        float absY = Mathf.Abs(self.y);

        //TODO - Check accuracy of this math
        if (absX > absY)
        {
            if (absX >= absY * 2.5f)
                yFactor = 0;
            else if (absX >= absY * 1.5f)
                xFactor = 2;
        }
        else if (absY > absX)
        {
            if (absY >= absX * 2.5f)
                xFactor = 0;
            else if (absY >= absX * 1.5f)
                yFactor = 2;
        }

        return new Vector2(xFactor * Mathf.RoundToInt(Mathf.Sign(self.x)), yFactor * Mathf.RoundToInt(Mathf.Sign(self.y))).normalized;
    }

    public static Vector2 VectorFromDirection16(Direction16 dir)
    {
        switch (dir)
        {
            default:
            case Direction16.None:
                return Vector2.zero;
            case Direction16.Up:
                return Vector2.up;
            case Direction16.UpUpRight:
                return new Vector2(1, 2).normalized;
            case Direction16.UpRight:
                return new Vector2(1, 1).normalized;
            case Direction16.UpRightRight:
                return new Vector2(2, 1).normalized;
            case Direction16.Right:
                return Vector2.right;
            case Direction16.DownRightRight:
                return new Vector2(2, -1).normalized;
            case Direction16.DownRight:
                return new Vector2(1, -1).normalized;
            case Direction16.DownDownRight:
                return new Vector2(1, -2).normalized;
            case Direction16.Down:
                return Vector2.down;
            case Direction16.DownDownLeft:
                return new Vector2(-1, -2).normalized;
            case Direction16.DownLeft:
                return new Vector2(-1, -1).normalized;
            case Direction16.DownLeftLeft:
                return new Vector2(-2, -1).normalized;
            case Direction16.Left:
                return Vector2.left;
            case Direction16.UpLeftLeft:
                return new Vector2(-2, 1).normalized;
            case Direction16.UpLeft:
                return new Vector2(-1, 1).normalized;
            case Direction16.UpUpLeft:
                return new Vector2(-1, 2).normalized;
        }
    }
}
