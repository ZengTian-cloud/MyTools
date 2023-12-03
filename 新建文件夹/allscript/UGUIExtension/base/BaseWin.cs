using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.TextCore.Text;
using Cysharp.Threading.Tasks;

/// <summary>
/// 界面基类
/// </summary>
public abstract class BaseWin
{
    //ui层级
    public abstract UILayerType uiLayerType { get; }
    public abstract string uiAtlasName { get; }
    public UIStatusType UIStatus { get; private set; }
    //预制体名
    public abstract string Prefab { get; }

    // 打开界面参数
    public object[] openArgs { get; set; }
    public string UIName { get; set; }

    public Animator anim;

    public Canvas uiCanvas = null;
    public int sortingOrder;

    // 存放button控件
    public List<Button> btns = new List<Button>();
    // 存放toogle控件
    public List<Toggle> toggles = new List<Toggle>();

    //ui预支体
    public GameObject uiRoot;
    //baseui_root
    public Transform baseUIRoot;

    //世界节点
    public Transform worldRoot;

    private List<BaseWin> popListBeforeJump;//记录该界面跳转前的所有弹窗
    private bool inJump;//是否是跳转进入




    public bool isRegister = false;
    /// <summary>
    /// 加载界面
    /// </summary>
    /// <param name="baseui"></param>
    /// <param name="root"></param>
    public void LoadWin(Transform baseui, GameObject root)
    {
        UIStatus = UIStatusType.None;

        //todo:加载改界面所需要的 ab资源

        uiRoot = root;
        baseUIRoot = baseui;
        worldRoot = baseUIRoot.transform.Find("world");

        // canvas
        if (root.GetComponent<Canvas>() == null)
            uiCanvas = uiRoot.AddComponent<Canvas>();
        else
            uiCanvas = root.GetComponent<Canvas>();
        if (root.GetComponent<GraphicRaycaster>() == null)
            root.AddComponent<GraphicRaycaster>();

        root.transform.localScale = Vector3.one;
        RectTransform uiRect = root.GetComponent<RectTransform>();
        uiRect.anchoredPosition = Vector2.zero;
        uiRect.localScale = Vector3.one;
        uiRect.anchorMax = new Vector2(1, 1);
        uiRect.anchorMin = new Vector2(0, 0);
        uiRect.pivot = new Vector2(0.5f, 0.5f);
        uiRect.anchoredPosition = Vector2.zero;
        uiRect.offsetMin = new Vector2(0, 0);
        uiRect.offsetMax = new Vector2(0, 0);

        ResetActive();
        InitUI();
        OnInit();
    }

    /// <summary>
    /// 打开界面时运行
    /// </summary>
    public void Open(bool addStack = true)
    {
        if (UIStatus == UIStatusType.Active)
        {
            return;
        }
        if (this.uiRoot == null)
        {
            Debug.LogError($"ui界面:{this.UIName}加载失败，请检查");
            return;
        }

        this.uiRoot.transform.SetParent(GameCenter.mIns.m_UIMgr.winRootDict[this.uiLayerType]);
        GameCenter.mIns.m_UIMgr.RemoveFromCacheList(this.UIName);

        OnRegister(true);
        GameCenter.mIns.m_UIMgr.AddOpenWin(this, addStack);
        ChangedStatus(UIStatusType.Active);
        OnOpen();
    }

    /// <summary>
    /// 跳转到目标界面
    /// </summary>
    public T DoJump<T>(params object[] args) where T : BaseWin, new()
    {
        T win = GameCenter.mIns.m_UIMgr.Open<T>();
        return win;
    }


    public void ChangedStatus(UIStatusType newStatus)
    {
        if (UIStatus == newStatus)
        {
            return;
        }
        UIStatus = newStatus;
    }

    public int GetSortingGroupOrder()
    {
        //return (int)SortingGroup * 10;
        return 0;
    }

    bool EnableAnim()
    {
        return anim != null && anim.isActiveAndEnabled;
    }

    

