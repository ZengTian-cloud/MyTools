using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 每秒回复生命值（以释放者最大生命值计算）
/// </summary>
public class Buff23 : BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            target.buffStackCompent.AddOneBuff(this, 4);
            return true;
        }
        return false;
    }

    public override void OnChildCheckBuff(params object[] parm)
    {
        BaseObject curTarge = (BaseObject)parm[0];
        if (curTarge != null && !curTarge.bRecycle)
        {
            float value = InitValue();
            curTarge.OnHeal(value);
        }
    }

    private float InitValue()
    {
        float value = DamageTool.ComputeBaseObjHealthValue(mainBuff.skillCfg, mainBuff.buffCfg, mainBuff.holder, target, 3);
        return value;
    }

    public override void RemoveBuffChild(BaseObject target)
    {
        if (this.buffEffect != null)
        {
            for (int i = 0; i < buffEffect.Count; i++)
            {
                buffEffect[i].SetActive(false);
                BattlePoolManager.Instance.InPool(ERootType.Effect, buffEffect[i], buffEffect[i].name);
            }
            buffEffect.Clear();
        }
    }
}

