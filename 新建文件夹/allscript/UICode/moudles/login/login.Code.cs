using System.Collections.Generic;
using UnityEngine;
using static UIManager;
using LitJson;
using static RSA;
using Cysharp.Threading.Tasks;
using YooAsset;
using GameTiJi;

public partial class login
{
    public override UILayerType uiLayerType => UILayerType.Login;
    public override string uiAtlasName => "login";
    private HotUpdate hotUpdate = null;

    partial void btn_editor_Click()
    {
        showEdit();
    }
    private async void showEdit()
    {
        //加载配置
        GameCenter.mIns.m_CfgMgr.LoadAllCfg();
        //加载图集
        await SpriteManager.Instance.InitAtlasConfig();//加载图集，可以移到后面登录 界面操作
        this.Close(false);
        GameCenter.mIns.m_UIMgr.Open<editorlist>();
    }

    partial void btn_login_Click()
    {
        onLogin();
    }

    protected override void OnInit()
    {
        updBox.gameObject.SetActive(true);

        txtAppver.text = GameCenter.mIns.gameInfo.appVer;
        txtver.text = GameCenter.mIns.gameInfo.codeVer;
        // GameCenter.mIns.m_CamMgr.AddCameraToMainCamera(uiCamera);
        if (!GameTiJi.TijiConfig.copyright.Equals(""))
        {
            txtCopyright.gameObject.SetActive(true);
            txtCopyright.text = GameTiJi.TijiConfig.copyright;
        }
        else {
            txtCopyright.gameObject.SetActive(false);
        }

        GameLoad gameload = GameObject.Find("GameLunch").GetComponent<GameLoad>();
        gameload.closeUI();

        GameCenter.mIns.m_NetMgr.startLogin();

        //加载包
        ResourcesManager.Instance.loadAllPackageTags();

        if (PackManager.mIns.PlayMode== EPlayMode.EditorSimulateMode)
        {
            onUpdateComplate();
        }
        else {
            //检测热更
            //OnUpdate().Forget();
            hotUpdate = new HotUpdate(updBox.gameObject, () => {
                onUpdateComplate();
            });
            hotUpdate.startUpdate().Forget();
        }
    }


    private void onUpdateComplate()
    {
        Debug.Log("开始进入游戏======================");
        updBox.gameObject.SetActive(false);
        if (GameTiJi.TijiConfig.visitor)
        {
            //输入账号登录 
            string lastUserId = datatool.getstring("lastUserId", "");
            if (string.IsNullOrEmpty(lastUserId))
                ipt_openid.text = "5005000";
            else
                ipt_openid.text = lastUserId;
            editorBox.gameObject.SetActive(true);
        }
        else
        {
            //SDK登录 
            Sdk.Login((param) => {
                onLogin();
            });
        }
        //onLogin();
    }
   

