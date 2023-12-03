using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUIContactList
{
    public ChatContactItem currSelectCCI = null;
    private List<ChatContactItem> chatContactListItems = new List<ChatContactItem>();
    private TableView chatContactListTableView;
    private TableItem chatContactListTableItem;
    private Action<ChatContactItem> clickCallbak;
    private ContactData defSelectedData;
    public ChatUIContactList(TableView tableView, TableItem tableItem, Action<ChatContactItem> clickCallbak)
    {
        defSelectedData = null;
        chatContactListTableView = tableView;
        chatContactListTableItem = tableItem;
        this.clickCallbak = clickCallbak;
        Clear();
    }

    public void Clear()
    {
        currSelectCCI = null;
        chatContactListTableView.ClearAll();
        if (chatContactListItems != null && chatContactListItems.Count > 0)
        {
            foreach (var item in chatContactListItems)
            {
                item.OnDestroy();
            }
        }
        chatContactListItems.Clear();
    }

    public void CreateChatList(List<ContactData> cds, ContactData selectedData = null)
    {
        Debug.LogWarning("~~ CreateChatLis:" + cds.Count);
        if (selectedData != null)
        {
            Debug.LogWarning("~~ CreateChatLis selectedData:" + selectedData.ToString());
            defSelectedData = selectedData;
        }
        foreach (var md in cds)
        {
            ChatContactItem cci = new ChatContactItem(md);
            chatContactListItems.Add(cci);
        }

        if (chatContactListItems != null)
        {
            chatContactListTableView.onItemRender = OnChatContactItemRender;
            chatContactListTableView.onItemDispose = OnChatContactItemDispose;
            chatContactListTableView.SetDatas(chatContactListItems.Count, false);
        }
    }

    private ChatContactItem GetChatContactItemByIndex(int index)
    {
        int counter = 0;
        foreach (var mi in chatContactListItems)
        {
            if (counter == index)
                return mi;
            counter++;
        }
        return null;
    }

    private void OnChatContactItemRender(GameObject obj, int index)
    {
        ChatContactItem cci = GetChatContactItemByIndex(index - 1);
        if (cci != null)
        {
            obj.name = "frienditem_" + cci.contactData.roleid.ToString();
            cci.BindObj(obj.transform.parent, obj, index, (_cci) =>
            {
                if (_cci != null)
                {
                    OnSelectedItem(_cci);
                }
            });
            if (currSelectCCI != null)
            {
                cci.SetSelectedState(currSelectCCI.contactData.roleid == cci.contactData.roleid);
            }

            // 默认选择
            if (defSelectedData != null)
            {
                Debug.LogWarning("~~ OnChatContactItemRender item.contactData.roleid:" + cci.contactData.roleid + " - " + defSelectedData.ToString());
                if (cci.contactData.roleid == defSelectedData.roleid)
                {
                    OnSelectedItem(cci);
                    defSelectedData = null;
                }
            }
        }
    }

    private void OnChatContactItemDispose()
    {

    }

    private void OnSelectedItem(ChatContactItem cci)
    {
        if (cci != null)
        {
            if (currSelectCCI != null && currSelectCCI.contactData.roleid == cci.contactData.roleid)
            {
                return;
            }

            foreach (var _cci in chatContactListItems)
            {
                bool b = cci.contactData.roleid == _cci.contactData.roleid;
                if (_cci.o != null && cci.o != null && _cci.o.name == cci.o.name && _cci.contactData.roleid != cci.contactData.roleid)
                {
                    // 之前点击的对象和现在点击的对象共同使用一个obj，不处理
                }
                else
                {
                    _cci.SetSelectedState(cci.contactData.roleid == _cci.contactData.roleid);
                    if (cci.contactData.roleid == _cci.contactData.roleid)
                    {
                        Debug.Log("on select cd:" + _cci.contactData.ToString());
                        clickCallbak?.Invoke(_cci);
                        // GameEventMgr.Distribute(GameEventKey.OnSelecetFriend.ToString(), _cci.contactData);
                    }
                }
            }
            currSelectCCI = cci;
        }
    }
}

public class ChatContactItem
{
    public Transform parent;
    public int index;
    public ContactData contactData;
    public GameObject o;
    public Button btn;
    public GameObject selectObj;
    public Image avatar;
    public TMP_Text name;
    public TMP_Text lv;
    public Action<ChatContactItem> clickCallback;
    public ChatContactItem(ContactData contactData)
    {
        this.contactData = contactData;
    }

    public void BindObj(Transform parent, GameObject o, int index, Action<ChatContactItem> clickCallback)
    {
        this.parent = parent;
        this.o = o;
        this.index = index;
        this.clickCallback = clickCallback;
        btn = o.GetComponent<Button>();
        selectObj = o.transform.FindHideInChild("imgSelect").gameObject;
        avatar = o.transform.Find("imgAvatar").GetComponent<Image>();
        name = o.transform.Find("txRoleName").GetComponent<TMP_Text>();
        lv = o.transform.Find("imgAvatar/txLv").GetComponent<TMP_Text>();
        SetSelectedState(false);

        btn.AddListenerBeforeClear(OnClick);
        SetData(contactData);
    }

    public void SetData(ContactData cd = null)
    {
        if (cd == null && contactData == null) return;
        if (cd == null) cd = contactData;

        name.SetTextExt(cd.nickname);
        lv.SetTextExt("Lv." + cd.level);
    }

    public void SetSelectedState(bool b)
    {
        if (selectObj != null)
        {
            selectObj.SetActive(b);
        }
    }

    public void OnClick()
    {
        clickCallback?.Invoke(this);
    }

    public void OnDestroy()
    {
        if (o != null)
        {
            GameObject.Destroy(o);
        }
    }
}