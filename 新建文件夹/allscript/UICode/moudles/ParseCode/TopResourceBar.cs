using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public partial class TopResourceBar
{
	public TopResourceBar(GameObject parentNode)
	{
		InitUI(null );
	}
	public TopResourceBar(GameObject root,bool rootIsSelf)
	{
		InitUI(root);
	}
	public TopResourceBar(GameObject parentNode, BaseWin belongWin, Func<bool> closeCallback = null,string title = null)
	{
		this.belongWin = belongWin;
		this.closeCallback = closeCallback;
		InitUI(null, parentNode, title);
	}
	public GameObject _Root;
	public Transform leftTran;
	public Button btnReturn;
	public Text txTitem;
	public GameObject resItem_1;
	public GameObject resItem_2;
	public GameObject resList;
	private BaseWin belongWin;
	private Func<bool> closeCallback;
	private Text txtCoinNum;
	private Text txtGemNum;
	private static string callbackname = "topresourcebar";

	public async void InitUI(GameObject root, GameObject parentNode = null, string title = null)
    {
        if (root == null)
        {
            root = await ResourcesManager.Instance.LoadUIPrefabSync("topresourcebar");
		}
        if (parentNode != null)
        {
			root.transform.SetParent(parentNode.transform);
		}
		_Root = root;
		leftTran = _Root.transform.Find("left");
		btnReturn = _Root.transform.Find("left/btnReturn").GetComponent<Button>();
		txTitem = _Root.transform.Find("left/txTitem").GetComponent<Text>();
		resItem_1 = _Root.transform.Find("resList/resItem_1").gameObject;
		resItem_2 = _Root.transform.Find("resList/resItem_2").gameObject;
		resList = _Root.transform.Find("resList").gameObject;
		txtCoinNum = resList.transform.Find("resItem_1/txNumber").GetComponent<Text>();
        txtGemNum = resList.transform.Find("resItem_2/txNumber").GetComponent<Text>();

        txtCoinNum.text = GameCenter.mIns.userInfo.getCoinNumStr();
        txtGemNum.text = GameCenter.mIns.userInfo.Gem.ToString();
		GameCenter.mIns.userInfo.addChangeCallback(callbackname, () => {
			GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
			{
				txtCoinNum.text = GameCenter.mIns.userInfo.getCoinNumStr();
				txtGemNum.text = GameCenter.mIns.userInfo.Gem.ToString();
			});
        });

        if (title!=null)
		{
            txTitem.text = title;
        }

        InitRect();
		RegisterButton();
		InitResItems();
	}

	private void InitRect()
    {
		_Root.transform.localPosition = Vector3.zero;
		_Root.transform.localScale = Vector3.one;
		RectTransform rect = _Root.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0, 1);
		rect.anchorMax = new Vector2(1, 1);
		rect.pivot = new Vector2(0.5f, 1.0f);
		rect.offsetMin = new Vector2(0, -60);
		rect.offsetMax = new Vector2(0, 0);

		DoAnim();
	}

	private void InitResItems()
    {

    }

	private void DoAnim()
    {
		leftTran.gameObject.GetOrAddCompoonet<UdoTween>().doanchorposx(0.0f, 0.3f, true).docanvasgroupfade(255.0f, 1f);
		resList.gameObject.GetOrAddCompoonet<UdoTween>().doanchorposy(0.0f, 0.3f, true).docanvasgroupfade(255.0f, 1f);
		//resItem_2.gameObject.GetOrAddCompoonet<UdoTween>().doanchorposy(0.0f, 0.3f, true).docanvasgroupfade(255.0f, 1f);
	}

	private void RegisterButton()
    {
		btnReturn.onClick.AddListener(OnReturn);
	}

	private void UnRegisterButton()
	{
		btnReturn.onClick.RemoveAllListeners();
	}

	private void OnReturn()
    {
        if (closeCallback != null)
        {
			bool isDestroy = closeCallback.Invoke();
			if(isDestroy)
                OnDestroy();
		}
		else { 
			OnDestroy();
        }
    }

	public void OnDestroy()
    {
		GameCenter.mIns.userInfo.removeChangeCallback(callbackname);
        belongWin = null;
		closeCallback = null;
		UnRegisterButton();
        if (_Root != null)
        {
			GameObject.Destroy(_Root);
        }
	}
}
