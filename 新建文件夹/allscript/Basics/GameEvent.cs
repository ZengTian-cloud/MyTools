using System.Collections.Generic;
using System.Runtime.InteropServices;

/// <summary>
/// 事件参数接口
/// </summary>
public interface IGEventArgs
{
    public void OnRecycle();
}

/// <summary>
/// 事件参数类 - 定义args:用于简化（双方应自己知道参数顺序）
/// </summary>
[ComVisible(true)]
public abstract class GEventArgs : IGEventArgs
{
    public abstract object[] args { get; set; }

    public static readonly GEventArgs Empty;

    public GEventArgs() { }

    public virtual void OnRecycle() { }
}

/// <summary>
/// 事件enum, tostring转字符串使用
/// </summary>
public enum GEKey
{
    /// item 
    OnItemNumberChanged,
    OnItemAdd,
    OnItemRemove,

    // Mail
    OnGetAllMails,      // 获取所有邮件
    OnReadMails,        // 读取了邮件
    OnReceivedMails,    // 领取了邮件
    OnDeleteMails,      // 删除了邮件
    OnServerMailPush,   // 服务端邮件推送

    // Friend
    OnSelecetFriend,    // 点击了好友对象

    // Chat
    Chat_ClickChatEmo,    // 点击了聊天表情
    Chat_GetCurrChatObj,    // 获取当前聊天对象
    Chat_RefrehNotReadCount,    // 未读聊天数据需要刷新了

    // 章节、关卡
    CM_OpenMissionInfo, // 打开关卡详情ui
    CM_RecvMissions, // 获取某章所有关卡信息
    CM_RecvOneMission, // 获取某章某个关卡信息

    //战斗
    Battle_OnBattleEnd,//战斗结束事件-param[] [0]=(long)关卡id [1]=(int)战斗是否胜利 1-胜利 0-失败

    //Npc交互
    NpcInteracton_OnNpcTriggerEnter,//进入NPC交互范围
    NpcInteracton_OnNpcTriggerExit,//退出NPC交互范围
    NpcInteracton_OnDialogueStart,//对话开始
    NpcInteracton_OnDialogueEnd,//对话结束
    NpcInteracton_OnInteractionEcd,//交互结束
    NpcInteracton_OnInteractionCompleted,//交互完成

    NpcInteracton_OnReachedRewardNode,//奖励节点

    //任务
    Task_Main_AutoReward,//自动领取某个主线任务时

    OnUnLoadScene,//卸载场景
    OnEnterMainScene,
    OnLeaveMainScene,
}

//// 暂不扩展，使用game enum key字符串直接tostring
//public static class GEKey
//{
//    public static string GetString<GameEventKey>(this GameEventKey gameEventKey)
//    {
//        return gameEventKey.ToString();
//    }
//}

///// <summary>
///// GameEventArgs - GEventArgs
///// </summary>
//public class GameEventArgs : GEventArgs { }

/// <summary>
/// GameEventMgr - 简化写法
/// </summary>
public class GameEventMgr
{
    private static GameEvent<GEventArgs> m_ge = new GameEvent<GEventArgs>();

    public static bool CheckGEventArgsLegality(GEventArgs gEventArgs)
    {
        return (gEventArgs != null && gEventArgs.args != null && gEventArgs.args.Length > 0);
    }

