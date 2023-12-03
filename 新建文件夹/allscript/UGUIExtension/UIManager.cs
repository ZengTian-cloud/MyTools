using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Basics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;
using System.Collections.Concurrent;
using Managers;
using Cysharp.Threading.Tasks;
using System.Collections;

public enum UILayerType : int
{
    // None
    None = -1,
    // Backgroud->节点bgwnd
    Backgroud = 1,
    //登录 界面
    Login = 50,
    // Normal->节点uiwnd1
    Normal = 100,
    // Float->节点uiFloat
    Float = 300,
    // Pop->节点uiwnd2
    Pop = 500,
}

public enum UIStatusType
{
    // None
    None = -1,
    // 加载中
    Lading = 1,
    // 激活的
    Active = 2,
    // 隐藏的
    Hidden = 3,
    // 冻结的
    Frozen = 4,
    // 待销毁（已回收缓存）
    WaitDestroy = 5,
    // 已销毁
    Destroyed = 6,
}

public partial class UIManager : SingletonOjbect
{
    //ui相机
    public Camera UICamera { get; private set; }
    public int curCount;
    public EventSystem eventSystem;

    //当前打开的所有ui界面
    public Dictionary<UILayerType, List<BaseWin>> openWinList = new Dictionary<UILayerType, List<BaseWin>>();
    //缓存列表的所有ui界面
    private Dictionary<Type, BaseWin> cacheWinList = new Dictionary<Type, BaseWin>();
    //执行update的界面
    private List<BaseWin> winUpdateList = new List<BaseWin>();
    //是否执行update
    private bool bUpdate = true;

    //存放normal类型界面的列表，用作界面的打开关系逻辑
    private List<BaseWin> NormalWinStack = new List<BaseWin>();

    //ui根节点
    public GameObject uiRoot;
    //baseui_root
    private Transform baseUIRoot;
    //ui缓存节点
    private Transform uiCacheRoot;
    //ui层级节点
    public Dictionary<UILayerType, Transform> winRootDict = new Dictionary<UILayerType, Transform>();

    public Transform uiTopsideRoot;

    //初始化ui
    public void OnInit()
    {
        uiRoot = GameCenter.mIns.m_RootUI;
        baseUIRoot = uiRoot.transform.Find("baseui_root");
        uiCacheRoot = uiRoot.transform.Find("uiCacheRoot");
        UICamera = GameObject.Find("mainCamera/wndcam").GetComponent<Camera>();//UI相机

        GameObject bgmask = baseUIRoot.transform.Find("bgmask").gameObject;
        GameObject window = baseUIRoot.transform.Find("window").gameObject;
        if (bgmask != null)
        {
            Canvas canvas = bgmask.GetComponent<Canvas>();
            if (canvas != null) { canvas.worldCamera = UICamera; }
            bgmask.gameObject.SetActive(false);//bgmask下面有个button  会影响easytouch对输入事件的判断 先屏掉
        }

        if (window != null)
        {
            Canvas canvas = window.GetComponent<Canvas>();
            if (canvas != null) { canvas.worldCamera = UICamera; }
        }

        if (uiCacheRoot != null)
        {
            uiCacheRoot.gameObject.SetActive(false);
        }
        InitLayerRoot();
    }

    /// <summary>
    /// 初始化层级节点
    /// </summary>
    public void InitLayerRoot()
    {
        SetUIWinLayer(UILayerType.Backgroud, "bgwnd");
        SetUIWinLayer(UILayerType.Login, "uiwnd1");
        SetUIWinLayer(UILayerType.Normal, "uiwnd1");
        SetUIWinLayer(UILayerType.Float, "uiwnd1");
        SetUIWinLayer(UILayerType.Pop, "uiwnd2");

        uiTopsideRoot = baseUIRoot.Find("window/rootwnd/fadewnd");
        Canvas c = uiTopsideRoot.gameObject.AddComponent<Canvas>();
        uiTopsideRoot.gameObject.AddComponent<GraphicRaycaster>();
        if (c != null)
        {
            c.overrideSorting = true;
            c.sortingOrder = 1000;
        }

    }

