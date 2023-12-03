using System;
using System.Collections;
using System.Collections.Generic;
using Basics;
using UnityEngine;


/// <summary>
/// 天赋被动管理
/// </summary>
public class TalentManager:SingletonNotMono<TalentManager>
{
	/// <summary>
	/// 英雄战前检测天赋被动
	/// </summary>
	/// <param name="baseHero"></param>
	public void CheckTalentBeforBattleByHero(BaseHero baseHero)
	{
        List<long> talentList = GetAllTalentByHero(baseHero);
        for (int i = 0; i < talentList.Count; i++)
        {
            HandleTalent(baseHero,talentList[i]);
        }
    }

    /// <summary>
    /// 怪物检测被动
    /// </summary>
    /// <param name="baseMonster"></param>
    public void CheckTalentByMonster(BaseMonster baseMonster)
    {
        List<long> talentList = GetAllTalentByMonster(baseMonster);
        for (int i = 0; i < talentList.Count; i++)
        {
            HandleTalent(baseMonster, talentList[i]);
        }
    }

    /// <summary>
    /// 处理天赋
    /// </summary>
    public void HandleTalent(BaseHero baseHero,long tanlentid)
    {
        BattleSkillTalentCfg talentCfg = BattleCfgManager.Instance.GetTalentCfg(tanlentid);
        if (talentCfg.triger == 2)
        {
            if (!string.IsNullOrEmpty(talentCfg.skilllevelup))//技能升级
            {
                TalentSkillLevelUp(baseHero, talentCfg.skilllevelup);
            }
            if (!string.IsNullOrEmpty(talentCfg.exchangeskill))//修改英雄技能
            {
                TalentExchangeSkill(baseHero, talentCfg.exchangeskill);
            }
            if (!string.IsNullOrEmpty(talentCfg.addbuff))//添加buff
            {
                TalentAddBuff(baseHero, talentCfg.addbuff);
            }
        }
        else
        {
            baseHero.talentCompent.AddOneTalent(talentCfg.triger, talentCfg);
        }
    }

    public void HandleTalent(BaseMonster baseMonster, long tanlentid)
    {
        BattleSkillTalentCfg talentCfg = BattleCfgManager.Instance.GetTalentCfg(tanlentid);
        if (talentCfg.triger == 2)
        {
            if (!string.IsNullOrEmpty(talentCfg.addbuff))//添加buff
            {
                TalentAddBuff(baseMonster, talentCfg.addbuff);
            }
        }
        else
        {
            baseMonster.talentCompent.AddOneTalent(talentCfg.triger, talentCfg);
        }
    }

    /// <summary>
    /// 获得英雄激活的所有天赋和被动
    /// </summary>
    public List<long> GetAllTalentByHero(BaseHero baseHero)
	{
        List<long> talentList = new List<long>();
        //已激活被动
        foreach (var item in baseHero.heroData.skills)
        {
            if (item.Value > 0)
            {
                BattleSkillTalentCfg talentCfg = BattleCfgManager.Instance.GetTalentCfg(item.Key);
                if (talentCfg != null)
                {
                    talentList.Add(item.Key);
                }
            }
           
        }
        //已激活天赋
        List<long> longs = HeroDataManager.Instance.GetHeroTalentByHeroIDAndStar(baseHero.objID, baseHero.heroData.star);
        for (int i = 0; i < longs.Count; i++)
        {
            talentList.Add(longs[i]);
        }
        return talentList;
    }

    /// <summary>
    /// 获得怪物的所有天赋被动
    /// </summary>
    /// <returns></returns>
    public List<long> GetAllTalentByMonster(BaseMonster baseMonster)
    {
        List<long> talentList = new List<long>();
        if (!string.IsNullOrEmpty(baseMonster.cfgdata.talent))
        {
            string[] talents = baseMonster.cfgdata.talent.Split('|');
            for (int i = 0; i < talents.Length; i++)
            {
                talentList.Add(long.Parse(talents[i]));
            }
        }

        return talentList;
    }

