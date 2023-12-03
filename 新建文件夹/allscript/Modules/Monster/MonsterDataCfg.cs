using System;
using System.Collections.Generic;

/// <summary>
/// 怪物配置表数据
/// </summary>
public class MonsterDataCfg
{
	public long monsterid;//怪物id

	public long modelid;//怪物模型id

	public int type;//怪物类型 1-普通 2-精英 3-boss

	public int isfly;//是否飞行怪 1-true 0-false

	public int modelsize;//模型大小 百分位scale

	public int hitvolume;//受击体积半径

	public string describe;

	public string name;//名字

	public string note;//描述

	public int crash;//入怪扣血量

	public string skill;//技能

	public int aiid;//

	public string attr;//属性

    public string talent;//天赋

    public string icon;//icon

    public string showskill;// 展示的技能

    public string showtalent;// 展示的天赋

    public Dictionary<long, float> attrs;//怪物属性字典


    /// <summary>
    /// 获得属性值
    /// </summary>
    /// <param name="attrID"></param>
    public float GetAttrByAttrID(long attrID)
    {
        if (attrs != null && attrs.ContainsKey(attrID))
        {
            return attrs[attrID];

        }
        return 0;
    }
}

/// <summary>
/// 怪物成长表
/// </summary>
public class MonsterLevelCfgData
{
	public int monsterlevel;//等级

	public string attrgrowth;//怪物属性

	public Dictionary<long, float> attrs;

    /// <summary>
    /// 获得属性值
    /// </summary>
    /// <param name="attrID"></param>
    public float GetAttrByAttrID(long attrID)
    {
        if (attrs != null && attrs.ContainsKey(attrID))
        {
            return attrs[attrID];

        }
        return 0;
    }
}
