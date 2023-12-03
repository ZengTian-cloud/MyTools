using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroGrowNotePanel
{
    private HeroInfoCfgData heroInfo;
    private HeroLeftMenu leftMenu;
    private Transform parent;
    private float duration;
    private Action<BackType> onBack;
    private Transform panelSj;
    private TextMeshProUGUI levelcountText;
    private TextMeshProUGUI leveltishiText;
    private TextMeshProUGUI level_tishiText;
    /// <summary>
    /// 需求等级
    /// </summary>
    private TextMeshProUGUI heroLevel;
    /// <summary>
    /// 上限等级
    /// </summary>
    private TextMeshProUGUI heroLevelMax;
    private Transform startBox;
    public HeroData hero;
    private GameObject wupinItem;
    private GameObject wupinItem2;
    private Transform cailiaos;
    private Transform cailiaos2;
    private int propcount = 0;
    private List<HeroWupinItem> breakProps = new List<HeroWupinItem>();
    private List<HeroWupinItem> breakProps2 = new List<HeroWupinItem>();
    private Button btnLeft, btnRight;

    Transform selectedStar;

    int selectedIndex;

    public HeroGrowNotePanel() { }

    public HeroGrowNotePanel(Transform parent, float duration, HeroData hero, HeroLeftMenu leftMenu, Action<BackType> onBack)
    {
       

        this.hero = hero;
        this.parent = parent;
        this.duration = duration;
        this.onBack = onBack;
        this.leftMenu = leftMenu;
        panelSj = parent.Find("tupo_tanchuang");
        //levelcountText = panelSj.Find("levelneed/levelcount").GetComponent<TextMeshProUGUI>();
        leveltishiText = panelSj.Find("levelneed/tishi").GetComponent<TextMeshProUGUI>();
        level_tishiText = panelSj.Find("tishitext/level_tishi").GetComponent<TextMeshProUGUI>();
        //需求等级

        heroLevel = panelSj.transform.Find("levelneed/levelcount").GetComponent<TextMeshProUGUI>();
        heroLevelMax = panelSj.transform.Find("tishitext/level_tishi").GetComponent<TextMeshProUGUI>();
        startBox = panelSj.transform.Find("star");
        cailiaos = panelSj.Find("wuping");
        cailiaos2 = panelSj.Find("wuping2");
        wupinItem = cailiaos.Find("wupinItem").gameObject;
        //wupinItem2 = cailiaos2.Find("Scroll View/Viewport/Content/wupinItem1").gameObject;
        wupinItem2 = cailiaos2.Find("wupinItem1").gameObject;
        btnLeft = panelSj.Find("leftbtn").GetComponent<Button>();
        btnRight = panelSj.Find("rightbtn").GetComponent<Button>();
        //(BattleCfgManager.Instance.GetSkillCfgBySkillID(curSkillID);技能
        // BattleCfgManager.Instance.GetTalentCfg()
        btnLeft.AddListenerBeforeClear(OnClickLeftStar);
        btnRight.AddListenerBeforeClear(OnClickRightStar);

        Utils.Find<Button>(panelSj, "mask").AddListenerBeforeClear(() => hide(true));
        selectedStar = panelSj.Find("selectedStar");
    }
    int indexState;
    int indexState_L;
    private void OnClickRightStar()
    {
        if (selectedIndex >= 5) return;
        selectedIndex = Math.Clamp(++selectedIndex, 0, 5);
        SetSelectedStar();
        SetCurrentSelectedInfoShow();
    }
      
    private void OnClickLeftStar()
    {
        if (selectedIndex <= 0) return;
        selectedIndex = Math.Clamp(--selectedIndex, 0, 5);
        SetSelectedStar();
        SetCurrentSelectedInfoShow();
    }
    private HeroBreakData breakdata;
    private HeroLevelData Herolevel;
    BattleSkillCfg bsc;
    public void show(HeroData hero,bool isAnimations/*, long skillid*/)
    {
        this.hero = hero;
        if (isAnimations)
            panelSj.GetComponent<RectTransform>().DOAnchorPosX(0, duration);
        else
            panelSj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0);
        panelSj.gameObject.SetActive(true);

        HeroGrowUtils.setStarUi(hero.state, false, startBox);

        heroInfo = leftMenu.getSelectHero().heroInfo;

        selectedIndex = Math.Clamp(hero.state, 0, 5);

        SetSelectedStar();

        SetCurrentSelectedInfoShow();
    }
    private void UpdateItem(HeroBreakData breakdata)
    {
        clearPropsItem();
        Debug.Log("+++++++++++" + breakProps.Count);
        breakProps.Clear();
        breakProps2.Clear();
        //加载消耗品
        GameObject propori = wupinItem.gameObject;
        propori.SetActive(false);
        GameObject propori2 = wupinItem2.gameObject;
        propori2.SetActive(false);
        //加载材料
        foreach (var item in breakdata.costs)
        {
            GameObject cloneObj = GameObject.Instantiate(propori);
            //cloneObj.GetComponent<RectTransform>().localScale = new Vector3(0.75f, 0.75f, 0.75f);
            HeroWupinItem prop = new HeroWupinItem(cailiaos, cloneObj, item);
            breakProps.Add(prop);
        }
        Debug.Log("=================" + breakProps.Count);

        foreach (var item in breakdata.costs)
        {
            GameObject cloneObj2 = GameObject.Instantiate(propori2);
            //cloneObj.GetComponent<RectTransform>().localScale = new Vector3(0.75f, 0.75f, 0.75f);
            HeroWupinItem prop2 = new HeroWupinItem(cailiaos2, cloneObj2, item);
            breakProps2.Add(prop2);
        }
    }
    public void clearPropsItem()
    {
        foreach (var item in breakProps)
        {
            item.OnDestroy();
        }
        //breakProps.Clear();
        foreach (var item in breakProps2)
        {
            item.OnDestroy();
        }
        //breakProps2.Clear();
    }
    public void hide(bool isAnimations)
    {
        if (isAnimations)
            panelSj.GetComponent<RectTransform>().DOAnchorPosX(0, duration);
        else
            panelSj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0);
        panelSj.gameObject.SetActive(false);
        clearPropsItem();
        selectedStar.SetParent(panelSj);//框架的坑
    }
    /// <summary>
    /// 设置选中状态
    /// </summary>
    void SetSelectedStar()
    {
        selectedStar.SetParent(startBox.GetChild(selectedIndex));
        selectedStar.localPosition = Vector3.zero;
    }
    /// <summary>
    /// 设置当前选中星级的信息
    /// </summary>
    void SetCurrentSelectedInfoShow()
    {
        HeroBreakData currentSelectedStateData = heroInfo.getCurrBreakData(selectedIndex);
        HeroBreakData nextStateData = heroInfo.getNextBrakLevel(selectedIndex);
        heroLevel.SetText(currentSelectedStateData.herolevel.ToString());
        heroLevelMax.SetText(nextStateData.herolevel.ToString());
        leveltishiText.SetText(hero.state > selectedIndex?"已突破":"未突破");//显示相关的文本内容 后续加到语言表里 从语言表里获取
        UpdateItem(currentSelectedStateData);
    }
}
