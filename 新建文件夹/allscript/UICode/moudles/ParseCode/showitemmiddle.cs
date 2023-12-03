using UnityEngine;
using UnityEngine.UI;
public partial class showitemmiddle : BaseWin
{
	public showitemmiddle() { }
	public override string Prefab => "showitemmiddle";
	public GameObject _Root;
	public GameObject bagroot;
	public Image reward;
	public GameObject iteminfo;

	protected override void InitUI()
	{
		_Root = uiRoot;
		bagroot = _Root.transform.Find("reward/bagroot").gameObject;
		reward = _Root.transform.Find("reward").GetComponent<Image>();
		iteminfo = _Root.transform.Find("iteminfo").gameObject;
	}

}
