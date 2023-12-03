using System;
using System.Collections.Generic;

/// <summary>
/// 武器升级消耗表
/// </summary>
public class WeaponUpCostCfg
{
    public long heroid;//英雄编号

    public int state;//突破等级

    public int level;//英雄等级

    public int weaponexp;//消耗的经验

    public long breakcost;//突破消耗

}

public class WeaponBreakCfg
{
    //等级
    public int level;//等级
    //英雄编号
    public long heroid;//英雄编号
    //突破等级
    public int state;
    //突破消耗的材料
    public List<CostData> costs = new List<CostData>();

}
