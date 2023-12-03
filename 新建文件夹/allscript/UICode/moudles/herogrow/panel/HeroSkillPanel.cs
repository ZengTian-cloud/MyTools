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
using System.Text.RegularExpressions;

/// <summary>
/// 技能面板
/// </summary>
public class HeroSkillPanel : IHeroPanel
{

    public HeroData hero;
    public HeroInfoCfgData heroInfo;
    private long curSkillID;

    private Transform parent;
    private float duration;

    private GameObject curSkillTree;//当前显示的技能树
    private HeroSkillTree SkillTree;

    public HeroLeftMenu leftMenu = null;

    public GameObject skill_view;//技能树节点
    private GameObject skill_view_diyu;//抵御职业技能树
    private GameObject skill_view_jianmie;//歼灭职业技能树
    private GameObject skill_view_zhiyuan;//支援职业技能树
    private GameObject skill_view_zhanshu;//战术职业技能树

    public GameObject panel_skill_Info;//技能信息面板
    private TextMeshProUGUI name_text;//技能名字
    private Image expense_bg;//算力消耗背景
    private TextMeshProUGUI expense_text;//算力消耗
    private TextMeshProUGUI level_text;//技能等级
    private Button btn_nextLevel;//下一级
    private TextMeshProUGUI text_skillType;//技能类型
    private TextMeshProUGUI text_skillTag;//技能标签
    private TextMeshProUGUI text_des;//技能描述
    private GameObject expend_bg;
    private GameObject expendList;//材料列表
    private GameObject expend_item;//材料对象
    private GameObject text_cost;//消耗
    private Button btn_upLevel;//按钮
    private TextMeshProUGUI text_costDes;//消耗{0}

    private TextMeshProUGUI levelText_lan;//等级-固定读取多语言文本
    private TextMeshProUGUI expend_bg_text;//消耗材料-固定读取多语言文本

    private Dictionary<int, GameObject> dicSkillTree;//技能树字典 key-职业1=歼灭 2=战术 3=支援 4=抵御 value-预制体

    private List<GameObject> expendItems = new List<GameObject>();//升级消耗材料的item预制体列表

    public HeroSkillPanel(Transform parent, HeroLeftMenu leftMenu, float duration)
    {
        this.parent = parent;
        this.leftMenu = leftMenu;
        this.hero = leftMenu.getSelectHero().data;
        this.heroInfo = leftMenu.getSelectHero().heroInfo;
        this.duration = duration;

        skill_view = parent.transform.Find("skill_view").gameObject;
        skill_view_diyu = skill_view.transform.Find("skill_view_diyu").gameObject;
        skill_view_jianmie = skill_view.transform.Find("skill_view_jianmie").gameObject;
        skill_view_zhiyuan = skill_view.transform.Find("skill_view_zhiyuan").gameObject;
        skill_view_zhanshu = skill_view.transform.Find("skill_view_zhanshu").gameObject;

        panel_skill_Info = parent.transform.Find("panel_skill_Info").gameObject;
        name_text = panel_skill_Info.transform.Find("top/name_text").GetComponent<TextMeshProUGUI>();
        expense_bg = panel_skill_Info.transform.Find("top/expense_bg").GetComponent<Image>();
        expense_text = expense_bg.transform.Find("expense_text").GetComponent<TextMeshProUGUI>();
        level_text = panel_skill_Info.transform.Find("level_bg/level_text").GetComponent<TextMeshProUGUI>();
        btn_nextLevel = panel_skill_Info.transform.Find("level_bg/btn_nextLevel").GetComponent<Button>();
        text_skillType = panel_skill_Info.transform.Find("desc/text_skillType").GetComponent<TextMeshProUGUI>();
        text_skillTag = panel_skill_Info.transform.Find("desc/text_skillTag").GetComponent<TextMeshProUGUI>();
        text_des = panel_skill_Info.transform.Find("desc/text_des").GetComponent<TextMeshProUGUI>();
        expend_bg = panel_skill_Info.transform.Find("expend_bg").gameObject;
        expendList = panel_skill_Info.transform.Find("expendList").gameObject;
        expend_item = expendList.transform.Find("expend_item").gameObject;
        text_cost = panel_skill_Info.transform.Find("text_cost").gameObject;
        btn_upLevel = panel_skill_Info.transform.Find("btn_upLevel").GetComponent<Button>();
        text_costDes = panel_skill_Info.transform.Find("text_costDes").GetComponent<TextMeshProUGUI>();

        levelText_lan = panel_skill_Info.transform.Find("level_bg/text").GetComponent<TextMeshProUGUI>();
        expend_bg_text = panel_skill_Info.transform.Find("expend_bg/text").GetComponent<TextMeshProUGUI>();

        dicSkillTree = new Dictionary<int, GameObject>();
        dicSkillTree.Add(1, skill_view_jianmie);
        dicSkillTree.Add(2, skill_view_zhanshu);
        dicSkillTree.Add(3, skill_view_zhiyuan);
        dicSkillTree.Add(4, skill_view_diyu);
        InitDefultText();

        expend_item.SetActive(false);

        btn_upLevel.AddListenerBeforeClear(OnLevelUpBtnClick);

        leftMenu.setChangeHeroSelect((data, heroInfoData) => {
            this.hero = data;
            this.heroInfo = heroInfoData;
            ShowSkillTree(this.heroInfo.profession);
        });

        //LoadEffect();
    }

