using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗事件管理
/// </summary>
public class BattleEventManager 
{

    static Dictionary<string, Action> mEventList = new Dictionary<string, Action>();
    static ParamedEventHandle<string> mStringEventList = new ParamedEventHandle<string>();
    static ParamedEventHandle<float> mFloatEventList = new ParamedEventHandle<float>();
    static ParamedEventHandle<Vector3> mVector3EventList = new ParamedEventHandle<Vector3>();
    static ParamedEventHandle<bool> mBoolEventList = new ParamedEventHandle<bool>();
    static ParamedEventHandle<long> mLongEventList = new ParamedEventHandle<long>();
    static ParamedEventHandle<GameObject> mGameObjectEventList = new ParamedEventHandle<GameObject>();

    #region
    //注册/移除事件 
    public static void RegisterEvent(string name, Action callback, bool register)
    {
        if (register)
        {
            if (!mEventList.ContainsKey(name))
                mEventList.Add(name, callback);
            else
                mEventList[name] += callback;
        }
        else
            if (mEventList.ContainsKey(name))
            mEventList[name] -= callback;
    }

    //派发事件
    public static void Dispatch(string name)
    {
        if (mEventList.TryGetValue(name, out var _callbackList))
        {
            _callbackList?.Invoke();
        }
    }

    #endregion

    #region string�¼�
    public static void RegisterEvent(string name, Action<string> callback, bool register)
    {
        mStringEventList.RegisterEvent(name, callback, register);
    }
    public static void Dispatch(string name, string value)
    {
        mStringEventList.Dispatch(name, value);
        Dispatch(name);
    }
    #endregion

    #region Vector3�¼�
    public static void RegisterEvent(string name, Action<Vector3> callback, bool register)
    {
        mVector3EventList.RegisterEvent(name, callback, register);
    }
    public static void Dispatch(string name, Vector3 value)
    {
        mVector3EventList.Dispatch(name, value);
        Dispatch(name);
    }
    #endregion

    #region bool�¼�
    public static void RegisterEvent(string name, Action<bool> callback, bool register)
    {
        mBoolEventList.RegisterEvent(name, callback, register);
    }
    public static void Dispatch(string name, bool value)
    {
        mBoolEventList.Dispatch(name, value);
        Dispatch(name);
    }
    #endregion

    #region float�¼�
    public static void RegisterEvent(string name, Action<float> callback, bool register)
    {
        mFloatEventList.RegisterEvent(name, callback, register);
    }
    public static void Dispatch(string name, float value)
    {
        mFloatEventList.Dispatch(name, value);
        Dispatch(name);
    }
    #endregion

    #region long�¼�
    public static void RegisterEvent(string name, Action<long> callback, bool register)
    {
        mLongEventList.RegisterEvent(name, callback, register);
    }
    public static void Dispatch(string name, long value)
    {
        mLongEventList.Dispatch(name, value);
        Dispatch(name);
    }
    #endregion

    #region GameObject�¼�
    public static void RegisterEvent(string name, Action<GameObject> callback, bool register)
    {
        mGameObjectEventList.RegisterEvent(name, callback, register);
    }
    public static void Dispatch(string name, GameObject value)
    {
        mGameObjectEventList.Dispatch(name, value);
        Dispatch(name);
    }
    #endregion
}

public class ParamedEventHandle<T>
{
    private Dictionary<string, Action<T>> mEventList = new Dictionary<string, Action<T>>();
    public void RegisterEvent(string name, Action<T> callback, bool register)
    {
        if (register)
        {
            if (!mEventList.ContainsKey(name))
                mEventList.Add(name, callback);
            else
                mEventList[name] += callback;
        }
        else
        {
            if (mEventList.ContainsKey(name))
                mEventList[name] -= callback;
        }

    }

    public void Dispatch(string name, T value)
    {
        if (mEventList.TryGetValue(name, out var _callbackList))
        {
            _callbackList?.Invoke(value);
        }
    }
}
