using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 条件1003 判断射击盲区内的数量
/// </summary>
public class Condition1003 : BaseCondition
{
    public override bool OnCheck(BaseObject baseObject, string funcPm,params object[] param)
    {
        //英雄
        if (baseObject.objType == 1)
        {
            BaseHero baseHero = (BaseHero)baseObject;
            if (baseHero.baseSkillCfgData.guidetype != 2)
            {
                Debug.LogError($"角色{baseObject.objID}没有射击盲区概念，请检查天赋技能表条件判断!");
                return false;
            }
            return CheckDeadZoneByHero(baseHero);
        }
        return false;
    }

    /// <summary>
    /// 检测英雄盲区范围是否没有敌人
    /// </summary>
    /// <param name="baseHero"></param>
    private bool CheckDeadZoneByHero(BaseHero baseHero)
    {
        string[] rangeArray = baseHero.baseSkillCfgData.guiderange.Split(';');
        List<BaseObject> objList = baseHero.Controller.checkList;
        for (int i = 0;  i < objList.Count; i++)
        {
            //计算两点距离 判断是否在攻击盲区
            float dis = Vector3.Distance(objList[i].prefabObj.transform.position, baseHero.prefabObj.transform.position);
            if (dis < (float.Parse(rangeArray[0]) / 100))
            {
                return false;
            }
        }
        return true;
    }
}
