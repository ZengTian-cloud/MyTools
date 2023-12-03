using UnityEngine;
using UnityEngine.UI;
public partial class chapterwnd : BaseWin
{
	public chapterwnd() { }
	public override string Prefab => "chapterwnd";

    public override UILayerType uiLayerType => UILayerType.Normal;

    public GameObject _Root;
	public TableView content;
	public GameObject chapterItem;

	protected override void InitUI()
	{
		_Root = uiRoot;
		content = _Root.transform.Find("scroll/view/content").GetComponent<TableView>();
		chapterItem = _Root.transform.Find("chapterItem").gameObject;

	}

}
