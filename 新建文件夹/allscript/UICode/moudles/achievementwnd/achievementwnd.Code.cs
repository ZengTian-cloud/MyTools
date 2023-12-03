using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitJson;
using System;

/*
 * 因为暂时没有美术素材所以未完成的方法:
 * SetSceondLeftImage
 * achievementsecondmainslot文本里面设置图片
 * 测试成就完成时间completetime
 * UI上的中文是不是要用多语言来处理
 * 在领取奖励的时候不会改变奖杯数量
 */

public partial class achievementwnd
{
   public enum cupType {gold,sliver,copper }

    //成就表数据结构        
    struct AchieveGroupStruct
    {
        public int groupID;
        public string groupName;
        //已领取奖励的成就列表
        public List<AchieveStruct> willDoneList;
        //进行中的成就列表
        public List<AchieveStruct> processingList;
        //未领取的成就列表
        public List<AchieveStruct> finishedList;
        //通过任务ID找到这个成就的字典**注意!!!由于成就系统分floor,同一个id会对应多个任务!所以这里的key值用的是id*10+floor!!!
        public Dictionary<long, AchieveStruct> dicIDAndFloorToAchieve;
        public int totalnum//这个大类的成就总数
        { get { return willDoneList.Count + processingList.Count + finishedList.Count; } }
        public int finishNum
        { get { return willDoneList.Count + finishedList.Count; } }

    }
    //成就数据结构    
    public struct AchieveStruct
    {
        public long id;
        //当前进度
        public long count;
        //总进度
        public long total;
        //掉落物品ID
        public long dropid;
        //任务名字
        public string name;
        public cupType cup;
        //任务详细描述
        public string note;
        //任务状态-1=未接取任务 0=未完成 1=已完成 2=已领取",
        public int state;
        //任务档位
        public int floor;
        //是否要生成完成进度条,1是,0否
        public int progressbar;
        public DateTime completetime;
    }
    public override UILayerType uiLayerType => UILayerType.Normal;
    public override string uiAtlasName => "achievementwnd";
    //成就的group表
    [SerializeField] List<AchieveGroupStruct> AchieveGroupList;
    //通过groupID找到group
    Dictionary<long, AchieveGroupStruct> dicGroupIdtoGroup;
    //任务id*10+floor找到对应的字典
    Dictionary<long, AchieveGroupStruct> dicIDAndFloortoGroup;
    //通过01234找到第几个group
    Dictionary<long, AchieveGroupStruct> dicNumtoGroup;
    //用于记录第二页滑动面板的初始位置用于刷新
    Vector2 secondMainPos;
    //当前打开的第几个group
    int openningSecondindex = -1;
    protected override void OnInit()
    {
        base.OnInit();
        
        secondMainPos = secondMainContent.GetComponent<RectTransform>().anchoredPosition;
    }
    protected override void OnOpen()
    {
        base.OnOpen();
        //初始化成就group表
        firstpanel.gameObject.SetActive(false);
        SecondPanel.SetActive(false);

        InitDatas();
        //注册关闭界面
        GameCenter.mIns.m_UIMgr.AddUpdateWin(this);
        TopResourceBar topResBar = new TopResourceBar(_Root, this, () =>
        {
            if (firstpanel.gameObject.activeSelf)
            {
                //如果一级界面打开则关闭界面
                this.Close();
                topResBar = null;
                return true;
            }
            else
            {
                //如果在二级界面则返回一级界面
                SecondPanel.SetActive(false);
                OpenFirstPanel();
                return false;
            }
        }, GameCenter.mIns.m_LanMgr.GetLan("achievement"));


        //打开一级菜单
        OpenFirstPanel();
        //关闭二级菜单
        SecondPanel.SetActive(false);
        //向服务器请求任务信息
        TaskMsgManager.Instance.SendGetTaskList(ServiseCallBack, 6);
    }
    protected override void OnClose()
    {
        AchieveGroupList = null;
        dicNumtoGroup = null;
        AchieveGroupList = null;
        dicIDAndFloortoGroup = null;        
        firstcontent.ClearAll();
        secondLeftContent.ClearAll();
        secondMainContent.ClearAll();
        base.OnClose();
    }
    
    /// <summary>
    /// 数据初始化
    /// </summary>
    void InitDatas()
    {
        AchieveGroupList = new List<AchieveGroupStruct>();
        dicIDAndFloortoGroup = new Dictionary<long, AchieveGroupStruct>();
        dicNumtoGroup = new Dictionary<long, AchieveGroupStruct>();
        dicGroupIdtoGroup = new Dictionary<long, AchieveGroupStruct>();

        for (int i = 0; i < TaskCfgManager.Instance.TaskAchievemrntGroupCfgList.Count; i++)
        {
            AchieveGroupStruct temp = new AchieveGroupStruct();
            temp.groupID = TaskCfgManager.Instance.TaskAchievemrntGroupCfgList[i].group;
            temp.groupName = TaskCfgManager.Instance.TaskAchievemrntGroupCfgList[i].name;
            temp.finishedList = new List<AchieveStruct>();
            temp.processingList = new List<AchieveStruct>();
            temp.willDoneList = new List<AchieveStruct>();
            temp.dicIDAndFloorToAchieve = new Dictionary<long, AchieveStruct>();
            AchieveGroupList.Add(temp);
            dicGroupIdtoGroup.Add(temp.groupID, temp);
            dicNumtoGroup.Add(i, temp);
        }
    }

