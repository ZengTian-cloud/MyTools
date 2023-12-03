using System;
using System.Threading;
using UnityEngine;

namespace NetWork.socket
{
    public class GameSocketManager
    {
        private ISocketManager socketManager;

        private SocketSendManager sendManager;
        private SocketConnectManager connectManager;
        private SocketReceiveManager receiveManager;


        private ServerMessageDispatch serverMessageDispatch;
        private GameSocketManager(){}
        public GameSocketManager(string host, int port)
        {
          
            serverMessageDispatch = new ServerMessageDispatch();
            socketManager = new SocketManager(host, port);
            sendManager = new SocketSendManager(serverMessageDispatch, socketManager);
            connectManager = new SocketConnectManager(socketManager);
            receiveManager = new SocketReceiveManager(socketManager, serverMessageDispatch);
        }

        public void AddSocketStateChangeListener(Action<SocketStateEnum> listener, Type classType)
        {
            Debug.Log("业务：" + classType.FullName + "类添加网络状态监听");
            connectManager.SocketStateChangeEvent += listener;
        }

        public void RemoveSocketStateChangeListener(Action<SocketStateEnum> socketState, Type sceneClassType)
        {
            Debug.Log("业务：" + sceneClassType.FullName + "添加网络状态监听");
            connectManager.SocketStateChangeEvent -= socketState;
        }

        public void UpdateSocketState(SocketStateEnum socketState)
        {
            connectManager.UpdateSocketState(socketState);
        }


        public void Shutdown()
        {
            try
            {
                socketManager.CloseConnect();
                sendManager.Shutdown();
                receiveManager.Shutdown();
                connectManager.Shutdown();
            }
            catch (Exception e) {
                Debug.LogError("关闭服务器出错："+e.Message);
            }
        }

        public void AddPushMessageCallback(int messageId, string hid,Action<IGameMessage> Callback)
        {
            serverMessageDispatch.AddPushMessageCallback(messageId, hid,Callback);
        }

        /// <summary>
        /// 删除PUSH消息处理中的一个回调方法
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="hid"></param>
        /// <returns></returns>
        public PushMessageInfo DePushCallback(int messageId, string hid)
        {
            return serverMessageDispatch.DePushCallback(messageId,hid);
        }

        /// <summary>
        /// 根据业务ID删除PUSH消息处理的全部回调方法
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public PushMessageInfo DePushMessage(int messageId)
        {
            return serverMessageDispatch.DePushMessage(messageId);
        }

        /// <summary>
        /// 连接服务器，
        /// </summary>
        /// <param name="callback">连接回调方法</param>
        /// <param name="pushAction">PUSH消息收到时的公共处理方法，就是说不管收到什么PUSH消息这个方法都会执行</param>
        public void ConnectServer(Action<SocketStateEnum> callback,Action<IGameMessage> pushAction)
        {

            connectManager.ConnectServer((socketSate) =>
            {
                if (socketSate == SocketStateEnum.ConnectSuccess)
                {
                    receiveManager.StartReceive();
                }
                serverMessageDispatch.pushAction = pushAction;
                callback.Invoke(socketSate);
            });
        }

        public void CloseConnect()
        {
            connectManager.CloseSocket();
        }

        public bool IsConnected()
        {
            return socketManager.IsConnected();
        }
        public bool Send<T>(IGameMessage message, Action<IGameMessage> callback) where T : IGameMessage
        {
            return sendManager.Send<T>(message, callback);
        }


    }
}
