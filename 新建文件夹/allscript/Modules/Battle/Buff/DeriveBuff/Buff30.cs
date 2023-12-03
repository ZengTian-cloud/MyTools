
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 护盾-最大生命值
/// </summary>
public class Buff30 : BaseBuff
{
    private ShieldData shield;

    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            //添加护盾
            target.buffStackCompent.AddOneBuff(this, 103);
            float value = InitValue(); 
            shield = new ShieldData(this, value, value);
            target.ShieldChange(shield, true);
            BattleLog.Log($"对象:{mainBuff.holder.objID}获得最大生命值护盾，护盾总值:{value}");
        }
        return false;
    }


    private float InitValue()
    {
        float v = float.Parse(mainBuff.buffCfg.value) / 10000;
        return mainBuff.holder.GetBattleAttr(EAttr.HP) * v;
    }

    public override void RemoveBuffChild(BaseObject target)
    {
        if (this.buffEffect != null)
        {
            for (int i = 0; i < buffEffect.Count; i++)
            {
                if (buffEffect[i] != null)
                {
                    buffEffect[i].SetActive(false);
                    BattlePoolManager.Instance.InPool(ERootType.Effect, buffEffect[i], buffEffect[i].name);
                }
            }
            buffEffect.Clear();
        }

        if (target!= null )
        {
            if (shield != null)
            {
                target.ShieldChange(shield, false);
            }
        }
    }
}

