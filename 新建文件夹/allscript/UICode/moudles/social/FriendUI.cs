using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitJson;
using Cysharp.Threading.Tasks;
// 5610190 --> 7777777
//5610406 --> 88888888
public class FriendUI : SocialSubModuleUI
{
    private int maxFriendLimitNumber = 100;
    private int maxBlackLimitNumber = 30;
    public override EnumSocialTabType SocialTabType { get => EnumSocialTabType.Friend; }
    private GameObject uiRoot;
    private Transform uiRootTran;
    private RectTransform uiRootRect;
    public override GameObject UIRoot { get => uiRoot; }

    private GameObject topTitle;
    private Transform parent;
    private FriendListUI m_friendListUI;
    private FriendApplyUI m_friendApplyUI;
    private FriendLastestListUI m_lastestListUI;
    private FriendBlackListUI m_blackListUI;

    private Transform leftBtns;
    private Button btnLatest;
    private Button btnContact;
    private Button btnApply;
    private List<Button> leftBtnList = new List<Button>();

    private Transform listLastest;
    private Transform listApply;
    private Transform nodeContact;
    private List<GameObject> listNodeObjs = new List<GameObject>();

    private Button btnFoldFriend;
    private Transform listFriend;
    private Button btnFoldBlacklist;
    private Transform listBlacklist;
    private Dictionary<int, bool> foldState = new Dictionary<int, bool>();

    private GameObject applyBtnNode;
    private Button btnOneTimeRefuse;
    private Button btnOneTimeAgree;

    private bool openFlag;
    // 缓存本例打开界面时的好友和黑名单列表数据
    //private List<ContactData> cacheFriendList = new List<ContactData>();  
    //private List<ContactData> cacheBlackList = new List<ContactData>();

    public override async void LoadUIPrefab(Transform parent)
    {
        this.parent = parent;
        GameObject loadObj = await ResourcesManager.Instance.LoadUIPrefabSync("friendlist");
        if (loadObj != null)
        {
            uiRoot = loadObj;
            uiRootTran = uiRoot.transform;
            uiRootRect = uiRoot.GetComponent<RectTransform>();

            uiRootTran.parent = parent;
            uiRootTran.localScale = Vector3.one;
            uiRootRect.anchorMax = new Vector2(1, 1);
            uiRootRect.anchorMin = new Vector2(0, 0);
            uiRootRect.pivot = new Vector2(0.5f, 0.5f);
            uiRootRect.anchoredPosition = Vector2.zero;
            uiRootRect.offsetMin = new Vector2(0, 0);
            uiRootRect.offsetMax = new Vector2(0, 0);
            InitUI();
        }
        GameEventMgr.Register(GEKey.OnSelecetFriend, OnSelectFriendInfo);
        GameEventMgr.Register(NetCfg.FRIEND_LIST.ToString(), OnReceiveFriendList);
        GameEventMgr.Register(NetCfg.FRIEND_BLACK_LIST.ToString(), OnReceiveBlackList);
        GameEventMgr.Register(NetCfg.FRIEND_ELECT.ToString(), OnReceiveEleFriendList);
        GameEventMgr.Register(NetCfg.FRIEND_SEARCH.ToString(), OnReceiveSearchFriend);
        GameEventMgr.Register(NetCfg.FRIEND_APPLY_ADD.ToString(), OnReceiveApplyFriendList);
        GameEventMgr.Register(NetCfg.FRIEND_DISPOSE_APPLY.ToString(), OnReceiveDisposeFriendApply);
        GameEventMgr.Register(NetCfg.FRIEND_DELETE.ToString(), OnReceiveDeleteFriend);
        GameEventMgr.Register(NetCfg.FRIEND_DISPOSE_BLACK.ToString(), OnReceiveDisposeBlackFriend);
        GameEventMgr.Register(NetCfg.FRIEND_DELETE_BLACK.ToString(), OnReceiveDeleteBlack);
        GameEventMgr.Register(NetCfg.FRIEND_ONETIME_DISPOSE_APPLY.ToString(), OnReceiveOneTimeDisposeApplyList);
        GameEventMgr.Register(GEKey.Chat_RefrehNotReadCount, RefreshNotChatCount);

        // push
        GameEventMgr.Register(NetCfg.PUSH_FRIEND_ADD_RESULT_NOTIF.ToString(), OnReceiveFriendAddResultNotif);
        GameEventMgr.Register(NetCfg.PUSH_FRIEND_MSG_NOTIF.ToString(), OnReceiveFriendMsgNotif);
        GameEventMgr.Register(NetCfg.PUSH_FRIEND_ADD_NOTIF.ToString(), OnReceiveFriendAddNotif);
        GameEventMgr.Register(NetCfg.PUSH_FRIEND_DELETE_NOTIF.ToString(), OnReceiveFriendDeleteNotif);
        GameEventMgr.Register(NetCfg.PUSH_FRIEND_BLACK_NOTIF.ToString(), OnReceiveFriendBlackNotif);

        FriendManager.Instance.SendGetFriendListMsg(1);
        FriendManager.Instance.SendGetFriendListMsg(2);
        FriendManager.Instance.SendApplyFriendListMsg();
    }

