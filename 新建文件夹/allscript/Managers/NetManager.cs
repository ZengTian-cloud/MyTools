using System;
using System.Collections.Generic;
using Basics;
using NetWork.socket;
using UnityEngine;
using LitJson;
using Cysharp.Threading.Tasks;

namespace Managers
{
	public class NetManager:SingletonOjbect
	{

        /*
        *  //测试处理PUSH消息
                socketManger.AddPushMessageCallback(100, "1", (msg) =>
                {
                    Debug.Log("收到了100号PUSH消息:"+msg.getContent());
                });
        */
        private NetState netState = NetState.DEFAUIT;
        private string _host ;
        private int _port;

        private long _heartExpTime = 3000;
        private long _heartTime = 0;
        private static bool isSendHeart = false;//当前心跳开关

        private Action<int> connectSussAct;
        public Action<IGameMessage> pushHandle;

        private CommonPopWinPrefab commonpop;
        private NetPromptWin netprompt;

        private Queue<MessageCacheInfo> sendMsgQueue = new Queue<MessageCacheInfo>();

        // 本次登陆需要处理的离线push信息code列表
        private List<int> thisLoginOfflinePushCodes = new List<int>();
        public void SetOfflineCodes(string serverContent)
        {
            // todo
            zxlogger.log("OfflineCodes:" + serverContent.ToString());
        }
        public bool HasOfflinePushCode(int code)
        {
            return thisLoginOfflinePushCodes.Contains(code);
        }

        private void FixedUpdate() {
            try
            {
                switch (netState)
                {
                    case NetState.DEFAUIT://初始状态
                    break;
                    case NetState.CONNECTED://正在连接
                        checkNetConnectState();
                        break;
                    case NetState.FAIL:
                     break;
                    case NetState.OPERATION://连接成功
                        handleMessage();
                    break;
                    case NetState.RECONNECT://断线重连
                        reConnent(_host,_port);
                    break;
                    case NetState.RECMAX://重试连接次数已满，需要退回到登录界面
                        if (commonpop != null) break;
                        Debug.LogError("重连次数达到" + reCount + "，弹出网络错误提示窗口");
                        if (netprompt != null)
                        {
                            GameCenter.mIns.m_UIMgr.CloseCustomPrefab("netprompt");
                            netprompt = null;
                        }
                        string txt = GameCenter.mIns.m_LanMgr.GetLan("ui_serverconnectfail_net_error") ;
                        commonpop = GameCenter.mIns.m_UIMgr.PopWindowPrefab(new PopWinStyle(GameCenter.mIns.m_LanMgr.GetLan("login_15"), txt, 2, (confirm) => {
                            if (confirm == 1)
                            {
                                reCount = 0;//重置连接次数
                                netState = NetState.RECONNECT;//将状态改为断线重连模式
                            }
                            else
                            {
                                netState = NetState.EXIT;//将状态改为退出游戏模式
                            }
                            commonpop = null;

                        }, new List<string>() { GameCenter.mIns.m_LanMgr.GetLan("common_exit_game"), GameCenter.mIns.m_LanMgr.GetLan("common_btn_reset") }));
                        
                        break;
                    case NetState.EXIT://已准备退出游戏，直接退出
                        GameCenter.mIns.ExitGame();
                        break;
                    default:
                        break;
                }
            }catch(Exception e){
                string trackStr = new System.Diagnostics.StackTrace().ToString();
                Debug.LogError("网络核心出现了错误："+e.Message+" :"+ trackStr);
                throw e;
                
            }

        }

        private void checkNetConnectState() {
            if(socketManger!=null&&socketManger.IsConnected()){
                if (sendMsgQueue == null)
                {
                    sendMsgQueue = new Queue<MessageCacheInfo>();
                }
                checkGateway();
                netState= NetState.OPERATION;
            }
        }

