using DG.Tweening;
using LitJson;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static HeroGrow;

/// <summary>
/// 英雄详细界面
/// </summary>
public class HeroGrowPanel
{
    public HeroData hero;
    public long skillId;
    public HeroInfoCfgData heroInfo;

    private Transform parent;
    private Transform xiangqingObj;

    private TextMeshProUGUI heroName;
    private Image ysIcon;
    private Image zyIcon;
    private TextMeshProUGUI zyText;
    private TextMeshProUGUI heroLevel;
    private TextMeshProUGUI heroLevelMax;
    private Slider lvExp;
    private Transform startBox;
    private Transform argsPanel;
    private Transform skillPanel;
    private Button btnGrowGo;
    private TextMeshProUGUI btnTxt;
    private GameObject argitem;
    private SkillDetilsWidgets heroGrowXialakuangPanel;
    private HeroGrowNotePanel heroGrowNotePanel;
    private Button btnNoteGo;

    private float duration;
    //升级道具控件
    public HeroLeftMenu leftMenu = null;
    private HeroGrowUpgrades heroGrowUpgradesPanel = null;
    private HeroGrowBreak heroGrowBreakPanel = null;
    private List<StstsItem> statsItemList = new List<StstsItem>();

    public HeroGrowPanel(Transform parent, HeroLeftMenu leftMenu, float duration)
    {
        this.parent = parent;
        this.leftMenu = leftMenu;
        this.hero = leftMenu.getSelectHero().data;
        this.heroInfo = leftMenu.getSelectHero().heroInfo;
        this.duration = duration;
        xiangqingObj = parent.transform.Find("panel_xiangqing");
        heroName = xiangqingObj.transform.Find("name/nametext").GetComponent<TextMeshProUGUI>();
        ysIcon = xiangqingObj.transform.Find("name/ysicon").GetComponent<Image>();
        zyIcon = xiangqingObj.transform.Find("name/zyicon").GetComponent<Image>();
        zyText = xiangqingObj.transform.Find("name/zytext").GetComponent<TextMeshProUGUI>();
        heroLevel = xiangqingObj.transform.Find("levelpanel/level").GetComponent<TextMeshProUGUI>();
        lvExp = xiangqingObj.transform.Find("expSlider").GetComponent<Slider>();
        heroLevelMax = xiangqingObj.transform.Find("levelpanel/levelmax").GetComponent<TextMeshProUGUI>();
        startBox = xiangqingObj.transform.Find("starbox");
        argsPanel = xiangqingObj.transform.Find("basegrow");
        skillPanel = xiangqingObj.transform.Find("skills/skillsPanel/skills");
        argitem = argsPanel.Find("argitem").gameObject;
        btnGrowGo = xiangqingObj.transform.Find("btnGrowGo").GetComponent<Button>();
        btnTxt = btnGrowGo.transform.Find("txt").GetComponent<TextMeshProUGUI>();
        btnNoteGo = xiangqingObj.transform.Find("levelpanel/note").GetComponent<Button>();

        btnGrowGo.onClick.RemoveAllListeners();
        btnGrowGo.onClick.AddListener(btnGrowGoOnClick);
        btnNoteGo.AddListenerBeforeClear(btnOnClickTupo);

        leftMenu.setChangeHeroSelect((data, heroInfoData) =>
        {
            this.hero = data;
            this.heroInfo = heroInfoData;
            loadInfoData();

            if (heroGrowXialakuangPanel != null)
            {
                heroGrowXialakuangPanel.hide(false);

                isOpenXiangqing = true;
            }
        });

        //初始化升级面板
        heroGrowUpgradesPanel = new HeroGrowUpgrades(parent, duration, (backtype) =>
        {
            onSjOrTp(backtype, false);
        });
        heroGrowBreakPanel = new HeroGrowBreak(parent, duration, (backtype) =>
        {
            onSjOrTp(backtype, false);
        });
        heroGrowXialakuangPanel = new SkillDetilsWidgets(parent, duration, (backtype) =>
        {
            //onSjOrTp(backtype, false);

            DeselectSkill();

        });
        heroGrowNotePanel = new HeroGrowNotePanel(parent, duration, hero, leftMenu, (backtype) =>
        {
            //btnOnClickTupo(true);
        });
    }
    public void close()
    {
        OnDestroy();

        parent.gameObject.SetActive(false);
        heroGrowUpgradesPanel?.hide(false);
        heroGrowBreakPanel?.hide(false);
        heroGrowBreakPanel = null;
        heroGrowUpgradesPanel = null;
    }

