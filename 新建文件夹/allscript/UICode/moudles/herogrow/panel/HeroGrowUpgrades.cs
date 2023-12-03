using DG.Tweening;
using LitJson;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class HeroGrowUpgrades
{
    private Transform parent;
    private Transform panelSj;
    private TextMeshProUGUI levelText;
    private TextMeshProUGUI levelAddText;
    private TextMeshProUGUI expAddText;
    private TextMeshProUGUI expText;
    private Slider lvSjExp;
    private Image lvSjExpTo;
    private Transform propPanelSj;
    private GameObject argitem;
    private Transform cailiaos;
    private TextMeshProUGUI coinCount;
    private Button btnUp;
    private TextMeshProUGUI btnTxt;
    private Button btnAiZr;
    private Button btnClear;

    private int oldlevel = 0;
    private int addLevelCount = 0;
    private HeroData hero;
    private HeroInfoCfgData heroInfo;
    private float duration;
    private Action<BackType> onBack;

    private float scaleWidth = 0;
    private List<StstsItem> statsItemList = new List<StstsItem>();
    private HeroUpgradesProp heroUpgradesProp;

    public HeroGrowUpgrades(Transform parent,float duration, Action<BackType> onBack)
    {
        this.parent = parent;
        this.duration = duration;
        this.onBack = onBack;
        panelSj = parent.Find("Panel_shengji");
        levelText = panelSj.Find("levelpanel/level").GetComponent<TextMeshProUGUI>();
        levelAddText = panelSj.Find("levelpanel/levelAdd").GetComponent<TextMeshProUGUI>();
        expAddText = panelSj.Find("levelpanel/expAdd").GetComponent<TextMeshProUGUI>();
        expText = panelSj.Find("levelpanel/exp").GetComponent<TextMeshProUGUI>();
        lvSjExp = panelSj.Find("expSlider").GetComponent<Slider>();
        lvSjExpTo = lvSjExp.transform.Find("to").GetComponent<Image>();
        propPanelSj = panelSj.Find("basegrow");
        argitem = panelSj.Find("basegrow/argitem").gameObject;
        cailiaos = panelSj.Find("cailiaos");
        coinCount = panelSj.Find("btnbox/coinCount").GetComponent<TextMeshProUGUI>();
        btnUp = panelSj.Find("btnbox/btn").GetComponent<Button>();
        btnTxt = btnUp.transform.Find("txt").GetComponent<TextMeshProUGUI>();
        btnAiZr = panelSj.Find("bg_title/btns/zhiru").GetComponent<Button>();
        btnClear = panelSj.Find("bg_title/btns/clear").GetComponent<Button>();

        btnUp.onClick.RemoveAllListeners();
        btnUp.onClick.AddListener(onBtnUpgrades);
        btnAiZr.onClick.RemoveAllListeners();
        btnAiZr.onClick.AddListener(onBtnAi);
        btnClear.onClick.RemoveAllListeners();
        btnClear.onClick.AddListener(onBtnClear);
        btnClear.gameObject.SetActive(false);
        scaleWidth = lvSjExp.GetComponent<RectTransform>().rect.width;
        panelSj.GetComponent<RectTransform>().DOAnchorPosX(700, duration);
    }
    public void onBtnUpgrades()
    {
        if (heroUpgradesProp.totalExp == 0)
        {
            //请放入经验书
            GameCenter.mIns.m_UIMgr.PopMsg("请放入经验书");
            return;
        }

        int coin = heroUpgradesProp.totalCoin;
        int userCoin = GameCenter.mIns.userInfo.Coin;
        if (coin > userCoin)
        {
            //金币不足
            GameCenter.mIns.m_UIMgr.PopMsg("金币不足");
            return;
        }
        JsonData data = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        data["heroid"] = this.hero.heroID;
        heroUpgradesProp.getPropCounts(data);

        GameCenter.mIns.m_NetMgr.SendData(NetCfg.GROW_HERO_UPGRADES, data, (seqid, code, data) =>
        {
            if (code == 0)
            {
                addLevelCount = heroUpgradesProp.tolevel - hero.level;
                JsonData json = JsonMapper.ToObject(new JsonReader(data));
                JsonData chagedata = json["change"]?["changed"];
                if (chagedata != null)
                {
                    oldlevel = hero.level;
                    GameCenter.mIns.userInfo.onChange(chagedata);
                    GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                    {
                        initUI(true);
                        oldlevel = hero.level;
                    });
                }
            }
        });

    }

    /// <summary>
    /// 自动置入
    /// </summary>
    public void onBtnAi()
    {
        heroUpgradesProp.onAI();
    }
    /// <summary>
    /// 清空所有
    /// </summary>
    public void onBtnClear()
    {
        heroUpgradesProp.clearProp();
    }

    public void show(HeroData hero, HeroInfoCfgData heroInfo, bool isAnimations)
    {
        this.hero = hero;
        this.heroInfo = heroInfo;
        oldlevel = hero.level;
        HeroLevelData levelData = heroInfo.getCurrLevelData(hero.level, hero.state);

        if(isAnimations)
            panelSj.GetComponent<RectTransform>().DOAnchorPosX(-350, duration);
        else
            panelSj.GetComponent<RectTransform>().anchoredPosition = new Vector3(-350, 0);
        panelSj.gameObject.SetActive(true);

        loadData(levelData);
        LoadEffect();
    }
    /// <summary>
    /// 道具数据发生变化 ，
    /// </summary>
    /// <param name="levelData">等级信息</param>
    /// <param name="level">等级</param>
    /// <param name="exp">变化的经验</param>
    /// <param name="scale"></param>
    /// <param name="addExp">增加的经验</param>
    /// <param name="isChangeStatsInfo">是否拉取数据预览</param>
    public void onCountChange(HeroLevelData levelData,int level,int exp,float scale,int addExp,bool isChangeStatsInfo)
    {
        if (level != oldlevel)
        {
            int addLevel = level - hero.level;
            if (addLevel > 0)
            {
                levelAddText.text = "+" + addLevel;
                levelAddText.gameObject.SetActive(true);
                if(isChangeStatsInfo)
                { 
                    JsonData data = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
                    data["tolevel"] = level;
                    data["heroid"] = this.hero.heroID;
                    //请求服务器 获得升级后的属性预览
                    GameCenter.mIns.m_NetMgr.SendDataJson(NetCfg.GROW_GET_PROP_GO, data, (seqid, code, data) =>
                    {
                        if (code == 0)
                        {
                            JsonData attrs = data["attrs"];

                            foreach (var item in statsItemList)
                            {
                                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                                {
                                    int? val = attrs[item.data.statid.ToString()]?.ToInt32();
                                    item.showCountGo(val == null ? 0 : val.Value);
                                });
                            }
                        }
                    });
                }
            }
            else
            {
                levelAddText.gameObject.SetActive(false);
                expText.text = levelData.exp == -1 ? "MAX" : exp + "/" + levelData.exp;
                foreach (var item in statsItemList)
                {
                    item.hideCountGo();
                }
            }
            oldlevel = level;
        }

        HeroLevelData newLevelData = heroInfo.getCurrLevelData(level, hero.state);
        //设置金币数量 
        int coin = heroUpgradesProp.totalCoin;
        int userCoin = GameCenter.mIns.userInfo.Coin;
        coinCount.text = HeroGrowUtils.setCoinCountUI(coin, userCoin);
        if (addExp > 0)
        {
            scale = newLevelData.exp == -1 ? 1 : scale;
            expText.text = newLevelData.exp == -1? "MAX": exp + "/" + newLevelData.exp;
            expAddText.text = "+" + addExp + "";
            expAddText.gameObject.SetActive(true);

            scale = level > hero.level ? 1 : scale;
            lvSjExpTo.DOFillAmount(scale,duration);
            btnClear.gameObject.SetActive(true);
            btnAiZr.gameObject.SetActive(newLevelData.exp != -1);

            btnUp.interactable = (userCoin >= coin);
            btnUp.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(userCoin >= coin ? "ui_c_btn_cs" : "ui_c_btn_hs_jy");
            btnTxt.text = userCoin >= coin ? GameCenter.mIns.m_LanMgr.GetLan("grow_Upgrades_1") : GameCenter.mIns.m_LanMgr.GetLan("grow_Upgrades_2");
        }
        else
        {
            expText.text = newLevelData.exp == -1 ? "MAX" : hero.lvexp + "/" + newLevelData.exp;
            lvSjExpTo.DOFillAmount(0, duration);
            expAddText.gameObject.SetActive(false);
            btnClear.gameObject.SetActive(false);
            btnAiZr.gameObject.SetActive(true);

            btnUp.interactable = false;
            btnUp.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_hs_jy");
            btnTxt.text = GameCenter.mIns.m_LanMgr.GetLan("grow_Upgrades_3");
        }
    }
    public void hide(bool isAnimations) 
    {
        if (isAnimations)
            panelSj.GetComponent<RectTransform>().DOAnchorPosX(400, duration);
        else
            panelSj.GetComponent<RectTransform>().anchoredPosition = new Vector3(400, 0);

        if (effect != null)
        {
            GameObject.Destroy(effect.gameObject);
        }
       
        panelSj.gameObject.SetActive(false);

        OnDestroy();
    }
    public void initUI(bool isSetCount)
    {
        HeroLevelData levelData = heroInfo.getCurrLevelData(hero.level, hero.state);
        levelText.text = this.hero.level + "";
        btnUp.interactable = false;
        btnUp.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_hs_jy");
        btnTxt.text = GameCenter.mIns.m_LanMgr.GetLan("grow_Upgrades_4");

        float scale = float.Parse(this.hero.lvexp + "") / float.Parse(levelData.exp + "");
        scale = (float)Math.Round(scale, 2);
        if (scale > 1) scale = 1;

        levelAddText.gameObject.SetActive(false);
        expAddText.gameObject.SetActive(false);

        btnClear.gameObject.SetActive(false);
        btnAiZr.gameObject.SetActive(true);
        expText.text = levelData.exp == -1 ? "MAX" : hero.lvexp + "/" + levelData.exp;
        lvSjExpTo.fillAmount = 0;//重置进度条
        coinCount.text = "0";
        heroUpgradesProp.reset();

        if (oldlevel == hero.level)
        {
            lvSjExp.DOValue(scale, duration);
        }
        else
        {
            lvSjExp.DOValue(1, duration).OnComplete(() =>
            {
                lvSjExp.value = 0;
                int anCount = addLevelCount > 1 ? addLevelCount - 1 : 1;
                lvSjExp.DOValue(1, anCount<10?duration:0.15f).SetLoops(anCount).OnComplete(() => {
                    lvSjExp.value = 0;
                    lvSjExp.DOValue(scale, duration).OnComplete(() => {
                        if (HeroGrowUtils.backType != BackType.Sj)
                        {
                            return;
                        }
                        HeroLevelData levelData = heroInfo.getCurrLevelData(hero.level, hero.state);
                        if (isSetCount)
                        {
                            foreach (var item in statsItemList)
                            {
                                item.setCount();
                                item.hideCountGo();
                            }
                        }
                        if (levelData.exp == -1&& HeroGrowUtils.backType == BackType.Sj)
                        {
                            //可以突破了，跳转到突破界面
                            onBack?.Invoke(BackType.Sj);
                        }
                    });
                });
            });
            
        }     
    }
    public void loadData(HeroLevelData levelData)
    {
        
        oldlevel = hero.level;
        //初始化升级道具
        if (heroUpgradesProp == null) heroUpgradesProp = new HeroUpgradesProp(cailiaos, levelData, hero, (level, exp, scale, addExp, isChangeStatsInfo) => {
            onCountChange(levelData, level, exp, scale, addExp, isChangeStatsInfo);
        });

        //加载属性界面
        GameObject ori = argitem.gameObject;
        ori.SetActive(false);

        List<StatData> list = levelData.attrs;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].show == 1) { 
                GameObject cloneObj = GameObject.Instantiate(ori);

                StstsItem item = new StstsItem(propPanelSj, list[i], this.hero, i == list.Count - 1, cloneObj, i,true);
                statsItemList.Add(item);
            }
        }

        initUI(false);
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
        if (heroUpgradesProp != null)
        {
            heroUpgradesProp.reset();
            heroUpgradesProp.OnDestroy();
            heroUpgradesProp = null;
        }
        clearStstsItem();
    }

    UIParticleEffect effect;
    private void LoadEffect()
    {
        if (effect != null)
        {
            effect.gameObject.SetActive(true);
            effect.Play();
            return;
        }

        UIEffectManager.Instance.LoadUIEffect("", (go) =>
        {
            effect = go.AddComponent<UIParticleEffect>();

            effect.SetUP(new UIParticleEffect.ShowInfo()
            {
                _offset = 1,

                _canvas = panelSj.transform.GetComponentInParent<Canvas>(),
            });
            go.transform.SetParent(lvSjExp.transform.Find("Handle Slide Area"), false);

            go.transform.localPosition = Vector3.zero;
        });
    }

}

