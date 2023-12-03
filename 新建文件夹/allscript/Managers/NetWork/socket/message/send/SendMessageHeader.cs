using System.Threading;

public class SendMessageHeader : GameMessageHeader
{

    //public const int HEADER_WRITE_LENGTH = 26;

    public override void Write(ByteBuf byteBuf)
    {

        byteBuf.WriteBytes(DateUtil.int2bytes(MessageSize));
        byteBuf.WriteBytes(DateUtil.int2bytes(Version));
        byteBuf.WriteBytes(DateUtil.int2bytes(MessageSeqId));
        byteBuf.WriteBytes(DateUtil.int2bytes(MessageId));
        // byteBuf.WriteInt(MessageSize);
        // byteBuf.WriteInt(Version);
        // byteBuf.WriteInt(MessageSeqId);
        // byteBuf.WriteInt(MessageId);
        byteBuf.WriteLong(mtime);
        byteBuf.WriteByte(IsCompress ? 1 : 0);
        byteBuf.WriteByte(IsEncrypt ? 1 : 0);
        

    }



    public override void Read(ByteBuf byteBuf)
    {
        this.MessageSize = byteBuf.ReadInt();
        this.Version = byteBuf.ReadInt();
        this.MessageSeqId = byteBuf.ReadInt();
        this.MessageId = byteBuf.ReadInt();
        this.mtime = byteBuf.ReadLong();
        this.IsCompress = byteBuf.ReadByte() == 1 ? true : false;
        this.IsEncrypt = byteBuf.ReadByte() == 1 ? true : false;  
    }

    public override int getHeaderSize(){
        return 26;
    }

}