    public override void InitUI()
    {
        Transform _root = uiRootTran.FindHideInChild("root");
        topTitle = _root.FindHideInChild("topTitle").gameObject;

        // key: 好友-0, 黑名单-1,,, value false:收起, true:展开
        foldState.Add(0, false);
        foldState.Add(1, false);

        applyBtnNode = _root.FindHideInChild("applyBtnNode").gameObject;
        btnOneTimeRefuse = applyBtnNode.transform.FindHideInChild("btnOneTimeRefuse").GetComponent<Button>();
        btnOneTimeAgree = applyBtnNode.transform.FindHideInChild("btnOneTimeAgree").GetComponent<Button>();
        btnOneTimeRefuse.AddListenerBeforeClear(() =>
        {
            // 一键拒绝好友申请
            if (m_friendListUI == null || FriendManager.Instance.GetContactDatas(EnumContactType.Apply).Count <= 0)
            {
                GameCenter.mIns.m_UIMgr.PopMsg(GameCenter.mIns.m_LanMgr.GetLan("FriendUI_3"));
                return;
            }
            FriendManager.Instance.SendOneTimeDisposeApplyListMsg(1);
            leftBtnList[2].transform.FindHideInChild("number").GetComponentInChildren<TMP_Text>().text = "";
            leftBtnList[2].transform.FindHideInChild("number").gameObject.SetActive(false);
        });
        btnOneTimeAgree.AddListenerBeforeClear(() =>
        {
            // 一键同意好友申请
            if (m_friendListUI == null || FriendManager.Instance.GetContactDatas(EnumContactType.Apply).Count <= 0)
            {
                GameCenter.mIns.m_UIMgr.PopMsg(GameCenter.mIns.m_LanMgr.GetLan("FriendUI_3"));
                return;
            }
            FriendManager.Instance.SendOneTimeDisposeApplyListMsg(2);
            leftBtnList[2].transform.FindHideInChild("number").GetComponentInChildren<TMP_Text>().text = "";
            leftBtnList[2].transform.FindHideInChild("number").gameObject.SetActive(false);
        });

        leftBtns = uiRootTran.Find("root/leftBtns");
        btnLatest = uiRootTran.Find("root/leftBtns/btnLatest").GetComponent<Button>();
        btnContact = uiRootTran.Find("root/leftBtns/btnContact").GetComponent<Button>();
        btnApply = uiRootTran.Find("root/leftBtns/btnApply").GetComponent<Button>();

        listLastest = uiRootTran.Find("root/listLastest");
        listApply = uiRootTran.Find("root/listApply");
        nodeContact = uiRootTran.Find("root/nodeContact");

        btnFoldFriend = uiRootTran.Find("root/nodeContact/btnFoldFriend").GetComponent<Button>();
        listFriend = uiRootTran.Find("root/nodeContact/listFriend");
        btnFoldBlacklist = uiRootTran.Find("root/nodeContact/btnFoldBlacklist").GetComponent<Button>();
        listBlacklist = uiRootTran.Find("root/nodeContact/listBlacklist");

        m_friendListUI = listFriend.GetOrAddCompoonet<FriendListUI>();
        m_friendApplyUI = listApply.GetOrAddCompoonet<FriendApplyUI>();
        m_lastestListUI = listLastest.GetOrAddCompoonet<FriendLastestListUI>();
        m_blackListUI = listBlacklist.GetOrAddCompoonet<FriendBlackListUI>();

        btnLatest.AddListenerBeforeClear(OnClickLastest);
        btnContact.AddListenerBeforeClear(OnClickContact);
        btnApply.AddListenerBeforeClear(OnClickApply);

        btnFoldFriend.AddListenerBeforeClear(OnClickFoldFriend);
        btnFoldBlacklist.AddListenerBeforeClear(OnClickFoldBlacklist);
        InitEleList();

        leftBtnList = new List<Button>();
        leftBtnList.Add(btnLatest);
        leftBtnList.Add(btnContact);
        leftBtnList.Add(btnApply);

        listNodeObjs = new List<GameObject>();
        listNodeObjs.Add(listLastest.gameObject);
        listNodeObjs.Add(nodeContact.gameObject);
        listNodeObjs.Add(listApply.gameObject);
        foreach (var o in listNodeObjs)
        {
            o.gameObject.SetActive(false);
        }

        m_friendListUI.OnInit(listFriend.gameObject);
        m_friendApplyUI.OnInit(listApply.gameObject);
        m_lastestListUI.OnInit(listLastest.gameObject);
        m_blackListUI.OnInit(listBlacklist.gameObject);
        OnClickContact();

        btnFoldFriend.SetText(GameCenter.mIns.m_LanMgr.GetLan("FriendUI_4"));
        btnFoldBlacklist.SetText(GameCenter.mIns.m_LanMgr.GetLan("FriendUI_5"));


        OnClickFoldFriend();
        openFlag = true;

        RefreshNotChatCount(null);
    }

    public override void SetActive(bool b)
    {
        if (UIRoot != null)
        {
            UIRoot.SetActive(b);
        }
        if (friendInfo != null)
        {
            friendInfo.SetActive(b);
        }
        if (b)
        {
            RefreshList((EnumContactType)currIndex);
        }
    }

    public override void DestroyUIPrefab()
    {
        if (friendInfo != null)
        {
            friendInfo.DestroyUIPrefab();
        }
        friendInfo = null;
        if (UIRoot != null)
        {
            GameObject.Destroy(UIRoot);
        }

        GameEventMgr.UnRegister(GEKey.OnSelecetFriend.ToString(), OnSelectFriendInfo);
        GameEventMgr.UnRegister(NetCfg.FRIEND_LIST.ToString(), OnReceiveFriendList);
        GameEventMgr.UnRegister(NetCfg.FRIEND_BLACK_LIST.ToString(), OnReceiveBlackList);
        GameEventMgr.UnRegister(NetCfg.FRIEND_ELECT.ToString(), OnReceiveEleFriendList);
        GameEventMgr.UnRegister(NetCfg.FRIEND_SEARCH.ToString(), OnReceiveSearchFriend);
        GameEventMgr.UnRegister(NetCfg.FRIEND_APPLY_ADD.ToString(), OnReceiveApplyFriendList);
        GameEventMgr.UnRegister(NetCfg.FRIEND_DISPOSE_APPLY.ToString(), OnReceiveDisposeFriendApply);
        GameEventMgr.UnRegister(NetCfg.FRIEND_DELETE.ToString(), OnReceiveDeleteFriend);
        GameEventMgr.UnRegister(NetCfg.FRIEND_DISPOSE_BLACK.ToString(), OnReceiveDisposeBlackFriend);
        GameEventMgr.UnRegister(NetCfg.FRIEND_DELETE_BLACK.ToString(), OnReceiveDeleteBlack);
        GameEventMgr.UnRegister(NetCfg.FRIEND_ONETIME_DISPOSE_APPLY.ToString(), OnReceiveOneTimeDisposeApplyList);
        GameEventMgr.UnRegister(GEKey.Chat_RefrehNotReadCount.ToString(), RefreshNotChatCount);

        // push
        GameEventMgr.UnRegister(NetCfg.PUSH_FRIEND_ADD_RESULT_NOTIF.ToString(), OnReceiveFriendAddResultNotif);
        GameEventMgr.UnRegister(NetCfg.PUSH_FRIEND_MSG_NOTIF.ToString(), OnReceiveFriendMsgNotif);
        GameEventMgr.UnRegister(NetCfg.PUSH_FRIEND_ADD_NOTIF.ToString(), OnReceiveFriendAddNotif);
        GameEventMgr.UnRegister(NetCfg.PUSH_FRIEND_DELETE_NOTIF.ToString(), OnReceiveFriendDeleteNotif);
        GameEventMgr.UnRegister(NetCfg.FRIEND_BLACK_LIST.ToString(), OnReceiveFriendBlackNotif);

        openFlag = false;
        eleSendMark = false;
    }

