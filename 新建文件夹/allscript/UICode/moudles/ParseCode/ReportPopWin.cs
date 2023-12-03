using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.VFX;

public class ReportPopWin : CommomCustomPrefab
{
	#region for PrefabUIBase
	public ReportPopWin()
	{
	}

	public ReportPopWin(GameObject parentNode, string resName, string abName = "", params object[] param)
	{
		Init(parentNode, resName, abName, param);
	}

	public override string PrefabType { get => "CustomPrefab"; }


	public new async void Init(GameObject parentNode, string resName, string abName = "", params object[] param)
	{
		prefabName = "reportpopwin";
        GameObject root = await ResourcesManager.Instance.LoadUIPrefabSync(resName);
		this.param = param;
		InitUI(root , resName, parentNode);
	}

	public new async void InitUI(GameObject root, string resName, GameObject parentNode = null, string abName = "")
	{
		prefabName = "reportpopwin";
		if (root == null)
		{
			root = await ResourcesManager.Instance.LoadUIPrefabSync(resName);
		}
		if (parentNode != null)
		{
			root.transform.SetParent(parentNode.transform);
		}
		_Root = root;

		InitRect();
		RegisterButton();
		BindObj(_Root);
	}

	public new void OnDestroy()
	{
		if (_Root != null)
		{
			GameObject.Destroy(_Root);
		}
	}
	#endregion
	private object[] param;
	public GameObject imgBg;
	public TMP_Text txTitle;
	public TMP_Text txContent;

	public Button btnNo;
	public Button btnOk;

    public Transform toggles;
    public GameObject toggleObj;
	public Toggle toggleOther;
    public TMP_InputField inputOther;

	public void BindObj(GameObject root)
	{
		_Root = root;
		btns = new List<Button>();
		txTitle = _Root.transform.Find("imgBg/txTitle").GetComponent<TMP_Text>();
		txContent = _Root.transform.Find("imgBg/txContent").GetComponent<TMP_Text>();
		Transform btnParent = _Root.transform.Find("imgBg/hlg");
		btnNo = btnParent.FindHideInChild("btnNo").GetComponent<Button>();
		btnOk = btnParent.FindHideInChild("btnOk").GetComponent<Button>();
        inputOther = _Root.transform.Find("imgBg/inputOther").GetComponent<TMP_InputField>();
        toggleOther = _Root.transform.Find("imgBg/toggleOther").GetComponent<Toggle>();
		toggles = _Root.transform.Find("imgBg/toggles").transform;
        toggleObj = toggles.FindHideInChild("toggleItem").gameObject;

        txTitle.text = param[0].ToString();
		txContent.text = param[1].ToString();

		btnNo.SetText(param[2].ToString());
		btnOk.SetText(param[3].ToString());

		Action<List<int>, string> callback = (Action<List<int>, string>)param[4];
		btnNo.AddListenerBeforeClear(() => {
			callback?.Invoke(null, inputOther.text);
			GameCenter.mIns.m_UIMgr.CloseCustomPrefab(prefabName);
		});
		btnOk.AddListenerBeforeClear(() => {
			callback?.Invoke(GetIsOnToggls(), inputOther.text);
			GameCenter.mIns.m_UIMgr.CloseCustomPrefab(prefabName);
		});

		CreateToggles();
		toggleOther.onValueChanged.AddListener((b) => {
			if (b && GetIsToggleNumber() > m_maxToggleNumber)
			{
				toggleOther.isOn = !b;
				GameCenter.mIns.m_UIMgr.PopMsg(string.Format("最多选择{0}个", m_maxToggleNumber));
			}
        });
    }

	public override void InitRect()
	{
		_Root.transform.localPosition = Vector3.zero;
		_Root.transform.localScale = Vector3.one;
		//RectTransform rect = _Root.GetComponent<RectTransform>();
		//rect.anchorMin = new Vector2(0, 0);
		//rect.anchorMax = new Vector2(1, 1);
		//rect.pivot = new Vector2(0.5f, 0.5f);
		//rect.offsetMin = new Vector2(0, 0);
		//rect.offsetMax = new Vector2(0, 0);

		DoAnim();
	}


	public override void DoAnim()
	{

	}

	public override void RegisterButton()
	{
		//btns = new List<Button>();
		//btnOK.onClick.AddListener(OnClockOK);
		//btnNO.onClick.AddListener(OnClickNO);
		//btns.Add(btnOK);
		//btns.Add(btnNO);
	}