    public void show(bool isAnimations)
    {
        loadInfoData();
        parent.gameObject.SetActive(true);

        if (isAnimations)
            xiangqingObj.GetComponent<RectTransform>().DOAnchorPosX(-350, duration, false);
        else
            xiangqingObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(-350, 0);

        xiangqingObj.gameObject.SetActive(true);

        heroGrowXialakuangPanel.hide(false);

        isOpenXiangqing = true;


    }
    public void hide(bool isAnimations)
    {
        if (isAnimations)
            xiangqingObj.GetComponent<RectTransform>().DOAnchorPosX(400, duration).OnComplete(() => xiangqingObj.gameObject.SetActive(false));
        else
        {
            xiangqingObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(400, 0);
            xiangqingObj.gameObject.SetActive(false);
        }
        clearStstsItem();

        heroGrowXialakuangPanel.hide(false);

        isOpenXiangqing = true;


    }
    SkillItem currentSelectSkill;

    private void DeselectSkill()
    {
        currentSelectSkill?.SetSelectedState(false);
    }


    private void loadInfoData()
    {
        heroName.text = GameCenter.mIns.m_LanMgr.GetLan(heroInfo.name2);
        heroLevel.text = hero.level + "";

        HeroBreakData breakdata = heroInfo.getCurrBreakData(hero.state);
        heroLevelMax.text = breakdata.herolevel.ToString();
        ysIcon.sprite = hero.GetYuansuIcon(heroInfo.element);
        ysIcon.SetNativeSize();
        zyIcon.sprite = hero.GetZhiYeIcon(heroInfo.profession);
        zyIcon.SetNativeSize();
        zyText.text = hero.GetZhiYeName(heroInfo.profession);

        //加载突破等级
        HeroGrowUtils.setStarUi(hero.state, false, startBox);

        clearStstsItem();
        //加载属性
        GameObject ori = argitem.gameObject;
        ori.SetActive(false);

        HeroBreakData data = heroInfo.getCurrBreakData(6);
        if (data != null)
        {
            for (int i = 0; i < data.attrs.Count; i++)
            {
                if (data.attrs[i].show == 1)
                {
                    GameObject cloneObj = GameObject.Instantiate(ori);

                    StstsItem item = new StstsItem(argsPanel, data.attrs[i], this.hero, i == data.attrs.Count - 1, cloneObj, i, false);
                    statsItemList.Add(item);
                }
            }
        }

        //加载技能
        for (int i = 1; i <= 3; i++)
        {
            long sid = 0;
            if (i == 1)
            {
                sid = heroInfo.cardskill1;
            }
            else if (i == 2)
            {
                sid = heroInfo.cardskill2;
            }
            else if (i == 3)
            {
                sid = heroInfo.cardskill3;
            }
            SkillItem skillItem = new SkillItem(skillPanel, i, sid, heroInfo, (skillItem) =>
            {
                currentSelectSkill?.SetSelectedState(false);
                //skillItem.
                skillItem.SetSelectedState(true);

                currentSelectSkill = skillItem;
                //skillitem
                btnOnClickXiangqing(hero, true, skillItem.skillid);
            });
            skillItem.bindUI();
        }

        HeroLevelData levelData = heroInfo.getCurrLevelData(hero.level, hero.state);

        if (levelData.exp == -1)
        {
            if (breakdata.costs == null || breakdata.costs.Count < 1)
            {
                lvExp.value = 0;
                btnGrowGo.interactable = false;
                btnGrowGo.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_hs_jy");
                btnTxt.text = GameCenter.mIns.m_LanMgr.GetLan("grow_Panel_1");
            }
            else
            {
                lvExp.value = 0;
                btnGrowGo.interactable = true;
                btnGrowGo.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_cs");
                btnTxt.text = GameCenter.mIns.m_LanMgr.GetLan("grow_Panel_2");
            }
        }
        else
        {
            float scale = float.Parse(hero.lvexp + "") / float.Parse(levelData.exp + "");
            scale = (float)Math.Round(scale, 2);
            if (scale > 1) scale = 1;
            lvExp.value = scale;
            btnGrowGo.interactable = true;
            btnGrowGo.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_cs");
            btnTxt.text = GameCenter.mIns.m_LanMgr.GetLan("grow_Panel_2");
        }
    }




