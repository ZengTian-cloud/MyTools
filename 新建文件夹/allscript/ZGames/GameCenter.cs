using System;
using System.Collections;
using System.Collections.Generic;
using Basics;
using GameTiJi;
using HybridCLR;
using LitJson;
using Managers;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using YooAsset;

public class GameCenter : MonoBehaviour
{
    private static GameCenter _inst = null;
    public static GameCenter mIns
    {
        get
        {
            return _inst;
        }
    }


    private static void CheckGameCenter()
    {
        Debug.Log("GIANTGAME CheckGameCenter---------------------------");
        GameObject gameCenterObj = GameObject.Find("gamecenter");
        if (gameCenterObj == null)
        {
            ResourcePackage resourcePackage = YooAssets.GetPackage("DefaultPackage");
            AssetOperationHandle handle = resourcePackage.LoadAssetSync<UnityEngine.Object>("Assets/afirsts/prefabs/gamecenter.prefab");
            handle.Completed += (handle) =>
            {
                gameCenterObj = handle.InstantiateSync();
                gameCenterObj.name = "gamecenter";
                gameCenterObj.transform.localPosition = Vector3.zero;
                gameCenterObj.transform.localScale = Vector3.one;
                gameCenterObj.transform.localEulerAngles = Vector3.zero;
                DontDestroyOnLoad(gameCenterObj);
                Debug.Log("-----GIANTGAME gameCenterObj--------------:" + gameCenterObj);

            };
        }
    }

    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private void loadMetadataForAOTAssemblies()
    {
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误

        List<string> aotMetaAssemblyFiles = new List<string>()
        {
            "Assets/afirsts/codes/mscorlib.dll.bytes",
            "Assets/afirsts/codes/System.dll.bytes",
            "Assets/afirsts/codes/System.Core.dll.bytes",
            "Assets/afirsts/codes/AnumationInstance.dll.bytes",
            "Assets/afirsts/codes/Crypto.dll.bytes",
            "Assets/afirsts/codes/CSharpZip.dll.bytes",
            "Assets/afirsts/codes/DOTween.dll.bytes",
            "Assets/afirsts/codes/LitJson.dll.bytes",
            "Assets/afirsts/codes/Mono.Security.dll.bytes",
            "Assets/afirsts/codes/SerializableDictionary.dll.bytes",
            "Assets/afirsts/codes/YooAsset.dll.bytes",
            "Assets/afirsts/codes/Unity.Timeline.dll.bytes",
            "Assets/afirsts/codes/Cinemachine.dll.bytes",
            "Assets/afirsts/codes/UniTask.dll.bytes",
            "Assets/afirsts/codes/GameTiJi.dll.bytes",
        }; 

        HomologousImageMode mode = HomologousImageMode.SuperSet;
        var package = YooAssets.GetPackage("DefaultPackage");
        foreach (var aotDllName in aotMetaAssemblyFiles)
        {
            RawFileOperationHandle handle = package.LoadRawFileAsync(aotDllName);
            handle.Completed += (handle) =>
            {
                byte[] dllBytes = handle.GetRawFileData();
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"GIANTGAME LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            };
        }
    }

    public static JsonData updateData = null;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="isHot">是否是编辑器</param>
    /// <param name="data">更新检测数据</param>
    public static void EnterGame(JsonData data)
    {
        Debug.Log($"GIANTGAME gamecenter EnterGame START----------------------data:{data}");
        updateData = data;
        //LoadMetadataForAOTAssemblies();
        CheckGameCenter();
        
    }

    public EPlayMode EPlayMode;

    public GameInfo gameInfo = new GameInfo();
    public UserInfo userInfo = new UserInfo();
    public HeroGrowCfg heroGrowCfg = new HeroGrowCfg();//记录养成功能的信息
    public GameObject m_RootUI;
    public DownLoadManager m_DownLoadMgr { get; private set; }

    public HttpManager m_HttpMgr { get; private set; }
    public NetManager m_NetMgr { get; private set; }
    //public HotUpdateManager m_HotUpdateMgr { get; private set; }
    public PostProcessManager m_PostProcessMgr { get; private set; }
    // public ResManager m_ResMgr { get; private set; }
    public SdkManager m_SdkMgr { get; private set; }
    public PointLightManager m_pointMgr { get; private set; }
    public UIManager m_UIMgr { get; private set; }
    public CoreObjectManager m_CoreObjMgr { get; private set; }
    public BattleManager m_BattleMgr { get; private set; }
    public LanManager m_LanMgr { get; private set; }
    public CfgManager m_CfgMgr { get; private set; }
    public CameraManager m_CamMgr { get; private set; }
    public CoroutineManager m_CoroutineMgr { get; private set; }

    public QualityManager m_QualityManager { get; private set; }

    public FogManager m_FogManager { get; private set; }
    public ResourcesManager m_ResManager { get; private set; }
    // public UnityEngine.iOS.LocalNotification m_LocalNotification { get; private set; }

    private int lastWidth = 0;
    private int lastHeight = 0;
    private int checkIdx = 0;
    public IEnumerator LaunchObjects()
    {
        var listobject = GetComponents<SingletonOjbect>();
        if (listobject != null)
        {
            for (int i = 0; i < listobject.Length; i++)
            {
                yield return listobject[i].LaunchOne();
            }
        }
    }

    public void CheckScreenRatio()
    {
        if (m_RootUI == null)
        {
            return;
        }
        lastWidth = zxconfig.screenwidth;
        lastHeight = zxconfig.screenheight;

        var desratio = (float)zxconfig.designwidth / zxconfig.designheight;
        var temratio = (float)zxconfig.screenwidth / zxconfig.screenheight;

        float w = zxconfig.designwidth;
        float h = zxconfig.designheight;
        if (temratio < desratio) // 锁高
            w = zxconfig.designheight * temratio;
        else // 锁宽
            h = zxconfig.designwidth / temratio;

        if (zxconfig.blandscape)
        {
            // 横屏
            zxconfig.adaptwidth = Mathf.CeilToInt(w);
            zxconfig.adaptheight = Mathf.CeilToInt(h);
            zxconfig.usablewidth = Mathf.CeilToInt(w);
            zxconfig.usableheight = Mathf.CeilToInt(h);
            //if (temratio > 1.93f)
            //{
            //    zxconfig.adaptheight = zxconfig.designheight;
            //    zxconfig.adaptwidth = Mathf.CeilToInt(zxconfig.adaptheight * temratio);
            //    zxconfig.usableheight = zxconfig.designheight;
            //    zxconfig.usablewidth = zxconfig.adaptwidth - zxconfig.maxunusedlen;
            //}
            //else
            //{
            //    zxconfig.adaptwidth = zxconfig.designwidth;
            //    zxconfig.adaptheight = Mathf.CeilToInt(zxconfig.adaptwidth / temratio);
            //    zxconfig.usablewidth = zxconfig.adaptwidth;
            //    zxconfig.usableheight = zxconfig.adaptheight;
            //}
        }
        else
        {
            // 竖屏
            if (temratio < 0.518f)
            {
                zxconfig.adaptwidth = zxconfig.designwidth;
                zxconfig.adaptheight = Mathf.CeilToInt(zxconfig.adaptwidth / temratio);
                zxconfig.usablewidth = zxconfig.designwidth;
                zxconfig.usableheight = zxconfig.adaptheight - zxconfig.maxunusedlen;
            }
            else
            {
                zxconfig.adaptheight = zxconfig.designheight;
                zxconfig.adaptwidth = Mathf.CeilToInt(zxconfig.adaptheight * temratio);
                zxconfig.usableheight = zxconfig.adaptheight;
                zxconfig.usablewidth = zxconfig.adaptwidth;
            }
        }

        // 设置节点适配
        var vcsex = m_RootUI.GetComponentsInChildren<CanvasScalerEx>(true);
        if (vcsex != null)
        {
            for (var i = 0; i < vcsex.Length; i++)
            {
                vcsex[i].DoRefresh();
            }
        }
        var vadaptui = m_RootUI.GetComponentsInChildren<AdaptUI>(true);
        if (vadaptui != null)
        {
            for (var i = 0; i < vadaptui.Length; i++)
            {
                vadaptui[i].DoRefresh();
            }
        }
        var vadaptignore = m_RootUI.GetComponentsInChildren<AdaptIgnore>(true);
        if (vadaptignore != null)
        {
            for (var i = 0; i < vadaptignore.Length; i++)
            {
                vadaptignore[i].DoRefresh();
            }
        }
        var vadpatbg = m_RootUI.GetComponentsInChildren<AdaptBG>(true);
        if (vadpatbg != null)
        {
            for (var i = 0; i < vadpatbg.Length; i++)
            {
                vadpatbg[i].DoRefresh();
            }
        }
        // m_LuaMgr.adaptEvent?.Invoke();
    }

    private TChild AddComponent<TChild>() where TChild : SingletonOjbect
    {
        TChild childcomp = gameObject.GetComponent<TChild>();
        return childcomp ? childcomp : gameObject.AddComponent<TChild>();
    }

    private void Awake()
    {
#if !UNITY_EDITOR
        Debug.Log("GIANTGAME Awake-----------------------------------");
        loadMetadataForAOTAssemblies();
#endif

        Debug.Log("GIANTGAME 初始化绑定类-----------------------------------");
        _inst = GetComponent<GameCenter>();
        m_DownLoadMgr = AddComponent<DownLoadManager>();
        //m_HotUpdateMgr = AddComponent<HotUpdateManager>();
        m_HttpMgr = AddComponent<HttpManager>();
        m_NetMgr = AddComponent<NetManager>();
        // m_LuaMgr = AddComponent<LuaManager>();
        m_PostProcessMgr = AddComponent<PostProcessManager>();
        // m_ResMgr = AddComponent<ResManager>();
        m_SdkMgr = AddComponent<SdkManager>();
        m_pointMgr = AddComponent<PointLightManager>();
        m_UIMgr = AddComponent<UIManager>();
        m_BattleMgr = AddComponent<BattleManager>();
        m_BattleMgr.enabled = false;
        m_LanMgr = AddComponent<LanManager>();
        m_CfgMgr = AddComponent<CfgManager>();
        m_CoroutineMgr = AddComponent<CoroutineManager>();
        m_QualityManager = AddComponent<QualityManager>();
        m_FogManager = AddComponent<FogManager>();
        m_ResManager = AddComponent<ResourcesManager>();
        // m_LocalNotification = AddComponent<UnityEngine.iOS.LocalNotification>();

        ResourcePackage resourcePackage = YooAssets.GetPackage("DefaultPackage");
        gameInfo.appVer = Application.version;
        gameInfo.codeVer = resourcePackage.GetPackageVersion();
        gameInfo.Deviceid = TalkingDataSDK.GetDeviceId();
        gameInfo.Os = TijiConfig.getOs();
        gameInfo.Hv = "1";
        gameInfo.Lan = TijiConfig.getLanguage();
        gameInfo.Appid = "1";
        Sdk.init();
        string sdkinfo = Sdk.GetSdkChannelId();
        //挂载PUSH消息处理器
        m_NetMgr.pushHandle = (msg) =>
        {
            PushHandle.Inst.handlePushMessage(msg);
        };

        Debug.Log("_inst:" + _inst);
        InitUIRoot();
    }

    private void Start()
    {

    }

    private void LateUpdate()
    {
        checkIdx++;
        if (checkIdx >= 50)
        {
            checkIdx = 0;
            if (zxconfig.screenwidth != lastWidth || zxconfig.screenheight != lastHeight)
            {
                CheckScreenRatio();
            }
        }
        else
        {
            checkIdx++;
        }
        // m_LuaMgr.lateEvent?.Invoke();
    }

    private void OnDestroy()
    {

    }


    /// <summary>
    /// 退出游戏
    /// </summary>
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
	Application.Quit();
#endif

    }

