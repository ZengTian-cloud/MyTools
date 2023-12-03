using UnityEngine;
using UnityEngine.UI;
public partial class commonloading_wnd : BaseWin
{
	public commonloading_wnd() { }
	public override string Prefab => "commonloading_wnd";
    public GameObject _Root;
	public CanvasGroup bg;

	protected override void InitUI()
	{
		_Root = uiRoot;
		bg = _Root.transform.Find("bg").GetComponent<CanvasGroup>();

	}

}
