using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 条件管理类
/// </summary>
public class ConditionManager
{
    private static ConditionManager Ins;
    public static ConditionManager ins
    {
        get
        {
            if (Ins == null)
            {
                Ins = new ConditionManager();
            }
            return Ins;
        }
    }

    public long guid = 100000;

    private Dictionary<long, BaseCondition> dicCondition = new Dictionary<long, BaseCondition>(); 

    /// <summary>
    /// 检测条件
    /// </summary>
    /// <param name="conditionID">条件ID</param>
    /// <param name="param">参数列表</param>
    public bool CheckCondition(BaseObject baseObject, long conditionID,params object[] param)
    {
        BattleSkillConditionCfg conditionCfg = BattleCfgManager.Instance.GetConditionCfg(conditionID);
        var deriverd = GetConditionClassByID(conditionCfg.conditiontype);
        if (deriverd != null)
        {
            return deriverd.CheckConditionBool(baseObject, conditionCfg.conditionpm, param);
        }
        return true;
    }

    /// <summary>
    /// 获得条件类型ID对应的派生类
    /// </summary>
    public BaseCondition GetConditionClassByID(int conditionType)
    {
        switch (conditionType)
        {
            case 1001:
                return new Condition1001();
            case 1002:
                return new Condition1002();
            case 1003:
                return new Condition1003();
            case 1004:
                return new Condition1004();
            case 1005:
                return new Condition1005();
            case 1006:
                return new Condition1006();
            case 1008:
                return new Condition1008();
            case 1011:
                return new Condition1011();
            case 1012:
                return new Condition1012();
            case 2001:
                return new Condition2001();
            default:
                break;
        }
        Debug.LogError($"未找到ID为{conditionType}的条件，请检查配置！！！");
        return null;
    }

}
