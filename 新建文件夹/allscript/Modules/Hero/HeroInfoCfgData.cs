using System;
using System.Collections.Generic;

/// <summary>
/// 英雄信息配置数据
/// </summary>
public class HeroInfoCfgData
{
    public long heroid;//英雄id

    public int state;

    public int open;//是否战斗中可以用

    public int quality;//英雄品质 3=蓝 4=紫 5=橙 6=红

    public string name;

    public string name2;

    public string profile;

    public string picture1;//卡牌图标

    public string picture2;//小头像图标

    public string picture3;//立绘

    public string describe;

    public int atktype;//攻击类型 1=近战 2=远程 3=十字

    public int profession;//职业类型 1=歼灭 2=战术 3=支援 4=抵御

    public int element;//元素属性 1=水 2=火 3=风 4=雷

    public long baseskill;//基础攻击

    public long cardskill1;//卡牌技能1

    public long cardskill2;//卡牌技能2

    public long cardskill3;//卡牌技能3

    public string note;//note
    public string note1;//note1
    //等级配置
    public Dictionary<string,HeroLevelData> levelcfgMap = new Dictionary<string, HeroLevelData>();
    public Dictionary<int, HeroLevelData> levelcfgs = new Dictionary<int, HeroLevelData>();

    //突破配置
    public Dictionary<int,HeroBreakData> breakcfg = new Dictionary<int, HeroBreakData>();

    //武器升级消耗表
    public Dictionary<string, WeaponUpCostCfg> weaponLevelcfgMap = new Dictionary<string, WeaponUpCostCfg>();
    public Dictionary<int, WeaponUpCostCfg> weaponLevelcfgs = new Dictionary<int, WeaponUpCostCfg>();

    //武器突破配置
    public Dictionary<int, WeaponBreakCfg> weaponBreakcfg = new Dictionary<int, WeaponBreakCfg>();

    /// <summary>
    /// 增加武器升级配置
    /// </summary>
    /// <param name="data"></param>
    public void addWeaponLevelCfg(WeaponUpCostCfg data)
    {
        weaponLevelcfgMap.Add(data.level + "_" + data.state, data);
        if (data.weaponexp != -1)
        {
            weaponLevelcfgs.Add(data.level, data);
        }
    }
    public void addWeaponBreakCfg(WeaponBreakCfg data)
    {
        weaponBreakcfg.Add(data.state, data);
    }
    public WeaponUpCostCfg getWeaponLevelData(int level)
    {
        return weaponLevelcfgs.GetValueOrDefault(level);
    }

    public WeaponUpCostCfg getCurrWeaponLevelData(int level, int state)
    {
        return weaponLevelcfgMap.GetValueOrDefault(level + "_" + state);
    }

    public WeaponBreakCfg getCurrWeaponBreakData(int state)
    {
        return weaponBreakcfg.GetValueOrDefault(state);
    }

    /// <summary>
    /// 英雄增加升级配置
    /// </summary>
    /// <param name="data"></param>
    public void addLevelCfg(HeroLevelData data)
    {
        levelcfgMap.Add(data.herolevel+"_"+data.state,data);
        if (data.exp != -1)
        {
            levelcfgs.Add(data.herolevel, data);
        }
    }

    public void addBreakCfg(HeroBreakData data)
    {
        breakcfg.Add(data.state, data);
    }
    public HeroLevelData getLevelData(int level)
    {
        return levelcfgs.GetValueOrDefault(level);
    }
    public HeroLevelData getCurrLevelData(int level,int state)
    { 
        return levelcfgMap.GetValueOrDefault(level+"_"+state);
    }
    public HeroBreakData getCurrBreakData(int state)
    {
        return breakcfg.GetValueOrDefault(state);
    }
    public HeroBreakData getNextBrakLevel(int state)
    {
        return breakcfg.GetValueOrDefault(state+1);
    }

    /// <summary>
    /// 获得英雄所有被动
    /// </summary>
    public List<long> GetAllPassivityByHeroID(long heroID)
    {
        List<long> passivityIDs = new List<long>();
        foreach (var item in levelcfgs)
        {
            if (item.Value.talentid > 0 && item.Value.heroid == heroID)
            {
                passivityIDs.Add(item.Value.talentid);
            }
        }
        return passivityIDs;
    }

    /// <summary>
    /// 获得被动技能解锁的突破等级
    /// </summary>
    /// <param name="talentID"></param>
    /// <returns></returns>
    public int GetUnLockTanlentState(long talentID)
    {
        foreach (var item in levelcfgs)
        {
            if (item.Value.talentid == talentID)
            {
                return item.Value.state;
            }
        }
        return 0;
    }

    /// <summary>
    /// 根据突破等级获得武器当前最高等级
    /// </summary>
    /// <returns></returns>
    public int GetWaponMaxLevelByState( int state)
    {
        return getCurrWeaponBreakData(state).level;
    }

}


/// <summary>
/// 英雄天赋表
/// </summary>
public class HeroTalentCfg
{
    public long heroid;//英雄id

    public int herostar;//英雄星级

    public int costid;//升星消耗

    public long talentid;//天赋id

}

public class HeroLevelData
{
    //英雄编号
    public long heroid;
    //等级
    public int herolevel;
    //突破等级
    public int state;
    //突破消耗材料
    public long costid = 0;
    //消耗经验
    public int exp;
    //解锁的被动技能
    public long talentid;
    //升级影响的属性
    public List<StatData> attrs = new List<StatData>();
}

public class HeroBreakData
{
    //等级
    public int herolevel;
    //英雄编号
    public long heroid;
    //突破等级
    public int state;
    //突破消耗的材料
    public List<CostData> costs = new List<CostData>();
    //突破影响的属性
    public List<StatData> attrs = new List<StatData>();
    //解锁的被动技能
    public long talentid;

}
public class StatData
{
    public long statid;
    public string text;
    public int show;
    public int vtype;
    public int val;
    public int sort;
    public StatData(long statid,string text,int show,int vtype,int sort, int val)
    { 
        this.statid = statid;
        this.show = show; 
        this.vtype = vtype;
        this.text = text;
        this.val = val;
        this.sort = sort;
    }

}
public class CostData
{
    public long propid;
    public int count;
}