    public void close()
    {
        //skill_view.GetComponent<RectTransform>().DOAnchorPosX(1800, duration);
        panel_skill_Info.GetComponent<RectTransform>().DOAnchorPosX(400, duration);
        if (effect!= null)
        {
            GameObject.Destroy(effect.gameObject);
        }
        parent.gameObject.SetActive(false);
    }

    public void show()
    {
        parent.gameObject.SetActive(true);
        //skill_view.GetComponent<RectTransform>().DOAnchorPosX(288, duration);
        parent.GetComponent<RectTransform>().DOAnchorPosX(0, duration);
        ShowSkillTree(this.heroInfo.profession);

    }


    /// <summary>
    /// 初始化技能树面板
    /// </summary>
    /// <param name="type">职业类型</param>
    private void ShowSkillTree(int type)
    {
        foreach (var item in dicSkillTree)
        {
            if (item.Key == type)
            {
                curSkillTree = item.Value;
            }
            item.Value.SetActive(item.Key == type);
        }
        RefreshSkillTree(curSkillTree, duration);
    }

    /// <summary>
    /// 刷新技能树
    /// </summary>
    /// <param name="skillTree"></param>
    private void RefreshSkillTree(GameObject skillTree, float duration)
    {
        if (SkillTree == null)
        {
            SkillTree = new HeroSkillTree(skillTree, hero, heroInfo, duration, this);
        }
        else
        {
            SkillTree.refreshData(skillTree, hero, heroInfo, duration, this);
        }
        SkillTree.show();
    }


    /// <summary>
    /// 初始化默认文本
    /// </summary>
    private void InitDefultText()
    {
        btn_nextLevel.SetText(GameCenter.mIns.m_LanMgr.GetLan("common_nextLevel"));
        expend_bg_text.text = GameCenter.mIns.m_LanMgr.GetLan("common_expend_1");
        levelText_lan.text = GameCenter.mIns.m_LanMgr.GetLan("common_level");

    }

    /// <summary>
    /// 刷新主动技能技能详情面板
    /// </summary>
    public void RefreshSkillInfoPanel(BattleSkillCfg skillCfg)
    {
        this.curSkillID = skillCfg.skillid;
        int skillLv = hero.GetSkillLvBySkillID(skillCfg.skillid);
        name_text.text = GameCenter.mIns.m_LanMgr.GetLan(skillCfg.name);
        expense_text.text = skillCfg.energycost.ToString();
        level_text.gameObject.SetActive(true);
        level_text.text = skillLv.ToString();
        switch (skillCfg.skilltype)
        {
            case 1://普通攻击
                text_skillType.text = GameCenter.mIns.m_LanMgr.GetLan("common_baseskill");
                break;
            case 2://战技
                text_skillType.text = GameCenter.mIns.m_LanMgr.GetLan("common_skill1");
                break;
            case 3://秘技
                text_skillType.text = GameCenter.mIns.m_LanMgr.GetLan("common_skill2");
                break;
            case 4://终结技
                text_skillType.text = GameCenter.mIns.m_LanMgr.GetLan("common_skill3");
                break;
        }
        string skilltag = GameCenter.mIns.m_LanMgr.GetLan(skillCfg.note1);
        if (string.IsNullOrEmpty(skilltag))
        {
            text_skillTag.gameObject.SetActive(false);
        }
        else
        {
            text_skillTag.gameObject.SetActive(true);
            text_skillTag.text = $"【{skilltag}】";
        }


        text_des.text = BattleCfgManager.Instance.GetSkillCfgNoteValue(skillCfg, skillLv);
        RefreshUpLevelCost(skillCfg.skillid);

        panel_skill_Info.GetComponent<RectTransform>().DOAnchorPosX(400, duration).From();
    }

