using System;
public class SocketDisconnectException : Exception
{
    public SocketDisconnectException() : base("网络已断开")
    {
    }
}
