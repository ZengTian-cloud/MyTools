using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendInfoUI : MonoBehaviour
{
    public GameObject uiRoot;
    public Transform uiRootTran;
    public RectTransform uiRootRect;

    private TMP_Text txRoleName;
    private TMP_Text txTeam;
    private Image imgAvatar;
    private TMP_Text txLv;
    private GameObject nodeBtns;
    private GameObject btnItem;
    private TMP_Text txid;
    private Button btnCopy;
    private TMP_Text txLastOnlineTime;

    private List<BtnItem> btnitems = new List<BtnItem>();

    public ContactData contactData;

    //public void LoadUIPrefab(Transform parent)
    //{
    //    GameObject loadObj = GameObject.Instantiate(GameCenter.mIns.m_ResMgr.LoadSyncRes("socialui_p", "friendInfo")) as GameObject;
    //    if (loadObj != null)
    //    {
    //        uiRoot = loadObj;
    //        uiRootTran = uiRoot.transform;
    //        uiRootRect = uiRoot.GetComponent<RectTransform>();

    //        uiRootTran.parent = parent;
    //        uiRootTran.localScale = Vector3.one;
    //        uiRootRect.anchorMax = new Vector2(1, 1);
    //        uiRootRect.anchorMin = new Vector2(0, 0);
    //        uiRootRect.pivot = new Vector2(0.5f, 0.5f);
    //        uiRootRect.anchoredPosition = Vector2.zero;
    //        uiRootRect.offsetMin = new Vector2(0, 0);
    //        uiRootRect.offsetMax = new Vector2(0, 0);

    //        InitUI();
    //    }
    //}

    public void InitUI(ContactData contactData)
    {
        this.contactData = contactData;
        Debug.Log(" friend detail ui InitUI contactData:" + contactData.ToString());
        txRoleName = uiRoot.transform.Find("root/txRoleName").GetComponent<TMP_Text>();
        txTeam = uiRoot.transform.Find("root/txTeam").GetComponent<TMP_Text>();
        imgAvatar = uiRoot.transform.Find("root/imgAvatar").GetComponent<Image>();
        txLv = uiRoot.transform.Find("root/imgAvatar/txLv").GetComponent<TMP_Text>();
        nodeBtns = uiRoot.transform.Find("root/nodeBtns").gameObject;
        btnItem = uiRoot.transform.Find("root/nodeBtns/btnItem").gameObject;
        txid = uiRoot.transform.Find("root/txid").GetComponent<TMP_Text>();
        btnCopy = uiRoot.transform.Find("root/btnCopy").GetComponent<Button>();
        txLastOnlineTime = uiRoot.transform.Find("root/txLastOnlineTime").GetComponent<TMP_Text>();

        btnItem.gameObject.SetActive(false);
        for (int i = 0; i < 6; i++)
        {
            GameObject o = GameObject.Instantiate(btnItem);
            o.transform.parent = btnItem.transform.parent;
            o.transform.localScale = Vector3.one;

            BtnItem _bi = new BtnItem(o, GetBtnName(i), i, (_index)=> {
                OnClickBtn(_index);
            });
            o.SetActive(true);
            btnitems.Add(_bi);
        }

        SetData(contactData);
    }

    public void SetData(ContactData contactData)
    {
        this.contactData = contactData;
        txRoleName.SetTextExt(contactData.nickname);
        txTeam.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_6"));
        txLv.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_7") + contactData.level);
        txid.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_8") + contactData.roleid);
        txLastOnlineTime.text = contactData.line == 1 ? GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_9") : datetool.FriendOnlineTimeFormat(contactData.lasttime, "HMi") + GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_17");

        for (int i = 0; i < 6; i++)
        {
            btnitems[i].SetSelImg(false);
            btnitems[i].btn.SetText(GetBtnName(i));
        }
    }

    public void SetActive(bool b)
    {
        if (uiRoot != null)
        {
            uiRoot.gameObject.SetActive(b);
        }
    }

    public void DestroyUIPrefab()
    {
        if (uiRoot != null)
        {
            GameObject.Destroy(uiRoot);
        }
    }

    private void OnClickBtn(int index)
    {
        switch (index)
        {
            case 0:
                if (contactData.contactType != (int)EnumContactType.Friend)
                {
                    GameCenter.mIns.m_UIMgr.PopMsg(GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_10"));
                    return;
                }
                // 私聊
                GameCenter.mIns.m_UIMgr.Open<ChatUI>(this.contactData);
                SetActive(false);
                break;
            case 1:
                // 删除好友
                //ContactData cd = FriendManager.Instance.GetContactData(EnumContactType.Friend, contactData.roleid);
                //if (cd == null)
                if (contactData.contactType != (int)EnumContactType.Friend)
                {
                    GameCenter.mIns.m_UIMgr.PopMsg(GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_10"));
                    return;
                }
                FriendManager.Instance.SendDeleteFriendMsg(contactData.roleid);
                break;
            case 2:
                // 跟随
                if (contactData.contactType != (int)EnumContactType.Friend)
                {
                    GameCenter.mIns.m_UIMgr.PopMsg(GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_10"));
                    return;
                }
                break;
            case 3:
                // 邀请加入房间
                if (contactData.contactType != (int)EnumContactType.Friend)
                {
                    GameCenter.mIns.m_UIMgr.PopMsg(GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_10"));
                    return;
                }
                break;
            case 4:
                // 拉黑玩家
                bool isBlack = contactData.contactType == (int)EnumContactType.Blacklist;
                // 防止刷新不及时显示错误
                List<ContactData> blacklist = FriendManager.Instance.GetContactDatas(EnumContactType.Blacklist);
                foreach (var bd in blacklist)
                {
                    if (bd.roleid == contactData.roleid)
                    {
                        isBlack = true;
                        break;
                    }
                }

                if (!isBlack)
                    FriendManager.Instance.SendDisposeBlackFriendMsg(contactData);
                else
                    FriendManager.Instance.SendDeleteBlackMsg(contactData.roleid);
                break;
            case 5:
                // 举报玩家
                Action<List<int>, string> callback = ReportBoxCallback;
                GameCenter.mIns.m_UIMgr.PopCustomPrefab("reportpopwin", "common",
                            GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_11"), GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_12"), GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_13"), GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_14"), callback);
            break;
        }
        //for (int i = 0; i < btnitems.Count; i++)
        //{
        //    btnitems[i].SetSelImg(i == index);
        //}
    }
    public void ReportBoxCallback(List<int> indexs, string text)
    {
        if (indexs != null)
        {
            string stype = "";
            for (int i = 0; i < indexs.Count; i++)
            {
                stype = i != indexs.Count - 1 ? stype + indexs[i] + "," : stype + indexs[i];
            }
            Debug.Log(" reportpopwin stype:" + stype + " - text:" + text + " - rid:" + contactData.roleid);
            FriendManager.Instance.SendReportRleMsg(contactData.roleid, stype, text);
        }
    }
    private string GetBtnName(int index)
    {
        bool isBlack = contactData.contactType == (int)EnumContactType.Blacklist;
        // 防止刷新不及时显示错误
        List<ContactData> blacklist = FriendManager.Instance.GetContactDatas(EnumContactType.Blacklist);
        foreach (var bd in blacklist)
        {
            if (bd.roleid == contactData.roleid)
            {
                isBlack = true;
                break;
            }
        }
        string name = "";
        switch (index)
        {
            case 0:
                name = GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_1");
                break;
            case 1:
                name = GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_2");
                break;
            case 2:
                name = GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_3");
                break;
            case 3:
                name = GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_4");
                break;
            case 4:
                name = !isBlack ? GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_15") : GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_16");
                break;
            case 5:
                name = GameCenter.mIns.m_LanMgr.GetLan("FriendInfoUI_5");
                break;
        }
        return name;
    }

    private class BtnItem
    {
        public Button btn;
        public GameObject imgSel;
        public TMP_Text tx;
        public Action<int> callback;
        public int index;
        public BtnItem(GameObject o, string  btnName, int index, Action<int> callback)
        {
            this.callback = callback;
            this.index = index;
            btn = o.GetComponent<Button>();
            imgSel = o.transform.FindHideInChild("img").gameObject;
            tx = o.transform.FindHideInChild("tx").GetComponent<TMP_Text>();

            btn.AddListenerBeforeClear(()=> {
                this.callback?.Invoke(this.index);
            });

            btn.SetText(btnName);
        }

        public void SetSelImg(bool bactive)
        {
            imgSel.gameObject.SetActive(bactive);
        }
    }

}


