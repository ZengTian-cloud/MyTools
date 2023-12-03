using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using static HeroGrow;
/// <summary>
/// 英雄技能树
/// </summary>
public class HeroSkillTree
{
    public HeroSkillPanel skillPanel;

    public HeroData hero;
    public HeroInfoCfgData heroInfo;
    private Dictionary<int, GameObject> dicTreePoint;
    private List<long> passivityIds;
    private float duration;

    public GameObject treeRoot;

    public GameObject curSelctBox;//当前选中框
    public long curSkillID;

    public HeroSkillTree(GameObject tree, HeroData hero, HeroInfoCfgData heroInfo, float duration,HeroSkillPanel skillPanel)
	{
        this.treeRoot = tree;
        this.hero = hero;
        this.heroInfo = heroInfo;
        this.duration = duration;
        this.skillPanel = skillPanel;

        dicTreePoint = new Dictionary<int, GameObject>();
        for (int i = 1; i <= 16; i++)
        {
            if (i <= 4 || i >= 11) 
            {
                dicTreePoint.Add(i, treeRoot.transform.Find($"skill_{i}").gameObject);
            }
        }
        //所有被动
        passivityIds = heroInfo.GetAllPassivityByHeroID(hero.heroID);
    }

    public void refreshData(GameObject tree, HeroData hero, HeroInfoCfgData heroInfo, float duration, HeroSkillPanel skillPanel)
    {
        this.treeRoot = tree;
        this.hero = hero;
        this.heroInfo = heroInfo;
        this.duration = duration;
        this.skillPanel = skillPanel;

        dicTreePoint = new Dictionary<int, GameObject>();
        for (int i = 1; i <= 16; i++)
        {
            if (i <= 4 || i >= 11)
            {
                dicTreePoint.Add(i, treeRoot.transform.Find($"skill_{i}").gameObject);
            }
        }

        passivityIds = heroInfo.GetAllPassivityByHeroID(hero.heroID);
    }

    public void show()
    {
        if (curSelctBox!=null)
        {
            curSelctBox.SetActive(false);
        }
        curSelctBox = null;
        RefreshAllSkill();
        
    }

    public void RefreshAllSkill()
    {
        foreach (var item in dicTreePoint)
        {
            item.Value.SetActive(false);
        }
        InitActiveSkill();
        InitPassiveSkill();
    }

    /// <summary>
    /// 初始化主动技能信息
    /// </summary>
    private void InitActiveSkill()
    {
        for (int i = 1; i <= 4; i++)
        {
            RefreshOneItemByActiveSkill(i);
        }
    }

    /// <summary>
    /// 初始化被动
    /// </summary>
    private void InitPassiveSkill()
    {
        for (int i = 11; i <= 16; i++)
        {
            RefreshOneItemByPassiveSkill(i);
        }
    }



