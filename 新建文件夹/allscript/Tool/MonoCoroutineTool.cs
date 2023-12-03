using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MonoCoroutineTool
{
    private class MyMonoBehaviour : MonoBehaviour
    {
        public void SetDonotDestory(GameObject o)
        {
            DontDestroyOnLoad(o);
        }
    }

    private static MyMonoBehaviour m_MonoBehaviour;

    static MonoCoroutineTool()
    {
        GameObject o = GameObject.Find("MonoCoroutineTool");
        if (o == null)
        {
            o = new GameObject("MonoCoroutineTool");
        }
        m_MonoBehaviour = o.AddComponent<MyMonoBehaviour>();
        m_MonoBehaviour.SetDonotDestory(o);
    }

    /// <summary>
    /// 开始协程
    /// </summary>
    /// <param name="routine"></param>
    /// <returns></returns>
    public static Coroutine StartCoroutine(IEnumerator routine)
    {
        if (routine == null)
            return null;
        return m_MonoBehaviour.StartCoroutine(routine);
    }

    /// <summary>
    /// 终止协程
    /// </summary>
    /// <param name="routine"></param>
    public static void StopCoroutine(ref Coroutine routine)
    {
        if (routine != null)
        {
            m_MonoBehaviour.StopCoroutine(routine);
            routine = null;
        }
    }

    /// <summary>
    /// 延迟调用, by Second
    /// </summary>
    /// <param name="action"></param>
    /// <param name="delaySeconds"></param>
    /// <returns></returns>
    public static Coroutine DelayInvokeBySecond(Action action, float delaySeconds)
    {
        if (action == null) return null;
        return m_MonoBehaviour.StartCoroutine(StartDelayInvokeBySecond(action, delaySeconds));
    }

    /// <summary>
    /// start 延迟调用, by Second
    /// </summary>
    /// <param name="action"></param>
    /// <param name="delaySeconds"></param>
    /// <returns></returns>
    private static IEnumerator StartDelayInvokeBySecond(Action action, float delaySeconds)
    {
        if (delaySeconds > 0)
            yield return new WaitForSeconds(delaySeconds);
        else
            yield return null;
        action?.Invoke();
    }

    /// <summary>
    /// 延迟调用, by Frame
    /// </summary>
    /// <param name="action"></param>
    /// <param name="delayFrames"></param>
    /// <returns></returns>
    public static Coroutine DelayInvokeByFrame(Action action, int delayFrames)
    {
        if (action == null)
            return null;
        return m_MonoBehaviour.StartCoroutine(StartDelayInvokeByFrame(action, delayFrames));
    }

    /// <summary>
    /// start 延迟调用, by Frame
    /// </summary>
    /// <param name="action"></param>
    /// <param name="delayFrames"></param>
    /// <returns></returns>
    private static IEnumerator StartDelayInvokeByFrame(Action action, int delayFrames)
    {
        if (delayFrames > 1)
        {
            for (int i = 0; i < delayFrames; i++)
            {
                yield return null;
            }
        }
        else
            yield return null;
        action?.Invoke();
    }

    /// <summary>
    /// 循环调用 by time
    /// </summary>
    /// <param name="duration">执行的最长时间，-1 为永不停止</param>
    /// <param name="interval">间隔时间</param>
    /// <param name="fun"></param>
    /// <returns></returns>
    public static Coroutine LoopInvokeByTime(float duration, float interval, Func<bool> fun)
    {
        if (fun == null)
            return null;
        if (interval <= 0 )
            return null;
        return m_MonoBehaviour.StartCoroutine(StartLoopInvokeByTime(duration, interval, fun));
    }

    /// <summary>
    /// start 循环调用 by time
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="interval"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    private static IEnumerator StartLoopInvokeByTime(float duration, float interval, Func<bool> fun)
    {
        yield return new CustomLoopInvokeByTime(duration, interval, fun);
    }

    /// <summary>
    /// 循环调用 by fram count
    /// </summary>
    /// <param name="loopCount"></param>
    /// <param name="interval"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Coroutine LoopInvokeByCount(int loopCount, float interval, Action action)
    {
        if (action == null)
            return null;
        if (loopCount <= 0 || interval <= 0)
            return null;
        return m_MonoBehaviour.StartCoroutine(StartLoopInvokeByCount(loopCount, interval, action));
    }

    /// <summary>
    /// start 循环调用 by fram count
    /// </summary>
    /// <param name="loopCount"></param>
    /// <param name="interval"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    private static IEnumerator StartLoopInvokeByCount(int loopCount, float interval, Action action)
    {
        yield return new CustomLoopInvokeByCount(loopCount, interval, action);
    }

    /// <summary>
    /// custom loop time
    /// </summary>
    private class CustomLoopInvokeByTime : CustomYieldInstruction
    {
        private Func<bool> callback;
        private float startTime;
        private float lastTime;
        private float interval;
        private float duration;

        public CustomLoopInvokeByTime(float _duration, float _interval, Func<bool> _callback)
        {
            // 记录开始时间
            startTime = Time.time;
            // 记录上一次间隔时间
            lastTime = Time.time;
            // 记录间隔调用时间
            interval = _interval;
            // 记录总时间
            duration = _duration;
            // 间隔回调
            callback = _callback;
        }

        // 保持协程暂停返回true。让coroutine继续执行返回 false。
        // 在MonoBehaviour.Update之后、MonoBehaviour.LateUpdate之前，每帧都会查询keepWaiting属性。
        public override bool keepWaiting
        {
            get
            {
                //此方法返回false表示协程结束
                if (duration>0&&Time.time - startTime >= duration)
                {
                    return false;
                }
                else if (Time.time - lastTime >= interval)
                {
                    //更新上一次间隔时间
                    lastTime = Time.time;
                    return callback.Invoke();
                }
                return true;
            }
        }
    }

    /// <summary>
    ///  custom loop frame count
    /// </summary>
    private class CustomLoopInvokeByCount : CustomYieldInstruction
    {
        private Action callback;
        private float lastTime;
        private float interval;
        private int curCount;
        private int loopCount;

        public CustomLoopInvokeByCount(int _loopCount, float _interval, Action _callback)
        {
            // 记录开始时间
            lastTime = Time.time;
            // 记录上一次间隔时间
            interval = _interval;
            // 当前count
            curCount = 0;
            // all count
            loopCount = _loopCount;
            // 间隔回调
            callback = _callback;
        }

        public override bool keepWaiting
        {
            get
            {
                if (curCount > loopCount)
                {
                    return false;
                }
                else if (Time.time - lastTime >= interval)
                {
                    //更新上一次间隔时间
                    lastTime = Time.time;
                    curCount++;
                    callback?.Invoke();
                }
                return true;
            }
        }
    }

}