    protected override void OnOpen()
    {
        base.OnOpen();
        // 注册update
        GameCenter.mIns.m_UIMgr.AddUpdateWin(this);
    }
    private void onLogin()
    {
        editorBox.gameObject.SetActive(false);
        txtStatus.text = GameCenter.mIns.m_LanMgr.GetLan("login_1");
        JsonData loginData = new JsonData();
        loginData["openid"] = ipt_openid.text;
        loginData["openkey"] = "";
        loginData["plat"] = "1";
        loginData["os"] = GameCenter.mIns.gameInfo.Os;
        loginData["appid"] = GameCenter.mIns.gameInfo.Appid;

        HttpHelp.sendData(NetCfg.URL_LOGIN, loginData, (status, data, code, msg) =>
        {
            if (status == 200)
            {
                if (code == 1)
                {
                    //{ "uid":"gta5610001","token":"59a511e184414d8b8738ba68c8f5b1bc","isnew":0,"zones":[{ "zid":100020,"zoneid":1001,"beforezoneid":1001,"zonename":"1服","desc":"0.3服","host":"192.168.0.3","port":8088,"apiurl":"","state":2,"zonenum":1,"newzone":1,"recommend":1,"starttime":1685595600000,"isreg":0,"isdebug":1,"isplacard":0},{ "zid":100021,"zoneid":1002,"beforezoneid":1002,"zonename":"2服","desc":"本地服","host":"127.0.0.1","port":8088,"apiurl":"","state":2,"zonenum":1,"newzone":1,"recommend":1,"starttime":1685595600000,"isreg":0,"isdebug":1,"isplacard":0}]}
                    GameCenter.mIns.userInfo.Uid = data["uid"].ToString();
                    GameCenter.mIns.userInfo.Token = data["token"].ToString();
                    GameCenter.mIns.userInfo.IsNew = data["isnew"].ToInt32() == 1;
                    JsonData zoneData = data["zones"];

                    if (zoneData == null || !zoneData.IsArray || zoneData.Count < 1)
                    {
                        //出错了
                        Debug.LogError("服务器信息获取错误");
                        GameCenter.mIns.m_UIMgr.PopWindowPrefab(new PopWinStyle(GameCenter.mIns.m_LanMgr.GetLan("login_2"), GameCenter.mIns.m_LanMgr.GetLan("login_3"), 2, (confirm) =>
                        {
                            if (confirm == 1)
                            {
                                //重试读取服务器列表
                                onLogin();
                            }
                            else
                            {
                                //退出游戏
                                GameCenter.mIns.ExitGame();
                            }
                        }, new List<string> { GameCenter.mIns.m_LanMgr.GetLan("login_4"), GameCenter.mIns.m_LanMgr.GetLan("login_5") }));
                    }
                    else if (zoneData.Count == 1)
                    {
                        //只有一个服务器直接进入
                        createUser(zoneData[0]);
                    }
                    else
                    {
                        //有多个服务器，需要打开服务器选择窗口
                        
                        GameCenter.mIns.m_UIMgr.PopWindowPrefab("服务器列表",new LoginServerWin(zoneData, (zone) => {
                            //选择了服务器
                            createUser(zone);
                        }, () => {
                            //取消了登录 
                            onUpdateComplate();
                        }));
                    }

                }
                else
                {
                    string txt = GameCenter.mIns.m_LanMgr.GetLan("login_6") + code + ",msg:" + msg;
                    txtStatus.text = txt;
                    GameCenter.mIns.m_UIMgr.PopWindowPrefab(new PopWinStyle(GameCenter.mIns.m_LanMgr.GetLan("login_15"), txt, 2, (confirm) =>
                    {
                        if (confirm == 1)
                        {
                            //重试读取服务器列表
                            onLogin();
                        }
                        else
                        {
                            //退出游戏
                            GameCenter.mIns.ExitGame();
                        }
                    }, new List<string> { GameCenter.mIns.m_LanMgr.GetLan("login_4"), GameCenter.mIns.m_LanMgr.GetLan("login_5") }));

                }
            }
        });
    }

