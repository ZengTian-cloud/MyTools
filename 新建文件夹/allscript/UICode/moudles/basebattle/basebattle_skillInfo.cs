using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text.RegularExpressions;
using System.Linq;

public class basebattle_skillInfo
{
    private bool isInit = false;
    private BattleSkillCfg m_CurrBattleSkillCfg;
    private HeroInfoCfgData m_CurrHeroInfoCfgData;
    private GameObject m_SkillInfoRoot;

    private Image m_HeroIcon;
    private RectTransform m_HeroIconRt;
    private Image m_HeroBg;
    private Transform m_Root;
    private Transform m_Bg;

    private RectTransform m_BgMengBan_1;

    private RectTransform skillType;
    private Image skillType_imgEle;
    private TMP_Text skillType_txSkillType;

    private RectTransform nodeContent;
    private RectTransform nodeSkillIcon;
    private RectTransform skillIcon;
    private Image imgSkillIcon;
    private Image imgSkillQuality;
    private TMP_Text txSkillIconType;
    private TMP_Text txConsumeDes;
    private Image imgConsume;
    private TMP_Text txConsume;

    private RectTransform nodeSkillDes;
    private RectTransform nodeSkillName;
    private Image imgSkillEle;
    private TMP_Text txSkillName;
    private TMP_Text txSkillDes;

    private RectTransform nodeSkillExt;
    private RectTransform nodeSkillExtEmpty;
    private GameObject ori_extra;

    private HeroData heroData;

    private BaseHero baseHero;

    private List<ExtraAddItem> extraAddItems = new List<ExtraAddItem>();
    public void InitSkillInfo(GameObject skillInfoRoot, long skillId, long heroId)
    {
        if (skillInfoRoot == null || skillId == 0) { return; }
        m_SkillInfoRoot = skillInfoRoot;
        m_CurrBattleSkillCfg =  BattleCfgManager.Instance.GetSkillCfgBySkillID(skillId);
        m_CurrHeroInfoCfgData = GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(heroId);
        heroData = HeroDataManager.Instance.GetHeroData(heroId);
        baseHero = BattleHeroManager.Instance.GetBaseHeroByHeroID(heroId);
        BindUI();
        SetUIData();
    }

    // 1:出现, -1:隐藏
    private int m_State;
    public void DoFade(int state)
    {
        if (state == m_State)
        {
            return;
        }
    }

    private void BindUI()
    {
        m_HeroIcon = m_SkillInfoRoot.transform.parent.Find("icon").GetComponent<Image>();
        m_HeroIconRt = m_HeroIcon.GetComponent<RectTransform>();
        m_HeroBg = m_HeroIcon.transform.parent.GetComponent<Image>();
        m_Root = m_SkillInfoRoot.transform;
        m_Bg = m_SkillInfoRoot.transform.Find("bg").transform;
        m_BgMengBan_1 = m_Bg.Find("bg1").GetComponent<RectTransform>();

        skillType = m_Bg.transform.Find("skillType").GetComponent<RectTransform>();
        skillType_imgEle = skillType.transform.Find("imgEle").GetComponent<Image>();
        skillType_txSkillType = skillType.transform.Find("txSkillType").GetComponent<TMP_Text>();

        nodeContent = m_Bg.transform.Find("nodeContent").GetComponent<RectTransform>();

        nodeSkillIcon = nodeContent.transform.Find("nodeSkillIcon").GetComponent<RectTransform>();
        skillIcon = nodeSkillIcon.transform.Find("skillIcon").GetComponent<RectTransform>();
        imgSkillIcon = skillIcon.transform.Find("icon").GetComponent<Image>();
        imgSkillQuality = skillIcon.transform.Find("quality").GetComponent<Image>();
        txSkillIconType = skillIcon.transform.Find("txSkillIconType").GetComponent<TMP_Text>();
        txConsumeDes = nodeSkillIcon.transform.Find("txConsumeDes").GetComponent<TMP_Text>();
        imgConsume = nodeSkillIcon.transform.Find("imgConsume").GetComponent<Image>();
        txConsume = imgConsume.transform.Find("txConsume").GetComponent<TMP_Text>();

        nodeSkillDes = nodeContent.transform.Find("nodeSkillDes").GetComponent<RectTransform>();
        nodeSkillName = nodeSkillDes.transform.Find("nodeSkillName").GetComponent<RectTransform>();
        imgSkillEle = nodeSkillName.transform.FindHideInChild("imgSkillEle").GetComponent<Image>();
        txSkillName = nodeSkillName.transform.Find("txSkillName").GetComponent<TMP_Text>();
        txSkillDes = nodeSkillDes.transform.Find("txSkillDes").GetComponent<TMP_Text>();

        nodeSkillExt = nodeContent.transform.Find("nodeSkillExt").GetComponent<RectTransform>();
        nodeSkillExtEmpty = nodeSkillExt.transform.Find("empty").GetComponent<RectTransform>();
        ori_extra = nodeSkillExt.transform.FindHideInChild("ori_extra").gameObject;
    }

