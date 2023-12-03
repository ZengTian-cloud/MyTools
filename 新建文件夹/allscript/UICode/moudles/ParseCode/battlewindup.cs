using UnityEngine;
using UnityEngine.UI;
public partial class battlewindup : BaseWin
{
	public battlewindup() { }
	public override string Prefab => "battlewindup";

    public GameObject _Root;
	public CanvasGroup win;
	public CanvasGroup lose;
	public GameObject buttons;
	public Button btn_back;
	public Button btn_next;
	public Button btn_restar;
	public Image mask;

	protected override void InitUI()
	{
		_Root = uiRoot;
		win = _Root.transform.Find("mask/win").GetComponent<CanvasGroup>();
		lose = _Root.transform.Find("mask/lose").GetComponent<CanvasGroup>();
		buttons = _Root.transform.Find("mask/buttons").gameObject;
        btn_back = _Root.transform.Find("mask/buttons/btn_back").GetComponent<Button>();
		btn_next = _Root.transform.Find("mask/buttons/btn_next").GetComponent<Button>();
		btn_restar = _Root.transform.Find("mask/buttons/btn_restar").GetComponent<Button>();
		mask = _Root.transform.Find("mask").GetComponent<Image>();
		btn_back.onClick.AddListener(btn_back_Click);
		btn_next.onClick.AddListener(btn_next_Click);
		btn_restar.onClick.AddListener(btn_restar_Click);

	}
	partial void btn_back_Click();
	partial void btn_next_Click();
	partial void btn_restar_Click();

}
