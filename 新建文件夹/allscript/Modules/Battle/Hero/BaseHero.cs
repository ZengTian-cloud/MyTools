using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 战斗内使用 英雄数据
/// </summary>
public class BaseHero:BaseObject
{
    //控制器数据
    public HeroController Controller { get; private set; }
    //配置数据
    public HeroInfoCfgData cfgdata { get; private set; }
    //英雄数据
    public HeroData heroData { get; private set; }


    #region 人物普攻、主动技、天赋、被动属性定义，战中可被替换，需要用属性索引
    //普通攻击
    public long baseSkill { get; private set; }
    //普通攻击等级
    public int baseSkillLV { get; private set; }
    //战技
    public long skill1 { get; private set; }
    //战技等级
    public int skill1LV { get; private set; }
    //秘技
    public long skill2 { get; private set; }
    //秘技等级
    public int skill2LV { get; private set; }
    //终结技
    public long skill3 { get; private set; }
    //终结技等级
    public int skill3LV { get; private set; }

    //天赋技能 
    public long talent1 { get; private set; }
    public long talent2 { get; private set; }
    public long talent3 { get; private set; }
    public long talent4 { get; private set; }
    public long talent5 { get; private set; }
    public long talent6 { get; private set; }

    //天赋技能等级
    public int talent1LV { get; private set; }
    public int talent2LV { get; private set; }
    public int talent3LV { get; private set; }
    public int talent4LV { get; private set; }
    public int talent5LV { get; private set; }
    public int talent6LV { get; private set; }

    //被动技能
    public long passive1 { get; private set; }
    public long passive2 { get; private set; }
    public long passive3 { get; private set; }
    public long passive4 { get; private set; }
    public long passive5 { get; private set; }
    public long passive6 { get; private set; }

    //被动技能等级
    public int passive1LV { get; private set; }
    public int passive2LV { get; private set; }
    public int passive3LV { get; private set; }
    public int passive4LV { get; private set; }
    public int passive5LV { get; private set; }
    public int passive6LV { get; private set; }
    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="heroData">玩家英雄数据</param>
    /// <param name="Controller">英雄控制器</param>
    /// <param name="cfgdata">英雄配置数据</param>
    /// <param name="root">英雄节点</param>
    /// <param name="hero">英雄模型</param>
    /// <param name="guid"></param>
    public BaseHero(HeroData heroData, HeroController Controller, HeroInfoCfgData cfgdata,GameObject root,GameObject hero)
    {
        this.heroData = heroData;
        this.Controller = Controller;
        this.cfgdata = cfgdata;
        this.prefabObj = root;
        this.roleObj = hero;
        this.GUID = this.heroData.heroID;

        this.objType = 1;
        this.objID = this.heroData.heroID;
        this.lv = this.heroData.level;
        this.battleAttr = new Dictionary<long, float>();
        this.bRecycle = false;
        bdie = false;
        foreach (var item in this.heroData.attrs)
        {
            this.battleAttr.Add(item.Key, item.Value);
        }
        this.curHp = GetBattleAttr(EAttr.HP);

        this.baseSkill = this.cfgdata.baseskill;
        this.baseSkillLV = this.heroData.GetSkillLvBySkillID(this.baseSkill);
        this.skill1 = this.cfgdata.cardskill1;
        this.skill1LV = this.heroData.GetSkillLvBySkillID(this.skill1);
        this.skill2 = this.cfgdata.cardskill2;
        this.skill2LV = this.heroData.GetSkillLvBySkillID(this.skill2);
        this.skill3 = this.cfgdata.cardskill3;
        this.skill3LV = this.heroData.GetSkillLvBySkillID(this.skill3);


        InitTalent();
    }





    //-----------------------------------------------------------------------------
    public BattleSkillCfg baseSkillCfgData;//普通攻击配置表数据

    //-----------------------------------------------------------------------------
    public BattleSkillCfg skill1CfgData;//战技技能配置表数据

    //-----------------------------------------------------------------------------
    public BattleSkillCfg skill2CfgData;//秘技技能配置表数据


    //-----------------------------------------------------------------------------
    public BattleSkillCfg skill3CfgData;//终结技技能配置表数据


    /// <summary>
    /// 战斗内获取技能等级
    /// </summary>
    /// <param name="skillID"></param>
    public int GetSkilllVBySkillID(long skillID)
    {
        BattleSkillCfg skillCfg = BattleCfgManager.Instance.GetSkillCfgBySkillID(skillID);
        switch (skillCfg.skilltype)
        {
            case 1:
                return this.baseSkillLV;
            case 2:
                return this.skill1LV;
            case 3:
                return this.skill2LV;
            case 4:
                return this.skill3LV;
            default:
                return -1;
        }
    }

