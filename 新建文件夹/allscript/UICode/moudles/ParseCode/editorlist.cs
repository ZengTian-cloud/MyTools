using UnityEngine;
using UnityEngine.UI;
public partial class editorlist : BaseWin
{
	public editorlist() { }
	public override string Prefab => "editorlist";
	public GameObject _Root;
	public Button btn_back;
	public Button btn_tool;
	public GameObject toolList;

	protected override void InitUI()
	{
		_Root = uiRoot;
		btn_back = _Root.transform.Find("btn_back").GetComponent<Button>();
		btn_tool = _Root.transform.Find("toolList/btn_tool").GetComponent<Button>();
		toolList = _Root.transform.Find("toolList").gameObject;
		btn_back.onClick.AddListener(btn_back_Click);
	}
	partial void btn_back_Click();
	partial void btn_tool_Click();

}
