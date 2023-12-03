using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ChatUIContentList
{
    private Action<ChatData> clickCallbak;
    private ChatInfoListHelper chatInfoListHelper;
    private GameObject chatsr;

    private ContactData currContactData;
    List<ChatData> cds = new List<ChatData>();

    public ChatUIContentList(GameObject chatsr, Action<ChatData> clickCallbak)
    {
        this.chatsr = chatsr;
        this.clickCallbak = clickCallbak;
        Clear();
    }

    void UpdateFunc(int index, RectTransform item)
    {
        ChatData chatData = cds[index];
        item.gameObject.SetActive(true);
        //Debug.LogError("~~ UpdateFunc index:" + index + " - :" + chatData.chatStr);
        // item.transform.Find("Text").GetComponent<Text>().text = string.Format("{0}_{1}", data.name, index);
    }

    private Vector2 ItemSizeFunc(int index)
    {
        ChatData chatData = cds[index];
        Debug.LogWarning("~~ ItemSizeFunc index:" + index + " - :" + chatData.chattext.Length);
        if (chatData.chattext.Length >= 300)
            return new Vector2(300, 150);
        else if (chatData.chattext.Length >= 200)
            return new Vector2(300, 125);
        else if (chatData.chattext.Length >= 100)
            return new Vector2(300, 100);
        else if (chatData.chattext.Length >= 50)
            return new Vector2(300, 75);
        else // "S"
            return new Vector2(300, 50);
    }

    private int ItemCountFunc()
    {
        return cds.Count;
    }

    public void Clear()
    {
        cds.Clear();
        if (chatInfoListHelper)
            chatInfoListHelper.Clear();
    }

    public void OnSelectedContact(ContactData contactData)
    {
        currContactData = contactData;
        Clear();
        Debug.LogWarning("~~ On Selected Contact display chat info:" + contactData.ToString());
    }

    public void AddNewInfo(string sendToRoleId, string content)
    {
        Debug.LogWarning("~~ AddNewInfo sendToRoleId:" + sendToRoleId + " - content:" + content + " - currContactData.roleid:" + currContactData.roleid);
        if (string.IsNullOrEmpty(sendToRoleId) && currContactData == null)
        {
            return;
        }

        if (sendToRoleId != currContactData.roleid || content == null)
        {
            return;
        }

        JsonData cjd = jsontool.newwithstring(content);
        if (cjd.ContainsKey("chat"))
        {
            ChatData cd = new ChatData(cjd["chat"]);
            if (cds != null && cd != null)
            {
                cds.Add(cd);
                chatInfoListHelper.AddNewChat(cd);
            }
        }
    }

    public void AddNewInfo(string sendToRoleId, JsonData jd)
    {
        Debug.LogWarning("~~ AddNewInfo sendToRoleId:" + sendToRoleId + " - jd:" + jsontool.tostring(jd) + " - currContactData.roleid:" + currContactData.roleid);
        if (string.IsNullOrEmpty(sendToRoleId) && currContactData == null)
        {
            return;
        }

        if (sendToRoleId != currContactData.roleid || jd == null)
        {
            return;
        }

        ChatData cd = new ChatData(jd);
        if (cds != null && cd != null)
        {
            cds.Add(cd);
            chatInfoListHelper.AddNewChat(cd);
        }
    }

    public void DoNewInfo(string roleId)
    {
        if (string.IsNullOrEmpty(roleId) && currContactData == null)
        {
            return;
        }
        string displayRoleId = roleId == currContactData.roleid ? roleId : currContactData.roleid;
        List<ChatData> chatDatas = ChatManager.Instance.GetContactChatList(displayRoleId);
        // ÅÅ¸öÐò
        chatDatas.Sort(new ChatDataSort());
        if (chatDatas != null)
        {
            cds = chatDatas;
            CreateChatList(cds);
        }
    }

    public void CreateChatList(List<ChatData> cds)
    {
        chatInfoListHelper = chatsr.GetOrAddCompoonet<ChatInfoListHelper>();
        chatInfoListHelper.Init(cds);
    }
}
