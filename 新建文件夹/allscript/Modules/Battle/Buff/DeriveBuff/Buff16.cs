using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// buff类型16:免疫一次死亡血量回复为1并附加一个buff
/// </summary>
public class Buff16:BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle) 
        {
            string[] funcPm = mainBuff.buffCfg.functionpm.Split('|');
            target.bImmunedDeath = true;
            target.OnHeal(1);
            List<BaseObject> targetBase = new List<BaseObject>() { target };
            for (int p = 0; p < funcPm.Length; p++)
            {
                BuffManager.ins.DOBuff(long.Parse(funcPm[p]), mainBuff.holder, mainBuff.holder, targetBase, false, default, mainBuff.skillCfg, false);
            }
            return true;
        }
        return false;
    }
}

