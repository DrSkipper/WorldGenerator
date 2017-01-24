using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileTextureAtlas
{
    public int TileWidth { get; private set; }
    public int TileHeight { get; private set; }
    public int NumTiles { get; private set; }

    public TileTextureAtlas(Texture2D texture, int tileWidth, int tileHeight)
    {
        _texture = texture;
        this.TileWidth = tileWidth;
        this.TileHeight = tileHeight;

        _columns = texture.width / tileWidth;
        _rows = texture.height / tileHeight;
        this.NumTiles = _rows * _columns;
    }

    public Vector2[] GetUVsForSprite(int spriteIndex)
    {
        if (_tileUVData == null)
            gatherTileUVData();
        return _tileUVData[spriteIndex];
    }

    public Color[] GetPixelsForSprite(int spriteIndex)
    {
        if (_tilePixelData == null)
            gatherTilePixelData();
        return _tilePixelData[spriteIndex];
    }

    public void ClearCachedTextureData()
    {
        _tileUVData = null;
        _tilePixelData = null;
    }

    /**
     * Private
     */
    private Texture2D _texture;
    private int _columns;
    private int _rows;
    private List<Vector2[]> _tileUVData;
    private List<Color[]> _tilePixelData;

    private void gatherTileUVData()
    {
        _tileUVData = new List<Vector2[]>();
        float lastX = _texture.width;
        float lastY = _texture.height;

        for (int y = 0; y < _texture.height; y += this.TileHeight)
        {
            for (int x = 0; x < _texture.width; x += this.TileWidth)
            {
                Vector2[] spriteUVs = new Vector2[4];
                spriteUVs[0] = new Vector2((float)x / lastX, (float)y / lastY); // bottom left
                spriteUVs[1] = new Vector2((float)(x + this.TileWidth) / lastX, spriteUVs[0].y); // bottom right
                spriteUVs[2] = new Vector2(spriteUVs[0].x, (float)(y + this.TileHeight) / lastY); // top left
                spriteUVs[3] = new Vector2(spriteUVs[1].x, spriteUVs[2].y); // top right
                _tileUVData.Add(spriteUVs);
            }
        }
    }

    private void gatherTilePixelData()
    {
        _tilePixelData = new List<Color[]>();
        for (int y = 0; y < _texture.height; y += this.TileHeight)
        {
            for (int x = 0; x < _texture.width; x += this.TileWidth)
            {
                _tilePixelData.Add(_texture.GetPixels(x, y, this.TileWidth, this.TileHeight));
            }
        }
    }
}
