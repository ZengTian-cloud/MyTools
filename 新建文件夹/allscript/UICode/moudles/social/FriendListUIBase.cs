using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitJson;

public abstract class FriendListUIBase : MonoBehaviour
{
    public abstract string LIST_TYPE { get; }

    public abstract GameObject root { get; set; }
    public abstract ScrollRect listsr { get; set; }
    public abstract TableView tableView { get; set; }
    public abstract TableItem tableItem { get; set; }

    public abstract List<ContactListItem> contactListItems { get; set; }

    public abstract ContactListItem currSelectItem { get; set; }

    public virtual void SetActive(bool b)
    {
        if (root != null)
        {
            root.SetActive(b);
        }
    }

    public virtual void OnInit(GameObject root)
    {
        this.root = root;

        listsr = root.GetComponent<ScrollRect>();
        tableView = root.transform.GetChild(0).GetComponent<TableView>();
        tableItem = root.GetComponentInChildren<TableItem>();

        if (tableView == null)
        {
            zxlogger.logerrorformat("Error: frient list tableview is null!");
            return;
        }
    }

    public abstract void InitListData(Action<bool> callback);

    protected virtual void InitListItems(List<ContactData> datas, Action<bool> callback)
    {
        if (datas == null)
        {
            return;
        }
        Debug.LogWarning("Contact ui list ui init data:  data count:" + datas.Count + " - LIST_TYPE:" + LIST_TYPE);
        if (contactListItems != null && contactListItems.Count > 0)
        {
            foreach (var item in contactListItems)
            {
                item.OnDestroy();
            }
        }
        contactListItems = new List<ContactListItem>();

        foreach (var md in datas)
        {
            ContactListItem cli = new ContactListItem(md);
            contactListItems.Add(cli);
        }

        if (contactListItems != null)
        {
            tableView.onItemRender = OnItemRender;
            tableView.onItemDispose = OnItemDispose;
            tableView.SetDatas(contactListItems.Count, false);
        }

        callback?.Invoke(true);
    }

    public virtual void UpdateListData(List<ContactData> datas, Action<bool> callback, bool bactive = true)
    {
        Debug.LogWarning("~~ UpdateListData.datas:" + datas.Count);
        contactListItems = new List<ContactListItem>();
        foreach (var md in datas)
        {
            ContactListItem cli = new ContactListItem(md);
            contactListItems.Add(cli);
        }
        tableView.SetDatas(datas.Count, false);
        SetActive(bactive);
    }

    public virtual void UpdateListData(EnumContactType contactType, Action<bool> callback, bool bactive = true)
    {
        List<ContactData> datas = FriendManager.Instance.GetContactDatas(contactType);
        Debug.LogWarning("~~ OnReceiveFriendBlackNotif UpdateListData datas " + datas.Count);
        if (datas == null)
            return;

        if (contactListItems != null && contactListItems.Count > 0)
        {
            foreach (var item in contactListItems)
            {
                item.OnDestroy();
            }
        }
        contactListItems = new List<ContactListItem>();

        foreach (var md in datas)
        {
            ContactListItem cli = new ContactListItem(md);
            contactListItems.Add(cli);
        }

        tableView.SetDatas(datas.Count, false);
        SetActive(bactive);
    }

    protected virtual ContactListItem GetMailItemByIndex(int index)
    {
        int counter = 0;
        foreach (var mi in contactListItems)
        {
            if (counter == index)
                return mi;
            counter++;
        }
        return null;
    }

    protected virtual void OnItemRender(GameObject obj, int index)
    {
        ContactListItem cli = GetMailItemByIndex(index - 1);
        if (cli != null)
        {
            obj.name = "frienditem_" + cli.contactData.roleid.ToString();
            cli.BindObj(obj.transform.parent, obj, index, (_cli) =>
            {
                if (_cli != null)
                {
                    OnSelectedItem(_cli);
                }
            }, false);


            if (currSelectItem != null)
            {
                cli.SetSelectedState(currSelectItem.contactData.roleid == cli.contactData.roleid);
            }
        }
    }

    protected virtual void OnItemDispose()
    {

    }