    //////////////////////////////////////////天赋检测操作//////////////////////////////////////////
    /// <summary>
    /// 天赋添加buff
    /// </summary>
    /// <param name="baseObject"></param>
    /// <param name="addBuff"></param>
    /// <param name="parm"> parm[0] = 索敌列表 ，parm[1] = 是否主动释放</param>
    public void TalentAddBuff(BaseObject baseObject, string addBuff, params object[] parm)
    {
        List<BaseObject> target = new List<BaseObject>();

        string[] buffList = addBuff.Split('|');
        List<BaseObject> oldTarget = null;
        if (parm.Length > 0)
        {
            oldTarget = (List<BaseObject>)parm[0];
        }

        for (int i = 0; i < buffList.Length; i++)
        {
            string[] buffs = buffList[i].Split(';');
            long buffid = long.Parse(buffs[0]);
            switch (buffs[1])
            {
                case "1"://沿用本次目标
                    if (oldTarget != null)
                    {
                        for (int t = 0; t < oldTarget.Count; t++)
                        {
                            if (oldTarget[i] != null && !oldTarget[i].bRecycle)
                            {
                                target.Add(oldTarget[i]);
                            }
                        }
                    }
                    break;
                case "2"://自己
                    if (baseObject!= null && !baseObject.bRecycle)
                    {
                        target.Add(baseObject);
                    }
                    break;
                case "3"://怪物随机
                    break;
                case "4"://英雄随机
                    break;
                case "5"://怪物全体
                    break;
                case "6"://英雄全体
                    break;
                default:
                    break;
            }
            BuffManager.ins.DOBuff(buffid, baseObject, baseObject, target, false, default, null, true, false, parm);
        }
    }

    /// <summary>
    /// 天赋移除buff
    /// </summary>
    /// 
    /// <param name="baseObject"></param>
    /// <param name="addBuff"></param>
    public void TalentRemoveBuff(BaseObject baseObject, string addBuff)
    {

    }

    /// <summary>
    ///  天赋升级技能
    /// </summary>
    /// <param name="baseHero"></param>
    /// <param name="skilllevelup"></param>
    public void TalentSkillLevelUp(BaseHero baseHero,string skilllevelup)
    {
        string[] skills = skilllevelup.Split('|');
        for (int i = 0; i < skills.Length; i++)
        {
            string[] param = skills[i].Split(';');
            baseHero.LevelUpSkill(int.Parse(param[0]), int.Parse(param[1]));
        }
    }

    /// <summary>
    /// 天赋改变技能
    /// </summary>
    /// <param name="baseHero"></param>
    /// <param name="exchangeskill"></param>
    public void TalentExchangeSkill(BaseHero baseHero,string exchangeskill)
    {
        string[] skills = exchangeskill.Split('|');
        for (int i = 0; i < skills.Length; i++)
        {
            string[] param = skills[i].Split(';');
            baseHero.ReplaceSkill(int.Parse(param[0]), long.Parse(param[1]));
        }
    }

