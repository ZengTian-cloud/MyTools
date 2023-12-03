using UnityEngine;
using UnityEngine.UI;
using TMPro;
public partial class login : BaseWin
{
	public login() { }
	public override string Prefab => "login";
	public GameObject _Root;
	public Button btn_editor;
	public Button btn_login;
	public TMP_InputField ipt_openid;
	public TextMeshProUGUI txtStatus;
	public Transform editorBox;
	public Transform updBox;
	public TextMeshProUGUI txtCopyright;
	public TextMeshProUGUI txtAppver;
	public TextMeshProUGUI txtver;


    protected override void InitUI()
	{
		_Root = uiRoot;
        editorBox = _Root.transform.Find("editor_box");
		updBox = _Root.transform.Find("upd_box");
        btn_editor = editorBox.Find("btn_editor").GetComponent<Button>();
		btn_login = editorBox.Find("btn_login").GetComponent<Button>();
		btn_editor.onClick.AddListener(btn_editor_Click);
		btn_login.onClick.AddListener(btn_login_Click);
		ipt_openid = editorBox.Find("ipt_openid").GetComponent<TMP_InputField>();
        txtCopyright = _Root.transform.Find("copyright").GetComponent<TextMeshProUGUI>();
		txtAppver = _Root.transform.Find("left/txtappver").GetComponent<TextMeshProUGUI>();
        txtver = _Root.transform.Find("left/txtver").GetComponent<TextMeshProUGUI>();
        txtStatus = updBox.transform.Find("txtbg/txtStatus").GetComponent<TextMeshProUGUI>();

	}
	partial void btn_editor_Click();
	partial void btn_login_Click();

}
