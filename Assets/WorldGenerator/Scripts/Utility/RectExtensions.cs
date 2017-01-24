using UnityEngine;
using System.Collections.Generic;

public static class RectExtensions
{
	public static bool Intersects(this Rect self, Rect other)
	{
		return (self.xMin <= other.xMax && self.xMax >= other.xMin && self.yMin <= other.yMax && self.yMax >= other.yMin);
	}

    public static int IntX(this Rect self) { return Mathf.RoundToInt(self.x); }
    public static int IntY(this Rect self) { return Mathf.RoundToInt(self.y); }
    public static int IntWidth(this Rect self) { return Mathf.RoundToInt(self.width); }
    public static int IntHeight(this Rect self) { return Mathf.RoundToInt(self.height); }
    public static int IntXMin(this Rect self) { return Mathf.RoundToInt(self.xMin); }
    public static int IntXMax(this Rect self) { return Mathf.RoundToInt(self.xMax); }
    public static int IntYMin(this Rect self) { return Mathf.RoundToInt(self.yMin); }
    public static int IntYMax(this Rect self) { return Mathf.RoundToInt(self.yMax); }
}

// Json-serializable rect
public struct SimpleRect
{
    public int X;
    public int Y;
    public int Width;
    public int Height;

    public SimpleRect(int x, int y, int width, int height)
    {
        this.X = x;
        this.Y = y;
        this.Width = width;
        this.Height = height;
    }

    public static SimpleRect RectToSimpleRect(Rect rect)
    {
        return new SimpleRect(rect.IntXMin(), rect.IntYMin(), rect.IntWidth(), rect.IntHeight());
    }

    public static Rect SimpleRectToRect(SimpleRect rect)
    {
        return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static List<SimpleRect> RectListToSimpleRectList(List<Rect> rects)
    {
        List<SimpleRect> simpleRects = new List<SimpleRect>();
        foreach (Rect rect in rects)
        {
            simpleRects.Add(RectToSimpleRect(rect));
        }
        return simpleRects;
    }

    public static List<Rect> SimpleRectListToRectList(List<SimpleRect> simpleRects)
    {
        List<Rect> rects = new List<Rect>();
        foreach (SimpleRect rect in simpleRects)
        {
            rects.Add(SimpleRectToRect(rect));
        }
        return rects;
    }
}