    /*
     <-显示算力消耗、技能简介	
	
    <-显示技能元素（若无元素则不显示）；显示技能名称	
	
    <-显示技能详细描述	
	
	
    <-若无额外加成则不显示	
	
    <-显示与该技能相关的被动或者天赋	
    <-若是天赋，则显示为星级图标，已激活使用彩色显示，未激活使用灰色显示	
    <-若是被动，则显示为突破图标，已激活使用彩色显示，未激活使用灰色显示	
	
    信息读取自t_skill.relationskills字段	
    该字段中会填写与本技能相关的天赋和被动ID	
    根据填写的ID可以索引到以下信息：	
    加成的类型（来自天赋还是被动），若是天赋显示星级图标	
    天赋/被动的名称	
    天赋/被动的描述	

     */
    public void UpdateFadeBody(int lorr)
    {
        if (!isInit) return;
        // left-> x=153, right-> x=-159
        float heroIconXAnchorPos = 0;//153;

        if (lorr > 0)
        {
            // right
            heroIconXAnchorPos = 0;//-159;
            m_HeroIcon.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            m_HeroIcon.transform.localScale = new Vector3(1, 1, 1);
        }
        m_HeroIconRt.anchoredPosition = new Vector2(heroIconXAnchorPos, m_HeroIconRt.anchoredPosition.y);
    }

    private async void SetUIData()
    {
        if (m_CurrHeroInfoCfgData == null) 
        {
            return;
        }
        // 立绘
        m_HeroIcon.sprite = await SpriteManager.Instance.GetTextureSpriteSync($"fadebody/hero_fadebody_{m_CurrHeroInfoCfgData.heroid}_01");
        //UpdateFadeBody(0);

        // 技能元素 0 = 无元素 1 = 水 2 = 火 3 = 风 4 = 雷
        // 元素属性  1 = 水 2 = 火 3 = 风 4 = 雷
        m_HeroBg.sprite = SpriteManager.Instance.GetSpriteSync(commontool.GetHeroInfoBgResName(m_CurrHeroInfoCfgData.element));


        /// -- skillType
        skillType_imgEle.gameObject.SetActive(m_CurrBattleSkillCfg.skillelement != 0);
        // 技能类型1 = 普攻 2 = 战技 3 = 秘技 4 = 终结技
        //  Sprite skillIconType = commontool.GetSkillTypeDesIcon(m_CurrBattleSkillCfg.skilltype);
        string skillTypeDes = m_CurrBattleSkillCfg.skilltype == 1 ? "普攻" :
                                (m_CurrBattleSkillCfg.skilltype == 2 ? "战技" :
                                (m_CurrBattleSkillCfg.skilltype == 3 ? "秘技" : "终结技"));
        skillType_txSkillType.SetTextExt(skillTypeDes);
        if (m_CurrBattleSkillCfg.skillelement == 0)
        {
            skillType_txSkillType.GetComponent<RectTransform>().anchoredPosition = new Vector2(47f, 0.7f);
        }
        else
        {
            skillType_txSkillType.GetComponent<RectTransform>().anchoredPosition = new Vector2(73.5f, 0.7f);
            skillType_imgEle.sprite = commontool.GetYuansuIcon(m_CurrBattleSkillCfg.skillelement);
        }

        /// -- skill icon
        imgSkillIcon.sprite = SpriteManager.Instance.GetSpriteSync(m_CurrBattleSkillCfg.icon);
        imgSkillQuality.sprite = commontool.GetQualityBarIcon(m_CurrHeroInfoCfgData.quality);
        //txSkillIconType
        txSkillIconType.SetText(GameCenter.mIns.m_LanMgr.GetLan(m_CurrBattleSkillCfg.note1));
        txConsumeDes.SetText("算力消耗");
        txConsume.SetText(m_CurrBattleSkillCfg.energycost.ToString());

        /// -- Skill Des
        // imgSkillEle
        txSkillName.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(m_CurrBattleSkillCfg.name));

        int skillLv = baseHero.GetSkilllVBySkillID(m_CurrBattleSkillCfg.skillid);
        txSkillDes.SetTextExt(BattleCfgManager.Instance.GetSkillCfgNoteValue(m_CurrBattleSkillCfg, skillLv));