    /// <summary>
    /// 清理所有事件
    /// </summary>
    public static void Clear()
    {
        m_ge.Clear();
        m_ge = new GameEvent<GEventArgs>();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static bool Register(string gameEventKey, GameEvent<GEventArgs>.EventCallback callback)
    {
        return m_ge.Register(gameEventKey, callback);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static bool Register(GEKey gameEventKey, GameEvent<GEventArgs>.EventCallback callback)
    {
        return m_ge.Register(gameEventKey.ToString(), callback);
    }

    /// <summary>
    /// 注销某类事件注册
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <returns></returns>
    public static void UnRegisterAll(string gameEventKey)
    {
        m_ge.UnRegisterAll(gameEventKey);
    }

    /// <summary>
    /// 注销某类事件注册
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <returns></returns>
    public static void UnRegisterAll(GEKey gameEventKey)
    {
        m_ge.UnRegisterAll(gameEventKey.ToString());
    }

    /// <summary>
    /// 注销单个事件注册
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static bool UnRegister(string gameEventKey, GameEvent<GEventArgs>.EventCallback callback)
    {
        return m_ge.UnRegister(gameEventKey, callback);
    }

    /// <summary>
    /// 注销单个事件注册
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static bool UnRegister(GEKey gameEventKey, GameEvent<GEventArgs>.EventCallback callback)
    {
        return m_ge.UnRegister(gameEventKey.ToString(), callback);
    }

    /// <summary>
    /// 派发事件，可变参数
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static bool Distribute<T>(string gameEventKey, T args) where T : GEventArgs
    {
        return m_ge.Distribute(gameEventKey, args);
    }

    /// <summary>
    /// 派发事件，可变参数
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static bool Distribute<T>(GEKey gameEventKey, T args) where T : GEventArgs
    {
        return m_ge.Distribute(gameEventKey.ToString(), args);
    }

    /// <summary>
    /// 派发事件，使用可变参数
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static bool Distribute(string gameEventKey, params object[] args)
    {
        return m_ge.Distribute(gameEventKey, GetEventArgs<GEventArgs>(args));
    }

    /// <summary>
    /// 派发事件，使用可变参数
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static bool Distribute(GEKey gameEventKey, params object[] args)
    {
        return m_ge.Distribute(gameEventKey.ToString(), GetEventArgs<GEventArgs>(args));
    }


    /// <summary>
    /// 是否有该类型的注册事件
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <returns></returns>
    public static bool HasRegister(string gameEventKey)
    {
        return m_ge.HasRegister(gameEventKey);
    }

    /// <summary>
    /// 是否有该类型的注册事件
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <returns></returns>
    public static bool HasRegister(GEKey gameEventKey)
    {
        return m_ge.HasRegister(gameEventKey.ToString());
    }

    /// <summary>
    /// 按参数顺序
    /// </summary>
    /// <returns></returns>
    private static T GetEventArgs<T>(params object[] args) where T : GEventArgs
    {
        return (T)(new SequenceEventArgs(args) as GEventArgs);
    }

    /// <summary>
    /// 按参数顺序GEventArgs
    /// </summary>
    private class SequenceEventArgs : GEventArgs
    {
        public SequenceEventArgs(params object[] args)
        {
            if (args.Length > 0)
                this.args = args;
        }

        public override object[] args { get; set; }

        public override void OnRecycle()
        {
            base.OnRecycle();
        }
    }
}

/// <summary>
/// 事件定义
/// </summary>
public class GameEvent<T> where T : GEventArgs
{
    public delegate void EventCallback(T args);

    /// <summary>
    /// 事件字典
    /// </summary>
    private Dictionary<string, HashSet<EventCallback>> m_callbacks = new Dictionary<string, HashSet<EventCallback>>();

    /// <summary>
    /// 初始化
    /// </summary>
    public GameEvent()
    {
        m_callbacks = new Dictionary<string, HashSet<EventCallback>>();
    }

    /// <summary>
    /// 是否有该类型的注册事件
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public bool HasRegister(string gameEventKey)
    {
        return m_callbacks.ContainsKey(gameEventKey);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public bool Register(string gameEventKey, EventCallback callback)
    {
        HashSet<EventCallback> list;
        if (!m_callbacks.TryGetValue(gameEventKey, out list))
        {
            m_callbacks[gameEventKey] = list = new HashSet<EventCallback>();
        }
        bool rb = list.Add(callback);
        return rb;
    }

    /// <summary>
    /// 取消某个类型事件注册
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <returns></returns>
    public void UnRegisterAll(string gameEventKey)
    {
        HashSet<EventCallback> list;
        if (!m_callbacks.TryGetValue(gameEventKey, out list))
            m_callbacks[gameEventKey] = list = new HashSet<EventCallback>();
        list.Clear();
    }

    /// <summary>
    /// 清空事件
    /// </summary>
    public void Clear()
    {
        foreach (HashSet<EventCallback> hsec in m_callbacks.Values)
            hsec.Clear();
        m_callbacks = null;
    }

    /// <summary>
    /// 取消单个事件注册
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public bool UnRegister(string gameEventKey, EventCallback callback)
    {
        HashSet<EventCallback> list;
        if (!m_callbacks.TryGetValue(gameEventKey, out list))
            m_callbacks[gameEventKey] = list = new HashSet<EventCallback>();
        return list.Remove(callback);
    }

    /// <summary>
    /// 派发事件，可变参数
    /// </summary>
    /// <param name="gameEventKey"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public bool Distribute(string gameEventKey, T args)
    {
        HashSet<EventCallback> list;
        if (m_callbacks.TryGetValue(gameEventKey, out list))
        {
            foreach (var eventCallback in list)
                eventCallback(args);
            return true;
        }
        return false;
    }
}



