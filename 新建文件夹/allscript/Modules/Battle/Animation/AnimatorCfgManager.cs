using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 动画事件配置管理
/// </summary>
public class AnimatorCfgManager
{
    private static AnimatorCfgManager Ins;
    public static AnimatorCfgManager ins
    {
        get
        {
            if (Ins == null)
            {
                Ins = new AnimatorCfgManager();
            }
            return Ins;
        }
    }

    //本场战斗所有动画配置，key = objid，value = 动画事件配置数据
    private Dictionary<long, AnimatorEventDataCfg> dicAllAnimatorCfg = new Dictionary<long, AnimatorEventDataCfg>();

	/// <summary>
	/// 根据对象id获得动画表现配置
	/// </summary>
	/// <param name="objID"></param>
	public AnimatorEventDataCfg GetAnimatorCfgByObjID(long objID)
	{
		if (dicAllAnimatorCfg.ContainsKey(objID))
		{
			return dicAllAnimatorCfg[objID];

		}
		else
		{
			AnimatorEventDataCfg animatorEventData = GameCenter.mIns.m_CfgMgr.JsonToSingleClass<AnimatorEventDataCfg>($"event_{objID}");
			dicAllAnimatorCfg.Add(objID, animatorEventData);
            return animatorEventData;
        }
	}


    public AnimatorEventData GetAnimatorEventDataByType(AnimatorEventDataCfg cfg,string action)
    {
        if (cfg == null)
        {
            return null;
        }
        switch (action)
        {
            case "entrance"://出场
                return cfg.entrance;
            case "idle":
                return cfg.idle;
            case "move":
                return cfg.move;
            case "stun":
                return cfg.stun;
            case "death":
                return cfg.death;
            case "attack_1":
                return cfg.attack_1;
            case "attack_2":
                return cfg.attack_2;
            case "skill1_1":
                return cfg.skill1_1;
            case "skill1_2":
                return cfg.skill1_2;
            case "skill2_1":
                return cfg.skill2_1;
            case "skill2_2":
                return cfg.skill2_2;
            case "skill3_1":
                return cfg.skill3_1;
            case "skill3_2":
                return cfg.skill3_2;
            case "showidle":
                return cfg.showidle;
            case "showloopidle":
                return cfg.showloopidle;
            default:
                return null;
        }

    }
}

