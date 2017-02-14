using UnityEngine;
using System.Collections.Generic;

public class EntityTracker : MonoBehaviour
{
    public const string PLAYER = "player";

    public class Entity
    {
        public string QuadName;
        public string EntityName;
        public bool Consumed;
        public bool AttemptingLoad;
        public bool Loaded;
        public bool CanLoad { get { return !this.Consumed && !this.Loaded && !this.AttemptingLoad; } }
        public bool StillInInitialState { get { return true; } } //TODO - class should handle more data such as health, and if data isn't equal to initial load state when unloading, we'll have to remember its state while unloaded.

        public Entity(string quadName, string entityName)
        {
            this.QuadName = quadName;
            this.EntityName = entityName;
            this.Consumed = false;
            this.Loaded = false;
            this.AttemptingLoad = false;
        }
    }

    void Awake()
    {
        GlobalEvents.Notifier.Listen(EntityConsumedEvent.NAME, this, entityConsumed);
    }

    public Entity GetEntity(string quadName, string entityName)
    {
        // Players owned globally, not by quad
        if (entityName == PLAYER)
            quadName = PLAYER;

        if (!_trackedEntities.ContainsKey(quadName))
        {
            _trackedEntities.Add(quadName, new Dictionary<string, Entity>());
        }

        if (!_trackedEntities[quadName].ContainsKey(entityName))
        {
            _trackedEntities[quadName].Add(entityName, new Entity(quadName, entityName));
        }

        return _trackedEntities[quadName][entityName];
    }

    // Pre: Caller has already checked if entity is already loaded by calling GetEntity, preferably before the WorldEntity is even created
    public void TrackLoadedEntity(WorldEntity entity)
    {
        _loadedEntities.Add(entity);
        _trackedEntities[entity.QuadName][entity.EntityName].Loaded = true;
    }

    public void QuadsUnloaded(IntegerRect loadBounds)
    {
        for (int i = 0; i < _loadedEntities.Count;)
        {
            WorldEntity entity = _loadedEntities[i];
            IntegerVector entityPos = (Vector2)entity.transform.position;

            if (!loadBounds.Contains(entityPos))
            {
                _trackedEntities[entity.QuadName][entity.EntityName].Loaded = false;
                ObjectPools.Release(entity.gameObject);
                _loadedEntities.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }
    }

    /**
     * Private
     */
    private Dictionary<string, Dictionary<string, Entity>> _trackedEntities = new Dictionary<string, Dictionary<string, Entity>>();
    private List<WorldEntity> _loadedEntities = new List<WorldEntity>();

    private void entityConsumed(LocalEventNotifier.Event e)
    {
        EntityConsumedEvent consumedEvent = e as EntityConsumedEvent;

        if (!_trackedEntities.ContainsKey(consumedEvent.QuadName))
        {
            Debug.LogWarning("EntityTracker notified of consumption with invalid quad owner name: " + consumedEvent.QuadName + ". Entity name is: " + consumedEvent.EntityName);
        }
        else if (!_trackedEntities[consumedEvent.QuadName].ContainsKey(consumedEvent.EntityName))
        {
            Debug.LogWarning("EntityTracker notified of consumption of invalid entity named: " + consumedEvent.EntityName + ", owned by quad: " + consumedEvent.QuadName);
        }
        else
        {
            _trackedEntities[consumedEvent.QuadName][consumedEvent.EntityName].Consumed = true;
            for (int i = 0; i < _loadedEntities.Count; ++i)
            {
                if (_loadedEntities[i].QuadName == consumedEvent.QuadName && _loadedEntities[i].EntityName == consumedEvent.EntityName)
                {
                    _loadedEntities.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
