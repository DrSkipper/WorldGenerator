using UnityEngine;
using System;
using System.Collections.Generic;

//NOTE - Solids ignore "tag", honestly should probably get rid of tag and just use layer??
public class CollisionManager : VoBehavior
{
    public LayerMask SolidsLayerMask;
    public int MaxSolidSize = 32;
    public int RaycastChunkSize = 32;

    public const float RAYCAST_MAX_POSITION_INCREMENT = 2.0f;
    public const int MAX_SOLIDS_X = 1024;
    public const int MAX_SOLIDS_Y = 1024;
    public const int MAX_SOLIDS_X_2 = MAX_SOLIDS_X / 2;
    public const int MAX_SOLIDS_Y_2 = MAX_SOLIDS_Y / 2;

    public static CollisionManager Instance { get { return _instance; } }

    public struct RaycastCollision
    {
        public GameObject CollidedObject;
        public IntegerVector CollisionPoint;
        public bool CollidedX;
        public bool CollidedY;
    }

    public struct RaycastResult
    {
        public bool Collided { get { return this.Collisions != null && this.Collisions.Length != 0; } }
        public RaycastCollision[] Collisions;
        public IntegerVector FarthestPointReached;
    }

    public void Awake()
    {
        _instance = this;
    }

    public void AddCollider(LayerMask layer, IntegerCollider collider)
    {
        if ((layer & this.SolidsLayerMask) != 0)
        {
            int x = xPositionToSolidsIndex(collider.Bounds.Center.X);
            int y = yPositionToSolidsIndex(collider.Bounds.Center.Y);

            if (_solids[x, y] == null)
                _solids[x, y] = new List<IntegerCollider>();
            _solids[x, y].Add(collider);
        }
        else
        {
            if (!_collidersByLayer.ContainsKey(layer))
                _collidersByLayer.Add(layer, new List<IntegerCollider>());
            _collidersByLayer[layer].AddUnique(collider);
        }
    }

    public void RemoveCollider(LayerMask layer, IntegerCollider collider)
    {
        if ((layer & this.SolidsLayerMask) != 0)
        {
            int x = xPositionToSolidsIndex(collider.Bounds.Center.X);
            int y = yPositionToSolidsIndex(collider.Bounds.Center.Y);

            if (_solids[x, y] != null)
            {
                _solids[x, y].Remove(collider);
            }
            /*else
            {
                Debug.LogWarning("Collider to remove not found! " + collider.transform + ", " + collider.transform.root);
            }*/
        }
        else
        {
            if (_collidersByLayer.ContainsKey(layer))
                _collidersByLayer[layer].Remove(collider);
        }
    }

    public List<IntegerCollider> GetCollidersInRange(IntegerRect range, int mask = Physics2D.DefaultRaycastLayers, string objectTag = null)
    {
        List<IntegerCollider> colliders = new List<IntegerCollider>();

        foreach (LayerMask key in _collidersByLayer.Keys)
        {
            if ((key & mask) != 0)
            {
                foreach (IntegerCollider collider in _collidersByLayer[key])
                {
                    if ((objectTag == null || collider.tag == objectTag) && collider.enabled &&
                        collider.Bounds.Overlaps(range))
                        colliders.Add(collider);
                }
            }
        }

        if ((mask & this.SolidsLayerMask) != 0)
        {
            for (int x = xPositionToSolidsIndex(range.Min.X - this.MaxSolidSize); x <= xPositionToSolidsIndex(range.Max.X + this.MaxSolidSize); ++x)
            {
                for (int y = yPositionToSolidsIndex(range.Min.Y - this.MaxSolidSize); y <= yPositionToSolidsIndex(range.Max.Y + this.MaxSolidSize); ++y)
                {
                    if (_solids[x, y] != null)
                        colliders.AddRange(_solids[x, y]);
                }
            }
        }

        return colliders;
    }

