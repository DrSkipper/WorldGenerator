using UnityEngine;
using System;

public static class MathExtensions
{
    public const float PI_2 = 1.5707963f;
    public const float PI_4 = 0.7853982f;
    public const float PI_8 = 0.3926991f;

    public static float Approach(this float self, float target, float maxChange)
    {
        maxChange = Mathf.Abs(maxChange);
        return self <= target ? Mathf.Min(self + maxChange, target) : Mathf.Max(self - maxChange, target);
    }

    public static int Approach(this int self, int target, int maxChange)
    {
        maxChange = Math.Abs(maxChange);
        return self <= target ? Math.Min(self + maxChange, target) : Math.Max(self - maxChange, target);
    }

    public static uint Approach(this uint self, uint target, uint maxChange)
    {
        if (target < self)
        {
            if (self - target < maxChange)
                return target;
            return self - maxChange;
        }
        else if (target > self)
        {
            if (target - self < maxChange)
                return target;
            return self + maxChange;
        }
        return self;
    }

    public static Vector2 AngleToVector(float angleRadians, float length)
    {
        return new Vector2((float)Math.Cos((double)angleRadians) * length, (float)Math.Sin((double)angleRadians) * length);
    }
}