    public override void Display()
    {
        if (friendInfo != null && friendInfo.contactData != null)
        {
            friendInfo.SetActive(true);
        }
        RefreshList((EnumContactType)currIndex);
    }

    private bool canRefreshEle = true;
    private float eleTimer = 0.0f;
    private float eleTimeLimit = 5.0f;
    public override void Update(float dt)
    {
        if (!canRefreshEle)
        {
            eleTimer += dt;
            if (eleTimer >= eleTimeLimit)
            {
                eleTimer = 0.0f;
                canRefreshEle = true;
            }
        }
    }

    private void OnClickLastest()
    {
        ClickLeftButtons(0);
    }


    private void OnClickContact()
    {
        ClickLeftButtons(1);
        // 比对缓存数据和最新的本地数据差异，使用新数据刷新列表（界面没切换tab不会向服务器获取列表）
        //List<ContactData> comparedDatas = FriendManager.Instance.CompareContactListDiffWithLocal(EnumContactType.Friend, cacheFriendList);
        //if (comparedDatas != null && m_friendListUI.B_INIT)
        //{
        //    cacheFriendList = comparedDatas;
        //    m_friendListUI.UpdateListData(cacheFriendList, (b) => { });
        //}

        //// todo: 黑名单也在此处检查刷新
        //// 比对缓存数据和最新的本地数据差异，使用新数据刷新列表（界面没切换tab不会向服务器获取列表）
        //List<ContactData> comparedDatas_black = FriendManager.Instance.CompareContactListDiffWithLocal(EnumContactType.Blacklist, cacheBlackList);
        //if (comparedDatas_black != null && m_blackListUI.B_INIT)
        //{
        //    cacheBlackList = comparedDatas_black;
        //    m_blackListUI.UpdateListData(cacheBlackList, (b) => { });
        //}
    }

    private void OnClickApply()
    {
        FriendManager.Instance.SendApplyFriendListMsg();
        ClickLeftButtons(2);
    }

    private void OnClickFoldFriend()
    {
        if (foldState.Count > 0)
            SetFoldBtn(0, !foldState[0]);
        if (m_blackListUI != null)
            m_blackListUI.ExitSelect();
    }

    private void OnClickFoldBlacklist()
    {
        if (foldState.Count > 0)
            SetFoldBtn(1, !foldState[1]);
        if (m_friendListUI != null)
            m_friendListUI.ExitSelect();
    }

    private void SetFoldBtn(int index, bool state)
    {
        if (foldState.Count > 0)
            foldState[index] = state;
        if (index == 0)
        {
            btnFoldFriend.transform.FindHideInChild("imgB").gameObject.SetActive(state);
            btnFoldFriend.transform.FindHideInChild("imgR").gameObject.SetActive(!state);
            if (state)
            {
                SetFoldBtn(1, false);
                if (!m_friendListUI.B_INIT)
                    m_friendListUI.InitListData((b) => { m_friendListUI.SetActive(true); });
                else
                    m_friendListUI.SetActive(true);
            }
            listFriend.gameObject.SetActive(state);
        }
        if (index == 1)
        {
            btnFoldBlacklist.transform.FindHideInChild("imgB").gameObject.SetActive(state);
            btnFoldBlacklist.transform.FindHideInChild("imgR").gameObject.SetActive(!state);
            if (state)
            {
                SetFoldBtn(0, false);
                if (!m_blackListUI.B_INIT)
                    m_blackListUI.InitListData((b) => { m_blackListUI.SetActive(true); });
                else
                    m_blackListUI.SetActive(true);
            }
            listBlacklist.gameObject.SetActive(state);
        }
    }

