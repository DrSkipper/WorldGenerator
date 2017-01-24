using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [HideInInspector]
    public int PoolId = 0; // Set automatically
    public int MaxToStore = 128;

    public PooledObject Retain()
    {
        return ObjectPools.Retain(this);
    }

    //NOTE: Can instead directly call "ObjectPools.Release(GameObject)" if you don't have a reference to the PooledObject component.
    public void Release()
    {
        ObjectPools.Release(this);
    }
}