    protected virtual void OnSelectedItem(ContactListItem cli)
    {
        if (cli != null)
        {
            if (currSelectItem != null && currSelectItem.contactData.roleid == cli.contactData.roleid)
            {
                return;
            }

            foreach (var _cli in contactListItems)
            {
                bool b = _cli.contactData.roleid == _cli.contactData.roleid;
                if (_cli.obj != null && cli.obj != null && _cli.obj.name == cli.obj.name && _cli.contactData.roleid != cli.contactData.roleid)
                {
                    // 之前点击的对象和现在点击的对象共同使用一个obj，不处理
                }
                else
                {
                    _cli.SetSelectedState(cli.contactData.roleid == _cli.contactData.roleid);
                    if (cli.contactData.roleid == _cli.contactData.roleid)
                    {
                        Debug.Log("on select cd:" + _cli.contactData.ToString());
                        GameEventMgr.Distribute(GEKey.OnSelecetFriend, _cli.contactData);
                    }
                }
            }
            currSelectItem = cli;
        }
    }

    public virtual void Clear()
    {
        contactListItems.Clear();
        tableView.SetDatas(0, false);
    }

    public virtual void ExitSelect()
    {
        if (currSelectItem != null)
        {
            currSelectItem.SetSelectedState(false);
            currSelectItem = null;
        }
    }

    public virtual void OnReceiveFriendMsgNotif(string sendRoleId, string recvRoleId, JsonData jsonData)
    {
        if (LIST_TYPE == "FriendList")
        {
            foreach (var item in contactListItems)
            {
                if (item.contactData.roleid == sendRoleId)
                {
                    item.UpdateChat(jsonData["chattext"].ToString());
                }
            }
        }

        //FriendEventArgs fargs = (FriendEventArgs)gEventArgs;
        //if (fargs != null && fargs.serverJsonData != null)
        //{
        //    Debug.Log("~~ OnReceiveFriendMsgNotif serverJsonData " + jsontool.tostring(fargs.serverJsonData));
        //    // 被动收到聊天消息roleid==发送者id，friendid==自己的id
        //    // m_ChatUIContentList.AddNewInfo(fargs.serverJsonData["roleid"].ToString(), fargs.serverJsonData);
        //    // {"ce":0,"chattext":"7","chattype":1,"face":"face","friendid":"5610190","lasttime":1690966742000,"level":1,"nickname":"GTA-伏凡之","os":0,"roleid":"5610406","sendtime":1690969978725,"viplevel":0}
        //    string sendRoleId = fargs.serverJsonData["roleid"].ToString();
        //    string recvRoleId = fargs.serverJsonData["friendid"].ToString();
        //    Debug.Log("~~ OnReceiveFriendMsgNotif sendRoleId: " + sendRoleId + " - recvRoleId:" + recvRoleId);
        //    if (m_friendListUI != null)
        //    {
        //        m_friendListUI.OnReceiveFriendMsgNotif(sendRoleId, recvRoleId, fargs.serverJsonData);
        //    }
        //}
    }
}

public class ContactListItem
{
    public ContactData contactData;

    public Transform parent;
    public GameObject obj;
    public int index;
    public Action<ContactListItem> clickCallback;
    public GameObject imgBg;
    public GameObject imgSelect;
    public GameObject imgAvatar;
    public TMP_Text txRoleName;
    public TMP_Text txLastestChat;
    public TMP_Text txLastestChatTime;
    public TMP_Text txLv;
    public GameObject number;
    public TMP_Text txNumber;
    public Button btn;

    private int notReadChatCount = 0;

    public ContactListItem(ContactData contactData)
    {
        notReadChatCount = 0;
        this.contactData = contactData;

        if (contactData.contactType == (int)EnumContactType.Friend)
        {
            // 是否有未读信息
            int notReadChat = ChatManager.Instance.GetNotReadChatNumber(contactData.roleid);
            if (notReadChat > 0)
                notReadChatCount = notReadChat;
        }
        else
        {
            // todo
        }
    }