    /// <summary>
    /// 刷新主动技能的单个节点
    /// </summary>
    public void RefreshOneItemByActiveSkill(int skillType)
    {
        if (dicTreePoint.ContainsKey(skillType))
        {
            long skillID = GetSkillIDByType(skillType);
            if (skillID <= 0) 
            {
                return;
            }
            HeroSkillTreeCfg treeCfg = GameCenter.mIns.m_CfgMgr.GetHeroSkillTreeCfg(hero.heroID, skillID);

            BattleSkillCfg skillCfg = BattleCfgManager.Instance.GetSkillCfgBySkillID(skillID);
            GameObject item = dicTreePoint[treeCfg.slotid];
            //item.transform.Find("select").gameObject.SetActive(false);
            item.SetActive(true);
            if (!string.IsNullOrEmpty(skillCfg.icon))
            {
                item.transform.Find("skillIcon").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(skillCfg.icon);
            }
            else
            {
                Debug.LogError($"未找到skillid为：{skillID}的技能图片资源，请检查！");
            }
            int curLv = hero.GetSkillLvBySkillID(skillID);
            int maxLv = GetSkillMaxLevel(hero.state);
            item.transform.Find("level/lvText").GetComponent<TextMeshProUGUI>().text = $"{curLv}/{maxLv}";
            item.GetComponent<Button>().onClick.RemoveAllListeners();
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (curSelctBox != null)
                {
                    curSelctBox.SetActive(false);
                }
                curSkillID = skillID;
                curSelctBox = item.transform.Find("select").gameObject;
                curSelctBox.SetActive(true);
                GoToSkillLevelUpPanel(skillCfg);
            });
        }
    }

    /// <summary>
    /// 刷新被动技能的单个节点
    /// </summary>
    public void RefreshOneItemByPassiveSkill(int skillType)
    {
        if (dicTreePoint.ContainsKey(skillType))
        {

            long skillID = GetSkillIDByType(skillType);
            if (skillID <= 0)
            {
                return;
            }
            HeroSkillTreeCfg treeCfg = GameCenter.mIns.m_CfgMgr.GetHeroSkillTreeCfg(hero.heroID, skillID);
            if (treeCfg.slotid <= 0)
            {
                Debug.LogError($"技能树表错误配置,未找到{treeCfg.slotid}槽位：heroID:{hero.heroID},skillid:{skillID},slotid:{treeCfg.slotid} ");
                return;
            }
            GameObject item = dicTreePoint[treeCfg.slotid];
            //item.transform.Find("select").gameObject.SetActive(false);
            item.SetActive(true);
            BattleSkillTalentCfg talentCfg = BattleCfgManager.Instance.GetTalentCfg(skillID);



  
            //被动当前等级 0=可解锁 1=已解锁 -1=未解锁
            int curLv = hero.GetSkillLvBySkillID(skillID);
            if (curLv == -1 || curLv == 0)
            {
                if (treeCfg.shape == 2)
                {
                    item.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_d_pnl_jinengdian_hui");
                }
                else
                {
                    item.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_d_pnl_jinengdian01_hui");
                }
                
            }
            else
            {
                if (treeCfg.shape == 2)
                {
                    item.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_d_pnl_jinengdian_liang");
                }
                else
                {
                    item.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_d_pnl_jinengdian01_liang");
                }
                    
            }
            if (treeCfg.shape == 2)
            {
                item.transform.Find("select").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_d_pnl_jinengdian_xuanzhong");
            }
            else
            {
                item.transform.Find("select").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_d_pnl_jinengdian01_xuanzhong");
            }
            item.GetComponent<Button>().onClick.RemoveAllListeners();
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (curSelctBox != null)
                {
                    curSelctBox.SetActive(false);
                }
                curSkillID = skillID;
                curSelctBox = item.transform.Find("select").gameObject;
                curSelctBox.SetActive(true);
                GoToSkillLevelUpPanel(talentCfg);
            });

        }
    }


    /// <summary>
    /// 根据技能类型获得对应技能id
    /// </summary>
    private long GetSkillIDByType(int skillType)
    {
        switch (skillType)
        {
            case 1://普通攻击
                return heroInfo.baseskill;
            case 2://战技
                return heroInfo.cardskill1;
            case 3://秘技
                return heroInfo.cardskill2;
            case 4://终结技
                return heroInfo.cardskill3;
            default://被动 11-16
                return passivityIds.Find(id => id % 100 == skillType);
        }
    }

    private int GetSkillMaxLevel(int heroState)
    {
        if (heroState < 1)
        {
            return 3;
        }
        else
        {
            return 3 + heroState * 2;
        }
    }

    /// <summary>
    /// 前往技能升级界面
    /// </summary>
    private void GoToSkillLevelUpPanel(BattleSkillCfg skillCfg)
    {
        HeroGrowUtils.backType = BackType.Jn;
        skillPanel.RefreshSkillInfoPanel(skillCfg);
        skillPanel.leftMenu.hide();
        skillPanel.skill_view.GetComponent<RectTransform>().DOAnchorPosX(-450, duration);
        skillPanel.panel_skill_Info.SetActive(true);
        skillPanel.panel_skill_Info.GetComponent<RectTransform>().DOAnchorPosX(-350, duration);
    }

    /// <summary>
    /// 前往技能升级界面
    /// </summary>
    private void GoToSkillLevelUpPanel(BattleSkillTalentCfg talentCfg)
    {
        HeroGrowUtils.backType = BackType.Jn;
        skillPanel.RefreshSkillInfoPanel(talentCfg);
        skillPanel.leftMenu.hide();
        skillPanel.skill_view.GetComponent<RectTransform>().DOAnchorPosX(-450, duration);
        skillPanel.panel_skill_Info.SetActive(true);
        skillPanel.panel_skill_Info.GetComponent<RectTransform>().DOAnchorPosX(-350, duration);
    }
}

