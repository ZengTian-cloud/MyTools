using System;
using UnityEngine;

namespace NetWork.socket
{
    class SocketConnectManager
    {
        //处理网络状态的线程
        private EventExecutor networkStatusExecutor = new EventExecutor();

        private ISocketManager socketManager;

        private volatile SocketStateEnum socketState;
        internal event Action<SocketStateEnum> SocketStateChangeEvent;

        public SocketConnectManager(ISocketManager socketManager)
        {
            this.socketManager = socketManager;
        }

        public void ConnectServer(Action<SocketStateEnum> callback)
        {

            networkStatusExecutor.Execute(() =>
            {
                this.UpdateSocketState(SocketStateEnum.Connecting);
                try
                {
                    socketManager.ConnectServer();
                    this.UpdateSocketState(SocketStateEnum.ConnectSuccess);
                    callback.Invoke(this.socketState);
                }
                catch(Exception e)
                {
                    Debug.Log("NetManager 连接服务器异常" + e);
                    UpdateSocketState(SocketStateEnum.ConnectFailed);
                }
                
            });
            
        }

        public void CloseSocket()
        {
            networkStatusExecutor.Execute(() =>
            {
                socketManager.CloseConnect();
                this.UpdateSocketState(SocketStateEnum.Disconnect);
            });
        }

        public void UpdateSocketState(SocketStateEnum socketState)
        {
            Debug.Log("NetManager 修改网络状态，" + this.socketState + "->" + socketState);
            this.socketState = socketState;
            SocketStateChangeEvent?.Invoke(socketState);
        }

        public void Shutdown()
        {
            networkStatusExecutor.Shutdown();
        }


    }
}
