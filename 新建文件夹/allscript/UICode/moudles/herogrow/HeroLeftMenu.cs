
using System;
using UnityEngine;
using DG.Tweening;
using static HeroGrow;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Org.BouncyCastle.Asn1.Crmf;
using Google.Protobuf.WellKnownTypes;
using Cysharp.Threading.Tasks;

public class HeroLeftMenu
{
    private float duration;
    private Transform leftMenu;
    private GameObject tabItem;
    private Transform heroMenu;
    private Transform listhero;

    private Image heroList;
    private GameObject heroItem;

    private Action<TabType, TabType> OnSwitchTabType;
    private Action<HeroData, HeroInfoCfgData> onSwitchHero;
    private Action<HeroData, HeroInfoCfgData> onSwitchHeroChild;

    private List<HeroMinItem> heroItemList = new List<HeroMinItem>();
    //当前选中的英雄
    private HeroMinItem currSelectedHeroItem = null;
    //左边菜单列表
    private List<MenuTabItem> leftTabItems = new List<MenuTabItem>();
    //当前选中的TAB
    public MenuTabItem oldSelectedTab = null;
    //当前选中的TAB
    public MenuTabItem currSelectedTab = null;
    //列表
    private TableView tableView = null;
    // 是否切换了tab
    private bool isSwitchedTab = false;
    private bool bOpenAnim = false;

    public HeroLeftMenu(Transform parent, float duration, Action<TabType, TabType> onSwitchTab = null, Action<HeroData, HeroInfoCfgData> onSwitchHero = null)
    {
        this.duration = duration;
        this.leftMenu = parent;
        heroMenu = parent.Find("tab/menu");
        tabItem = heroMenu.transform.Find("tabItem").gameObject;

        heroItem = parent.Find("listhero/content/heroitem").gameObject;
        heroList = parent.Find("listhero").GetComponent<Image>();

        this.OnSwitchTabType = onSwitchTab;
        this.onSwitchHero = onSwitchHero;
    }
    /// <summary>
    /// 当前选择的英雄
    /// </summary>
    /// <returns></returns>
    public HeroMinItem getSelectHero()
    {
        return currSelectedHeroItem;
    }
    /// <summary>
    /// 当前选择的TAB
    /// </summary>
    /// <returns></returns>
    public MenuTabItem getSelectTab()
    {
        return currSelectedTab;
    }
    /// <summary>
    /// 上一个选择的TAB
    /// </summary>
    /// <returns></returns>
    public MenuTabItem getPreSelectTab()
    {
        return oldSelectedTab;
    }
    public void hide()
    {
        leftMenu.GetComponent<RectTransform>().DOAnchorPosX(-600, duration);
    }
    public void show()
    {
        leftMenu.GetComponent<RectTransform>().DOAnchorPosX(0, duration);
    }
    public void init()
    {
        InitHeroDatas();
        InstantHeroItems();
        //选中英雄
        if (heroItemList.Count > 0 && currSelectedHeroItem == null)
        {
            if (GameCenter.mIns.heroGrowCfg.selectHero > 0)
            {
                foreach (var item in heroItemList)
                {
                    if (item.data.heroID == GameCenter.mIns.heroGrowCfg.selectHero)
                    {
                        OnSelectItem(item);
                        break;
                    }
                }
            }
            else
            {
               

                OnSelectItem(heroItemList[0]);
            }
        }

        InstantLeftTabs();
    }
    public void InstantLeftTabs()
    {
        leftTabItems = new List<MenuTabItem>();
        GameObject ori = tabItem.gameObject;
        ori.SetActive(false);
        for (int i = 0; i <= 4; i++)
        {
            GameObject cloneObj = GameObject.Instantiate(ori);

            MenuTabItem leftTabItem = new MenuTabItem(ori.transform.parent, cloneObj, i, (index) =>
            {
                GameCenter.mIns.heroGrowCfg.selectTab = index;
                OnSwitchTab(index);
            });
            leftTabItems.Add(leftTabItem);
        }

        OnSwitchTab(GameCenter.mIns.heroGrowCfg.selectTab, true);
    }

    private void OnSwitchTab(int index, bool isInit = false)
    {

        if (currSelectedTab != null && currSelectedTab.index == index)
        {
            return;
        }
        for (int i = 0; i <= leftTabItems.Count - 1; i++)
        {
            leftTabItems[i].SetImgSelectedState(index == i, (repeatClick) =>
            {
                if (!repeatClick)
                {
                    OnSwitchTabType?.Invoke(currSelectedTab == null ? TabType.Info : (TabType)currSelectedTab.index, (TabType)index);
                }
            });
        }
        currSelectedTab = leftTabItems[index];
        /*        if (tableView != null && !isInit)
                {
                    tableView.ScrollToIndex(1, 1, 1);
                }*/

        isSwitchedTab = true;

    }

    private void InitHeroDatas()
    {
        Dictionary<long, HeroData> heros = HeroDataManager.Instance.HeroList;

        if (heros == null || heros.Count < 1)
        {
            return;
        }

        foreach (var hero in heros)
        {
            HeroMinItem item = new HeroMinItem(hero.Value);
            heroItemList.Add(item);
        }
    }

    /// <summary>
    /// 初始化英雄列表items
    /// </summary>
    private void InstantHeroItems()
    {
        tableView = heroList.transform.GetComponentInChildren<TableView>();
        if (tableView == null)
        {
            zxlogger.logerrorformat("Error: ware house tableview is null!");
            return;
        }
        tableView.onItemRender = OnItemRender;
        tableView.onItemDispose = OnItemDispose;
        tableView.SetDatas(heroItemList.Count, false);
    }

