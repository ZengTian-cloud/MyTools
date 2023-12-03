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
    /// ��ʼЭ��
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
    /// ��ֹЭ��
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
    /// �ӳٵ���, by Second
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
    /// start �ӳٵ���, by Second
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
    /// �ӳٵ���, by Frame
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
    /// start �ӳٵ���, by Frame
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
    /// ѭ������ by time
    /// </summary>
    /// <param name="duration">ִ�е��ʱ�䣬-1 Ϊ����ֹͣ</param>
    /// <param name="interval">���ʱ��</param>
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
    /// start ѭ������ by time
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
    /// ѭ������ by fram count
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
    /// start ѭ������ by fram count
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
            // ��¼��ʼʱ��
            startTime = Time.time;
            // ��¼��һ�μ��ʱ��
            lastTime = Time.time;
            // ��¼�������ʱ��
            interval = _interval;
            // ��¼��ʱ��
            duration = _duration;
            // ����ص�
            callback = _callback;
        }

        // ����Э����ͣ����true����coroutine����ִ�з��� false��
        // ��MonoBehaviour.Update֮��MonoBehaviour.LateUpdate֮ǰ��ÿ֡�����ѯkeepWaiting���ԡ�
        public override bool keepWaiting
        {
            get
            {
                //�˷�������false��ʾЭ�̽���
                if (duration>0&&Time.time - startTime >= duration)
                {
                    return false;
                }
                else if (Time.time - lastTime >= interval)
                {
                    //������һ�μ��ʱ��
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
            // ��¼��ʼʱ��
            lastTime = Time.time;
            // ��¼��һ�μ��ʱ��
            interval = _interval;
            // ��ǰcount
            curCount = 0;
            // all count
            loopCount = _loopCount;
            // ����ص�
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
                    //������һ�μ��ʱ��
                    lastTime = Time.time;
                    curCount++;
                    callback?.Invoke();
                }
                return true;
            }
        }
    }

}