    /// <summary>
    /// 刷新被动技能技能详情面板
    /// </summary>
    public void RefreshSkillInfoPanel(BattleSkillTalentCfg talentCfg)
    {
        this.curSkillID = talentCfg.talentid;

        name_text.text = GameCenter.mIns.m_LanMgr.GetLan(talentCfg.name);
        //expense_text.text = talentCfg.energycost.ToString();
        level_text.gameObject.SetActive(false);
        text_skillType.text = GameCenter.mIns.m_LanMgr.GetLan("common_passiveSkill");
        //text_skillTag.text = $"【{GameCenter.mIns.m_LanMgr.GetLan(talentCfg.note1)}】";
        text_des.text = GameCenter.mIns.m_LanMgr.GetLan(talentCfg.note);
        RefreshUpLevelCost(talentCfg.talentid);

        panel_skill_Info.GetComponent<RectTransform>().DOAnchorPosX(400, duration).From();
    }

    /// <summary>
    /// 刷新技能升级消耗
    /// </summary>
    public void RefreshUpLevelCost(long skillID)
    {
        for (int i = 0; i < expendItems.Count; i++)
        {
            GameObject.Destroy(expendItems[i]);
        }
        expendItems.Clear();

        //技能类型
        int skillType = (int)skillID % 100;
        //技能等级
        int curLv = hero.GetSkillLvBySkillID(skillID);

        bool bGlod = false;//是否消耗金币
        bool bOther = false;//是否消耗其他材料
        //所有等级的消耗
        List<HeroSkillUpCostCfg> allCostCfg = GameCenter.mIns.m_CfgMgr.GetAllHeroSkillUpCostCfgBySkillID(skillID);
        HeroSkillUpCostCfg costCfg;
        if (curLv == -1)//不可解锁的被动 取0级配置
        {
             costCfg = GameCenter.mIns.m_CfgMgr.GetHeroSkillUpCostCfgBySkillIDAndSkillLevel(skillID, 0);
        }
        else
        {
             costCfg = GameCenter.mIns.m_CfgMgr.GetHeroSkillUpCostCfgBySkillIDAndSkillLevel(skillID, curLv);
        }
        
        if (costCfg != null) 
        {
            //是否是最大等级
            bool isMax = CheckIsMaxLevel(allCostCfg, costCfg);
            //材料展示
            expend_bg.SetActive(!isMax);
            expendList.SetActive(!isMax);
            if (!isMax)
            {
                //刷新材料花费列表
                List<CostData> costDatas = GameCenter.mIns.m_CfgMgr.GetCostByCostID(costCfg.costid).getCosts();
                for (int i = 0; i < costDatas.Count; i++)
                {
                    if (costDatas[i].propid == 801)
                    {
                        bGlod = true;
                        text_cost.GetComponentInChildren<TextMeshProUGUI>().text = costDatas[i].count.ToString();
                    }
                    else
                    {
                        bOther = true;
                        GameObject item = GameObject.Instantiate(expend_item, expendList.transform);
                        expendItems.Add(item);
                    }
                }
                text_cost.SetActive(bGlod);
                text_costDes.gameObject.SetActive(bGlod);
                expend_bg.SetActive(bOther);
                expendList.SetActive(bOther);

                //判断等级是否足够
                if (skillType >= 11) //被动技能
                {
                    commontool.SetGary(btn_upLevel.GetComponent<Image>(), curLv != 0);
                    btn_upLevel.interactable = curLv == 0;
                    if (curLv == -1)//不可解锁
                    {
                        text_cost.SetActive(false);
                        text_costDes.gameObject.SetActive(false);
                        int unlockState = heroInfo.GetUnLockTanlentState(skillID);
                        btn_upLevel.GetComponentInChildren<TextMeshProUGUI>().text = string.Format(GameCenter.mIns.m_LanMgr.GetLan("grow_unlockDes_1"), unlockState);
                    }
                    else if (curLv == 0)//可解锁
                    {
                        btn_upLevel.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("common_unlock_1");
                    }
                }
                else//主动技能
                {
                    int state = GameCenter.mIns.m_CfgMgr.GetUnLockActiveSkillState(skillID, curLv);
                    if (hero.state >= state)//可升级
                    {
                        commontool.SetGary(btn_upLevel.GetComponent<Image>(), false);
                        btn_upLevel.interactable = true;
                        btn_upLevel.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("common_levelUp");
                    }
                    else
                    {
                        commontool.SetGary(btn_upLevel.GetComponent<Image>(), true);
                        btn_upLevel.interactable = false;
                        btn_upLevel.GetComponentInChildren<TextMeshProUGUI>().text = string.Format(GameCenter.mIns.m_LanMgr.GetLan("grow_unlockDes_1"), state);
                    }
                }
            }
            else//满级
            {
                text_cost.SetActive(false);
                text_costDes.gameObject.SetActive(false);
                //按钮置灰
                commontool.SetGary(btn_upLevel.GetComponent<Image>(), true);
                btn_upLevel.interactable = false;
                if (skillType >= 11)//被动技能
                {
                    btn_upLevel.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("common_unlock");
                }
                else
                {
                    btn_upLevel.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("grow_upLevelMax");
                }

            }
        }
        else
        {
            Debug.LogError($"未找到{skillID}的{curLv}级的升级消耗配置！");
        }
    }

