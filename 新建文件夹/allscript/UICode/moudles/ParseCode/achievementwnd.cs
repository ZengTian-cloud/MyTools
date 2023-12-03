using UnityEngine;
using UnityEngine.UI;
using TMPro;
public partial class achievementwnd : BaseWin
{
	public achievementwnd() { }
	public override string Prefab => "achievementwnd";
	public GameObject _Root;
	public TextMeshProUGUI firsttopallnumtxt;
	public TextMeshProUGUI goldnumtxt;
	public TextMeshProUGUI silvernumtxt;
	public TextMeshProUGUI coppernumtxt;
	public TableView firstcontent;
	public GameObject firstpanel;
	public TextMeshProUGUI secondpaneltitle;
	public TableView secondLeftContent;
	public GameObject secondProcessPivot;
	public TableView secondMainContent;
	

	public GameObject SecondPanel;

	protected override void InitUI()
	{
		_Root = uiRoot;
		firsttopallnumtxt = _Root.transform.Find("allpanels/firstpanel/firsttoppanel/firsttopmidimg/firsttopmidmidimg/firsttopallnumtxt").GetComponent<TextMeshProUGUI>();
		goldnumtxt = _Root.transform.Find("allpanels/firstpanel/firsttoppanel/gold/goldnumtxt").GetComponent<TextMeshProUGUI>();
		silvernumtxt = _Root.transform.Find("allpanels/firstpanel/firsttoppanel/silver/TextShadow/silvernumtxt").GetComponent<TextMeshProUGUI>();
		coppernumtxt = _Root.transform.Find("allpanels/firstpanel/firsttoppanel/copper/TextShadow/coppernumtxt").GetComponent<TextMeshProUGUI>();
		firstcontent = _Root.transform.Find("allpanels/firstpanel/firstpanelscrollview/firstviewport/firstcontent").GetComponent<TableView>();
		firstpanel = _Root.transform.Find("allpanels/firstpanel").gameObject;
		secondpaneltitle = _Root.transform.Find("allpanels/SecondPanel/Title/secondpaneltitle").GetComponent<TextMeshProUGUI>();
		secondLeftContent = _Root.transform.Find("allpanels/SecondPanel/leftscrollrect/Viewport/secondLeftContent").GetComponent<TableView>();
		secondProcessPivot = _Root.transform.Find("allpanels/SecondPanel/Image/secondProcessPivot").gameObject;
		SecondPanel = _Root.transform.Find("allpanels/SecondPanel").gameObject;
		secondMainContent= _Root.transform.Find("allpanels/SecondPanel/mainscrollview/Viewport/secondMainContent").GetComponent<TableView>();
	}

}
