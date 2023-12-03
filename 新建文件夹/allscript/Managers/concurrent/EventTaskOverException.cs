using System;
public class EventTaskOverException : Exception
{
    public EventTaskOverException() : base("任务数量超出最大值")
    {

    }
}