    /// <summary>
    /// 初始化英雄技能配置数据
    /// </summary>
    public void InitSkillCfg()
    {

        #region 普通攻击
        //if (baseSkillCfgData == null)
        {
            baseSkillCfgData = BattleCfgManager.Instance.GetSkillCfgBySkillID(this.baseSkill);
        }

        #endregion

        #region 战技
        //if (skill1CfgData == null)
        {
            skill1CfgData = BattleCfgManager.Instance.GetSkillCfgBySkillID(this.skill1);
        }

        #endregion

        #region 秘技
        //if (skill2CfgData == null)
        {
            skill2CfgData = BattleCfgManager.Instance.GetSkillCfgBySkillID(this.skill2);
        }
        #endregion

        #region 终结技
        //if (skill3CfgData == null)
        {
            skill3CfgData = BattleCfgManager.Instance.GetSkillCfgBySkillID(this.skill3);
        }
        #endregion
    }

    /// <summary>
    /// 初始化天赋被动
    /// </summary>
    private void InitTalent()
    {
        BattleSkillTalentCfg talentCfg;
        foreach (var item in this.heroData.skills)
        {
            talentCfg = BattleCfgManager.Instance.GetTalentCfg(item.Key);
            if (talentCfg != null) 
            {
                int yu = (int)item.Key % 100;
                switch (yu)
                {
                    case 5://天赋1
                        this.talent1 = item.Key;
                        this.talent1LV = item.Value;
                        break;
                    case 6://天赋2
                        this.talent2 = item.Key;
                        this.talent2LV = item.Value;
                        break;
                    case 7://天赋3
                        this.talent3 = item.Key;
                        this.talent3LV = item.Value;
                        break;
                    case 8://天赋4
                        this.talent4 = item.Key;
                        this.talent4LV = item.Value;
                        break;
                    case 9://天赋5
                        this.talent5 = item.Key;
                        this.talent5LV = item.Value;
                        break;
                    case 10://天赋6
                        this.talent6 = item.Key;
                        this.talent6LV = item.Value;
                        break;
                    case 11://被动1
                        this.passive1 = item.Key;
                        this.passive1LV = item.Value;
                        break;
                    case 12://被动2
                        this.passive2 = item.Key;
                        this.passive2LV = item.Value;
                        break;
                    case 13://被动3
                        this.passive3 = item.Key;
                        this.passive3LV = item.Value;
                        break;
                    case 14://被动4
                        this.passive4 = item.Key;
                        this.passive4LV = item.Value;
                        break;
                    case 15://被动5
                        this.passive5 = item.Key;
                        this.passive5LV = item.Value;
                        break;
                    case 16://被动6
                        this.passive6 = item.Key;
                        this.passive6LV = item.Value;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 替换技能
    /// </summary>
    public void ReplaceSkill(int type,long skillid)
    {
        switch (type)
        {
            case 1:
                this.baseSkill = skillid;
                break;
            case 2:
                this.skill1 = skillid;
                break;
            case 3:
                this.skill2 = skillid;
                break;
            case 4:
                this.skill3 = skillid;
                break;
            default:
                break;
        }
    }

    /// <summary>
    ///  战斗前升级某一个技能
    /// </summary>
    public void LevelUpSkill(int type,int addValue)
    {
        switch (type)
        {
            case 1://普攻
                this.baseSkillLV += addValue;
                break;
            case 2://战技
                this.skill1LV += addValue;
                break;
            case 3://秘技
                this.skill2LV += addValue;
                break;
            case 4://终结技
                this.skill3LV += addValue;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 获得普通攻击索敌列表 
    /// </summary>
    /// <param name="objList"></param>
    /// <param name="range"></param>
    /// <param name="shizistate"></param>
    /// <returns></returns>
    public List<BaseObject> GetBaseSkillMonsterList(List<BaseObject> objList,string range,out int shizistate)
	{
        int state = 0;
        List<BaseObject> targetList = new List<BaseObject>();
        if (objList.Count > 0)
        {
            switch (cfgdata.atktype)
            {
                case 1://近战
                    targetList = SkillManager.ins.GetBaseSkillTarget_1(this.roleObj, objList, range);
                    break;
                case 2://远程 单体
                    targetList.Clear();
                    targetList.Add(SkillManager.ins.GetBaseSkillTarget_2(this.prefabObj, objList, range));
                    break;
                case 3://十字
                    targetList = SkillManager.ins.GetBaseSkillTarget_3(this.prefabObj, objList, range, out state);
                    break;
                default:
                    break;
            }
        }
        shizistate = state;
        return targetList;
    }



    public override void OnChildDie()
    {
        this.bRecycle = true;
        BattleEventManager.Dispatch(BattleEventName.battle_heroDie, this.objID);
        BattleHeroManager.Instance.RemoveCardByHero(this.objID);
        GameObject.Destroy(prefabObj.GetComponent<HeroController>());
        BattlePoolManager.Instance.InPool(ERootType.Hero, prefabObj, objID.ToString());

        BattleHeroManager.Instance.HeroDie(this.objID);
        HpSliderManager.ins.OnOneObjDisappear(this);

    }


}

