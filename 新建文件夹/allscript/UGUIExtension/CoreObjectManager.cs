using System;
using System.Collections.Generic;
using Basics;

public class CoreObjectManager
{
    private static CoreObjectManager instance;
    public static CoreObjectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CoreObjectManager();
            }
            return instance;
        }
    }

    int _objectIdOrder = 0;
    Dictionary<int, CoreObject> _objectMap = new Dictionary<int, CoreObject>();
    List<CoreObject> _toRecycle = new List<CoreObject>();
    List<CoreObject> _toAdd = new List<CoreObject>();
    //ObjectPool<Type, CoreObject> objectPool = new ObjectPool<Type, CoreObject>();


    public T Request<T>() where T : CoreObject, new()
    {
        //if (!(objectPool.Request(typeof(T)) is T ret))
        //{
        var ret = new T();
        //}
        ret.Id = ++_objectIdOrder;
        ret.Active = true;
        ret.Recycled = false;
        AddObject(ret, false);
        return ret;
    }

    public void Recycle(CoreObject o)
    {
        o.Recycled = true;
        o.Active = false;
        o.Recycle();
        _toRecycle.Add(o);
    }

    public Dictionary<int, CoreObject> GetAllObject()
    {
        return _objectMap;
    }

    public void Clear()
    {
        foreach (var item in _objectMap)
        {
            _Recycle(item.Value);
        }

        foreach (var item in _toAdd)
        {
            _Recycle(item);
        }

        _objectIdOrder = 0;
        _objectMap.Clear();
        _toRecycle.Clear();
        _toAdd.Clear();
        //objectPool.Clear();
    }

    public void Update()
    {
        foreach (var item in _objectMap)
        {
            if (item.Value.Active)
            {
                item.Value.Update();
            }
        }
        ProcessAdd();
        ProcessDelete();
    }

    public void AddObject(CoreObject o, bool delay = true)
    {
        if (o.Id != 0 && !_objectMap.ContainsKey(o.Id))
        {
            if (delay)
            {
                _toAdd.Add(o);
            }
            else
            {
                _objectMap[o.Id] = o;
                o.Start();
            }
        }
    }

    public T GetObject<T>(int id) where T : CoreObject
    {
        CoreObject o = null;
        if (id != 0)
        {
            _objectMap.TryGetValue(id, out o);
            if (o != null && o.Recycled)
            {
                o = null;
            }
        }
        T ret = o as T;
        return ret;
    }


    void _Recycle(CoreObject o)
    {
        // objectPool.Recycle(o.GetType(), o);
    }

    void ProcessDelete()
    {
        for (int i = 0; i < _toRecycle.Count; i++)
        {
            CoreObject o = _toRecycle[i];
            _objectMap.Remove(o.Id);
            _Recycle(o);
        }
        _toRecycle.Clear();
    }

    void ProcessAdd()
    {
        for (int i = 0; i < _toAdd.Count; i++)
        {
            AddObject(_toAdd[i], false);
        }
        _toAdd.Clear();
    }
}

