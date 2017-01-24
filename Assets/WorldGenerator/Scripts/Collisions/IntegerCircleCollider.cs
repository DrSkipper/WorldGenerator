using UnityEngine;

public class IntegerCircleCollider : IntegerCollider
{
    public int Radius = 1;
    public int Diameter { get { return this.Radius * 2; } }
    public override IntegerRect Bounds { get { return new IntegerRect(this.integerPosition + this.Offset, new IntegerVector(this.Diameter, this.Diameter)); } }

    void OnDrawGizmos()//Selected()
    {
        if (this.enabled)
        {
            IntegerRect bounds = this.Bounds;
            Gizmos.color = this.DebugColor;
            Gizmos.DrawWireSphere(new Vector3(bounds.Center.X, bounds.Center.Y), this.Radius);
        }
    }

    public override bool Overlaps(IntegerCollider other, int offsetX = 0, int offsetY = 0)
    {
        IntegerVector center = this.Bounds.Center;
        center.X += offsetX;
        center.Y += offsetY;
        return this.Contains(other.ClosestContainedPoint(this.Bounds.Center), offsetX, offsetY);
    }

    public override IntegerVector ClosestContainedPoint(IntegerVector point)
    {
        if (this.Contains(point))
            return point;

        IntegerVector center = this.Bounds.Center;
        IntegerVector difference = point - center;
        difference = new IntegerVector(((Vector2)difference).normalized * this.Radius);
        return center + difference;
    }

    public override bool Contains(IntegerVector point, int offsetX = 0, int offsetY = 0)
    {
        return Mathf.RoundToInt(Vector2.Distance(this.Bounds.Center, point)) <= this.Radius;
    }
}