    public GameObject CollidePointFirst(IntegerVector point, int mask = Physics2D.DefaultRaycastLayers, string objectTag = null)
    {
        if ((mask & this.SolidsLayerMask) != 0)
        {
            int midX = xPositionToSolidsIndex(point.X);
            int midY = yPositionToSolidsIndex(point.Y);
            int minX = midX > 1 ? midX - 1 : 0;
            int maxX = midX < MAX_SOLIDS_X - 1 ? midX + 1 : MAX_SOLIDS_X - 1;
            int minY = midY > 1 ? midY - 1 : 0;
            int maxY = midY < MAX_SOLIDS_Y - 1 ? midY + 1 : MAX_SOLIDS_Y - 1;

            for (int x = minX; x <= maxX; ++x)
            {
                for (int y = minY; y <= maxY; ++y)
                {
                    if (_solids[x, y] != null)
                    {
                        foreach (IntegerCollider collider in _solids[x, y])
                        {
                            if (collider.Contains(point) && collider.enabled)
                                return collider.gameObject;
                        }
                    }
                }
            }
        }

        foreach (LayerMask key in _collidersByLayer.Keys)
        {
            if ((key & mask) != 0)
            {
                foreach (IntegerCollider collider in _collidersByLayer[key])
                {
                    if ((objectTag == null || collider.tag == objectTag) && collider.enabled &&
                        collider.Contains(point))
                        return collider.gameObject;
                }
            }
        }

        return null;
    }

    public GameObject CollidePointFirst(IntegerVector point, List<IntegerCollider> potentialCollisions)
    {
        foreach (IntegerCollider collider in potentialCollisions)
        {
            if (collider.enabled && collider.Contains(point))
                return collider.gameObject;
        }
        return null;
    }

    public RaycastResult RaycastFirst(IntegerVector origin, Vector2 direction, float range = 100000.0f, int mask = Physics2D.DefaultRaycastLayers, string objectTag = null)
    {
        Vector2 d = direction * range;
        Vector2 chunkD = range <= this.RaycastChunkSize ? d : direction * this.RaycastChunkSize;

        IntegerVector halfwayPoint = new IntegerVector(chunkD / 2.0f) + origin;
        IntegerVector rangeVector = new IntegerVector(Mathf.RoundToInt(Mathf.Abs(chunkD.x) + 2.5f), Mathf.RoundToInt(Mathf.Abs(chunkD.y) + 2.5f));
        List<IntegerCollider> possibleCollisions = this.GetCollidersInRange(new IntegerRect(halfwayPoint, rangeVector), mask);

        Vector2 positionModifier = Vector2.zero;
        IntegerVector position = origin;

        float incX = d.x;
        float incY = d.y;

        if (Mathf.Abs(incX) > RAYCAST_MAX_POSITION_INCREMENT || Mathf.Abs(incY) > RAYCAST_MAX_POSITION_INCREMENT)
        {
            Vector2 dNormalized = d.normalized * RAYCAST_MAX_POSITION_INCREMENT;
            incX = dNormalized.x;
            incY = dNormalized.y;
        }

        Vector2 projected = Vector2.zero;
        Vector2 soFar = Vector2.zero;
        float dMagnitude = d.magnitude;
        RaycastResult result = new RaycastResult();
        bool endReached = false;
        int chunksSoFar = 1;

        while (true)
        {
            if (soFar.magnitude >= this.RaycastChunkSize * chunksSoFar)
            {
                // Recalculate chunk
                halfwayPoint = new IntegerVector(chunkD / 2.0f) + position;
                rangeVector = new IntegerVector(Mathf.RoundToInt(Mathf.Abs(chunkD.x) + 2.55f), Mathf.RoundToInt(Mathf.Abs(chunkD.y) + 2.55f));
                possibleCollisions = this.GetCollidersInRange(new IntegerRect(halfwayPoint, rangeVector), mask);
                ++chunksSoFar;
            }

            projected.x += incX;
            projected.y += incY;

            if (projected.magnitude > dMagnitude)
            {
                incX = d.x - soFar.x;
                incY = d.y - soFar.y;
                endReached = true;
            }

            positionModifier.x += incX;
            int move = (int)positionModifier.x;

            positionModifier.x -= move;
            int unitDir = Math.Sign(move);

            while (move != 0)
            {
                IntegerVector checkPos = new IntegerVector(position.X + unitDir, position.Y);
                GameObject collision = this.CollidePointFirst(checkPos, possibleCollisions);
                if (collision)
                {
                    result.Collisions = new RaycastCollision[1];
                    RaycastCollision hit = new RaycastCollision();
                    hit.CollidedObject = collision;
                    hit.CollisionPoint = position;
                    hit.CollidedX = true;
                    result.Collisions[0] = hit;
                }

                position = checkPos;

                if (result.Collided)
                    break;

                move -= unitDir;
            }

            if (result.Collided)
                break;

            positionModifier.y += incY;
            move = (int)positionModifier.y;

            positionModifier.y -= move;
            unitDir = Math.Sign(move);

            while (move != 0)
            {
                IntegerVector checkPos = new IntegerVector(position.X, position.Y + unitDir);
                GameObject collision = this.CollidePointFirst(checkPos, possibleCollisions);
                if (collision)
                {
                    if (result.Collided)
                    {
                        result.Collisions[0].CollidedY = true;
                    }
                    else
                    {
                        result.Collisions = new RaycastCollision[1];
                        RaycastCollision hit = new RaycastCollision();
                        hit.CollidedObject = collision;
                        hit.CollisionPoint = position;
                        hit.CollidedY = true;
                        result.Collisions[0] = hit;
                    }
                }

                position = checkPos;

                if (result.Collided)
                    break;

                move -= unitDir;
            }

            if (result.Collided || endReached)
                break;

            soFar.x = projected.x;
            soFar.y = projected.y;
        }

        result.FarthestPointReached = position;
        return result;
    }

