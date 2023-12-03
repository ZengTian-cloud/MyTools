using UnityEngine;
using UnityEngine.UI;
public partial class mapeditor : BaseWin
{
	public mapeditor() { }
	public override string Prefab => "mapeditor";
	public GameObject _Root;
	public Button btn_back;
	public InputField input_id;
	public Button btn_get;
	public Button btn_setcan;
	public Button btn_setcannot;
	public Button btn_save;
	public Button btn_change;
	public Button btn_switch;
	public Button btn_init;
	public TableView content;
	public Button btn_close;
	public Button btn_ro;
	public GameObject matlist;
	public GameObject airwall;

    protected override void InitUI()
	{
		_Root = uiRoot;
		btn_back = _Root.transform.Find("btn_back").GetComponent<Button>();
		input_id = _Root.transform.Find("input_id").GetComponent<InputField>();
		btn_get = _Root.transform.Find("btn_get").GetComponent<Button>();
		btn_setcan = _Root.transform.Find("btn_setcan").GetComponent<Button>();
		btn_setcannot = _Root.transform.Find("btn_setcannot").GetComponent<Button>();
		btn_save = _Root.transform.Find("btn_save").GetComponent<Button>();
		btn_change = _Root.transform.Find("btn_change").GetComponent<Button>();
		btn_switch = _Root.transform.Find("btn_switch").GetComponent<Button>();
		btn_init = _Root.transform.Find("btn_init").GetComponent<Button>();
		content = _Root.transform.Find("matlist/scroll/view/content").GetComponent<TableView>();
		btn_close = _Root.transform.Find("matlist/btn_close").GetComponent<Button>();
		btn_ro = _Root.transform.Find("matlist/btn_ro").GetComponent<Button>();
		matlist = _Root.transform.Find("matlist").gameObject;
        airwall = _Root.transform.FindHideInChild("airwall").gameObject;
        btn_back.onClick.AddListener(btn_back_Click);
		btn_get.onClick.AddListener(btn_get_Click);
		btn_setcan.onClick.AddListener(btn_setcan_Click);
		btn_setcannot.onClick.AddListener(btn_setcannot_Click);
		btn_save.onClick.AddListener(btn_save_Click);
		btn_change.onClick.AddListener(btn_change_Click);
		btn_switch.onClick.AddListener(btn_switch_Click);
		btn_init.onClick.AddListener(btn_init_Click);
		btn_close.onClick.AddListener(btn_close_Click);
		btn_ro.onClick.AddListener(btn_ro_Click);

	}
	partial void btn_back_Click();
	partial void btn_get_Click();
	partial void btn_setcan_Click();
	partial void btn_setcannot_Click();
	partial void btn_save_Click();
	partial void btn_change_Click();
	partial void btn_switch_Click();
	partial void btn_init_Click();
	partial void btn_close_Click();
	partial void btn_ro_Click();

}
