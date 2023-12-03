using LitJson;
using System;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

public class HeroGrowBreak
{
    private HeroData heroData;

    private Transform parent;

    //突破
    private Transform panelTp;
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

    private HeroData hero;
    private HeroInfoCfgData heroInfo;
    private float duration;
    private Action<BackType> onBack;

    private List<StstsItem> statsItemList = new List<StstsItem>();
    private List<HeroPropItem> breakProps = new List<HeroPropItem>();

    public HeroGrowBreak(Transform parent, float duration,Action<BackType> onBack)
    {
        this.parent = parent;
        this.duration = duration;
        this.onBack = onBack;
        panelTp = parent.Find("Panel_tupo");
        starTpBox = panelTp.transform.Find("starpanel/star/starbox");
        starGoTpBox = panelTp.transform.Find("starpanel/star_tupo/starbox");
        propPanelTp = panelTp.transform.Find("basegrow");
        argItem = panelTp.transform.Find("basegrow/argitem").gameObject;
        levelTxt = panelTp.transform.Find("djsx/sx").gameObject.GetComponent<TextMeshProUGUI>();
        nextLevelTxt = panelTp.transform.Find("djsx/sx_tp").gameObject.GetComponent<TextMeshProUGUI>();
        cailiaos = panelTp.Find("cailiaos");
        propitem = cailiaos.Find("propitem").gameObject;
        btnBreak = panelTp.Find("btnbox/btn").gameObject.GetComponent<Button>();
        btnBreakImg = btnBreak.GetComponent<Image>();
        btnTxt = panelTp.Find("btnbox/btn/txt").gameObject.GetComponent<TextMeshProUGUI>();
        btnBreak.onClick.RemoveAllListeners();
        btnBreak.onClick.AddListener(btnBreakGo);

        panelTp.GetComponent<RectTransform>().DOAnchorPosX(400, duration);
    }

    private void btnBreakGo()
    {
        if (!propIsCount())
        {
            //道具不足
            GameCenter.mIns.m_UIMgr.PopMsg("材料不足");
            return;
        }

        JsonData data = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        data["heroid"] = this.hero.heroID;
        data["tostate"] = this.hero.state + 1;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.GROW_HERO_BREAK, data, (seqid, code, data) =>
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
                        onBack?.Invoke(BackType.Tp);
                    });
                }
            }
        });
    }

    public void show( HeroData hero, HeroInfoCfgData heroInfo, bool isAnimations)
    {
        this.hero = hero;
        this.heroInfo = heroInfo;

        if (isAnimations)
            panelTp.GetComponent<RectTransform>().DOAnchorPosX(-350, duration);
        else
            panelTp.GetComponent<RectTransform>().anchoredPosition = new Vector3(-350, 0);
        panelTp.gameObject.SetActive(true);

        HeroGrowUtils.setStarUi(hero.state, false, starTpBox);
        HeroGrowUtils.setStarUi(hero.state, true, starGoTpBox);
        levelTxt.text = hero.level.ToString();
        nextLevelTxt.text = heroInfo.getNextBrakLevel(hero.state)?.herolevel.ToString();

        //加载属性界面
        GameObject ori = argItem.gameObject;
        ori.SetActive(false);

        HeroBreakData breakdata = heroInfo.getCurrBreakData(hero.state);
        List<StatData> list = breakdata.attrs;
        for (int i = 0; i < list.Count; i++)
        {
            GameObject cloneObj = GameObject.Instantiate(ori);

            StstsItem item = new StstsItem(propPanelTp, list[i], hero, i == list.Count - 1, cloneObj, i,true);
            statsItemList.Add(item);
        }

        GameObject propori = propitem.gameObject;
        propori.SetActive(false);
        //加载材料
        foreach (var item in breakdata.costs)
        {
            GameObject cloneObj = GameObject.Instantiate(propori);

            HeroPropItem prop = new HeroPropItem(cailiaos, cloneObj, item);
            breakProps.Add(prop);
        }

        if (propIsCount())
        {
            btnBreakImg.sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_cs");
            btnTxt.text = GameCenter.mIns.m_LanMgr.GetLan("grow_Break_1");
            btnBreak.interactable = true;
        }
        else
        {
            btnBreakImg.sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_hs_jy");
            btnTxt.text = GameCenter.mIns.m_LanMgr.GetLan("grow_Break_2");
            btnBreak.interactable = false;
        }

        //加载突破参数变化内容
        JsonData data = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        data["tolevel"] = -1;
        data["heroid"] = hero.heroID;

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
            panelTp.GetComponent<RectTransform>().DOAnchorPosX(400, duration);
        else
            panelTp.GetComponent<RectTransform>().anchoredPosition = new Vector3(400, 0);
        panelTp.gameObject.SetActive(false);
        OnDestroy();
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

