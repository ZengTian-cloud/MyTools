using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static HeroGrow;
using LitJson;

/// <summary>
/// 英雄武器信息界面
/// </summary>
public class HeroWeaponInfoPanel
{
    private TextMeshProUGUI name;//武器名字
    private TextMeshProUGUI type;//武器类型

    private GameObject starRoot;
    private List<GameObject> starBox;//星级
    private Transform basegrowTr;
    private GameObject argitem;
    private Slider expSlider;//经验条
    //private Image to;//

    private TextMeshProUGUI levelText;
    private TextMeshProUGUI level;

    private TextMeshProUGUI attrTitle;
    private Image attrIcon;

    private TextMeshProUGUI texingTitle;

    private TextMeshProUGUI texingName;
    private TextMeshProUGUI texingDesc;

    private Button btn_wear;
    private Button btn_cultivate;

    private GameObject jiexi;
    private TextMeshProUGUI expend_text;
    private GameObject expendList;
    private GameObject expend_item;
    private Button btn_jiexi;


    private WeaponDataCfg weaponData;
    private HeroLeftMenu leftMenu;
    private float duration;


    public HeroData hero;
    public HeroInfoCfgData heroInfo;
    public HeroWeaponPanel weaponPanel;
    private Transform Root;
    private bool bLock;
    private List<GameObject> expendItems = new List<GameObject>();//升级消耗材料的item预制体列表

    private HeroWeaponPeiyangPanel peiyangPanel;
    private List<WeaponStstsItem> statsItemList = new List<WeaponStstsItem>();

    public HeroWeaponInfoPanel(Transform parent, HeroLeftMenu leftMenu, float duration, HeroWeaponPanel weaponPanel)
    {
        this.Root = parent;
        this.leftMenu = leftMenu;
        this.duration = duration;
        //最上级武器界面
        this.weaponPanel = weaponPanel;

        name = parent.Find("name/nametext").GetComponent<TextMeshProUGUI>();
        type = parent.Find("name/type").GetComponent<TextMeshProUGUI>();

        starRoot = parent.Find("starbox").gameObject;
        starBox = new List<GameObject>();

        for (int i = 0; i < starRoot.transform.childCount; i++)
        {
            starBox.Add(starRoot.transform.GetChild(i).gameObject);
        }

        expSlider = parent.Find("expSlider").GetComponent<Slider>();
        levelText = parent.Find("bg_level/text").GetComponent<TextMeshProUGUI>();
        levelText.text = GameCenter.mIns.m_LanMgr.GetLan("common_level");
        level = parent.Find("bg_level/level").GetComponent<TextMeshProUGUI>();
        basegrowTr = parent.Find("basegrow");
        argitem = parent.Find("basegrow/argitem").gameObject;
        attrTitle = parent.Find("weapon_attr/title").GetComponent<TextMeshProUGUI>();
        attrTitle.text = GameCenter.mIns.m_LanMgr.GetLan("grow_jiexi_level");
        attrIcon = parent.Find("weapon_attr/icon").GetComponent<Image>();

        texingTitle = parent.Find("bg_title/text").GetComponent<TextMeshProUGUI>();
        texingTitle.text = GameCenter.mIns.m_LanMgr.GetLan("grow_weapon_texing");
        texingName = parent.Find("weapon_info/name").GetComponent<TextMeshProUGUI>();
        texingDesc = parent.Find("weapon_info/desc").GetComponent<TextMeshProUGUI>();

        btn_wear = parent.Find("btn_wear").GetComponent<Button>();
        btn_wear.transform.Find("text").GetComponent<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("grow_wear");
        btn_cultivate = parent.Find("btn_cultivate").GetComponent<Button>();
        btn_cultivate.transform.Find("text").GetComponent<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("grow_cultivate");

        jiexi = parent.Find("jiexi").gameObject;
        expend_text = jiexi.transform.Find("expend_bg/text").GetComponent<TextMeshProUGUI>();
        expendList = jiexi.transform.Find("expendList").gameObject;
        expend_item = expendList.transform.Find("expend_item").gameObject;
        btn_jiexi = jiexi.transform.Find("btn_jiexi").GetComponent<Button>();
        btn_jiexi.transform.Find("text").GetComponent<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("grow_jiexi");
        expend_item.SetActive(false);
        btn_wear.AddListenerBeforeClear(OnBtnWearClick);
        btn_cultivate.AddListenerBeforeClear(OnCultivateClick);
        btn_jiexi.AddListenerBeforeClear(OnBtnJiexiClick);

        
    }