    /// <summary>
    /// 天赋额外释放技能
    /// </summary>
    /// <param name="baseHero"></param>
    /// <param name="useskill"></param>
    public void TalentUseSkill(BaseObject baseObject, string useskill)
    {
        BattleLog.Log($"天赋额外释放技能--对象:{baseObject.objID},使用技能:{useskill}");
        string[] skills = useskill.Split('|');
        for (int i = 0; i < skills.Length; i++)
        {
            string[] skillPm = skills[i].Split(';');
            if (baseObject.objType == 2)//怪物
            {
                BattleSkillCfg battleSkillCfg = BattleCfgManager.Instance.GetSkillCfgBySkillID(long.Parse(skillPm[0]));
                List<BaseObject> targetList = new List<BaseObject>();
                switch (skillPm[2])
                {
                    case "1"://沿用当前目标
                        break;
                    case "2"://沿用当前位置
                        break;
                    case "3"://随机目标
                        int rang3 = UnityEngine.Random.Range(0, BattleHeroManager.Instance.depolyHeroList.Count - 1);
                        targetList.Add(BattleHeroManager.Instance.depolyHeroList[rang3]);
                        break;
                    case "4"://自己
                        targetList.Add(baseObject);
                        break;
                    case "5"://随机目标脚下格子
                        break;
                    default:
                        break;
                }
                baseObject.isOnSkill = true;
                baseObject.OnSkillBase(targetList, battleSkillCfg);
            }
            else if (baseObject.objType == 1)//英雄
            {
                BaseHero baseHero = BattleHeroManager.Instance.GetBaseHeroByHeroID(long.Parse(skillPm[0]));
                if (baseHero != null && !baseHero.bRecycle)
                {
                    BattleSkillCfg battleSkillCfg = null;
                    switch (skillPm[1])
                    {
                        case "1"://普攻
                            battleSkillCfg = baseHero.baseSkillCfgData;
                            break;
                        case "2"://战技
                            battleSkillCfg = baseHero.skill1CfgData;
                            break;
                        case "3"://秘技
                            battleSkillCfg = baseHero.skill2CfgData;
                            break;
                        case "4"://终结技
                            battleSkillCfg = baseHero.skill3CfgData;
                            break;
                        default:
                            break;
                    }
                    List<BaseObject> targetList = new List<BaseObject>();
                    Vector3 tagetPos = default;
                    switch (skillPm[2])
                    {
                        case "1"://沿用当前目标
                            break;
                        case "2"://沿用当前位置
                            break;
                        case "3"://随机目标
                            List<long> guidlist3 = new List<long>(BattleMonsterManager.Instance.dicMonsterList.Keys);
                            int rang3 = UnityEngine.Random.Range(0, guidlist3.Count);
                            targetList.Add(BattleMonsterManager.Instance.GetOneMonsterByGUID(guidlist3[rang3]));
                            break;
                        case "4"://自己
                            targetList.Add(baseObject);
                            break;
                        case "5"://随机目标脚下格子
                            List<long> guidlist = new List<long>(BattleMonsterManager.Instance.dicMonsterList.Keys);
                            int rang = UnityEngine.Random.Range(0, guidlist.Count);
                            tagetPos = BattleMonsterManager.Instance.GetOneMonsterByGUID(guidlist[rang]).Controller.GetCurPointPos();
                            break;
                        default:
                            break;
                    }
                    if (battleSkillCfg.guidetype != 2 && battleSkillCfg.guidetype != 5)//非单体
                    {
                        if (!baseObject.isOnSkill)
                        {
                            baseObject.isOnSkill = true;
                            baseObject.OnSkillBase(tagetPos, battleSkillCfg,false);
                        }
                        else
                        {
                            baseObject.AddWillDoSkill(tagetPos, battleSkillCfg);
                        }
                    }
                    else
                    {
                        if (!baseObject.isOnSkill)
                        {
                            baseObject.isOnSkill = true;
                            baseObject.OnSkillBase(targetList, battleSkillCfg, false);
                        }
                        else
                        {
                            baseObject.AddWillDoSkill(targetList, battleSkillCfg);
                        }
                        
                    }
                }
            }
            
        }
    }

    /// <summary>
    ///  天赋主角技能cd减少
    /// </summary>
    public void TalentLeaderSkillCDSub(string cooldown)
    {
        Debug.Log("---------天赋主角技能cd减少");
    }

    /// <summary>
    /// 天赋算力回复
    /// </summary>
    /// <param name="value"></param>
    public void TalentAddenergy(int value)
    {


    }