        private void handleMessage() {
            if (socketManger == null || !socketManger.IsConnected())
            {
                if (socketManger != null)
                {
                    socketManger.UpdateSocketState(SocketStateEnum.ConnectFailed);
                }
                netState = NetState.RECONNECT;
                return;
            }
            else {
                //从队列发送信息
                if (sendMsgQueue.Count > 0) { 
                    MessageCacheInfo msg = sendMsgQueue.Dequeue();
                    socketManger.Send<IGameMessage>(msg.Message, (resMessage) => {
                        _heartTime = DateUtil.CurrentSecond();
                        ReceiceMessageHeader header = (ReceiceMessageHeader)resMessage.GetHeader();
#if UNITY_EDITOR
                        if (msg.MessageId != 3) Debug.Log("收到response数据:" + resMessage.getContent() + " - MessageId:" + msg.MessageId + " - header.ErrorCode:" + header.ErrorCode);
#endif
                        msg.Tcb?.Invoke(header.MessageSeqId, header.ErrorCode, resMessage.getContent());
                    });

                    msg.MessageSeqAction?.Invoke(msg.SeqId);
                }else if (isSendHeart)
                {
                    /**/
                    //发送心跳
                    long currTime = DateUtil.CurrentSecond();
                    long kep = currTime - _heartTime;
                    if (_heartTime != 0 && kep > 10000 && kep < 20000)
                    {
                        Debug.Log("NetManager ：  发送一个心跳");
                        sendHeart();
                    }
                    else if (kep >= 20000)
                    {
                        //超过20秒都没有发送成功
                        _heartTime = DateUtil.CurrentSecond();
                        //如果当前是连接成功的状态，则启动重连
                        if (netState == NetState.OPERATION)
                            netState = NetState.RECONNECT;
                    }
                }
            }
            
        }

        private GameSocketManager socketManger =null;

        public NetManager(string host, int port){

            Debug.Log("NetManager init!!!!!!!!!!!!!!!!!");

        }

        private static bool isLogin = false;

        /// <summary>
        /// 连接过程完成时调用，主要用于登录1001命令完成并验证成功后
        /// 在这里需要进行一些登录成功后的网络模块初始化
        /// </summary>
        public void LoginSucc() {
            _heartTime = DateUtil.CurrentSecond();
            isSendHeart = true;
            isLogin = false;
        }
        public void startLogin() {
            netprompt = null;
            commonpop = null;
            isLogin = true;
        }

        private int reCount = 0;//当前重连次数
        private static int maxCount = 3;//最大重连次数
        public void reConnent(string host, int port) {
            if (reCount >= maxCount) {
                netState = NetState.RECMAX;
                return;
            }
            reCount++;
            string txt = string.Format(GameCenter.mIns.m_LanMgr.GetLan("ui_login_re_connecting"), reCount);
            if (netprompt == null&& !isLogin)
                netprompt = (NetPromptWin)GameCenter.mIns.m_UIMgr.PopCustomPrefab("netprompt", "common", txt);
            else if(!isLogin)
                netprompt.RefreshContent(txt);
            //connect(host, port);
            Connent(host,port,(state) =>
            {
                if (state == 3)
                {
                    //Debug.LogError("重连连接成功，开始拉取数据!");
                    //checkGateway();
                }
                else
                {
                    //Debug.LogError("重连失败!");
                }
            }, reCount);
        }

        public void Connent(string host, int port, Action<int> tcb, int reCount = 1)
        {
            
            this.reCount = reCount;
            this._host = host;
            this._port=port;
            connectSussAct = tcb;
            
            connect(host,port);
        }

        private void connect(string host, int port){

            Debug.Log("NetManager connect start !!!!!!!!!!!!!!!!!("+reCount+")");
            netState= NetState.CONNECTED;//将网络状态改为需要连接状态

            if (socketManger != null) {
                socketManger.Shutdown();
                socketManger = null;
            }
            socketManger= new GameSocketManager(host,port);

            socketManger.AddSocketStateChangeListener((state)=>{
                connectStateChange(state);
            },typeof(NetManager));

            socketManger.ConnectServer((state)=>{
                //Debug.Log("NetManager connect state->"+state+" tcb->"+connectSussAct);
                if (state != SocketStateEnum.ConnectSuccess) {
                    //如果返回的连接是失败的，则启动重连
                    reConnent(host,port);
                }
            },(msg) => {
                //接收到PUSH消息的公共方法，在收到PUSH消息时，这方法总是被执行
                _heartTime = DateUtil.CurrentSecond();
                pushHandle?.Invoke(msg);
            });
            
            Debug.Log("NetManager connect end !!!!!!!!!!!!!!!!!(" + reCount + ")");
        }


