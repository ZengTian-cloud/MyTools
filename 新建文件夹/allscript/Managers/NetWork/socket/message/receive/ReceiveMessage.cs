using System;
using System.Collections.Generic;
using System.Text;

public class ReceiveMessage : IGameMessage
{
    private ReceiceMessageHeader header;

    public string content{get;set;}


    public ReceiveMessage(int MessageSeqId)
    {
        header = new ReceiceMessageHeader
        {
            MessageSeqId = MessageSeqId,
            MessageId = -1,
        };
    }

    public ReceiveMessage(int MessageId,int MessageSeqId)
    {
        header = new ReceiceMessageHeader
        {
            MessageId = MessageId,
            MessageSeqId = MessageSeqId,
        };
    }
    public GameMessageHeader GetHeader()
    {
        return header;
    }

    public virtual void Read(byte[] body) {
           content = Encoding.UTF8.GetString(body);
    }

    public virtual  byte[] Write()
    {
        return null;
    }

    public string getContent()
    {
        return content;
    }

    public string Log(){
        return content;
        
    }

}
