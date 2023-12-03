using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDataManager: SingletonNotMono<MonsterDataManager>
{

    private List<MonsterDataCfg> _monsterDataList;
    public List<MonsterDataCfg> MonsterDataList { get {
            if (_monsterDataList == null)
            {
                _monsterDataList = GameCenter.mIns.m_CfgMgr.JsonToListClass<MonsterDataCfg>("t_monster");
                AttrDataToDic();
            }
            return _monsterDataList;
    } }

    private List<MonsterLevelCfgData> _monsterLevelCfgDataList;
    public List<MonsterLevelCfgData> monsterLevelCfgDatas {
        get {
            if (_monsterLevelCfgDataList == null)
            {
                _monsterLevelCfgDataList = GameCenter.mIns.m_CfgMgr.JsonToListClass<MonsterLevelCfgData>("t_monsterlevel");
                AttrDataToDic_Level();
            }
            return _monsterLevelCfgDataList;
        }
    }

    private Dictionary<long, MonsterDataCfg> dicMonsterCfg;
    private Dictionary<long, MonsterLevelCfgData> dicMonsterlevelCfg;

    private void MonsterDataListToDicionary()
    {
        dicMonsterCfg = new Dictionary<long, MonsterDataCfg>();
        for (int i = 0; i < MonsterDataList.Count; i++)
        {
            dicMonsterCfg.Add(MonsterDataList[i].monsterid, MonsterDataList[i]);
        }
    }

    private void MonsterLevelDataListToDicionary()
    {
        dicMonsterlevelCfg = new Dictionary<long, MonsterLevelCfgData>();
        for (int i = 0; i < monsterLevelCfgDatas.Count; i++)
        {
            dicMonsterlevelCfg.Add(monsterLevelCfgDatas[i].monsterlevel, monsterLevelCfgDatas[i]);
        }
    }

    public MonsterDataCfg GetMonsterCfgByMonsterID(long monsterID)
    {
        if (dicMonsterCfg == null)
        {
            MonsterDataListToDicionary();
        }
        if (dicMonsterCfg.ContainsKey(monsterID))
        {
            return dicMonsterCfg[monsterID];
        }
        Debug.LogError($"未在配置表内找到monsterid为{monsterID}的怪物，请检查！");
        return null;
    }

    public MonsterLevelCfgData GetMonsterLevelCfgByMonsterID(int level)
    {
        if (dicMonsterlevelCfg == null)
        {
            MonsterLevelDataListToDicionary();
        }
        if (dicMonsterlevelCfg.ContainsKey(level))
        {
            return dicMonsterlevelCfg[level];
        }
        Debug.LogError($"未在配置表内找到等级为{level}的怪物系数配置，请检查！");
        return null;
    }

    /// <summary>
    /// 获得指定id怪物的指定attrid的属性值
    /// </summary>
    /// <param name="monsterid"></param>
    /// <param name="attrid"></param>
    /// <returns></returns>
    public float GetMonsterAttrByMonsterIDAndAttrID(long monsterid,long attrid)
    {
        MonsterDataCfg monsterDataCfg = GetMonsterCfgByMonsterID(monsterid);
        if (monsterDataCfg.attrs.ContainsKey(attrid))
        {
            return monsterDataCfg.attrs[attrid];
        }
        return 0;
    }

    /// <summary>
    /// 获得怪物指定等级的属性系数值
    /// </summary>
    /// <returns></returns>
    public float GetMonsterLevelAttrByLvAndAttrid(int level,long attrid)
    {
        MonsterLevelCfgData monsterLevelCfgData = GetMonsterLevelCfgByMonsterID(level);
        if (monsterLevelCfgData.attrs.ContainsKey(attrid))
        {
            return monsterLevelCfgData.attrs[attrid];
        }
        return 10000;
    }

    /// <summary>
    /// 属性字符串转换为字典结构 
    /// </summary>
    public void AttrDataToDic()
    {
        string attr;
        string[] attrGroup;
        string[] kvAttr;
        for (int i = 0; i < MonsterDataList.Count; i++)
        {
            attr = MonsterDataList[i].attr;
            attrGroup = attr.Split('|');
            for (int g = 0; g < attrGroup.Length; g++)
            {
                kvAttr = attrGroup[g].Split(';');
                if (MonsterDataList[i].attrs == null)
                {
                    MonsterDataList[i].attrs = new Dictionary<long, float>();
                }
                MonsterDataList[i].attrs.Add(long.Parse(kvAttr[0]), float.Parse(kvAttr[1]));
            }
        }
    }




    /// <summary>
    /// 属性字符串转换为字典结构 
    /// </summary>
    public void AttrDataToDic_Level()
    {
        string attr;
        string[] attrGroup;
        string[] kvAttr;
        for (int i = 0; i < monsterLevelCfgDatas.Count; i++)
        {
            attr = monsterLevelCfgDatas[i].attrgrowth;
            attrGroup = attr.Split('|');
            for (int g = 0; g < attrGroup.Length; g++)
            {
                kvAttr = attrGroup[g].Split(';');
                if (monsterLevelCfgDatas[i].attrs == null)
                {
                    monsterLevelCfgDatas[i].attrs = new Dictionary<long, float>();
                }
                monsterLevelCfgDatas[i].attrs.Add(long.Parse(kvAttr[0]), float.Parse(kvAttr[1]));
            }
        }
    }
}

