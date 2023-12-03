using System;
using LitJson;
using System.Collections.Generic;
/// <summary>
/// 任务通信
/// </summary>
public class TaskMsgManager:SingletonNotMono<TaskMsgManager>
{
	/// <summary>
	/// 后端下发任务完成通知监听
	/// </summary>
	public void PushHandleTask(IGameMessage message)
	{
        JsonData data = jsontool.newwithstring(message.getContent());
		if (data.ContainsKey("success"))
		{
			JsonData success = data["success"];
			//获得各个模块完成的任务 ===> key-模块id 派发时间 各个模块根据需求自己接收 
			for (int i = 1; i <= 9; i++)
			{
				int model = i;
				string key = model.ToString();
				if (success.ContainsKey(key))
				{
                    List<long> taskList = JsonMapper.ToObject<List<long>>(JsonMapper.ToJson(success[key]));
                    if (model == 1)//主线任务需要判断是否自动领取任务-特殊处理
                    {
                        for (int t = 0; t < taskList.Count; t++)
                        {
                            TaskMainCfg mainCfg = TaskCfgManager.Instance.GetTaskMianCfgByTaskID(taskList[t]);
                            if (mainCfg.autoreward == 1)
                            {
                                this.SendReceiveReward(null, taskList[t]);
                                GameEventMgr.Distribute(GEKey.Task_Main_AutoReward, mainCfg.taskid);
                            }
                        }
                    }


					//派发任务结束
                    GameEventMgr.Distribute(NetCfg.PUSH_TASK_END_NOTIF.ToString(), model, taskList);
                }
			}
        }
    }


	/// <summary>
	/// 请求任务列表
	/// </summary>
	/// <param name="cb">回调</param>
	/// <param name="model"></param>
	/// <param name="groupid"></param>
	public void SendGetTaskList(Action<JsonData> cb, long model, int groupid = -1)
	{
		JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
		jsonData["model"] = model;
		if (groupid > 0)
		{
			jsonData["groupid"] = groupid;
		}
		GameCenter.mIns.m_NetMgr.SendData(NetCfg.TASK_GET_TASKLIST, jsonData, (eqid, code, content) =>
		{
			if (code == 0)
			{
				GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() => {
                    JsonData msg = jsontool.newwithstring(content);
					cb?.Invoke(msg);
                });
            }
		});
	}

	/// <summary>
	/// 领取任务奖励
	/// </summary>
	/// <param name="cb"></param>
	/// <param name="taskid"></param>
	/// <param name="floor"></param>
	public void SendReceiveReward(Action<JsonData> cb,long taskid,int floor = -1)
	{
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
		jsonData["taskid"] = taskid;
		if (floor > 0)
		{
            jsonData["floor"] = floor;
        }
		GameCenter.mIns.m_NetMgr.SendData(NetCfg.TASK_GET_TASKREWARD, jsonData,(eqid, code, content) => {
            if (code == 0)
            {
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() => {
                    JsonData msg = jsontool.newwithstring(content);
                    JsonData chagedata = msg["change"]?["changed"];
                    if (chagedata != null)
                    {
                        GameCenter.mIns.userInfo.onChange(chagedata);
                    }

                    JsonData changing = msg["change"]?["changing"];
					if (changing!= null)
					{
						WarehouseManager.Instance.ShowChangingTip(changing);
					}
                    cb?.Invoke(msg);
                });
            }
        });
    }

	/// <summary>
	/// 接取任务
	/// </summary>
	/// <param name="cb"></param>
	/// <param name="taskid"></param>
	public void SendAccessTask(Action<JsonData> cb,long taskid)
	{
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["taskid"] = taskid;
		GameCenter.mIns.m_NetMgr.SendData(NetCfg.TASK_ACCESS_TASK, jsonData, (eqid, code, content) =>
		{
			if (code == 0)
			{
				GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() => {
                    JsonData msg = jsontool.newwithstring(content);
                    cb?.Invoke(msg);
                });
            }
		});
    }
}


public class ModelsMsg
{
	//分组ID
	public int groupid;
	//分组奖励状态 0-未完成 1-已完成 2-已领取
	public int state;
}