    public void show(WeaponDataCfg weaponData, HeroLeftMenu leftMenu, bool bLock,bool isAnimations)
    {
        this.weaponData = weaponData;
        this.bLock = bLock;
        this.leftMenu = leftMenu;
        this.hero = leftMenu.getSelectHero().data;
        this.heroInfo = leftMenu.getSelectHero().heroInfo;

        refresh();
        //this.Root.gameObject.SetActive(true);

        if (isAnimations)
            this.Root.GetComponent<RectTransform>().DOAnchorPosX(-350, duration, false);
        else
            this.Root.GetComponent<RectTransform>().anchoredPosition = new Vector3(-350, 0);
        this.Root.gameObject.SetActive(true);

    }

    /// <summary>
    /// 刷新界面
    /// </summary>
    /// <param name="weaponData">选中的武器数据</param>
    /// <param name="leftMenu"></param>
    /// <param name="bLock">是否解锁</param>
    public void refresh()
    {
        //切换武器类型回调
        weaponPanel.SetTypeSwitchCallBack(() =>
        {
            int curType = HeroGrowUtils.getHeroWeaponSelectedType(hero.heroID, weaponPanel.allWeapon, hero.weaponid);
            weaponPanel.showInfoPanel(weaponPanel.allWeapon[curType].weaponid);
        });


        Root.GetComponent<RectTransform>().DOAnchorPosX(-350, duration);
        RefreshPanel();
    }

