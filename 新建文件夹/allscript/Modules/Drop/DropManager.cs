using System.Collections.Generic;
using LitJson;
using UnityEngine;
/// <summary>
/// 掉落组管理
/// </summary>
public class DropManager:SingletonNotMono<DropManager>
{
	private Dictionary<long, List<DropCfgData>> dicAllDroip;

	private string basicPath = "t_drop_basic";

	private string playPath = "t_drop_play";

	private string activityPath = "t_drop_activity";
	
	public void InitAllDropCfg()
	{
		List<DropCfgData> _basic = GameCenter.mIns.m_CfgMgr.JsonToListClass<DropCfgData>(basicPath);

        List<DropCfgData> _play = GameCenter.mIns.m_CfgMgr.JsonToListClass<DropCfgData>(playPath);

        List<DropCfgData> _activity = GameCenter.mIns.m_CfgMgr.JsonToListClass<DropCfgData>(activityPath);

		dicAllDroip = new Dictionary<long, List<DropCfgData>>();


        for (int i = 0; i < _basic.Count; i++)
		{
			AddData(_basic[i].dropid, _basic[i]);

        }

        for (int i = 0; i < _play.Count; i++)
        {
            AddData(_play[i].dropid, _play[i]);

        }

        for (int i = 0; i < _activity.Count; i++)
        {
            AddData(_activity[i].dropid, _activity[i]);

        }
    }

	private void AddData(long dropID, DropCfgData cfg)
	{
		if (!dicAllDroip.ContainsKey(dropID))
		{
			dicAllDroip.Add(dropID, new List<DropCfgData>());
        }
        dicAllDroip[dropID].Add(cfg);
    }


    public List<DropCfgData> GetDropListByDropID(long dropid)
    {
        if (dicAllDroip.ContainsKey(dropid))
        {
            return dicAllDroip[dropid];
        }
        Debug.LogError($"未在掉落表中找到dropid为{dropid}的配置，请检查！");
        return null;
    }

}

