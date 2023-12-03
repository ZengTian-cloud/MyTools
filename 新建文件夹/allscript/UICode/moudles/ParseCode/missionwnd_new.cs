using UnityEngine;
using UnityEngine.UI;
public partial class missionwnd_new : BaseWin
{
	public missionwnd_new() { }
	public override string Prefab => "missionwnd_new";
	public GameObject _Root;
	/*public GameObject moudleList;
	public GameObject titletext;
	public GameObject missionitem;
	public GameObject rewardTitle;
	public GameObject rewardGroup;
	public GameObject reward;
	public Button btnMask;
	public GameObject missionName;
	public GameObject missionCondition;
	public Button battleBtn;
	public Button btnTestStart;
	public Button btnTestResult;
	public Button btnClose;
	public Button btnUnlock;
	public GameObject missionInfo;
	public GameObject verticalPoint;
	public GameObject horizontalPoint;*/

	protected override void InitUI()
	{
		_Root = uiRoot;
		/*moudleList = _Root.transform.Find("moudleList").gameObject;
		titletext = _Root.transform.Find("missionitem/root/titleimg/titletext").gameObject;
		missionitem = _Root.transform.Find("missionitem").gameObject;
		rewardTitle = _Root.transform.Find("reward/rewardTitle").gameObject;
		rewardGroup = _Root.transform.Find("reward/rewardGroup").gameObject;
		reward = _Root.transform.Find("reward").gameObject;
		btnMask = _Root.transform.Find("missionInfo/btnMask").GetComponent<Button>();
		missionName = _Root.transform.Find("missionInfo/bg/missionName").gameObject;
		missionCondition = _Root.transform.Find("missionInfo/bg/missionCondition").gameObject;
		battleBtn = _Root.transform.Find("missionInfo/bg/battleBtn").GetComponent<Button>();
		btnTestStart = _Root.transform.Find("missionInfo/bg/btnTestStart").GetComponent<Button>();
		btnTestResult = _Root.transform.Find("missionInfo/bg/btnTestResult").GetComponent<Button>();
		btnClose = _Root.transform.Find("missionInfo/bg/btnClose").GetComponent<Button>();
		btnUnlock = _Root.transform.Find("missionInfo/bg/btnUnlock").GetComponent<Button>();
		missionInfo = _Root.transform.Find("missionInfo").gameObject;
		verticalPoint = _Root.transform.Find("verticalPoint").gameObject;
		horizontalPoint = _Root.transform.Find("horizontalPoint").gameObject;
		btnMask.onClick.AddListener(btnMask_Click);
		battleBtn.onClick.AddListener(battleBtn_Click);
		btnTestStart.onClick.AddListener(btnTestStart_Click);
		btnTestResult.onClick.AddListener(btnTestResult_Click);
		btnClose.onClick.AddListener(btnClose_Click);
		btnUnlock.onClick.AddListener(btnUnlock_Click);*/

	}
	/*partial void btnMask_Click();
	partial void battleBtn_Click();
	partial void btnTestStart_Click();
	partial void btnTestResult_Click();
	partial void btnClose_Click();
	partial void btnUnlock_Click();*/

}
