using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Managers;

/// <summary>
/// 天赋管理组件
/// </summary>
public class TalentCompent : MonoBehaviour
{
    public BaseObject baseObject;

    //对象的天赋管理组件 key-检测时机 value-天赋id
    //触发条件
    //1= 进入战斗后持续监测（每0.5秒检测一次）
    //2= 战斗开始时触发
    //3=释放技能后触发
    //4=造成伤害计算前触发(元素)
    //5=造成伤害计算前触发(技能）
    //6=受到伤害计算后触发
    //7=死亡时触发
    //8=清除BUFF时触发
    //10=摸取技能牌时触发
    public Dictionary<int, List<BattleSkillTalentCfg>> dicTalent = new Dictionary<int, List<BattleSkillTalentCfg>>();

    public Dictionary<long, int> talentCounter = new Dictionary<long, int>();//触发天赋次数的计数器 作用于天赋的次数限制条件

    public float interval = 0.5f;//检测间隔
    public float timer = 0f;

    private void Update()
    {
        //进入战斗后持续监测（每0.5秒检测一次）
        if (GameCenter.mIns.m_BattleMgr.curBattleState == EBattleState.Start)
        {
            timer += Time.deltaTime;
            if (timer >= interval) 
            {
                if (dicTalent.ContainsKey(1))
                {
                    TalentManager.Instance.CheckTalentType_1(baseObject, dicTalent[1]);
                }
                timer = 0;
            }
        }
    }

    /// <summary>
    /// 主动释放技能时检测
    /// </summary>
    public void OnInitiativeDoSkill(int skillType, params string[] parm)
    {
        if (dicTalent.ContainsKey(11))
        {
            TalentManager.Instance.CheckTalentType_3(baseObject, dicTalent[11], skillType, parm);
        }
    }

    /// <summary>
    /// 释放技能时检测 parm[0] = 索敌列表 ，parm[1] = 是否主动释放
    /// </summary>
    public void OnDoSkill(int skillType, params object[] parm)
    {
        if (dicTalent.ContainsKey(3))
        {
            TalentManager.Instance.CheckTalentType_3(baseObject, dicTalent[3], skillType, parm);
        }
    }

    /// <summary>
    /// 造成伤害时触发
    /// </summary>
    /// <param name="elment">元素伤害类型</param>
    public void DoDamageByElment(int elment)
    {
        if (dicTalent.ContainsKey(4))
        {
            TalentManager.Instance.CheckTalentType_4(baseObject, dicTalent[4], elment);
        }
    }

    /// <summary>
    /// 造成伤害时触发
    /// </summary>
    /// <param name="skillType">技能类型</param>
    public void DoDamageBySkillType(int skillType)
    {
        if (dicTalent.ContainsKey(5))
        {
            TalentManager.Instance.CheckTalentType_5(baseObject, dicTalent[5], skillType);
        }
    }

    /// <summary>
    /// 收到伤害时触发
    /// </summary>
    public void OnHurt()
    {
        if (dicTalent.ContainsKey(6))
        {
            TalentManager.Instance.CheckTalentType_6(baseObject, dicTalent[6]);
        }
    }

    /// <summary>
    /// 死亡时触发
    /// </summary>
    public void OnDie()
    {
        if (dicTalent.ContainsKey(7))
        {
            TalentManager.Instance.CheckTalentType_7(baseObject, dicTalent[7]);
        }
    }

    /// <summary>
    /// 清除指定buff时触发
    /// </summary>
    public void OnClearBuff(long buffID)
    {
        if (dicTalent.ContainsKey(8))
        {
            TalentManager.Instance.CheckTalentType_8(baseObject, dicTalent[8]);
        }
    }

    /// <summary>
    /// 普通攻击时触发
    /// </summary>
    public void OnBaseSkill(List<BaseObject> objList)
    {
        if (dicTalent.ContainsKey(9))
        {
            TalentManager.Instance.CheckTalentType_9(baseObject, dicTalent[9], objList);
        }
    }


    /// <summary>
    /// 抽取技能卡牌时触发
    /// </summary>
    public DrawCardData OnDrawSkillCard(long heroid,int skillType)
    {

        if (dicTalent.ContainsKey(10))
        {
            return TalentManager.Instance.CheckTalentType_10(baseObject, dicTalent[10], heroid, skillType);
        }

        return null;
    }

    /// <summary>
    /// 添加一个天赋被动到对象的控制器上（开战前已完成）
    /// </summary>
    public void AddOneTalent(int checkType, BattleSkillTalentCfg battleSkillTalentCfg)
    {
        if (!dicTalent.ContainsKey(checkType))
        {
            dicTalent.Add(checkType, new List<BattleSkillTalentCfg>());
        }

        BattleSkillTalentCfg skillTalentCfg = new BattleSkillTalentCfg()
        {
            talentid = battleSkillTalentCfg.talentid,
            describe = battleSkillTalentCfg.describe,
            note = battleSkillTalentCfg.note,
            triger = battleSkillTalentCfg.triger,
            trigerpm = battleSkillTalentCfg.trigerpm,
            condition = battleSkillTalentCfg.condition,
            trigerlimit = battleSkillTalentCfg.trigerlimit,
            addbuff = battleSkillTalentCfg.addbuff,
            removebuff = battleSkillTalentCfg.removebuff,
            skilllevelup = battleSkillTalentCfg.skilllevelup,
            exchangeskill = battleSkillTalentCfg.exchangeskill,
            useskill = battleSkillTalentCfg.useskill,
            cooldown = battleSkillTalentCfg.cooldown,
            attrbonus = battleSkillTalentCfg.useskill,
            addenergy = battleSkillTalentCfg.addenergy,
        };
        dicTalent[checkType].Add(skillTalentCfg);
    }


}

