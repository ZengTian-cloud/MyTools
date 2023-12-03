using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using TMPro;
using DG.Tweening;
using Random = System.Random;
using Cysharp.Threading.Tasks;

public partial class logbookwnd
{
    public override UILayerType uiLayerType => UILayerType.Normal;
    public override string uiAtlasName => "logbookwnd";
    public Random random;
    protected override void OnInit()
    {
        m_notselectobg.SetActive(false);
        m_selectobg.SetActive(true);
        d_notselectobg.SetActive(true);
        d_selectobg.SetActive(false);
        w_notselectobg.SetActive(true);
        w_selectobg.SetActive(false);

        listmilestone.SetActive(true);
        listdailyactivity.SetActive(false);
        listweeklyactivity.SetActive(false);
    }
    protected override void OnOpen()
    {
        base.OnOpen();

        GameCenter.mIns.m_UIMgr.AddUpdateWin(this);
        TopResourceBar topResBar = new TopResourceBar(_Root, this, () =>
        {
            this.Close();
            topResBar = null;
            return true;
        }, GameCenter.mIns.m_LanMgr.GetLan("logbook_title"));
        InitMileStone();
        InitDailyActivity();
        InitWeeklyActivity();

        RefreshDayTime();
        RefreshWeekTime();
    }

    public void InitMileStone()
    {
        int model = 7;
        SendCurrentMilestoneMsg(model);
    }
    public void InitDailyActivity()
    {
        int model = 5;
        SendDailyTaskMsg(model);
    }
    public void InitWeeklyActivity()
    {
        int model = 9;
        SendWeeklyTaskMsg(model);
    }


    private async void RefreshDayTime()
    {
        while(true)
        {
            DateTime dt1 = DateTime.Parse(DateTime.Now.ToShortDateString() + " 23:59:59");
            DateTime dt2 = DateTime.Now;
            _ = new TimeSpan();
            TimeSpan ts = dt1 - dt2;
            d_refreshtime.SetText($"{GameCenter.mIns.m_LanMgr.GetLan("logbook_time")}  {ts.Hours}{GameCenter.mIns.m_LanMgr.GetLan("hour")}{ts.Minutes}{GameCenter.mIns.m_LanMgr.GetLan("minute")}");
            await UniTask.Delay(1000, true);
        }
    }
    private async void RefreshWeekTime()
    {
        while(true)
        {
            DateTime dt1 = DateTime.Parse(DateTime.Now.AddDays(7 - Convert.ToInt16(DateTime.Now.DayOfWeek)).ToShortDateString() + " 23:59:59");
            DateTime dt2 = DateTime.Now;
            _ = new TimeSpan();
            TimeSpan ts = dt1 - dt2;
            w_refreshtime.SetText($"{GameCenter.mIns.m_LanMgr.GetLan("logbook_time")}  {ts.Days}{GameCenter.mIns.m_LanMgr.GetLan("day")}{ts.Hours}{GameCenter.mIns.m_LanMgr.GetLan("hour")}{ts.Minutes}{GameCenter.mIns.m_LanMgr.GetLan("minute")}");
            await UniTask.Delay(1000, true);
        }

    }

