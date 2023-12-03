using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using LitJson;

public class MissionHelper : SingletonNotMono<MissionHelper>
{
    //关卡节点 k-模块 v-关卡列表
    public Dictionary<int, List<missionNodeData>> dicMissionNodes = new Dictionary<int, List<missionNodeData>>();

    /// <summary>
    /// 刷新关卡节点数据
    /// </summary>
    /// <param name="chapter">章节ID</param>
    /// <param name="missionMsgs">关卡通行数据</param>
    public void RefreshMissionNodesData(int chapter, List<MissionMsg> missionMsgs)
	{
        if (dicMissionNodes != null || dicMissionNodes.Count > 0)
        {
            dicMissionNodes.Clear();
        }
        //当前章节下所有关卡
        List<MissionCfgData> missionCfgDatas = MissionCfgManager.Instance.GetAllMissionByChapterID(chapter);
        for (int i = 0; i < missionCfgDatas.Count; i++)
        {
            //当前关卡通信数据 可能为null
            MissionMsg msg = missionMsgs.Find(m => m.id == missionCfgDatas[i].mission);
            missionNodeData node = new missionNodeData(missionCfgDatas[i].mission, missionCfgDatas[i], msg);
            if (!dicMissionNodes.ContainsKey(missionCfgDatas[i].areaid))
            {
                dicMissionNodes.Add(missionCfgDatas[i].areaid, new List<missionNodeData>());
            }
            dicMissionNodes[missionCfgDatas[i].areaid].Add(node);
        }
    }

    /// <summary>
    /// 请求关卡信息
    /// </summary>
    public void SendMissionMsg(int chapter,Action<List<MissionMsg>, List<AreaMsg>> cb)
    {
        //获取关卡信息-需要拿到当前处于的模块才能开始加载
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["chapter"] = chapter;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.CHAPTER_GET_MISSIONS, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {
                    //章节数据
                    JsonData msg = jsontool.newwithstring(content);
                    List<MissionMsg> missionMsgs = null;
                    if (msg.ContainsKey("missions"))
                    {
                        missionMsgs = JsonMapper.ToObject<List<MissionMsg>>(JsonMapper.ToJson(msg["missions"]));
                        RefreshMissionNodesData(chapter, missionMsgs);
            
                    }
                    List<AreaMsg> areaMsgs = null;
                    if (msg.ContainsKey("areas"))
                    {
                        areaMsgs = JsonMapper.ToObject<List<AreaMsg>>(JsonMapper.ToJson(msg["areas"]));
                    }
                    cb?.Invoke(missionMsgs, areaMsgs);
                });
            }
        });
    }

    /// <summary>
    /// 获得区块下所有关卡节点的数据
    /// </summary>
    public List<missionNodeData> GetMissionNodeDatasByAreaID(int chapter)
    {
        if (dicMissionNodes.ContainsKey(chapter))
        {
            return dicMissionNodes[chapter];
        }
        return null;
    }
}

