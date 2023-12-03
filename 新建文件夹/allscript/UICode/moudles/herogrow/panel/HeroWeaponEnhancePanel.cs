using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static HeroGrow;
using LitJson;
using System.Collections.Generic;
using System;
//using static UnityEditor.Progress;

/// <summary>
/// 强化界面
/// </summary>
public class HeroWeaponEnhancePanel
{

    private Transform root;
    public HeroData hero;
    public HeroInfoCfgData heroInfo;
    private WeaponDataCfg weaponData;
    private float duration;

    private TextMeshProUGUI name;//武器名字
    private TextMeshProUGUI type;//武器类型

    private Slider lvExp;
    private Image lvExpTo;
    private Transform propPanelSj;
    private GameObject argitem;
    private Transform cailiaos;
    private TextMeshProUGUI coinCount;
    private TextMeshProUGUI coinLabel;
    private Button btnUp;
    private TextMeshProUGUI btnTxt;
    private Transform clTitleTr;
    private Button btnAiZr;
    private Button btnClear;

    private TextMeshProUGUI levelText;
    private TextMeshProUGUI levelTxt;
    private TextMeshProUGUI expTxt;
    private TextMeshProUGUI levelAddText;
    private TextMeshProUGUI expAddText;

    private int oldlevel = 0;
    private int addLevelCount = 0;
    private List<WeaponStstsItem> statsItemList = new List<WeaponStstsItem>();
    private WeaponUpgradesProp weaponUpgradesProp;
    private Action<BackType> onBack;

    public HeroWeaponEnhancePanel(Transform parent, float duration, Action<BackType> onBack)
    {
        this.root = parent;
        this.duration = duration;
        this.onBack = onBack;

        name = parent.Find("name/nametext").GetComponent<TextMeshProUGUI>();
        type = parent.Find("name/type").GetComponent<TextMeshProUGUI>();

        levelText = parent.Find("bg_level/text").GetComponent<TextMeshProUGUI>();
        levelText.text = GameCenter.mIns.m_LanMgr.GetLan("common_level");
        levelTxt = parent.Find("bg_level/level").GetComponent<TextMeshProUGUI>();
        expTxt = parent.Find("bg_level/exp").GetComponent<TextMeshProUGUI>();
        levelAddText = parent.Find("bg_level/levelAdd").GetComponent<TextMeshProUGUI>();
        expAddText = parent.Find("bg_level/expAdd").GetComponent<TextMeshProUGUI>();

        lvExp = parent.Find("expSlider").GetComponent<Slider>();
        lvExpTo = lvExp.transform.Find("to").GetComponent<Image>();
        propPanelSj = parent.Find("basegrow");
        argitem = parent.Find("basegrow/argitem").gameObject;
        cailiaos = parent.Find("cailiaos");
        coinCount = parent.Find("btnbox/coinCount").GetComponent<TextMeshProUGUI>();
        btnUp = parent.Find("btnbox/btn").GetComponent<Button>();
        btnTxt = btnUp.transform.Find("txt").GetComponent<TextMeshProUGUI>();
        clTitleTr = parent.Find("bg_title");
        coinLabel = parent.Find("btnbox/txt").GetComponent<TextMeshProUGUI>();
        btnAiZr = parent.Find("bg_title/btns/zhiru").GetComponent<Button>();
        btnClear = parent.Find("bg_title/btns/clear").GetComponent<Button>();

        btnUp.onClick.RemoveAllListeners();
        btnUp.onClick.AddListener(OnUpLevelClick);
        btnAiZr.onClick.RemoveAllListeners();
        btnAiZr.onClick.AddListener(onBtnAi);
        btnClear.onClick.RemoveAllListeners();
        btnClear.onClick.AddListener(onBtnClear);
        btnClear.gameObject.SetActive(false);
    }

    /// <summary>
    /// 自动置入
    /// </summary>
    public void onBtnAi()
    {
        weaponUpgradesProp.onAI();
    }
    /// <summary>
    /// 清空所有
    /// </summary>
    public void onBtnClear()
    {
        weaponUpgradesProp.clearProp();
    }

    public void show(WeaponDataCfg weaponData, HeroData hero, HeroInfoCfgData heroInfo, bool isblock,  bool isAnimations)
    {
        this.hero = hero;
        this.heroInfo = heroInfo;
        this.weaponData = weaponData;
        HeroGrowUtils.backType = BackType.WqSj;

        if (isAnimations)
            root.GetComponent<RectTransform>().DOAnchorPosX(-350, duration);
        else
            root.GetComponent<RectTransform>().anchoredPosition = new Vector3(-350, 0);
        root.gameObject.SetActive(true);

        loadData(weaponData);
    }

