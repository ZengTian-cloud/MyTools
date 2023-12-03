using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public partial class Warehouse : BaseWin
{
	public Warehouse() { }
	public override string Prefab => "warehouse";

    public GameObject _Root;
	public GameObject tabItem;
	public GameObject tabList;
	public Image nodeLeft;
	public GameObject whItem;
	public Image warehouseList;
	public GameObject nodeMiddle;
	public Image imgItemDetailQuality;
	public Image imgItemDetailBgFrame;
	public Image imgItemDetailName1;
	public Image imgItemDetailName2;
	public Image imgItemDetailIcon;
	public Image imgItemDetailNumber;
	public Image btnItemDetail;
	public Text txItemDetailNumberDes;
	public Text txItemDetailNumberValue;
	public TMPro.TextMeshProUGUI txItemDetailName;
	public Text txBottomDes;
	public Text txBottomDes1;
	public Button btnUse;
	public Image getItem;
	public Image getList;
	public GameObject nodeRight;
	public GameObject nodeRightAreaConent;
	public GameObject imgLockMask;

	public GameObject pop;
	public Text popTxName;
	public Text popTxContent;

	protected override void InitUI()
	{
		btns = new List<Button>();
		_Root = uiRoot;
		tabItem = _Root.transform.Find("nodeLeft/tabList/content/tabItem").gameObject;
		tabList = _Root.transform.Find("nodeLeft/tabList").gameObject;
		nodeLeft = _Root.transform.Find("nodeLeft").GetComponent<Image>();
		whItem = _Root.transform.Find("nodeMiddle/areaList/warehouseList/content/whItem").gameObject;
		warehouseList = _Root.transform.Find("nodeMiddle/areaList/warehouseList").GetComponent<Image>();
		nodeMiddle = _Root.transform.Find("nodeMiddle").gameObject;
		imgItemDetailQuality = _Root.transform.Find("nodeRight/areaContent/top/imgItemDetailQuality").GetComponent<Image>();
		imgItemDetailBgFrame = _Root.transform.Find("nodeRight/areaContent/top/imgItemDetailBgFrame").GetComponent<Image>();
		//imgItemDetailName1 = _Root.transform.Find("nodeRight/areaContent/top/imgItemDetailName1").GetComponent<Image>();
		//imgItemDetailName2 = _Root.transform.Find("nodeRight/areaContent/top/imgItemDetailName1/txItemDetailName/imgItemDetailName2").GetComponent<Image>();
		imgItemDetailIcon = _Root.transform.Find("nodeRight/areaContent/top/imgItemDetailIcon").GetComponent<Image>();
		imgItemDetailNumber = _Root.transform.Find("nodeRight/areaContent/top/imgItemDetailNumber").GetComponent<Image>();
		btnItemDetail = _Root.transform.Find("nodeRight/areaContent/top/btnItemDetail").GetComponent<Image>();
		txItemDetailNumberDes = _Root.transform.Find("nodeRight/areaContent/top/txItemDetailNumberDes").GetComponent<Text>();
		txItemDetailNumberValue = _Root.transform.Find("nodeRight/areaContent/top/txItemDetailNumberValue").GetComponent<Text>();
		txItemDetailName = _Root.transform.Find("nodeRight/areaContent/top/imgItemDetailName1/txItemDetailName").GetComponent<TMPro.TextMeshProUGUI>();
		txBottomDes = _Root.transform.Find("nodeRight/areaContent/bottom/txBottomDes").GetComponent<Text>();
		txBottomDes1 = _Root.transform.Find("nodeRight/areaContent/bottom/txBottomDes1").GetComponent<Text>();
		btnUse = _Root.transform.Find("nodeRight/areaContent/bottom/btnUse").GetComponent<Button>();
		getItem = _Root.transform.Find("nodeRight/areaContent/bottom/getList/content/getItem").GetComponent<Image>();
		getList = _Root.transform.Find("nodeRight/areaContent/bottom/getList").GetComponent<Image>();
		nodeRight = _Root.transform.Find("nodeRight").gameObject;
		nodeRightAreaConent = _Root.transform.Find("nodeRight/areaContent").gameObject;
		imgLockMask = _Root.transform.FindHideInChild("imgLockMask").gameObject;

		pop = btnItemDetail.transform.FindHideInChild("pop").gameObject;
		popTxName = pop.transform.Find("txName").GetComponent<Text>();
		popTxContent = pop.transform.Find("txContent").GetComponent<Text>();


		//tabItem.onClick.AddListener(tabItem_Click);
		//whItem.onClick.AddListener(whItem_Click);
		btnUse.onClick.AddListener(btnUse_Click);
		btnItemDetail.gameObject.GetComponent<Button>().onClick.AddListener(btnItemDetail_Click);
		btns.Add(btnUse);
		btns.Add(btnItemDetail.GetComponent<Button>());
	}
	partial void tabItem_Click();
	partial void whItem_Click();
	partial void btnUse_Click();
	partial void btnItemDetail_Click();
}
