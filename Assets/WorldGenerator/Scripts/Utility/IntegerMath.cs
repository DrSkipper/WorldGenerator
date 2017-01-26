using UnityEngine;
using System;

[System.Serializable]
public struct IntegerRect
{
    public IntegerVector Center;
    public IntegerVector Size;
    public IntegerVector Extents { get { return this.Size / 2; } }

    public IntegerVector Min
    {
        get { return this.Center - this.Extents; }
        set
        {
            IntegerVector newCenter = (value + this.Max) / 2;
            this.Size = this.Max - value;
            this.Center = newCenter;
        }
    }

    public IntegerVector Max
    {
        get { return this.Center + this.Extents; }
        set
        {
            IntegerVector newCenter = (this.Min + value) / 2;
            this.Size = value - this.Min;
            this.Center = newCenter;
        }
    }

    public IntegerRect(int centerX = 0, int centerY = 0, int sizeX = 0, int sizeY = 0)
    {
        this.Center = new IntegerVector(centerX, centerY);
        this.Size = new IntegerVector(sizeX, sizeY);
    }

    public IntegerRect(IntegerVector center, IntegerVector size)
    {
        this.Center = center;
        this.Size = size;
    }

    public static IntegerRect ConstructRectFromMinAndSize(IntegerVector min, IntegerVector size)
    {
        IntegerRect rect = new IntegerRect();
        rect.Center = min + size / 2;
        rect.Size = size;
        return rect;
    }

    public static IntegerRect ConstructRectFromMinAndSize(int minX, int minY, int sizeX, int sizeY)
    {
        return ConstructRectFromMinAndSize(new IntegerVector(minX, minY), new IntegerVector(sizeX, sizeY));
    }

    public bool Overlaps(IntegerRect other)
    {
        IntegerVector selfMin = this.Min;
        IntegerVector selfMax = this.Max;
        IntegerVector otherMin = other.Min;
        IntegerVector otherMax = other.Max;

        return ((otherMin.X >= selfMin.X && otherMin.X < selfMax.X) || (otherMax.X > selfMin.X && otherMax.X <= selfMax.X) ||
                (selfMin.X >= otherMin.X && selfMin.X < otherMax.X) || (selfMax.X > otherMin.X && selfMax.X <= otherMax.X)) &&
               ((otherMin.Y >= selfMin.Y && otherMin.Y < selfMax.Y) || (otherMax.Y > selfMin.Y && otherMax.Y <= selfMax.Y) ||
                (selfMin.Y >= otherMin.Y && selfMin.Y < otherMax.Y) || (selfMax.Y > otherMin.Y && selfMax.Y <= otherMax.Y));
    }

    public bool Contains(IntegerVector point)
    {
        IntegerVector selfMin = this.Min;
        IntegerVector selfMax = this.Max;
        return point.X >= selfMin.X && point.X <= selfMax.X && point.Y >= selfMin.Y && point.Y <= selfMax.Y;
    }

    //http://stackoverflow.com/questions/20453545/how-to-find-the-nearest-point-in-the-perimeter-of-a-rectangle-to-a-given-point
    public IntegerVector ClosestContainedPoint(IntegerVector point)
    {
        if (this.Contains(point))
            return point;

        IntegerVector selfMin = this.Min;
        IntegerVector selfMax = this.Max;
        int clampedX = Mathf.Clamp(point.X, selfMin.X, selfMax.X);
        int clampedY = Mathf.Clamp(point.Y, selfMin.Y, selfMax.Y);

        int dl = Math.Abs(selfMin.X - clampedX);
        int dr = Math.Abs(clampedX - selfMax.X);
        int db = Math.Abs(selfMin.Y - clampedY);
        int dt = Math.Abs(clampedY - selfMax.Y);

        int min = Mathf.Min(new int[] { dl, dr, db, dt });
        if (min == db) return new IntegerVector(clampedX, selfMin.Y);
        if (min == dt) return new IntegerVector(clampedX, selfMax.Y);
        if (min == dl) return new IntegerVector(selfMin.X, clampedY);
        return new IntegerVector(selfMax.X, clampedY);
    }
}

[System.Serializable]
public struct IntegerVector
{
    public int X;
    public int Y;

    public IntegerVector(int x = 0, int y = 0)
    {
        this.X = x;
        this.Y = y;
    }

    public IntegerVector(Vector2 v)
    {
        this.X = Mathf.RoundToInt(v.x);
        this.Y = Mathf.RoundToInt(v.y);
    }

    public static IntegerVector Zero { get { return new IntegerVector(); } }

    public static IntegerVector Min(IntegerVector v1, IntegerVector v2)
    {
        return new IntegerVector(v1.X < v2.X ? v1.X : v2.X, v1.Y < v2.Y ? v1.Y : v2.Y);
    }

    public static IntegerVector Max(IntegerVector v1, IntegerVector v2)
    {
        return new IntegerVector(v1.X > v2.X ? v1.X : v2.X, v1.Y > v2.Y ? v1.Y : v2.Y);
    }

    public static IntegerVector Clamp(IntegerVector v, IntegerVector min, IntegerVector max)
    {
        return new IntegerVector(Mathf.Clamp(v.X, min.X, max.X), Mathf.Clamp(v.Y, min.Y, max.Y));
    }

    public static IntegerVector operator +(IntegerVector v1, IntegerVector v2)
    {
        return new IntegerVector(v1.X + v2.X, v1.Y + v2.Y);
    }

    public static IntegerVector operator -(IntegerVector v1, IntegerVector v2)
    {
        return new IntegerVector(v1.X - v2.X, v1.Y - v2.Y);
    }

    public static IntegerVector operator *(IntegerVector v, int i)
    {
        return new IntegerVector(v.X * i, v.Y * i);
    }

    public static IntegerVector operator /(IntegerVector v, int i)
    {
        return new IntegerVector(v.X / i, v.Y / i);
    }

    public static implicit operator Vector2(IntegerVector v)
    {
        return new Vector2(v.X, v.Y);
    }

    public static implicit operator IntegerVector(Vector2 v)
    {
        return new IntegerVector(v);
    }

    public static bool operator ==(IntegerVector v1, IntegerVector v2)
    {
        return v1.X == v2.X && v1.Y == v2.Y;
    }

    public static bool operator !=(IntegerVector v1, IntegerVector v2)
    {
        return !(v1 == v2);
    }

    public override bool Equals(object obj)
    {
        if (obj is IntegerVector)
            return this == (IntegerVector)obj;
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