    int currIndex = -1;
    private void ClickLeftButtons(int index)
    {
        // 切换标签先关闭详情界面哦
        if (friendInfo != null)
        {
            friendInfo.SetActive(false);
        }

        for (int i = 0; i < leftBtnList.Count; i++)
        {
            leftBtnList[i].transform.FindHideInChild("imgSel").gameObject.SetActive(i == index);
            leftBtnList[i].transform.FindHideInChild("txBtnName").GetComponent<TMP_Text>().color = i == index ? Color.white : commontool.GetColor("FF5F6469");
            listNodeObjs[i].SetActive(i == index);
        }
        m_lastestListUI.ExitSelect();
        m_friendListUI.ExitSelect();
        m_friendApplyUI.ExitSelect();
        switch (index)
        {
            case 0:
                if (!m_lastestListUI.B_INIT)
                    m_lastestListUI.InitListData((b) => { m_lastestListUI.SetActive(true); });
                else
                {
                    m_lastestListUI.SetActive(true);
                    m_lastestListUI.UpdateListData(FriendManager.Instance.GetContactDatas(EnumContactType.Stranger), (b) => { });
                }
                break;
            case 1:
                if (!m_friendListUI.B_INIT)
                    m_friendListUI.InitListData((b) => { m_friendListUI.SetActive(true); });
                else
                {
                    m_friendListUI.SetActive(true);
                    m_friendListUI.UpdateListData(FriendManager.Instance.GetContactDatas(EnumContactType.Friend), (b) => { });
                }
                break;
            case 2:
                if (!m_friendApplyUI.B_INIT)
                    m_friendApplyUI.InitListData((b) => { m_friendApplyUI.SetActive(true); });
                else
                {
                    m_friendApplyUI.SetActive(true);
                    m_friendApplyUI.UpdateListData(FriendManager.Instance.GetContactDatas(EnumContactType.Apply), (b) => { });
                }
                break;
        }
        if (index != -1)
        {
            nodeEle.SetActive(false);
            btnEle.transform.FindHideInChild("imgnosel").gameObject.SetActive(true);
            btnEle.transform.FindHideInChild("imgsel").gameObject.SetActive(false);
        }
        topTitle.SetActive(index == 2);
        applyBtnNode.SetActive(index == 2);
        currIndex = index;
    }

    public void OnReceiveFriendList(GEventArgs gEventArgs)
    {
        if (!openFlag)
        {
            return;
        }

        FriendEventArgs friendEventArgs = (FriendEventArgs)gEventArgs;
        if (friendEventArgs != null)
        {
            List<ContactData> contactDatas = FriendManager.Instance.GetContactDatas(EnumContactType.Friend);//  friendEventArgs.cds;
            if (m_friendListUI != null)
            {
                if (m_friendListUI.B_INIT)
                    m_friendListUI.UpdateListData(contactDatas, (b) => { m_friendApplyUI.SetActive(true); });
                else
                    m_friendListUI.InitListData((b) => { });
            }
            //cacheFriendList = contactDatas;
            btnFoldFriend.transform.FindHideInChild("txNumber").GetComponent<TMP_Text>().SetTextExt($"{contactDatas.Count}/{maxFriendLimitNumber}");
        }
    }

    public void OnReceiveBlackList(GEventArgs gEventArgs)
    {
        if (!openFlag)
        {
            return;
        }

        FriendEventArgs friendEventArgs = (FriendEventArgs)gEventArgs;
        if (friendEventArgs != null)
        {
            List<ContactData> contactDatas = FriendManager.Instance.GetContactDatas(EnumContactType.Blacklist);//  friendEventArgs.cds;
            if (m_blackListUI != null)
            {
                if (m_blackListUI.B_INIT)
                    m_blackListUI.UpdateListData(contactDatas, (b) => { m_blackListUI.SetActive(true); });
                else
                    m_blackListUI.InitListData((b) => { });
            }
            //cacheBlackList = contactDatas;
            btnFoldBlacklist.transform.FindHideInChild("txNumber").GetComponent<TMP_Text>().SetTextExt($"{contactDatas.Count}/{maxBlackLimitNumber}");
        }
    }

    public void OnReceiveFriendAddResultNotif(GEventArgs gEventArgs)
    {
        FriendEventArgs fargs = (FriendEventArgs)gEventArgs;
        if (fargs != null && fargs.args != null)
        {
            Debug.Log("~~ OnReceiveFriendAddResultNotif fargs " + fargs.args[0].ToString());
        }
    }


    public void OnReceiveFriendMsgNotif(GEventArgs gEventArgs)
    {
        //FriendEventArgs fargs = (FriendEventArgs)gEventArgs;
        //if (fargs != null && fargs.serverJsonData != null)
        //{
        //    Debug.Log("~~ OnReceiveFriendMsgNotif serverJsonData " + jsontool.tostring(fargs.serverJsonData));

        //}
        // m_ChatUIContentList.AddNewInfo(fargs.serverJsonData["roleid"].ToString(), fargs.serverJsonData);

        FriendEventArgs fargs = (FriendEventArgs)gEventArgs;
        //Debug.LogError("~~ OnReceiveFriendMsgNotif serverJsonData " + jsontool.tostring(fargs.serverJsonData));
        if (fargs != null && fargs.serverJsonData != null)
        {
            //Debug.Log("~~ OnReceiveFriendMsgNotif serverJsonData " + jsontool.tostring(fargs.serverJsonData));
            // 被动收到聊天消息roleid==发送者id，friendid==自己的id
            // m_ChatUIContentList.AddNewInfo(fargs.serverJsonData["roleid"].ToString(), fargs.serverJsonData);
            // {"ce":0,"chattext":"7","chattype":1,"face":"face","friendid":"5610190","lasttime":1690966742000,"level":1,"nickname":"GTA-伏凡之","os":0,"roleid":"5610406","sendtime":1690969978725,"viplevel":0}
            string sendRoleId = fargs.serverJsonData["roleid"].ToString();
            string recvRoleId = fargs.serverJsonData["friendid"].ToString();
            //Debug.Log("~~ OnReceiveFriendMsgNotif sendRoleId: " + sendRoleId + " - recvRoleId:" + recvRoleId);
            if (m_friendListUI != null)
            {
                m_friendListUI.OnReceiveFriendMsgNotif(sendRoleId, recvRoleId, fargs.serverJsonData);
            }
        }
    }


    public void OnReceiveFriendAddNotif(GEventArgs gEventArgs)
    {
        FriendEventArgs fargs = (FriendEventArgs)gEventArgs;
        if (fargs != null && fargs.args != null)
        {
            Debug.Log("~~ OnReceiveFriendAddNotif fargs " + fargs.args[0].ToString());
        }
    }