    /// <summary>
    /// 点击升级按钮事件回调
    /// </summary>
    public void OnLevelUpBtnClick()
    {
        JsonData data = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        data["heroid"] = this.hero.heroID;
        data["skillid"] = this.curSkillID;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.GROW_HERO_SKILLUP, data, (seqid, code, data) =>
        {
            if (code == 0)
            {
                JsonData json = JsonMapper.ToObject(new JsonReader(data));
                JsonData chagedata = json["change"]?["changed"];
                if(chagedata != null)
                {
                    GameCenter.mIns.userInfo.onChange(chagedata);
                    GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                    {
                        int skillType = (int)this.curSkillID % 100;
                        if (skillType >= 11)//被动技能
                        {
                            SkillTree.RefreshOneItemByPassiveSkill(skillType);
                            RefreshSkillInfoPanel(BattleCfgManager.Instance.GetTalentCfg(curSkillID));
                        }
                        else {
                            SkillTree.RefreshOneItemByActiveSkill(skillType);
                            RefreshSkillInfoPanel(BattleCfgManager.Instance.GetSkillCfgBySkillID(curSkillID));
                        }

                    });
                }
            }
        });
    }

    /// <summary>
    /// 判断技能是否最大等级
    /// </summary>
    public bool CheckIsMaxLevel(List<HeroSkillUpCostCfg> allCostCfg, HeroSkillUpCostCfg curCostCfg)
    {
        for (int i = allCostCfg.Count - 1; i >= 0; i--) 
        {
            if (allCostCfg[i].level > curCostCfg.level)
            {
                return false;
            }
        }
        return true;
    }



    /// <summary>
    /// 从升级界面返回到技能界面
    /// </summary>
    public void BackSkillPanelByUpLevel()
    {
        panel_skill_Info.GetComponent<RectTransform>().DOAnchorPosX(400, duration);
        skill_view.GetComponent<RectTransform>().DOAnchorPosX(288, duration);
        leftMenu.show();
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

        UIEffectManager.Instance.LoadUIEffect("effs_ui_qtlz_01", (go) =>
        {
            effect = go.AddComponent<UIParticleEffect>();

            effect.SetUP(new UIParticleEffect.ShowInfo()
            {
                _offset = -5,

                _canvas = parent.GetComponentInParent<Canvas>(),
            });
            go.transform.SetParent(skill_view.transform, false);

            go.transform.localPosition = Vector3.zero;
        });
    }
}
