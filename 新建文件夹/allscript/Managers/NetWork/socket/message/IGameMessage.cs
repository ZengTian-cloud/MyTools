using System;
public interface IGameMessage
{
    GameMessageHeader GetHeader();
    void Read(byte[] body);
    byte[] Write();
    string Log();
    string getContent();
}
