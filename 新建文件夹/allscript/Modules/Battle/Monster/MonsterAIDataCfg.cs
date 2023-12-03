using System;
using System.Collections.Generic;

/// <summary>
/// 怪物ai表配置数据管理
/// </summary>
public class MonsterAICfgDataManager
{
    private static MonsterAICfgDataManager Ins;
    public static MonsterAICfgDataManager ins
    {
        get
        {
            if (Ins == null)
            {
                Ins = new MonsterAICfgDataManager();
            }
            return Ins;
        }
    }

	private List<MonsterAIDataCfg> _monsterAIDataCfgs;
    public List<MonsterAIDataCfg> monsterAIDataCfgs { get {
			if (_monsterAIDataCfgs == null)
			{
				_monsterAIDataCfgs = GameCenter.mIns.m_CfgMgr.JsonToListClass<MonsterAIDataCfg>($"t_skill_ai");

            }
			return _monsterAIDataCfgs;

    } }

	//怪物ai数据 k-怪物AIid v-ai数据列表
	private Dictionary<long, List<MonsterAIDataCfg>> dicMonsterAICfgData;

	private void ListToDictionary()
	{
        dicMonsterAICfgData = new Dictionary<long, List<MonsterAIDataCfg>>();
		for (int i = 0; i < monsterAIDataCfgs.Count; i++)
		{
			if (!dicMonsterAICfgData.ContainsKey(monsterAIDataCfgs[i].aiid))
			{
				dicMonsterAICfgData.Add(monsterAIDataCfgs[i].aiid, new List<MonsterAIDataCfg>());
            }
			dicMonsterAICfgData[monsterAIDataCfgs[i].aiid].Add(monsterAIDataCfgs[i]);
        }
    }

	/// <summary>
	/// 获得怪物ai配置表数据
	/// </summary>
	/// <param name="aiid"></param>
	/// <returns></returns>
	public List<MonsterAIDataCfg> GetMonsterAICfgDataByAiId(long aiid)
	{
		if (dicMonsterAICfgData.ContainsKey(aiid))
		{
			return dicMonsterAICfgData[aiid];

        }
		return null;
	}
}

/// <summary>
/// 怪物ai配置表数据结构
/// </summary>
public class MonsterAIDataCfg
{
	public long aiid;//

	public long moveid;

	public string describe;

	public int condition;//生效条件

	public string param;//生效参数

	public int importance;//优先级

	public int checktype;//检测类型

	public string checkparam;//检测参数

	public long skill;//释放技能

	public int prepare;//准备时间

	public int breath;//执行下一次行为的时间

	public int cooldown;//再次执行本行为的cd

	public string resetmove;//重置行为cd

	public int nextmove;//指定下一行为的id

	public int breakdown;//被打断后 1=重新检索符合条件的行为 2=继续进行该行为检索
}

