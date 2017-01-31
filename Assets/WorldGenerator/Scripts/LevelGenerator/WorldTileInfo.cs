using System.Collections.Generic;

public class WorldTileInfo
{
    public enum TileType
    {
        Plains,
        Hills,
        Mountains,
        Water,
        Desert
    }

    public enum TileTrait
    {
        Forest,
        MineralVein,
        City
    }

    public TileType GetTileType() { return (TileType)this.Type; }
    public void SetTileType(TileType type) { this.Type = (int)type; }
    public int NumTraits { get { return this.Traits == null ? 0 : this.Traits.Count; } }

    public TileTrait GetTileTrait(int index)
    {
        return (TileTrait)this.Traits[index];
    }

    public bool HasTrait(TileTrait trait)
    {
        for (int i = 0; i < this.NumTraits; ++i)
        {
            if (this.GetTileTrait(i) == trait)
                return true;
        }
        return false;
    }

    public void AddTileTrait(TileTrait trait)
    {
        if (this.Traits == null)
            this.Traits = new List<int>();
        if (!this.HasTrait(trait))
            this.Traits.Add((int)trait);
    }

    public void RemoveTileTrait(TileTrait trait)
    {
        for (int i = 0; i < this.NumTraits; ++i)
        {
            if (this.GetTileTrait(i) == trait)
            {
                this.Traits.RemoveAt(i);
                break;
            }
        }
    }

    /**
     * Data
     */
    public int Type;
    public List<int> Traits;
}