	public override void UnRegisterButton()
	{
		if (btns != null && btns.Count > 0)
		{
			foreach (var btn in btns)
			{
				btn.onClick.RemoveAllListeners();
				GameObject.Destroy(btn.gameObject);
			}
		}
	}

	List<ToggleItem> toggleItems = new List<ToggleItem>();
	private int m_maxToggleNumber = 3;
    private int GetIsToggleNumber()
	{
		int number = 0;
		foreach (var item in toggleItems)
		{
			number += (item.toggle.isOn ? 1 : 0);
        }
		return toggleOther.isOn ? number + 1 : number;
    }

    private List<int> GetIsOnToggls()
    {
		List<int> indexs = new List<int>();
        int index = 1;
        foreach (var item in toggleItems)
        {
			if (item.toggle.isOn)
			{
                indexs.Add(index);
            }
            index++;
        }
        return indexs;
    }

    private void CreateToggles()
	{
		foreach (var item in toggleItems)
		{
            item.OnDestroy();
        }
		toggleItems.Clear();

		// TODO:后去走配置
		foreach (EnumReportType tp in Enum.GetValues(typeof(EnumReportType)))
		{
			if (tp != EnumReportType.None && tp != EnumReportType.Other)
			{
                GameObject o = GameObject.Instantiate(toggleObj);
                o.name = "toggle_" + tp.ToString();
                o.transform.SetParent(toggles);
                o.transform.localScale = Vector3.one;
				ToggleItem toggleItem = new ToggleItem(o.GetComponent<Toggle>(), tp, OnToggleValueChanged);
				o.SetActive(true);
				toggleItems.Add(toggleItem);
            }
        }
	}

	private void OnToggleValueChanged(ToggleItem toggleItem, bool isOn)
	{
        if (isOn && GetIsToggleNumber() > m_maxToggleNumber)
        {
            if (toggleItem != null)
            {
                toggleItem.toggle.isOn = !isOn;
				GameCenter.mIns.m_UIMgr.PopMsg(string.Format("最多选择{0}个", m_maxToggleNumber));
            }
        }
	}

    private class ToggleItem
	{
		public Toggle toggle;
		public EnumReportType reportType;
        public Action<ToggleItem, bool> callback;
        public ToggleItem(Toggle toggle, EnumReportType reportType, Action<ToggleItem, bool> callback)
		{
			this.toggle = toggle;
			this.reportType = reportType;
			this.callback = callback;
			toggle.onValueChanged.AddListener((b) => {
				callback?.Invoke(this, b);
            });
			SetText();
        }

		public void OnDestroy()
		{
			if (toggle != null)
			{
				GameObject.Destroy(toggle.gameObject);
			}
		}

		public void SetText()
		{
			switch (reportType)
			{
				case EnumReportType.None:
					toggle.GetComponentInChildren<TMP_Text>().SetTextExt("");
                    break;
				case EnumReportType.AbuseOtherRole:
                    toggle.GetComponentInChildren<TMP_Text>().SetTextExt("辱骂他人");
                    break;
				case EnumReportType.ImproperSpeech:
                    toggle.GetComponentInChildren<TMP_Text>().SetTextExt("不正当言论");
                    break;
				case EnumReportType.Advertising:
                    toggle.GetComponentInChildren<TMP_Text>().SetTextExt("发布广告");
                    break;
				case EnumReportType.OnHook:
                    toggle.GetComponentInChildren<TMP_Text>().SetTextExt("挂机/消极游戏");
                    break;
				case EnumReportType.ExternalHanging:
                    toggle.GetComponentInChildren<TMP_Text>().SetTextExt("使用外挂");
                    break;
				case EnumReportType.DataViolation:
                    toggle.GetComponentInChildren<TMP_Text>().SetTextExt("资料违规");
                    break;
				case EnumReportType.Other:
                    toggle.GetComponentInChildren<TMP_Text>().SetTextExt("其他");
                    break;
				default:
					break;
			}
		}
    }

    // TODO:后去走配置
    private enum EnumReportType
	{
		// None
		None = 0,
		// 辱骂他人
        AbuseOtherRole = 1,
        // 不正当言论
        ImproperSpeech,
        // 发布广告
        Advertising,
        // 挂机/消极游戏
        OnHook,
        // 使用外挂
        ExternalHanging,
        // 资料违规
        DataViolation,
        // 其他
		Other,
    }
}