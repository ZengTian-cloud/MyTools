using System;
public interface ISocketManager
{
    void ConnectServer();
    void CloseConnect();
    void Send(byte[] data);
    int Receive(byte[] buffer, int offset, int size);
    bool IsConnected();
}
