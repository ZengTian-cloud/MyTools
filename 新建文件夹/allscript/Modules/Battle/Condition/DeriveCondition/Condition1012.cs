using System;

/// <summary>
/// 释放的技能是否是 指定元素 的 指定（技能类型） 技能+概率
/// </summary>
public class Condition1012 : BaseCondition
{
	//配置的元素列表
	private string[] pmElement;
	//配置的技能类型
	private string[] pmSkillType;
    private string range;

	private bool bElement;
	private bool bSkillType;

    private bool bRandm;
	public override bool OnCheck(BaseObject baseObject, string funcPm, params object[] param)
	{
        if (baseObject.objType != 1)
        {
            return false;
        }

		InitParam(funcPm);

        BattleSkillCfg skillcfg = (BattleSkillCfg)param[0];
        bElement = false;
		for (int i = 0; i < pmElement.Length; i++)
		{
			if (int.Parse(pmElement[i]) != -1)
			{
				if (skillcfg.skillelement == int.Parse(pmElement[i]))
				{
					bElement = true;
					break;
				}
			}
			else
			{
                bElement = true;
                break;
            }
		}

		bSkillType = false;
        for (int i = 0; i < pmSkillType.Length; i++)
        {
            if (int.Parse(pmSkillType[i]) != -1)
            {
                if (int.Parse(pmSkillType[i]) == skillcfg.skilltype)
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


        bRandm = false;
        Condition1004 cond = new Condition1004();
        bRandm = cond.OnCheck(null, range);

        return bSkillType && bElement;//&& bRandm;
    }

	private void InitParam(string funcPm)
	{
        string[] conditionPm = funcPm.Split(';');
        pmElement = conditionPm[0].Split('|');
        pmSkillType = conditionPm[1].Split('|');
        range = conditionPm[2];
    }
}

