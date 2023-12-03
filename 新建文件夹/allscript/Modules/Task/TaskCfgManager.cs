using System;
using UnityEngine;
using System.Collections.Generic;

public class TaskCfgManager:SingletonNotMono<TaskCfgManager>
{
	//任务总表
	private string taskBaseCfgPath = "t_task";
	//分表-主线任务
	private string taskMainCfgPath = "t_task_main";
	//分表-冒险任务
	private string taskAdventurePath = "t_task_adventure";
	//分表-成就任务
	private string taskAchievementGrouppath = "t_task_achievementgroup";
	private string taskAchievementpath = "t_task_achievement";

	//任务总表
	private List<TaskBaseCfg> taskBaseCfgList = new List<TaskBaseCfg>();
	private Dictionary<long, TaskBaseCfg> dicTaskBaseCfg = new Dictionary<long, TaskBaseCfg>();

	//分表-主线任务
	private List<TaskMainCfg> taskMainCfgList = new List<TaskMainCfg>();
	private Dictionary<long, TaskMainCfg> dicTaskMainCfg = new Dictionary<long, TaskMainCfg>();

	//分表-冒险任务
	private List<TaskAdventureCfg> taskAdventureCfgList = new List<TaskAdventureCfg>();
	private Dictionary<long, TaskAdventureCfg> dicTaskAdventureCfg = new Dictionary<long, TaskAdventureCfg>();
	//分表-成就
	private List<TaskAchievementCfg> taskAchievemrntCfgList = new List<TaskAchievementCfg>();//成就group表
	private Dictionary<long, TaskAchievementCfg> dicTaskAchievementCfg = new Dictionary<long, TaskAchievementCfg>();
	private List<TaskAchievemrntGroupCfg> taskAchievemrntGroupCfgList = new List<TaskAchievemrntGroupCfg>();//成就group表
	public List<TaskAchievemrntGroupCfg> TaskAchievemrntGroupCfgList//成就group表访问器
	{ get { return taskAchievemrntGroupCfgList; } }
	private Dictionary<long, TaskAchievemrntGroupCfg> dicTaskAchievementGroupCfg = new Dictionary<long, TaskAchievemrntGroupCfg>();//成就group表字典
	/// <summary>
	/// 初始化所有任务配置表
	/// </summary>
	public void InitAllCfg()
	{
		//任务总表
        taskBaseCfgList = GameCenter.mIns.m_CfgMgr.JsonToListClass<TaskBaseCfg>(taskBaseCfgPath);
		for (int i = 0; i < taskBaseCfgList.Count; i++)
		{
			dicTaskBaseCfg.Add(taskBaseCfgList[i].taskid, taskBaseCfgList[i]);
        }

		//分表-主线任务
		taskMainCfgList = GameCenter.mIns.m_CfgMgr.JsonToListClass<TaskMainCfg>(taskMainCfgPath);
		for (int i = 0; i < taskMainCfgList.Count; i++)
		{
			dicTaskMainCfg.Add(taskMainCfgList[i].taskid, taskMainCfgList[i]);
        }

        //分表-冒险任务
        taskAdventureCfgList = GameCenter.mIns.m_CfgMgr.JsonToListClass<TaskAdventureCfg>(taskAdventurePath);
		for (int i = 0; i < taskAdventureCfgList.Count; i++)
		{
			dicTaskAdventureCfg.Add(taskAdventureCfgList[i].taskid, taskAdventureCfgList[i]);

        }

		//分表-成就任务		
		//成就表
		taskAchievemrntCfgList = GameCenter.mIns.m_CfgMgr.JsonToListClass<TaskAchievementCfg>(taskAchievementpath);
		for (int i = 0; i < taskAchievemrntCfgList.Count; i++)
		{
			dicTaskAchievementCfg.Add(taskAchievemrntCfgList[i].taskid, taskAchievemrntCfgList[i]);
		}
		//成就group表
		taskAchievemrntGroupCfgList = GameCenter.mIns.m_CfgMgr.JsonToListClass<TaskAchievemrntGroupCfg>(taskAchievementGrouppath);
		for (int i = 0; i < taskAchievemrntGroupCfgList.Count; i++)
		{
			dicTaskAchievementGroupCfg.Add(taskAchievemrntGroupCfgList[i].group, taskAchievemrntGroupCfgList[i]);
		}
	}

	/// <summary>
	/// 获得任务在任务总表内配置
	/// </summary>
	/// <param name="taskid"></param>
	public TaskBaseCfg GetTaskBaseCfgByTaskID(long taskid)
	{
		if (dicTaskBaseCfg.ContainsKey(taskid))
		{
			return dicTaskBaseCfg[taskid];

        }
		Debug.LogError($"未在任务总表【t_task】内找到任务id为：{taskid}的配置！");
		return null;
	}

	/// <summary>
	/// 获得主线任务表配置
	/// </summary>
	/// <param name="taskid"></param>
	/// <returns></returns>
	public TaskMainCfg GetTaskMianCfgByTaskID(long taskid)
	{
		if (dicTaskMainCfg.ContainsKey(taskid))
		{
			return dicTaskMainCfg[taskid];
        }
        Debug.LogError($"未在主线任务表【t_task_main】内找到任务id为：{taskid}的配置！");
		return null;
    }

	/// <summary>
	/// 获得冒险任务表配置
	/// </summary>
	/// <param name="taskid"></param>
	/// <returns></returns>
	public TaskAdventureCfg GetTaskAdventureCfgByTaskID(long taskid)
	{
		if (dicTaskAdventureCfg.ContainsKey(taskid))
		{
			return dicTaskAdventureCfg[taskid];
        }
        Debug.LogError($"未在冒险任务表【t_task_adventure】内找到任务id为：{taskid}的配置！");
		return null;
    }

	/// <summary>
	/// 获得成就任务表配置
	/// </summary>
	/// <param name="taskid"></param>
	/// <returns></returns>
	public TaskAchievementCfg GetTaskAchievementCfgByTaskID(long taskid)
	{
		if (dicTaskAchievementCfg.ContainsKey(taskid))
		{
			return dicTaskAchievementCfg[taskid];
		}
		Debug.Log($"未在成就任务表表【t_task_achievement】内找到任务id为：{taskid}的配置！");
		return null;
	}
	/// <summary>
	/// 通过成就的taskid查询所属的group
	/// </summary>
	/// <param name="taskid"></param>
	public int GetGroupByAchievementID(long taskid)
	{
		if (dicTaskAchievementCfg.ContainsKey(taskid))
			return dicTaskAchievementCfg[taskid].group;
		else
		{
			Debug.Log("没有找到对应的group");
			return -1;
		}
	}

}



