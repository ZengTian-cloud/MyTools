
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// buff2:改变属性值 
/// </summary>
public class Buff2:BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            int state = target.buffStackCompent.AddOneBuff(this, 101);
            switch (state)
            {
                case -1://
                    break;
                case 1://改变属性
                    target.AttrChange(long.Parse(mainBuff.buffCfg.functionpm), InitValue());
                    return true;
                default:
                    break;
            }
        }
        return false;
    }



    private float InitValue()
    {
        if (!string.IsNullOrEmpty(mainBuff.buffCfg.value))
        {
            return float.Parse(mainBuff.buffCfg.value);
        }
        else
        {
           return DamageTool.GetSkillValue(mainBuff.skillCfg, mainBuff.buffCfg, mainBuff.holder);
        }
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
            //target.buffStackCompent.RemoveOneBuff(buffData);
            target.AttrChange(long.Parse(mainBuff.buffCfg.functionpm), InitValue() * -1);
        }
    }

}

