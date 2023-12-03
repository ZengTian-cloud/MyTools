using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 条件1006 攻击范围内敌人数量是否满足条件
/// </summary>
public class Condition1006 : BaseCondition
{
    //条件参数
    public string[] conditionParm;
    //攻击范围内的数量
    public int count;

    public override bool OnCheck(BaseObject baseObject,string funcPm, params object[] param)
    {
        InitParam(funcPm);
        //英雄
        if (baseObject.objType == 1)
        {
            BaseHero baseHero = (BaseHero)baseObject;
            return CheckAtkRangeByHero(baseHero);
        }
        return false;
    }

    /// <summary>
    /// 检测英雄攻击范围内敌人数量
    /// </summary>
    /// <param name="baseHero"></param>
    private bool CheckAtkRangeByHero(BaseHero baseHero)
    {

        string[] rangeArray = baseHero.baseSkillCfgData.guiderange.Split(';');
        int state = int.Parse(conditionParm[0]);
        int checkCopunt = int.Parse(conditionParm[1]);
        switch (state)
        {
            case 1://>=
                return baseHero.Controller.checkList.Count >= checkCopunt;
            case 2://<=
                return baseHero.Controller.checkList.Count <= checkCopunt;
            case 3://==
                return baseHero.Controller.checkList.Count == checkCopunt;
            default:
                return false;
        }
    }

    private void InitParam( string funcPm)
    {
        conditionParm = funcPm.Split(';');
    }
}
