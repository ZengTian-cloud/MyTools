using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// buff6 持续伤害 每秒检测一次
/// </summary>
public class Buff6 : BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            target.buffStackCompent.AddOneBuff(this, 4);
            return true;
        }
        return false;
    }

    public override void OnChildCheckBuff(params object[] parm)
    {
        //本次目标
        BaseObject curTarget = (BaseObject)parm[0];
        if (curTarget != null && !curTarget.bRecycle)
        {
            float value = InitValue();
            curTarget.OnHurt(value, mainBuff.holder);
        }

    }

    private float InitValue()
    {
        float value = 0;
        //如果基础数值为空
        if (string.IsNullOrEmpty(mainBuff.buffCfg.value))
        {
            BattleSkillValueCfg valueCfg = BattleCfgManager.Instance.GetSkillValueCfg(long.Parse(mainBuff.buffCfg.growvalueid));
            string[] values;//数值系数
            float damageValue = 0;//系数伤害
            if (!string.IsNullOrEmpty(valueCfg.basevalue))//固定系数不为空
            {
                //固定系数
                damageValue = float.Parse(valueCfg.basevalue);
            }
            else if (!string.IsNullOrEmpty(valueCfg.value))//成长系数
            {
                //技能等级
                int skilllv = 0;
                if (mainBuff.holder.objType == 1)
                {
                    BaseHero baseHero = (BaseHero)mainBuff.holder;
                    switch (mainBuff.skillCfg.skilltype)
                    {
                        case 1:
                            skilllv = baseHero.baseSkillLV;
                            break;
                        case 2:
                            skilllv = baseHero.skill1LV;
                            break;
                        case 3:
                            skilllv = baseHero.skill2LV;
                            break;
                        case 4:
                            skilllv = baseHero.skill3LV;
                            break;
                        default:
                            break;
                    }
                    values = valueCfg.value.Split('|');
                    if (values.Length > 1)//系数数值有多个 根据等级取出值
                    {
                        value = float.Parse(values[skilllv]);
                    }
                    else//取第一个
                    {
                        value = float.Parse(values[0]);
                    }
                }
            }
        }
        else
        {
            value = float.Parse(mainBuff.buffCfg.value);
        }
        return value;
    }
}