    public RaycastResult Raycast(IntegerVector origin, Vector2 direction, float range = 100000.0f, int mask = Physics2D.DefaultRaycastLayers, string objectTag = null)
    {
        Vector2 d = direction * range;
        Vector2 chunkD = range <= this.RaycastChunkSize ? d : direction * this.RaycastChunkSize;

        IntegerVector halfwayPoint = new IntegerVector(chunkD / 2.0f) + origin;
        IntegerVector rangeVector = new IntegerVector(Mathf.RoundToInt(Mathf.Abs(chunkD.x) + 2.55f), Mathf.RoundToInt(Mathf.Abs(chunkD.y) + 2.55f));
        List<IntegerCollider> possibleCollisions = this.GetCollidersInRange(new IntegerRect(halfwayPoint, rangeVector), mask);

        Vector2 positionModifier = Vector2.zero;
        IntegerVector position = origin;

        float incX = d.x;
        float incY = d.y;

        if (Mathf.Abs(incX) > RAYCAST_MAX_POSITION_INCREMENT || Mathf.Abs(incY) > RAYCAST_MAX_POSITION_INCREMENT)
        {
            Vector2 dNormalized = d.normalized * RAYCAST_MAX_POSITION_INCREMENT;
            incX = dNormalized.x;
            incY = dNormalized.y;
        }

        Vector2 projected = Vector2.zero;
        Vector2 soFar = Vector2.zero;
        float dMagnitude = d.magnitude;
        RaycastResult result = new RaycastResult();
        List<RaycastCollision> collisions = new List<RaycastCollision>();
        List<IntegerCollider> collided = new List<IntegerCollider>();
        bool endReached = false;
        int chunksSoFar = 1;

        while (true)
        {
            if (soFar.magnitude >= this.RaycastChunkSize * chunksSoFar)
            {
                // Recalculate chunk
                halfwayPoint = new IntegerVector(chunkD / 2.0f) + position;
                rangeVector = new IntegerVector(Mathf.RoundToInt(Mathf.Abs(chunkD.x) + 2.55f), Mathf.RoundToInt(Mathf.Abs(chunkD.y) + 2.55f));
                possibleCollisions = this.GetCollidersInRange(new IntegerRect(halfwayPoint, rangeVector), mask);
                foreach (IntegerCollider collider in collided)
                    possibleCollisions.Remove(collider);
                ++chunksSoFar;
            }

            projected.x += incX;
            projected.y += incY;

            if (projected.magnitude > dMagnitude)
            {
                incX = d.x - soFar.x;
                incY = d.y - soFar.y;
                endReached = true;
            }

            positionModifier.x += incX;
            int move = (int)positionModifier.x;

            positionModifier.x -= move;
            int unitDir = Math.Sign(move);

            while (move != 0)
            {
                IntegerVector checkPos = new IntegerVector(position.X + unitDir, position.Y);
                GameObject collision = this.CollidePointFirst(checkPos, possibleCollisions);
                if (collision)
                {
                    IntegerCollider collider = collision.GetComponent<IntegerCollider>();
                    possibleCollisions.Remove(collider);
                    RaycastCollision hit = new RaycastCollision();
                    hit.CollidedObject = collision;
                    hit.CollisionPoint = position;
                    hit.CollidedX = true;
                    collisions.Add(hit);
                    collided.Add(collider);
                }

                position = checkPos;
                move -= unitDir;
            }

            positionModifier.y += incY;
            move = (int)positionModifier.y;

            positionModifier.y -= move;
            unitDir = Math.Sign(move);

            while (move != 0)
            {
                IntegerVector checkPos = new IntegerVector(position.X, position.Y + unitDir);
                GameObject collision = this.CollidePointFirst(checkPos, possibleCollisions);
                if (collision)
                {
                    IntegerCollider collider = collision.GetComponent<IntegerCollider>();
                    possibleCollisions.Remove(collider);
                    RaycastCollision hit = new RaycastCollision();
                    hit.CollidedObject = collision;
                    hit.CollisionPoint = position;
                    hit.CollidedY = true;
                    collisions.Add(hit);
                    collided.Add(collider);
                }

                position = checkPos;
                move -= unitDir;
            }

            if (endReached)
                break;

            soFar.x = projected.x;
            soFar.y = projected.y;
        }

        result.Collisions = collisions.ToArray();
        result.FarthestPointReached = position;
        return result;
    }

