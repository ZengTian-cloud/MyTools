using UnityEngine;
using UnityEngine.UI;
public partial class showiteminfo : BaseWin
{
	public showiteminfo() { }
	public override string Prefab => "showiteminfo";
	// public showiteminfo(GameObject root,bool rootIsSelf)
	// {
	// 	InitUI(root);
	// }
	public GameObject _Root;
	public GameObject content;
	public GameObject iteminfo;
	public GameObject bagroot;

	// public void InitUI(GameObject root)
	// {
	// 	_Root = uiRoot;
	// 	content = _Root.transform.Find("tableview/view/content").gameObject;
	// 	iteminfo = _Root.transform.Find("tableview/view/iteminfo").gameObject;
	// 	bagroot = _Root.transform.Find("reward/bagroot").gameObject;

	// }
	protected override void InitUI()
	{
		_Root = uiRoot;
		content = _Root.transform.Find("tableview/view/content").gameObject;
		iteminfo = _Root.transform.Find("tableview/view/iteminfo").gameObject;
		bagroot = _Root.transform.Find("reward/bagroot").gameObject;
	}

}