    public void reloadGame()
    {
        SceneManager.LoadScene(zxconfig.launchscene);
    }

    public void restartGame()
    {

        if (Application.isEditor) return;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                const int kIntent_FLAG_ACTIVITY_CLEAR_TASK = 0x00008000;
                const int kIntent_FLAG_ACTIVITY_NEW_TASK = 0x10000000;

                var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
                var intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);

                intent.Call<AndroidJavaObject>("setFlags", kIntent_FLAG_ACTIVITY_NEW_TASK | kIntent_FLAG_ACTIVITY_CLEAR_TASK);
                currentActivity.Call("startActivity", intent);
                currentActivity.Call("finish");
                var process = new AndroidJavaClass("android.os.Process");
                int pid = process.CallStatic<int>("myPid");
                process.CallStatic("killProcess", pid);
            }

        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            //测试只有下面俩种类型好用，FatalError几率卡界面
            UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.FatalError);
            //UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.PureVirtualFunction);
        }
        else
        {
            Application.Quit();
        }

    }

    private void InitUIRoot()
    {
        Debug.Log("GIANTGAME InitUIRoot-----------------------------------");
        // 设置游戏运行模式
#if UNITY_EDITOR
        EPlayMode = YooAsset.EPlayMode.EditorSimulateMode;
#else
        EPlayMode = YooAsset.EPlayMode.OfflinePlayMode;
#endif

        //加载uiroot
        ResourcePackage resourcePackage = YooAssets.GetPackage("DefaultPackage");
        AssetOperationHandle handle = resourcePackage.LoadAssetSync<UnityEngine.Object>("Assets/afirsts/prefabs/[uiroot].prefab");
        handle.Completed += (handle) =>
        {
            m_RootUI = handle.InstantiateSync();
            m_RootUI.name = "[uiroot]";
            m_RootUI.transform.localPosition = Vector3.zero;
            m_RootUI.transform.localScale = Vector3.one;
            m_RootUI.transform.localEulerAngles = Vector3.zero;
            m_RootUI.transform.SetAsFirstSibling();
            CheckScreenRatio();
            // 启动游戏
            StartCoroutine(LaunchGameInit());
        };
    }

    private IEnumerator LaunchGameInit()
    {
        Debug.Log("GIANTGAME LaunchGameInit-----------------------------------");
        GameNode.Launch();
        //yield return m_HotUpdateMgr.LoadVersionData();
        yield return LaunchObjects();
        // cid
        zxconfig.channelstr = string.Empty;
        string channelpath = pathtool.combine(pathtool.reqabpackpath, zxconfig.channelpath);
        using (UnityWebRequest trequest = UnityWebRequest.Get(channelpath))
        {
            yield return trequest.SendWebRequest();
            if (string.IsNullOrEmpty(trequest.error))
            {
                zxconfig.channelstr = trequest.downloadHandler.text;
            }
        }

        gameObject.AddComponent<ResourcesManager>();
        Debug.Log("GIANTGAME 加载必备公共资源包 加载完成后开始热更新-----------------------------------");
        //加载必备公共资源包，加载完成后开始热更新
        ResourcesManager.Instance.Init(() =>
        {
            Debug.Log("GIANTGAME 公共资源AB包加载完成");
            // 配置
            m_LanMgr.Init();
            Debug.Log("GIANTGAME 语言信息加载完成");
            m_UIMgr.OnInit();
            Debug.Log("GIANTGAME UI初始化完成");
            // 相机初始化
            m_CamMgr = AddComponent<CameraManager>();
            m_CamMgr.InitUICamera();

            m_UIMgr.Open<login>();
        });

    }

    public void RunWaitCoroutine(Action callback, float time = 0)
    {
        StartCoroutine(WaitSecond(callback, time));
    }

    private IEnumerator WaitSecond(Action callback, float time = 0)
    {
        if (time == 0)
        {
            yield return null;
        }
        else
        {
            yield return new WaitForSeconds(time);
        }
        callback?.Invoke();
    }
}