    /// <summary>
    /// 二级界面左边的面板刷新
    /// </summary>
    void RefreshSecondLeft()
    {

        secondLeftContent.ClearAll();
        //二级界面左边面板无限滚动处理
        if (AchieveGroupList.Count > 0)
        {
            secondLeftContent.onItemRender = (item, index) =>
              {
                  int temp = index - 1;

                  //设置当前的button是否可以触发
                  if (temp == openningSecondindex)//不能触发(当前打开的就是这一页)
                      item.GetComponent<ExButton>().enabled = false;
                  else
                  {
                      item.GetComponent<ExButton>().enabled = true;
                      //事件注册
                      item.GetComponent<achievementSecondLeftbutton>().Init(temp, this);
                  }
                  //图片处理
                  SetSceondLeftImage(item.GetComponent<Image>(), temp);
              };
            secondLeftContent.SetDatas(AchieveGroupList.Count, false);
        }
    }
    /// <summary>
    /// 给二级界面左边按钮触发提供的接口
    /// </summary>
    /// <param name="index"></param>
    public void SecondLeftbuttoninterface(int index)
    {
        openningSecondindex = index;
        RefreshSecondLeft();
        RefreshSecondMainPanel(index);
    }

    /// <summary>
    /// 刷新一级面板
    /// </summary>
    void OpenFirstPanel()
    {

        firstpanel.SetActive(true);
    }
    public void OpenSecondPanel(int index)
    {
        openningSecondindex = index;
        RefreshSecondLeft();
        RefreshSecondMainPanel(index);
        firstpanel.SetActive(false);
        SecondPanel.SetActive(true);
    }
    /// <summary>
    /// 对于二级界面右边的刷新
    /// </summary>
    /// <param name="index">刷新第几大类的成就</param>
    void RefreshSecondMainPanel(int index)
    {

        //标题刷新
        secondpaneltitle.text = GameCenter.mIns.m_LanMgr.GetLan(dicNumtoGroup[index].groupName);

        //成就进度刷新
        secondProcessPivot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dicNumtoGroup[index].totalnum.ToString();

        secondProcessPivot.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = dicNumtoGroup[index].finishNum.ToString();
        //成就进度布局刷新
        //强制刷新sizeFilter
        LayoutRebuilder.ForceRebuildLayoutImmediate(secondProcessPivot.transform.GetChild(0).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(secondProcessPivot.transform.GetChild(2).GetComponent<RectTransform>());
        //计算坐标
        float wide = 0f;
        RectTransform rect;//用于方便书写的一个临时变量
        wide -= secondProcessPivot.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;
        rect = secondProcessPivot.transform.GetChild(1).GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(wide, rect.anchoredPosition.y);
        wide -= rect.sizeDelta.x;
        rect = secondProcessPivot.transform.GetChild(2).GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(wide, rect.anchoredPosition.y);
        wide -= rect.sizeDelta.x;
        rect = secondProcessPivot.transform.GetChild(3).GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(wide, rect.anchoredPosition.y);

        secondMainContent.ClearAll();
        secondMainContent.GetComponent<RectTransform>().anchoredPosition = secondMainPos;

        //二级界面主菜单无限滚动刷新
        AchieveGroupStruct group = dicNumtoGroup[index];
        if (group.totalnum > 0)
        {
            secondMainContent.onItemRender = (GameObject item, int num) =>
            {
                
                int temp = num - 1;
                if (temp < group.finishedList.Count)
                    item.GetComponent<achievementsecondmainslot>().Init(group.finishedList[temp], this);
                else if (temp < group.finishedList.Count + group.processingList.Count)
                    item.GetComponent<achievementsecondmainslot>().Init(group.processingList[temp - group.finishedList.Count], this);
                else
                {                    
                        item.GetComponent<achievementsecondmainslot>().Init(group.willDoneList[temp - group.finishedList.Count - group.processingList.Count], this);           
                }
            };
            //生成slot
            secondMainContent.SetDatas(dicNumtoGroup[index].totalnum, false);
        }
    }
    /// <summary>
    /// 设置二级面板左边的图片
    /// </summary>    
    /// /// <param index=""></第几大类的图片,从0开始>
    void SetSceondLeftImage(Image img, int index)
    {

    }

    /// <summary>
    /// 服务器回调
    /// </summary>
    void ServiseCallBack(JsonData data)
    {
        if (data != null)
        {
            //奖杯数
            if (data.ContainsKey("props"))
            {

                int gold = (int)(data["props"]["2000101"]);
                int silver = (int)(data["props"]["2000102"]);
                int copper = (int)(data["props"]["2000103"]);
                int total = gold + silver + copper;
                //设置奖杯UI
                firsttopallnumtxt.text = total.ToString();
                goldnumtxt.text = gold.ToString();
                silvernumtxt.text = silver.ToString();
                coppernumtxt.text = copper.ToString();
            }

            //处理成就信息
            if (data.ContainsKey("tasks"))
            {

                foreach (JsonData js in data["tasks"])
                {

                    AchieveStruct temp = new AchieveStruct();
                    temp.id = (long)js["taskid"];
                    temp.count = (long)js["count"];
                    temp.total = (long)js["total"];
                    temp.dropid = (long)js["dropid"];
                    temp.name = js["name"].ToString();
                    temp.note = js["note"].ToString();
                    temp.state = (int)js["state"];
                    //解析奖杯类型
                    temp.cup = AnalizeCup(temp.dropid);
                    if (js.ContainsKey("floor"))
                        temp.floor = (int)js["floor"];
                    if (js.ContainsKey("completetime"))
                        temp.completetime = commontool.TimestampToDataTime((long)js["completetime"]);
                    //通过id查到所属group
                    int group = TaskCfgManager.Instance.GetGroupByAchievementID(temp.id);
                    temp.progressbar = TaskCfgManager.Instance.GetTaskAchievementCfgByTaskID(temp.id).progressbar;
                    //加入每个group中的字典
                    dicGroupIdtoGroup[group].dicIDAndFloorToAchieve.Add(temp.id * 10 + temp.floor, temp);
                    dicIDAndFloortoGroup.Add(temp.id * 10 + temp.floor, dicGroupIdtoGroup[group]);
                    if (group > 0)
                    {
                        switch (temp.state)
                        {
                            case 0:
                                dicGroupIdtoGroup[group].processingList.Add(temp);
                                break;
                            case 1:
                                dicGroupIdtoGroup[group].finishedList.Add(temp);
                                break;
                            case 2:
                                dicGroupIdtoGroup[group].willDoneList.Add(temp);
                                break;
                            default:
                                Debug.Log("成就任务状态没有正确分配");
                                break;
                        }
                    }
                }
            }
            if (AchieveGroupList.Count > 0)
            {
                firstcontent.ClearAll();
                //一级界面无限滚动处理
                firstcontent.onItemRender = (GameObject item, int index) =>
                {

                    //设置名字
                    item.transform.Find("prefabccut/achivementclassname").GetComponent<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan(AchieveGroupList[index - 1].groupName);
                    //设置完成数
                    item.transform.Find("prefabccut/prefabfinishcount").GetComponent<TextMeshProUGUI>().text = AchieveGroupList[index - 1].finishNum.ToString();
                    //设置总数
                    item.transform.Find("prefabccut/prefabtotalcount").GetComponent<TextMeshProUGUI>().text = AchieveGroupList[index - 1].totalnum.ToString();

                    //button事件注册
                    item.GetComponent<ExButton>().onClick.AddListener(() =>
                        {
                            int temp = index - 1;
                            OpenSecondPanel(temp);
                        });
                };
                //生成slot
                firstcontent.SetDatas(AchieveGroupList.Count, false);
            }
        }

    }
    /// <summary>
    /// 领取一个奖励的数据处理
    /// </summary>
    /// <param name="id"></param>
    public void FinishAAchieve(long id, int floor)
    {
        long num = id * 10 + floor;        
        AchieveGroupStruct group=new AchieveGroupStruct();
        if (dicIDAndFloortoGroup.ContainsKey(id * 10 + floor))
            group = dicIDAndFloortoGroup[id * 10 + floor];                    
        AchieveStruct achieve=new AchieveStruct();                
        //在待领取列表中找到这个
        for (int i = group.finishedList.Count-1; i >=0; i--)
        {
            if (group.finishedList[i].id * 10 + group.finishedList[i].floor == num)
            {                
                achieve = group.finishedList[i];
                achieve.state = 2;
                achieve.completetime = DateTime.Now;
                group.finishedList.RemoveAt(i);
                break;
            }
            if (i == 0)
                Debug.Log("待领取列表中没找到,出问题了通知天真");
        }           
            group.willDoneList.Add(achieve);
        //刷新二级列表
        RefreshSecondMainPanel(openningSecondindex);
    }
    private cupType AnalizeCup(long dropid)
    {
        List<DropCfgData> datas=DropManager.Instance.GetDropListByDropID(dropid);
        for (int i = 0; i < datas.Count; i++)
        {
            if (datas[i].pid == 2000101)
                return cupType.gold;
            if (datas[i].pid == 2000102)
                return cupType.sliver;
            if (datas[i].pid == 2000103)
                return cupType.copper;
        }
        Debug.Log("未能成功解析成就奖杯信息,请通知天真");
        return cupType.gold;
    }
}