    public void RefreshMilestoneSummary()
    {
    }
    public void RenderMilestoneTableview(List<TaskInfo> data)
    {
        int completenum = 0;
        TableView tableview = rightcontent.GetComponent<TableView>();
        tableview.onItemRender =  (GameObject item, int index) => {
            TaskInfo taskinfo = data[index - 1];
            int model = 7;
            int taskid = taskinfo.taskid;
            long count = taskinfo.count;
            long total = taskinfo.total;
            int state = taskinfo.state;
            float degress = (float)(count * 1.0 / total);
            item.transform.name = taskid.ToString();
            TaskOneCfgData taskOneCfgData = LogBookManager.Instance.GetTaskOneCfgByTaskid(taskid);
            TMP_Text taskDes = item.transform.Find("taskdes").GetComponent<TMP_Text>();
            taskDes.SetText(GameCenter.mIns.m_LanMgr.GetLan(taskOneCfgData.note));

            GameObject m_rightbtns = item.transform.Find("rightbtns").gameObject;
            GameObject m_getrewardbtn = m_rightbtns.transform.Find("getrewardbtn").gameObject;
            GameObject m_jumpbtn = m_rightbtns.transform.Find("jumpbtn").gameObject;
            GameObject m_intask = m_rightbtns.transform.Find("intask").gameObject;
            GameObject m_isreceived = m_rightbtns.transform.Find("isreceived").gameObject;
            Image m_filledimg = item.transform.Find("progress/filledimg").gameObject.GetComponent<Image>();
            TMP_Text currentnum = item.transform.Find("progress/currentnum").gameObject.GetComponent<TMP_Text>();
            TMP_Text totalnum = item.transform.Find("progress/totlenum").gameObject.GetComponent<TMP_Text>();
            GameObject content = item.transform.Find("rewardtable/view/content").gameObject;

            Button btn = m_getrewardbtn.transform.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(delegate {SendTaskRewardMsg(taskid, model);});

            List<DropCfgData> dropCfgData = DropManager.Instance.GetDropListByDropID(taskOneCfgData.dropid);
            foreach (DropCfgData onedata in dropCfgData)
            {
                if(content.transform.Find($"{onedata.pid}"))
                    return;
                random = new();
                int num = onedata.min == onedata.max ? onedata.min : random.Next(onedata.min, onedata.max);
                ItemData itemData = new((int)onedata.onlyid, onedata.pid, num);
                CommonItem commonItem = LoadCommonitemPrefab(content.transform, itemData);
                commonItem.SetSize(Vector3.one * 0.65f);
            }

            if (count == total && state == 1)
            {
                m_rightbtns.SetActive(true);
                m_getrewardbtn.SetActive(true);
                m_jumpbtn.SetActive(false);
                m_intask.SetActive(false);
                m_isreceived.SetActive(false);
            }
            else if (count == total && state == 2)
            {
                completenum ++;
                m_rightbtns.SetActive(true);
                m_getrewardbtn.SetActive(false);
                m_jumpbtn.SetActive(false);
                m_intask.SetActive(false);
                m_isreceived.SetActive(true);
            }
            else if (count < total && state == 0)
            {
                m_rightbtns.SetActive(true);
                m_getrewardbtn.SetActive(false);
                m_jumpbtn.SetActive(true);
                m_intask.SetActive(false);
                m_isreceived.SetActive(false);
            }
            m_filledimg.DOFillAmount(degress, 0.3f);
            currentnum.SetText(count.ToString());
            totalnum.SetText(total.ToString());

            if(index == data.Count)
            {
                SetSummaryProgress(completenum, data.Count);
            }
        };
        tableview.SetDatas(data.Count, false);
    }

    private void SetSummaryProgress(int count, int total)
    {
        float progress = (float)(count * 1.0 / total);
        summarycurrentnum.SetText(count.ToString());
        summarytotlenum.SetText(total.ToString());
        summaryfilledimg.DOFillAmount(progress, 0.3f);
        if (progress == 1.0f)
        {
            getrewardobj.SetActive(true);
            summarytip.SetActive(false);
        }
        else
        {
            getrewardobj.SetActive(false);
            summarytip.SetActive(true);
        }
    }

