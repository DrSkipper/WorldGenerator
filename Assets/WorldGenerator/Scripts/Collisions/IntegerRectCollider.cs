using UnityEngine;
using System.Collections.Generic;
using System;

//TODO - Take into account rotation in Overlaps and Collides
public class IntegerRectCollider : IntegerCollider
{
    public IntegerVector Size;
    public override IntegerRect Bounds { get { return new IntegerRect(this.integerPosition + this.Offset, this.Size); } }

    void OnDrawGizmosSelected()
    {
        if (this.enabled)
        {
            IntegerRect bounds = this.Bounds;
            Gizmos.color = this.DebugColor;
            Gizmos.DrawWireCube(new Vector3(bounds.Center.X, bounds.Center.Y), new Vector3(this.Size.X, this.Size.Y));
        }
    }

    //TODO - use dynamic keyword to make use of run-time overloading? and have an extensions method place for inter-collider type collisions?
    //http://stackoverflow.com/questions/13095544/overloaded-method-why-is-base-class-given-precedence#comment25529590_13096565
    public override bool Overlaps(IntegerCollider other, int offsetX = 0, int offsetY = 0)
    {
        if (other.GetType() == typeof(IntegerCircleCollider))
            return other.Overlaps(this, -offsetX, -offsetY);
        return base.Overlaps(other, offsetX, offsetY);
    }
}
