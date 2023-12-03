using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class Warehouse
{
    #region 定义和生命周期
    public override UILayerType uiLayerType => UILayerType.Normal;
    public override string uiAtlasName => "warehouse";

    private float openAnimTimer = 0.0f;
    private float itemAnimTimer = 0.0f;
    private bool bOpenAnim = false;

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

        TopResourceBar topResBar = new TopResourceBar(_Root, this, () =>
        {
            this.Close();
            topResBar = null;
            return true;
        });
        // data
        // WarehouseManager.Instance.InitWarehouseDatas();

        InitDatas();
        InstantLeftTabs();
        InstantWarehouseItems();
        // 右侧详情动画
        // nodeRightAreaConent.GetOrAddCompoonet<RectTransform>().anchoredPosition = new Vector2(650, nodeRightAreaConent.GetOrAddCompoonet<RectTransform>().anchoredPosition.y);
        // nodeRightAreaConent.GetOrAddCompoonet<UdoTween>().doanchorposx(19f, 0.5f, false);
        // 锁定mask, 动画完成前无法点击
        // todo:更好的方式
        imgLockMask.SetActive(true);
        openAnimTimer = 0.0f;
        itemAnimTimer = 0.0f;
        bOpenAnim = true;
        WHItemQueueWhenUIOpening = new Queue<WareHouseItem>();
    }

    public override void UpdateWin()
    {
        if (bOpenAnim)
        {
            openAnimTimer += Time.deltaTime;
            if (openAnimTimer >= 1.0f)
            {
                imgLockMask.SetActive(false);
                bOpenAnim = false;
                openAnimTimer = 0.0f;
                if (WHItemQueueWhenUIOpening.Count > 0)
                {
                    int counter = 0;
                    while (WHItemQueueWhenUIOpening.Count > 0 && counter <= 1000)
                    {
                        counter++;
                        WareHouseItem whi = DequeueWHItemWhenUIOpening();
                        if (whi != null)
                            whi.DoAnim();
                    }
                }
                if (WarehouseManager.Instance.currSelectedItemID > 0)
                {
                    // 默认选中
                    int index = 0;
                    foreach (var item in wareHouseDict[WarehouseManager.Instance.currSelectItemTabType])
                    {
                        if (item.data.Id == WarehouseManager.Instance.currSelectedItemID)
                        {
                            int toIndex = index + 10 >= wareHouseDict[WarehouseManager.Instance.currSelectItemTabType].Count ? wareHouseDict[WarehouseManager.Instance.currSelectItemTabType].Count : index + 10;
                            tableView.ScrollToIndex(toIndex, 0.5f, 1f);
                            OnSelectItem(item);
                            break;
                        }
                        index++;
                    }
                }
            }

            itemAnimTimer += Time.deltaTime;
            if (itemAnimTimer >= 0.02f)
            {
                WareHouseItem whi = DequeueWHItemWhenUIOpening();
                if (whi != null)
                    whi.DoAnim();
                itemAnimTimer = 0.0f;
            }

        }
    }

    protected override void OnClose()
    {
        base.OnClose();
        foreach (var item in leftTabItems)
        {
            item.OnDestroy();
        }
        ClearUI();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    #endregion

    #region 事件
    protected override void OnRegister(bool register)
    {
        if (register)
        {
            AddUIEvent(GEKey.OnItemNumberChanged, OnItemNumberChanged);
            AddUIEvent(GEKey.OnItemAdd, OnItemAdd);
            AddUIEvent(GEKey.OnItemRemove, OnItemRemove);
        }
        base.OnRegister(register);
    }

    partial void tabItem_Click()
    {

    }

    partial void whItem_Click()
    {

    }

    /// <summary>
    /// 点击使用物品
    /// </summary>
    partial void btnUse_Click()
    {
        // test
        // GameEventMgr.Distribute(GameEventKey.OnItemNumberChanged.ToString(), 411201, 5);

        // GameEventMgr.Distribute(GameEventKey.OnItemAdd.ToString(), new ItemData(411204, 411204, 6));

        //GameCenter.mIns.m_UIMgr.PopCustomPrefab("netprompt");

        //GameCenter.mIns.m_UIMgr.PopMsg("test");

        //GameCenter.mIns.m_UIMgr.PopWindowPrefab(new PopWinStyle(
        //        "标题",
        //        "内容",
        //        2
        //    ));

        //GameCenter.mIns.m_UIMgr.PopFullScreenPrefab(new PopFullScreenStyle(
        //       "标题",
        //       "内容"
        //   ));
    }

    /// <summary>
    /// 点击使用详情弹窗
    /// </summary>
    partial void btnItemDetail_Click()
    {
        if (currSelectedWHItem != null)
        {
            pop.gameObject.SetActive(true);
            pop.gameObject.GetOrAddCompoonet<UdoTween>().docanvasgroupfade(255f, 0.2f);
            popTxName.text = currSelectedWHItem.cfg.name;
            popTxContent.text = currSelectedWHItem.cfg.describe;
        }
    }

    /// <summary>
    /// 物品数量发生变化了
    /// </summary>
    /// <param name="gEventArgs"></param>
    private void OnItemNumberChanged(GEventArgs gEventArgs)
    {
        if (GameEventMgr.CheckGEventArgsLegality(gEventArgs))
        {
            int id = (int)gEventArgs.args[0];
            int newNumber = (int)gEventArgs.args[1];
            WareHouseItem wareHouseItem = GetWareHouseItem(id);
            if (wareHouseItem != null)
            {
                ItemType iType = (ItemType)wareHouseItem.cfg.type;
                if (newNumber <= 0)
                {
                    DoRemoveItem(wareHouseItem, iType);
                }
                else
                {
                    wareHouseItem.UpdateNumber(newNumber);
                }
            }
        }
    }

    /// <summary>
    /// 来了新物品
    /// </summary>
    /// <param name="gEventArgs"></param>
    private void OnItemAdd(GEventArgs gEventArgs)
    {
        if (GameEventMgr.CheckGEventArgsLegality(gEventArgs))
        {
            try
            {
                ItemData itemData = (ItemData)gEventArgs.args[0];
                WareHouseItem wareHouseItem = GetWareHouseItem(itemData.Id);
                if (itemData == null || wareHouseItem != null)
                {
                    zxlogger.logerror("Error: Add Same Item! itemData:" + itemData.ToString());
                    return;
                }
                WareHouseItem whi = new WareHouseItem(itemData);
                // todo: test item type
                wareHouseDict[(ItemType)wareHouseItem.cfg.type].Add(whi);
                if (true)//(whi.cfg.type == currSelectTab)
                {
                    tableView.SetDatas(wareHouseDict[(ItemType)currSelectTab].Count, false);
                    tableView.RefreshDatas();
                }
            }
            catch (Exception ex)
            {
                zxlogger.logerror("Error: AOnItemAdd ex:" + ex);
            }
        }
    }

    /// <summary>
    /// 物品被移除了
    /// </summary>
    /// <param name="gEventArgs"></param>
    private void OnItemRemove(GEventArgs gEventArgs)
    {
        if (GameEventMgr.CheckGEventArgsLegality(gEventArgs))
        {
            int id = (int)gEventArgs.args[0];
            WareHouseItem wareHouseItem = GetWareHouseItem(id);
            if (wareHouseItem != null)
            {
                ItemType iType = (ItemType)wareHouseItem.cfg.type;

                DoRemoveItem(wareHouseItem, iType);
            }
        }
    }

    /// <summary>
    /// 移除一个物品
    /// </summary>
    /// <param name="wareHouseItem"></param>
    /// <param name="iType"></param>
    private void DoRemoveItem(WareHouseItem wareHouseItem, ItemType iType)
    {
        List<WareHouseItem> whis = wareHouseDict[iType];
        for (int i = whis.Count - 1; i >= 0; i--)
        {
            WareHouseItem whi = whis[i];
            if (whi.data.Id == wareHouseItem.data.Id)
            {
                //whi.OnDestroy();
                whis.RemoveAt(i);
                if (iType == (ItemType)currSelectTab)
                {
                    tableView.RefreshDatas();
                }
                break;
            }
        }
    }
    #endregion

    #region functions
    /// 这里只处理ui表现层对象
    TableView tableView = null;
    // 当前选中标签
    private int currSelectTab = -1;
    // 左边tab列表
    private List<LeftTabItem> leftTabItems = new List<LeftTabItem>();
    // 仓库Item字典
    private Dictionary<ItemType, List<WareHouseItem>> wareHouseDict = new Dictionary<ItemType, List<WareHouseItem>>();
    // 当前选中item
    private WareHouseItem currSelectedWHItem = null;
    // 是否切换了tab
    private bool isSwitchedTab = false;

    /// <summary>
    /// 初始化同步仓库数据
    /// </summary>
    private void InitDatas()
    {
        for (int i = 1; i <= 4; i++)
        {
            if (!wareHouseDict.ContainsKey((ItemType)i))
                wareHouseDict.Add((ItemType)i, new List<WareHouseItem>());
        }

        Dictionary<long, ItemData> serverdata = WarehouseManager.Instance.GetDatas();
        if (serverdata == null || serverdata.Count <= 0)
        {
            return;
        }

        foreach (var seritem in serverdata)
        {
            ItemCfgData itemCfgData = GameCenter.mIns.m_CfgMgr.GetItemCfgData(seritem.Value.Pid);
            if (itemCfgData != null)
            {
                WareHouseItem whi = new WareHouseItem(seritem.Value);
                // todo: test item type
                wareHouseDict[(ItemType)whi.cfg.type].Add(whi);
            }
        }
    }

    /// <summary>
    /// 重置ui, 清空数据
    /// </summary>
    public void ClearUI()
    {
        tableView = null;
        currSelectTab = -1;
        leftTabItems = new List<LeftTabItem>();
        wareHouseDict = new Dictionary<ItemType, List<WareHouseItem>>();
    }

    /// <summary>
    /// 获取仓库界面item
    /// </summary>
    /// <param name="itemType"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private WareHouseItem GetWareHouseItem(ItemType itemType, int index)
    {
        if (wareHouseDict.ContainsKey(itemType))
        {
            int counter = 0;
            foreach (var item in wareHouseDict[itemType])
            {
                if (counter == index)
                {
                    return item;
                }
                counter++;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取仓库界面item
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private WareHouseItem GetWareHouseItem(int id)
    {
        foreach (var items in wareHouseDict)
        {
            foreach (var item in items.Value)
            {
                if (item.data.Id == id)
                {
                    return item;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 初始化tab列表
    /// </summary>
    private void InstantLeftTabs()
    {
        leftTabItems = new List<LeftTabItem>();
        GameObject ori = tabItem.gameObject;
        ori.SetActive(false);
        for (int i = 1; i <= 4; i++)
        {
            GameObject cloneObj = GameObject.Instantiate(ori);
            LeftTabItem leftTabItem = new LeftTabItem(ori.transform.parent, cloneObj, i, (index) =>
            {
                OnSwitchTab(index);
            });
            leftTabItems.Add(leftTabItem);
        }


        OnSwitchTab((int)WarehouseManager.Instance.currSelectItemTabType, true);
    }

    /// <summary>
    /// 切换tab
    /// </summary>
    /// <param name="index">新选中的tabidnex</param>
    private void OnSwitchTab(int index, bool isInit = false)
    {
        if (currSelectTab == index)
        {
            return;
        }
        for (int i = 0; i <= leftTabItems.Count -1 ; i++)
        {
            leftTabItems[i].SetImgSelectedState(index == i +1, (repeatClick) =>
            {
                if (!repeatClick)
                {
                    OnSwitchWareHoseType((ItemType)index);
                }
            });
        }
        currSelectTab = index;
        if (tableView != null && !isInit)
        {
            tableView.ScrollToIndex(1, 1, 1);
        }
        isSwitchedTab = true;
    }

    /// <summary>
    /// 初始化仓库items
    /// </summary>
    private void InstantWarehouseItems()
    {
        tableView = warehouseList.transform.GetComponentInChildren<TableView>();
        if (tableView == null)
        {
            zxlogger.logerrorformat("Error: ware house tableview is null!");
            return;
        }
        tableView.onItemRender = OnItemRender;
        tableView.onItemDispose = OnItemDispose;
        tableView.SetDatas(wareHouseDict[(ItemType)currSelectTab].Count, false);
    }

    /// <summary>
    /// 当一个item渲染
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="index"></param>
    private void OnItemRender(GameObject obj, int index)
    {
        WareHouseItem wareHouseItem = GetWareHouseItem((ItemType)currSelectTab, index - 1);
        if (wareHouseItem != null)
        {
            obj.name = wareHouseItem.data.Pid.ToString();
            wareHouseItem.BindObj(obj.transform.parent, obj, index, (whi) =>
            {
                if (whi != null)
                {
                    OnSelectItem(whi);
                }
            }, bOpenAnim);

            if (bOpenAnim)
            {
                EnqueueWHItemWhenUIOpening(wareHouseItem);
            }

            if (isSwitchedTab && index == 1)
            {
                if (currSelectedWHItem != null)
                {
                    currSelectedWHItem.DoOnClickEffect(false);
                }
                isSwitchedTab = false;
                OnSelectItem(wareHouseItem);
            }
        }
    }

    /// <summary>
    /// 动画打开是加载的item对象队列，用于动画表现
    /// </summary>
    private Queue<WareHouseItem> WHItemQueueWhenUIOpening = new Queue<WareHouseItem>();

    /// <summary>
    /// 将入队列
    /// </summary>
    /// <param name="whi"></param>
    private void EnqueueWHItemWhenUIOpening(WareHouseItem whi)
    {
        WHItemQueueWhenUIOpening.Enqueue(whi);
    }

    /// <summary>
    /// 取出，表现动画
    /// </summary>
    /// <returns></returns>
    private WareHouseItem DequeueWHItemWhenUIOpening()
    {
        if (WHItemQueueWhenUIOpening.Count <= 0)
        {
            return null;
        }
        return WHItemQueueWhenUIOpening.Dequeue();
    }

    /// <summary>
    /// 当一个item消除
    /// </summary>
    private void OnItemDispose()
    {

    }

    /// <summary>
    /// 切换了仓库显示item类型
    /// </summary>
    /// <param name="itemType">新的显示类型</param>
    private void OnSwitchWareHoseType(ItemType itemType)
    {
        if (wareHouseDict.ContainsKey(itemType) && tableView != null)
        {
            tableView.SetDatas(wareHouseDict[itemType].Count, false);
        }

        switch (itemType)
        {
            case ItemType.Debris:
            default:

                break;
            case ItemType.GiftBag:
                break;
            case ItemType.Equipment:

                break;
            case ItemType.Materials:

                break;
        }
    }

    /// <summary>
    /// 排序
    /// </summary>
    private void Sort(WarehouseSortType wareHoseSortType)
    {
        bool sortSucc = WarehouseManager.Instance.ChangedSortType(wareHoseSortType);
        if (sortSucc)
        {
            // todo:refresh items

        }
    }

    /// <summary>
    /// 选中item
    /// </summary>
    /// <param name="selectedWHItem"></param>
    private void OnSelectItem(WareHouseItem selectedWHItem)
    {
        if (selectedWHItem == null)
        {
            return;
        }

        if (currSelectedWHItem != null)
        {
            currSelectedWHItem.DoOnClickEffect(false);
        }
        currSelectedWHItem = selectedWHItem;
        currSelectedWHItem.DoOnClickEffect(true);
        RefreshItemDetailInfo(currSelectedWHItem);
        WarehouseManager.Instance.currSelectedItemID = currSelectedWHItem.data.Id;
    }

    private void RefreshItemDetailInfo(WareHouseItem selectedWHItem)
    {
        if (selectedWHItem == null)

        {
            return;
        }

        //imgItemDetailQuality.sprite = null;
        //imgItemDetailIcon.sprite = null;
        txItemDetailNumberValue.text = selectedWHItem.data.Number.ToString();
        txItemDetailName.text = GameCenter.mIns.m_LanMgr.GetLan(selectedWHItem.cfg.name);
        txBottomDes.text = selectedWHItem.cfg.describe;
        imgItemDetailIcon.sprite = SpriteManager.Instance.GetSpriteSync("Icon_pack_libao_blue");

        LayoutRebuilder.ForceRebuildLayoutImmediate(txItemDetailName.transform.GetComponent<RectTransform>());
    }

    #endregion

    #region inner class
    #region left tab item
    /// <summary>
    /// tab 对象
    /// </summary>
    private class LeftTabItem
    {
        private Transform parent;
        private Transform trans;
        private int index;
        private Action<int> clickCallback;
        public Button btnClick;
        public GameObject imgSelected;
        public Image imgIcon;
        public Text txTab;

        public LeftTabItem() { }
        public LeftTabItem(Transform parent, GameObject obj, int index, Action<int> clickCallback)
        {
            if (obj == null)
            {
                return;
            }
            trans = obj.transform;
            this.parent = parent;
            this.index = index;
            this.clickCallback = clickCallback;
            btnClick = trans.GetChild(0).GetComponent<Button>();
            imgSelected = trans.GetChild(0).GetChild(0).gameObject;
            imgIcon = trans.GetChild(0).Find("imgIcon").GetComponent<Image>();
            txTab = trans.GetChild(0).Find("txName").GetComponent<Text>();

            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;

            btnClick.onClick.AddListener(OnClick);
            SetStely(false);
            DoAnim();
        }

        public void DoAnim()
        {
            //GameObject animRoot = trans.GetChild(0).gameObject;
            //animRoot.GetComponent<RectTransform>().anchoredPosition = new Vector2(-300, 0);
            //CanvasGroup canvasGroup = animRoot.GetOrAddCompoonet<CanvasGroup>();
            //canvasGroup.alpha = 0;
            trans.gameObject.SetActive(true);
            //animRoot.GetOrAddCompoonet<UdoTween>().doanchorposx(0f, 0.5f, false).docanvasgroupfade(255f, 0.5f);
        }

        private void SetStely(bool isClick)
        {
            string iconResName = "";
            string tx = "";
            switch ((ItemType)index)
            {
                case ItemType.Debris:
                default:
                    iconResName = isClick ? "Icon_pack_suipian_white" : "Icon_pack_suipian_black";
                    tx = GameCenter.mIns.m_LanMgr.GetLan("warehouse_1");
                    break;
                case ItemType.GiftBag:
                    iconResName = isClick ? "Icon_pack_suipian_white" : "Icon_pack_libao_black";
                    tx = GameCenter.mIns.m_LanMgr.GetLan("warehouse_2");
                    break;
                case ItemType.Equipment:
                    iconResName = isClick ? "Icon_pack_suipian_white" : "Icon_pack_zhuangbei_black";
                    tx = GameCenter.mIns.m_LanMgr.GetLan("warehouse_3");
                    break;
                case ItemType.Materials:
                    iconResName = isClick ? "Icon_pack_suipian_white" : "Icon_pack_cailiao_black";
                    tx = GameCenter.mIns.m_LanMgr.GetLan("warehouse_4");
                    break;
            }

            imgIcon.sprite = SpriteManager.Instance.GetSpriteSync(iconResName);
            txTab.text = tx;
            txTab.color = isClick ? Color.white : Color.black;
            txTab.GetComponent<RectTransform>().anchoredPosition = isClick ? new Vector2(45f, 3f) : new Vector2(22.5f, 3f);
        }

        /// <summary>
        /// 点击
        /// </summary>
        public void OnClick()
        {
            //SetImgSelectedState(true, null);
            clickCallback?.Invoke(index);
            SetStely(true);
            WarehouseManager.Instance.currSelectItemTabType = (ItemType)index;
        }

        /// <summary>
        /// 选中
        /// </summary>
        /// <param name="bActive">是否选中</param>
        /// <param name="repeatClick">选中回调</param>
        public void SetImgSelectedState(bool bActive, Action<bool> repeatClick)
        {
            if (imgSelected.activeSelf != bActive)
            {
                imgSelected.SetActive(bActive);
                SetStely(bActive);
                if (bActive)
                {
                    repeatClick?.Invoke(false);
                }
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void OnDestroy()
        {
            if (trans != null)
            {
                GameObject.Destroy(trans.gameObject);
            }
        }
    }
    #endregion
    #endregion
}