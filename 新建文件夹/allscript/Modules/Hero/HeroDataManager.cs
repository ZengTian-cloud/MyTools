using System;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class HeroDataManager: SingletonNotMono<HeroDataManager>
{
    public Dictionary<long, HeroData> HeroList { get {
            if (heroList != null)
            {
                return heroList;
            }
            return null;
    } }

    //英雄列表
    private Dictionary<long, HeroData> heroList = new Dictionary<long, HeroData>();

    private List<HeroData> heroDataList = new List<HeroData>();

    //天赋配置表
    private List<HeroTalentCfg> heroTalentCfgs;
    private Dictionary<long, List<HeroTalentCfg>> dicHeroTanlentCfgs;

    //添加一个英雄数据
    public void AddOrUpdateHero(JsonData heroData)
    {
        //英雄ID
        long heroid = long.Parse(heroData.ContainsKey("heroid") ? heroData["heroid"].ToString() : "0");
        //英雄等级
        int level = int.Parse(heroData.ContainsKey("lv") ? heroData["lv"].ToString() : "0");
        //武器等级
        int weaponlevel = int.Parse(heroData.ContainsKey("weaponlevel") ? heroData["weaponlevel"].ToString() : "0");
        //当前经验
        int lvexp = int.Parse(heroData.ContainsKey("lvexp") ? heroData["lvexp"].ToString() : "0");
        //武器当前经验
        int weaponexp = int.Parse(heroData.ContainsKey("weaponexp") ? heroData["weaponexp"].ToString() : "0");
        //突破等级
        int state = int.Parse(heroData.ContainsKey("state") ? heroData["state"].ToString() : "0");
        //星级
        int star = int.Parse(heroData.ContainsKey("star") ? heroData["star"].ToString() : "0");
        //战力
        long ce = long.Parse(heroData.ContainsKey("ce") ? heroData["ce"].ToString() : "0");
        //武器id
        long weaponid = long.Parse(heroData.ContainsKey("weaponid") ? heroData["weaponid"].ToString() : "0");
        //武器突破
        int weaponstate = int.Parse(heroData.ContainsKey("weaponstate") ? heroData["weaponstate"].ToString() : "0");

        List<WeaponInfo> weaponinfo = null;
        if (heroData.ContainsKey("weaponinfo"))
        {
            if (heroData["weaponinfo"].ContainsKey("map"))
            {
                weaponinfo = JsonMapper.ToObject<List<WeaponInfo>>(JsonMapper.ToJson(heroData["weaponinfo"]["map"]));
            }
        }

        //技能
        JsonData skillsData = heroData["skills"];
        List<string> skillkeys = new List<string>(skillsData.Keys);
        Dictionary<long, int> skills = new Dictionary<long, int>();
        long skillid;
        int skilllevel;
        for (int i = 0; i < skillkeys.Count; i++)
        {
            skillid = long.Parse(skillkeys[i]);
            skilllevel = int.Parse(skillsData[skillkeys[i]].ToString());
            skills.Add(skillid, skilllevel);
        }

        //属性
        JsonData attrsData = heroData["attrs"];
        List<string> keys = new List<string>(attrsData.Keys);
        Dictionary<long, float> attrs = new Dictionary<long, float>();
        long attrid;
        float value;
        for (int i = 0; i < keys.Count; i++)
        {
            attrid = long.Parse(keys[i]);
            value = float.Parse(attrsData[keys[i]].ToString());
            attrs.Add(attrid, value);
        }

        HeroData data = null;
        if (heroList.ContainsKey(heroid))
        {
            Debug.LogWarning($"添加相同英雄！heroid:{heroid}");
            data = heroList[heroid];
            data.update(heroid, lvexp, level, state, star, skills, attrs, weaponid, weaponstate, weaponinfo, weaponlevel, weaponexp);
            return;
        }
        data = new HeroData(heroid, lvexp,level, state,star, skills, attrs, weaponid, weaponstate, weaponinfo, weaponlevel, weaponexp);
   
        heroList.Add(data.heroID, data);
        heroDataList.Add(data);
    }

    /// <summary>
    /// 获得玩家当前英雄列表
    /// </summary>
    public List<HeroData> GetHeroDataList()
    {
        return heroDataList;
    }

    /// <summary>
    /// 获得英雄数据
    /// </summary>
    public HeroData GetHeroData(long heroId)
    {
        foreach (var hd in heroDataList)
        {
            if (hd.heroID == heroId)
            {
                return hd;
            }
        }
        return null;
    }

    /// <summary>
    /// 获得英雄已解锁天赋列表
    /// </summary>
    /// <param name="heroid"></param>
    /// <param name="star"></param>
    /// <returns></returns>
    public List<long> GetHeroTalentByHeroIDAndStar(long heroid,int star)
    {
        if (heroTalentCfgs == null)
        {
            InitHeroTalentCfg();
        }
        List<long> talentidList = new List<long>();
        if (dicHeroTanlentCfgs.ContainsKey(heroid))
        {
            for (int i = 0; i < dicHeroTanlentCfgs[heroid].Count; i++)
            {
                if (star >= dicHeroTanlentCfgs[heroid][i].herostar) 
                {
                    if (dicHeroTanlentCfgs[heroid][i].talentid > 0)
                    {
                        talentidList.Add(dicHeroTanlentCfgs[heroid][i].talentid);
                    }

                }
            }
            return talentidList;
        }
        Debug.LogError($"未在「t_hero_talent」表中找到heroid:{heroid}的配置，请检查！");
        return null;
    }

    /// <summary>
    /// 获得英雄指定星级的天赋
    /// </summary>
    /// <param name="heroid"></param>
    /// <param name="star"></param>
    public HeroTalentCfg GetHeroTalent(long heroid, int star)
    {
        if (heroTalentCfgs == null)
        {
            InitHeroTalentCfg();
        }
        if (dicHeroTanlentCfgs.ContainsKey(heroid))
        {
            return dicHeroTanlentCfgs[heroid].Find(x => x.herostar == star);
        }
        return null;
    }

    /// <summary>
    /// 初始化英雄天赋配置表
    /// </summary>
    private void InitHeroTalentCfg()
    {
        heroTalentCfgs = GameCenter.mIns.m_CfgMgr.JsonToListClass<HeroTalentCfg>("t_hero_talent");
        dicHeroTanlentCfgs = new Dictionary<long, List<HeroTalentCfg>>();
        for (int i = 0; i < heroTalentCfgs.Count; i++)
        {
            if (!dicHeroTanlentCfgs.ContainsKey(heroTalentCfgs[i].heroid))
            {
                dicHeroTanlentCfgs.Add(heroTalentCfgs[i].heroid, new List<HeroTalentCfg>());
            }
            dicHeroTanlentCfgs[heroTalentCfgs[i].heroid].Add(heroTalentCfgs[i]);
        }
    }
}



