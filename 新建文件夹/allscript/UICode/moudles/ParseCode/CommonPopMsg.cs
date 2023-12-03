using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public partial class CommonPopMsg : PrefabUIBase
{
	public override string PrefabType { get => "PopMsg"; }

	public CommonPopMsg(GameObject parentNode)
	{
		InitUI(null);
	}
	public CommonPopMsg(GameObject root, bool rootIsSelf)
	{
		InitUI(root);
	}
	public CommonPopMsg(GameObject parentNode, string msg, Action<CommonPopMsg> closeCallback = null)
	{
		this.closeCallback = closeCallback;
		InitUI(null, parentNode);
		txMsg.text = msg;
	}
	public void RepetCommonPopMsg(GameObject _Root, GameObject parentNode, string msg, Action<CommonPopMsg> closeCallback = null)
	{
		this.closeCallback = closeCallback;
		InitUI(_Root, parentNode);
		txMsg.text = msg;
	}
	public Transform root;
	public Text txMsg;
	private Action<CommonPopMsg> closeCallback;

	public async void InitUI(GameObject root, GameObject parentNode = null)
	{
		if (root == null)
		{
			root = await ResourcesManager.Instance.LoadUIPrefabSync("commonpopmsg");
		}
		if (parentNode != null)
		{
			root.transform.SetParent(parentNode.transform);
		}
		_Root = root;
		this.root = _Root.transform.Find("root").transform;
		txMsg = _Root.transform.Find("root/imgBg/txMsg").GetComponent<Text>();

		InitRect();
		RegisterButton();
		InitResItems();
	}

	public override void InitRect()
	{
		_Root.transform.localPosition = Vector3.zero;
		_Root.transform.localScale = Vector3.one;
		RectTransform rect = _Root.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0, 0);
		rect.anchorMax = new Vector2(1, 1);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.offsetMin = new Vector2(0, 0);
		rect.offsetMax = new Vector2(0, 0);

		root.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		//CanvasGroup _cg = root.GetComponent<CanvasGroup>();
		//if (_cg != null)
		//	_cg.alpha = 0;
		DoAnim();
	}

	private void InitResItems()
	{

	}

	public override void DoAnim()
	{
		UcsdoTween.playgameobject(root.gameObject, root.gameObject.GetInstanceID()).doanchorposy(200.0f, 2.0f, true).oncomplete(() => {
			OnDestroy();
		});
	}

	public override void RegisterButton()
	{
	}

	public override void UnRegisterButton()
	{
	}

	private void OnReturn()
	{
		OnDestroy();
	}

	private void OnDestroy()
	{
		closeCallback?.Invoke(this);
		UnRegisterButton();
	}
}