        /// -- nodeSkillExt
        // m_TxExtraAddTitle.SetTextExt("额外加成");
        ClearExtraAddItems();
        int extCount = 0;
        if (string.IsNullOrEmpty(m_CurrBattleSkillCfg.relationskills))
        {
            nodeSkillExtEmpty.gameObject.SetActive(true);
        }
        else
        {
            string[] idSplit = m_CurrBattleSkillCfg.relationskills.Split('|');
            extCount = idSplit.Length;
            if (idSplit.Length >= 5)
            {
                nodeSkillExtEmpty.gameObject.SetActive(false);
            }
            for (int i = 0; i < idSplit.Length; i++)
            {
                GameObject eObj = GameObject.Instantiate(ori_extra);
                eObj.transform.SetParent(ori_extra.transform.parent);
                eObj.transform.localPosition = Vector3.zero;
                eObj.transform.localScale = Vector3.one;
                eObj.name = "extra_" + i;
                ExtraAddItem extraAddItem = new ExtraAddItem(eObj, m_CurrBattleSkillCfg);
                extraAddItems.Add(extraAddItem);
                extraAddItem.SetActive(true, long.Parse(idSplit[i]));
                nodeSkillExtEmpty.transform.SetAsLastSibling();
            }
        }

        // ˢ��
        LayoutRebuilder.ForceRebuildLayoutImmediate(nodeSkillExt);
        LayoutRebuilder.ForceRebuildLayoutImmediate(nodeSkillName);
        LayoutRebuilder.ForceRebuildLayoutImmediate(nodeSkillDes);
        LayoutRebuilder.ForceRebuildLayoutImmediate(skillIcon);
        LayoutRebuilder.ForceRebuildLayoutImmediate(nodeSkillIcon);
        LayoutRebuilder.ForceRebuildLayoutImmediate(nodeContent);

        // 延迟再刷一下
        GameCenter.mIns.RunWaitCoroutine(() => {
            LayoutRebuilder.ForceRebuildLayoutImmediate(nodeSkillExt);
            LayoutRebuilder.ForceRebuildLayoutImmediate(nodeSkillName);
            LayoutRebuilder.ForceRebuildLayoutImmediate(nodeSkillDes);
            LayoutRebuilder.ForceRebuildLayoutImmediate(skillIcon);
            LayoutRebuilder.ForceRebuildLayoutImmediate(nodeSkillIcon);
            LayoutRebuilder.ForceRebuildLayoutImmediate(nodeContent);

            // bg 初始min=699 二个天赋->966 三个天赋max=1266
            float y = extCount == 2 ? 966 : (extCount >= 3 ? 1266 : 699);
            m_BgMengBan_1.sizeDelta = new Vector2(m_BgMengBan_1.sizeDelta.x, y);
        }, 0.05f);

        isInit = true;
    }

    private void ClearExtraAddItems()
    {
        if (extraAddItems != null)
        {
            foreach (var item in extraAddItems)
            {
                item.Destroy();
            }
            extraAddItems.Clear();
        }
        else
        {
            extraAddItems = new List<ExtraAddItem>();
        }
    }

    public void Clear()
    {
        isInit = false;
        ClearExtraAddItems();
    }
}

public class ExtraAddItem
{
    public GameObject obj;
    public Image imgExtraAddItemType;
    public TMP_Text txExtraAddItemName;
    public TMP_Text txExtraAddItemDes;
    public BattleSkillCfg battleSkillCfg;

    public ExtraAddItem(GameObject obj, BattleSkillCfg battleSkillCfg)
    {
        this.obj = obj;
        this.battleSkillCfg = battleSkillCfg;

        imgExtraAddItemType = obj.transform.Find("nodeTitle/imgExtraAddItemType").GetComponent<Image>();
        txExtraAddItemName = obj.transform.Find("nodeTitle/txExtraAddItemName").GetComponent<TMP_Text>();
        txExtraAddItemDes = obj.transform.Find("txExtraAddItemDes").GetComponent<TMP_Text>();

        txExtraAddItemName.SetTextExt("");
        txExtraAddItemDes.SetTextExt("");
    }

    public void SetActive(bool bActive, long id = 0)
    {
        if (id != 0)
        {
            BattleSkillTalentCfg battleSkillTalentCfg = BattleCfgManager.Instance.GetTalentCfg(id);
            if (battleSkillTalentCfg != null)
            {
                txExtraAddItemName.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(battleSkillTalentCfg.name));
                txExtraAddItemDes.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(battleSkillTalentCfg.note));

                // ui_c_icon_shengxing_cheng
                // ui_c_icon_shengxing_hui

                // ui_d_icon_xingxing_jin
                // ui_d_icon_xingxing_bai

                int iType = (int)(battleSkillTalentCfg.talentid % 100);
                if (iType <= 10)
                {
                    // �츳
                    imgExtraAddItemType.sprite = SpriteManager.Instance.GetSpriteSync("ui_c_icon_shengxing_cheng");
                }
                else
                {
                    // ����
                    imgExtraAddItemType.sprite = SpriteManager.Instance.GetSpriteSync("ui_d_icon_xingxing_jin");
                }
            }
        }

        if (obj != null)
        {
            obj.SetActive(bActive);
        }
    }

    public void Destroy()
    {
        if (obj != null)
        {
            GameObject.Destroy(obj);
        }
    }
}
