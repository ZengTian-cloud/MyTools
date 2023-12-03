using UnityEngine;
using UnityEngine.UI;
public partial class showitemlist : BaseWin
{
	public showitemlist() { }
	public override string Prefab => "showitemlist";
	public GameObject _Root;
	public GameObject content;
	public GameObject iteminfo;
	public GameObject title;
	public Button confirmbtn;

	protected override void InitUI()
	{
		_Root = uiRoot;
		content = _Root.transform.Find("tableview/view/content").gameObject;
		iteminfo = _Root.transform.Find("tableview/view/iteminfo").gameObject;
		title = _Root.transform.Find("title").gameObject;
		confirmbtn = _Root.transform.Find("confirmbtn").GetComponent<Button>();

	}
}