    public void loadData(WeaponDataCfg weaponData)
    {

        oldlevel = hero.weaponLevel;
        //初始化升级道具
        if (weaponUpgradesProp == null) weaponUpgradesProp = new WeaponUpgradesProp(cailiaos, weaponData, hero, (level, exp, scale, addExp, isChangeStatsInfo) => {
            onCountChange(weaponData, level, exp, scale, addExp, isChangeStatsInfo);
        });

        //加载属性界面
        GameObject ori = argitem.gameObject;
        ori.SetActive(false);

        clearStstsItem();
        WeaponAttrMode mode = weaponData.getWeaponAttr(hero.weaponLevel,hero.weaponstate);
        List<StatData> list = mode.attrs;
        for (int i = 0; i < list.Count; i++)
        {
            GameObject cloneObj = GameObject.Instantiate(ori);

            WeaponStstsItem item = new WeaponStstsItem(propPanelSj, list[i], this.hero, weaponData, i == list.Count - 1, cloneObj, i, true);
            statsItemList.Add(item);
        }

        initUI(false);
    }

    private void initUI(bool isSetCount)
    {
        WeaponUpCostCfg levelData = heroInfo.getCurrWeaponLevelData(hero.weaponLevel,hero.weaponstate);
        name.text = GameCenter.mIns.m_LanMgr.GetLan(weaponData.name);
        type.text = GameCenter.mIns.m_LanMgr.GetLan($"grow_type_{weaponData.type}");
        levelTxt.text = hero.weaponLevel.ToString();
        expTxt.text = levelData.weaponexp == -1 ? "MAX" :  $"{hero.weaponexp}/{levelData.weaponexp}";

        float scale = float.Parse(this.hero.weaponexp + "") / float.Parse(levelData.weaponexp + "");
        scale = (float)Math.Round(scale, 2);
        if (scale > 1) scale = 1;

        levelAddText.gameObject.SetActive(false);
        expAddText.gameObject.SetActive(false);

        lvExpTo.fillAmount = 0;//重置进度条
        coinCount.text = "0";
        
        //已经升到了最高级
        if (levelData.weaponexp == -1 && levelData.state >= 6)
        {
            cailiaos.gameObject.SetActive(false);
            clTitleTr.gameObject.SetActive(false);
            coinCount.gameObject.SetActive(false);
            coinLabel.gameObject.SetActive(false);
            btnUp.interactable = false;
            btnUp.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_hs_jy");
            btnTxt.text = GameCenter.mIns.m_LanMgr.GetLan("grow_WeaponEnhancePanel_1");
        }
        else {
            cailiaos.gameObject.SetActive(true);
            clTitleTr.gameObject.SetActive(true);
            coinCount.gameObject.SetActive(true);
            coinLabel.gameObject.SetActive(true);
            btnClear.gameObject.SetActive(false);
            btnAiZr.gameObject.SetActive(true);
            btnUp.interactable = true;
            btnUp.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_cs");
            btnTxt.text = GameCenter.mIns.m_LanMgr.GetLan("grow_WeaponEnhancePanel_2");
        }
        weaponUpgradesProp.reset();

        if (oldlevel == hero.weaponLevel)
        {
            lvExp.DOValue(scale, duration);
        }
        else
        {
            lvExp.DOValue(1, duration).OnComplete(() =>
            {
                lvExp.value = 0;
                int anCount = addLevelCount > 1 ? addLevelCount - 1 : 1;
                lvExp.DOValue(1, anCount < 10 ? duration : 0.15f).SetLoops(anCount).OnComplete(() => {
                    lvExp.value = 0;
                    lvExp.DOValue(scale, duration).OnComplete(() => {
                        if (HeroGrowUtils.backType != BackType.WqSj)
                        {
                            return;
                        }
                        WeaponUpCostCfg levelData = heroInfo.getCurrWeaponLevelData(hero.weaponLevel, hero.weaponstate);

                        if (isSetCount)
                        {
                            foreach (var item in statsItemList)
                            {
                                item.setCount(item.data.val);
                                item.hideCountGo();
                            }
                        }
                        if (levelData.weaponexp == -1 && levelData.state >= 6)
                        {
                            cailiaos.gameObject.SetActive(false);
                            clTitleTr.gameObject.SetActive(false);
                            coinCount.gameObject.SetActive(false);
                            coinLabel.gameObject.SetActive(false);
                            btnUp.interactable = false;
                            btnUp.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_hs_jy");
                            btnTxt.text = GameCenter.mIns.m_LanMgr.GetLan("grow_WeaponEnhancePanel_1");
                        }
                        else if (levelData.weaponexp == -1 && HeroGrowUtils.backType == BackType.WqSj)
                        {
                            this.hide(false);
                            //可以突破了，跳转到突破界面
                            onBack?.Invoke(BackType.WqSj);
                        }
                    });
                });
            });

        }
    }

