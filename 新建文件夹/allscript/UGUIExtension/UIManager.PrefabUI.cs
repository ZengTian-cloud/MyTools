using System;
using System.Collections.Generic;
using UnityEngine;

public partial class UIManager
{
    #region PopMsg Prefab & PopWindow Prefab
    /// <summary>
    /// 预制件类ui类型
    /// </summary>
    private enum PopPrefabUIType
    {
        PopMsg, // 信息提示
        PopWindowPrefab,    // 通用弹窗预制件
        PopFullScreenPrefab,    // 通用弹窗预制件(全屏)
        CustomPrefab    // 自定义预制件(最顶层)
    }

    // 每10秒从缓存销毁一个ui预制
    float popTimer = 0.0f;
    float popTimeLimit = 10.0f;
    private void DoPopUpdate()
    {
        if (HasPrefabUICache())
        {
            popTimer += Time.deltaTime;
            if (popTimeLimit <= popTimer)
            {
                foreach (var item in popPrefabDict)
                {
                    if (item.Value.Count > 0)
                    {
                        OnDestroyOnePopMsg(item.Key);
                        break;
                    }
                }
            }
        }
        else
        {
            popTimer = 0;
        }
    }

    /// <summary>
    /// 是否存在ui预制件
    /// </summary>
    /// <returns></returns>
    private bool HasPrefabUICache()
    {
        if (popPrefabDict == null)
        {
            return false;
        }
        foreach (var item in popPrefabDict)
        {
            if (item.Value.Count > 0)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 缓存ui预制件字典
    /// </summary>
    private Dictionary<string, Queue<PrefabUIBase>> popPrefabDict = new Dictionary<string, Queue<PrefabUIBase>>();

    private void PrefabDictSafeCheck(string stype)
    {
        if (popPrefabDict == null)
            popPrefabDict = new Dictionary<string, Queue<PrefabUIBase>>();
        if (!popPrefabDict.ContainsKey(stype))
            popPrefabDict.Add(stype, new Queue<PrefabUIBase>());
    }

    /// <summary>
    /// 弹出一个消息类（飘字）
    /// </summary>
    /// <param name="msg">飘字文本</param>
    public void PopMsg(string msg)
    {
        PrefabDictSafeCheck(PopPrefabUIType.PopMsg.ToString());
        if (popPrefabDict[PopPrefabUIType.PopMsg.ToString()].Count > 0)
        {
            DoOpenPrefabUI(PopPrefabUIType.PopMsg, popPrefabDict[PopPrefabUIType.PopMsg.ToString()].Dequeue(), msg);
            return;
        }

        CommonPopMsg commonPopMsg = new CommonPopMsg(uiTopsideRoot.gameObject, msg, (_commonPopMsg) => {
            DoEnqueue(_commonPopMsg);
        });
    }

    /// <summary>
    /// 弹出一个通用弹窗预制窗口
    /// </summary>
    /// <param name="popWinStyle">窗口风格</param>
    public CommonPopWinPrefab PopWindowPrefab(PopWinStyle popWinStyle)
    {
        PrefabDictSafeCheck(PopPrefabUIType.PopWindowPrefab.ToString());
        popWinStyle.SetCloseCallback((i, _commonPopWinPrefab) => {
            DoEnqueue(_commonPopWinPrefab);
        });
        if (popPrefabDict[PopPrefabUIType.PopWindowPrefab.ToString()].Count > 0)
        {
            CommonPopWinPrefab commonPopWinPrefabCache = popPrefabDict[PopPrefabUIType.PopWindowPrefab.ToString()].Dequeue() as CommonPopWinPrefab;
            commonPopWinPrefabCache.popWinStyle = popWinStyle;
            DoOpenPrefabUI(PopPrefabUIType.PopWindowPrefab, commonPopWinPrefabCache, popWinStyle);
            return commonPopWinPrefabCache;
        }

        CommonPopWinPrefab commonPopWinPrefab = new CommonPopWinPrefab(winRootDict[UILayerType.Pop].gameObject, popWinStyle);
        return commonPopWinPrefab;
    }

    /// <summary>
    /// 弹出一个通用弹窗预制窗口
    /// </summary>
    /// <param name="popWinStyle">窗口风格</param>
    public CommonPopFullScreenPrefab PopFullScreenPrefab(PopFullScreenStyle popFullScreenStyle)
    {
        PrefabDictSafeCheck(PopPrefabUIType.PopFullScreenPrefab.ToString());
        popFullScreenStyle.SetCloseCallback((i, _commonPopFullScreenprefab) => {
            DoEnqueue(_commonPopFullScreenprefab);
        });
        if (popPrefabDict[PopPrefabUIType.PopFullScreenPrefab.ToString()].Count > 0)
        {
            CommonPopFullScreenPrefab commonPopFullScreenPrefabCache = popPrefabDict[PopPrefabUIType.PopFullScreenPrefab.ToString()].Dequeue() as CommonPopFullScreenPrefab;
            commonPopFullScreenPrefabCache.popFullScreenStyle = popFullScreenStyle;
            DoOpenPrefabUI(PopPrefabUIType.PopFullScreenPrefab, commonPopFullScreenPrefabCache, popFullScreenStyle);
            return commonPopFullScreenPrefabCache;
        }

        CommonPopFullScreenPrefab commonPopFullScreenPrefab = new CommonPopFullScreenPrefab(winRootDict[UILayerType.Pop].gameObject, popFullScreenStyle);
        return commonPopFullScreenPrefab;
    }

    /// <summary>
    /// 当前打开的custom列表（其他类型不需要）
    /// </summary>
    private List<PrefabUIBase> openingCustomPrefabs = new List<PrefabUIBase>();
    /// <summary>
    /// 弹出一个自定义预制件于ui最上层
    /// </summary>
    /// <param name="prefabName">预制件资源名</param>
    /// <param name="abName">资源ab名（可为空，为空等于资源名）</param>
    /// <param name="param">可变参数</param>
    /// <returns></returns>
    public PrefabUIBase PopCustomPrefab(string prefabName, string abName = "", params object[] param)
    {
        PrefabDictSafeCheck(PopPrefabUIType.CustomPrefab.ToString());
        if (popPrefabDict[PopPrefabUIType.CustomPrefab.ToString()].Count > 0)
        {
            PrefabUIBase _prefabUIBase = popPrefabDict[PopPrefabUIType.CustomPrefab.ToString()].Dequeue();
            if (_prefabUIBase != null)
            {
                DoOpenPrefabUI(PopPrefabUIType.CustomPrefab, _prefabUIBase, prefabName, abName, param);
                openingCustomPrefabs.Add(_prefabUIBase);
                return _prefabUIBase;
            }
        }
        /// TODO:优化实现
        PrefabUIBase commomCustomPrefab = null;
        if (prefabName.Equals("netprompt"))
        {
            commomCustomPrefab = new NetPromptWin(uiTopsideRoot.gameObject, prefabName, abName, param);
        }
        else if (prefabName.Equals("commonitemdetail"))
        {
            commomCustomPrefab = new CommonItemDetail(uiTopsideRoot.gameObject, prefabName, abName, param);
        }
        else if (prefabName.Equals("addfriendpopwin"))
        {
            commomCustomPrefab = new AddFriendPopWin(uiTopsideRoot.gameObject, prefabName, abName, param);
        }
        else if (prefabName.Equals("reportpopwin"))
        {
            commomCustomPrefab = new ReportPopWin(uiTopsideRoot.gameObject, prefabName, abName, param);
        }
        else
        {
            commomCustomPrefab = new CommomCustomPrefab();
            (commomCustomPrefab as CommomCustomPrefab).Init(uiTopsideRoot.gameObject, prefabName, abName, param);
        }
        openingCustomPrefabs.Add(commomCustomPrefab);
        return commomCustomPrefab;
    }

    public PrefabUIBase PopCustomPrefab<T>(T t, string prefabName, string abName = "", params object[] param) where T : CommomCustomPrefab
    {
        PrefabDictSafeCheck(PopPrefabUIType.CustomPrefab.ToString());
        if (popPrefabDict[PopPrefabUIType.CustomPrefab.ToString()].Count > 0)
        {
            PrefabUIBase _prefabUIBase = popPrefabDict[PopPrefabUIType.CustomPrefab.ToString()].Dequeue();
            if (_prefabUIBase != null)
            {
                DoOpenPrefabUI(PopPrefabUIType.CustomPrefab, _prefabUIBase, prefabName, abName, param);
                openingCustomPrefabs.Add(_prefabUIBase);
                return _prefabUIBase;
            }
        }
        if (t != null)
        {
            Debug.LogError("`` prefabName:" + prefabName);
            t.Init(uiTopsideRoot.gameObject, prefabName, abName, param);
            openingCustomPrefabs.Add(t);
        }
        return t;
    }

    /// <summary>
    /// 关闭自定义预制件-一组
    /// </summary>
    /// <param name="prefabName"></param>
    public void CloseCustomPrefab(string prefabName, bool isEnqueueCache = false)
    {
        if (openingCustomPrefabs != null && openingCustomPrefabs.Count > 0)
        {
            for (int i = openingCustomPrefabs.Count - 1; i >= 0; i--)
            {
                CommomCustomPrefab ccp = openingCustomPrefabs[i] as CommomCustomPrefab;
                if (ccp != null && ccp.prefabName == prefabName)
                {
                    //DoEnqueue(ccp);
                    ccp.OnDestroy();
                    openingCustomPrefabs.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// 关闭自定义预制件-一个
    /// </summary>
    /// <param name="_prefabUIBase"></param>
    public void CloseCustomPrefab(PrefabUIBase _prefabUIBase)
    {
        if (_prefabUIBase == null)
            return;
        CommomCustomPrefab ccp = _prefabUIBase as CommomCustomPrefab;
        if (ccp != null && !string.IsNullOrEmpty(ccp.prefabName))
            CloseCustomPrefab(ccp.prefabName);
    }

    /// <summary>
    /// 执行打开操作
    /// </summary>
    /// <param name="popPrefabUIType"></param>
    /// <param name="prefabUIBase"></param>
    /// <param name="args"></param>
    private void DoOpenPrefabUI(PopPrefabUIType popPrefabUIType, PrefabUIBase prefabUIBase, params object[] args)
    {
        if (prefabUIBase == null)
            return;

        if (popPrefabUIType == PopPrefabUIType.PopMsg)
        {
            CommonPopMsg commonPopMsgCache = prefabUIBase as CommonPopMsg;
            if (commonPopMsgCache != null)
            {
                commonPopMsgCache.RepetCommonPopMsg(commonPopMsgCache._Root, winRootDict[UILayerType.Pop].gameObject, args[0].ToString(), (_commonPopMsg) =>
                {
                    DoEnqueue(_commonPopMsg);
                });
            }
        }
        else if (popPrefabUIType == PopPrefabUIType.PopWindowPrefab)
        {
            CommonPopWinPrefab commonPopWinPrefabCache = prefabUIBase as CommonPopWinPrefab;
            if (commonPopWinPrefabCache != null)
            {
                commonPopWinPrefabCache.RepetCommonPopWinPrefab(commonPopWinPrefabCache._Root, winRootDict[UILayerType.Pop].gameObject, args[0] as PopWinStyle);
            }
        }
        else if (popPrefabUIType == PopPrefabUIType.PopFullScreenPrefab)
        {
            CommonPopFullScreenPrefab commonPopFullScreenPrefabCache = prefabUIBase as CommonPopFullScreenPrefab;
            if (commonPopFullScreenPrefabCache != null)
            {
                commonPopFullScreenPrefabCache.RepetCommonPopFullScreenPrefab(commonPopFullScreenPrefabCache._Root, winRootDict[UILayerType.Pop].gameObject, args[0] as PopFullScreenStyle);
            }
        }
        else if (popPrefabUIType == PopPrefabUIType.CustomPrefab)
        {
            CommomCustomPrefab commomCustomPrefab = prefabUIBase as CommomCustomPrefab;
            if (commomCustomPrefab.prefabName.Equals("netprompt"))
            {
                object[] args2 = args[2] as object[];
                commomCustomPrefab = new NetPromptWin(uiTopsideRoot.gameObject, args[0].ToString(), args[1].ToString(), args2[0].ToString());
            }
            else
            {
                commomCustomPrefab = new CommomCustomPrefab();
                (commomCustomPrefab as CommomCustomPrefab).Init(uiTopsideRoot.gameObject, args[0].ToString(), args[1].ToString(), args[2]);
            }
            openingCustomPrefabs.Add(commomCustomPrefab);
        }
    }

    /// <summary>
    /// 关闭后压入缓存
    /// </summary>
    /// <param name="_prefabUIBase"></param>
    private void DoEnqueue(PrefabUIBase _prefabUIBase)
    {
        if (popPrefabDict[_prefabUIBase.PrefabType].Count >= 20)
        {
            OnDestroyCommonPopMsg(_prefabUIBase);
            return;
        }
        if (_prefabUIBase != null)
        {
            popPrefabDict[_prefabUIBase.PrefabType].Enqueue(_prefabUIBase);
            _prefabUIBase._Root.transform.SetParent(uiCacheRoot);
        }
    }

    /// <summary>
    /// 销毁一个
    /// </summary>
    /// <param name="popPrefabUIType"></param>
    private void OnDestroyOnePopMsg(string popPrefabUIType)
    {
        if (popPrefabDict.ContainsKey(popPrefabUIType.ToString()) && popPrefabDict[popPrefabUIType.ToString()].Count > 0)
        {
            OnDestroyCommonPopMsg(popPrefabDict[popPrefabUIType.ToString()].Dequeue());
        }
    }

    /// <summary>
    /// 销毁一个
    /// </summary>
    /// <param name="_prefabUIBase"></param>
    private void OnDestroyCommonPopMsg(PrefabUIBase _prefabUIBase)
    {
        if (_prefabUIBase != null && _prefabUIBase._Root != null)
        {
            GameObject.Destroy(_prefabUIBase._Root);
        }
    }
    #endregion
}

/// <summary>
/// 通用弹出框风格
/// </summary>
public class PopWinStyle
{
    public string title;
    public string content;
    public int btnCount;
    public Action<int> btnClickCallback;
    public Action<int, CommonPopWinPrefab> closeCallback;
    public List<string> btnTxs = new List<string>();
    public List<string> btnResNames = new List<string>();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="content">文本内容</param>
    /// <param name="btnCount">按钮个数</param>
    /// <param name="btnClickCallback">点击按钮回调（参数为点击的按钮索引）</param>
    /// <param name="btnTxs">对应的按钮显示文本</param>
    /// <param name="btnResNames">对应的按钮图片资源名称</param>
    public PopWinStyle(string title, string content, int btnCount, Action<int> btnClickCallback = null, List<string> btnTxs = null, List<string> btnResNames = null)
    {
        this.title = title;
        this.content = content;
        this.btnCount = btnCount;
        this.btnClickCallback = btnClickCallback;
        this.btnTxs = btnTxs;
        this.btnResNames = btnResNames;
    }

    /// <summary>
    /// 关闭回调（uimanager使用）
    /// </summary>
    /// <param name="closeCallback"></param>
    public void SetCloseCallback(Action<int, CommonPopWinPrefab> closeCallback)
    {
        this.closeCallback = closeCallback;
    }
}

/// <summary>
/// 通用弹出框风格(全屏)
/// </summary>
public class PopFullScreenStyle
{
    public string title;
    public string content;
    public int btnCount;
    public Action<int> btnClickCallback;
    public Action<int, CommonPopFullScreenPrefab> closeCallback;
    public List<string> btnTxs = new List<string>();
    public List<string> btnResNames = new List<string>();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="content">文本内容</param>
    /// <param name="btnClickCallback">点击按钮回调（参数为点击的按钮索引）</param>
    /// <param name="btnTxs">对应的按钮显示文本</param>
    /// <param name="btnResNames">对应的按钮图片资源名称</param>
    public PopFullScreenStyle(string title, string content, int btnCount, Action<int> btnClickCallback = null, List<string> btnTxs = null, List<string> btnResNames = null)
    {
        this.title = title;
        this.content = content;
        this.btnClickCallback = btnClickCallback;
        this.btnTxs = btnTxs;
        this.btnResNames = btnResNames;
    }

    /// <summary>
    /// 关闭回调（uimanager使用）
    /// </summary>
    /// <param name="closeCallback"></param>
    public void SetCloseCallback(Action<int, CommonPopFullScreenPrefab> closeCallback)
    {
        this.closeCallback = closeCallback;
    }
}
