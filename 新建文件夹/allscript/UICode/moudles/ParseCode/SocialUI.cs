using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public partial class SocialUI : BaseWin
{
	public SocialUI() { }
	public override string Prefab => "socialui";

    public GameObject _Root;
	public Transform nodeLeft;

	public Button btnReturn;
	public Button btnSet;
	public Transform btnList;
	public Transform btnItem;

	protected override void InitUI()
	{
		btns = new List<Button>();
		_Root = uiRoot;

		btnReturn = _Root.transform.Find("nodeLeft/btnReturn").GetComponent<Button>();
		btnList = _Root.transform.Find("nodeLeft/btnList");
		btnItem = _Root.transform.Find("nodeLeft/btnList/btnItem");
		btnSet = _Root.transform.Find("nodeLeft/btnSet").GetComponent<Button>();

		btns.Add(btnReturn);
		btns.Add(btnSet);

		btnReturn.AddListenerBeforeClear(OnClickClose);
		btnSet.AddListenerBeforeClear(OnClickSet);
	}
	partial void OnClickClose();
	partial void OnClickSet();
}
