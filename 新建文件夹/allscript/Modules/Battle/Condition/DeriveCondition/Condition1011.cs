using System;

/// <summary>
/// 是否是指定角色释放指定技能
/// </summary>
public class Condition1011: BaseCondition
{
    //配置角色列表
    private string[] pmTarget;
    //配置技能类型列表
    private string[] skillType;


    private bool bTarget;
    private bool bSkillType;

    
    public override bool OnCheck(BaseObject baseObject,string funcPm, params object[] param)
    {
        InitParm(funcPm);

        bTarget = false;
        for (int i = 0; i < pmTarget.Length; i++)
        {
            if (long.Parse(pmTarget[i]) != -1)
            {
                if (baseObject.objID == long.Parse(pmTarget[i]))
                {
                    bTarget = true;
                    break;
                }
            }
            else
            {
                bTarget = true;
                break;
            }
        }

        BattleSkillCfg skillcfg = (BattleSkillCfg)param[0];
        bSkillType = false;
        for (int i = 0; i < skillType.Length; i++)
        {
            if (int.Parse(skillType[i]) != -1)
            {
                if (int.Parse(skillType[i]) == skillcfg.skilltype)
                {
                    bSkillType = true;
                    break;
                }
            }
            else
            {
                bSkillType = true;
                break;
            }
        }
        return bSkillType && bTarget;
    }

    private void InitParm(string funcPm)
    {
        string[] conditionPm = funcPm.Split(';');
        pmTarget = conditionPm[0].Split('|');
        skillType = conditionPm[1].Split('|');
    }
}

