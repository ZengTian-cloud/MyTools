using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LitJson;

/// <summary>
/// 任务条目预制体
/// </summary>
public class taskItem
{
	public GameObject curRoot;

	public TextMeshProUGUI title;

	public GameObject line;

	public Image icon;

	public TextMeshProUGUI name;

	public Image tag;

	private int curModel;

	private long curTaskid;

	private task_wnd task_Wnd;

    public string taskName;

    private string curTargetTaskKey = $"{GameCenter.mIns.userInfo.Uid}_curTargetTask";

    private leftGroupItem curGroupItem;

    public taskItem(GameObject item,Transform parent, int model, task_wnd task_Wnd,leftGroupItem groupItem)
	{
		this.curModel = model;
		this.task_Wnd = task_Wnd;
        this.curGroupItem = groupItem;

        this.curRoot = item;
		this.title = item.transform.Find("title").GetComponent<TextMeshProUGUI>();
		this.line = item.transform.Find("line").gameObject;
		this.icon = item.transform.Find("icon").GetComponent<Image>();
		this.name = item.transform.Find("name").GetComponent<TextMeshProUGUI>();
		this.tag = item.transform.Find("tag").GetComponent<Image>();

		item.transform.SetParent(parent);
		item.transform.localScale = Vector3.one;
		item.SetActive(true);
    }

	/// <summary>
	/// 刷新单个任务条目
	/// </summary>
	/// <param name="msg"></param>
	public void RefreshItem(JsonData msg)
	{
		if (msg != null)
		{
			if (msg.ContainsKey("taskid"))
			{
				this.curTaskid = long.Parse(msg["taskid"].ToString());
				this.RefreshTraceState();
                switch (this.curModel)
                {
                    case 1://主线
                        TaskMainCfg mainCfg = TaskCfgManager.Instance.GetTaskMianCfgByTaskID(this.curTaskid);
						this.RefreshItemByMainItem(msg, mainCfg);
                        break;
                    case 2://每日委托
                        break;
                    case 3://冒险任务
						TaskAdventureCfg adventureCfg = TaskCfgManager.Instance.GetTaskAdventureCfgByTaskID(this.curTaskid);
                        this.RefreshItemByAdventureItem(msg, adventureCfg);
                        break;
                    case 4://同伴任务
                        break;
                    default:
                        Debug.LogError("未知/新增模块？请检查！");
                        break;
                }


                //优先选中追踪任务
                if (PlayerPrefs.HasKey(this.curTargetTaskKey))
                {
                    long tagrtTask = long.Parse(PlayerPrefs.GetString(this.curTargetTaskKey).Split(';')[0]);
                    if (this.curTaskid == tagrtTask)
                    {
                        task_Wnd.SlectLeftTaskItem(this, this.curGroupItem);
                        task_Wnd.RefreshRightInfo(msg, this.curModel);
                    }
                }
                else
                {
                    //默认选中第一个任务
                    if (task_Wnd.curTaskItem == null)
                    {
                        task_Wnd.SlectLeftTaskItem(this, this.curGroupItem);
                        task_Wnd.RefreshRightInfo(msg, this.curModel);
                    }
                }

                this.curRoot.SetActive(true);
            }
        }
	}

	/// <summary>
	/// 按主任务模块刷新
	/// </summary>
	public void RefreshItemByMainItem(JsonData msg, TaskMainCfg mainCfg)
	{
		this.title.gameObject.SetActive(true);
		this.title.text = GameCenter.mIns.m_LanMgr.GetLan(mainCfg.name);
        this.taskName = GameCenter.mIns.m_LanMgr.GetLan(mainCfg.name1);
        this.name.text = this.taskName;

        this.curRoot.GetComponent<Button>().AddListenerBeforeClear(() =>
		{
			task_Wnd.SlectLeftTaskItem(this, this.curGroupItem);
            task_Wnd.RefreshRightInfo(msg,1);
		});


        
	}

    /// <summary>
    /// 按冒险模块刷新
    /// </summary>
    public void RefreshItemByAdventureItem(JsonData msg, TaskAdventureCfg adventureCfg)
    {
        this.title.gameObject.SetActive(false);
        this.taskName = GameCenter.mIns.m_LanMgr.GetLan(adventureCfg.name1);
        this.name.text = this.taskName;
        this.curRoot.GetComponent<Button>().AddListenerBeforeClear(() =>
        {
            task_Wnd.SlectLeftTaskItem(this, this.curGroupItem);
            task_Wnd.RefreshRightInfo(msg,3);
        });
    }

    /// <summary>
    /// 设置选中状态
    /// </summary>
    /// <param name="bSelect"></param>
    public void SetSelectState(bool bSelect)
	{
		this.icon.sprite = bSelect ? SpriteManager.Instance.GetSpriteSync("ui_s_icon_fubiaoti_01") : SpriteManager.Instance.GetSpriteSync("ui_s_icon_fubiaoti_02");
		this.name.color = bSelect ? commontool.GetColor("#FFFFFF") : commontool.GetColor("#4F6484");
    }

	/// <summary>
	/// 刷新任务追踪状态
	/// </summary>
	public void RefreshTraceState()
	{
		if (PlayerPrefs.HasKey(curTargetTaskKey) && this.curTaskid == long.Parse(PlayerPrefs.GetString(curTargetTaskKey).Split(';')[0]))
		{
            switch (this.curModel)
            {
                case 1://主线
                    this.tag.sprite = SpriteManager.Instance.GetSpriteSync("ui_h_icon_renwu_zhuxian");
                    break;
                case 2://每日委托
                    this.tag.sprite = SpriteManager.Instance.GetSpriteSync("ui_h_icon_renwu_tansuo");
                    break;
                case 3://冒险任务
                    this.tag.sprite = SpriteManager.Instance.GetSpriteSync("ui_h_icon_renwu_maoxian");
                    break;
                case 4://同伴任务
                    this.tag.sprite = SpriteManager.Instance.GetSpriteSync("ui_h_icon_renwu_tongban");
                    break;
                default:
                    Debug.LogError("未知/新增模块？请检查！");
                    break;
            }
            this.tag.gameObject.SetActive(true);
        }
		else
		{
            this.tag.gameObject.SetActive(false);
        }
	}

    
}



