using System;
using System.Collections.Generic;
using UnityEngine;

public static class BattleHeroHelp
{
	/// <summary>
	/// 获得英雄普通攻击列表-近战
	/// </summary>
	public static void GetBaseSkillTartget_1(BaseHero baseHero, List<BaseObject> checkList)
	{
		List<BaseObject> targetList;
		int shizistate = 0;
        targetList = baseHero.GetBaseSkillMonsterList(checkList, baseHero.baseSkillCfgData.guiderange, out shizistate);
        ///筛选怪物类型
        targetList = ScreenMonsterType(targetList, baseHero.baseSkillCfgData);
        if (targetList.Count > 0)
        {
            baseHero.roleObj.transform.LookAt(targetList[0].prefabObj.transform);
            baseHero.OnAtkBase(targetList, baseHero.baseSkillCfgData.guiderange);
        }
    }

    /// <summary>
    /// 获得英雄普通攻击列表-单体
    /// </summary>
    /// <param name="baseHero"></param>
    /// <param name="checkList"></param>
    /// <param name="guiderange"></param>
    public static void GetBaseSkillTartget_2(BaseHero baseHero, List<BaseObject> checkList)
    {
        List<BaseObject> targetList;
        int shizistate = 0;
        targetList = baseHero.GetBaseSkillMonsterList(checkList, baseHero.baseSkillCfgData.guiderange, out shizistate);
        ///筛选怪物类型
        targetList = ScreenMonsterType(targetList, baseHero.baseSkillCfgData);
        if (targetList.Count > 0)
        {
            baseHero.roleObj.transform.LookAt(targetList[0].prefabObj.transform);
            baseHero.OnAtkBase(targetList);
        }
    }

    /// <summary>
    /// 获得英雄普通攻击列表-十字-选一边
    /// </summary>
    /// <param name="baseHero"></param>
    /// <param name="checkList"></param>
    public static void GetBaseSkillTartget_3(BaseHero baseHero, List<BaseObject> checkList)
    {
        List<BaseObject> targetList;
        int shizistate = 0;
        targetList = baseHero.GetBaseSkillMonsterList(checkList, baseHero.baseSkillCfgData.guiderange, out shizistate);
        ///筛选怪物类型
        targetList = ScreenMonsterType(targetList, baseHero.baseSkillCfgData);
        if (targetList.Count > 0)
        {
            baseHero.roleObj.transform.localRotation = Quaternion.Euler(0, (shizistate - 1) * 90, 0);
            baseHero.OnAtkBase(targetList, baseHero.baseSkillCfgData.guiderange, shizistate);
        }
    }



    //筛选怪物类型(筛选飞行怪)
    private static List<BaseObject> ScreenMonsterType(List<BaseObject> targetList, BattleSkillCfg battleSkillCfg)
    {
        List<BattleSkillBulletCfg> bulletCfgs = new List<BattleSkillBulletCfg>();
        string[] bullets = battleSkillCfg.bulletids.Split('|');
        for (int i = 0; i < bullets.Length; i++)
        {
            bulletCfgs.Add(BattleCfgManager.Instance.GetBulletCfg(long.Parse(bullets[i])));
        }


        string[] logics = bulletCfgs[0].logicid.Split('|');
        string flytarget = BattleCfgManager.Instance.GetLogicCfg(long.Parse(logics[0])).flytarget;
        string[] par = flytarget.Split('|');
        for (int i = targetList.Count - 1; i >= 0; i--)
        {
            if (targetList[i] != null && !targetList[i].bRecycle)
            {
                int isfly = targetList[i].bFly;
                bool checkFly = false;
                for (int w = 0; w < par.Length; w++)
                {
                    if (int.Parse(par[w]) == isfly)
                    {
                        checkFly = true;
                    }

                }
                if (!checkFly)
                {
                    targetList.Remove(targetList[i]);
                }
            }
            else
            {
                targetList.Remove(targetList[i]);
            }
        }
        return targetList;
    }
}