    private void setStateText(string txt) {
        GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
        {
            txtStatus.text = txt;
        });
    }
    private void createUser(JsonData zone)
    {
        updBox.gameObject.SetActive(true);
        setStateText(GameCenter.mIns.m_LanMgr.GetLan("login_7"));
        GameCenter.mIns.userInfo.ZoneId = zone["zoneid"].ToInt32();
        JsonData senddata = HttpHelp.initUserParams();

        HttpHelp.sendData(NetCfg.URL_LOGIN_SERVER, senddata, (status, data, code, msg) =>
        {
            if (status == 200 && code == 1)
            {
                txtStatus.text = GameCenter.mIns.m_LanMgr.GetLan("login_8");
                GameCenter.mIns.userInfo.RoleId = data["roleid"].ToString();
                JsonData rolelist = data["rolelist"];
                if (rolelist != null && rolelist.IsArray && rolelist.Count > 0)
                {
                    JsonData role = rolelist[0];
                }

                //开始进入服务器
                string host = zone["host"].ToString();
                int port = zone["port"].ToInt32();
                //host = "192.168.0.55";
                GameCenter.mIns.m_NetMgr.Connent(host, port, (state) =>
                {
                    if (state == 3)
                    {
                        //连接成功，开始拉取数据
                        //socketServerLogin();
                        //  getKey();
                        //checkGateway();
                        socketServerLogin();
                    }
                    else
                    {
                        string txt = string.Format(GameCenter.mIns.m_LanMgr.GetLan("login_9"), code, msg);
                        txtStatus.text = txt;
                        GameCenter.mIns.m_UIMgr.PopWindowPrefab(new PopWinStyle(GameCenter.mIns.m_LanMgr.GetLan("login_15"), txt, 2, (confirm) =>
                        {
                            if (confirm == 1)
                            {
                                //重试读取服务器列表
                                createUser(zone);
                            }
                            else
                            {
                                //退出游戏
                                GameCenter.mIns.ExitGame();
                            }
                        }, new List<string> { GameCenter.mIns.m_LanMgr.GetLan("login_4"), GameCenter.mIns.m_LanMgr.GetLan("login_5") }));

                    }
                });
            }
        });
    }

    /// <summary>
    /// 开始进入服务器
    /// </summary>
    /// <param name="zone"></param>
    private void joinServer(JsonData zone)
    {



    }

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
                string txt = GameCenter.mIns.m_LanMgr.GetLan("login_10") + code + ",msg:" + content;
                txtStatus.text = txt;
                GameCenter.mIns.m_UIMgr.PopWindowPrefab(new PopWinStyle(GameCenter.mIns.m_LanMgr.GetLan("login_15"), txt, 2, (confirm) =>
                {
                    GameCenter.mIns.ExitGame();
                }, new List<string> { GameCenter.mIns.m_LanMgr.GetLan("login_4"), GameCenter.mIns.m_LanMgr.GetLan("login_5") }));
            }
        },null,false,true);
    }

    private void getKey()
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
                string txt = GameCenter.mIns.m_LanMgr.GetLan("login_9") + code + ",msg:" + content;
                txtStatus.text = txt;
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

    private void sendKey()
    {
        JsonData data = new JsonData();
        data["aesKey"] = RsaHelp.getEncryptKey();
        data["roleid"] = GameCenter.mIns.userInfo.RoleId;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.SOCKET_SEND_KEY, data, (seqid, code, content) =>
        {
            if (code == 0 && data != null)
            {
                socketServerLogin();
            }
            else
            {
                string txt = GameCenter.mIns.m_LanMgr.GetLan("login_9") + code + ",msg:" + content;
                txtStatus.text = txt;
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

    private void socketServerLogin()
    {
        setStateText(GameCenter.mIns.m_LanMgr.GetLan("login_11"));
        JsonData data = new JsonData();
        data["roleid"] = GameCenter.mIns.userInfo.RoleId;
        data["uid"] = GameCenter.mIns.userInfo.Uid;
        data["zoneid"] = GameCenter.mIns.userInfo.ZoneId;
        data["token"] = GameCenter.mIns.userInfo.Token;

        GameCenter.mIns.m_NetMgr.SendData(NetCfg.SOCKET_COMMENTID_LOGIN, data,  (seqid, code, content) =>
        {
            if (code == 0)
            {
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {
                    GameCenter.mIns.m_CfgMgr.LoadAllCfg();
                    Debug.Log("配置加载完成------------");
                    // 初始化用户数据
                    GameCenter.mIns.userInfo.InitUserInfo(content);
                    joinGame();
                });
            }
            else
            {
                string txt = GameCenter.mIns.m_LanMgr.GetLan("login_12") + code + ",msg:" + content;
                txtStatus.text = txt;
                GameCenter.mIns.m_UIMgr.PopWindowPrefab(new PopWinStyle(GameCenter.mIns.m_LanMgr.GetLan("login_15"), txt, 2, (confirm) =>
                {
                    if (confirm == 1)
                    {
                        //重试读取服务器列表
                        socketServerLogin();
                    }
                    else
                    {
                        //退出游戏
                        GameCenter.mIns.ExitGame();
                    }
                }, new List<string> { GameCenter.mIns.m_LanMgr.GetLan("login_4"), GameCenter.mIns.m_LanMgr.GetLan("login_5") }));
            }
        });
    }

    private async void joinGame()
    {
        txtStatus.text = GameCenter.mIns.m_LanMgr.GetLan("login_13");
        GameCenter.mIns.m_NetMgr.LoginSucc();
        if (GameTiJi.TijiConfig.visitor)
        {
            // 保存本次登录账号到本地
            datatool.setstring("lastUserId", ipt_openid.text);
        }
        //拉取所有关卡信息
        await GameCenter.mIns.userInfo.InitCompletedGameLevelDic();
        //拉取NPC信息
        await NpcInteractionManager.Instance.Init();
        await GameLobbyManager.Instance.Join();
        //拉取邮件信息
        MailManager.Instance.GetServerMailList();
        await SpriteManager.Instance.LoadCommonAtlas();//加载常驻图集，可以移到后面登录 界面操作
        await UniTask.Delay(50);
        //asyncLoad();
        txtStatus.text = GameCenter.mIns.m_LanMgr.GetLan("login_14");
        // 登陆成功, 进入主界面
        // 加载主场景
        GameSceneManager.Instance.LoadMainScene();
       // await UniTask.Delay(200);
        GameCenter.mIns.m_UIMgr.Open<mainui>();
        this.Close(false, true);
        
        getOffLinePushInfo();

       
    }

    private void getOffLinePushInfo()
    {
        JsonData data = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        if (data.ContainsKey("roleid"))
        {
            GameCenter.mIns.m_NetMgr.SendData(NetCfg.SOCKET_COMMENTID_OFFLINE_PUSH, data, (seqid, code, content) =>
            {
                if (code == 0)
                {
                    GameCenter.mIns.m_NetMgr.SetOfflineCodes(content);
                }
                else
                {
                    if (code == -2)
                        zxlogger.logerror("Error: 获取离线通知失败: 非法参数! code:" + code);
                    else
                        zxlogger.logerror("Error:  获取离线通知失败: code:" + code);
                }
            });
        }
    }
}