        private void connectStateChange(SocketStateEnum state){
            switch(state){
                case SocketStateEnum.ConnectSuccess:
                    Debug.Log("NetManager connect state change->"+state);
                    _heartTime = DateUtil.CurrentSecond();
                    isSendHeart = true;
                    if (netprompt != null) {
                        GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                        {
                            GameCenter.mIns.m_UIMgr.CloseCustomPrefab("netprompt");
                            netprompt = null;
                        });
                    } 
                    break;
                case SocketStateEnum.ConnectFailed:
                    isSendHeart = false;
                    netState = NetState.RECONNECT;
                break;
            }
        }

        private void sendHeart(){
            isSendHeart = false;//在发送心跳时，暂时关闭
            SendData(NetCfg.SOCKET_HEART,null,(msgid,code,content)=> {
                //如果为心跳，则丢弃不处理
                    isSendHeart = true;//收到处理后，打开心跳，这里存在一个关键如果一直收不到心跳的回复，则心跳永远关闭了
                    Debug.Log("---收到心跳，则丢弃不处理---");
            },null,false,true);
        }


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="MessageId">消息业务编码</param>
        /// <param name="tcontent">消息内容 JSON格式字符串</param>
        /// <param name="tcb">回调信息 <int ,int,string>对应<seqid=消息唯一编号 ,code=返回的状态码，0=成功，其他=失败,content=如果状态码为0，则为返回的数据，如果状态码为非0，则为错误信息></param>
        /// <param name="getMessageSeqId">注册这个回调回获取本次消息的序列id</param>
        /// <param name="isForcedUncompression">强制不压缩</param>
        /// <param name="isForcedUnencryption">强制不加密</param>
        public void SendData( int MessageId,JsonData tcontent, Action<int,int, string> tcb, Action<int> getMessageSeqId = null , bool isForcedUncompression =false, bool isForcedUnencryption = false)
        {
            
            if(socketManger==null || !socketManger.IsConnected()){
                netState = NetState.RECONNECT;
                return ;
            }

            int MessageSeqId = GameClient.nextSeqId();
            string cont = "";
            if (tcontent != null) {
                cont = tcontent.ToJson();
            }

            IGameMessage message = new SendMessage(MessageId,MessageSeqId, cont, isForcedUncompression, isForcedUnencryption);
            MessageCacheInfo msginfo = new MessageCacheInfo(MessageId, MessageSeqId, message, tcb, getMessageSeqId);
            sendMsgQueue.Enqueue(msginfo);
        }

        public async UniTask SendDataSync(int MessageId, JsonData tcontent, Action<int, int, string> tcb,int timeout = 3)
        {
            string res = string.Empty;
            int errcode = -999;
            bool resulted = false;
            float timer = Time.time;
            SendData(MessageId, tcontent, (seqid, errorcode, response) =>
            {
                errcode = errorcode;
                res = response;
                resulted = true;
            });
            await UniTask.WaitUntil(()=> resulted);
            tcb?.Invoke(MessageId, errcode, res);
        }


        public void SendDataJson(int MessageId, JsonData tcontent, Action<int, int, JsonData> tcb, Action<int> getMessageSeqId = null, bool isForcedUncompression = false, bool isForcedUnencryption = false)
        {

            if (socketManger == null || !socketManger.IsConnected())
            {
                netState = NetState.RECONNECT;
                return;
            }

            int MessageSeqId = GameClient.nextSeqId();
            string cont = "";
            if (tcontent != null)
            {
                cont = tcontent.ToJson();
            }

            IGameMessage message = new SendMessage(MessageId, MessageSeqId, cont, isForcedUncompression, isForcedUnencryption);
            MessageCacheInfo msginfo = new MessageCacheInfo(MessageId,MessageSeqId, message, (seqid, code, str) => {
                JsonData data = JsonMapper.ToObject(new JsonReader(str));
                tcb?.Invoke(MessageSeqId, code, data);
            }, getMessageSeqId);
            sendMsgQueue.Enqueue(msginfo);
            /*socketManger.Send<IGameMessage>(message, (resMessage) => {
                _heartTime = DateUtil.CurrentSecond();
                ReceiceMessageHeader header = (ReceiceMessageHeader)resMessage.GetHeader();
#if UNITY_EDITOR
                if (MessageId != 3) Debug.Log("收到response数据:" + resMessage.getContent() + " - MessageId:" + MessageId + " - header.ErrorCode:" + header.ErrorCode);
#endif
                JsonData data = JsonMapper.ToObject(new JsonReader(resMessage.getContent()));
                tcb?.Invoke(header.MessageSeqId, header.ErrorCode, data);
            });
            getMessageSeqId?.Invoke(MessageSeqId);*/
        }

