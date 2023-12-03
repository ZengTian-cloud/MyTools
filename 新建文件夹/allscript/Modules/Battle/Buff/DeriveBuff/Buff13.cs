using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// buff类型13:buff层数到达一定层数后，清除自身转换为另一个buff
/// </summary>
public class Buff13:BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            target.buffStackCompent.AddOneBuff(this, 5);
            return true;
        }
        return true;
    }

    public override void OnChildCheckBuff(params object[] parm)
    {
        BaseObject atker = (BaseObject)parm[0];
    }
   
}

