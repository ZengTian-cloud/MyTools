using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogBookManager : SingletonNotMono<LogBookManager>
{
   // 任务总表 k-taskid
    public Dictionary<int, TaskCfgData> dicTaskCfg = new Dictionary<int, TaskCfgData>();
    // 里程碑分表 k-taskid
    public Dictionary<int, LogBookCfgData> dicLogBookCfg = new Dictionary<int, LogBookCfgData>();
    // 日常、周常类别表 k-taskid
    public Dictionary<int, TaskFourCfgData> dicTaskFourCfg = new Dictionary<int, TaskFourCfgData>();
    // 里程碑类别表 k-taskid
    public Dictionary<int, TaskOneCfgData> dicTaskOneCfg = new Dictionary<int, TaskOneCfgData>();
    // 里程碑奖励单独表 k-stage
    public Dictionary<int, MilestoneRewardCfgData> dicMilestoneRewardCfgData = new Dictionary<int,MilestoneRewardCfgData>();
    // 日常分表 k-taskid
    public Dictionary<int, DailyTaskCfgData> dicDailyTaskCfgData = new Dictionary<int,DailyTaskCfgData>();
    // 周常分表 k-taskid
    public Dictionary<int, WeeklyTaskCfgData> dicWeeklyTaskCfgData = new Dictionary<int,WeeklyTaskCfgData>();
    // 日常活跃奖励 k-needactvalue
    public Dictionary<int, DailyValueCfgData> dicDailyValueCfgData = new();
    // 周常活跃奖励 k-needactvalue
    public Dictionary<int, WeeklyValueCfgData> dicWeeklyValueCfgData = new();
    /// <summary>
    /// 初始化配置信息
    /// </summary>
    public void InitCfg()
    {
        List<TaskCfgData> taskCfgDatas = GameCenter.mIns.m_CfgMgr.JsonToListClass<TaskCfgData>("t_task");
        for (int i = 0; i < taskCfgDatas.Count; i++)
        {
            if (dicTaskCfg.ContainsKey(taskCfgDatas[i].taskid))
            {
                Debug.LogError($"t_task表中存在相同的taskid，taskid:{taskCfgDatas[i].taskid}");
                return;
            }
            dicTaskCfg.Add(taskCfgDatas[i].taskid, taskCfgDatas[i]);
        }

        List<LogBookCfgData> logBookCfgDatas = GameCenter.mIns.m_CfgMgr.JsonToListClass<LogBookCfgData>("t_task_stage");
        for (int i = 0; i < logBookCfgDatas.Count; i++)
        {
            if (dicLogBookCfg.ContainsKey(logBookCfgDatas[i].taskid))
            {
                Debug.LogError($"t_task_stage表中存在相同的taskid，taskid:{logBookCfgDatas[i].taskid}");
                return;
            }
            dicLogBookCfg.Add(logBookCfgDatas[i].taskid, logBookCfgDatas[i]);
        }

        List<TaskFourCfgData> taskFourCfgData = GameCenter.mIns.m_CfgMgr.JsonToListClass<TaskFourCfgData>("t_task_4");
        for (int i = 0; i < taskFourCfgData.Count; i++)
            {
                if (dicTaskFourCfg.ContainsKey(taskFourCfgData[i].taskid))
                {
                    Debug.LogError($"t_task_4表中存在相同的taskid，taskid:{taskFourCfgData[i].taskid}");
                    return;
                }
                dicTaskFourCfg.Add(taskFourCfgData[i].taskid, taskFourCfgData[i]);
            }

        List<TaskOneCfgData> taskOneCfgData = GameCenter.mIns.m_CfgMgr.JsonToListClass<TaskOneCfgData>("t_task_1");
        for (int i = 0; i < taskOneCfgData.Count; i++)
            {
                if (dicTaskOneCfg.ContainsKey(taskOneCfgData[i].taskid))
                {
                    Debug.LogError($"t_task_1表中存在相同的taskid，taskid:{taskOneCfgData[i].taskid}");
                    return;
                }
                dicTaskOneCfg.Add(taskOneCfgData[i].taskid, taskOneCfgData[i]);
            }

        List<MilestoneRewardCfgData> milestoneRewardCfgData = GameCenter.mIns.m_CfgMgr.JsonToListClass<MilestoneRewardCfgData>("t_task_stagevalue");
        for (int i = 0; i < milestoneRewardCfgData.Count; i++)
            {
                if (dicMilestoneRewardCfgData.ContainsKey(milestoneRewardCfgData[i].stage))
                {
                    Debug.LogError($"t_task_stagevalue表中存在相同的stage，stage:{milestoneRewardCfgData[i].stage}");
                    return;
                }
                dicMilestoneRewardCfgData.Add(milestoneRewardCfgData[i].stage, milestoneRewardCfgData[i]);
            }

        List<DailyTaskCfgData> dailyTaskCfgData = GameCenter.mIns.m_CfgMgr.JsonToListClass<DailyTaskCfgData>("t_task_daily");
        for (int i = 0; i < dailyTaskCfgData.Count; i++)
            {
                if (dicDailyTaskCfgData.ContainsKey(dailyTaskCfgData[i].taskid))
                {
                    Debug.LogError($" t_task_daily 表中存在相同的 taskid， taskid :{dailyTaskCfgData[i].taskid}");
                    return;
                }
                dicDailyTaskCfgData.Add(dailyTaskCfgData[i].taskid, dailyTaskCfgData[i]);
            }

        List<WeeklyTaskCfgData> weeklyTaskCfgData = GameCenter.mIns.m_CfgMgr.JsonToListClass<WeeklyTaskCfgData>("t_task_week");
        for (int i = 0; i < weeklyTaskCfgData.Count; i++)
            {
                if (dicWeeklyTaskCfgData.ContainsKey(weeklyTaskCfgData[i].taskid))
                {
                    Debug.LogError($" t_task_week 表中存在相同的 taskid， taskid :{weeklyTaskCfgData[i].taskid}");
                    return;
                }
                dicWeeklyTaskCfgData.Add(weeklyTaskCfgData[i].taskid, weeklyTaskCfgData[i]);
            }

        List<DailyValueCfgData> dailyValueCfgData = GameCenter.mIns.m_CfgMgr.JsonToListClass<DailyValueCfgData>("t_task_dailyvalue");
        for (int i = 0; i < dailyValueCfgData.Count; i++)
            {
                if (dicDailyValueCfgData.ContainsKey(dailyValueCfgData[i].needactvalue))
                {
                    Debug.LogError($" t_task_week 表中存在相同的 taskid， taskid :{dailyValueCfgData[i].needactvalue}");
                    return;
                }
                dicDailyValueCfgData.Add(dailyValueCfgData[i].needactvalue, dailyValueCfgData[i]);
            }

        List<WeeklyValueCfgData> weeklyValueCfgData = GameCenter.mIns.m_CfgMgr.JsonToListClass<WeeklyValueCfgData>("t_task_weekvalue");
        for (int i = 0; i < weeklyValueCfgData.Count; i++)
            {
                if (dicWeeklyValueCfgData.ContainsKey(weeklyValueCfgData[i].needactvalue))
                {
                    Debug.LogError($" t_task_week 表中存在相同的 taskid， taskid :{weeklyValueCfgData[i].needactvalue}");
                    return;
                }
                dicWeeklyValueCfgData.Add(weeklyValueCfgData[i].needactvalue, weeklyValueCfgData[i]);
            }
        }

    public TaskCfgData GetTaskCfgByTaskid(int taskid)
    {
        if (dicTaskCfg.ContainsKey(taskid))
            return dicTaskCfg[taskid];
        Debug.LogError($"未在t_task表中找到id为{taskid}的配置，请检查！");
        return null;
    }
    public LogBookCfgData GetLogBookCfgByTaskid(int taskid)
    {
        if (dicLogBookCfg.ContainsKey(taskid))
            return dicLogBookCfg[taskid];
        Debug.LogError($"未在t_task_stage表中找到id为{taskid}的配置，请检查！");
        return null;
    }
    public TaskFourCfgData GetTaskFourCfgByTaskid(int taskid)
    {
        if (dicTaskFourCfg.ContainsKey(taskid))
            return dicTaskFourCfg[taskid];
        Debug.LogError($"未在t_task_4表中找到id为{taskid}的配置，请检查！");
        return null;
    }
    public TaskOneCfgData GetTaskOneCfgByTaskid(int taskid)
    {
        if (dicTaskOneCfg.ContainsKey(taskid))
            return dicTaskOneCfg[taskid];
        Debug.LogError($"未在t_task_1表中找到id为{taskid}的配置，请检查！");

        Debug.Log($"{dicTaskOneCfg[taskid]}");

        return null;
    }
    public MilestoneRewardCfgData GetMilestoneRewardCfgBystage(int stage)
    {
        if (dicMilestoneRewardCfgData.ContainsKey(stage))
            return dicMilestoneRewardCfgData[stage];
        Debug.LogError($"未在t_task_stagevalue表中找到id为{stage}的配置，请检查！");
        return null;
    }

    public DailyTaskCfgData GetDailyTaskCfgBytaskid(int taskid)
    {
        if (dicDailyTaskCfgData.ContainsKey(taskid))
            return dicDailyTaskCfgData[taskid];
        Debug.LogError($"未在 t_task_daily 表中找到 id 为 {taskid} 的配置，请检查！");
        return null;
    }

    public WeeklyTaskCfgData GetWeeklyTaskCfgBytaskid(int taskid)
    {
        if (dicWeeklyTaskCfgData.ContainsKey(taskid))
            return dicWeeklyTaskCfgData[taskid];
        Debug.LogError($"未在 t_task_week 表中找到 id 为 {taskid} 的配置，请检查！");
        return null;
    }
    public DailyValueCfgData GetDailyValueCfgByneedactvalue(int needactvalue)
    {
        if (dicDailyValueCfgData.ContainsKey(needactvalue))
            return dicDailyValueCfgData[needactvalue];
        Debug.LogError($"未在 t_task_dailyvalue 表中找到 id 为 {needactvalue} 的配置，请检查！");
        return null;
    }
    public WeeklyValueCfgData GetWeeklyValueCfgByneedactvalue(int needactvalue)
    {
        if (dicWeeklyValueCfgData.ContainsKey(needactvalue))
            return dicWeeklyValueCfgData[needactvalue];
        Debug.LogError($"未在 t_task_weekvalue 表中找到 id 为 {needactvalue} 的配置，请检查！");
        return null;
    }
}
