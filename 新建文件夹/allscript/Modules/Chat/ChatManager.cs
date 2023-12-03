using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ChatManager : SingletonNotMono<ChatManager>
{
    // 本地的聊天数据
    private JsonData localChatData;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        JsonData localData = LocalDataManager.Instance.GetLocalData();
        if (!localData.ContainsKey("chat"))
        {
            localData["chat"] = jsontool.getemptytable();
            LocalDataManager.Instance.CoverLocalData(localData);
        }
        localChatData = localData["chat"];

        // 是否进行了数据容错处理
        bool isFaultTolerance = false;
        List<string> ukeys = new List<string>(localChatData.Keys);
        foreach (var ukey in ukeys)
        {
            JsonData chatList = localChatData[ukey];
            foreach (JsonData oneChat in chatList)
            {
                // 如果旧的聊天数据没有已读状态标记，则都标记为未读
                if (!oneChat.ContainsKey("readstate"))
                {
                    oneChat["readstate"] = 0;
                    isFaultTolerance = true;
                }
            }
        }

        if (isFaultTolerance)
        {
            CoverNewLocalChatDatas();
        }
    }

    /// <summary>
    /// 获取聊天key： 自己id_对话的玩家id 组成和某个玩家聊天的唯一标识key
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public string GetChatUKey(string roleId)
    {
        return string.Format("{0}_{1}", GameCenter.mIns.userInfo.RoleId, roleId);
    }

    /// <summary>
    /// 获取与某个玩家的所有聊天
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public JsonData GetContactChatJson(string roleId)
    {
        string ukey = GetChatUKey(roleId);
        if (localChatData.ContainsKey(ukey))
        {
            return localChatData[ukey];
        }
        return null;
    }

    /// <summary>
    /// 获取与某个玩家的所有聊天
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public List<ChatData> GetContactChatList(string roleId)
    {
        string ukey = GetChatUKey(roleId);
        List<ChatData> list = new List<ChatData>();
        if (localChatData.ContainsKey(ukey))
        {
            JsonData jdlist = localChatData[ukey];
            foreach (JsonData jd in jdlist)
            {
                ChatData chatData = new ChatData(jd);
                list.Add(chatData);
            }
        }
        return list;
    }

    /// <summary>
    /// 获取有聊天数据的联系人列表
    /// </summary>
    /// <returns></returns>
    public List<ContactData> GetHasChatContactList()
    {
        List<ContactData> cds = new List<ContactData>();
        List<string> ukeys = new List<string>(localChatData.Keys);
        foreach (var ukey in ukeys)
        {
            string[] twoRoleId = ukey.Split('_');
            if (twoRoleId.Length == 2)
            {
                string chatWithRoleId = twoRoleId[1];
                ContactData contactData = FriendManager.Instance.GetContactData(chatWithRoleId);
                if (contactData != null)
                {
                    cds.Add(contactData);
                }
            }
        }
        return cds;
    }

    /// <summary>
    /// 添加一个聊天数据
    /// 如果当前界面停留在与该玩家对话界面，数据标记为已读，否则标记为未读
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="jsonData"></param>
    public void AddChat(string roleId, JsonData jsonData)
    {
        string ukey = GetChatUKey(roleId);
        if (!localChatData.ContainsKey(ukey))
            localChatData[ukey] = new JsonData();

        if (localChatData.ContainsKey(ukey))
        {
            Debug.LogWarning("Add Chat ukey:" + ukey + " - has reg:" + GameEventMgr.HasRegister(GEKey.Chat_GetCurrChatObj) + " - jsonData:" + jsontool.tostring(jsonData));
            if (GameEventMgr.HasRegister(GEKey.Chat_GetCurrChatObj))
            {
                Action<ContactData> callback = (contactData) =>
                {
                    int readState = 0;
                    Debug.LogWarning("Add Chat ukey contactData:" + contactData);
                    if (contactData != null && jsonData.ContainsKey("roleid") && contactData.roleid == jsonData["roleid"].ToString())
                    {
                        readState = 1;
                        Debug.LogWarning("Add Chat ukey contactData:" + contactData.ToString());
                    }
                    Debug.LogWarning("Add Chat ukey readState:" + readState);
                    jsonData["readstate"] = readState; // 未读
                    localChatData[ukey].Add(jsonData);
                };
                GameEventMgr.Distribute(GEKey.Chat_GetCurrChatObj, callback);
            }
            else
            {
                jsonData["readstate"] = 0; // 未读
                localChatData[ukey].Add(jsonData);
            }
        }
    }

    /// <summary>
    /// 添加一个聊天数据
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="chatData"></param>
    public void AddChat(string roleId, ChatData chatData)
    {
        string ukey = GetChatUKey(roleId);
        if (!localChatData.ContainsKey(ukey))
            localChatData[ukey] = new JsonData();

        if (localChatData.ContainsKey(ukey))
        {
            localChatData[ukey].Add(chatData.ToJson());
        }
    }

    /// <summary>
    /// 移除与某人的聊天数据
    /// </summary>
    /// <param name="roleId"></param>
    public void RemovContactChat(string roleId)
    {
        string ukey = GetChatUKey(roleId);
        if (localChatData.ContainsKey(ukey))
        {
            localChatData.Remove(ukey);
        }
    }

    /// <summary>
    /// 清空聊天数据
    /// </summary>
    public void ClearChats()
    {
        List<string> ukeys = new List<string>(localChatData.Keys);
        foreach (string ukey in ukeys)
        {
            localChatData.Remove(ukey);
        }
    }

    /// <summary>
    /// 覆盖本地聊天数据
    /// </summary>
    public void CoverNewLocalChatDatas()
    {
        JsonData localData = LocalDataManager.Instance.GetLocalData();
        localData["chat"] = localChatData;
        LocalDataManager.Instance.CoverLocalData(localData);
    }

    /// <summary>
    /// 当就收到好友最新发来的聊天数据
    /// 这里是拉取与对话玩家的聊天最新数据,标记为已读
    /// </summary>
    /// <param name="content"></param>
    public void OnRecvFriendChatTx(string content)
    {
        Debug.LogWarning("~~ OnRecvFriendChatTx:" + content);
        // {"chats":[{"ce":0,"chattext":"123","chattype":1,"face":"face","lasttime":1690613232000,"level":1,"nickname":"GTA-许冷玉","os":0,"roleid":"5610190","friendid":"5610406","sendtime":1690613259137,"viplevel":0}]}
        // 这里是获取好友最新聊天，"roleid" "friendid" 有可能相反，因为包含了主动发送的和被动接受的
        JsonData jd = jsontool.newwithstring(content);
        if (jd != null && jd.ContainsKey("chats"))
        {
            JsonData chatList = jd["chats"];
            if (chatList.Count > 0)
            {
                string withChatRoleId = "";
                //JsonData oneNewChatJD = null;
                foreach (JsonData onechat in chatList)
                {
                    Debug.LogWarning("~~ withChatRoleId:" + withChatRoleId);
                    if (string.IsNullOrEmpty(withChatRoleId))
                    {
                        string userRoleId = GameCenter.mIns.userInfo.RoleId;
                        string chatRoleId = onechat["roleid"].ToString();
                        string chatFrinedId = onechat["friendid"].ToString();
                        withChatRoleId = chatRoleId.Equals(userRoleId) ? onechat["roleid"].ToString() + "_" + onechat["friendid"].ToString() : onechat["friendid"].ToString() + "_" + onechat["roleid"].ToString();
                    }
                    onechat["readstate"] = 1;

                    if (!localChatData.ContainsKey(withChatRoleId))
                    {
                        localChatData[withChatRoleId] = new JsonData
                        {
                            onechat
                        };
                        continue;
                    }

                    bool hasSame = false;
                    foreach (JsonData oldjd in localChatData[withChatRoleId])
                    {
                        if (oldjd["sendtime"].ToString() == onechat["sendtime"].ToString())
                        {
                            hasSame = true;
                            break;
                        }
                    }
                    if (!hasSame)
                        localChatData[withChatRoleId].Add(onechat);
                }
                //localChatData[withChatRoleId] = chatList;
                CoverNewLocalChatDatas();
            }
        }
    }

    /// <summary>
    /// 当就收到玩家主动发送聊天服务端返回的数据
    /// 这个数据直接标记为已读
    /// </summary>
    /// <param name="sendToRoleId"></param>
    /// <param name="content"></param>
    public void OnRecvSendFriendChatTx(string sendToRoleId, string content)
    {
        Debug.LogWarning("~~ OnRecvSendFriendChatTx:" + content);
        // {"chat":{"ce":0,"chattext":"123","chattype":1,"face":"face","lasttime":1690613232000,"level":1,"nickname":"GTA-许冷玉","os":0,"roleid":"5610190","friendid":"5610406","sendtime":1690613259137,"viplevel":0}}
        // 这里是主动发送聊天，"roleid":"发送者自己的id", "friendid":"接收者的id"
        JsonData jd = jsontool.newwithstring(content);
        if (jd != null && jd.ContainsKey("chat"))
        {
            JsonData chatjd = jd["chat"];
            chatjd["readstate"] = 1;
            string withChatRoleId = chatjd["friendid"].ToString();
            AddChat(withChatRoleId, chatjd);
        }
    }

    /// <summary>
    /// 清除聊天数据
    /// </summary>
    public void ClearChatData()
    {
        JsonData localData = LocalDataManager.Instance.GetLocalData();
        if (localData.ContainsKey("chat"))
        {
            localData["chat"] = jsontool.getemptytable();
            LocalDataManager.Instance.CoverLocalData(localData);
        }
    }

    /// <summary>
    /// 获取最新对话
    /// </summary>
    /// <param name="roleId"></param>
    public ChatData GetLatestChat(string roleId)
    {
        string ukey = GameCenter.mIns.userInfo.RoleId + "_" + roleId;
        if (!string.IsNullOrEmpty(ukey) && localChatData.ContainsKey(ukey))
        {
            List<ChatData> chatDatas = new List<ChatData>();
            JsonData jdList = localChatData[ukey];
            if (jdList != null && jdList.GetJsonType() == JsonType.Array)
            {
                foreach (JsonData onechat in jdList)
                {
                    // 只取对方给我发送的
                    if (onechat.ContainsKey("friendid") && onechat["friendid"].ToString() == GameCenter.mIns.userInfo.RoleId)
                    {
                        ChatData chatData = new ChatData(onechat);
                        if (chatData != null)
                        {
                            chatDatas.Add(chatData);
                        }
                    }
                }
                if (chatDatas.Count > 0)
                {
                    chatDatas.Sort(new ChatDataSort());
                    return chatDatas[chatDatas.Count - 1];
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 与某个玩家是否有未读聊天
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns>未读聊天个数</returns>
    public int GetNotReadChatNumber(string roleId)
    {
        int count = 0;
        string ukey = GetChatUKey(roleId);
        if (localChatData.ContainsKey(ukey))
        {
            foreach (JsonData onechat in localChatData[ukey])
            {
                count = !onechat.ContainsKey("readstate") ? count : (onechat["readstate"].ToString() == "0" ? count + 1 : count);
            }
        }

        return count;
    }

    /// <summary>
    /// 获取所有玩家未读消息个数
    /// </summary>
    /// <returns>所有玩家未读聊天个数</returns>
    public int GetNotReadChatNumberAll()
    {
        int count = 0;
        List<string> ukeys = new List<string>(localChatData.Keys);
        foreach (string ukey in ukeys)
        {
            foreach (JsonData onechat in localChatData[ukey])
            {
                count = !onechat.ContainsKey("readstate") ? count : (onechat["readstate"].ToString() == "0" ? count + 1 : count);
            }
        }
        return count;
    }


    /// <summary>
    /// 设置某个玩家聊天数据为已读(全部)
    /// </summary>
    /// <returns></returns>
    public void SetChatReadStateToRead(string roleId)
    {
        string ukey = GetChatUKey(roleId);
        if (localChatData.ContainsKey(ukey))
        {
            foreach (JsonData onechat in localChatData[ukey])
            {
                onechat["readstate"] = 1;
            }
        }

        CoverNewLocalChatDatas();
        GameEventMgr.Distribute(GEKey.Chat_RefrehNotReadCount, new FriendEventArgs(roleId));
    }

    /// <summary>
    /// 获取最近聊天时间戳
    /// </summary>
    /// <returns></returns>
    public void GetLastestChatTime(string roleId)
    {
        string ukey = GetChatUKey(roleId);
        Debug.LogError("~~ ukey:" + ukey);
        if (localChatData.ContainsKey(ukey))
        {
            foreach (JsonData onechat in localChatData[ukey])
            {
                onechat["readstate"] = 1;
            }
        }

        CoverNewLocalChatDatas();
        GameEventMgr.Distribute(GEKey.Chat_RefrehNotReadCount, new FriendEventArgs(roleId));
    }
}
public class ChatData
{
    // 唯一key(客户端使用: roleid_friendid)
    public string ukey;

    // 私信类型 1=文本 2=语音
    public int chattype;
    // 聊天文本
    public string chattext;
    // 聊天时间
    public long sendtime;
    // 帐号id
    public string uuid;
    // 玩家id(发送者id)
    public string roleid;
    // 好友id(接收者id)
    public string friendid;
    // 渠道
    public string cid;
    // 系统
    public int os;
    // 多语言 cn=中文 en=英文
    public string lan;
    // 昵称
    public string nickname;
    // 等级
    public int level;
    // vip等级
    public int viplevel;
    // 当前头像
    public string face;
    // 战力值
    public long ce;
    // 最后登录时间戳
    public long lasttime;

    /// <summary>
    /// 客户端使用数据
    /// </summary>
    // 聊天文本类型
    public EnumChatTextType chatTextType;
    // 索引
    public int index;
    // 是否已读(0:未读, 1:已读)
    public int readstate;
    // 是否显示发送时间（前后两条聊天数据间隔大于2分钟，需要显示）
    public bool isDisplaySendTime = false;

    public ChatData() { }

    public ChatData(JsonData jsonData)
    {
        ChatData cd = JsonMapper.ToObject<ChatData>(JsonMapper.ToJson(jsonData));
        Debug.LogWarning("cd:" + cd.ToString());
        if (cd != null)
        {
            // test
            cd.chatTextType = EnumChatTextType.Common;
        }
        Paste(cd);
    }

    public bool Paste(ChatData cd)
    {
        if (cd == null)
        {
            zxlogger.logerror($"Error: Paste contact data error! the contact data is null!");
            return false;
        }
        chattype = cd.chattype;
        chattext = cd.chattext;
        sendtime = cd.sendtime;
        uuid = cd.uuid;
        roleid = cd.roleid;
        friendid = cd.friendid;
        cid = cd.cid;
        os = cd.os;
        lan = cd.lan;
        nickname = cd.nickname;
        level = cd.level;
        viplevel = cd.viplevel;
        face = cd.face;
        ce = cd.ce;
        lasttime = cd.lasttime;
        chatTextType = cd.chatTextType;
        index = cd.index;
        ukey = string.Format("{0}_{1}", cd.roleid, cd.friendid);
        isDisplaySendTime = false;
        return true;
    }
    public void SetReadState(int state)
    {
        readstate = state;
    }

    public JsonData ToJson()
    {
        return JsonMapper.ToObject(JsonMapper.ToJson(this));
    }

    public override string ToString()
    {
        return $"roleid:{roleid}, friendid:{friendid}, chatTextType:{chatTextType}, sendtime:{sendtime}, text:{chattext}";
    }
}

public enum EnumChatTextType
{
    Common = 1,
    SeqImage = 2,
    Time = 3,
}

public class ChatDataSort : IComparer<ChatData>
{
    public int Compare(ChatData cda, ChatData cdb)
    {
        if (cda.sendtime != cdb.sendtime)
        {
            return cda.sendtime.CompareTo(cdb.sendtime);
        }
        return 0;
    }
}

//public class ChatDataSort : IComparer<ChatData>
//{
//    public int Compare(ChatData cda, ChatData cdb)
//    {
//        if (cda.sendtime != cdb.sendtime)
//        {
//            return cda.sendtime.CompareTo(cdb.sendtime);
//        }
//        return 0;
//    }
//}