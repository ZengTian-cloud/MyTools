using System;
using System.Collections.Generic;

/// <summary>
/// 计数器buff 时机->条件->次数->执行buff
/// </summary>
public class Buff31 : BaseBuff
{
    //当前计数
    private int curStak;
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            curStak = 0;
            int timerState = int.Parse(mainBuff.buffCfg.functionpm.Split(';')[0]);
            target.buffStackCompent.AddOneBuff(this, timerState);
            return true;
        }
        return false;
    }

    public override void OnChildCheckBuff(params object[] parm)
    {
        string[] funcPm = mainBuff.buffCfg.functionpm.Split(';');
        //条件
        string[] conditions = funcPm[1].Split('|');
        //目标次数
        int target = int.Parse(funcPm[2]);
        //buff列表
        string[] buffList = funcPm[3].Split('|');

        bool bCondition = true;
        for (int i = 0; i < conditions.Length; i++)
        {
            if (conditions[i] != "-1")
            {
                if (!ConditionManager.ins.CheckCondition(mainBuff.holder, long.Parse(conditions[i])))
                {
                    bCondition = false;
                    break;
                }
            }
        }

        if (bCondition)
        {
            curStak += 1;
        }
        if (curStak >= target)
        {
            curStak = 0;
            List<BaseObject> targetList = new List<BaseObject>();
            targetList.Add(mainBuff.holder);
            for (int i = 0; i < buffList.Length; i++)
            {
                BuffManager.ins.DOBuff(long.Parse(buffList[i]), mainBuff.holder, mainBuff.holder, targetList, false, default, null, false, true, parm);
            }
        }
    }
}

