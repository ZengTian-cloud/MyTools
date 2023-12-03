using System;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.Security.Cryptography;

public partial class FriendManager : SingletonNotMono<FriendManager>
{
    public void SendMsg(int msgId, JsonData jsonData, Action<int, int, string> callback)
    {
        GameCenter.mIns.m_NetMgr.SendData(msgId, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                callback?.Invoke(eqid, code, content);
            });
        });
    }

    /// <summary>
    /// 获取好友私聊
    /// </summary>
    /// <param name="roleId">好友id</param>
    /// <param name="readtime">最后读取的时间戳</param>
    public void SendGetFriendChatMsg(string roleId, long readtime)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["friendid"] = roleId;
        jsonData["readtime"] = readtime;

        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.FRIEND_CHAT_TX, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                Debug.Log("SendGetFriendChatMsg code:" + code + " - content:" + content);
                ChatManager.Instance.OnRecvFriendChatTx(content);
                GameEventMgr.Distribute(NetCfg.FRIEND_CHAT_TX.ToString(), roleId);
            });
        });
    }

    /// <summary>
    /// 标记已读私信
    /// </summary>
    /// <param name="roleId">好友id</param>
    /// <param name="readtime">已读私信的时间戳</param>
    public void SendMarkChatMsg(string roleId, long readtime)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["friendid"] = roleId;
        jsonData["readtime"] = readtime;
        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.FRIEND_MARKCHAT_TX_READED, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {

            });
        });
    }

    /// <summary>
    /// 发送好友私聊
    /// </summary>
    /// <param name="roleId">好友id</param>
    /// <param name="cType">类型1=文本 2=语音</param>
    /// <param name="text">私信文本</param>
    public void SendSendFriendChatMsg(string roleId, int cType, string text)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["friendid"] = roleId;
        jsonData["type"] = cType;
        jsonData["text"] = text;
        string sendToRoleId = roleId;
        Debug.Log("SendSendFriendChatMsg code:" + jsontool.tostring(jsonData));
        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.FRIEND_CHAT_TX_SEND, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                Debug.Log("SendSendFriendChatMsg code:" + code + " - content:" + content);
                ChatManager.Instance.OnRecvSendFriendChatTx(sendToRoleId, content);
                GameEventMgr.Distribute(NetCfg.FRIEND_CHAT_TX_SEND.ToString(), sendToRoleId, content);
            });
        });
    }

    // todo
    Dictionary<int, int> getFriendListMsgTypeCache = new Dictionary<int, int>();
    /// <summary>
    /// 获取所有好友列表，或黑名单
    /// </summary>
    /// <param name="ftype">1=所有好友列表, 2=拉黑列表</param>
    public void SendGetFriendListMsg(int ftype)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["type"] = ftype;
        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.FRIEND_LIST, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                Debug.Log("eqid:" + eqid + " - content:" + content);
                Debug.Log("getFriendListMsgTypeCache.ContainsKey(eqid):" + getFriendListMsgTypeCache.ContainsKey(eqid));
                if (getFriendListMsgTypeCache.ContainsKey(eqid))
                {
                    if (getFriendListMsgTypeCache[eqid] == 1)
                    {
                        // friend list
                        OnGetNewAllFriendList(content);
                    }
                    else
                    {
                        // black list
                        OnGetNewAllBlackList(content);
                    }
                    getFriendListMsgTypeCache.Remove(eqid);
                }
                // GameEventMgr.Distribute(NetCfg.FRIEND_LIST.ToString(), new FriendEventArgs(content));
            });
        },
        (msgSeqId) =>
        {
            Debug.Log("msgSeqId:" + msgSeqId + " - ftype:" + ftype);
            getFriendListMsgTypeCache.Add(msgSeqId, ftype);
        });
    }

    /// <summary>
    /// 获取推举好友
    /// </summary>
    public void SendGetElectFriendMsg()
    {
        // reflush	必选	int	是否刷新 1 = 刷新
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["reflush"] = 1;
        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.FRIEND_ELECT, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                GameEventMgr.Distribute(NetCfg.FRIEND_ELECT.ToString(), new FriendEventArgs(content));
            });
        });
    }

    /// <summary>
    /// 获取搜索好友
    /// </summary>
    /// <param name="roleId"></param>
    public void SendSearchFriendMsg(string roleId)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["friendid"] = roleId;
        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.FRIEND_SEARCH, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                GameEventMgr.Distribute(NetCfg.FRIEND_SEARCH.ToString(), new FriendEventArgs(content));
            });
        });
    }


    /// <summary>
    /// 添加好友
    /// </summary>
    /// <param name="friendid"></param>
    /// <param name="text"></param>
    public void SendAddFriendMsg(string roleId, string text)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["friendid"] = roleId;
        jsonData["text"] = text;
        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.FRIEND_APPLY_ADD, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                GameEventMgr.Distribute(NetCfg.FRIEND_APPLY_ADD.ToString(), new FriendEventArgs(content));
            });
        });
    }

    /// <summary>
    /// 获取申请好友列表
    /// </summary>
    /// <param name="friendid"></param>
    /// <param name="text"></param>
    public void SendApplyFriendListMsg()
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.FRIEND_GET_APPLYLIST, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                OnGetNewApplyFriendList(content);
                //GameEventMgr.Distribute(NetCfg.FRIEND_GET_APPLYLIST.ToString(), new FriendEventArgs(content));
            });
        });
    }

    /// <summary>
    /// 处理好友申请
    /// </summary>
    /// <param name="friendid"></param>
    /// <param name="state">1=拒绝 2=通过</param>
    public void SendDisposeFriendApplyMsg(string roleId, int state)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["friendid"] = roleId;
        jsonData["state"] = state;
        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.FRIEND_DISPOSE_APPLY, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                if (content != null)
                {
                    JsonData jd = jsontool.newwithstring(content);
                    string did = jd["friendid"].ToString();
                    string dstr = jd["state"].ToString();
                    if (jd.ContainsKey("friendid") && jd.ContainsKey("state"))
                    {
                        RemoveContactData(EnumContactType.Apply, did);
                        GameEventMgr.Distribute(NetCfg.FRIEND_DISPOSE_APPLY.ToString(), new FriendEventArgs(content));
                        if (dstr == "2")
                        {
                            // 同意了好友添加，拉去好友列表
                            SendGetFriendListMsg(1);
                            Debug.LogWarning(" get all friend list! because has new friend add!");
                        }
                    }
                }
            });
        });
    }

    /// <summary>
    /// 处理拉黑好友
    /// </summary>
    /// <param name="friendid"></param>
    public void SendDisposeBlackFriendMsg(ContactData cd)
    {
        if (cd == null)
        {
            return;
        }
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["friendid"] = cd.roleid;
        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.FRIEND_DISPOSE_BLACK, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                if (code == 0)
                {
                    JsonData jd = jsontool.newwithstring(content);
                    string bid = jd["friendid"].ToString();
                    if (jd.ContainsKey("friendid"))
                    {
                        RemoveContactData(EnumContactType.Friend, bid);
                        AddContactData(EnumContactType.Blacklist, cd);
                        GameEventMgr.Distribute(NetCfg.FRIEND_DISPOSE_BLACK.ToString(), new FriendEventArgs(content));
                    }
                }
                else
                {
                    Debug.LogWarning($"拉黑失败:{content}! error code:{code}!");
                    GameCenter.mIns.m_UIMgr.PopMsg($"拉黑失败:{content}!");
                }
            });
        });
    }


    /// <summary>
    /// 删除好友
    /// </summary>
    /// <param name="friendid"></param>
    public void SendDeleteFriendMsg(string roleId)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["friendid"] = roleId;
        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.FRIEND_DELETE, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                if (content != null)
                {
                    JsonData jd = jsontool.newwithstring(content);
                    string did = jd["friendid"].ToString();
                    if (jd.ContainsKey("friendid"))
                    {
                        RemoveContactData(EnumContactType.Friend, did);
                        GameEventMgr.Distribute(NetCfg.FRIEND_DELETE.ToString(), new FriendEventArgs(content));
                    }
                }
            });
        });
    }

    /// <summary>
    /// 删除黑名单
    /// </summary>
    /// <param name="friendid"></param>
    public void SendDeleteBlackMsg(string roleId)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["friendid"] = roleId;
        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.FRIEND_DELETE_BLACK, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                RemoveContactData(EnumContactType.Blacklist, roleId);
                GameEventMgr.Distribute(NetCfg.FRIEND_DELETE_BLACK.ToString(), new FriendEventArgs(content));
                //if (content != null)
                //{
                //    JsonData jd = jsontool.newwithstring(content);
                //    if (jd.ContainsKey("friendid"))
                //    {
                //        string did = jd["friendid"].ToString();
                //        RemoveContactData(EnumContactType.Blacklist, did);
                //        GameEventMgr.Distribute(NetCfg.FRIEND_DELETE_BLACK.ToString(), new FriendEventArgs(content));
                //    }
                //}
            });
        });
    }

    /// <summary>
    /// 一键处理好友申请
    /// </summary>
    /// <param name="state">1=全部拒绝 2=全部通过</param>
    public void SendOneTimeDisposeApplyListMsg(int state)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["state"] = state;
        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.FRIEND_ONETIME_DISPOSE_APPLY, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                JsonData jd = jsontool.newwithstring(content);
                if (jd.ContainsKey("state"))
                {
                    int returnstate = (int)jd["state"];
                    // "state":"1=全部拒绝 2=全部通过"
                    Debug.LogWarning("~~ SendOneTimeDisposeApplyListMsg state:" + state + " - returnstate:" + returnstate);
                    if (state == 1)
                    {
                        FriendManager.Instance.RemoveContactDatas(EnumContactType.Apply);
                        GameCenter.mIns.m_UIMgr.PopMsg("一键拒绝成功!");
                    }
                    else
                    {
                        FriendManager.Instance.RemoveContactDatas(EnumContactType.Apply, EnumContactType.Friend);
                        GameCenter.mIns.m_UIMgr.PopMsg("一键同意成功!");
                    }
                }
                GameEventMgr.Distribute(NetCfg.FRIEND_ONETIME_DISPOSE_APPLY.ToString(), new FriendEventArgs(content));
            });
        });
    }

    /// <summary>
    /// 举报玩家
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="rType">举报类型,逗号拼接(如:1,2,6)</param>
    /// <param name="text">举报文本</param>
    public void SendReportRleMsg(string roleId, string rType, string text)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["friendid"] = roleId;
        jsonData["type"] = rType;
        jsonData["text"] = text;
        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.FRIEND_REPORT_ROLE, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                GameEventMgr.Distribute(NetCfg.FRIEND_REPORT_ROLE.ToString(), new FriendEventArgs(content));
            });
        });
    }

    /// <summary>
    /// 处理邮件,好友推送消息
    /// </summary>
    /// <param name="pushCode">推送消息码</param>
    public void DisposeMailFriendPushNotif(int pushCode, IGameMessage message = null)
    {
        GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
        {
            JsonData contentJD = null;
            if (message != null)
            {
                Debug.Log("DisposeMailFriendPushNotif MessageId：" + message.GetHeader().MessageId + " - content:" + message.getContent());
                contentJD = jsontool.newwithstring(message.getContent());
            }
            // 处理好友推送消息
            switch (pushCode)
            {
                case 101:
                    //101-邮件通知
                    GameEventMgr.Distribute(GEKey.OnServerMailPush, new MailEventArgs(message.getContent()));
                    break;
                case 102:
                    // 好友申请结果
                    GameEventMgr.Distribute(NetCfg.PUSH_FRIEND_ADD_RESULT_NOTIF.ToString(), new FriendEventArgs(message.getContent()));
                    break;
                case 103:
                    // 好友消息通知
                    /*
                     content:{"chat":{"ce":0,"chattext":"1","chattype":1,"face":"face","lasttime":1690799400000,"level":1,"nickname":"GTA-许冷玉","os":0,"roleid":"5610190","sendtime":1690803325846,"viplevel":0}}
                     */
                    if (contentJD != null && contentJD.ContainsKey("chat"))
                    {
                        JsonData chatJD = contentJD["chat"];
                        if (chatJD.ContainsKey("roleid"))
                        {
                            string rid = chatJD["roleid"].ToString();
                            ChatManager.Instance.AddChat(rid, chatJD);
                            GameEventMgr.Distribute(NetCfg.PUSH_FRIEND_MSG_NOTIF.ToString(), new FriendEventArgs(chatJD));
                        }
                    }
                    break;
                case 104:
                    // 好友申请通知
                    /*
                     {
                        "apply":{
                            "line":"0=离线 1=在线",
                            "applytext":"申请内容",
                            "uuid":"帐号id",
                            "roleid":"玩家id",
                            "cid":"渠道",
                            "os":"系统",
                            "lan":"多语言 cn=中文 en=英文",
                            "nickname":"昵称",
                            "level":"等级",
                            "viplevel":"vip等级",
                            "face":"当前头像",
                            "ce":"战力值",
                            "lasttime":"最后登录时间戳"
                          }
                        }
                     */
                    if (contentJD != null && contentJD.ContainsKey("apply"))
                    {
                        JsonData applyContactJD = contentJD["apply"];
                        ContactData contactData = new ContactData(EnumContactType.Apply, applyContactJD);
                        if (contactData != null)
                        {
                            AddContactData(EnumContactType.Apply, contactData, true);
                            GameEventMgr.Distribute(NetCfg.FRIEND_APPLY_ADD.ToString(), new FriendEventArgs(new List<ContactData>() { contactData }));
                        }
                    }
                    break;
                case 105:
                    // 删除好友通知
                    //  content:{"friendid":"5610190"}
                    if (contentJD != null && contentJD.ContainsKey("friendid"))
                    {
                        ContactData contactData = GetContactData(contentJD["friendid"].ToString());
                        if (contactData != null)
                        {
                            RemoveContactData((EnumContactType)contactData.contactType, contentJD["friendid"].ToString());
                            GameEventMgr.Distribute(NetCfg.PUSH_FRIEND_DELETE_NOTIF.ToString(), new FriendEventArgs(contentJD["friendid"].ToString()));
                        }
                    }
                    break;
                case 106:
                    // 拉黑好友通知
                    //  content:{"friendid":"5610190"}
                    // 有人拉黑了自己, 检查是不是自己的好友，是的话从好友列表中删除
                    if (contentJD != null && contentJD.ContainsKey("friendid"))
                    {
                        ContactData contactData = GetContactData(contentJD["friendid"].ToString());
                        if (contactData != null && contactData.contactType != (int)EnumContactType.Blacklist)
                        {
                            RemoveContactData((EnumContactType)contactData.contactType, contentJD["friendid"].ToString());
                            GameEventMgr.Distribute(NetCfg.PUSH_FRIEND_BLACK_NOTIF.ToString(), new FriendEventArgs(contentJD["friendid"].ToString()));
                        }
                    }
                    break;
                default:
                    break;
            }
        });
    }
}



public class FriendEventArgs : GEventArgs
{
    public override object[] args { get; set; }
    public List<int> disposeFriends = new List<int>();
    public List<ContactData> cds = new List<ContactData>();
    public JsonData serverJsonData;
    public string serverContent;
    public FriendEventArgs(string serverContent)
    {
        Debug.Log("`` server push friends serverContent data:" + jsontool.tostring(serverContent));
        this.serverContent = serverContent;
    }

    public FriendEventArgs(JsonData jsonData)
    {
        Debug.Log("`` server push friends json data:" + jsontool.tostring(jsonData));
        serverJsonData = jsonData;
    }

    public FriendEventArgs(int roleId)
    {
        disposeFriends = new List<int>();
        disposeFriends.Add(roleId);
    }

    public FriendEventArgs(List<int> roleIds)
    {
        disposeFriends = new List<int>();
        if (roleIds != null)
        {
            disposeFriends = roleIds;
        }
    }

    public FriendEventArgs(List<ContactData> cds)
    {
        this.cds = cds;
    }
}

