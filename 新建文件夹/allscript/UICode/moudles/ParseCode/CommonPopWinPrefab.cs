using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommonPopWinPrefab : PrefabUIBase
{
    public override string PrefabType { get => "PopWindowPrefab"; }

    public CommonPopWinPrefab(GameObject parentNode, PopWinStyle popWinStyle)
	{
		InitUI(null, popWinStyle, parentNode);
	}
    //public commonpopwinprefab(GameObject parentNode, string title, string content, Action<commonpopwinprefab, bool> closeCallback = null)
    //{
    //	this.closeCallback = closeCallback;
    //	InitUI(null, title, content, parentNode);
    //}
    public void RepetCommonPopWinPrefab(GameObject _Root, GameObject parentNode, PopWinStyle popWinStyle)
    {
		InitUI(_Root, popWinStyle, parentNode);
    }

	public PopWinStyle popWinStyle;
	private Action<int> btnCallback = null;
	private Action<int, CommonPopWinPrefab> closeCallback = null;
	public GameObject imgBg;
	public TMP_Text txTitle;
	public TMP_Text txContent;


	public Button btn;

	public async void InitUI(GameObject root, PopWinStyle popWinStyle, GameObject parentNode = null)
	{
		this.popWinStyle = popWinStyle;
		if (root == null)
		{
			root = await ResourcesManager.Instance.LoadUIPrefabAndAtlasSync("commonpopwinprefab","pop");
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
		btn = btnParent.FindHideInChild("btn").GetComponent<Button>();

        txTitle.text = popWinStyle.title;
		txContent.text = popWinStyle.content;
		btnCallback = popWinStyle.btnClickCallback;
		closeCallback = popWinStyle.closeCallback;


		if (popWinStyle.btnCount > 0)
        {
			
            for (int i = 0; i < popWinStyle.btnCount; i++)
            {
				Button button = GameObject.Instantiate(btn.gameObject).gameObject.GetComponent<Button>();
				button.transform.SetParent(btnParent);
				btns.Add(button);
				button.name = "btn_" + i;
				button.onClick.AddListener(delegate () { OnClickBtn(button.gameObject); });
                if (popWinStyle.btnTxs != null && popWinStyle.btnTxs[i] != null)
                {
					button.transform.GetChild(0).GetComponent<TMP_Text>().text = popWinStyle.btnTxs[i];
				}
                else
                {
                    if (i == 0)
						button.transform.GetChild(0).GetComponent<TMP_Text>().text = GameCenter.mIns.m_LanMgr.GetLan("common_confirm_ok");
                    else
						button.transform.GetChild(0).GetComponent<TMP_Text>().text = GameCenter.mIns.m_LanMgr.GetLan("common_confirm_cancel");
				}
				if (popWinStyle.btnResNames != null && popWinStyle.btnResNames[i] != null)
				{
                    button.gameObject.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(popWinStyle.btnResNames[i]);
				}
				button.transform.localScale = Vector3.one;
				button.gameObject.SetActive(true);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(btnParent.GetComponent<RectTransform>());
			float allBtnSize_x = btn.GetComponent<RectTransform>().sizeDelta.x * popWinStyle.btnCount;
            if (allBtnSize_x > btnParent.GetComponent<RectTransform>().sizeDelta.x)
            {
				float btnSizeX = btnParent.GetComponent<RectTransform>().sizeDelta.x / popWinStyle.btnCount;
                foreach (var item in btns)
                {
					item.GetComponent<RectTransform>().sizeDelta = new Vector2(btnSizeX, item.GetComponent<RectTransform>().sizeDelta.y);
                }
			}
		}


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

    private void OnClickBtn(GameObject btnObj)
    {
		UnRegisterButton();
		int index = int.Parse(btnObj.name.Split(new char[] { '_' })[1]);
		btnCallback?.Invoke(index);
		closeCallback?.Invoke(index, this);
    }
}