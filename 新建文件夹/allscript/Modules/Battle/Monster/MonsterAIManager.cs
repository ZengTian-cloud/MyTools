using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 怪物ai管理器
/// </summary>
public class MonsterAIManager
{
    private static MonsterAIManager Ins;
    public static MonsterAIManager ins
    {
        get
        {
            if (Ins == null)
            {
                Ins = new MonsterAIManager();
            }
            return Ins;
        }
    }

    /// <summary>
    /// 所有怪物的配置数据
    /// </summary>
    private List<MonsterAIDataCfg> _allMonsterAIDataCfgs;
    public List<MonsterAIDataCfg> allMonsterAIDataCfgs
    {
        get
        {
            if (_allMonsterAIDataCfgs == null)
            {
                _allMonsterAIDataCfgs = GameCenter.mIns.m_CfgMgr.JsonToListClass<MonsterAIDataCfg>($"t_skill_ai");
            }
            return _allMonsterAIDataCfgs;

        }
    }

    //怪物的ai数据 key-怪物id value-<k-aiid,v-ai数据>
    public  Dictionary<long,List<MonsterAIData>> allMonsterAIData = new Dictionary<long, List<MonsterAIData>>();

    //怪物的ai数据 k-行为id v-数据
    public Dictionary<long, MonsterAIData> dicAllMonsterAIData;

    /// <summary>
    /// 获得怪物ai数据
    /// </summary>
    /// <param name="monsterID"></param>
    public void GetMonsterAIDataByMonsterID(long monsterID)
    {
        int aiid = MonsterDataManager.Instance.GetMonsterCfgByMonsterID(monsterID).aiid;
    }

    /// <summary>
    /// 初始化战斗怪物的ai数据为字典
    /// </summary>
    /// <param name="baseMonsters"></param>
    public void InitAllMonsterAI()
    {
        if (dicAllMonsterAIData == null) 
        {
            dicAllMonsterAIData = new Dictionary<long, MonsterAIData>();
            for (int i = 0; i < allMonsterAIDataCfgs.Count; i++)
            {

                if (!allMonsterAIData.ContainsKey(allMonsterAIDataCfgs[i].aiid))
                {
                    allMonsterAIData.Add(allMonsterAIDataCfgs[i].aiid, new List<MonsterAIData>());
                }
                float interval = float.Parse(allMonsterAIDataCfgs[i].checkparam.Split(';')[0]) / 1000;
                MonsterAIData monsterAIData = new MonsterAIData(allMonsterAIDataCfgs[i], interval);
                allMonsterAIData[allMonsterAIDataCfgs[i].aiid].Add(monsterAIData);

                if (!dicAllMonsterAIData.ContainsKey(allMonsterAIDataCfgs[i].moveid))
                {
                    dicAllMonsterAIData.Add(allMonsterAIDataCfgs[i].moveid, monsterAIData);
                }
                else
                {
                    Debug.LogError($"存在相同的行为id，请检查！moveid:{allMonsterAIDataCfgs[i].moveid}");
                }
            }
        }
    }

    /// <summary>
    /// 获得一个怪物下所有ai行为
    /// </summary>
    /// <param name="aiid"></param>
    public List<MonsterAIData> GetMonsterAIByAIID(long aiid)
    {
        List<MonsterAIData> monsterAIDatas = new List<MonsterAIData>();
        if (allMonsterAIData.ContainsKey(aiid))
        {
            for (int i = 0; i < allMonsterAIData[aiid].Count; i++)
            {
                monsterAIDatas.Add(new MonsterAIData(allMonsterAIData[aiid][i].cfg, allMonsterAIData[aiid][i].checkInterval));
            }
            return monsterAIDatas;
        }
        return null;
    }

    /// <summary>
    /// 根据行为id获得单个ai数据
    /// </summary>
    /// <param name="moveid"></param>
    /// <returns></returns>
    public MonsterAIData GetMonsterAIDataByMoveid(long moveid)
    {
        if (dicAllMonsterAIData.ContainsKey(moveid))
        {
            return dicAllMonsterAIData[moveid];
        }
        return null;
    }


    /// <summary>
    /// 检测怪物ai的技能范围索敌
    /// </summary>
    public List<BaseObject> CheckMonsterAISkillRange(long skill,float checkRange,BaseMonster monster)
    {
       BattleSkillCfg skillCfg = BattleCfgManager.Instance.GetSkillCfgBySkillID(skill);
        List<BaseObject> targetList = null;
       switch (skillCfg.guidetype)
       {
            case 1://扇形范围
                targetList = SkillManager.ins.GetMonsterAITarget_1(monster, skillCfg);
                break;
            case 2://单体技能
                BaseObject baseObject =  SkillManager.ins.GetMonsterAITarget_2(monster, checkRange, skillCfg);
                if (baseObject!=null)
                {
                    targetList = new List<BaseObject>();
                    targetList.Add(baseObject);
                }
                break;
            case 4://矩形范围

                break;
            case 5://单体  无限
                baseObject = SkillManager.ins.GetMonsterAITarget_2(monster, checkRange, skillCfg);
                if (baseObject != null)
                {
                    targetList = new List<BaseObject>();
                    targetList.Add(baseObject);
                }
                break;
           default:
               break;
       }
        return targetList;
    }
}

public class MonsterAIData
{
    public MonsterAIDataCfg cfg;

    public bool bCondition;//条件是否满足

    public float checkTimer;//检测计时

    public float checkInterval;//检测间隔

    public bool isCoolDown;//是否可执行本行为


    public MonsterAIData(MonsterAIDataCfg monsterAIDataCfg,float checkInterval)
    {
        this.cfg = monsterAIDataCfg;
        this.bCondition = false;
        this.checkTimer = 0;
        this.isCoolDown = true;
        this.checkInterval = checkInterval;
    }

}



