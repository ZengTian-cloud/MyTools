using System.Collections;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public partial class ChatUI
{
    #region 定义和生命周期
    public override UILayerType uiLayerType => UILayerType.Normal;
    public override string uiAtlasName => "socialui";

    private bool openFlag;
    protected override void OnInit()
    {
        //检测热更
        GameCenter.mIns.m_HttpMgr.SendData("POST", 30, "", "", (state, content, val) =>
        {

        });
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        // 注册update
        GameCenter.mIns.m_UIMgr.AddUpdateWin(this);
        ContactData selectedData = null;
        if (openArgs != null && openArgs.Length > 0)
        {
            selectedData = (ContactData)openArgs[0];
        }

        GameEventMgr.Register(NetCfg.FRIEND_CHAT_TX_SEND.ToString(), OnReceiveSendFriendChatTx);
        GameEventMgr.Register(NetCfg.FRIEND_CHAT_TX.ToString(), OnReceiveFriendChatTx);
        GameEventMgr.Register(NetCfg.PUSH_FRIEND_MSG_NOTIF.ToString(), OnReceiveFriendMsgNotif);
        GameEventMgr.Register(GEKey.Chat_ClickChatEmo, OnReceivOnClickChatEmo);
        GameEventMgr.Register(GEKey.Chat_GetCurrChatObj, OnGetCurrChatObj);
        InitData(selectedData);
        m_ChatUIEmoPanel = new ChatUIEmoPanel(emoPanel, OnClickEmo);
        openFlag = true;
    }

    protected override void OnClose()
    {
        base.OnClose();
        GameEventMgr.UnRegister(NetCfg.FRIEND_CHAT_TX_SEND.ToString(), OnReceiveSendFriendChatTx);
        GameEventMgr.UnRegister(NetCfg.FRIEND_CHAT_TX.ToString(), OnReceiveFriendChatTx);
        GameEventMgr.UnRegister(NetCfg.PUSH_FRIEND_MSG_NOTIF.ToString(), OnReceiveFriendMsgNotif);
        GameEventMgr.UnRegister(GEKey.Chat_ClickChatEmo, OnReceivOnClickChatEmo);
        GameEventMgr.UnRegister(GEKey.Chat_GetCurrChatObj, OnGetCurrChatObj);
        openFlag = false;
        m_ChatUIContactList.Clear();
        if (m_ChatUIContentList != null)
            m_ChatUIContentList.Clear();
        if (m_ChatUIEmoPanel != null)
            m_ChatUIEmoPanel.Clear();
        m_ChatUIContactList = null;
        m_ChatUIContentList = null;
        m_ChatUIEmoPanel = null;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

    }
    #endregion

    #region functions
    /// <summary>
    /// register
    /// </summary>
    /// <param name="register"></param>
    protected override void OnRegister(bool register)
    {
        if (register)
        {
            //AddUIEvent(GameEventKey.OnItemNumberChanged.ToString(), OnItemNumberChanged);
        }
        base.OnRegister(register);
    }

    /// <summary>
    /// 返回
    /// </summary>
    partial void OnClickClose()
    {
        this.Close();
    }

    /// <summary>
    /// 设置按钮
    /// </summary>
    partial void OnClickSet()
    {
        GameCenter.mIns.m_UIMgr.PopMsg("未开发");
    }

    partial void OnClickVoice()
    {
        GameCenter.mIns.m_UIMgr.PopMsg("未开发");
    }

    partial void OnClickSend()
    {
        string sendText = inputChat.text;
        if (string.IsNullOrEmpty(sendText))
        {
            GameCenter.mIns.m_UIMgr.PopMsg("请输入文本!");
            return;
        }

        if (m_ChatUIContactList == null)
        {
            return;
        }

        if (m_ChatUIContactList.currSelectCCI == null)
        {
            return;
        }

        string roleId = m_ChatUIContactList.currSelectCCI.contactData.roleid;

        FriendManager.Instance.SendSendFriendChatMsg(roleId, 1, sendText);
        inputChat.text = "";
    }

    partial void OnClickEmote()
    {
        Debug.LogWarning("~~ m_ChatUIEmoPanel:" + m_ChatUIEmoPanel);
        if (m_ChatUIEmoPanel != null)
        {
            SetEmoPanelActive(true);
        }
    }

    private void OnReceivOnClickChatEmo(GEventArgs gEventArgs)
    {
        if (gEventArgs != null && gEventArgs.args != null && gEventArgs.args.Length > 0)
        {
            string emoStr = (string)gEventArgs.args[0];
            inputChat.text += emoStr;
        }
    }

    private void OnGetCurrChatObj(GEventArgs gEventArgs)
    {
        if (gEventArgs != null && gEventArgs.args != null && gEventArgs.args.Length > 0)
        {
            Action<ContactData> callback = (Action<ContactData>)gEventArgs.args[0];
            ContactData contactData = null;
            if (m_ChatUIContactList != null && m_ChatUIContactList.currSelectCCI != null)
            {
                contactData = m_ChatUIContactList.currSelectCCI.contactData;
            }
            callback?.Invoke(contactData);
        }
    }

    /// <summary>
    /// 接受到聊天信息
    /// </summary>
    /// <param name="gEventArgs"></param>
    public void OnReceiveSendFriendChatTx(GEventArgs gEventArgs)
    {
        Debug.LogWarning("~~ OnReceiveSendFriendChatTx openFlag:" + openFlag);
        if (!openFlag)
        {
            return;
        }
        // sendToRoleId, content
        string sendToRoleId = gEventArgs.args[0].ToString();
        string content = gEventArgs.args[1].ToString();
        m_ChatUIContentList.AddNewInfo(sendToRoleId, content);
    }

    /// <summary>
    /// 接受到某个玩家最新的聊天信息
    /// </summary>
    /// <param name="gEventArgs"></param>
    public void OnReceiveFriendChatTx(GEventArgs gEventArgs)
    {
        if (!openFlag)
        {
            return;
        }
        m_ChatUIContentList.DoNewInfo(gEventArgs.args[0].ToString());
    }

    /// <summary>
    /// 接受到某个玩家发来的聊天消息
    /// </summary>
    /// <param name="gEventArgs"></param>
    public void OnReceiveFriendMsgNotif(GEventArgs gEventArgs)
    {
        if (!openFlag)
        {
            return;
        }
        FriendEventArgs fargs = (FriendEventArgs)gEventArgs;
        if (fargs != null && fargs.serverJsonData != null)
        {
            Debug.Log("~~ OnReceiveFriendMsgNotif serverJsonData " + jsontool.tostring(fargs.serverJsonData));
            // 被动收到聊天消息roleid==发送者id，friendid==自己的id
            m_ChatUIContentList.AddNewInfo(fargs.serverJsonData["roleid"].ToString(), fargs.serverJsonData);
        }
    }
    #endregion

    // chat contact list
    private ChatUIContactList m_ChatUIContactList;
    // chat content list
    private ChatUIContentList m_ChatUIContentList;
    // chat emo panel
    private ChatUIEmoPanel m_ChatUIEmoPanel;
    private void InitData(ContactData selectedData = null)
    {
        if (selectedData != null) Debug.Log("~~ open chat ui first selected contact is:" + selectedData.ToString());
        InitChatContactList(selectedData);
    }

    public void InitChatContactList(ContactData selectedData = null)
    {
        if (m_ChatUIContactList == null)
        {
            m_ChatUIContactList = new ChatUIContactList(chatContactListTableView, chatContactListTableItem,
                (selectCCI) =>
                {
                    if (selectCCI != null)
                    {
                        DisplayChatInfo(selectCCI.contactData);
                        chatAreaTitleName.SetTextExt(selectCCI.contactData.nickname);
                    }
                });
        }
        List<ContactData> cds = ChatManager.Instance.GetHasChatContactList();
        if (selectedData != null)
        {
            bool selcdInList = false;
            foreach (ContactData contactData in cds)
            {
                if (contactData.roleid == selectedData.roleid)
                {
                    selcdInList = true;
                    break;
                }
            }
            if (!selcdInList)
            {
                cds.Insert(0, selectedData);
            }
        }
        m_ChatUIContactList.CreateChatList(cds, selectedData);
    }

    public void InitChatArea()
    {

    }

    public void DisplayChatInfo(ContactData contactData)
    {
        if (m_ChatUIContentList == null)
        {
            m_ChatUIContentList = new ChatUIContentList(chatsr.gameObject,
                (selectCI) =>
                {
                    if (selectCI != null)
                    {

                    }
                });

        }
        else
        {
        }
        // 1690613259137

        long lastestChatTime = 0;
        // 获取最近一条聊天时间
        ChatData lastestChat = ChatManager.Instance.GetLatestChat(contactData.roleid);
        if (lastestChat != null)
        {
            zxlogger.logwarning("DisplayChatInfo lastestChat:" + lastestChat.ToString());
            lastestChatTime = lastestChat.sendtime;
        }
        zxlogger.logwarning("DisplayChatInfo lastestChatTime:" + lastestChatTime.ToString());
        //1690613258137
        FriendManager.Instance.SendGetFriendChatMsg(contactData.roleid, lastestChatTime);
        // SendGetFriendChatMsg(string roleId, long readtime)
        m_ChatUIContentList.OnSelectedContact(contactData);
        ChatManager.Instance.SetChatReadStateToRead(contactData.roleid);
    }

    public void Clear()
    {

    }

    public void SetEmoPanelActive(bool b)
    {
        Debug.LogWarning("~~ SetEmoPanelActive:" + b);
        if (b != emoPanel.activeSelf && m_ChatUIEmoPanel != null)
        {
            m_ChatUIEmoPanel.SetActive(b);
        }
    }

    private void OnClickEmo(EnumEmoType emoType, int index)
    {

    }
}

