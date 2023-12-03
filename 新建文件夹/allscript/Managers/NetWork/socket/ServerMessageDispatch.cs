using System;
using System.Collections.Concurrent;
using System.Threading;
using Cysharp.Threading.Tasks;
using NetWork.socket;
using UnityEngine;

public class ServerMessageDispatch
{
    private ConcurrentDictionary<int, MessageMappingInfo> cache = new ConcurrentDictionary<int, MessageMappingInfo>();

    public ConcurrentDictionary<int, PushMessageInfo> pushCallback = new ConcurrentDictionary<int, PushMessageInfo>();

    //private ConcurrentQueue<IGameMessage> messageCache = new ConcurrentQueue<IGameMessage>();

    public Action<IGameMessage> pushAction { get; set; }

    private EventExecutor eventExecutor = new EventExecutor();

    public void AddPushMessageCallback(int messageId,string hid, Action<IGameMessage> Callback)
    {
        PushMessageInfo info = GetPushCallback(messageId);
        if (info == null) {
            info = new PushMessageInfo();
            pushCallback.TryAdd(messageId, info);
        }
        info.Callbacks.TryAdd(hid, Callback);
        
    }

    private PushMessageInfo GetPushCallback(int messageId)
    {
        PushMessageInfo value;
        pushCallback.TryGetValue(messageId, out value);
        return value;
    }

    public PushMessageInfo DePushCallback(int messageId,string hid)
    {
        Action<IGameMessage> callback;
        PushMessageInfo push = GetPushCallback(messageId);
        if (push != null) {
            push.Callbacks.TryRemove(hid, out callback);
        }
        return push;
    }

    public PushMessageInfo DePushMessage(int messageId)
    {
        PushMessageInfo push;
        pushCallback.TryRemove(messageId, out push);
        return push;
    }


    public void AddRequestMappingCache(MessageMappingInfo messageMappingInfo)
    {
        int clienSeqID = messageMappingInfo.ClientSeqID;
        cache.TryAdd(clienSeqID, messageMappingInfo);
    }

    public MessageMappingInfo DeMessageMapping(int clientSeqID)
    {
        MessageMappingInfo messageMappingInfo;
        cache.TryRemove(clientSeqID, out messageMappingInfo);
        return messageMappingInfo;
    }


    public void addMessageCache(ByteBuf byteBuf){
        byteBuf.MarkReaderIndex();
        byteBuf.SkipBytes(8);//跳过包的总大小字节
        int clientSeqID = byteBuf.ReadInt();
        int messageID = byteBuf.ReadInt();
        EnumMessageType messageType = (EnumMessageType)byteBuf.ReadByte();
        byteBuf.ResetReaderIndex();
        switch (messageType) {
            case EnumMessageType.RESPONSE:
                MessageMappingInfo messageMappingInfo = this.DeMessageMapping(clientSeqID);
                if (messageMappingInfo != null)
                {
                    IGameMessage responseMsg = messageMappingInfo.ResponseMessage;
                    //用线程打印网络耗时
                    eventExecutor.Execute(() =>
                    {
                        long currtime = DateUtil.CurrentSecond();
                        long time = currtime - messageMappingInfo.mtime;
                        if (time > 10)
                        {
                            Debug.LogWarning("SOCKET收消息时长：messageid->" + responseMsg.GetHeader().MessageId + ",time->" + time);
                        }
                    });

                    //读取响应信息
                    CodecFactory.DecodeMessage(byteBuf, responseMsg);
                    //messageCache.Enqueue(responseMessage);
                    messageMappingInfo.Action.Invoke(responseMsg);

                }
                break;
            case EnumMessageType.PUSH:

                IGameMessage responseMessage = new ReceiveMessage(clientSeqID);
                //读取响应信息
                CodecFactory.DecodeMessage(byteBuf, responseMessage);
                pushAction?.Invoke(responseMessage);

                PushMessageInfo push = GetPushCallback(messageID);
                if (push != null && push.Callbacks.Count > 0)
                {
                    foreach (var callbacks in push.Callbacks)
                    {
                        string key = callbacks.Key;
                        Action<IGameMessage> call = callbacks.Value;
                        call?.Invoke(responseMessage);
                    }  
                }

                break;
            default:
                break;
        }
    }

    public void DispatchMessage(ByteBuf byteBuf)
    {
        
    }



}



