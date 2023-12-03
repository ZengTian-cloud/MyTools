using System;
using System.Threading;
using UnityEngine;

namespace NetWork.socket
{
    class SocketSendManager
    {
        private ServerMessageDispatch serverMessageDispatch;
        //处理消息发送的线程
        private EventExecutor eventExecutor = new EventExecutor();
        private ISocketManager socketManager;

        public SocketSendManager(ServerMessageDispatch serverMessageDispatch, ISocketManager socketManager)
        {
            this.serverMessageDispatch = serverMessageDispatch;
            this.socketManager = socketManager;
        }

        public void Shutdown()
        {
            eventExecutor.Shutdown();
        }

        public bool Send<T>(IGameMessage message, Action<IGameMessage> callback) where T : IGameMessage
        {
            if (eventExecutor.TaskCount() > 2)
            {
                Debug.Log("NetManager 本次消息发送失败，网络消息发送太频繁了");
                return false;
            }
            eventExecutor.Execute(() =>
            {
                try
                {
                    //判断网络是否正常，如果没有连接，就等待
                    while (!socketManager.IsConnected())
                    {
                        Thread.Sleep(300);
                    }

                    
                    byte[] data = CodecFactory.EncodeMessage(message);
                    int seqId = message.GetHeader().MessageSeqId;
                    int messageid = message.GetHeader().MessageId;
#if UNITY_EDITOR
                    if (messageid!=3) Debug.LogError("NetManager 发送消息：" + "[seqId:" + seqId + "],messageID:" + message.GetHeader().MessageId + "->" + message.Log() + ",消息大小" + data.Length);
#endif
                    socketManager.Send(data);

                    MessageMappingInfo messageMappingInfo;
                    if (messageid == 3)
                    {
                        messageMappingInfo = new MessageMappingInfo();
                    }
                    else {
                        messageMappingInfo = new MessageMappingInfo(serverMessageDispatch, messageid);
                    }
                    IGameMessage responseMessage = new ReceiveMessage(seqId);

                    messageMappingInfo.ClientSeqID = seqId;
                    messageMappingInfo.ResponseMessage = responseMessage;
                    messageMappingInfo.Action = callback;
                    messageMappingInfo.mtime = DateUtil.CurrentSecond();
                    serverMessageDispatch.AddRequestMappingCache(messageMappingInfo);
                }
                catch (Exception e)
                {
                    Debug.LogError("NetManager 网络连接失败，请检测网络，" + e);
                }


            });
            return true;
        }
    }

}
