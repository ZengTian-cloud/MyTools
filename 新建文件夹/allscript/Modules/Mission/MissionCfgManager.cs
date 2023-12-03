using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡信息配置表
/// </summary>
public class MissionCfgManager : SingletonNotMono<MissionCfgManager>
{
    //关卡总表 k-关卡id
    public Dictionary<long, MissionCfgData> dicMissionCfgs = new Dictionary<long, MissionCfgData>();
    //关卡区块表 k-区块id
    public Dictionary<int, MissionAreaCfgData> dicMissionAreaCfgs = new Dictionary<int, MissionAreaCfgData>();

    /// <summary>
    /// 初始化关卡配置信息
    /// </summary>
    public void InitMissionCfg()
    {
        //关卡总表
        List<MissionCfgData> missionCfgDatas = GameCenter.mIns.m_CfgMgr.JsonToListClass<MissionCfgData>("t_mission_main");
        for (int i = 0; i < missionCfgDatas.Count; i++)
        {
            if (dicMissionCfgs.ContainsKey(missionCfgDatas[i].mission))
            {
                Debug.LogError($"t_mission_main表中存在相同的关卡id，mission:{missionCfgDatas[i].mission}");
                return;
            }
            dicMissionCfgs.Add(missionCfgDatas[i].mission, missionCfgDatas[i]);
        }

        //关卡区块表
        List<MissionAreaCfgData> missionAreaCfgDatas = GameCenter.mIns.m_CfgMgr.JsonToListClass<MissionAreaCfgData>("t_mission_area");
        for (int i = 0; i < missionAreaCfgDatas.Count; i++)
        {
            if (dicMissionAreaCfgs.ContainsKey(missionAreaCfgDatas[i].areaid))
            {
                Debug.LogError($"t_mission_area表中存在相同的区块id，mission:{missionAreaCfgDatas[i].areaid}");
                return;
            }
            dicMissionAreaCfgs.Add(missionAreaCfgDatas[i].areaid, missionAreaCfgDatas[i]);
        }


    }

    /// <summary>
    /// 获得关卡区块信息
    /// </summary>
    /// <returns></returns>
    public MissionAreaCfgData GetMissionAreaCfgDataByAreaID(int areaID)
    {
        if (dicMissionAreaCfgs.ContainsKey(areaID))
        {
            return dicMissionAreaCfgs[areaID];
        }
        Debug.LogError($"未在关卡区块表中找到区块id为{areaID}的配置，请检查！");
        return null;
    }

    /// <summary>
    /// 获得关卡区块信息byIndex
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public MissionAreaCfgData GetMissionAreaCfgDataByIndex(int index)
    {
        foreach (var item in dicMissionAreaCfgs)
        {
            if (item.Value.list == index)
            {
                return item.Value;
            }
        }
        return null;
    }


    /// <summary>
    /// 获得关卡信息
    /// </summary>
    public MissionCfgData GetMissionCfgByMissionID(long missionID)
    {
        if (dicMissionCfgs.ContainsKey(missionID))
        {
            return dicMissionCfgs[missionID];
        }
        Debug.LogError($"未在关卡表中找到关卡id为{missionID}的配置，请检查！");
        return null;
    }

    /// <summary>
    /// 获得某个区块下所有关卡
    /// </summary>
    public List<MissionCfgData> GetAllMissionByAreaID(int chapterID, int areaID)
    {
        List<MissionCfgData> curchapterList = GetAllMissionByChapterID(chapterID);
        List<MissionCfgData> missionsList = new List<MissionCfgData>();
        foreach (var item in curchapterList)
        {
            if (item.areaid == areaID)
            {
                missionsList.Add(item);
            }
        }
        return missionsList;
    }

    /// <summary>
    /// 获得某个章节下所有关卡
    /// </summary>
    /// <param name="chapterID"></param>
    /// <returns></returns>
    public List<MissionCfgData> GetAllMissionByChapterID(int chapterID)
    {
        List<MissionCfgData> missionsList = new List<MissionCfgData>();
        foreach (var item in dicMissionCfgs)
        {
            if (item.Value.chapter == chapterID)
            {
                missionsList.Add(item.Value);
            }
        }
        return missionsList;
    }
}

