using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// buff类型10:嘲讽
/// </summary>
public class Buff10:BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            target.buffStackCompent.AddOneBuff(this, 7);
            return true;
        }
        return false;
    }

  
    public override void OnChildCheckBuff(params object[] parm)
    {
        BaseObject atker = (BaseObject)parm[0];
        if (atker!= null && !atker.bRecycle)
        {
            atker.PriorityTarget = mainBuff.holder;
        }
    }

    public override void RemoveBuffChild(BaseObject target)
    {
        if (target != null && !target.bRecycle)
        {
            target.PriorityTarget = null;
        }
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

