using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// buff3:回复生命值 不添加到buff列表 直接执行 以自身攻击力计算
/// </summary>
public class Buff3:BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle) 
        {
            float value = DamageTool.ComputeBaseObjHealthValue(mainBuff.skillCfg, mainBuff.buffCfg, mainBuff.holder, target, 1);
            target.OnHeal(value);
          
            BattleLog.Log($"执行Buff_3[治疗(以角色攻击力)]，目标:{target.objID}，释放者:{mainBuff.holder.objID}，数值:{value}");
            return true;
        }
        return false;
    }
}