    /// <summary>
    /// 打开详情弹窗
    /// </summary>
    private bool isOpenXiangqing = true;
    public void btnOnClickXiangqing(HeroData hero, bool idOpen, long skillid)
    {
        heroGrowXialakuangPanel.show(hero, idOpen, skillid);

        //if (isOpenXiangqing)
        //{
        //    Debug.Log("isOpenXiangqing");


        //    isOpenXiangqing = false;
        //}
        //else
        //{
        //    Debug.Log("isNotOpenXiangqing");

        //    heroGrowXialakuangPanel.hide(idOpen);
        //    isOpenXiangqing = true;
        //}
    }
    /// <summary>
    /// 打开突破弹窗
    /// </summary>
    //private bool isOpenTupo = true;
    public void btnOnClickTupo()
    {
        heroGrowNotePanel.show(hero, true);

        //if (isOpenTupo)
        //{
        //    heroGrowNotePanel.show(true);
        //    isOpenTupo = false;
        //}
        //else
        //{
        //    heroGrowNotePanel.hide(false);
        //    isOpenTupo = true;
        //}
    }
    /// <summary>
    /// 点击培养按扭 进入到升级界面
    /// </summary>
    public void btnGrowGoOnClick()
    {
        int level = hero.level;
        int state = hero.state;
        HeroLevelData levelData = heroInfo.getCurrLevelData(level, state);
        if (levelData.exp == -1 && levelData.costid < 1)
        {
            //本界面 等级最高了
            GameCenter.mIns.m_UIMgr.PopMsg("已升至最高等级");
            return;
        }
        onSjOrTp(BackType.Info, true);
    }
    public void onSjOrTp(BackType backtype, bool isAnimations)
    {

        switch (backtype)
        {
            case BackType.Tp:
                heroGrowBreakPanel.hide(isAnimations);
                break;
            case BackType.Sj:
                heroGrowUpgradesPanel.hide(isAnimations);
                break;
            case BackType.Info:
                leftMenu.hide();
                hide(true);
                break;
        }

        int level = hero.level;
        int state = hero.state;
        HeroLevelData levelData = heroInfo.getCurrLevelData(level, state);
        if (backtype == BackType.Info && levelData.exp == -1 && levelData.costid < 1)
        {
            //本界面 等级最高了
            GameCenter.mIns.m_UIMgr.PopMsg("已升至最高等级");
            return;
        }
        else if (backtype != BackType.Info && levelData.exp == -1 && levelData.costid < 1)
        {
            //非本界面 等级最高了，则显示 详情界面
            leftMenu.show();
            show(isAnimations);
        }
        else if (levelData.exp == -1)
        {
            heroGrowBreakPanel.show(hero, heroInfo, isAnimations);
            HeroGrowUtils.backType = BackType.Tp;
        }
        else
        {
            heroGrowUpgradesPanel.show(hero, heroInfo, isAnimations);
            HeroGrowUtils.backType = BackType.Sj;
        }

    }
    /// <summary>
    /// 从任意界面返回到信息详情界面
    /// </summary>
    public void goInfoBySj()
    {
        heroGrowUpgradesPanel.hide(true);
        leftMenu.show();
        show(true);
    }
    public void goInfoByTp()
    {
        heroGrowBreakPanel.hide(true);
        leftMenu.show();
        show(true);
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
        if (heroGrowUpgradesPanel != null)
            heroGrowUpgradesPanel.OnDestroy();
        if (heroGrowBreakPanel != null)
            heroGrowBreakPanel.OnDestroy();

    }

