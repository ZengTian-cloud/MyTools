using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// buff类型4:记录buff来源，造成元素伤害时，附加了来源数值
/// </summary>
public class Buff4:BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            target.buffStackCompent.AddOneBuff(this, 1);
            return true;
        }
        return false;
    }

    
    /// <summary>
    /// 检测buff
    /// </summary>
    /// <param name="buffData"></param>
    /// <param name="target">作用对象</param>
    public override void OnChildCheckBuff(params object[] parm)
    {
   
        BaseObject holder = (BaseObject)parm[0];
        //本次目标
        BaseObject curTarget = (BaseObject)parm[1];
        int element = (int)parm[2];
        if (element > 100)
        {
            if (holder.buffStackCompent.CheckBuffIsHas(mainBuff.buffCfg.buffid)) //攻击者身上有该buff
            {
                if (curTarget != null && !curTarget.bRecycle)
                {
                    float value = InitValue();
                    curTarget.OnHurt(value, mainBuff.holder, 0, 3);
                }
            }
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
        value = value / 10000 * mainBuff.holder.GetBattleAttr(EAttr.ATK);
        return value;
    }
}


