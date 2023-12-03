using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// buff类型18:造成伤害（以目标最大生命值计算）
/// </summary>
public class Buff18:BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            //计算伤害
            float value = DamageTool.ComputeBaseObjUltimateInjury(mainBuff.skillCfg, mainBuff.buffCfg, mainBuff.holder, mainBuff.computeBase, target, 5);
            //调用受伤
            target.OnHurt(value, mainBuff.holder);
            //造成伤害时检测一次相关buff
            target.buffStackCompent.OnElementDamage(mainBuff.holder,target, mainBuff.buffCfg.functionpm);


            return true;
        }
        return false;
    }
}