        /// <summary>
        /// 获取发送消息基础json数据，所有发送数据需要带有roleid
        /// </summary>
        /// <returns></returns>
        public JsonData GetSendMsgBaseData()
        {
            JsonData jd = new JsonData();
            jd["roleid"] = GameCenter.mIns.userInfo.RoleId;
            return jd;
        }


        #region net key
        /// <summary>
        ///  临时处理断线重连失败问题。重连需要走getkey，sendkey1、2号协议流程，
        ///  TODO:整合login.Code中1、2号协议相同流程
        /// </summary>
        private void checkGateway()
        {
            JsonData jsonData = new JsonData();
            jsonData["uid"] = GameCenter.mIns.userInfo.Uid;
            jsonData["roleid"] = GameCenter.mIns.userInfo.RoleId;
            jsonData["token"] = GameCenter.mIns.userInfo.Token;
            GameCenter.mIns.m_NetMgr.SendData(NetCfg.SOCKET_GATEWAY_CHECK, jsonData, (seqid, code, content) =>
            {
                if (code == 0)
                {
                    getKey();
                }
                else
                {
                    string txt = string.Format(GameCenter.mIns.m_LanMgr.GetLan("login_10"), code, content);
                    //txt_Status.text = txt;
                    GameCenter.mIns.m_UIMgr.PopWindowPrefab(new PopWinStyle(GameCenter.mIns.m_LanMgr.GetLan("login_15"), txt, 2, (confirm) =>
                    {
                        GameCenter.mIns.ExitGame();
                    }, new List<string> { GameCenter.mIns.m_LanMgr.GetLan("login_4"), GameCenter.mIns.m_LanMgr.GetLan("login_5") }));
                }
            },null,false,true);

        }
        public void getKey()
        {
            JsonData jsonData = new JsonData();
            jsonData["roleid"] = GameCenter.mIns.userInfo.RoleId;
            GameCenter.mIns.m_NetMgr.SendData(NetCfg.SOCKET_GET_KEY, jsonData, (seqid, code, content) =>
            {
                if (code == 0)
                {
                    JsonData data = JsonMapper.ToObject(new JsonReader(content));
                    RsaHelp.ServerPublicKey = data["publicKey"].ToString();
                    sendKey();
                }
                else
                {
                    string txt = string.Format(GameCenter.mIns.m_LanMgr.GetLan("login_9"), code, content);
                    //txt_Status.text = txt;
                    GameCenter.mIns.m_UIMgr.PopWindowPrefab(new PopWinStyle(GameCenter.mIns.m_LanMgr.GetLan("login_15"), txt, 2, (confirm) =>
                    {
                        if (confirm == 1)
                        {
                            //重试读取服务器列表
                            getKey();
                        }
                        else
                        {
                            //退出游戏
                            GameCenter.mIns.ExitGame();
                        }
                    }, new List<string> { GameCenter.mIns.m_LanMgr.GetLan("login_4"), GameCenter.mIns.m_LanMgr.GetLan("login_5") }));
                }
            },null,false,true);
        }

        public void sendKey()
        {
            JsonData data = new JsonData();
            data["aesKey"] = RsaHelp.getEncryptKey();
            data["roleid"] = GameCenter.mIns.userInfo.RoleId;
            GameCenter.mIns.m_NetMgr.SendData(NetCfg.SOCKET_SEND_KEY, data, (seqid, code, content) =>
            {
                if (code == 0 && data != null)
                {
                    connectSussAct?.Invoke(3);
                    connectSussAct = null;
                }
                else
                {
                    string txt = string.Format(GameCenter.mIns.m_LanMgr.GetLan("login_9"), code, content);
                    //txt_Status.text = txt;
                    GameCenter.mIns.m_UIMgr.PopWindowPrefab(new PopWinStyle(GameCenter.mIns.m_LanMgr.GetLan("login_15"), txt, 2, (confirm) =>
                    {
                        if (confirm == 1)
                        {
                            //重试读取服务器列表
                            sendKey();
                        }
                        else
                        {
                            //退出游戏
                            GameCenter.mIns.ExitGame();
                        }
                    }, new List<string> { GameCenter.mIns.m_LanMgr.GetLan("login_4"), GameCenter.mIns.m_LanMgr.GetLan("login_5") }));
                }

            },null,false,true);
        }
        #endregion
    }
}