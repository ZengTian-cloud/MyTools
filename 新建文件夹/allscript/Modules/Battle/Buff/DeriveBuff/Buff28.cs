using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 改变属性 对特定怪物类型额外加成
/// </summary>
public class Buff28 : BaseBuff
{
    private long attrID;
    private float value;
    private float addValue;
    private string[] monsterTypes;

    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle && target.objType == 2)
        {
            InitValue();
            BaseMonster baseMonster = (BaseMonster)target;
            int state = target.buffStackCompent.AddOneBuff(this, 101);
            switch (state)
            {
                case -1://
                    break;
                case 1://改变属性
                    bool bAdd = false;
                    for (int i = 0; i < monsterTypes.Length; i++)
                    {
                        if (int.Parse(monsterTypes[i]) == baseMonster.cfgdata.type)
                        {
                            bAdd = true;
                            break;
                        }
                    }
                    target.AttrChange(attrID, bAdd ? value + addValue : value);
                    return true;
                default:
                    break;
            }
        }
        return false;
    }

    private void InitValue()
    {
        string[] funcPm = mainBuff.buffCfg.functionpm.Split(';');
        attrID = long.Parse(funcPm[0]);
        addValue = float.Parse(funcPm[1]);
        monsterTypes = funcPm[2].Split('|');

        if (!string.IsNullOrEmpty(mainBuff.buffCfg.value))
        {
            value = float.Parse(mainBuff.buffCfg.value);
        }
        else
        {
            value = DamageTool.GetSkillValue(mainBuff.skillCfg, mainBuff.buffCfg, mainBuff.holder);
        }
    }
}

