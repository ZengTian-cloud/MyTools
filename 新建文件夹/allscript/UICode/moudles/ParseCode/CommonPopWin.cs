using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class CommonPopWin : BaseWin
{
	public CommonPopWin() { }
	public override string Prefab => "commonpopwin";
    public GameObject _Root;
	public GameObject imgBg;
	public TextMeshProUGUI txTitle;
	public TextMeshProUGUI txContent;


	public Button btn_left;
	public Button btn_right;

	protected override void InitUI()
	{
		btns = new List<Button>();
		_Root = uiRoot;
		txTitle = _Root.transform.Find("imgBg/txTitle").GetComponent<TextMeshProUGUI>();
		txContent = _Root.transform.Find("imgBg/txContent").GetComponent<TextMeshProUGUI>();
        btn_left = _Root.transform.Find("imgBg/btn_left").GetComponent<Button>();
        btn_right = _Root.transform.Find("imgBg/btn_right").GetComponent<Button>();
        btn_left.onClick.AddListener(btnOK_Click);
        btn_right.onClick.AddListener(btnNO_Click);
		btns.Add(btn_left);
		btns.Add(btn_right);
	}
	partial void btnOK_Click();
	partial void btnNO_Click();
}