    /// <summary>
    /// 重新激活界面
    /// </summary>
    /// <param name="parent"></param>
    public void ResetActive(Transform parent = null)
    {
        if (uiRoot != null)
        {
            if (parent != null)
            {
                uiRoot.transform.SetParent(parent);
            }
            uiCanvas.overrideSorting = true;
            sortingOrder = GameCenter.mIns.m_UIMgr.GetUIAutoSortingOrder(this);
            uiCanvas.sortingOrder = sortingOrder;
        }
    }

    /// <summary>
    /// 关闭界面
    /// </summary>
    /// <param name="bShowLast">关闭后是否自动打开上一个normal界面</param>
    /// <param name="bDestroy">是否直接销毁ui（不进缓存）</param>
    public void Close(bool bShowLast = true,bool bDestroy = false)
    {
        if (UIStatus == UIStatusType.WaitDestroy || UIStatus == UIStatusType.Destroyed)
        {
            return;
        }
        ChangedStatus(UIStatusType.WaitDestroy);

        GameCenter.mIns.m_UIMgr.RemoveOpenWin(this, bShowLast);
        OnRegister(false);
        OnClose();
        if (bDestroy)
        {
            DestroyUI();
        }
    }

    /// <summary>
    /// 销毁ui
    /// </summary>
    public void DestroyUI()
    {
        if (this.uiRoot!= null)
        {
            GameObject.Destroy(this.uiRoot.gameObject);
        }
        OnDestroy();
    }

    /// <summary>
    /// ui动画中？
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool IsInAnim(string name)
    {
        if (EnableAnim())
        {
            var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(name);
        }
        return true;
    }


    ////////////////////////ui生命周期---从上到下////////////////////////
    /// <summary>
    /// 初始化组件
    /// </summary>
    protected virtual void InitUI() { }

    /// <summary>
    /// 初始化
    /// </summary>
    protected virtual void OnInit() { mainThreadPreQueue = new Queue<Action>(); }

    /// <summary>
    /// 每次打开ui时
    /// </summary>
    protected virtual void OnOpen() { }

    /// <summary>
    /// 关闭时
    /// </summary>
    protected virtual void OnClose() { }

    /// <summary>
    /// 销毁时
    /// </summary>
    protected virtual void OnDestroy() { }


    protected virtual void OnRegister(bool register)
    {
        if (isRegister == register)
        {
            return;
        }
        isRegister = register;
        if (register)
        {
            foreach (KeyValuePair<string, GameEvent<GEventArgs>.EventCallback> kv in eventDict)
                GameEventMgr.Register(kv.Key, kv.Value);
        }
        else
        {
            if (eventDict != null && eventDict.Count > 0)
            {
                foreach (KeyValuePair<string, GameEvent<GEventArgs>.EventCallback> kv in eventDict)
                    GameEventMgr.UnRegister(kv.Key, kv.Value);
                eventDict = new Dictionary<string, GameEvent<GEventArgs>.EventCallback>();
            }

            if (btns != null && btns.Count > 0)
            {
                foreach (var btn in btns)
                    btn.onClick.RemoveAllListeners();
                btns = new List<Button>();
            }
            if (toggles != null && toggles.Count > 0)
            {
                foreach (var toggle in toggles)
                    toggle.onValueChanged.RemoveAllListeners();
                toggles = new List<Toggle>();
            }
        }
    }

    // 主线程预处理方法队列，非主线程需要处理unity相关资源对象操作可加入此队列处理
    public Queue<Action> mainThreadPreQueue = new Queue<Action>();
    public virtual void UpdateWin()
    {
        while (mainThreadPreQueue != null && mainThreadPreQueue.Count > 0)
        {
            mainThreadPreQueue.Dequeue()?.Invoke();
        }
    }


    // 注册的事件字典可使用
    public Dictionary<string, GameEvent<GEventArgs>.EventCallback> eventDict = new Dictionary<string, GameEvent<GEventArgs>.EventCallback>();
    protected virtual void AddUIEvent(string key, GameEvent<GEventArgs>.EventCallback func)
    {
        if (eventDict == null)
            eventDict = new Dictionary<string, GameEvent<GEventArgs>.EventCallback>();
        if (!eventDict.ContainsKey(key))
            eventDict.Add(key, func);
    }

    protected virtual void AddUIEvent(GEKey key, GameEvent<GEventArgs>.EventCallback func)
    {
        AddUIEvent(key.ToString(), func);
    }
}

