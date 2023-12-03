using System.Threading;

public class ReceiceMessageHeader : GameMessageHeader
{


    public const int HEADER_WRITE_LENGTH = 24;

    public EnumMessageType MessageType { get; set; }
    

    public override void Write(ByteBuf byteBuf)
    {

        byteBuf.WriteInt(MessageSeqId);
        byteBuf.WriteInt(MessageId);
        byteBuf.WriteLong(mtime);
        byteBuf.WriteInt(Version);
        byteBuf.WriteByte(IsCompress ? 1 : 0);
        byteBuf.WriteByte(IsEncrypt ? 1 : 0);
        
    }


    public override void Read(ByteBuf byteBuf)
    {
        this.MessageSize = byteBuf.ReadInt();
        this.Version = byteBuf.ReadInt();
        this.MessageSeqId = byteBuf.ReadInt();
        this.MessageId = byteBuf.ReadInt();
        this.MessageType = (EnumMessageType)byteBuf.ReadByte();
        this.mtime = byteBuf.ReadLong();
        this.ErrorCode = byteBuf.ReadInt();
        //byteBuf.SkipBytes(this.ErrorLen);//跳过错误内容的读取
        if (ErrorCode == 0)
        {
            this.IsCompress = byteBuf.ReadByte() == 1 ? true : false;
            this.IsEncrypt = byteBuf.ReadByte() == 1 ? true : false;
        }
        else {
            this.ErrorLen = byteBuf.ReadShort();
        }

        
    }

    public override int getHeaderSize()
    {
        return 26;
    }

}