    public void onCountChange(WeaponDataCfg weaponData, int level, int exp, float scale, int addExp, bool isChangeStatsInfo)
    {
        if (level != oldlevel)
        {
            int addLevel = level - hero.weaponLevel;
            if (addLevel > 0)
            {
                levelAddText.text = $"+{addLevel}" ;
                levelAddText.gameObject.SetActive(true);
                if (isChangeStatsInfo)
                {
                    WeaponAttrMode mode = weaponData.getWeaponAttr(level, hero.weaponstate);
                    List<StatData> list = mode.attrs;
                    foreach (var item in statsItemList)
                    {
                        foreach (var stat in list)
                        {
                            if (stat.statid == item.ststsId)
                            {
                                item.showCountGo(stat.val);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                levelAddText.gameObject.SetActive(false);
                WeaponUpCostCfg levelData = heroInfo.getCurrWeaponLevelData(hero.weaponLevel, hero.weaponstate);
                expTxt.text = levelData.weaponexp == -1 ? "MAX" :   $"{exp}/{levelData.weaponexp}";
                foreach (var item in statsItemList)
                {
                    item.hideCountGo();
                }
            }
            oldlevel = level;
        }

        WeaponUpCostCfg newLevelData = heroInfo.getCurrWeaponLevelData(level, hero.weaponstate);
        //设置金币数量 
        int coin = weaponUpgradesProp.totalCoin;
        int userCoin = GameCenter.mIns.userInfo.Coin;
        coinCount.text = HeroGrowUtils.setCoinCountUI(coin, userCoin);
        if (addExp > 0)
        {
            scale = newLevelData.weaponexp == -1 ? 1 : scale;
            expTxt.text = newLevelData.weaponexp == -1 ? "MAX" : $"{exp}/{newLevelData.weaponexp}";
            expAddText.text = "+" + addExp + "";
            expAddText.gameObject.SetActive(true);

            scale = level > hero.weaponLevel ? 1 : scale;
            lvExpTo.DOFillAmount(scale, duration);
            btnClear.gameObject.SetActive(true);
            btnAiZr.gameObject.SetActive(newLevelData.weaponexp != -1);

            btnUp.interactable = (userCoin >= coin);
            btnUp.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(userCoin >= coin ? "ui_c_btn_cs" : "ui_c_btn_hs_jy");
            btnTxt.text = userCoin >= coin ? GameCenter.mIns.m_LanMgr.GetLan("grow_WeaponEnhancePanel_2") : GameCenter.mIns.m_LanMgr.GetLan("grow_WeaponEnhancePanel_3");
        }
        else
        {
            expTxt.text =
            expTxt.text = newLevelData.weaponexp == -1 ? "MAX" :  $"{hero.weaponexp}/{newLevelData.weaponexp}";
            lvExpTo.DOFillAmount(0, duration);
            expAddText.gameObject.SetActive(false);
            btnClear.gameObject.SetActive(false);
            btnAiZr.gameObject.SetActive(true);

            btnUp.interactable =false;
            btnUp.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync( "ui_c_btn_hs_jy");
            btnTxt.text=GameCenter.mIns.m_LanMgr.GetLan("grow_WeaponEnhancePanel_4");
        }    
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

    private Tween sliderDot;

    public void OnUpLevelClick()
    {
        if (weaponUpgradesProp.totalExp == 0)
        {
            //请放入经验书
            GameCenter.mIns.m_UIMgr.PopMsg("请放入经验书");
            return;
        }

        int coin = weaponUpgradesProp.totalCoin;
        int userCoin = GameCenter.mIns.userInfo.Coin;
        if (coin > userCoin)
        {
            //金币不足
            GameCenter.mIns.m_UIMgr.PopMsg("金币不足");
            return;
        }
        JsonData data = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        data["heroid"] = this.hero.heroID;
        weaponUpgradesProp.getPropCounts(data);

        GameCenter.mIns.m_NetMgr.SendData(NetCfg.GROW_HERO_WEAPONLEVELUP, data, (seqid, code, data) =>
        {
            if (code == 0)
            {
                addLevelCount = weaponUpgradesProp.tolevel - hero.weaponLevel;
                JsonData json = JsonMapper.ToObject(new JsonReader(data));
                JsonData chagedata = json["change"]?["changed"];
                if (chagedata != null)
                {
                    oldlevel = hero.weaponLevel;
                    GameCenter.mIns.userInfo.onChange(chagedata);
                    GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                    {
                        initUI(true);
                        oldlevel = hero.weaponLevel;
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
    public void OnDestroy()
    {
        if (weaponUpgradesProp != null)
        {
            weaponUpgradesProp.reset();
            weaponUpgradesProp.OnDestroy();
            weaponUpgradesProp = null;
        }
        clearStstsItem();
    }

}

