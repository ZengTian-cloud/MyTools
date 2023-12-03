
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommonPopFullScreenPrefab : PrefabUIBase
{
	public override string PrefabType { get => "PopFullScreenPrefab"; }

	public CommonPopFullScreenPrefab(GameObject parentNode, PopFullScreenStyle popFullScreenStyle)
	{
		InitUI(null, popFullScreenStyle, parentNode);
	}
	public void RepetCommonPopFullScreenPrefab(GameObject _Root, GameObject parentNode, PopFullScreenStyle popFullScreenStyle)
	{
		InitUI(_Root, popFullScreenStyle, parentNode);
	}

	public PopFullScreenStyle popFullScreenStyle;
	private Action<int> btnCallback = null;
	private Action<int, CommonPopFullScreenPrefab> closeCallback = null;
	public GameObject imgBg;
	public TMP_Text txTitle;
	public TMP_Text txContent;


	public Button btnL;
	public Button btnR;

	public async void InitUI(GameObject root, PopFullScreenStyle popFullScreenStyle, GameObject parentNode = null)
	{
		
		this.popFullScreenStyle = popFullScreenStyle;
		if (root == null)
		{
			root = await ResourcesManager.Instance.LoadUIPrefabAndAtlasSync("commonpopfullscreenprefab", "pop");
		}
		if (parentNode != null)
		{
			root.transform.SetParent(parentNode.transform);
		}
		_Root = root;
		btns = new List<Button>();
		txTitle = _Root.transform.Find("imgBg/txTitle").GetComponent<TMP_Text>();
		txContent = _Root.transform.Find("imgBg/txContent").GetComponent<TMP_Text>();
		Transform btnParent = _Root.transform.Find("imgBg/hlg");
		btnL = _Root.transform.Find("imgBg/btnL").GetComponent<Button>();
		btnR = _Root.transform.Find("imgBg/btnR").GetComponent<Button>();

		string ls = GameCenter.mIns.m_LanMgr.GetLan("common_fullconfirm_cancel");
		string rs = GameCenter.mIns.m_LanMgr.GetLan("common_fullconfirm_ok");

		if (popFullScreenStyle.btnCount == 1)
        {
			btnL.gameObject.SetActive(false);
			ls = (popFullScreenStyle.btnResNames != null && popFullScreenStyle.btnResNames.Count > 0) ? popFullScreenStyle.btnResNames[0] : GameCenter.mIns.m_LanMgr.GetLan("common_fullconfirm_cancel");
			rs = (popFullScreenStyle.btnResNames != null && popFullScreenStyle.btnResNames.Count > 0) ? popFullScreenStyle.btnResNames[0] : GameCenter.mIns.m_LanMgr.GetLan("common_fullconfirm_ok");
		}
        else
        {
			btnL.gameObject.SetActive(true);
			ls = (popFullScreenStyle.btnResNames != null && popFullScreenStyle.btnResNames.Count > 1) ? popFullScreenStyle.btnResNames[0] : GameCenter.mIns.m_LanMgr.GetLan("common_fullconfirm_cancel");
			rs = (popFullScreenStyle.btnResNames != null && popFullScreenStyle.btnResNames.Count > 1) ? popFullScreenStyle.btnResNames[1] : GameCenter.mIns.m_LanMgr.GetLan("common_fullconfirm_ok");
		}

		btnL.transform.GetChild(0).GetComponent<TMP_Text>().text = ls;
		btnR.transform.GetChild(0).GetComponent<TMP_Text>().text = rs;

		btnL.onClick.AddListener(delegate () { OnClickBtnNo(btnL.gameObject); });
		btnR.onClick.AddListener(delegate () { OnClickBtnOk(btnR.gameObject); });
		btns.Add(btnL);
		btns.Add(btnR);

		txTitle.text = popFullScreenStyle.title;
		txContent.text = popFullScreenStyle.content;
		btnCallback = popFullScreenStyle.btnClickCallback;
		closeCallback = popFullScreenStyle.closeCallback;

		InitRect();
		RegisterButton();
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

	}

	public override void UnRegisterButton()
	{
		if (btns != null && btns.Count > 0)
		{
			foreach (var btn in btns)
			{
				btn.onClick.RemoveAllListeners();
			}
		}
	}

	private void OnClickBtnOk(GameObject btnObj)
	{
		UnRegisterButton();
		btnCallback?.Invoke(1);
		closeCallback?.Invoke(1, this);
	}

	private void OnClickBtnNo(GameObject btnObj)
	{
		UnRegisterButton();
		btnCallback?.Invoke(0);
		closeCallback?.Invoke(0, this);
	}
}