using System;
using System.Collections;
using System.Collections.Generic;

public class SendMessage : IGameMessage
{
    private SendMessageHeader header;
    public string content{get;set;}

    public SendMessage(int MessageId,int MessageSeqId,String content, bool isForcedUncompression, bool isForcedUnencryption)
    {
        InitHeader( MessageId, MessageSeqId, isForcedUncompression, isForcedUnencryption);
        this.content = content;
    }
    public void InitHeader(int MessageId,int MessageSeqId, bool isForcedUncompression,bool isForcedUnencryption)
    {
        header = new SendMessageHeader
        {
            Version = 1,
            MessageSeqId = MessageSeqId,
            MessageId = MessageId,
            mtime = DateUtil.CurrentSecond(),
            IsCompress = false,
            IsEncrypt = false,

            IsForcedUncompression = isForcedUncompression,
            IsForcedUnencryption = isForcedUnencryption,
        };
    }
    public GameMessageHeader GetHeader()
    {
        return header;
    }

    public void Read(byte[] body) { }

    public  byte[] Write()
    {
        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    public string getContent() {
        return content;
    }

    public string Log(){
        return content;
    }


}
