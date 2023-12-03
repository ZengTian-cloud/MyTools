using System.Threading;

public abstract class GameMessageHeader
{

    public int MessageSeqId { get; set; }
    public int Version { get; set; }
    public int MessageSize { set; get; }
    public int MessageId { get; set; }
    public long mtime { get; set; }
    public int ErrorCode { get; set; }
    public short ErrorLen { get; set; }
    public bool IsEncrypt { get; set; }
    public bool IsCompress { get; set; }
    public bool IsForcedUncompression { get; set; }
    public bool IsForcedUnencryption { get; set; }

    public static int ReadMessageId(ByteBuf byteBuf)
    {
        byteBuf.MarkReaderIndex();
        byteBuf.SkipBytes(8);
        int messageId = byteBuf.ReadInt();
        byteBuf.ResetReaderIndex();
        return messageId;
    }

    public abstract void Read(ByteBuf byteBuf);

    public abstract void Write(ByteBuf byteBuf);

    public abstract int getHeaderSize();

}
