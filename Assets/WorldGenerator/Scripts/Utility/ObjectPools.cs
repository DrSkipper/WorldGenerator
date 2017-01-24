using UnityEngine;
using System.Collections.Generic;

public class ObjectPools : MonoBehaviour
{
    public PooledObject[] PrefabsToPool;

    void Awake()
    {
        _instance = this;
        _pools = new List<PooledObject>[this.PrefabsToPool.Length];

        for (int i = 0; i < this.PrefabsToPool.Length; ++i)
        {
            // Set pool ids, create pools
            PooledObject prefab = this.PrefabsToPool[i];
            prefab.PoolId = i;
            _pools[i] = new List<PooledObject>(prefab.MaxToStore);

            // Preload
            while (_pools[i].Count < prefab.MaxToStore)
            {
                returnObject(_pools[i], instantiate(prefab), false);
            }
        }
    }

    public static PooledObject Retain(PooledObject prefab)
    {
        if (_instance != null)
            return _instance.retain(prefab);
        Debug.LogWarning("No ObjectPools instance exists, cannot retain " + prefab);
        return instantiate(prefab);
    }

    public static void Release(GameObject toRelease)
    {
        PooledObject pooledObject = toRelease.GetComponent<PooledObject>();
        if (pooledObject == null)
        {
            Debug.LogWarning("No PooledObject script found on " + toRelease);
            Destroy(toRelease.gameObject);
        }
        else
        {
            Release(pooledObject);
        }
    }

    public static void Release(PooledObject toRelease)
    {
        if (_instance != null)
            _instance.release(toRelease);
        else
            Debug.LogWarning("No ObjectPools instance exists, cannot release " + toRelease);
    }

    /**
     * Private
     */
    private static ObjectPools _instance;
    private List<PooledObject>[] _pools;

    private PooledObject retain(PooledObject prefab)
    {
        int poolId = prefab.PoolId;
        if (poolId < 0 || poolId >= _pools.Length)
        {
            Debug.LogWarning("No pool found with id " + poolId + ", specified on prefab " + prefab);
        }
        else
        {
            List<PooledObject> pool = _pools[prefab.PoolId];
            if (pool.Count > 0)
            {
                PooledObject instance = pool.Pop();
                instance.gameObject.SetActive(true);
                return instance;
            }
        }

        return instantiate(prefab);
    }

    private void release(PooledObject toRelease)
    {
        int poolId = toRelease.PoolId;
        if (poolId < 0 || poolId >= _pools.Length)
        {
            Debug.LogWarning("No pool found with id " + poolId + ", specified on object " + toRelease);
        }
        else
        {
            if (returnObject(_pools[poolId], toRelease))
                return;
        }

        Destroy(toRelease.gameObject);
    }

    private static PooledObject instantiate(PooledObject prefab)
    {
        return Instantiate<PooledObject>(prefab);
    }

    private bool returnObject(List<PooledObject> pool, PooledObject obj, bool broadcastMessage = true)
    {
        if (pool.Count < pool.Capacity)
        {
            if (broadcastMessage)
                obj.BroadcastMessage(POOL_RETURN_METHOD, SendMessageOptions.DontRequireReceiver);
            obj.transform.parent = null;
            obj.gameObject.SetActive(false);
            pool.Add(obj);
            return true;
        }
        return false;
    }

    private const string POOL_RETURN_METHOD = "OnReturnToPool";
}
