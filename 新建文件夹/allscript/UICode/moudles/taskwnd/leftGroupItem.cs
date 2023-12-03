using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LitJson;

public class leftGroupItem
{
	public task_wnd task_Wnd;

	private GameObject curRoot;
	private Image title;
	private Image icon;
	private TextMeshProUGUI text;
	private Image jiantou;

	private Transform taskList;
	private GameObject taskItem;

	private int curModel;
	private List<taskItem> taskItemList = new List<taskItem>();

	private bool bOpen;//是否展开
	public leftGroupItem(GameObject item , Transform parent,int model, task_wnd task_Wnd)
	{
		this.curModel = model;
		this.task_Wnd = task_Wnd;

        this.curRoot = item;
		this.title = item.transform.Find("title").GetComponent<Image>();
        this.icon = item.transform.Find("title/icon").GetComponent<Image>();
		this.text = item.transform.Find("title/text").GetComponent<TextMeshProUGUI>();
		this.jiantou = item.transform.Find("title/jiantou").GetComponent<Image>();

		this.taskList = item.transform.Find("taskList");
		this.taskItem = item.transform.Find("taskList/taskItem").gameObject;

        switch (model)
		{
			case 1:
				this.text.text = GameCenter.mIns.m_LanMgr.GetLan("task_title_1");
				break;
			case 2:
                this.text.text = GameCenter.mIns.m_LanMgr.GetLan("task_title_2");
                break;
			case 3:
                this.text.text = GameCenter.mIns.m_LanMgr.GetLan("task_title_3");
                break;
			case 4:
                this.text.text = GameCenter.mIns.m_LanMgr.GetLan("task_title_4");
                break;
			default:
				break;
		}
        item.transform.SetParent(parent);
        item.transform.localScale = Vector3.one;
		item.SetActive(true);
        this.SetBOpen();
        this.title.GetComponent<Button>().AddListenerBeforeClear(() =>
		{
            this.SetBOpen();
			this.taskList.gameObject.SetActive(bOpen);
			task_Wnd.SelectLeftGroup(this);
            task_Wnd.RefreshLayout();


        });
    }

	public void RefreshTask(JsonData msgs)
	{
		for (int i = 0; i < this.taskItemList.Count; i++)
		{
            this.taskItemList[i].curRoot.SetActive(false);
        }
        if (msgs.ContainsKey("tasks"))
		{
			int index = 0;
			foreach (JsonData js in msgs["tasks"])
			{

                int state = js["state"].ToInt32();
				if (state != 2)
				{
                    taskItem item = null;
                    if (index < this.taskItemList.Count)
                    {
                        item = this.taskItemList[index];
                    }
                    else
                    {
                        GameObject obj = GameObject.Instantiate(this.taskItem);
                        item = new taskItem(obj, this.taskList, this.curModel, this.task_Wnd, this);
                        taskItemList.Add(item);
                    }
                    item.RefreshItem(js);
                    index++;
                }
            }
		}
	}

	public void RefreshTrace()
	{
		for (int i = 0; i < taskItemList.Count; i++)
		{
			taskItemList[i].RefreshTraceState();
        }
	}

	/// <summary>
	/// 设置选中状态
	/// </summary>
	/// <param name="bSelect">是否选中</param>
	public void SetSlectState(bool bSelect)
	{
		this.title.sprite = bSelect ? SpriteManager.Instance.GetSpriteSync("ui_c_btn_caidan_dj_chang") : SpriteManager.Instance.GetSpriteSync("ui_c_btn_caidan_zc");
        this.text.color = bSelect ? commontool.GetColor("#FFFFFF") : commontool.GetColor("#2F2F2F");
        this.jiantou.sprite = bSelect ? SpriteManager.Instance.GetSpriteSync("ui_h_btn_liaotian_jiantou_01") : SpriteManager.Instance.GetSpriteSync("ui_h_btn_renwu_jiantou_01");

    }

	private void SetBOpen()
	{
        bOpen = !bOpen;
        if (bOpen)//是否展开
        {
			this.jiantou.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            this.jiantou.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 180f);
        }
    }
}

