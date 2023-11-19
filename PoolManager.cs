using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoSingleton<PoolManager>
{
    Dictionary<string, ResourceData> resourcePool = new Dictionary<string, ResourceData>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.gameObject);
    }
    public GameObject GetAResource(string theName)
    {
        if (resourcePool.ContainsKey(theName) && resourcePool[theName].count > 0)
            return resourcePool[theName].GetAResource();
        else
            return null;
    }
    public void RemoveAResource(string theKey)
    {
        if (resourcePool.ContainsKey(theKey))
        {
            for (int i = resourcePool[theKey].count-1; i>=0 ; i--)
            {
                if (resourcePool[theKey].pool[i] != null)
                    Destroy(resourcePool[theKey].pool[i]);
            }
            resourcePool.Remove(theKey);
        }
    }
    public void Recycle(GameObject go, string theKey)
    {
        go.SetActive(false);
        if (resourcePool.ContainsKey(theKey))
        {
            resourcePool[theKey].Recycle(go);
        }
        else
        {
            resourcePool.Add(theKey, new ResourceData());
            resourcePool[theKey].Recycle(go);
        }
        
    }
    public void ClearAll()
    {
        foreach (var item in resourcePool)
        {
            if (item.Value.count > 0)
            {
                for (int i =item.Value.count-1; i >=0; i--)
                {
                    if (item.Value.pool[i] != null)
                        Destroy(item.Value.pool[i].gameObject);
                }
                item.Value.pool.Clear();
            }
        }
        resourcePool.Clear();


    }
}

public class ResourceData
{
    
    public List<GameObject> pool = new List<GameObject>();
    public int count { get { return pool.Count; } }
    public GameObject GetAResource()
    {
        GameObject go = pool[0];
        pool.RemoveAt(0);
        return go;
    }
    public void Recycle(GameObject go)
    {
        pool.Add(go);
    }

}
