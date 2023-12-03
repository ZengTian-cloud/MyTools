using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 条件基类
/// </summary>
public class BaseCondition
{
    public long guid;
    /// <summary>
    /// 检测条件
    /// </summary>
    /// <param name="baseObject"></param>
    /// <param name="param">param[0]=battleskillCfg</param>
    /// <returns></returns>
    public bool CheckConditionBool(BaseObject baseObject, string funcPm, params object[] param)
    {
        return OnCheck(baseObject, funcPm, param);
    }

    public virtual bool OnCheck(BaseObject baseObject,string funcPm, params object[] param) { return true; }
}
