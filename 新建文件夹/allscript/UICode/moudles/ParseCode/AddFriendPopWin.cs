using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AddFriendPopWin : CommomCustomPrefab
{
	#region for PrefabUIBase
	public AddFriendPopWin()
	{
	}

	public AddFriendPopWin(GameObject parentNode, string resName, string abName = "", params object[] param)
	{
		Init(parentNode, resName, abName, param);
	}

	public override string PrefabType { get => "CustomPrefab"; }


	public new async void Init(GameObject parentNode, string resName, string abName = "", params object[] param)
	{
		prefabName = "addfriendpopwin";
        GameObject root = await ResourcesManager.Instance.LoadUIPrefabSync(resName);
		this.param = param;
		InitUI(root, resName, parentNode);
	}

	public new async void InitUI(GameObject root, string resName, GameObject parentNode = null, string abName = "")
	{
		prefabName = "addfriendpopwin";
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
	public Button btnClear;
	public TMP_InputField input;

	public void BindObj(GameObject root)
	{
		_Root = root;
		btns = new List<Button>();
		txTitle = _Root.transform.Find("imgBg/txTitle").GetComponent<TMP_Text>();
		txContent = _Root.transform.Find("imgBg/txContent").GetComponent<TMP_Text>();
		Transform btnParent = _Root.transform.Find("imgBg/hlg");
		btnNo = btnParent.FindHideInChild("btnNo").GetComponent<Button>();
		btnOk = btnParent.FindHideInChild("btnOk").GetComponent<Button>();
		btnClear = _Root.transform.FindHideInChild("imgBg/btnClear", true).GetComponent<Button>();
		input = _Root.transform.Find("imgBg/input").GetComponent<TMP_InputField>();

		txTitle.text = param[0].ToString();
		txContent.text = param[1].ToString();

		btnNo.SetText(param[2].ToString());
		btnOk.SetText(param[3].ToString());

		Action<bool, string> callback = (Action<bool, string>)param[4];
		btnNo.AddListenerBeforeClear(() => {
			callback?.Invoke(false, input.text);
			GameCenter.mIns.m_UIMgr.CloseCustomPrefab(prefabName);
		});
		btnOk.AddListenerBeforeClear(() => {
			callback?.Invoke(true, input.text);
			GameCenter.mIns.m_UIMgr.CloseCustomPrefab(prefabName);
		});

		btnClear.AddListenerBeforeClear(() => {
			input.text = "";
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
}