    public RaycastResult RaycastUntil(List<RaycastCollision> passThroughCollisions, IntegerVector origin, Vector2 direction, int passThroughMask, int haltMask, float range = 100000.0f)
    {
        passThroughMask &= ~haltMask;
        Vector2 d = direction * range;
        Vector2 chunkD = range <= this.RaycastChunkSize ? d : direction * this.RaycastChunkSize;

        IntegerVector halfwayPoint = new IntegerVector(chunkD / 2.0f) + origin;
        IntegerVector rangeVector = new IntegerVector(Mathf.RoundToInt(Mathf.Abs(chunkD.x) + 2.55f), Mathf.RoundToInt(Mathf.Abs(chunkD.y) + 2.55f));
        List<IntegerCollider> possibleCollisions = this.GetCollidersInRange(new IntegerRect(halfwayPoint, rangeVector), passThroughMask | haltMask);

        Vector2 positionModifier = Vector2.zero;
        IntegerVector position = origin;

        float incX = d.x;
        float incY = d.y;

        if (Mathf.Abs(incX) > RAYCAST_MAX_POSITION_INCREMENT || Mathf.Abs(incY) > RAYCAST_MAX_POSITION_INCREMENT)
        {
            Vector2 dNormalized = d.normalized * RAYCAST_MAX_POSITION_INCREMENT;
            incX = dNormalized.x;
            incY = dNormalized.y;
        }

        Vector2 projected = Vector2.zero;
        Vector2 soFar = Vector2.zero;
        float dMagnitude = d.magnitude;
        RaycastResult result = new RaycastResult();
        List<IntegerCollider> collided = new List<IntegerCollider>();
        bool endReached = false;
        int chunksSoFar = 1;

        while (true)
        {
            if (soFar.magnitude >= this.RaycastChunkSize * chunksSoFar)
            {
                // Recalculate chunk
                halfwayPoint = new IntegerVector(chunkD / 2.0f) + position;
                rangeVector = new IntegerVector(Mathf.RoundToInt(Mathf.Abs(chunkD.x) + 2.55f), Mathf.RoundToInt(Mathf.Abs(chunkD.y) + 2.55f));
                possibleCollisions = this.GetCollidersInRange(new IntegerRect(halfwayPoint, rangeVector), passThroughMask | haltMask);
                foreach (IntegerCollider collider in collided)
                    possibleCollisions.Remove(collider);
                ++chunksSoFar;
            }

            projected.x += incX;
            projected.y += incY;

            if (projected.magnitude > dMagnitude)
            {
                incX = d.x - soFar.x;
                incY = d.y - soFar.y;
                endReached = true;
            }

            positionModifier.x += incX;
            int move = (int)positionModifier.x;

            positionModifier.x -= move;
            int unitDir = Math.Sign(move);

            while (move != 0)
            {
                IntegerVector checkPos = new IntegerVector(position.X + unitDir, position.Y);
                GameObject collision = this.CollidePointFirst(checkPos, possibleCollisions);
                if (collision)
                {
                    IntegerCollider collider = collision.GetComponent<IntegerCollider>();
                    possibleCollisions.Remove(collider);
                    RaycastCollision hit = new RaycastCollision();
                    hit.CollidedObject = collision;
                    hit.CollisionPoint = position;
                    hit.CollidedX = true;
                    if (((1 << collision.layer) & passThroughMask) != 0)
                    {
                        passThroughCollisions.Add(hit);
                        collided.Add(collider);
                    }
                    else
                    {
                        result.Collisions = new RaycastCollision[1];
                        result.Collisions[0] = hit;
                    }
                }

                position = checkPos;

                if (result.Collided)
                    break;

                move -= unitDir;
            }

            if (result.Collided)
                break;

            positionModifier.y += incY;
            move = (int)positionModifier.y;

            positionModifier.y -= move;
            unitDir = Math.Sign(move);

            while (move != 0)
            {
                IntegerVector checkPos = new IntegerVector(position.X, position.Y + unitDir);
                GameObject collision = this.CollidePointFirst(checkPos, possibleCollisions);
                if (collision)
                {
                    IntegerCollider collider = collision.GetComponent<IntegerCollider>();
                    possibleCollisions.Remove(collider);
                    RaycastCollision hit = new RaycastCollision();
                    hit.CollidedObject = collision;
                    hit.CollisionPoint = position;
                    hit.CollidedY = true;
                    if (((1 << collision.layer) & passThroughMask) != 0)
                    {
                        passThroughCollisions.Add(hit);
                        collided.Add(collider);
                    }
                    else
                    {
                        result.Collisions = new RaycastCollision[1];
                        result.Collisions[0] = hit;
                    }
                }

                position = checkPos;

                if (result.Collided)
                    break;

                move -= unitDir;
            }

            if (result.Collided || endReached)
                break;

            soFar.x = projected.x;
            soFar.y = projected.y;
        }

        result.FarthestPointReached = position;
        return result;
    }

