using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 降低消耗
/// </summary>
public class Buff11:BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            int state = target.buffStackCompent.AddOneBuff(this, 11);
            BattleEventManager.Dispatch(BattleEventName.battle_cardCostChange);
            return true;
        }
        return false;
    }


    /// <summary>
    /// 检测buff
    /// </summary>
    /// <param name="buffData"></param>
    /// <param name="target">作用对象</param>
    public override void OnChildCheckBuff(params object[] parm)
    {
        BaseObject baseObject = (BaseObject)parm[0];
        BattleSkillCfg skillCfg = (BattleSkillCfg)parm[1];
        if (baseObject != null && !baseObject.bRecycle) 
        {
            string[] pm = this.mainBuff.buffCfg.functionpm.Split(';');
            if (long.Parse(pm[0]) == baseObject.objID && int.Parse(pm[1]) == skillCfg.skilltype)
            {
                target.buffStackCompent.RemoveOneBuff(this);
            }
        }
    }

    /// <summary>
    /// 移除buff
    /// </summary>
    /// <param name="target"></param>
    public override void RemoveBuffChild(BaseObject target)
    {
        BattleEventManager.Dispatch(BattleEventName.battle_cardCostChange);
    }

}

