using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LitJson;
using TMPro;

public partial class task_wnd
{
    public override UILayerType uiLayerType =>UILayerType.Normal;
    public override string uiAtlasName => "taskwnd";

    //k-模块ID
    private Dictionary<int, leftGroupItem> dicLeftGroup;

    private leftGroupItem curLeftGroup;//当前选中组
    public taskItem curTaskItem;//任务条目
    private int curRightModel;//当前右侧数据所属模块

    //本地存储的当前追踪任务的key值
    private string curTargetTaskKey = $"{GameCenter.mIns.userInfo.Uid}_curTargetTask";
    private long curTargetTask;//当前追踪的任务-本地存储到playerprefab;


    private JsonData curRightJsonData;//当前右侧信息数据
    private List<GameObject> rewardItemList = new List<GameObject>();

    //缓存的条件列表
    private List<conditionItem> conditionItems = new List<conditionItem>();
    TopResourceBar topResBar;
    protected override void OnInit()
    {
        InitLeftGroup();
    }

    protected override void OnOpen()
    {

        topResBar = new TopResourceBar(this.uiRoot, this, () =>
        {
                this.Close();
                return true;
        }, GameCenter.mIns.m_LanMgr.GetLan("task_title"));
        RefreshAllTask();
    }

    private void RefreshAllTask()
    {
        //主线任务
        TaskMsgManager.Instance.SendGetTaskList((msg) =>
        {
            RefreshGroupTask(dicLeftGroup[1], msg);
            if (curTaskItem == null)
            {
                this.right.SetActive(false);
                this.nothing.SetActive(true);
            }
        }, 1);
        /*TaskMsgManager.Instance.SendGetTaskList((msg) =>
        {
            RefreshGroupTask(dicLeftGroup[2], msg);
        }, 2);*/
        //冒险任务
        /*
        TaskMsgManager.Instance.SendGetTaskList((msg) =>
        {
            RefreshGroupTask(dicLeftGroup[3], msg);
        }, 3);*/


        /*
        TakeMsgManager.Instance.SendGetTaskList((msg) =>
        {
        RefreshGroupTask(dicLeftGroup[2], msg);
        }, 4);*/

        
    }

    

    

    /// <summary>
    /// 初始化左侧页签
    /// </summary>
    private void InitLeftGroup()
    {
        this.dicLeftGroup = new Dictionary<int, leftGroupItem>();
        this.dicLeftGroup.Add(1, this.CreatGroupItem(1));//主线任务
        this.dicLeftGroup.Add(2, this.CreatGroupItem(2));//每日委托
        this.dicLeftGroup.Add(3, this.CreatGroupItem(3));//冒险任务
        this.dicLeftGroup.Add(4, this.CreatGroupItem(4));//同伴任务
    }

    private leftGroupItem CreatGroupItem(int model)
    {
        GameObject item = GameObject.Instantiate(this.groupItem);
        leftGroupItem groupItem = new leftGroupItem(item, this.scroll_group.content, model,this);
        return groupItem;
    }

    /// <summary>
    /// 刷新任务组节点下的任务
    /// </summary>
    private void RefreshGroupTask(leftGroupItem groupItem,JsonData msg)
    {
        groupItem.RefreshTask(msg);
        this.RefreshLayout();
    }

    public void RefreshLayout()
    {
        GameCenter.mIns.m_CoroutineMgr.DelayNextFrame(() => {
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.scroll_group.content.GetComponent<RectTransform>());
        });


