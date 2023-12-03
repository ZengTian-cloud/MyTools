using System;

/// <summary>
/// 组合条件 “与”关系
/// </summary>
public class Condition2001 : BaseCondition
{
    public override bool OnCheck(BaseObject baseObject, string funcPm, params object[] param)
    {
        string[] conditionIDs = funcPm.Split('|');
        bool bCheck = true;
        for (int i = 0; i < conditionIDs.Length; i++)
        {
            if (!ConditionManager.ins.CheckCondition(baseObject, long.Parse(conditionIDs[i]), param))
            {
                bCheck = false;
                break;
            }
        }
        return bCheck;
    }
}

