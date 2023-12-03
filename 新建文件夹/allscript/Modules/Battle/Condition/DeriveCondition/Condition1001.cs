using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 条件1001，目标是否拥有指定buff
/// </summary>
public class Condition1001 : BaseCondition
{
    //buff分类id
    private string[] classIDs;

    public List<BaseBuff> buffslist;

    public override bool OnCheck(BaseObject baseObject,string funcPm, params object[] param)
    {
        InitParam(funcPm);

        buffslist = baseObject.buffStackCompent.curBuffList;
        if (buffslist!= null && buffslist.Count > 0)
        {
            for (int i = 0; i < buffslist.Count; i++)
            {
                for (int d = 0; d < classIDs.Length; d++)
                {
                    if ((int)buffslist[i].mainBuff.buffCfg.classid == int.Parse(classIDs[d])) 
                    {
                        return true;
                    }
                }      
            }
        }
        return false;
    }

    /// <summary>
    /// 初始化参数
    /// </summary>
    public void InitParam(string funcPm)
    {
        classIDs = funcPm.Split('|');
    }
}
