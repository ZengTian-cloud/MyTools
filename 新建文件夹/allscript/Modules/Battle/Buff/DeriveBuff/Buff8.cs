using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// buff类型8:受伤时反击 对攻击者释加一个buff（伤害/效果buff)
/// </summary>
public class Buff8 : BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            target.buffStackCompent.AddOneBuff(this, 2);
            return true;
        }
        return false;
    }

    public override void OnChildCheckBuff(params object[] parm)
    {
        BaseObject curTarget = (BaseObject)parm[0];
        List<BaseObject> baseObjects = new List<BaseObject>() { curTarget };
        if (!string.IsNullOrEmpty(mainBuff.buffCfg.functionpm))
        {
            string[] buffs = mainBuff.buffCfg.functionpm.Split('|');
            for (int i = 0; i < buffs.Length; i++)
            {
                BuffManager.ins.DOBuff(long.Parse(buffs[i]), mainBuff.holder, mainBuff.holder, baseObjects, false, default, mainBuff.skillCfg, false,false);
            }
        }
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

