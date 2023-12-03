using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// buff类型1:瞬时伤害 不添加到buff列表 直接执行 （）
/// </summary>
public class Buff1 : BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            //计算伤害
            float value = DamageTool.ComputeBaseObjUltimateInjury(mainBuff.skillCfg, mainBuff.buffCfg, mainBuff.holder, mainBuff.computeBase, target, 1);
            if (mainBuff.holder.talentCompent != null)
            {
                mainBuff.holder.talentCompent.DoDamageByElment(int.Parse(mainBuff.buffCfg.functionpm));

            }
            //调用受伤
            target.OnHurt(value, mainBuff.holder, int.Parse(mainBuff.buffCfg.functionpm));
            //造成伤害时检测一次相关buff
            if (string.IsNullOrEmpty(mainBuff.buffCfg.functionpm))
            {
                Debug.LogError($"伤害buff{mainBuff.buffCfg.buffid}没有配置伤害类型，请检查");
                return true;
            }
            mainBuff.holder.buffStackCompent.OnElementDamage(mainBuff.holder,target, mainBuff.buffCfg.functionpm);

            return true;
        }
        return false;
    }
}

