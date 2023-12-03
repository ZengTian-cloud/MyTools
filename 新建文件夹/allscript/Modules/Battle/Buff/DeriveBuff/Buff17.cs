using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// buff17:回复生命值(以被回复者最大生命值计算)
/// </summary>
public class Buff17 : BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {

        if (target != null && target.bRecycle)
        {
            float value = DamageTool.ComputeBaseObjHealthValue(mainBuff.skillCfg, mainBuff.buffCfg, mainBuff.holder, target, 5);
            target.OnHeal(value);
            return true;
        }
        return false;

    }

}