        GameCenter.mIns.m_CoroutineMgr.DelayNextFrame(() => {
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.scroll_target.content.GetComponent<RectTransform>());
        });

    }

    /// <summary>
    /// 刷新任务右侧信息样式
    /// </summary>
    /// <param name="jsonData"></param>
    /// <param name="model">模块</param>
    public void RefreshRightInfo(JsonData jsonData, int model)
    {
        for (int i = 0; i < conditionItems.Count; i++)
        {
            conditionItems[i].curRoot.SetActive(false);
        }
        this.curRightModel = model;
        this.curRightJsonData = jsonData;

        if (jsonData.ContainsKey("subtask"))//是多条件组合任务
        {
            JsonData subTask = jsonData["subtask"];
            int index = 0;
            foreach (JsonData item in subTask)
            {
                if (index < conditionItems.Count)
                {
                    conditionItems[index].RefreshCondition(item);
                }
                else
                {
                    GameObject conditionItem = GameObject.Instantiate(this.contionItem);
                    conditionItem condition = new conditionItem(conditionItem, this.conditionList.transform, model);
                    condition.RefreshCondition(item);
                    conditionItems.Add(condition);
                }
                index++;
            }
        }
        else
        {
            if (conditionItems.Count > 0)
            {
                conditionItems[0].RefreshCondition(jsonData);
            }
            else
            {
                GameObject conditionItem = GameObject.Instantiate(this.contionItem);
                conditionItem condition = new conditionItem(conditionItem, this.conditionList.transform, model);
                condition.RefreshCondition(jsonData);
                conditionItems.Add(condition);
            }
        }
        if (jsonData.ContainsKey("note"))
        {
            this.desc.text = GameCenter.mIns.m_LanMgr.GetLan(jsonData["note"].ToString());
        }
        long taskid = jsonData["taskid"].ToInt64();
        int state = jsonData["state"].ToInt32();

        string name = jsonData["name"].ToString();
        if (state == 0)//未完成
        {
            //是否已有正在追踪的任务
            if (PlayerPrefs.HasKey(curTargetTaskKey))
            {
                curTargetTask = long.Parse(PlayerPrefs.GetString(curTargetTaskKey).Split(';')[0]);
                if (taskid == curTargetTask)//当前任务是追踪任务-按钮置灰
                {
                    this.btn_go.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("taskwnd_1");
                    this.btn_go.AddListenerBeforeClear(() =>
                    {
                        this.CancelTraceTask();
                    });
                }
                else
                {
                    this.btn_go.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("taskwnd_2");//追踪任务
                    this.btn_go.AddListenerBeforeClear(() =>
                    {
                        this.TraceTask(taskid, name, model);
                    });
                }
            }
            else
            {
                this.btn_go.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("taskwnd_2");//追踪任务
                this.btn_go.AddListenerBeforeClear(() =>
                {
                    this.TraceTask(taskid, name, model);
                });
            }
        }
        else if (state == 1)//已完成
        {
            this.btn_go.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("taskwnd_3");

            this.btn_go.AddListenerBeforeClear(() =>
            {
                this.ReceiveReward(taskid);
            });
        }
        else if (state == 2)//已领取
        {
            this.btn_go.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("taskwnd_4");
        }
        else if (state == -1)//可接取
        {
            this.btn_go.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("taskwnd_5");
            this.btn_go.AddListenerBeforeClear(() =>
            {

            });
        }

        this.btn_go.interactable = state != 2;

        long dropid = 0;
        if (model == 1)//主线任务另外取奖励配置
        {
            dropid = TaskCfgManager.Instance.GetTaskMianCfgByTaskID(taskid).rewardpreview;
        }
        else
        {
            if (jsonData.ContainsKey("dropid"))
            {
                dropid = jsonData["dropid"].ToInt64();
            }
        }
        //刷新奖励
        if (dropid > 0)
        {
            List<DropCfgData> dropList = DropManager.Instance.GetDropListByDropID(dropid);
            for (int i = 0; i < dropList.Count; i++)
            {
                GameObject node = null;
                if (i < rewardItemList.Count)
                {
                    node = rewardItemList[i];
                }
                else
                {
                    node = GameObject.Instantiate(this.commonitem, this.scroll_reward.content);
                    node.transform.localScale = Vector3.one;
                    node.SetActive(true);
                    rewardItemList.Add(node);
                }
                ItemData itemData = new ItemData((int)dropList[i].onlyid, dropList[i].pid, dropList[i].max);
                CommonItem item = new CommonItem(itemData, node, (item) =>
                {
                });
            }
        }
        this.RefreshLayout();
        this.right.SetActive(true);
    }

    /// <summary>
    /// 追踪任务
    /// </summary>
    /// <param name="taskid"></param>
    /// <param name="taskType">任务类型</param>
    public void TraceTask(long taskid,string name,int model)
    {
        PlayerPrefs.SetString(curTargetTaskKey, $"{taskid};{name};{this.curTaskItem.taskName};{model};0");
        this.RefreshRightInfo(this.curRightJsonData, this.curRightModel);
        this.RefreshTrace();

        this.Close();
    }

    /// <summary>
    /// 取消追踪
    /// </summary>
    public void CancelTraceTask()
    {
        if (PlayerPrefs.HasKey(curTargetTaskKey))
        {
            PlayerPrefs.DeleteKey(curTargetTaskKey);
        }
        this.RefreshRightInfo(this.curRightJsonData, this.curRightModel);

        this.RefreshTrace();
    }

    /// <summary>
    /// 刷新追踪任务
    /// </summary>
    public void RefreshTrace()
    {
        foreach (var item in dicLeftGroup)
        {
            item.Value.RefreshTrace();
        }
    }

    /// <summary>
    /// 领取奖励
    /// </summary>
    /// <param name="taskid"></param>
    public void ReceiveReward(long taskid)
    {
        TaskMsgManager.Instance.SendReceiveReward((msg) => {
            curTaskItem = null;
            RefreshAllTask();
            this.CancelTraceTask();
        }, taskid);
    }



    /// <summary>
    ///  选中左侧任务组
    /// </summary>
    /// <param name="left"></param>
    public void SelectLeftGroup(leftGroupItem left)
    {
        if (this.curLeftGroup != null)
        {
            this.curLeftGroup.SetSlectState(false);
        }
        this.curLeftGroup = left;
        this.curLeftGroup.SetSlectState(true);
    }

    /// <summary>
    /// 选中左侧任务条目
    /// </summary>
    /// <param name="item"></param>
    public void SlectLeftTaskItem(taskItem item, leftGroupItem left)
    {
        if (this.curTaskItem != null)
        {
            this.curTaskItem.SetSelectState(false);
        }
        this.curTaskItem = item;
        this.curTaskItem.SetSelectState(true);
        this.SelectLeftGroup(left);
    }


    protected override void OnClose()
    {
        if (this.curTaskItem != null)
        {
            this.curTaskItem.SetSelectState(false);
            this.curTaskItem = null;
        }

    }
}

