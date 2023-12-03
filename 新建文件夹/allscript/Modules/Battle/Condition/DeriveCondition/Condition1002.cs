using System.Collections;
using System.Collections.Generic;
using DG.Tweening.Core;
using UnityEngine;
/// <summary>
/// 条件1002 目标没有指定的buff类型
/// </summary>
public class Condition1002 : BaseCondition
{
    //buff分类id
    private string[] classIDs;


    public List<BaseBuff> buffslist;
    public override bool OnCheck(BaseObject baseObject, string funcPm,params object[] param)
    {
        InitParam(funcPm);
        buffslist = baseObject.buffStackCompent.curBuffList;
        if (buffslist != null && buffslist.Count > 0)
        {
            for (int i = 0; i < buffslist.Count; i++)
            {
                for (int d = 0; d < classIDs.Length; d++)
                {
                    if ((int)buffslist[i].mainBuff.buffCfg.classid == int.Parse(classIDs[d]))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private void InitParam(string funcPm)
    {
        classIDs = funcPm.Split('|');
    }
}
