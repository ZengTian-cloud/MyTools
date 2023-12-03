using System.Collections;
using System.Threading;

public class GameClient
{

    /// <summary>
    /// Http请求的超时时间，单位是秒
    /// </summary>
    public static  int HttpRequestTimeout = 32;
    /// <summary>
    /// 消息压缩的分界值，超过此值，将压缩消息发送。
    /// </summary>
    public static int CompressSize = 512;
    /// <summary>
    /// 网关连接的超时时间，单位是毫秒
    /// </summary>
    public static  int SocketConnectTimeout = 15000;//mill
    /// <summary>
    /// 是否开启心跳,false不开启
    /// </summary>
    public static  bool EnableHeartbeat = true;


    private static int MESSAGE_SEQ_ID = 0;


    public static int nextSeqId(){
        return Interlocked.Increment(ref MESSAGE_SEQ_ID);
    }

}
