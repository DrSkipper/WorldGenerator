using UnityEngine;
using System.Collections.Generic;

public static class ListExtensions
{
    public static void Shuffle<T>(this List<T> self)
    {
        for (int i = 0; i < self.Count; i++)
        {
            int randomIndex = Random.Range(i, self.Count);
            self.Swap(i, randomIndex);
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

    public static void Swap<T>(this List<T> self, int first, int second)
    {
        T temp = self[first];
        self[first] = self[second];
        self[second] = temp;
    }

    /**
     * Precondition: Only one occurrence of each element in list
     */
    public static void RemoveAllOptimized<T>(this List<T> self, List<T> toRemove)
    {
        int r, removed = 0;
        bool remove = false;
        for (int i = 0; i < self.Count - removed;)
        {
            remove = false;
            for (r = removed; r < toRemove.Count; ++r)
            {
                if (self[i].Equals(toRemove[r]))
                {
                    remove = true;
                    if (r != removed)
                        toRemove.Swap(r, removed);
                    ++removed;
                    break;
                }
            }

            if (remove)
            {
                self.Swap(i, self.Count - removed);
                if (removed == toRemove.Count)
                    break;
            }
            else
                ++i;
        }

        self.RemoveRange(self.Count - removed, removed);
    }
}