    public void close()
    {
        Root.GetComponent<RectTransform>().DOAnchorPosX(400, duration);
        weaponPanel.SetTypeSwitchCallBack(null);
    }

   
    private void RefreshPanel()
    {

        name.text = GameCenter.mIns.m_LanMgr.GetLan(weaponData.name);
        type.text = GameCenter.mIns.m_LanMgr.GetLan($"grow_type_{weaponData.type}");

        int star = hero.GetWeaponStar(weaponData.weaponid);
        if (star == -1) {
            attrIcon.gameObject.SetActive(false);
        }else
        { 
            attrIcon.sprite = SpriteManager.Instance.GetSpriteSync(HeroGrowUtils.getWeaponStarIcon(star));
            attrIcon.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan($"grow_shendujiexi_{star}");
            attrIcon.gameObject.SetActive(true);
        }
        for (int i = 0; i < starBox.Count; i++)
        {
            starBox[i].transform.Find("c").gameObject.SetActive(hero.weaponstate >= i + 1);
        }

        //当前等级所需总经验
        float total = heroInfo.getCurrWeaponLevelData(hero.weaponLevel, hero.weaponstate).weaponexp;
        expSlider.value = hero.weaponexp / total;
        int curMaxlv = heroInfo.getCurrWeaponBreakData(hero.weaponstate).level;
        level.text = $"{hero.weaponLevel}/{curMaxlv}";

        texingName.text = "暂无";
        texingDesc.text = GameCenter.mIns.m_LanMgr.GetLan(weaponData.note);

        //加载属性界面
        GameObject ori = argitem.gameObject;
        ori.SetActive(false);
        clearStstsItem();
        WeaponAttrMode mode = weaponData.getWeaponAttr(hero.weaponLevel, hero.weaponstate);
        List<StatData> list = mode.attrs;
        for (int i = 0; i < list.Count; i++)
        {
            GameObject cloneObj = GameObject.Instantiate(ori);

            WeaponStstsItem item = new WeaponStstsItem(basegrowTr, list[i], this.hero, weaponData, i == list.Count - 1, cloneObj, i, true);
            statsItemList.Add(item);
        }

        btn_wear.gameObject.SetActive(bLock);
        btn_cultivate.gameObject.SetActive(bLock);
        jiexi.SetActive(!bLock);
        if (bLock)//解锁
        {
            btn_wear.interactable = hero.weaponid != weaponData.weaponid;
            commontool.SetGary(btn_wear.GetComponent<Image>(), hero.weaponid == weaponData.weaponid);
        }
        else//未解锁 显示解析界面
        {
            bool canJiexi = true;
            if (this.weaponData.cost > 0)
            {
                List<CostData> costDatas = GameCenter.mIns.m_CfgMgr.GetCostByCostID(this.weaponData.cost).getCosts();
                if (costDatas.Count > 0)
                {
                    for (int i = 0; i < expendItems.Count; i++)
                    {
                        expendItems[i].SetActive(false);
                    }
                    for (int i = 0; i < costDatas.Count; i++)
                    {
                        GameObject item = null;
                        if (expendItems.Count > i + 1 && expendItems[i] != null)
                        {
                            item = expendItems[i];
                        }
                        else
                        {
                            item = GameObject.Instantiate(expend_item, expendList.transform);
                            expendItems.Add(item);
                        }
                        item.SetActive(true);
                        int curNum = GameCenter.mIns.userInfo.getPropCount(costDatas[i].propid);
                        item.transform.Find("bg_count/text_count").GetComponent<TextMeshProUGUI>().text = HeroGrowUtils.parsePropCountStr( costDatas[i].count, curNum);
                        if (curNum < costDatas[i].count)
                        {
                            canJiexi = false;
                        }
                    }
                    
                }
            }
            btn_jiexi.interactable = canJiexi;
            commontool.SetGary(btn_jiexi.GetComponent<Image>(), !canJiexi);
        }
    }

    /// <summary>
    /// 穿戴武器
    /// </summary>
    private void OnBtnWearClick()
    {
        //btn_wear.interactable = false;
        //commontool.SetGary(btn_wear.GetComponent<Image>(),true);
        JsonData data = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        data["heroid"] = this.hero.heroID;
        data["weaponid"] = this.weaponData.weaponid;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.GROW_HERO_SWITCHWEAPON, data, (seqid, code, data) =>
        {
            if (code == 0)
            {
                JsonData json = JsonMapper.ToObject(new JsonReader(data));
                JsonData chagedata = json["change"]?["changed"];
                if (chagedata != null)
                {
                    GameCenter.mIns.userInfo.onChange(chagedata);
                    GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                    {
                        weaponPanel.Refresh();
                    });
                }
            }
        });
    }

    /// <summary>
    /// 解析武器
    /// </summary>
    private void OnBtnJiexiClick()
    {
        JsonData data = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        data["heroid"] = this.hero.heroID;
        data["weaponid"] = this.weaponData.weaponid;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.GROW_HERO_JIEXIAPON, data, (seqid, code, data) =>
        {
            if (code == 0)
            {
                JsonData json = JsonMapper.ToObject(new JsonReader(data));
                JsonData chagedata = json["change"]?["changed"];
                if (chagedata != null)
                {
                    GameCenter.mIns.userInfo.onChange(chagedata);
                    GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                    {
                        weaponPanel.Refresh();
                    });
                }
            }
        });
    }

    /// <summary>
    /// 培养
    /// </summary>
    private void OnCultivateClick()
    {
        this.close();
        weaponPanel.ShowPeiyangpanel(weaponData.weaponid);
    }

    public void clearStstsItem()
    {
        foreach (var item in statsItemList)
        {
            item.OnDestroy();
        }
        statsItemList.Clear();
    }
    public void OnDestroy()
    {
        clearStstsItem();
    }
}

