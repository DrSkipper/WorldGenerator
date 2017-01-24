using System.Collections.Generic;

public class MapGridSpaceInfo
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
    
    public int Type;
    public List<int> Traits;
    public int NumTraits { get { return this.Traits == null ? 0 : this.Traits.Count; } }

    public TileType GetTileType()
    {
        return (TileType)this.Type;
    }

    public void SetTileType(TileType type)
    {
        this.Type = (int)type;
    }

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

    public override bool Equals(object obj)
    {
        MapGridSpaceInfo other = obj as MapGridSpaceInfo;
        if (other == null)
            return false;
        if (other.Type != this.Type)
            return false;
        if (other.NumTraits != this.NumTraits)
            return false;
        for (int i = 0; i < this.NumTraits; ++i)
            if (!other.HasTrait(this.GetTileTrait(i)))
                return false;
        return true;
    }
}