    public void OnReceiveFriendDeleteNotif(GEventArgs gEventArgs)
    {
        FriendEventArgs fargs = (FriendEventArgs)gEventArgs;
        if (fargs != null && fargs.args != null)
        {
            if (currIndex == 1)
            {
                // cacheFriendList = FriendManager.Instance.GetContactDatas(EnumContactType.Friend);
                List<ContactData> contactDatas = FriendManager.Instance.GetContactDatas(EnumContactType.Friend);
                m_friendListUI.UpdateListData(contactDatas, (b) => { });
                btnFoldFriend.transform.FindHideInChild("txNumber").GetComponent<TMP_Text>().SetTextExt($"{contactDatas.Count}/{maxFriendLimitNumber}");
            }
        }
    }

    public void OnReceiveFriendBlackNotif(GEventArgs gEventArgs)
    {
        FriendEventArgs fargs = (FriendEventArgs)gEventArgs;
        Debug.Log("~~ OnReceiveFriendBlackNotif fargs " + fargs.args[0].ToString());
        if (fargs != null && fargs.args != null && fargs.args.Length > 0)
        {
            string rid = fargs.args[0].ToString();
            if (currIndex != -1)
            {
                if (currIndex == 0)
                    m_lastestListUI.UpdateListData(EnumContactType.Stranger, (b) => { });
                else if (currIndex == 1)
                    m_friendListUI.UpdateListData(EnumContactType.Friend, (b) => { });
                else if (currIndex == 2)
                    m_friendApplyUI.UpdateListData(EnumContactType.Apply, (b) => { });
            }
        }
    }

    public void OnReceiveApplyFriendList(GEventArgs gEventArgs)
    {
        if (!openFlag)
        {
            return;
        }

        FriendEventArgs friendEventArgs = (FriendEventArgs)gEventArgs;
        if (friendEventArgs != null)
        {
            List<ContactData> contactDatas = friendEventArgs.cds;
            if (m_friendApplyUI != null)
            {
                if (m_friendApplyUI.B_INIT)
                {
                    bool bActiveList = (!nodeEle.activeSelf && currIndex == 2);
                    m_friendApplyUI.UpdateListData(EnumContactType.Apply, (b) =>
                    {
                    }, bActiveList);
                }
                else
                    m_friendApplyUI.InitListData((b) => { });
            }

            int count = FriendManager.Instance.GetContactDatas(EnumContactType.Apply).Count;
            if (topTitle.gameObject.activeSelf)
            {
                topTitle.transform.FindHideInChild("txNoApply").gameObject.SetActive(count <= 0);
            }

            leftBtnList[2].transform.FindHideInChild("number").gameObject.SetActive(count > 0);
            TMP_Text numberText = leftBtnList[2].transform.FindHideInChild("number").GetComponentInChildren<TMP_Text>();
            leftBtnList[2].transform.FindHideInChild("number").GetComponentInChildren<TMP_Text>().text = count.ToString();
        }
    }

    /// <summary>
    /// 同意或拒接了好友申请
    /// </summary>
    /// <param name="gEventArgs"></param>
    public void OnReceiveDisposeFriendApply(GEventArgs gEventArgs)
    {
        if (!openFlag)
        {
            return;
        }

        FriendEventArgs friendEventArgs = (FriendEventArgs)gEventArgs;
        if (friendEventArgs != null && friendEventArgs.serverContent != null)
        {
            JsonData jd = jsontool.newwithstring(friendEventArgs.serverContent);
            if (jd.ContainsKey("friendid") && jd.ContainsKey("state"))
            {
                string did = jd["friendid"].ToString();
                string dstr = jd["state"].ToString();
                RefreshList(EnumContactType.Apply);
            }
        }

        int count = FriendManager.Instance.GetContactDatas(EnumContactType.Apply).Count;
        leftBtnList[2].transform.FindHideInChild("number").gameObject.SetActive(count > 0);
        leftBtnList[2].transform.FindHideInChild("number").GetComponentInChildren<TMP_Text>().text = count.ToString();
    }

