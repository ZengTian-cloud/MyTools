using System;
using System.Collections.Generic;

/// <summary>
/// 全局buff 释放技能后，条件检测
/// </summary>
public class Buff24:BaseBuff
{
   
    public override bool OnChlidStart(params object[] parm)
    {
        target.buffStackCompent.AddOneBuff(this, 11);
        return true;
    }

    public override void OnChildCheckBuff(params object[] parm)
    {
        BattleSkillCfg skillCfg = (BattleSkillCfg)parm[1];
        BaseObject skillHolder = (BaseObject)parm[2];

        string[] funcPm;
        bool bConditon = true;
        if (!string.IsNullOrEmpty(this.mainBuff.buffCfg.functionpm) )
        {
            funcPm = this.mainBuff.buffCfg.functionpm.Split(';');

            string[] conditions = funcPm[0].Split('|');
            for (int i = 0; i < conditions.Length; i++)
            {
                if (!ConditionManager.ins.CheckCondition(skillHolder,long.Parse(conditions[i]), skillCfg))
                {
                    bConditon = false;
                    break;
                }
            }

            if (bConditon)
            {
                string[] buffs = funcPm[1].Split('|');
                string[] target = funcPm[2].Split('|');

                List<BaseObject> targetList = new List<BaseObject>();
                for (int i = 0; i < target.Length; i++)
                {
                    switch (target[i])
                    {
                        case "-1"://友方全体
                            {
                                foreach (var item in BattleHeroManager.Instance.depolyHeroList)
                                {
                                    targetList.Add(item);
                                }
                            }
                            break;
                        case "0"://buff持有者
                            targetList.Add(mainBuff.holder);
                            break;
                        default:
                            break;
                    }
                }

                for (int i = 0; i < buffs.Length; i++)
                {
                    BuffManager.ins.DOBuff(long.Parse(buffs[i]), mainBuff.holder, mainBuff.holder, targetList, false, default, null, true);
                }
            }
        }

        

    }

    public override void RemoveBuffChild(BaseObject target)
    {
        
    }
}

