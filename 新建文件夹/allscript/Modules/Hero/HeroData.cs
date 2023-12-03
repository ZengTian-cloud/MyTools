using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 玩家英雄数据
/// </summary>
public class HeroData
{
	//英雄ID
	public long heroID { get; private set; }

	//等级
	public int level { get; private set; }

    public int weaponLevel { get; private set; }

    //等级当前经验
    public int lvexp { get; private set; }

    //武器当前经验
    public int weaponexp { get; private set; }

    //突破等级
    public int state { get; private set; }

	//英雄星级
	public int star { get; private set; }

	//武器ID
	public long weaponid { get;private set; }

	//武器突破
	public int weaponstate { get; private set; }

    public List<WeaponInfo> weaponinfo { get; private set; }

    //技能 key-技能id，value-等级
    public Dictionary<long, int> skills { get; private set; }

	//属性值列表 key-属性ID value-属性值
	public Dictionary<long,float> attrs { get; private set; }

	//战力值（暂无）
	public long ce { get; private set; }

    // 队伍id
    public int teamId { get; set; }


    public HeroData(long heroID, int lvexp,int level, int state, int star, Dictionary<long,int> skills, Dictionary<long, float> attrs, long weaponid, int weaponstate, List<WeaponInfo> weaponInfos, int weaponlevel, int weaponexp, long ce = 0)
    {
        update(heroID, lvexp, level, state, star, skills, attrs, weaponid, weaponstate, weaponInfos, weaponlevel, weaponexp, ce);
    }

    public void update(long heroID, int lvexp, int level, int state, int star, Dictionary<long, int> skills, Dictionary<long, float> attrs, long weaponid, int weaponstate, List<WeaponInfo> weaponInfos,int weaponlevel,int weaponexp, long ce = 0)
    {
        
        this.heroID = heroID;
        this.lvexp = lvexp;
        this.level = level;
        this.state = state;
        this.star = star;
        this.skills = skills;
        this.attrs = attrs;
        this.weaponid = weaponid;
        this.weaponstate = weaponstate;
        this.weaponinfo = weaponInfos;
        this.ce = ce;
        this.weaponLevel = weaponlevel;
        this.weaponexp = weaponexp;

        // test team
        // TODO: 目前demo阶段默认给英雄101001,101003为队伍1
        teamId = (heroID == 101001 || heroID == 101003) ? 1 : 0;
    }

