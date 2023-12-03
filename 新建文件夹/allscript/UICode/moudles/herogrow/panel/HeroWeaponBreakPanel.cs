
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static HeroGrow;
using LitJson;

/// <summary>
/// 武器升星界面
/// </summary>
public class HeroWeaponBreakPanel
{
    private HeroWeaponPanel heroWeaponPanel;

    private Transform root;
    public HeroData hero;
    public HeroInfoCfgData heroInfo;
    private WeaponDataCfg weaponData;
    private float duration;
    private bool block;

    private TextMeshProUGUI nameTxt;//武器名字
    private TextMeshProUGUI typeTxt;//武器类型
    private Transform starTpBox;
    private Transform starGoTpBox;
    private Transform propPanelTp;
    private GameObject argItem;
    private TextMeshProUGUI levelTxt;
    private TextMeshProUGUI nextLevelTxt;
    private GameObject propitem;
    private Transform cailiaos;
    private Button btnBreak;
    private Image btnBreakImg;
    private TextMeshProUGUI btnTxt;

    private Action<BackType> onBack;

    private List<WeaponStstsItem> statsItemList = new List<WeaponStstsItem>();
    private List<HeroPropItem> breakProps = new List<HeroPropItem>();

    public HeroWeaponBreakPanel(Transform parent,  float duration, HeroWeaponPanel weaponPanel)
    {
        this.root = parent;
        this.duration = duration;
        this.heroWeaponPanel = weaponPanel;

        nameTxt = parent.Find("name/nametext").GetComponent<TextMeshProUGUI>();
        typeTxt = parent.Find("name/type").GetComponent<TextMeshProUGUI>();
        starTpBox = parent.transform.Find("starpanel/star/starbox");
        starGoTpBox = parent.transform.Find("starpanel/star_tupo/starbox");
        propPanelTp = parent.transform.Find("basegrow");
        argItem = parent.transform.Find("basegrow/argitem").gameObject;
        levelTxt = parent.transform.Find("djsx/sx").gameObject.GetComponent<TextMeshProUGUI>();
        nextLevelTxt = parent.transform.Find("djsx/sx_tp").gameObject.GetComponent<TextMeshProUGUI>();
        cailiaos = parent.Find("cailiaos");
        propitem = cailiaos.Find("propitem").gameObject;
        btnBreak = parent.Find("btnbox/btn").gameObject.GetComponent<Button>();
        btnBreakImg = btnBreak.GetComponent<Image>();
        btnTxt = parent.Find("btnbox/btn/txt").gameObject.GetComponent<TextMeshProUGUI>();
        btnBreak.onClick.RemoveAllListeners();
        btnBreak.onClick.AddListener(OnBreakClick);

    }

    public void show(WeaponDataCfg weaponData, HeroData hero, HeroInfoCfgData heroInfo, bool block,bool isAnimations)
    {
        this.weaponData = weaponData;
        this.block = block;
        this.hero = hero;
        this.heroInfo = heroInfo;
        HeroGrowUtils.backType = BackType.WqTp;

        if (isAnimations)
            root.GetComponent<RectTransform>().DOAnchorPosX(-350, duration);
        else
            root.GetComponent<RectTransform>().anchoredPosition = new Vector3(-350, 0);
        root.gameObject.SetActive(true);
        HeroGrowUtils.setStarUi(hero.weaponstate, false, starTpBox);
        HeroGrowUtils.setStarUi(hero.weaponstate, true, starGoTpBox);
        nameTxt.text = GameCenter.mIns.m_LanMgr.GetLan(weaponData.name);
        typeTxt.text = GameCenter.mIns.m_LanMgr.GetLan($"grow_type_{weaponData.type}");
        levelTxt.text = hero.weaponLevel.ToString();
        nextLevelTxt.text = heroInfo.getCurrWeaponBreakData(hero.weaponstate+1)?.level.ToString();

        //加载属性界面
        GameObject ori = argItem.gameObject;
        ori.SetActive(false);

        //加载参数
        clearStstsItem();
        WeaponAttrMode mode = weaponData.getWeaponAttr(hero.weaponLevel, hero.weaponstate);
        List<StatData> list = mode.attrs;
        for (int i = 0; i < list.Count; i++)
        {
            GameObject cloneObj = GameObject.Instantiate(ori);

            WeaponStstsItem item = new WeaponStstsItem(propPanelTp, list[i], hero,weaponData, i == list.Count - 1, cloneObj, i, true);
            statsItemList.Add(item);
            int val = weaponData.getWeaponAttrStatsVal(hero.weaponLevel, hero.weaponstate+1, list[i].statid);
            item.showCountGo(val);
        }

        //加载材料
        if (breakProps.Count == 0) { 
            WeaponBreakCfg breakdata = heroInfo.getCurrWeaponBreakData(hero.weaponstate);
            GameObject propori = propitem.gameObject;
            propori.SetActive(false);
            foreach (var item in breakdata.costs)
            {
                GameObject cloneObj = GameObject.Instantiate(propori);

                HeroPropItem prop = new HeroPropItem(cailiaos, cloneObj, item);
                breakProps.Add(prop);
            }
        }

        if (propIsCount())
        {
            btnBreak.interactable = true;
            btnBreakImg.sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_cs");
            btnTxt.text = GameCenter.mIns.m_LanMgr.GetLan("grow_WeaponBreakPanel_1");
        }
        else
        {
            btnBreak.interactable = false;
            btnBreakImg.sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_hs_jy");
            btnTxt.text = GameCenter.mIns.m_LanMgr.GetLan("grow_WeaponBreakPanel_2");
        }
    }
    private bool propIsCount()
    {
        foreach (var item in breakProps)
        {
            if (!item.propIsCount()) return false;
        }
        return true;
    }
    public void hide(bool isAnimations)
    {
        if (isAnimations)
            root.GetComponent<RectTransform>().DOAnchorPosX(400, duration);
        else
            root.GetComponent<RectTransform>().anchoredPosition = new Vector3(400, 0);
        root.gameObject.SetActive(false);

        OnDestroy();
    }

    private void OnBreakClick()
    {
        if (!propIsCount())
        {
            //道具不足
            GameCenter.mIns.m_UIMgr.PopMsg(GameCenter.mIns.m_LanMgr.GetLan("grow_WeaponBreakPanel_2"));
            return;
        }

        JsonData data = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        data["heroid"] = this.hero.heroID;
        data["tostate"] = this.hero.weaponstate + 1;

        GameCenter.mIns.m_NetMgr.SendData(NetCfg.GROW_HERO_WEAPONBREAK, data, (seqid, code, data) =>
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
                        this.hide(false);
                        //突破完成 打开升级界面
                        int curType = HeroGrowUtils.getHeroWeaponSelectedType(hero.heroID, heroWeaponPanel.allWeapon, hero.weaponid);
                        heroWeaponPanel.ShowEnhancePanel(heroWeaponPanel.allWeapon[curType].weaponid,false);
                    });
                }
            }
        });
    }
    
    public void clearStstsItem()
    {
        foreach (var item in statsItemList)
        {
            item.OnDestroy();
        }
        statsItemList.Clear();
    }

    public void clearPropsItem()
    {
        foreach (var item in breakProps)
        {
            item.OnDestroy();
        }
        breakProps.Clear();
    }

    public void OnDestroy()
    {
        clearPropsItem();
        clearStstsItem();
    }
}

