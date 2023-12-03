using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class basebattle_heroInfo
{
    private bool isInit = false;
    private HeroData m_CurrHeroData;
    private GameObject m_HeroInfoRoot;

    private Image m_HeroIcon;
    private RectTransform m_HeroIconRt;
    private RectTransform m_HeroIconChildMaskRt;
    private Transform m_Root;
    private Transform m_Bg;
    private TMP_Text m_TxHeroType;
    private Image m_ImgHeroTypeBg;
    private Image m_ImgHeroTypeIcon;
    private TMP_Text m_TxHeroName;
    private Transform m_SkillBtnsRoot;
    private Image m_ImgSkillType;
    private TMP_Text m_TxSkillType;
    private ScrollRect m_SkillDesSr;
    private TMP_Text m_TxSkillDes;

    private List<SkillBtnItem> skillBtnItems = new List<SkillBtnItem>();
    public void InitHeroInfo(GameObject heroInfoRoot, long heroId)
    {
        if (heroInfoRoot == null || heroId == 0) { return; }
        m_HeroInfoRoot = heroInfoRoot;
        m_CurrHeroData = HeroDataManager.Instance.GetHeroData(heroId);
        BindUI();
        SetUIData();
    }

    private void BindUI()
    {
        m_HeroIcon = m_HeroInfoRoot.transform.parent.Find("icon").GetComponent<Image>();
        m_HeroIconRt = m_HeroIcon.GetComponent<RectTransform>();
        m_HeroIconChildMaskRt = m_HeroIcon.transform.GetChild(0).GetComponent<RectTransform>();
        m_Root = m_HeroInfoRoot.transform;
        m_Bg = m_HeroInfoRoot.transform.Find("bg").transform;

        m_TxHeroType = m_Bg.transform.Find("heroType/txHeroType").GetComponent<TMP_Text>();
        m_ImgHeroTypeBg = m_Bg.transform.Find("heroName/bg").GetComponent<Image>();
        m_ImgHeroTypeIcon = m_Bg.transform.Find("heroName/imgHeroTypeIcon").GetComponent<Image>();
        m_TxHeroName = m_Bg.transform.Find("heroName/txHeroName").GetComponent<TMP_Text>();

        m_SkillBtnsRoot = m_Bg.transform.Find("skillBtns").transform;

        m_ImgSkillType = m_Bg.transform.Find("imgSkillType").GetComponent<Image>();
        m_TxSkillType = m_Bg.transform.Find("txSkillType").GetComponent<TMP_Text>();
        m_SkillDesSr = m_Bg.transform.Find("skillDesSr").GetComponent<ScrollRect>();
        m_TxSkillDes = m_Bg.transform.Find("skillDesSr/txSkillDes").GetComponent<TMP_Text>();
    }

    public void UpdateFadeBody(int lorr)
    {
        if (!isInit) return;
        // left-> x=153, right-> x=-159
        float heroIconXAnchorPos = 0;//153;
        if (lorr >0)
        {
            // right
            heroIconXAnchorPos = 0;//-159;
            m_HeroIcon.transform.localScale = new Vector3(-1, 1, 1);
            m_HeroIconChildMaskRt.anchoredPosition = new Vector2(529, m_HeroIconChildMaskRt.anchoredPosition.y);
        }
        else
        {
            m_HeroIcon.transform.localScale = new Vector3(1, 1, 1);
            m_HeroIconChildMaskRt.anchoredPosition = new Vector2(536, m_HeroIconChildMaskRt.anchoredPosition.y);
        }
        m_HeroIconRt.anchoredPosition = new Vector2(heroIconXAnchorPos, m_HeroIconRt.anchoredPosition.y);
    }


    private async void SetUIData()
    {
        HeroInfoCfgData heroInfoCfgData = GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(m_CurrHeroData.heroID);
        if (heroInfoCfgData != null)
        {
            // 立绘
            m_HeroIcon.sprite = await SpriteManager.Instance.GetTextureSpriteSync($"fadebody/hero_fadebody_{m_CurrHeroData.heroID}_01");
            //UpdateFadeBody(0);

            // 元素属性  1 = 水 2 = 火 3 = 风 4 = 雷
            m_HeroInfoRoot.transform.parent.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(commontool.GetHeroInfoBgResName(heroInfoCfgData.element));

            m_TxHeroType.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(heroInfoCfgData.note));
            m_ImgHeroTypeIcon.sprite = m_CurrHeroData.GetYuansuIcon(heroInfoCfgData.element);
            m_TxHeroName.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(heroInfoCfgData.name));

            // skills
            skillBtnItems = new List<SkillBtnItem>();
            int counter = 1;
            List<BattleSkillCfg> skillCfgs = new List<BattleSkillCfg>();
            foreach (var kv in m_CurrHeroData.skills)
            {
                BattleSkillCfg skillCfg = BattleCfgManager.Instance.GetSkillCfgBySkillID(kv.Key);
                if (skillCfg != null)
                    skillCfgs.Add(skillCfg);
            }
            skillCfgs.Sort(new SkillBtnItemSort());
            foreach (var scfg in skillCfgs)
            {
                if (scfg != null && scfg.skilltype != 1)
                {
                    SkillBtnItem skillBtnItem = new SkillBtnItem(m_SkillBtnsRoot.Find($"skill_{counter}").gameObject, heroInfoCfgData, scfg, (_skillCfg) =>
                    {
                        if (_skillCfg != null)
                        {
                            // todo
                        }
                    });
                    skillBtnItems.Add(skillBtnItem);
                    counter++;
                }
            }

            // 职业类型:1 = 歼灭 2 = 战术 3 = 支援 4 = 抵御
            m_ImgSkillType.sprite = m_CurrHeroData.GetZhiYeIcon(heroInfoCfgData.profession, true);
            m_TxSkillType.SetTextExt(m_CurrHeroData.GetZhiYeName(heroInfoCfgData.profession));
            // m_SkillDesSr
            m_TxSkillDes.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(heroInfoCfgData.note1));

            // 技能元素 0 = 无元素 1 = 水 2 = 火 3 = 风 4 = 雷
            // element
            string imgEleBgType = commontool.GetYuansuBgIcon(heroInfoCfgData.element);
            m_ImgHeroTypeBg.sprite = SpriteManager.Instance.GetSpriteSync(imgEleBgType);
            isInit = true;
        }
    }

    public void Clear()
    {
        isInit = false;
        skillBtnItems.Clear();
    }

    /// <summary>
    /// 类型效的在前面
    /// </summary>
    private class SkillBtnItemSort : IComparer<BattleSkillCfg>
    {
        public int Compare(BattleSkillCfg a, BattleSkillCfg b)
        {
            return a.skilltype.CompareTo(b.skilltype);
        }
    }

    private class SkillBtnItem
    {
        public GameObject skillBtnObj;
        public Image imgConsume;
        public TMP_Text txConsume;
        public TMP_Text txDes;
        public TMP_Text txType;
        public Image imgSkillTYPE;
        public Image imgSkillTYPEDes;
        public BattleSkillCfg battleSkillCfg;
        public HeroInfoCfgData heroInfoCfgData;
        public Action<BattleSkillCfg> clickCallback;
        public SkillBtnItem(GameObject skillBtnObj,HeroInfoCfgData heroInfoCfgData, BattleSkillCfg battleSkillCfg, Action<BattleSkillCfg> clickCallback)
        {
            this.skillBtnObj = skillBtnObj;
            this.battleSkillCfg = battleSkillCfg;
            this.clickCallback = clickCallback;

            imgConsume = skillBtnObj.transform.Find("select/imgConsume").GetComponent<Image>();
            txConsume = skillBtnObj.transform.Find("select/imgConsume/txConsume").GetComponent<TMP_Text>();
            txDes = skillBtnObj.transform.Find("select/txDes").GetComponent<TMP_Text>();
            txType = skillBtnObj.transform.Find("select/txType").GetComponent<TMP_Text>();
            imgSkillTYPE = skillBtnObj.transform.Find("select/imgSkillTYPE").GetComponent<Image>();
            imgSkillTYPEDes = skillBtnObj.transform.Find("select/imgSkillTYPEDes").GetComponent<Image>();
            skillBtnObj.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(battleSkillCfg.icon);

            txConsume.SetTextExt(battleSkillCfg.energycost.ToString());
            // 技能类型1 = 普攻 2 = 战技 3 = 秘技 4 = 终结技
            string skillTypeDes = battleSkillCfg.skilltype == 1 ? "普攻" :
                                    (battleSkillCfg.skilltype == 2 ? "战技" :
                                    (battleSkillCfg.skilltype == 3 ? "秘技" : "终结技"));
            txDes.SetTextExt("");
            txType.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(battleSkillCfg.note1.ToString()));

            skillBtnObj.GetComponent<Button>().AddListenerBeforeClear(() =>
            {
                clickCallback?.Invoke(battleSkillCfg);
            });

            string imgConsumeResName = "ui_fight_icon_suanli_xiao01";//commontool.GetSkillConsumeIconResName(battleSkillCfg.skilltype);
            imgConsume.sprite = SpriteManager.Instance.GetSpriteSync(imgConsumeResName);

            // 技能元素 0 = 无元素 1 = 水 2 = 火 3 = 风 4 = 雷
            imgSkillTYPE.sprite = commontool.GetQualityBarIcon(heroInfoCfgData.quality);

            Sprite skillTypeDesIcon = commontool.GetSkillTypeDesIcon(battleSkillCfg.skilltype);
            imgSkillTYPEDes.sprite = skillTypeDesIcon;
            imgSkillTYPEDes.gameObject.SetActive(skillTypeDesIcon != null);
        }
    }
}