    public void RenderRewardTableview(List<ModelsInfo> data)
    {
        random = new();
        MilestoneRewardCfgData milestoneRewardCfgData = GetRewardData(data[0].groupid);
        long dropId = milestoneRewardCfgData.dropid;
        summarytitle.SetText(GameCenter.mIns.m_LanMgr.GetLan(milestoneRewardCfgData.name));
        getrewardbtn.onClick.RemoveAllListeners();
        getrewardbtn.onClick.AddListener(delegate{ Getrewardbtn_Click(data[0].groupid); });
        List<DropCfgData> dropCfgData = DropManager.Instance.GetDropListByDropID(dropId);

        TableView tableview = rewardcontent.GetComponent<TableView>();
        tableview.onItemRender =  (GameObject item, int index) => {
            DropCfgData onedata = dropCfgData[index - 1];
            int num = onedata.min == onedata.max ? onedata.min : random.Next(onedata.min, onedata.max);
            ItemData itemData = new((int)onedata.onlyid, onedata.pid, num);
            CommonItem commonItem = LoadCommonitemPrefab(item.transform, itemData);
        };
        tableview.SetDatas(dropCfgData.Count, false);
    }

    private CommonItem LoadCommonitemPrefab(Transform parent, ItemData itemData)
    {
        //GameObject itemobj = await ResourcesManager.Instance.LoadUIPrefabSync("conmonitem", parent, true);
        GameObject itemobj = GameObject.Instantiate(commonitem);
        List<CommonItem> rewards = new();
        itemobj.transform.SetParent(parent);
        itemobj.transform.SetLocalPositionAndRotation(new Vector2(-84, 84), parent.rotation);
        itemobj.transform.name = itemData.Pid.ToString();
        //itemobj.transform.localScale = Vector3.one;

        CommonItem commonItem = new(itemData, itemobj, (_item) =>
        {
            Debug.Log("???????????? callback");
        });
        //commonItem.SetSize(Vector3.one * 0.65f);
        commonItem.SetMaskNumber(itemData.Number);
        itemobj.SetActive(true);
        rewards.Add(commonItem);
        commonItem.ChangeType(true);
        return commonItem;
    }

