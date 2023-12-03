using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Basics;

public class CoroutineManager: SingletonOjbect
{
    private int mSeqId = 0;

    private int GetSequene()
    {
        ++mSeqId;
        if (mSeqId == 100000)
        {
            mSeqId = 1;
        }
        return mSeqId;
    }



    private Dictionary<int, Coroutine> repeatDic = new Dictionary<int, Coroutine>();

    // 每帧间隔回调
    public int InvokePerFrame(Action repeatAction)
    {
        int repeatId = GetSequene();
        Coroutine temcor = StartCoroutine(PerFrameInvoke(repeatId, repeatAction));
        repeatDic.Add(repeatId, temcor);
        return repeatId;
    }

    // 每间隔一定事件回调
    public int InvokeRepeating(Action repeatAction, int time, int repeatRate)
    {
        int repeatId = GetSequene();
        Coroutine temcor = StartCoroutine(DoRepeatInvoke(repeatId, time, repeatRate, repeatAction));
        repeatDic.Add(repeatId, temcor);
        return repeatId;

    }

    // 停止单个间隔回调携程
    public void StopRepeatInvoke(int repeatId)
    {
        if (repeatDic.ContainsKey(repeatId))
        {
            StopCoroutine(repeatDic[repeatId]);
            repeatDic.Remove(repeatId);
        }
    }

    // 停止所有间隔回调携程
    public void StopAllRepeatInvoke()
    {
        foreach (var item in repeatDic)
        {
            StopCoroutine(item.Value);
        }

        repeatDic.Clear();
    }

    private IEnumerator DoRepeatInvoke(int repeatId, int time, int repeatRate, Action repeatAction)
    {
        yield return new WaitForSeconds(time * 0.001f);

        while (repeatDic.ContainsKey(repeatId) && repeatAction != null)
        {
            yield return new WaitForSeconds(repeatRate * 0.001f);
        }

        if (repeatDic.ContainsKey(repeatId))
        {
            repeatDic.Remove(repeatId);
        }
        repeatAction?.Invoke();
        repeatAction = null;
    }

    private IEnumerator PerFrameInvoke(int repeatId, Action repeatAction)
    {
        yield return null;

        while (repeatDic.ContainsKey(repeatId) && repeatAction != null)
        {
            yield return null;
        }

        if (repeatDic.ContainsKey(repeatId))
        {
            repeatDic.Remove(repeatId);
        }
        repeatAction?.Invoke();
        repeatAction = null;
    }



    private Dictionary<int, Coroutine> delayDic = new Dictionary<int, Coroutine>();

    // 延迟一帧回调
    public int DelayNextFrame(Action tcallback)
    {
        int delayID = GetSequene();
        Coroutine tcor = StartCoroutine(DelayNextCoroutine(delayID, tcallback));
        delayDic.Add(delayID, tcor);
        return delayID;
    }

    // LateUpdate后回调
    public int DelayLateFrame(Action tcallback)
    {
        int delayID = GetSequene();
        Coroutine tcor = StartCoroutine(DelayLateCoroutine(delayID, tcallback));
        delayDic.Add(delayID, tcor);
        return delayID;
    }

    // 延迟xxms回调
    public int DoDelayInvoke(int delayTime, Action tcallback)
    {
        if (delayTime > 0)
        {
            int delayID = GetSequene();
            Coroutine tcor = StartCoroutine(DelayCoroutine(delayID, delayTime, tcallback));
            delayDic.Add(delayID, tcor);
            return delayID;
        }
        else
        {
            if (tcallback != null)
            {
                tcallback();
                tcallback = null;
            }
            return 0;
        }
    }

    // 停止单个延迟回调
    public void StopDelayInvoke(int delayID)
    {
        if (delayDic.ContainsKey(delayID))
        {
            StopCoroutine(delayDic[delayID]);
            delayDic.Remove(delayID);
        }
    }

    // 停止所有延迟回调
    public void StopAllDelayInvoke()
    {
        foreach (var item in delayDic)
        {
            StopCoroutine(item.Value);
        }
        delayDic.Clear();
    }

    private IEnumerator DelayCoroutine(int delayID, int delayTime, Action tcallback)
    {
        if (delayTime > 0)
        {
            yield return new WaitForSeconds(delayTime * 0.001f);
        }
        docallback(delayID, tcallback);
    }

    private IEnumerator DelayNextCoroutine(int delayID, Action tcallback)
    {
        yield return null;
        docallback(delayID, tcallback);
    }

    private IEnumerator DelayLateCoroutine(int delayID, Action tcallback)
    {
        yield return new WaitForEndOfFrame();
        docallback(delayID, tcallback);
    }

    private void docallback(int delayID, Action tcallback)
    {
        if (delayDic.ContainsKey(delayID))
        {
            if (tcallback != null)
            {
                tcallback();
                tcallback = null;
            }
            delayDic.Remove(delayID);
        }
    }
}


