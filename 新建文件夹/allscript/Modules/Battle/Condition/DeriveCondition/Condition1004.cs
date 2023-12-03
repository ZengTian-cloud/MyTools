using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 条件1004 随机概率
/// </summary>
public class Condition1004 : BaseCondition
{
    //概率
    private float range;
    public override bool OnCheck(BaseObject baseObject,string funcPm , params object[] param)
    {
        InitParam(funcPm);
        float r = Random.Range(0, 10000);
        if (r <= range)
        {
            return true;
        }
        return false;
    }

    private void InitParam(string funcPm)
    {
        range = float.Parse(funcPm);
    }
}
