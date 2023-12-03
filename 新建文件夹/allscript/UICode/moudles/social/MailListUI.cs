using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MailListUI : SocialSubModuleUI
{
    public override EnumSocialTabType SocialTabType { get => EnumSocialTabType.Mail; }
    private GameObject uiRoot;
    private Transform uiRootTran;
    private RectTransform uiRootRect;
    public override GameObject UIRoot { get => uiRoot; }

    private Transform parent;

    private ScrollRect listsr;
    private Transform listContent;
    private Transform listItem;
    private Button btnDelete;
    private Button btnOneTimeReceived;
    private TableView tableView = null;
    private TMP_Text noMailText = null;


    // 邮件item列表
    private List<MailListItem> mailListItems = new List<MailListItem>();
    // 当前选中的邮件
    private MailListItem currSelectedMI;
    // 邮件详情ui预制件
    private MailDetailUI mailDetailUI;

    /// <summary>
    /// 加载邮件列表ui
    /// </summary>
    /// <param name="parent"></param>
    public override async void LoadUIPrefab(Transform parent)
    {
        this.parent = parent;
        GameObject loadObj = await ResourcesManager.Instance.LoadUIPrefabSync("maillist");
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

        GameEventMgr.Register(GEKey.OnReadMails, OnMailRead);
        GameEventMgr.Register(GEKey.OnReceivedMails, OnMailReceived);
        GameEventMgr.Register(GEKey.OnDeleteMails, OnMailDelete);
        GameEventMgr.Register(GEKey.OnServerMailPush.ToString() + "_client", OnMailServerMailPush);
        GameEventMgr.Register(GEKey.OnGetAllMails, OnDataOK);
    }

    /// <summary>
    /// 初始化界面
    /// </summary>
    public override void InitUI()
    {
        Transform _root = uiRootTran.Find("root");
        listsr = uiRootTran.Find("root/list").GetComponent<ScrollRect>();
        listContent = uiRootTran.Find("root/list/content");
        listItem = listContent.FindHideInChild("mailItem");
        btnDelete = uiRootTran.Find("root/btnDelete").GetComponent<Button>();
        btnOneTimeReceived = uiRootTran.Find("root/btnOneTimeReceived").GetComponent<Button>();
        tableView = listContent.GetComponent<TableView>();
        noMailText = _root.FindHideInChild("noMailText").GetComponent<TMP_Text>();
        noMailText.gameObject.SetActive(false);

        btnDelete.SetText("删除已读");
        btnOneTimeReceived.SetText("一键领取");

        btnDelete.AddListenerBeforeClear(OnClickDelete);
        btnOneTimeReceived.AddListenerBeforeClear(OnClickOneTimeReceived);

        if (MailManager.Instance.IsHasNotMail())
        {
            // 若没有邮件数据，进入界面去拉取一下
            MailManager.Instance.OnGetAllList(() =>
            {
            });
        }
        else
        {
            InitUIData();
        }
    }

    /// <summary>
    /// 显隐
    /// </summary>
    /// <param name="b"></param>
    public override void SetActive(bool b)
    {
        if (UIRoot != null)
        {
            UIRoot.SetActive(b);
            if (mailDetailUI != null)
            {
                if (mailListItems.Count > 0 && currSelectedMI != null && b)
                    mailDetailUI.SetActive(true);
                else
                    mailDetailUI.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 销毁邮件列表预制件
    /// </summary>
    public override void DestroyUIPrefab()
    {
        if (mailDetailUI != null)
        {
            mailDetailUI.DestroyUIPrefab();
        }
        mailDetailUI = null;

        if (uiRoot != null)
        {
            GameObject.Destroy(uiRoot);
        }

        GameEventMgr.UnRegister(GEKey.OnReadMails, OnMailRead);
        GameEventMgr.UnRegister(GEKey.OnReceivedMails, OnMailReceived);
        GameEventMgr.UnRegister(GEKey.OnDeleteMails, OnMailDelete);
        GameEventMgr.UnRegister(GEKey.OnServerMailPush.ToString() + "_client", OnMailServerMailPush);
        GameEventMgr.UnRegister(GEKey.OnGetAllMails, OnDataOK);
    }

    private void OnDataOK(GEventArgs gEventArgs)
    {
        InitUIData();
    }

    /// <summary>
    /// 初始化ui数据
    /// </summary>
    private void InitUIData()
    {
        if (tableView == null)
        {
            zxlogger.logerrorformat("Error: mail list tableview is null!");
            return;
        }
        List<MailData> mailDatas = MailManager.Instance.GetMailDataList();
        mailDatas.Sort(new MailDataSort());

        noMailText.gameObject.SetActive(mailDatas.Count <= 0);
        Debug.Log("Mail list ui init data: mail data count:" + mailDatas.Count);
        foreach (var md in mailDatas)
        {
            MailListItem mailListItem = new MailListItem(md);
            bool isUpdate = false;
            for (int i = 0; i < mailListItems.Count; i++)
            {
                if (mailListItems[i].md.id == md.id)
                {
                    mailListItems[i] = mailListItem;
                    isUpdate = true;
                    break;
                }
            }
            if (!isUpdate)
                mailListItems.Add(mailListItem);
        }
        if (mailListItems != null)
        {
            tableView.onItemRender = OnMailItemRender;
            tableView.onItemDispose = OnMailItemDispose;
            tableView.SetDatas(mailListItems.Count, false);
        }

        if (mailListItems != null && mailListItems.Count > 0)
        {
            OnSelceted(mailListItems[0]);
        }
    }

    /// <summary>
    /// 通过列表索引获取邮件item
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private MailListItem GetMailItemByIndex(int index)
    {
        int counter = 0;
        foreach (var mi in mailListItems)
        {
            if (counter == index)
                return mi;
            counter++;
        }
        return null;
    }

    /// <summary>
    /// 一个邮件item被渲染
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="index"></param>
    private void OnMailItemRender(GameObject obj, int index)
    {
        MailListItem mi = GetMailItemByIndex(index - 1);
        if (mi != null)
        {
            obj.name = "mailitem_" + mi.md.id.ToString();
            mi.BindObj(obj.transform.parent, obj, index, (_mi) =>
            {
                if (_mi != null)
                {
                    OnSelceted(_mi);
                }
            }, false); // bOpenAnim);

            //if (bOpenAnim)
            //{
            //    EnqueueWHItemWhenUIOpening(wareHouseItem);
            //}

            if (currSelectedMI != null)
            {
                mi.SetSelectedImage(currSelectedMI.md.id == mi.md.id);
            }
        }
    }

    /// <summary>
    /// 一个邮件item取消渲染
    /// </summary>
    private void OnMailItemDispose()
    {

    }

    /// <summary>
    /// 一键删除已读
    /// </summary>
    private void OnClickDelete()
    {
        MailManager.Instance.OnOneTimeDeleteMail();

        // test
        //List<MailData> all_rd = MailManager.Instance.GetMailsByMailState(EnumMailState.Readed);
        //List<MailData> all_rr = MailManager.Instance.GetMailsByMailState(EnumMailState.AlreadyReceived);
        //List<int> deleteList = new List<int>();
        //foreach (var item in all_rd)
        //{
        //    deleteList.Add(item.id);
        //}
        //foreach (var item in all_rr)
        //{
        //    deleteList.Add(item.id);
        //}

        //MailManager.Instance.RemoveMails(deleteList);
        //GameEventMgr.Distribute(GameEventKey.OnDeleteMails.ToString(), new MailEventArgs(deleteList));
    }

    /// <summary>
    /// 一键领取
    /// </summary>
    private void OnClickOneTimeReceived()
    {
        MailManager.Instance.OnOneTimeReceivedMail();

        // test
        //List<MailData> all_ur = MailManager.Instance.GetMailsByMailState(EnumMailState.UnReceived);
        //List<int> receivedList = new List<int>();
        //foreach (var item in all_ur)
        //{
        //    MailManager.Instance.UpdateMailState(item.id, EnumMailState.AlreadyReceived);
        //    receivedList.Add(item.id);
        //}
        //GameEventMgr.Distribute(GameEventKey.OnReceivedMails.ToString(), new MailEventArgs(receivedList));
    }

    /// <summary>
    /// 选中一个邮件
    /// </summary>
    /// <param name="mi"></param>
    private void OnSelceted(MailListItem mi)
    {
        if (mi != null)
        {
            if (currSelectedMI != null && currSelectedMI.md.id == mi.md.id)
            {
                return;
            }

            foreach (var _mi in mailListItems)
            {
                bool b = _mi.md.id == mi.md.id;
                if (_mi.obj != null && mi.obj != null && _mi.obj.name == mi.obj.name && _mi.md.id != mi.md.id)
                {
                    // 之前点击的对象和现在点击的对象共同使用一个obj，不处理
                }
                else
                {
                    _mi.SetSelectedImage(_mi.md.id == mi.md.id);
                    if (mi.md.state == (int)EnumMailState.UnRead && _mi.md.id == mi.md.id)
                    {
                        MailManager.Instance.OnReadMail(mi.md.id);
                    }
                }
            }
            currSelectedMI = mi;
            LoadMailDetailPanel(currSelectedMI.md);
        }
    }

    /// <summary>
    /// 加载邮件详情ui
    /// </summary>
    /// <param name="mailData"></param>
    private void LoadMailDetailPanel(MailData mailData)
    {
        if (mailData == null)
        {
            return;
        }

        if (mailDetailUI == null)
        {
            mailDetailUI = new MailDetailUI();
            mailDetailUI.LoadUIPrefab(parent);
        }

        mailDetailUI.SetUIData(mailData);
    }

    /// <summary>
    /// 接受到邮件被读取事件
    /// </summary>
    /// <param name="gEventArgs"></param>
    private void OnMailRead(GEventArgs gEventArgs)
    {
        MailEventArgs mailEventArgs = (MailEventArgs)gEventArgs;
        if (mailEventArgs != null && mailEventArgs.disposeMails != null && mailEventArgs.disposeMails.Count > 0)
        {
            foreach (int disposeMailID in mailEventArgs.disposeMails)
            {
                foreach (var item in mailListItems)
                {
                    if (item.md.id == disposeMailID)
                    {
                        item.MarkReaded();
                        if (mailDetailUI != null)
                        {
                            mailDetailUI.MarkReaded(item.md);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 接收到邮件被领取事件
    /// </summary>
    /// <param name="gEventArgs"></param>
    private void OnMailReceived(GEventArgs gEventArgs)
    {
        MailEventArgs mailEventArgs = (MailEventArgs)gEventArgs;
        if (mailEventArgs != null && mailEventArgs.disposeMails != null && mailEventArgs.disposeMails.Count > 0)
        {
            foreach (int disposeMailID in mailEventArgs.disposeMails)
            {
                foreach (var item in mailListItems)
                {
                    if (item.md.id == disposeMailID)
                    {
                        item.MarkReceived();
                        if (mailDetailUI != null)
                        {
                            mailDetailUI.MarkReceived(item.md);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 接受到邮件被删除事件
    /// </summary>
    /// <param name="gEventArgs"></param>
    private void OnMailDelete(GEventArgs gEventArgs)
    {
        bool isDelectCurrSelectItem = false;
        MailEventArgs mailEventArgs = (MailEventArgs)gEventArgs;
        if (mailEventArgs != null && mailEventArgs.disposeMails != null && mailEventArgs.disposeMails.Count > 0)
        {
            foreach (int disposeMailID in mailEventArgs.disposeMails)
            {
                for (int i = mailListItems.Count - 1; i >= 0; i--)
                {
                    if (mailListItems[i].md.id == disposeMailID)
                    {
                        if (currSelectedMI != null && currSelectedMI.md.id == disposeMailID)
                        {
                            isDelectCurrSelectItem = true;
                        }
                        mailListItems.RemoveAt(i);
                    }
                }
            }
            tableView.SetDatas(mailListItems.Count, false);
            tableView.RefreshDatas();

            if (isDelectCurrSelectItem)
            {
                if (mailListItems != null && mailListItems.Count > 0)
                {
                    OnSelceted(mailListItems[0]);
                }
            }
        }
        if (mailListItems.Count <= 0)
        {
            if (mailDetailUI != null)
                mailDetailUI.SetActive(false);
            noMailText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 接受都服务端主动推送的邮件事件
    /// </summary>
    /// <param name="gEventArgs"></param>
    private void OnMailServerMailPush(GEventArgs gEventArgs)
    {
        if (mailListItems == null)
        {
            mailListItems = new List<MailListItem>();
        }
        List<MailData> mds = MailManager.Instance.GetAllLocalMailDatas();
        List<MailData> newAddMds = new List<MailData>();
        // 只检测新加入的
        foreach (var localmd in mds)
        {
            bool has = false;
            foreach (var uimd in mailListItems)
            {
                if (localmd.id == uimd.md.id)
                {
                    has = true;
                    break;
                }
            }
            if (!has)
            {
                newAddMds.Add(localmd);
            }
        }

        for (int i = 0; i < newAddMds.Count; i++)
        {
            MailListItem mli = new MailListItem(newAddMds[i]);
            bool isUpdate = false;
            for (int j = 0; j < mailListItems.Count; j++)
            {
                if (mailListItems[j].md.id == newAddMds[i].id)
                {
                    mailListItems[j] = mli;
                    isUpdate = true;
                    break;
                }
            }
            if (!isUpdate)
                mailListItems.Add(mli);
        }

        if (newAddMds.Count > 0)
        {
            tableView.SetDatas(mailListItems.Count, false);
            tableView.RefreshDatas();
        }
    }

    /// <summary>
    /// 邮件列表item对象
    /// </summary>
    private class MailListItem
    {
        public Transform parent;
        public GameObject obj;
        public int index;
        public MailData md;
        public Action<MailListItem> clickCallback;

        public Button btnClick;
        public GameObject bgNotSelect;
        public GameObject bgSelect;
        public GameObject mailIconObj;
        public MailIcon mailIcon;
        public TMP_Text txTitle;
        public TMP_Text txSender;
        public TMP_Text txTime;

        public MailListItem() { }

        public MailListItem(MailData md) { this.md = md; }

        public void BindObj(Transform parent, GameObject obj, int index, Action<MailListItem> clickCallback, bool bOpenAnim = false)
        {
            this.parent = parent;
            this.obj = obj;
            this.index = index;
            this.clickCallback = clickCallback;

            bgNotSelect = obj.transform.FindHideInChild("imgBg").gameObject;
            bgSelect = obj.transform.FindHideInChild("imgSelect").gameObject;

            mailIconObj = obj.transform.FindHideInChild("mailicon").gameObject;
            mailIcon = new MailIcon(md, mailIconObj, (_mailIcon) => { });

            txTitle = obj.transform.Find("txTitle").GetComponent<TMP_Text>();
            txSender = obj.transform.Find("txSender").GetComponent<TMP_Text>();
            txTime = obj.transform.Find("txTime").GetComponent<TMP_Text>();

            btnClick = obj.GetComponent<Button>();
            btnClick.AddListenerBeforeClear(OnClick);

            if (md != null)
            {
                txTitle.SetTextWithEllipsis(md.title, 16);
                txSender.SetTextExt(md.sender);
                txTime.SetTextExt(datetool.TimeStampToFormatDateString(md.sendtime, "Y-Mo-D H:Mi:S", false));
            }
            bgNotSelect.SetActive(true);
        }

        private void OnClick()
        {
            clickCallback?.Invoke(this);
        }

        public void SetSelectedImage(bool bSelected)
        {
            if (bgNotSelect != null)
                bgNotSelect.SetActive(!bSelected);
            if (bgSelect != null)
                bgSelect.SetActive(bSelected);

            if (txTitle != null)
                txTitle.color = bSelected ? Color.white : Color.black;
            // 83878B-> 邮件列表未选中文本
            // B0BAD0-> 邮件列表选中文本
            if (txSender != null)
                txSender.color = bSelected ? commontool.GetColor("FFB0BAD0") : commontool.GetColor("FF83878B");
            if (txTime != null)
                txTime.color = bSelected ? commontool.GetColor("FFB0BAD0") : commontool.GetColor("FF83878B");
        }

        public override string ToString()
        {
            string objName = obj == null ? "nil" : obj.name;
            return $"click obj:{objName}, mail data:{md.ToString()}";
        }

        /// <summary>
        /// 标记未已读
        /// </summary>
        public void MarkReaded()
        {
            if (md == null || mailIcon == null)
            {
                return;
            }
            md.SetState((int)EnumMailState.Readed);
            mailIcon.SetIcon((int)EnumMailState.Readed);
        }

        /// <summary>
        /// 标记未已领取
        /// </summary>
        public void MarkReceived()
        {
            if (md == null || mailIcon == null)
            {
                return;
            }
            md.SetState((int)EnumMailState.AlreadyReceived);
            mailIcon.SetIcon((int)EnumMailState.AlreadyReceived);
        }
    }
}