    /// <summary>
    /// 设置ui不同类型节点layer
    /// </summary>
    /// <param name="layerType"></param>
    /// <param name="rootName"></param>
    public void SetUIWinLayer(UILayerType layerType, string rootName)
    {
        Transform layerRoot = baseUIRoot.Find("window/rootwnd").Find(rootName);
        if (layerRoot == null)
        {
            Debug.LogError($"未在uiroot下找到对应的层级节点：{rootName}，layer：{layerType}，请检查!");
            return;
        }
        if (winRootDict.ContainsKey(layerType))
        {
            Debug.LogError($"重复初始化uiroot下层级节点：{rootName}，layer：{layerType}，请检查!");
            return;
        }
        winRootDict.Add(layerType, layerRoot);
        Canvas canvas = layerRoot.gameObject.AddComponent<Canvas>();
        layerRoot.gameObject.AddComponent<GraphicRaycaster>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = (int)layerType;
        }
    }

    /// <summary>
    /// ui时间轮询器
    /// </summary>
    public void Update()
    {
        UpdateThreadPreQueue();
        if (bUpdate)
        {
            for (int i = 0; i < winUpdateList.Count; i++)
            {
                winUpdateList[i].UpdateWin();
            }
        }
        DoPopUpdate();
    }


    // 主线程预处理方法队列，非主线程需要处理unity相关资源对象操作可加入此队列处理
    private Queue<Action> mainThreadPreQueue = new Queue<Action>();
    /// <summary>
    /// 加入主线程队列
    /// </summary>
    /// <param name="action"></param>
    public void EnThreadPreQueue(Action action)
    {
        mainThreadPreQueue.Enqueue(action);
    }

    private void UpdateThreadPreQueue()
    {
        while (mainThreadPreQueue != null && mainThreadPreQueue.Count > 0)
        {
            mainThreadPreQueue.Dequeue()?.Invoke();
        }
    }


    /// <summary>
    /// 界面注册到update列表中
    /// </summary>
    /// <param name="win">注册的win</param>
    public void AddUpdateWin(BaseWin win)
    {
        if (winUpdateList.Contains(win))
        {
            return;
        }
        winUpdateList.Add(win);
    }

    /// <summary>
    /// 获得UI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Get<T>() where T : BaseWin,new()
    {
        var v = typeof(T);
        string uiName = v.Name.ToLower();

        //是否已打开
        foreach (KeyValuePair<UILayerType, List<BaseWin>> kv in openWinList)
        {
            foreach (var ui in kv.Value)
            {
                if (ui.UIName == uiName)
                {
                    return ui as T;
                }
            }
        }
        //是否在缓存列表
        BaseWin win = GetFromCacheList(uiName);
        if (win == null)
        {
            return null;
        }    
        return win as T;
    }

    /// <summary>
    /// 打开一个ui
    /// </summary>
    /// <typeparam name="T">打开的ui</typeparam>
    /// <returns></returns>
    public T Open<T>(params object[] args) where T : BaseWin, new()
    {
        var v = typeof(T);
        //先获取已加载或缓存
        var ret = Get<T>();
        string uiName = v.Name.ToLower();
        //未获取到，新生成
        if (ret == null)
        {
            ret = new T();
            this.LoadPrefab(ret, uiName);
        }

        //开始ui的额外参数
        if (args!=null)
        {
            ret.openArgs = args;
        }
        ret.UIName = uiName;
        ret.ResetActive(winRootDict[ret.uiLayerType]);
        ret.Open();
        return ret;
    }

    public async void LoadPrefab(BaseWin win, string uiName)
    {
        GameObject o = await ResourcesManager.Instance.LoadUIPrefabAndAtlasSync(uiName, win.uiAtlasName);
        if (o != null)
        {
            if (winRootDict.ContainsKey(win.uiLayerType))
            {
                o.transform.SetParent(winRootDict[win.uiLayerType]);
            }
            o.name = win.Prefab;
            o.transform.localScale = Vector3.one;
            win.LoadWin(baseUIRoot, o);
        }
    }

    /// <summary>
    /// 获取相同类型ui的sortingorder，默认间隔10
    /// </summary>
    /// <param name="win">对应ui</param>
    /// <returns>当前所在layer值</returns>
    public int GetUIAutoSortingOrder(BaseWin win)
    {
        int layerNumber = -100;
        int layerInterval = 10;
        if (win == null)
        {
            return layerNumber;
        }

        if (!openWinList.ContainsKey(win.uiLayerType))
        {
            openWinList.Add(win.uiLayerType, new List<BaseWin>());
        }

        foreach (var ui in openWinList[win.uiLayerType])
        {
            if (ui.uiLayerType == win.uiLayerType)
            {
                if (layerNumber == -100)
                {
                    layerNumber = (int)win.uiLayerType;
                }
                layerNumber = layerNumber + layerInterval;
            }
        }
        return layerNumber == -100 ? (int)win.uiLayerType : layerNumber;
    }

    

    //当前打开的界面
    public BaseWin curNormalOpen;
    /// <summary>
    /// 将打开的ui加入打开列表
    /// </summary>
    /// <param name="w">加入的win</param>
    public void AddOpenWin(BaseWin w, bool addStack = true)
    {
        if (IsOpenedUI(w))
        {
            Debug.LogError($"尝试打开一个已打开的UI:{w.UIName},请检查！");
            return;
        }
        if (!openWinList.ContainsKey(w.uiLayerType))
        {
            openWinList.Add(w.uiLayerType, new List<BaseWin>());
        }
        openWinList[w.uiLayerType].Add(w);
        if (w.uiLayerType == UILayerType.Normal && addStack)
        {
            if (NormalWinStack.Contains(w))
            {
                NormalWinStack.Remove(w);
            }
            NormalWinStack.Add(w);
        }

        if (w.uiLayerType == UILayerType.Normal || w.UIName == "mainui")
            curNormalOpen = w;


        //如果打开的ui是normal层级或者float层级，关闭上一个normal层级的ui
        if (w.uiLayerType == UILayerType.Normal || w.uiLayerType == UILayerType.Float)
        {
            if (w.uiLayerType == UILayerType.Normal)
            {
                if (NormalWinStack.Count >= 2)
                {
                    NormalWinStack[NormalWinStack.Count - 2].Close(false);
                }
            }
            else if (w.uiLayerType == UILayerType.Float)
            {
                if (NormalWinStack.Count >= 1)
                {
                    NormalWinStack[NormalWinStack.Count - 1].Close(false);
                }
            }
            
            
            // 打开的是normalui或者float,则关闭mainui
            mainui _mui = Get<mainui>() as mainui;
            if (_mui != null)
            {
                _mui.Close();
            }
        }
        

    }


    /// <summary>
    /// 移除一个打开的ui
    /// </summary>
    /// <param name="w"></param>
    /// <param name="bShowLast">是否展示上一个界面</param>
    public void RemoveOpenWin(BaseWin w,bool bShowLast = true)
    {
        if (!openWinList.ContainsKey(w.uiLayerType))
        {
            Debug.LogError($"尝试关闭一个未打开的UI:{w.UIName}，请检查");
            return;
        }
        
        for (int i = openWinList[w.uiLayerType].Count - 1; i >= 0; i--)
        {
            if (openWinList[w.uiLayerType][i] == w)
            {
                openWinList[w.uiLayerType].RemoveAt(i);
                w.Close();
            }
        }
       
        winUpdateList.Remove(w);
        PushToCacheList(w);

        //是否有前置界面
        bool hasLast = false;
        //如果关闭的ui是normal层级或者float层级，打开前一个normal层级
        if (bShowLast)
        {
            int count = NormalWinStack.Count;
            if (w.uiLayerType == UILayerType.Normal)
            {
                if (count >= 2)//开启stack中倒数第二个
                {
                    hasLast = true;
                    BaseWin @base = NormalWinStack[count - 2];
                    NormalWinStack.RemoveAt(count - 1);
                    @base.Open(false);
                }  
            }
            else if (w.uiLayerType == UILayerType.Float)
            {
                if (count >= 1)//开启stack中最后一个
                {
                    hasLast = true;
                    BaseWin @base = NormalWinStack[count - 1];
                    @base.Open(false);
                }
            }
        }

        // 如果没有normal和float类型ui，则显示主界面
        bool bShowMain = false;
        if (w.uiLayerType == UILayerType.Normal)
        {
            bShowMain = (openWinList.ContainsKey(UILayerType.Normal) && openWinList[UILayerType.Normal].Count <= 0) && (openWinList.ContainsKey(UILayerType.Float) ? openWinList[UILayerType.Float].Count <= 0 : true);//是否存在normal界面
        }
        else if (w.uiLayerType == UILayerType.Float)
        {
            bShowMain = (openWinList.ContainsKey(UILayerType.Normal) && openWinList[UILayerType.Normal].Count <= 0) && (openWinList.ContainsKey(UILayerType.Float) && openWinList[UILayerType.Float].Count <= 0);
        }
        if (bShowMain && !hasLast)
        {
            Open<mainui>();
            NormalWinStack.Clear();//回到主界面，清空列表
        }
    }

    /// <summary>
    /// 是否是打开的ui
    /// </summary>
    /// <param name="w">window</param>
    /// <returns>是否存在</returns>
    public bool IsOpenedUI(BaseWin w)
    {
        if (!openWinList.ContainsKey(w.uiLayerType))
        {
            return false;
        }

        foreach (var ui in openWinList[w.uiLayerType])
        {
            if (ui == w)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 是否是打开的ui
    /// </summary>
    /// <param name="uiName">uiname</param>
    /// <returns>是否存在</returns>
    public bool IsOpenedUI(string uiName)
    {
        foreach (KeyValuePair<UILayerType, List<BaseWin>> kv in openWinList)
        {
            foreach (var ui in kv.Value)
            {
                if (ui.UIName == uiName)
                {
                    return true;
                }
            }
        }

        return false;
    }

   
    /// <summary>
    /// 将一个ui放入缓存
    /// </summary>
    /// <param name="w">w</param>
    private void PushToCacheList(BaseWin w)
    {
        cacheWinList.TryAdd(w.GetType(), w);
        if (uiCacheRoot!= null)
        {
            w.uiRoot.transform.SetParent(uiCacheRoot);
        }
    }

    /// <summary>
    /// 从ui缓存列表中获取ui
    /// </summary>
    /// <param name="uiName">uiname</param>
    /// <returns>缓存的ui</returns>
    private BaseWin GetFromCacheList(string uiName)
    {
        Type tk = null;
        BaseWin w = null;
        foreach (KeyValuePair<Type, BaseWin> kv in cacheWinList)
        {
            if (kv.Value.UIName == uiName)
            {
                tk = kv.Key;
                w = kv.Value;
            }
        }
        if (tk != null && w != null)
        {
            w.ResetActive();
        }
        return w;
    }

    /// <summary>
    /// 从缓存列表移除
    /// </summary>
    /// <param name="uiName"></param>
    public void RemoveFromCacheList(string uiName)
    {
        Type tk = null;
        foreach (KeyValuePair<Type, BaseWin> kv in cacheWinList)
        {
            if (kv.Value.UIName == uiName)
                tk = kv.Key;
        }
        if (tk != null)
        {
            cacheWinList.Remove(tk);
        }
    }

    /// <summary>
    /// 销毁一个ui
    /// </summary>
    /// <typeparam name="T">ui</typeparam>
    public async void DestroyWin<T>() where T : BaseWin, new()
    {
        if (cacheWinList.ContainsKey(typeof(T)))
            DestroyWin(Get<T>());
    }


    /// <summary>
    /// 销毁一个ui
    /// </summary>
    /// <param name="w"></param>
    public void DestroyWin(BaseWin w)
    {
        //if (updating)
        //{
        //    //将要销毁
        //    //toDestroy.Add(w.GetType());
        //}
        //else
        //{
            RemoveOpenWin(w);
            cacheWinList.Remove(w.GetType());
            winUpdateList.Remove(w);
            w.DestroyUI();
        //}
    }




    #region PopWindow UI
    /// <summary>
    /// 通用弹窗ui，只能存在一个
    /// </summary>
    /// <param name="callback"></param>
    public async void PopWindow(string title, string content, string left, Action leftCB, string right, Action rightCB)
    {
        CommonPopWin _commonPopWin = Open<CommonPopWin>();
        if (_commonPopWin != null )
        {
            _commonPopWin.RegisterClickListen(title, content, left, leftCB, right, rightCB);
        }
    }

    /// <summary>
    /// 通用弹窗 可调用多次
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="prefabName">预置名称</param>
    public void PopWindowPrefab(string title,BaseWindow window)
    {
        CommonPopWinPrefabUI _commonPopWin = Open<CommonPopWinPrefabUI>();
        if (_commonPopWin != null)
        {
            _commonPopWin.init(title, window);
        }
    }
    #endregion



}