using UnityEngine;
using System.Collections.Generic;

public static class Texture2DExtensions
{
    public static Dictionary<string, Sprite> GetSprites(this Texture2D self)
    {
        Dictionary<string, Sprite> spriteDictionary = new Dictionary<string, Sprite>();
        Sprite[] spriteArray = self.GetSpritesArray();

        foreach (Sprite sprite in spriteArray)
        {
            spriteDictionary[sprite.name] = sprite;
        }

        return spriteDictionary;
    }

    public static Sprite[] GetSpritesArray(this Texture2D self)
    {
        return Resources.LoadAll<Sprite>(self.name);
    }

    public static Vector2[] GetUVs(this Sprite self)
    {
        float minX = self.rect.xMin / self.texture.width;
        float minY = self.rect.yMin / self.texture.height;
        float maxX = self.rect.xMax / self.texture.width;
        float maxY = self.rect.yMax / self.texture.height;
        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(minX, minY);
        uvs[1] = new Vector2(maxX, minY);
        uvs[2] = new Vector2(minX, maxY);
        uvs[3] = new Vector2(maxX, maxY);
        return uvs;
    }
}
