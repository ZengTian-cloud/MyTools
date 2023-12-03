using System;
public class missionNodeData
{
    public long missionID;
    public MissionCfgData cfgData;
    public MissionMsg msg;

    public missionNodeData(long missionID, MissionCfgData cfgData, MissionMsg msg)
	{
        this.missionID = missionID;
        this.cfgData = cfgData;
        this.msg = msg;
	}

    /// <summary>
    /// 刷新通信数据
    /// </summary>
    /// <param name="msg"></param>
    public void RefreshMsg(MissionMsg msg)
    {
        if (msg != null)
        {
            this.msg = msg;
        }
    }
}

/// <summary>
/// 关卡通信的数据结构
/// </summary>
public class MissionMsg
{
    public long id;//关卡id
    public int star;//通关星级
    public int lockstate;//解锁状态 0-未解锁 1-已解锁
    public long time;//最后通关事件
    public int unlock;//解锁条件 (已通关 没有该参数)
    public string unlockparam;//解锁条件参数 (已通关 没有该参数)
}

