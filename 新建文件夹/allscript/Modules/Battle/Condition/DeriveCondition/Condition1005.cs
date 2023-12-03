using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 条件1005 血量（1=大于等于，2=小于等于，3=等于）配置值百分比（ConditionPM）时生效（5000=50%）
/// </summary>
public class Condition1005 : BaseCondition
{
    public override bool OnCheck(BaseObject baseObject,string funcPm, params object[] param)
    {
        string[] parms = funcPm.Split(';');
        //比较类型
        int type = int.Parse(parms[0]);
        //百分比
        float percent = float.Parse(parms[1]) / 10000;

        float curHp = baseObject.curHp;//当前血量
        float maxHp = baseObject.GetBattleAttr(EAttr.HP);//最大血量
        switch (type)
        {
            case 1:
                return curHp >= maxHp * percent;
            case 2:
                return curHp <= maxHp * percent;
            case 3:
                return curHp == maxHp * percent;
            default:
                return false;
        }
    }
}