    /// <summary>
    /// 武器是否解锁
    /// </summary>
    /// <returns></returns>
    public bool CheckWeaponUnLock(long weaponID)
    {
        for (int i = 0; i < weaponinfo.Count; i++)
        {
            if (weaponinfo[i].weaponid == weaponID)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获得武器星级
    /// </summary>
    /// <param name="weaponID"></param>
    /// <returns></returns>
    public int GetWeaponStar(long weaponID)
    {
        for (int i = 0; i < weaponinfo.Count; i++)
        {
            if (weaponinfo[i].weaponid == weaponID)
            {
                return weaponinfo[i].star;
            }
        }
        return -1;
    }

    /// <summary>
    /// 获得属性值
    /// </summary>
    /// <param name="attrID"></param>
    public float  GetAttrByAttrID(long attrID)
	{
		if (attrs != null && attrs.ContainsKey(attrID))
		{
			return attrs[attrID];

        }
		return 0;
    }

    /// <summary>
    /// 获取属性值并格式化转换为字符串
    /// </summary>
    /// <param name="attrID"></param>
    /// <returns></returns>
    public string GetAttrByAttrIdToStr(long attrID,int vtype)
    {
        float val = GetAttrByAttrID(attrID);
        switch (vtype)
        {
            case 1:
            default:
                return val.ToString();
            case 2:
                return Math.Round(val * 0.1, 1).ToString();
            case 3:
                return Math.Round(val * 0.01, 2) + "%";
        }   
    }

    /// <summary>
    /// 获取武器并格式化转换为字符串
    /// </summary>
    /// <param name="attrID"></param>
    /// <returns></returns>
    public string GetWeaponAttrToStr(int attrValue, int vtype)
    {
        switch (vtype)
        {
            case 1:
            default:
                return attrValue.ToString();
            case 2:
                return Math.Round(attrValue * 0.1, 1).ToString();
            case 3:
                return Math.Round(attrValue * 0.01, 2) + "%";
        }
    }



    /// <summary>
    /// 获取属性值并格式化转换为字符串
    /// </summary>
    /// <param name="attrID"></param>
    /// <returns></returns>
    public string GeAttrByAttrIdToStr(long attrID, int vtype)
    {
        float val = GetAttrByAttrID(attrID);
        switch (vtype)
        {
            case 1:
            default:
                return val.ToString();
            case 2:
                return Math.Round(val * 0.1, 1).ToString();
            case 3:
                return Math.Round(val * 0.01, 2) + "%";
        }
    }

    /// <summary>
    /// 将属性值格式化并返回字符串
    /// </summary>
    /// <param name="val"></param>
    /// <param name="vtype"></param>
    /// <returns></returns>
    public string GetAttrByValIdToStr(float val, int vtype)
    {
        switch (vtype)
        {
            case 1:
            default:
                return val.ToString();
            case 2:
                return Math.Round(val * 0.1, 1).ToString();
            case 3:
                return Math.Round(val *0.01, 2) + "%";
        }
    }

    /// <summary>
    /// 获得属性值
    /// </summary>
    /// <param name="attrID"></param>
    public float GetAttrByAttrID(EAttr attr)
    {
        return GetAttrByAttrID((long)attr);
    }


    /// <summary>
    /// 获得技能等级
    /// </summary>
    /// <param name="attrID"></param>
    public int GetSkillLvBySkillID(long skillid)
    {
        if (skills != null && skills.ContainsKey(skillid))
        {
            return skills[skillid];

        }
        return -1;
    }

    
    /// <summary>
    /// 获得元素图标
    /// </summary>
    /// <param name="ysID">元素id</param>
    /// <returns></returns>
    public Sprite GetYuansuIcon(int ysID)
	{
		switch (ysID)
		{
			case 1:
				return SpriteManager.Instance.GetSpriteSync("ui_c_icon_shui");
			case 2:
                return SpriteManager.Instance.GetSpriteSync("ui_c_icon_huo");
            case 3:
                return SpriteManager.Instance.GetSpriteSync("ui_c_icon_feng");
            case 4:
                return SpriteManager.Instance.GetSpriteSync("ui_c_icon_lei");
			default:
				return null;
		}
	}

    /// <summary>
    /// 获得职业图标
    /// </summary>
    /// <param name="ysID">元素id</param>
    /// <returns></returns>
    public Sprite GetZhiYeIcon(int zyID, bool isWhite = false)
    {
        switch (zyID)
        {
            case 1:
                return !isWhite ? SpriteManager.Instance.GetSpriteSync("ui_c_icon_jianmie") : SpriteManager.Instance.GetSpriteSync("ui_c_icon_jianmie_bai");
            case 2:
                return !isWhite ? SpriteManager.Instance.GetSpriteSync("ui_c_icon_zhanshu") : SpriteManager.Instance.GetSpriteSync("ui_c_icon_zhanshu_bai");
            case 3:
                return !isWhite ? SpriteManager.Instance.GetSpriteSync("ui_c_icon_zhiyuan") : SpriteManager.Instance.GetSpriteSync("ui_c_icon_zhanshu_bai");
            case 4:
                return !isWhite ? SpriteManager.Instance.GetSpriteSync("ui_c_icon_diyu") : SpriteManager.Instance.GetSpriteSync("ui_c_icon_zhanshu_bai");
            default:
                return null;
        }
    }

    /// <summary>
    /// 获得职业名称
    /// </summary>
    /// <param name="ysID">元素id</param>
    /// <returns></returns>
    public string GetZhiYeName(int zyID)
    {
        switch (zyID)
        {
            case 1:
                return "歼灭";
            case 2:
                return "战术";
            case 3:
                return "支援";
            case 4:
                return "抵御";
            default:
                return "";
        }
    }
    public int GetTeamId()
    {
        return teamId;
    }

    public void SetTeamId(int value)
    {
        teamId = value;
    }
}

public class WeaponInfo
{
    public long weaponid;

    public int star;
}

/// <summary>
/// 属性枚举
/// </summary>
public enum EAttr
{
    ATK = 10100101,//攻击力

    DEF = 10100102,//防御

    HP = 10100103,//最大生命值

    Hit_Rate = 10100104,//攻速

    Speed = 10100113,//移动速度

    ATK_Per = 10200101,//攻击百分比

    DEF_Per = 10200102,//防御百分比

    MaxHP_Per = 10200103,//最大生命值百分比

    Hit_Rate_Per = 10200104,//攻速百分比

    Crit = 10200105,//暴击

    Crit_Resist = 10200106,//抗暴

    Crit_Dmg = 10200107,//爆伤

    Crit_Def = 10200108,//爆防

    Dmg_Per = 10200109,//造成总伤害提高

    TakeDmg_Per = 10200110,//收到总伤害降低

    Heal_Per = 10200111,//治疗量百分比

    TakeHeal_Per = 10200112,//收到治疗效果提升百分比

    Speed_Per = 10200113,//移动速度百分比

    Dmg_Normal = 10200401,//造成普通伤害提升

    Dmg_Water = 10200402,//造成水元素伤害提升

    Dmg_Fire = 10200403,//造成火元素伤害提升

    Dmg_Wind = 10200404,//造成风元素伤害提升

    Dmg_Thunder = 10200405,//造成雷元素伤害提升

    TakeDmg_Normal = 10200406,//受到普通伤害提升

    TakeDmg_Water = 10200407,//受到水元素伤害提升

    TakeDmg_Fire = 10200408,//受到火元素伤害提升

    TakeDmg_Wind = 10200409,//受到风元素伤害提升

    TakeDmg_Thunder = 10200410,//受到雷元素伤害提升

    Dmg_NormalSkill = 10200301,//普通攻击伤害提升

    Dmg_BattleSkill = 10200302,//战技伤害提升

    Dmg_TactickSkill = 10200303,//秘技伤害提升

    Dmg_UltrasSkill = 10200304,//终结技伤害提升

    Dmg_ActiveSkill = 10200305,//主动技伤害提升

    TakeDmg_NormalSkill = 10200306,//受到普通攻击伤害提升

    TakeDmg_BattleSkill = 10200307,//受到战技伤害提升

    TakeDmg_TactickSkill = 10200308,//受到秘技伤害提升

    TakeDmg_UltrasSkill = 10200309,//受到终结技伤害提升

    TakeDmg_ActiveSkill = 10200310,//受到主动技伤害提升
}