    private class SkillItem
    {
        private Transform parent;
        private Transform node;
        private int index;
        public long skillid;
        private HeroInfoCfgData heroinfo;
        private Button btn;

        GameObject selectedObj;

        RectTransform selectLT, selectLB, selectRT, selectRB;


        public Image imgIcon { get; private set; }
        public Image imgQuality { get; private set; }
        public Image imgElementIcon { get; private set; }
        public Image imgSkillTypeDes { get; private set; }

        public TMP_Text desc2 { get; private set; }
        public GameObject expend { get; private set; }
        public TMP_Text txExpend { get; private set; }

        public SkillItem(Transform parent, int index, long skillid, HeroInfoCfgData heroinfo, Action<SkillItem> onBack)
        {
            this.parent = parent;
            this.index = index;
            this.skillid = skillid;
            this.heroinfo = heroinfo;

            node = parent.Find("skillbox" + index);
            imgIcon = node.Find("skill/bg/icon").GetComponent<Image>();
            imgQuality = node.Find("skill/bg/quality").GetComponent<Image>();
            imgElementIcon = node.Find("skill/bg/yuansu/icon").GetComponent<Image>();
            imgSkillTypeDes = node.Find("skill/bg/imgSkillTypeDes").GetComponent<Image>();
            desc2 = node.Find("skill/bg/desc2").GetComponent<TMP_Text>();
            expend = node.Find("skill/bg/expend").gameObject;
            txExpend = expend.transform.Find("text").GetComponent<TMP_Text>();
            btn = node.Find("skill").GetComponent<Button>();
            btn.AddListenerBeforeClear(() => { onBack?.Invoke(this); });

            selectedObj = node.Find("selects").gameObject;

            selectLT = Utils.Find<RectTransform>(selectedObj.transform, "select1");
            selectRT = Utils.Find<RectTransform>(selectedObj.transform, "select2");
            selectLB = Utils.Find<RectTransform>(selectedObj.transform, "select3");
            selectRB = Utils.Find<RectTransform>(selectedObj.transform, "select4");
        }

        public void bindUI()
        {
            BattleSkillCfg m_SkillCfg = BattleCfgManager.Instance.GetSkillCfgBySkillID(skillid);

            // 技能icon
            if (!string.IsNullOrEmpty(m_SkillCfg.icon))
            {
                imgIcon.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(m_SkillCfg.icon);
            }

            // 品质条
            if (heroinfo != null)
            {
                imgQuality.sprite = commontool.GetQualityBarIcon(GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(heroinfo.heroid).quality);
            }

            // 元素icon
            if (m_SkillCfg.skillelement > 0)
            {
                // 不显示
                imgElementIcon.sprite = commontool.GetYuansuIcon(m_SkillCfg.skillelement);
            }
            imgElementIcon.transform.parent.gameObject.SetActive(m_SkillCfg.skillelement != 0);

            // 类型描述
            Sprite skillTypeDesIcon = commontool.GetSkillTypeDesIcon(m_SkillCfg.skilltype);
            imgSkillTypeDes.sprite = skillTypeDesIcon;
            imgSkillTypeDes.gameObject.SetActive(skillTypeDesIcon != null);

            // 技能类型描述
            desc2.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(m_SkillCfg.note1));
            // cd

            // 花费
            expend.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_fight_icon_suanli_xiao01");
            txExpend.GetComponent<TextMeshProUGUI>().text = m_SkillCfg.energycost.ToString();

            // selected
        }

        public void SetSelectedState(bool isSelected)
        {
            selectedObj.SetActive(isSelected);

            if (isSelected)
            {
                selectLT.DOLocalMove(selectLT.localPosition * 2, 0.3f).From();
                selectLB.DOLocalMove(selectLB.localPosition * 2, 0.3f).From();
                selectRT.DOLocalMove(selectRT.localPosition * 2, 0.3f).From();
                selectRB.DOLocalMove(selectRB.localPosition * 2, 0.3f).From();
            }
        }
    }
}
