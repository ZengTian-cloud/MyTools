using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace NetWork.socket
{
    class SocketReceiveManager
    {
        private ISocketManager socketManager;

        private ServerMessageDispatch serverMessageDispatch;
        private volatile bool shuwdown;
        //接收数据线程
        private EventExecutor receiveExecutor = new EventExecutor();
        public SocketReceiveManager(ISocketManager socketManager,ServerMessageDispatch serverMessageDispatch)
        {
            this.socketManager = socketManager;
          
            this.serverMessageDispatch = serverMessageDispatch;
            
        }
        public void Shutdown()
        {
            shuwdown = true;
            receiveExecutor.Shutdown();
        }

        public void StartReceive()
        {
            receiveExecutor.Execute(() =>
            {
                ReadData((byteBuf) =>
                {
                    serverMessageDispatch.addMessageCache(byteBuf);
                });
            });
        }
        private void ReadData(Action<ByteBuf> callback)
        {
            byte[] buffer = new byte[1024 * 16];
            ByteBuf byteBuf = new ByteBuf(buffer);
            Debug.Log("NetManager 启动服务端消息读取");
            while (!shuwdown)
            {
                if (!socketManager.IsConnected())
                {
                    Debug.Log("NetManager 读取消息时，连接已断开");
                    break;
                }
                try
                {
                    //循环使用buffer,不用每次都创建一次buffer
                    int offset = byteBuf.WriteIndex;
                    int readSize = socketManager.Receive(buffer, offset, buffer.Length - offset);
                    int totalOffset = 0;
                    if (readSize > 0)
                    {
                        totalOffset = offset + readSize;
                    }

                    if (totalOffset >= buffer.Length - 128)
                    {
                        byteBuf.DiscardReadBytes();
                        offset = byteBuf.WriteIndex;
                    }
                    byteBuf.WriteIndex = byteBuf.WriteIndex + readSize;

                    int size = byteBuf.ReadableBytes();
                    if (size > 4)
                    {
                        byteBuf.MarkReaderIndex();

                        size = byteBuf.ReadInt() - 4;
                        //粘包处理
                        if (size <= byteBuf.ReadableBytes() && byteBuf.ReadableBytes() > 0)
                        {
                            //可以取出一个完整的包
                            byteBuf.ResetReaderIndex();
                            callback?.Invoke(byteBuf);
                            byteBuf.Clear();
                        }
                        else //不足一个完整包的数据
                        {
                            //断包处理,等待数据的到来
                            byteBuf.ResetReaderIndex();
                            Debug.Log("剩余可读取字节数：" + byteBuf.ReadableBytes());
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.LogError("数据解析错误：" + e);
                    byteBuf.Clear();
                    GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() => {
                        /// TODO:修改(2023-7-18 -- 9:38) "bugid:25060 断线弹出窗口都出现了2层界面"
                        /// 这里将全屏弹窗改为弹出msg文本形式，连接失败弹窗已经在NetManager中弹出，避免出现2个弹窗...
                        //GameCenter.mIns.m_UIMgr.PopMsg("服务器连接错误!");

                        //string txt = "服务器连接错误，请重试！";
                        //GameCenter.mIns.m_UIMgr.PopFullScreenPrefab(new PopFullScreenStyle("错误", txt, 1,(confirm) => {
                        //    GameCenter.mIns.ExitGame();
                        //}, new List<string> { "退出游戏" }));
                    });
                }
            }
            Debug.Log("结束服务器消息读取");
        }

    }
}
