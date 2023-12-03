using UnityEngine;
using UnityEngine.UI;
public partial class battledecode_wnd : BaseWin
{
	public battledecode_wnd() { }
	public override string Prefab => "battledecode_wnd";



    public GameObject _Root;
	public Button btn_setting;
	public GameObject missionIdText;
	public GameObject missionName;
	public Image title;
	public Button btn_back;
	public GameObject top;
	public GameObject EasyTouchCanvas;
	public Button btn_Switch;
	public Button btnResetLens;

	protected override void InitUI()
	{
		_Root = uiRoot;
		btn_setting = _Root.transform.Find("top/btn_setting").GetComponent<Button>();
		missionIdText = _Root.transform.Find("top/title/missionIdText").gameObject;
		missionName = _Root.transform.Find("top/title/missionName").gameObject;
		title = _Root.transform.Find("top/title").GetComponent<Image>();
		btn_back = _Root.transform.Find("top/btn_back").GetComponent<Button>();
		top = _Root.transform.Find("top").gameObject;
		EasyTouchCanvas = _Root.transform.Find("EasyTouchCanvas").gameObject;
		btn_Switch = _Root.transform.Find("btn_Switch").GetComponent<Button>();
		btnResetLens = _Root.transform.Find("btnResetLens").GetComponent<Button>();
		btn_setting.onClick.AddListener(btn_setting_Click);
		btn_back.onClick.AddListener(btn_back_Click);
		btn_Switch.onClick.AddListener(btn_Switch_Click);
		btnResetLens.onClick.AddListener(btnResetLens_Click);

	}
	partial void btn_setting_Click();
	partial void btn_back_Click();
	partial void btn_Switch_Click();
	partial void btnResetLens_Click();

}