    public void RemoveAllSolids()
    {
        for (int y = 0; y < _solids.GetLength(1); ++y)
        {
            for (int x = 0; x < _solids.GetLength(0); ++x)
            {
                if (_solids[x, y] != null)
                    _solids[x, y].Clear();
            }
        }
    }

    /**
     * Private
     */
    private Dictionary<LayerMask, List<IntegerCollider>> _collidersByLayer = new Dictionary<LayerMask, List<IntegerCollider>>();
    private List<IntegerCollider>[,] _solids = new List<IntegerCollider>[MAX_SOLIDS_X, MAX_SOLIDS_Y];
    private static CollisionManager _instance;

    private int xPositionToSolidsIndex(int posX)
    {
        int x = posX / this.MaxSolidSize + MAX_SOLIDS_X_2;
        if (x < 0)
            x = 0;
        else if (x >= MAX_SOLIDS_X)
            x = MAX_SOLIDS_X - 1;
        return x;
    }

    private int yPositionToSolidsIndex(int posY)
    {
        int y = posY / this.MaxSolidSize + MAX_SOLIDS_Y_2;
        if (y < 0)
            y = 0;
        else if (y >= MAX_SOLIDS_Y)
            y = MAX_SOLIDS_Y - 1;
        return y;
    }
}
