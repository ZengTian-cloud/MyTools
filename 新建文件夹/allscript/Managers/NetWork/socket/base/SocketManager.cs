using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
/// <summary>
/// 以同步的方法建立socket连接
/// </summary>
public class SocketManager : ISocketManager
{
    private string host;
    private int port;
    private Socket socket;

    public SocketManager(string host, int port)
    {
        this.host = host;
        this.port = port;

    }

    public void ConnectServer()
    {
        if (socket != null)
        {
            CloseConnect();
        }
        Debug.Log("开始连接服务器：" + host + ":" + port);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.SendTimeout = GameClient.SocketConnectTimeout;
        socket.Connect(host, port);
        Debug.Log("连接服务器成功：" + host + ":" + port);
    }

    public void CloseConnect()
    {

        if (socket != null)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                socket.Close();
                socket = null;
            }
            
            
            
        }
    }

    public void Send(byte[] data)
    {
        if (socket.Connected)
        {
            socket.Send(data, SocketFlags.None);
        }
    }

    public int Receive(byte[] buffer, int offset, int size)
    {
        return socket.Receive(buffer, offset, size, SocketFlags.None);
    }

    public bool IsConnected()
    {
        return socket != null && socket.Connected;
    }
}
