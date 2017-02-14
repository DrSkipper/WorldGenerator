using UnityEngine;

public class WorldEntity : VoBehavior
{
    public string QuadName;
    public string EntityName;

    void Awake()
    {
        _entityConsumedEvent = new EntityConsumedEvent(this.QuadName, this.EntityName);
    }

    public void TriggerConsumption()
    {
        _entityConsumedEvent.QuadName = this.QuadName;
        _entityConsumedEvent.EntityName = this.EntityName;
        GlobalEvents.Notifier.SendEvent(_entityConsumedEvent);
        ObjectPools.Release(this.gameObject);
    }

    /**
     * Private
     */
    private EntityConsumedEvent _entityConsumedEvent;
}