    /*
         文本颜色: 未选中: 名字和等级: 4A4A49  时间和对话:686969
         文本颜色: 选中: 名字和等级: 白色  时间和对话:BABBBE
     */
    public void BindObj(Transform parent, GameObject obj, int index, Action<ContactListItem> clickCallback, bool bOpenAnim = false)
    {
        this.parent = parent;
        this.obj = obj;
        this.index = index;
        this.clickCallback = clickCallback;
        imgBg = obj.transform.FindHideInChild("imgBg").gameObject;
        imgSelect = obj.transform.FindHideInChild("imgSelect").gameObject;
        imgAvatar = obj.transform.Find("imgAvatar").gameObject;
        txRoleName = obj.transform.Find("txRoleName").GetComponent<TMP_Text>();
        txLastestChat = obj.transform.Find("txLastestChat").GetComponent<TMP_Text>();
        txLastestChatTime = obj.transform.Find("txLastestChatTime").GetComponent<TMP_Text>();
        txLv = imgAvatar.transform.Find("txLv").GetComponent<TMP_Text>();
        number = obj.transform.FindHideInChild("number").gameObject;
        txNumber = number.transform.Find("txNumber").GetComponent<TMP_Text>();
        btn = obj.transform.FindHideInChild("btn").GetComponent<Button>();

        txRoleName.color = commontool.GetColor("FF4A4A49");
        txLv.color = commontool.GetColor("FF4A4A49");
        txLastestChat.color = commontool.GetColor("FF686969");
        txLastestChatTime.color = commontool.GetColor("FF686969");

        btn.AddListenerBeforeClear(() =>
        {
            if (contactData.contactType == (int)EnumContactType.Apply)
            {
                // 同意了
                FriendManager.Instance.SendDisposeFriendApplyMsg(contactData.roleid, 2);
            }
        });

        imgSelect.gameObject.SetActive(false);

        txRoleName.SetTextExt(contactData.nickname);
        txLastestChat.SetTextExt("");
        txLastestChatTime.SetTextExt(contactData.line == 1 ? "在线" : datetool.FriendOnlineTimeFormat(contactData.lasttime, "HMi") + "前");
        txLv.SetTextExt("Lv." + contactData.level.ToString());

        obj.GetComponent<Button>().AddListenerBeforeClear(() =>
        {
            clickCallback?.Invoke(this);
        });

        if (contactData.contactType == (int)EnumContactType.Apply)
        {
            number.gameObject.SetActive(false);
            txLastestChatTime.gameObject.SetActive(false);
            btn.gameObject.SetActive(true);
        }
        else if (contactData.contactType == (int)EnumContactType.Friend)
        {
            //string lastestChat = ChatManager.Instance.GetLatestChat(contactData.roleid);
            //if (!string.IsNullOrEmpty(lastestChat))
            //{
            //    txLastestChat.SetTextWithEllipsis(lastestChat, 10);
            //}
            if (contactData != null)
            {
                ChatData lastestChatData = ChatManager.Instance.GetLatestChat(contactData.roleid);
                if (lastestChatData != null)
                {
                    contactData.lastchattext = lastestChatData.chattext;
                }
            }

            if (string.IsNullOrEmpty(contactData.lastchattext))
                contactData.lastchattext = "";
            txLastestChat.SetTextWithEllipsis(contactData.lastchattext, 10);

            // 是否有未读信息
            if (notReadChatCount > 0)
            {
                txNumber.SetTextExt(notReadChatCount >= 99 ? "99+" : notReadChatCount.ToString());
            }
            number.gameObject.SetActive(notReadChatCount > 0);
        }
        else
        {
            // todo
        }
    }

    public void UpdateChat(string newChat)
    {
        if (contactData.contactType == (int)EnumContactType.Friend)
        {
            if (string.IsNullOrEmpty(newChat))
                contactData.lastchattext = "";
            txLastestChat.SetTextWithEllipsis(newChat, 10);
        }
    }

    public void SetSelectedState(bool b)
    {
        if (imgSelect != null)
            imgSelect.SetActive(b);
        if (imgBg != null)
            imgBg.SetActive(!b);

        if (b)
        {
            if (txRoleName == null || txLv == null || txLastestChat == null || txLastestChatTime == null)
                return;
            txRoleName.color = Color.white;
            txLv.color = Color.white;
            txLastestChat.color = commontool.GetColor("FFBABBBE");
            txLastestChatTime.color = commontool.GetColor("FFBABBBE");
        }
        else
        {
            if (txRoleName == null || txLv == null || txLastestChat == null || txLastestChatTime == null)
                return;
            txRoleName.color = commontool.GetColor("FF4A4A49");
            txLv.color = commontool.GetColor("FF4A4A49");
            txLastestChat.color = commontool.GetColor("FF686969");
            txLastestChatTime.color = commontool.GetColor("FF686969");
        }
    }

    public void OnDestroy()
    {
        notReadChatCount = 0;
        if (obj != null)
        {
            GameObject.Destroy(obj);
        }
    }

    public override string ToString()
    {
        return $"dat:" + contactData.ToString();
    }
}