    /// <summary>
    /// 删除了好友数据
    /// </summary>
    /// <param name="gEventArgs"></param>
    public void OnReceiveDeleteFriend(GEventArgs gEventArgs)
    {
        if (!openFlag)
        {
            return;
        }

        FriendEventArgs friendEventArgs = (FriendEventArgs)gEventArgs;
        if (friendEventArgs != null && friendEventArgs.serverContent != null)
        {
            JsonData jd = jsontool.newwithstring(friendEventArgs.serverContent);
            if (jd.ContainsKey("friendid"))
            {
                string did = jd["friendid"].ToString();
                RefreshList(EnumContactType.Friend);

                if (friendInfo != null && friendInfo.contactData != null && friendInfo.contactData.roleid == did)
                {
                    friendInfo.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 拉黑了好友
    /// </summary>
    /// <param name="gEventArgs"></param>
    public void OnReceiveDisposeBlackFriend(GEventArgs gEventArgs)
    {
        if (!openFlag)
        {
            return;
        }

        FriendEventArgs friendEventArgs = (FriendEventArgs)gEventArgs;
        if (friendEventArgs != null && friendEventArgs.serverContent != null)
        {
            FriendManager.Instance.SendGetFriendListMsg(1);
            FriendManager.Instance.SendGetFriendListMsg(2);
            JsonData jd = jsontool.newwithstring(friendEventArgs.serverContent);
            if (jd.ContainsKey("friendid"))
            {
                string did = jd["friendid"].ToString();
                RefreshList(EnumContactType.Friend);
                RefreshList(EnumContactType.Blacklist);

                if (friendInfo != null && friendInfo.contactData != null && friendInfo.contactData.roleid == did)
                {
                    friendInfo.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 解除拉黑
    /// </summary>
    /// <param name="gEventArgs"></param>
    public void OnReceiveDeleteBlack(GEventArgs gEventArgs)
    {
        if (!openFlag)
        {
            return;
        }

        FriendEventArgs friendEventArgs = (FriendEventArgs)gEventArgs;
        if (friendEventArgs != null && friendEventArgs.serverContent != null)
        {
            FriendManager.Instance.SendGetFriendListMsg(2);
            friendInfo.SetActive(false);
            //JsonData jd = jsontool.newwithstring(friendEventArgs.serverContent);
            //if (jd.ContainsKey("friendid"))
            //{
            //    string did = jd["friendid"].ToString();
            //    RefreshList(EnumContactType.Blacklist);
            //    if (friendInfo != null && friendInfo.contactData != null && friendInfo.contactData.roleid == did)
            //    {
            //        friendInfo.SetActive(false);
            //    }
            //}
        }
    }

    /// <summary>
    /// 一键好友处理
    /// </summary>
    /// <param name="gEventArgs"></param>
    public void OnReceiveOneTimeDisposeApplyList(GEventArgs gEventArgs)
    {
        if (!openFlag)
        {
            return;
        }

        FriendEventArgs friendEventArgs = (FriendEventArgs)gEventArgs;
        if (friendEventArgs != null && friendEventArgs.serverContent != null)
        {
            if (friendInfo != null)
                friendInfo.SetActive(false);
            JsonData jd = jsontool.newwithstring(friendEventArgs.serverContent);
            if (jd.ContainsKey("state"))
            {
                int state = (int)jd["state"];
                // "state":"1=全部拒绝 2=全部通过"
                //if (state == 1)
                //{
                //    GameCenter.mIns.m_UIMgr.PopMsg("已经拒绝全部申请!");
                //}
                //else
                //{
                //    GameCenter.mIns.m_UIMgr.PopMsg("已经同意全部申请!");
                //}
                // 清空申请列表
                m_friendApplyUI.Clear();
            }
        }
        List<ContactData> contactDatas = FriendManager.Instance.GetContactDatas(EnumContactType.Friend);
        btnFoldFriend.transform.FindHideInChild("txNumber").GetComponent<TMP_Text>().SetTextExt($"{contactDatas.Count}/{maxFriendLimitNumber}");
    }

    public void RefreshNotChatCount(GEventArgs gEventArgs)
    {
        if (!openFlag)
        {
            return;
        }

        int count = ChatManager.Instance.GetNotReadChatNumberAll();
        leftBtnList[1].transform.FindHideInChild("number").gameObject.SetActive(count > 0);
        leftBtnList[1].transform.FindHideInChild("number").GetComponentInChildren<TMP_Text>().text = count >= 99 ? "99+" : count.ToString();
    }

    public void RefreshList(EnumContactType contactType)
    {
        Debug.LogWarning("~~ RefreshList contactType:" + contactType);
        switch (contactType)
        {
            case EnumContactType.Friend:
                if (m_friendListUI != null && m_friendListUI.B_INIT)
                {
                    List<ContactData> datas = FriendManager.Instance.GetContactDatas(EnumContactType.Friend);
                    m_friendListUI.UpdateListData(datas, (b) => { });
                    btnFoldFriend.transform.FindHideInChild("txNumber").GetComponent<TMP_Text>().SetTextExt($"{datas.Count}/{maxFriendLimitNumber}");
                }
                break;
            case EnumContactType.Apply:
                if (m_friendApplyUI != null && m_friendApplyUI.B_INIT)
                    m_friendApplyUI.UpdateListData(FriendManager.Instance.GetContactDatas(EnumContactType.Apply), (b) => { });
                break;
            case EnumContactType.Blacklist:
                if (m_blackListUI != null && m_blackListUI.B_INIT)
                {
                    List<ContactData> datas = FriendManager.Instance.GetContactDatas(EnumContactType.Blacklist);
                    m_blackListUI.UpdateListData(datas, (b) => { });
                    btnFoldBlacklist.transform.FindHideInChild("txNumber").GetComponent<TMP_Text>().SetTextExt($"{datas.Count}/{maxFriendLimitNumber}");
                }
                break;
            case EnumContactType.Stranger:
                if (m_lastestListUI != null && m_lastestListUI.B_INIT)
                    m_lastestListUI.UpdateListData(FriendManager.Instance.GetContactDatas(EnumContactType.Stranger), (b) => { });
                break;
        }
    }

    #region ele
    private Button btnEle;
    private GameObject nodeEle;
    private Button btnRefresh;
    private GameObject elelist;
    private GameObject eleItem;
    private TMP_InputField inputSearch;
    private Button btnSearch;
    private List<EleItem> eleItems = new List<EleItem>();
    private GameObject searchItem;
    private TMP_Text eleTitle;

    private bool eleSendMark = false;
    public void InitEleList()
    {
        Transform _r = uiRootTran.Find("root");
        btnEle = uiRootTran.Find("root/btnEle").GetComponent<Button>();
        nodeEle = _r.FindHideInChild("nodeEle").gameObject;
        btnRefresh = nodeEle.transform.Find("btnRefresh").GetComponent<Button>();
        btnSearch = nodeEle.transform.Find("btnSearch").GetComponent<Button>();

        elelist = nodeEle.transform.Find("list").gameObject;
        eleItem = elelist.transform.FindHideInChild("eleItem").gameObject;
        inputSearch = nodeEle.transform.Find("inputSearch").GetComponent<TMP_InputField>();
        searchItem = nodeEle.transform.FindHideInChild("searchItem").gameObject;
        eleTitle = nodeEle.transform.Find("txTitle").GetComponent<TMP_Text>();

        eleTitle.text = GameCenter.mIns.m_LanMgr.GetLan("FriendUI_1");
        btnEle.AddListenerBeforeClear(OnClickEle);
        btnRefresh.AddListenerBeforeClear(OnClickRefresh);
        btnSearch.AddListenerBeforeClear(OnClickSearch);
    }

    public void DisplayEleList()
    {
        ClickLeftButtons(-1);
        nodeEle.SetActive(true);
        btnEle.transform.FindHideInChild("imgnosel").gameObject.SetActive(false);
        btnEle.transform.FindHideInChild("imgsel").gameObject.SetActive(true);
        if (!eleSendMark)
        {
            FriendManager.Instance.SendGetElectFriendMsg();
            eleSendMark = true;
        }
    }


    private void OnClickEle()
    {
        if (nodeEle.activeSelf)
        {
            return;
        }

        // 切换标签先关闭详情界面哦
        if (friendInfo != null)
        {
            friendInfo.SetActive(false);
        }

        // 获取好友推举列表
        DisplayEleList();
    }

    private void OnClickRefresh()
    {
        if (SearchItem != null)
        {
            searchItem.gameObject.SetActive(false);
            elelist.gameObject.SetActive(true);
            SearchItem = null;
            eleTitle.text = GameCenter.mIns.m_LanMgr.GetLan("FriendUI_1");
            return;
        }
        else
        {
            if (canRefreshEle)
            {
                FriendManager.Instance.SendGetElectFriendMsg();
                canRefreshEle = false;
                GameCenter.mIns.m_UIMgr.PopMsg(GameCenter.mIns.m_LanMgr.GetLan("FriendUI_6"));
            }
            else
            {
                GameCenter.mIns.m_UIMgr.PopMsg(GameCenter.mIns.m_LanMgr.GetLan("FriendUI_7"));
            }
        }
    }

    private void OnClickSearch()
    {
        string inputs = inputSearch.text;
        if (string.IsNullOrEmpty(inputs))
        {
            GameCenter.mIns.m_UIMgr.PopMsg(GameCenter.mIns.m_LanMgr.GetLan("FriendUI_8"));
            return;
        }
        int inputNum = 0;
        int.TryParse(inputs, out inputNum);
        if (inputNum == 0)
        {
            GameCenter.mIns.m_UIMgr.PopMsg(GameCenter.mIns.m_LanMgr.GetLan("FriendUI_9"));
            return;
        }

        if (inputs.Equals(GameCenter.mIns.userInfo.RoleId))
        {
            GameCenter.mIns.m_UIMgr.PopMsg(GameCenter.mIns.m_LanMgr.GetLan("FriendUI_10"));
            return;
        }

        // todo:check input string
        FriendManager.Instance.SendSearchFriendMsg(inputs);
    }

    public void OnReceiveEleFriendList(GEventArgs gEventArgs)
    {
        if (!openFlag)
        {
            return;
        }

        foreach (var item in eleItems)
        {
            if (item != null)
            {
                item.OnDestroy();
            }
        }
        eleItems = new List<EleItem>();

        FriendEventArgs friendEventArgs = (FriendEventArgs)gEventArgs;
        if (friendEventArgs != null)
        {
            Debug.Log("~~ OnReceiveEleFriendList " + friendEventArgs.serverContent);
            /*
             {"list":[{"cid":"qile","lan":"cn","lasttime":1690200552000,"level":1,"line":0,"nickname":"GTA-尹雪萍","os":0,"roleid":"5610315","uuid":"gta5610317","viplevel":0},{"cid":"qile","lan":"cn","lasttime":1690200552000,"level":1,"line":0,"nickname":"GTA-康凌瑶","os":0,"roleid":"5610379","uuid":"gta5610378","viplevel":0},{"cid":"qile","lan":"cn","lasttime":1690195781000,"level":1,"line":0,"nickname":"GTA-周幼旋","os":0,"roleid":"5610203","uuid":"gta5610207","viplevel":0},{"cid":"qile","lan":"cn","lasttime":1690200553000,"level":1,"line":0,"nickname":"GTA-岑雅霜","os":0,"roleid":"5610394","uuid":"gta5610396","viplevel":0},{"cid":"qile","lan":"cn","lasttime":1690200552000,"level":1,"line":0,"nickname":"GTA-赵千秋","os":0,"roleid":"5610359","uuid":"gta5610360","viplevel":0}]}
             */
            JsonData jd = jsontool.newwithstring(friendEventArgs.serverContent);
            if (jd != null && jd.ContainsKey("list"))
            {
                foreach (JsonData _jd in jd["list"])
                {
                    ContactData cd = JsonMapper.ToObject<ContactData>(JsonMapper.ToJson(_jd));
                    if (cd != null)
                    {
                        GameObject o = GameObject.Instantiate(eleItem);
                        o.transform.parent = eleItem.transform.parent;
                        o.transform.localScale = Vector3.one;
                        EleItem _eleItem = new EleItem(o, cd, true, (cd) =>
                        {
                            SetFriendInfoUIActive(true, cd);
                            foreach (var item in eleItems)
                            {
                                item.Selected(item.cd.roleid == cd.roleid);
                            }
                        },
                        (b, ei) =>
                        {
                            if (b)
                            {
                                // 移除该对象
                                for (int i = eleItems.Count - 1; i >= 0; i--)
                                {
                                    if (ei.cd.roleid == eleItems[i].cd.roleid)
                                    {
                                        if (friendInfo != null)
                                        {
                                            SetFriendInfoUIActive(false, eleItems[i].cd);
                                        }
                                        eleItems[i].OnDestroy();
                                        eleItems.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                        });
                        o.gameObject.SetActive(true);
                        eleItems.Add(_eleItem);
                    }
                }
            }
            //GameCenter.mIns.m_UIMgr.PopMsg("数据已刷新!");
        }
    }

    public void OnReceiveSearchFriend(GEventArgs gEventArgs)
    {
        if (!openFlag)
        {
            return;
        }

        FriendEventArgs friendEventArgs = (FriendEventArgs)gEventArgs;
        if (friendEventArgs != null)
        {
            Debug.Log("~~ OnReceiveSearchFriend " + friendEventArgs.serverContent);
            if (friendEventArgs.serverContent.ToString() == GameCenter.mIns.m_LanMgr.GetLan("FriendUI_2"))
            {
                GameCenter.mIns.m_UIMgr.PopMsg(friendEventArgs.serverContent.ToString());
                return;
            }
            JsonData jd = jsontool.newwithstring(friendEventArgs.serverContent);
            // {"friend":{"lan":"cn","lasttime":1690275310000,"level":1,"line":1,"nickname":"GTA-许冷玉","os":0,"roleid":"5610190","uuid":"gta5610191","viplevel":0}}
            if (jd != null && jd.ContainsKey("friend"))
            {
                ContactData cd = JsonMapper.ToObject<ContactData>(JsonMapper.ToJson(jd["friend"]));
                Debug.Log("~~ cd " + cd.ToString());
                SetSearchItemActive(true, cd);
            }
        }
    }




    private EleItem SearchItem = null;
    private void SetSearchItemActive(bool b, ContactData cd)
    {
        elelist.gameObject.SetActive(!b);
        string showid = inputSearch.text;
        if (b)
        {
            searchItem.gameObject.SetActive(b);
            SearchItem = new EleItem(searchItem, cd, true,
                (cd) =>
                {
                    SetFriendInfoUIActive(true, cd);
                },
                (b, ei) =>
                {
                    if (b)
                    {
                        OnClickRefresh();
                    }
                });
            showid = cd.roleid;
        }
        else
        {
            searchItem.gameObject.SetActive(false);
        }
        eleTitle.text = string.Format(GameCenter.mIns.m_LanMgr.GetLan("FriendUI_11"), showid);
    }

    private class EleItem
    {
        public GameObject o;
        public ContactData cd;
        private GameObject SelectedOBJ;
        Action<bool, EleItem> sendApplyCallback;
        public EleItem(GameObject o, ContactData cd, bool isSearch = false, Action<ContactData> clickCallback = null, Action<bool, EleItem> sendApplyCallback = null)
        {
            this.o = o;
            this.cd = cd;
            this.sendApplyCallback = sendApplyCallback;

            Transform tran = o.transform;
            SelectedOBJ = tran.FindHideInChild("imgSelect").gameObject;
            SelectedOBJ.SetActive(false);
            tran.FindHideInChild("txRoleName").GetComponent<TMP_Text>().SetTextExt(cd.nickname);
            Transform imgAvatarTran = tran.FindHideInChild("imgAvatar");
            imgAvatarTran.FindHideInChild("txLv").GetComponent<TMP_Text>().SetTextExt("Lv." + cd.level);
            tran.FindHideInChild("btnAdd").GetComponent<Button>().AddListenerBeforeClear(() =>
            {
                //
                Debug.Log(" select add cd:" + cd.ToString());
                Action<bool, string> callback = ApplyBoxCallback;
                GameCenter.mIns.m_UIMgr.PopCustomPrefab("addfriendpopwin", "common",
                    GameCenter.mIns.m_LanMgr.GetLan("FriendUI_12"), GameCenter.mIns.m_LanMgr.GetLan("FriendUI_13"), GameCenter.mIns.m_LanMgr.GetLan("FriendUI_14"), GameCenter.mIns.m_LanMgr.GetLan("FriendUI_15"), callback);
            });

            tran.GetComponent<Button>().AddListenerBeforeClear(() =>
            {
                clickCallback?.Invoke(cd);
            });
        }

        public void ApplyBoxCallback(bool isSure, string text)
        {
            Debug.Log(" ApplyBoxCallback isSure:" + isSure + " - text:" + text);
            if (isSure)
            {
                FriendManager.Instance.SendAddFriendMsg(cd.roleid, text);
                GameCenter.mIns.m_UIMgr.PopMsg(GameCenter.mIns.m_LanMgr.GetLan("FriendUI_16"));
            }
            sendApplyCallback?.Invoke(isSure, this);
        }

        public void Selected(bool b)
        {
            SelectedOBJ.SetActive(b);
        }

        public void OnDestroy()
        {
            if (o != null)
            {
                GameObject.Destroy(o);
            }
        }
    }
    #endregion



    #region FriendInfoUI
    private FriendInfoUI friendInfo;

    public void OnSelectFriendInfo(GEventArgs gEventArgs)
    {
        if (gEventArgs != null && gEventArgs.args != null)
        {
            ContactData contactData = (ContactData)gEventArgs.args[0];
            Debug.Log("on OnSelectFriendInfo contactData:" + contactData.ToString());
            if (contactData != null)
            {
                SetFriendInfoUIActive(true, contactData);
            }
        }
    }


    public async void SetFriendInfoUIActive(bool b, ContactData contactData)
    {
        if (friendInfo == null && b)
        {
            friendInfo = await OnLoadFriendInfoUIPrefab(contactData);
        }
        else
        {
            if (friendInfo != null)
            {
                friendInfo.SetActive(b);
                if (b)
                {
                    friendInfo.SetData(contactData);
                }
            }
        }
    }

    public async UniTask<FriendInfoUI> OnLoadFriendInfoUIPrefab(ContactData contactData)
    {
        GameObject loadObj = await ResourcesManager.Instance.LoadUIPrefabSync("friendinfo");
        if (loadObj != null)
        {
            FriendInfoUI friendInfo = loadObj.GetOrAddCompoonet<FriendInfoUI>();

            friendInfo.uiRoot = loadObj;
            friendInfo.uiRootTran = friendInfo.uiRoot.transform;
            friendInfo.uiRootRect = friendInfo.uiRoot.GetComponent<RectTransform>();

            friendInfo.uiRootTran.parent = parent;
            friendInfo.uiRootTran.localScale = Vector3.one;
            friendInfo.uiRootRect.anchorMax = new Vector2(1, 1);
            friendInfo.uiRootRect.anchorMin = new Vector2(0, 0);
            friendInfo.uiRootRect.pivot = new Vector2(0.5f, 0.5f);
            friendInfo.uiRootRect.anchoredPosition = Vector2.zero;
            friendInfo.uiRootRect.offsetMin = new Vector2(0, 0);
            friendInfo.uiRootRect.offsetMax = new Vector2(0, 0);

            friendInfo.InitUI(contactData);
            return friendInfo;
        }
        return null;
    }
    #endregion

}
