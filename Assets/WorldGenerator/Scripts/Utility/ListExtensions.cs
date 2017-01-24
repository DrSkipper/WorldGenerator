using UnityEngine;
using System.Collections.Generic;

public static class ListExtensions
{
    public static void Shuffle<T>(this List<T> self)
    {
        for (int i = 0; i < self.Count; i++)
        {
            int randomIndex = Random.Range(i, self.Count);
            T temp = self[i];
            self[i] = self[randomIndex];
            self[randomIndex] = temp;
        }
    }

    public static void AddUnique<T>(this List<T> self, T toAdd)
    {
        if (!self.Contains(toAdd))
            self.Add(toAdd);
    }

    public static T Pop<T>(this List<T> self)
    {
        if (self.Count > 0)
        {
            int last = self.Count - 1;
            T retVal = self[last];
            self.RemoveAt(last);
            return retVal;
        }
        return default(T);
    }
}