    public MilestoneRewardCfgData GetRewardData(int satge)
    {
        MilestoneRewardCfgData milestoneRewardCfgData = LogBookManager.Instance.GetMilestoneRewardCfgBystage(satge);
        return milestoneRewardCfgData;
    }
    partial void m_select_Click()
    {

    }
	partial void m_notselect_Click()
    {
        if(m_notselectobg.activeSelf)
        {
            m_notselectobg.SetActive(false);
            m_selectobg.SetActive(true);
            d_notselectobg.SetActive(true);
            d_selectobg.SetActive(false);
            w_notselectobg.SetActive(true);
            w_selectobg.SetActive(false);

            if(!listmilestone.activeSelf)
            {
                listmilestone.SetActive(true);
                listdailyactivity.SetActive(false);
                listweeklyactivity.SetActive(false);
            }
        }
    }
    partial void d_select_Click(){}
	partial void d_notselect_Click()
    {
        if(d_notselectobg.activeSelf)
        {
            d_notselectobg.SetActive(false);
            d_selectobg.SetActive(true);
            m_notselectobg.SetActive(true);
            m_selectobg.SetActive(false);
            w_notselectobg.SetActive(true);
            w_selectobg.SetActive(false);

            if(!listdailyactivity.activeSelf)
            {
                listmilestone.SetActive(false);
                listdailyactivity.SetActive(true);
                listweeklyactivity.SetActive(false);
            }
        }
    }
	partial void w_select_Click(){}
	partial void w_notselect_Click()
    {
        if(w_notselectobg.activeSelf)
        {
            w_notselectobg.SetActive(false);
            w_selectobg.SetActive(true);
            d_notselectobg.SetActive(true);
            d_selectobg.SetActive(false);
            m_notselectobg.SetActive(true);
            m_selectobg.SetActive(false);

            if(!listweeklyactivity.activeSelf)
            {
                listmilestone.SetActive(false);
                listdailyactivity.SetActive(false);
                listweeklyactivity.SetActive(true);
            }
        }
    }
    // 行动摘要领取事件
    public void Getrewardbtn_Click(int stage)
    {
        int model = 7;
        SendFuncRewardMsg(model, stage);
    }
    //服务器请求拉取里程碑任务数据
     private void SendMilestoneDataMsg(int model, int groupid)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["model"] = model;
        jsonData["groupid"] = groupid;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.TASK_GET_TASKLIST, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {
                    JsonData msg = jsontool.newwithstring(content);
                    if (msg.ContainsKey("tasks"))
                    {
                        List<TaskInfo> taskInfo = JsonMapper.ToObject<List<TaskInfo>>(JsonMapper.ToJson(msg["tasks"]));
                        if (taskInfo != null)
                        {
                            RenderMilestoneTableview(taskInfo);
                        }
                    }
                    if(msg.ContainsKey("models"))
                    {
                        List<ModelsInfo> modelsInfo = JsonMapper.ToObject<List<ModelsInfo>>(JsonMapper.ToJson(msg["models"]));
                        if (modelsInfo != null)
                        {
                            RenderRewardTableview(modelsInfo);
                        }
                    }
                    else
                    {
                        List<ModelsInfo> modelsInfo = new();
                        modelsInfo[0].groupid = groupid;
                        modelsInfo[0].state = 0;
                        RenderRewardTableview(modelsInfo);
                    }
                });
            }
        });
    }
    //服务器请求获取当前里程碑进度
     private void SendCurrentMilestoneMsg(int model)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["model"] = model;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.LOGBOOK_GET_CURRENT, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {
                    JsonData msg = jsontool.newwithstring(content);
                    if (msg.ContainsKey("groupid"))
                    {
                        int groupid = (int)msg["groupid"];
                        SendMilestoneDataMsg(model, groupid);
                    }
                });
            }
        });
    }

    //服务器请求获取每日任务数据
     private void SendDailyTaskMsg(int model)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["model"] = model;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.TASK_GET_TASKLIST, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {
                    JsonData msg = jsontool.newwithstring(content);
                    if (msg.ContainsKey("tasks"))
                    {
                        List<TaskInfo> taskInfo = JsonMapper.ToObject<List<TaskInfo>>(JsonMapper.ToJson(msg["tasks"]));
                        if (taskInfo != null)
                        {
                            RenderDailyTableview(taskInfo);
                        }
                    }
                    if (msg.ContainsKey("score"))
                    {
                        long score = (long)msg["score"];
                        InitTopProgress(score, d_topprogress, 1);
                    }
                    else
                    {
                        long score = 0;
                        InitTopProgress(score, d_topprogress, 1);
                    }
                });
            }
        });
    }

    private void InitTopProgress(long score, GameObject topobj, int type)
    {
        GameObject currentnum = topobj.transform.Find("current/num").gameObject;
        currentnum.GetComponent<TMP_Text>().SetText(score.ToString());
        float value = (float)(score * 1.0 / 150);
        topobj.transform.Find("filled").gameObject.GetComponent<Image>().DOFillAmount(value, 0.3f);
        GameObject lines = topobj.transform.Find("lines").gameObject;
        for(int i = 0; i < lines.transform.childCount; i++)
        {
            GameObject obj = lines.transform.GetChild(i).GetChild(1).gameObject;
            TMP_Text text = obj.GetComponent<TMP_Text>();
            if(score < (i + 1) * 30)
            {
                text.color = new Color(48 / 255f, 48 / 255f, 48 / 255f);
            }
            else
            {
                text.color = new Color(255, 255, 255);
            }
        }
        if(type == 1)
        {
            topobj.transform.Find("getrewardbtn").GetComponent<Button>().onClick.RemoveAllListeners();
            topobj.transform.Find("getrewardbtn").GetComponent<Button>().onClick.AddListener(delegate{D_OneClick_Claim(score);});
        }
        else if(type == 2)
        {
            topobj.transform.Find("getrewardbtn").GetComponent<Button>().onClick.RemoveAllListeners();
            topobj.transform.Find("getrewardbtn").GetComponent<Button>().onClick.AddListener(delegate{W_OneClick_Claim(score);});
        }
        InitTopItem(lines, type);
    }

    private void InitTopItem(GameObject lines, int type)
    {
        List<GameObject> list = new();
        for(int i = 0; i < lines.transform.childCount; i++)
        {
            list.Add(lines.transform.GetChild(i).GetChild(0).gameObject);
        }
        random = new();
        if(type == 1)
        {
            int count = 0;
            foreach (var data in LogBookManager.Instance.dicDailyValueCfgData)
            {
                List<DropCfgData> dropCfgData = DropManager.Instance.GetDropListByDropID(data.Value.dropid);
                if(list[count].transform.Find($"{dropCfgData[0].pid}"))
                    break;
                int num = dropCfgData[0].min == dropCfgData[0].max ? dropCfgData[0].min : random.Next(dropCfgData[0].min, dropCfgData[0].max);
                ItemData itemData = new((int)dropCfgData[0].onlyid, dropCfgData[0].pid, num);
                CommonItem commonItem = LoadCommonitemPrefab(list[count].transform, itemData);
                commonItem.SetSize(Vector3.one * 0.5f);
                list[count].transform.localPosition = new Vector3(0, 57.5f, 0);
                count ++;
            }
        }
        else if(type == 2)
        {
            int count = 0;
            foreach (var data in LogBookManager.Instance.dicWeeklyValueCfgData)
            {
                List<DropCfgData> dropCfgData = DropManager.Instance.GetDropListByDropID(data.Value.dropid);
                if(list[count].transform.Find($"{dropCfgData[0].pid}"))
                    break;
                int num = dropCfgData[0].min == dropCfgData[0].max ? dropCfgData[0].min : random.Next(dropCfgData[0].min, dropCfgData[0].max);
                ItemData itemData = new((int)dropCfgData[0].onlyid, dropCfgData[0].pid, num);
                CommonItem commonItem = LoadCommonitemPrefab(list[count].transform, itemData);
                commonItem.SetSize(Vector3.one * 0.5f);
                list[count].transform.localPosition = new Vector3(0, 57.5f, 0);
                count ++;
            }
        }
    }
    private void D_OneClick_Claim(long currentscore)
    {
        int model = 5;
        foreach (var data in LogBookManager.Instance.dicDailyValueCfgData)
        {
            if(currentscore > data.Value.needactvalue)
            {
                SendFuncRewardMsg(model, data.Value.needactvalue);
            }
        }
    }
    private void W_OneClick_Claim(long currentscore)
    {
        int model = 9;
        foreach (var data in LogBookManager.Instance.dicWeeklyValueCfgData)
        {
            if(currentscore > data.Value.needactvalue)
            {
                SendFuncRewardMsg(model, data.Value.needactvalue);
            }
        }
    }

    //服务器请求获取，每周任务数据
    private void SendWeeklyTaskMsg(int model)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["model"] = model;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.TASK_GET_TASKLIST, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {
                    JsonData msg = jsontool.newwithstring(content);
                    if (msg.ContainsKey("tasks"))
                    {
                        List<TaskInfo> taskInfo = JsonMapper.ToObject<List<TaskInfo>>(JsonMapper.ToJson(msg["tasks"]));
                        if (taskInfo != null)
                        {
                            RenderWeeklyTableview(taskInfo);
                        }
                    }
                    if (msg.ContainsKey("score"))
                    {
                        long score = (long)msg["score"];
                        InitTopProgress(score, w_topprogress, 2);
                    }
                    else
                    {
                        long score = 0;
                        InitTopProgress(score, w_topprogress, 2);
                    }
                });
            }
        });
    }
    // 请求服务器领取奖励
    private void SendTaskRewardMsg(int taskid, int model)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["taskid"] = taskid;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.TASK_GET_TASKREWARD, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {

                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() => {

                    // 领取成功
                    int receivedtaskid = int.Parse(jsontool.newwithstring(content)["taskid"].ToString());
                    if (taskid == receivedtaskid)
                    {
                        UpdateTaskState(receivedtaskid, model);
                    }
                    else
                    {
                    }
                    JsonData json = JsonMapper.ToObject(new JsonReader(content));
                    JsonData chagedata = json["change"]?["changed"];
                    {
                        GameCenter.mIns.userInfo.onChange(chagedata);
                    }
                    JsonData changing = json["change"]?["changing"];
                    if (changing != null)
                    {
                        Debug.Log("?????????????????");
                        WarehouseManager.Instance.ShowChangingTip(changing);
                    }

                });
                
            }
            else
            {
                zxlogger.logerror($"Error: on received reward fail! code:{code}");
            }
        });
    }
    // 请求服务器领取活跃度奖励
    private void SendFuncRewardMsg(int model, int groupid)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["model"] = model;
        jsonData["groupid"] = groupid;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.LOGBOOK_GET_FUNCREWARD, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {
                int receivedtaskid = 0;
                UpdateTaskState(receivedtaskid, model);

                JsonData json = JsonMapper.ToObject(new JsonReader(content));
                JsonData chagedata = json["change"]?["changed"];
                if (chagedata != null)
                {
                    GameCenter.mIns.userInfo.onChange(chagedata);
                }
            }
            else
            {
                zxlogger.logerror($"Error: on received reward fail! code:{code}");
            }
        });
    }

    private void UpdateTaskState(int receivedtaskid, int model)
    {
        switch (model)
        {
            case 5:
            InitDailyActivity();
            break;
            case 7:
            InitMileStone();
            break;
            case 9:
            InitWeeklyActivity();
            break;
            default:
            return;
        }
    }

    private void RenderWeeklyTableview(List<TaskInfo> data)
    {
        TableView tableview = weeklycontent.GetComponent<TableView>();
        tableview.onItemRender =  (GameObject item, int index) => {
            TaskInfo taskinfo = data[index - 1];
            int model = 9;
            int taskid = taskinfo.taskid;
            long count = taskinfo.count;
            long total = taskinfo.total;
            int state = taskinfo.state;
            long degress = count / total;
            item.transform.name = taskid.ToString();
            WeeklyTaskCfgData weeklyTaskCfgData = LogBookManager.Instance.GetWeeklyTaskCfgBytaskid(taskid);
            TaskFourCfgData taskFourCfgData = LogBookManager.Instance.GetTaskFourCfgByTaskid(taskid);
            TMP_Text taskDes = item.transform.Find("taskdes").GetComponent<TMP_Text>();
            TMP_Text addValue = item.transform.Find("addvalue").GetComponent<TMP_Text>();
            taskDes.SetText(GameCenter.mIns.m_LanMgr.GetLan(taskFourCfgData.note));
            addValue.SetText("+" + weeklyTaskCfgData.dropactvalue.ToString());

            GameObject w_btns = item.transform.Find("btns").gameObject;
            GameObject w_underway = w_btns.transform.Find("underway").gameObject;
            GameObject w_jumpbtn = w_btns.transform.Find("jumpbtn").gameObject;
            GameObject w_complete = w_btns.transform.Find("complete").gameObject;
            GameObject w_getreward = w_btns.transform.Find("getreward").gameObject;

            //Image d_filledimg = item.transform.Find("progress/filledimg").gameObject.GetComponent<Image>();
            TMP_Text currentnum = item.transform.Find("currentnum").gameObject.GetComponent<TMP_Text>();
            TMP_Text totalnum = item.transform.Find("totlenum").gameObject.GetComponent<TMP_Text>();

            Button btn = w_getreward.transform.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(delegate {SendTaskRewardMsg(taskid, model);});

            if (count == total && state == 1)
            {
                w_btns.SetActive(true);
                w_underway.SetActive(false);
                w_jumpbtn.SetActive(false);
                w_complete.SetActive(false);
                w_getreward.SetActive(true);
            }
            else if (count == total && state == 2)
            {
                w_btns.SetActive(true);
                w_underway.SetActive(false);
                w_jumpbtn.SetActive(false);
                w_complete.SetActive(true);
                w_getreward.SetActive(false);
            }
            else if (count < total && state == 0)
            {
                w_btns.SetActive(true);
                w_underway.SetActive(false);
                w_jumpbtn.SetActive(true);
                w_complete.SetActive(false);
                w_getreward.SetActive(false);
            }
            currentnum.SetText(count.ToString());
            totalnum.SetText(total.ToString());
        };
        tableview.SetDatas(data.Count, false);
    }

    private void RenderDailyTableview(List<TaskInfo> data)
    {
        TableView tableview = dailycontent.GetComponent<TableView>();
        tableview.onItemRender =  (GameObject item, int index) => {
            TaskInfo taskinfo = data[index - 1];
            int model = 5;
            int taskid = taskinfo.taskid;
            long count = taskinfo.count;
            long total = taskinfo.total;
            int state = taskinfo.state;
            long degress = count / total;
            item.transform.name = taskid.ToString();
            DailyTaskCfgData dailyTaskCfgData = LogBookManager.Instance.GetDailyTaskCfgBytaskid(taskid);
            TaskFourCfgData taskFourCfgData = LogBookManager.Instance.GetTaskFourCfgByTaskid(taskid);
            TMP_Text taskDes = item.transform.Find("taskdes").GetComponent<TMP_Text>();
            TMP_Text addValue = item.transform.Find("addvalue").GetComponent<TMP_Text>();
            taskDes.SetText(GameCenter.mIns.m_LanMgr.GetLan(taskFourCfgData.note));
            addValue.SetText("+" + dailyTaskCfgData.dropactvalue.ToString());

            GameObject d_btns = item.transform.Find("btns").gameObject;
            GameObject d_underway = d_btns.transform.Find("underway").gameObject;
            GameObject d_jumpbtn = d_btns.transform.Find("jumpbtn").gameObject;
            GameObject d_complete = d_btns.transform.Find("complete").gameObject;
            GameObject d_getreward = d_btns.transform.Find("getreward").gameObject;

            Button btn = d_getreward.transform.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(delegate {SendTaskRewardMsg(taskid, model);});

            //Image d_filledimg = item.transform.Find("progress/filledimg").gameObject.GetComponent<Image>();
            TMP_Text currentnum = item.transform.Find("currentnum").gameObject.GetComponent<TMP_Text>();
            TMP_Text totalnum = item.transform.Find("totlenum").gameObject.GetComponent<TMP_Text>();

            if (count == total && state == 1)
            {
                d_btns.SetActive(true);
                d_underway.SetActive(false);
                d_jumpbtn.SetActive(false);
                d_complete.SetActive(false);
                d_getreward.SetActive(true);
            }
            else if (count == total && state == 2)
            {
                d_btns.SetActive(true);
                d_underway.SetActive(false);
                d_jumpbtn.SetActive(false);
                d_complete.SetActive(true);
                d_getreward.SetActive(false);
            }
            else if (count < total && state == 0)
            {
                d_btns.SetActive(true);
                d_underway.SetActive(false);
                d_jumpbtn.SetActive(true);
                d_complete.SetActive(false);
                d_getreward.SetActive(false);
            }
            currentnum.SetText(count.ToString());
            totalnum.SetText(total.ToString());
        };
        tableview.SetDatas(data.Count, false);
    }
}

public class TaskInfo
{
    public int taskid;
    public long count;
    public long total;
    public int state;
    public int floor;
    public int groupid;
}

public class ModelsInfo
{
    public int groupid;
    public int state;
}
