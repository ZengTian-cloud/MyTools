using System;
using System.Collections.Generic;
/// <summary>
/// 武器配置数据
/// </summary>
public class WeaponDataCfg
{
	public long weaponid;//武器id

	public long heroid;//可用英雄

	public string describe;//描述

	public string name;//名称

	public string note;//描述

	public string icon;

	public int quality;//品质

	public int type;

	public long cost;//激活消耗

	public long attrmode;//数值模版

    //武器数值模版表
    private Dictionary<string, WeaponAttrMode> weaponAttrModeCfgs = new Dictionary<string, WeaponAttrMode>();
    //升星数据
    private Dictionary<int, WeaponStarData> weaponStarCfgs = new Dictionary<int, WeaponStarData>();

    public void addWeaponAttrMode(WeaponAttrMode mode)
    {
        weaponAttrModeCfgs.TryAdd($"{mode.level}_{mode.state}", mode);
    }

    public WeaponAttrMode getWeaponAttr(int level,int state)
    {
        return weaponAttrModeCfgs.GetValueOrDefault($"{level}_{state}");
    }
    /// <summary>
    /// 获取武器属性值 
    /// </summary>
    /// <param name="level"></param>
    /// <param name="state"></param>
    /// <param name="statid"></param>
    /// <returns></returns>
    public int getWeaponAttrStatsVal(int level, int state,long statid)
    {
        WeaponAttrMode mode = getWeaponAttr(level, state);
        if (mode==null)
        {
            return 0;
        }
        List<StatData> list = mode.attrs;
        if (list == null)
        {
            return 0;
        }
        foreach (StatData attr in list)
        {
            if (attr.statid == statid)
            {
                return attr.val;
            }
        }
        return 0;
    }

    public void addWeaponStarData(WeaponStarData data)
    {
        weaponStarCfgs.TryAdd(data.star,data);
    }

    public WeaponStarData getStarData(int star)
    {
        return weaponStarCfgs.GetValueOrDefault(star);
    }
    public int getMaxStar()
    {
        return weaponStarCfgs.Count;
    }

}
public class WeaponAttrMode
{
    public long attrmode;

    public int level;

    public int state;

    //突破影响的属性
    public List<StatData> attrs = new List<StatData>();
}
public class WeaponStarData
{
    public long weaponid;

    public int star;

    public List<CostData> costs = new List<CostData>();//升星消耗

    public long weaponskill;
}
