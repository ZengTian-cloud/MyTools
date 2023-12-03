using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 释放一个技能
/// </summary>
public class Buff36 : BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            if (!string.IsNullOrEmpty(mainBuff.buffCfg.functionpm))
            {
                string[] funcPm = mainBuff.buffCfg.functionpm.Split(';');
                if (target.objType == 1)//英雄
                {
                    BaseHero baseHero = BattleHeroManager.Instance.GetBaseHeroByHeroID(long.Parse(funcPm[0]));
                    if (baseHero != null && !baseHero.bRecycle)
                    {
                        BattleSkillCfg skillCfg = null;
                        switch (funcPm[1])
                        {
                            case "1"://普攻
                                skillCfg = baseHero.baseSkillCfgData;
                                break;
                            case "2"://战技
                                skillCfg = baseHero.skill1CfgData;
                                break;
                            case "3"://秘技
                                skillCfg = baseHero.skill2CfgData;
                                break;
                            case "4"://终结技
                                skillCfg = baseHero.skill3CfgData;
                                break;
                            default:
                                break;
                        }
                        List<BaseObject> targetList = new List<BaseObject>();
                        Vector3 tagetPos = default;
                        switch (funcPm[2])
                        {
                            case "1"://沿用当前目标
                                targetList.Add(target);
                                break;
                            case "2"://自己
                                targetList.Add(mainBuff.holder);
                                break;
                            case "3"://随机怪物目标
                                List<long> guidlist3 = new List<long>(BattleMonsterManager.Instance.dicMonsterList.Keys);
                                int rang3 = UnityEngine.Random.Range(0, guidlist3.Count);
                                targetList.Add(BattleMonsterManager.Instance.GetOneMonsterByGUID(guidlist3[rang3]));
                                break;
                            case "4"://随机英雄

                                break;
                            case "5"://怪物全体

                                break;
                            case "6"://英雄全体
                                break;
                            case "7"://额外参数目标
                                if (parm.Length > 0)
                                {
                                    targetList.Add((BaseObject)parm[0]);
                                }
                                break;
                            default:
                                break;
                        }
                        if (skillCfg.guidetype != 2 && skillCfg.guidetype != 5)//非单体
                        {
                            tagetPos = targetList[0].prefabObj.transform.position;
                            if (!baseHero.isOnSkill)
                            {
                                baseHero.isOnSkill = true;
                                baseHero.OnSkillBase(tagetPos, skillCfg, false);
                            }
                            else
                            {
                                baseHero.AddWillDoSkill(tagetPos, skillCfg);
                            }
                        }
                        else
                        {
                            if (!baseHero.isOnSkill)
                            {
                                baseHero.isOnSkill = true;
                                baseHero.OnSkillBase(targetList, skillCfg, false);
                            }
                            else
                            {
                                baseHero.AddWillDoSkill(targetList, skillCfg);
                            }

                        }
                    }
                }
            }
            return true;
        }
        return false;
    }
}

