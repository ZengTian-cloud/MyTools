using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

public class EventExecutor
{
    private BlockingCollection<Action> taskQueue = new BlockingCollection<Action>();
    private int maxCount = int.MaxValue;
    private Thread thread;
    private volatile bool shutdown;
    public EventExecutor() : this(0)
    {

    }



    public void Shutdown()
    {
            shutdown = true;
        taskQueue.Add(() => { });
    }
    public EventExecutor(int maxCount)
    {
        if (maxCount > 0)
        {
            this.maxCount = maxCount;
        }
        thread = new Thread(RunTask);
        thread.Start();
    }
    public int TaskCount()
    {
        return taskQueue.Count;
    }
    private void RunTask()
    {
        Debug.Log("启动线程：" + Thread.CurrentThread.ManagedThreadId);
        while (!shutdown)
        {
            try
            {
                Action task = taskQueue.Take();
                if (task != null)
                {
                    task.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.Log("执行任务异常：" + e);
            }

        }
        Debug.Log("线程退出：" + Thread.CurrentThread.ManagedThreadId);
    }
    

    public bool Execute(Action task)
    {

        if (taskQueue.Count > maxCount)
        {
            return false;
        }
        taskQueue.Add(task);
        return true;
    }
}