    /// <summary>
    /// 检测技能释放类型是否满足条件_type
    /// </summary>
    public bool CheckSkillType(BaseObject baseObject, string trigerpm,int skillType)
    {
        //可触发的技能
        string[] skills = trigerpm.Split('|');
        for (int i = 0; i < skills.Length; i++)
        {
            int heroid = int.Parse(skills[i].Split(';')[0]);
            int type = int.Parse(skills[i].Split(';')[1]);
            if ((baseObject.objID == heroid && skillType == type) || type == -1)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 检测技能释放类型是否满足条件_元素
    /// </summary>
    public bool CheckSkillElement(BaseObject baseObject, string trigerpm, int element)
    {
        //可触发的技能
        string[] skills = trigerpm.Split('|');
        for (int i = 0; i < skills.Length; i++)
        {
            if (element == int.Parse(skills[i]))
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// 检测技能释放类型是否满足条件_技能伤害类型
    /// </summary>
    public bool CheckSkillDamageType(BaseObject baseObject, string trigerpm, int skilltype)
    {
        //可触发的技能
        string[] skills = trigerpm.Split('|');
        for (int i = 0; i < skills.Length; i++)
        {
            if (skilltype == int.Parse(skills[i]))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 执行该天赋被动所有条目
    /// </summary>
    /// <param name="baseObject"></param>
    /// <param name="talentCfg"></param>
    /// <param name="conditionType"></param>
    /// <param name="parm">parm[0] = 索敌列表 ，parm[1] = 是否主动释放</param>
    public void DoAllTalent(BaseObject baseObject, BattleSkillTalentCfg talentCfg,int conditionType = 0, params object[] parm)
    {
        if (talentCfg.trigerlimit != -1 && talentCfg.trigerlimit < 1)
        {
            return;
        }
        if (talentCfg.trigerlimit != -1)
        {
            talentCfg.trigerlimit -= 1;
        }        

        if (!string.IsNullOrEmpty(talentCfg.addbuff))
        {
            TalentAddBuff(baseObject, talentCfg.addbuff, parm);
        }
        if (!string.IsNullOrEmpty(talentCfg.removebuff))
        {
            TalentRemoveBuff(baseObject, talentCfg.removebuff);
        }
        if (!string.IsNullOrEmpty(talentCfg.skilllevelup))
        {
            TalentSkillLevelUp((BaseHero)baseObject, talentCfg.skilllevelup);
        }
        if (!string.IsNullOrEmpty(talentCfg.exchangeskill))
        {
            if (conditionType != 10)
            {
                TalentExchangeSkill((BaseHero)baseObject, talentCfg.exchangeskill);
            }
        }
        if (!string.IsNullOrEmpty(talentCfg.useskill))
        {
            TalentUseSkill(baseObject, talentCfg.useskill);
        }
        if (!string.IsNullOrEmpty(talentCfg.cooldown))
        {
            TalentLeaderSkillCDSub(talentCfg.cooldown);
        }
        if (talentCfg.addenergy > 0)
        {
            TalentAddenergy(talentCfg.addenergy);
        }
    }

    //////////////////////////////////////////天赋检测//////////////////////////////////////////
    /// <summary>
    /// 检测天赋类型1_每0.5f检测一次 
    /// </summary>
    public void CheckTalentType_1(BaseObject baseObj, List<BattleSkillTalentCfg> talentCfgs)
    {
        for (int i = 0; i < talentCfgs.Count; i++)
        {
            if (ConditionManager.ins.CheckCondition(baseObj,talentCfgs[i].condition))
            {
                if (!string.IsNullOrEmpty(talentCfgs[i].addbuff))
                {
                    TalentAddBuff(baseObj,talentCfgs[i].addbuff);
                }
            }
        } 
    }

    /// <summary>
    /// 检测天赋类型3_释放技能检测一次
    /// </summary>
    /// <param name="baseObject">释放者</param>
    /// <param name="talentCfgs">天赋被动配置</param>
    /// <param name="skillType">技能类型</param>
    /// parm[0] = 索敌列表 ，parm[1] = 是否主动释放
    public void CheckTalentType_3(BaseObject baseObject,List<BattleSkillTalentCfg> talentCfgs,int skillType, params object[] parm)
    {
        bool bTriger;//技能触发
        bool bCondition;//条件判断
        for (int i = 0; i < talentCfgs.Count; i++)
        {
            bTriger = false;
            bCondition = false;
            //判断技能是否满足触发条件
            if (!string.IsNullOrEmpty(talentCfgs[i].trigerpm))
            {
                bTriger = CheckSkillType(baseObject, talentCfgs[i].trigerpm, skillType);
            }
            //
            if (bTriger)
            {
                bCondition = talentCfgs[i].condition > 0 ? ConditionManager.ins.CheckCondition(baseObject, talentCfgs[i].condition) : true;
            }
            if (bTriger && bCondition)
            {
                DoAllTalent(baseObject, talentCfgs[i], 0,parm);
            }
        }
    }


    /// <summary>
    /// 检测天赋类型4_造成元素伤害时检测一次
    /// </summary>
    /// <param name="baseObject"></param>
    /// <param name="talentCfgs"></param>
    /// <param name="element"></param>
    public void CheckTalentType_4(BaseObject baseObject, List<BattleSkillTalentCfg> talentCfgs, int element)
    {
        bool bTriger;//技能触发
        bool bCondition;//条件判断
        for (int i = 0; i < talentCfgs.Count; i++)
        {
            bTriger = false;
            bCondition = false;
            //判断技能是否满足触发条件
            if (!string.IsNullOrEmpty(talentCfgs[i].trigerpm))
            {
                bTriger = CheckSkillElement(baseObject, talentCfgs[i].trigerpm, element);
            }
            //
            if (bTriger)
            {
                bCondition = talentCfgs[i].condition > 0 ? ConditionManager.ins.CheckCondition(baseObject, talentCfgs[i].condition) : true;
            }

            if (bTriger && bCondition)
            {
                DoAllTalent(baseObject, talentCfgs[i]);
            }
        }
    }

    /// <summary>
    /// 检测天赋类型5_造成技能伤害时检测一次
    /// </summary>
    /// <param name="baseObject"></param>
    /// <param name="talentCfgs"></param>
    /// <param name="element"></param>
    public void CheckTalentType_5(BaseObject baseObject, List<BattleSkillTalentCfg> talentCfgs, int skillType)
    {
        bool bTriger;//技能触发
        bool bCondition;//条件判断
        for (int i = 0; i < talentCfgs.Count; i++)
        {
            bTriger = false;
            bCondition = false;
            //判断技能是否满足触发条件
            if (!string.IsNullOrEmpty(talentCfgs[i].trigerpm))
            {
                bTriger = CheckSkillDamageType(baseObject, talentCfgs[i].trigerpm, skillType);
            }
            //
            if (bTriger)
            {
                bCondition = talentCfgs[i].condition > 0 ? ConditionManager.ins.CheckCondition(baseObject, talentCfgs[i].condition) : true;
            }

            if (bTriger && bCondition)
            {
                DoAllTalent(baseObject, talentCfgs[i]);
            }
        }
    }


    /// <summary>
    /// 检测天赋类型6_受到伤害时检测一次
    /// </summary>
    /// <param name="baseObject"></param>
    /// <param name="talentCfgs"></param>
    /// <param name="element"></param>
    public void CheckTalentType_6(BaseObject baseObject, List<BattleSkillTalentCfg> talentCfgs)
    {
        bool bCondition;//条件判断
        for (int i = 0; i < talentCfgs.Count; i++)
        {
            bCondition = talentCfgs[i].condition > 0 ? ConditionManager.ins.CheckCondition(baseObject, talentCfgs[i].condition) : true;
            if (bCondition)
            {
                DoAllTalent(baseObject, talentCfgs[i]);
            }
        }
    }


    /// <summary>
    /// 检测天赋类型7_死亡时检测一次
    /// </summary>
    /// <param name="baseObject"></param>
    /// <param name="talentCfgs"></param>
    /// <param name="element"></param>
    public void CheckTalentType_7(BaseObject baseObject, List<BattleSkillTalentCfg> talentCfgs)
    {
        bool bCondition;//条件判断
        for (int i = 0; i < talentCfgs.Count; i++)
        {
            bCondition = talentCfgs[i].condition > 0 ? ConditionManager.ins.CheckCondition(baseObject, talentCfgs[i].condition) : true;
            if (bCondition)
            {
                DoAllTalent(baseObject, talentCfgs[i]);
            }
        }
    }

    /// <summary>
    /// 检测天赋类型8_清除buff时检测一次
    /// </summary>
    /// <param name="baseObject"></param>
    /// <param name="talentCfgs"></param>
    /// <param name="element"></param>
    public void CheckTalentType_8(BaseObject baseObject, List<BattleSkillTalentCfg> talentCfgs)
    {
        bool bCondition;//条件判断
        for (int i = 0; i < talentCfgs.Count; i++)
        {
            bCondition = talentCfgs[i].condition > 0 ? ConditionManager.ins.CheckCondition(baseObject, talentCfgs[i].condition) : true;
            if (bCondition)
            {
                DoAllTalent(baseObject, talentCfgs[i]);
            }
        }
    }


    /// <summary>
    /// 检测天赋类型9_普通攻击时触发一次
    /// </summary>
    /// <param name="baseObject"></param>
    /// <param name="talentCfgs"></param>
    /// <param name="element"></param>
    public void CheckTalentType_9(BaseObject baseObject, List<BattleSkillTalentCfg> talentCfgs, List<BaseObject> objList)
    {
        bool bCondition;//条件判断
        for (int i = 0; i < talentCfgs.Count; i++)
        {
            bCondition = talentCfgs[i].condition > 0 ? ConditionManager.ins.CheckCondition(baseObject, talentCfgs[i].condition) : true;
            if (bCondition)
            {
                DoAllTalent(baseObject, talentCfgs[i], 9, objList);
            }
        }
    }

    /// <summary>
    /// 检测天赋类型10_抽卡时触发
    /// </summary>
    /// <param name="baseObject"></param>
    /// <param name="talentCfgs"></param>
    /// <param name="element"></param>
    public DrawCardData CheckTalentType_10(BaseObject baseObject, List<BattleSkillTalentCfg> talentCfgs, long heroid, int skillType)
    {
        bool bCondition;//条件判断
        DrawCardData drawCardData = null;
        for (int i = 0; i < talentCfgs.Count; i++)
        {
            bool bTriger = false;
            if (!string.IsNullOrEmpty(talentCfgs[i].trigerpm))
            {
                string[] pm = talentCfgs[i].trigerpm.Split('|');
                for (int p = 0; p < pm.Length; p++)
                {
                    string[] par = pm[p].Split(';');
                    if (long.Parse(par[0]) == heroid && int.Parse(par[1]) == skillType)
                    {
                        bTriger = true;
                        break;
                    }
                }
            }
            else
            {
                bTriger = true;
            }
                    

            bCondition = talentCfgs[i].condition > 0 ? ConditionManager.ins.CheckCondition(baseObject, talentCfgs[i].condition) : true;
            if (bCondition && bTriger)
            {
                DoAllTalent(baseObject, talentCfgs[i], 10);
                drawCardData = DrawNewCard(baseObject, talentCfgs[i]);
            }
        }
        return drawCardData;
    }

    /// <summary>
    /// 抽取一张新卡 仅条件10
    /// </summary>
    public DrawCardData DrawNewCard(BaseObject baseObject, BattleSkillTalentCfg talentCfg)
    {
        if (!string.IsNullOrEmpty(talentCfg.exchangeskill))
        {
            string[] skillPm = talentCfg.exchangeskill.Split(';');
            int skillType = int.Parse(skillPm[0]);
            long skillid = long.Parse(skillPm[1]);
            DrawCardData drawCardData = new DrawCardData(-1, skillid, baseObject.objID, BattleCfgManager.Instance.GetSkillCfgBySkillID(skillid));
            return drawCardData;
        }
        return null;
    }
}