    /// <summary>
    /// 当一个item渲染
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="index"></param>
    private void OnItemRender(GameObject obj, int index)
    {
        HeroMinItem heroMinItem = heroItemList[index - 1];

        //Debug.Log($"<color=#ff0000>index:{index} \n heroID:{heroMinItem.data.heroID}</color>");

        if (heroMinItem != null)
        {
            if (obj.name == heroMinItem.data.heroID.ToString())
                return;

            obj.name = heroMinItem.data.heroID.ToString();
            heroMinItem.BindObj(obj.transform.parent, obj, index, (heroitem) =>
            {
                if (heroitem != null)
                {
                    OnSelectItem(heroitem);
                }
            }, bOpenAnim);

            if (currSelectedHeroItem != null && currSelectedHeroItem.data.heroID == heroMinItem.data.heroID)
            {
                OnSelectItem(heroMinItem);
            }
        }
    }

    private void OnItemDispose()
    {

    }
    public void setChangeHeroSelect(Action<HeroData, HeroInfoCfgData> changeHeroSelectAction)
    {
        this.onSwitchHeroChild = changeHeroSelectAction;
    }

    private void OnSelectItem(HeroMinItem selectedHeroItem)
    {
        if (selectedHeroItem == null )
        {
            return;
        }

        if (currSelectedHeroItem != null)
        {
            currSelectedHeroItem.DoOnClickEffect(false);
        }
        currSelectedHeroItem = selectedHeroItem;
        GameCenter.mIns.heroGrowCfg.selectHero = selectedHeroItem.data.heroID;

        currSelectedHeroItem.DoOnClickEffect(true);
        
        onSwitchHeroChild?.Invoke(selectedHeroItem.data, selectedHeroItem.heroInfo);
        onSwitchHero?.Invoke(selectedHeroItem.data, selectedHeroItem.heroInfo);
    }
    public void clearUI()
    {
        tableView = null;
        currSelectedHeroItem = null;
        heroItemList.Clear();

        foreach (var item in leftTabItems)
        {
            item.OnDestroy();
        }

        leftTabItems.Clear();
        currSelectedTab = null;
        isSwitchedTab = false;

    }
    public class MenuTabItem
    {
        private Transform parent;
        private Transform trans;
        public int index;
        private Action<int> clickCallback;
        public Button btnClick;
        public GameObject imgSelected;
        public Image imgIcon;
        public TextMeshProUGUI txTab;

        public MenuTabItem() { }
        public MenuTabItem(Transform parent, GameObject obj, int index, Action<int> clickCallback)
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
            txTab = trans.GetChild(0).Find("txName").GetComponent<TextMeshProUGUI>();

            obj.transform.SetParent(parent);
            /**/
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.SetActive(true);

            btnClick.onClick.AddListener(OnClick);
            SetStely(false);

            LoadEffect();
        }

        private void SetStely(bool isClick)
        {
            string iconResName = "";
            string tx = "";
            switch ((TabType)index)
            {
                case TabType.Info:
                default:
                    iconResName = isClick ? "ui_d_icon_xiangqing_bai" : "ui_d_icon_xiangqing_hei";
                    tx = GameCenter.mIns.m_LanMgr.GetLan("grow_leftMenu_1");
                    break;
                case TabType.Weapon:
                    iconResName = isClick ? "ui_d_icon_wuqi_bai" : "ui_d_icon_wuqi_hei";
                    tx = GameCenter.mIns.m_LanMgr.GetLan("grow_leftMenu_2");
                    break;
                case TabType.Skill:
                    iconResName = isClick ? "ui_d_icon_jinneg_bai" : "ui_d_icon_jinneg_hei";
                    tx = GameCenter.mIns.m_LanMgr.GetLan("grow_leftMenu_3");
                    break;
                case TabType.Talent:
                    iconResName = isClick ? "ui_d_icon_tianfu_bai" : "ui_d_icon_tianfu_hei";
                    tx = GameCenter.mIns.m_LanMgr.GetLan("grow_leftMenu_4");
                    break;
                case TabType.Profile:
                    iconResName = isClick ? "ui_d_icon_ziliao_bai" : "ui_d_icon_ziliao_hei";
                    tx = GameCenter.mIns.m_LanMgr.GetLan("grow_leftMenu_5");
                    break;
            }

            imgIcon.sprite = SpriteManager.Instance.GetSpriteSync(iconResName);

            txTab.text = tx;
            txTab.color = isClick ? Color.white : Color.black;

            //txTab.GetComponent<RectTransform>().anchoredPosition = isClick ? new Vector2(45f, 3f) : new Vector2(22.5f, 3f);
        }


        /// <summary>
        /// 点击
        /// </summary>
        public void OnClick()
        {
            if (imgSelected.activeSelf)
                return;

            //SetImgSelectedState(true, null);
            clickCallback?.Invoke(index);
            SetStely(true);
            WarehouseManager.Instance.currSelectItemTabType = (ItemType)index;

            effect.Play();
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

        UIParticleEffect effect;

        private void LoadEffect()
        {
            UIEffectManager.Instance.LoadUIEffect("effs_ui_btn_01_liuguang", (go) =>
            {
                effect = go.AddComponent<UIParticleEffect>();

                effect.SetUP(new UIParticleEffect.ShowInfo()
                {
                    _offset = 1,

                    _canvas = trans.GetComponentInParent<Canvas>(),
                });
                go.transform.SetParent(imgSelected.transform, false);

                go.transform.localPosition = new Vector3(-102.5f, -30);
            });
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




}
public enum TabType
{
    // 详情
    Info = 0,
    // 武器
    Weapon = 1,
    // 技能
    Skill = 2,
    // 天赋
    Talent = 3,
    // 资料
    Profile = 4
}

