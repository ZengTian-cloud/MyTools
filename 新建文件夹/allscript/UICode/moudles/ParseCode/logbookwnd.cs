using UnityEngine;
using UnityEngine.UI;
using TMPro;
public partial class logbookwnd : BaseWin
{
	public logbookwnd() { }
	public override string Prefab => "logbookwnd";
	public GameObject _Root;
	public GameObject commonitem;
	public GameObject left;
	public GameObject group;
	public GameObject milestone;
	public Button m_select;
	public Button m_notselect;
	public GameObject dailyactivity;
	public Button d_select;
	public Button d_notselect;
    private TMP_Text d_refreshtime;
    public GameObject weeklyactivity;
	public Button w_select;
	public Button w_notselect;
    private TMP_Text w_refreshtime;
    public TMP_Text summarytotlenum;
	public TMP_Text summarycurrentnum;
	public Image summaryfilledimg;
	public GameObject summaryprogress;
	public GameObject rewardcontent;
    private GameObject listmilestone;
    public GameObject rewardtable;
	public Button getrewardbtn;
	public GameObject getrewardobj;
	public GameObject summarytip;
	public GameObject summary;
	public TMP_Text summarytitle;
	public Button summaryleft;
	public Button summaryright;
	public GameObject rightcontent;
	public Image taskfilledimg;
	public GameObject milestonetotlenum;
	public GameObject currentnum;
    private GameObject m_rightbtns;
    private GameObject m_jumpbtn;
    private GameObject m_intask;
    private GameObject m_isreceived;
    private GameObject listdailyactivity;
    public Image taskprogress;
	public GameObject taskitem;
	public GameObject milestonetableview;
	public GameObject tableinfo;
	public GameObject m_selectobg;
	public GameObject m_notselectobg;
	public GameObject d_selectobg;
	public GameObject d_notselectobg;
	public GameObject w_selectobg;
	public GameObject w_notselectobg;
	public GameObject dailytableview;
	public GameObject d_topprogress;
    private GameObject dailycontent;
    private GameObject dailyfill;
    private GameObject d_lines;
	public GameObject d_getrewardbtn;
    private GameObject listweeklyactivity;
    public GameObject weeklytableview;
	public GameObject w_topprogress;
    private GameObject weeklycontent;
    private GameObject weeklyfill;
	public GameObject w_lines;
	public GameObject w_getrewardbtn;

    protected override void InitUI()
	{
		_Root = uiRoot;
		left = _Root.transform.Find("left").gameObject;
		group = left.transform.Find("group").gameObject;
		commonitem = _Root.transform.Find("commonitem").gameObject;

		// 里程碑
		milestone = group.transform.Find("milestone").gameObject;
		m_select = milestone.transform.Find("select").GetComponent<Button>();
		m_notselect = milestone.transform.Find("notselect").GetComponent<Button>();

		// 每日活动
		dailyactivity = group.transform.Find("dailyactivity").gameObject;
		d_select = dailyactivity.transform.Find("select").GetComponent<Button>();
		d_notselect = dailyactivity.transform.Find("notselect").GetComponent<Button>();

		// 每周活动
		weeklyactivity = group.transform.Find("weeklyactivity").gameObject;
		w_select = weeklyactivity.transform.Find("select").GetComponent<Button>();
		w_notselect = weeklyactivity.transform.Find("notselect").GetComponent<Button>();

		tableinfo = _Root.transform.Find("tableinfo").gameObject;

		// 行动摘要
		summary = tableinfo.transform.Find("milestone/summary").gameObject;
		summarytitle = summary.transform.Find("title/text").GetComponent<TMP_Text>();
		summaryleft = summary.transform.Find("title/left").GetComponent<Button>();
		summaryright = summary.transform.Find("title/right").GetComponent<Button>();
		getrewardobj = summary.transform.Find("getrewardbtn").gameObject;
		summarytip = summary.transform.Find("tip").gameObject;
		getrewardbtn = summary.transform.Find("getrewardbtn").GetComponent<Button>();
		rewardtable = summary.transform.Find("rewardtable").gameObject;
		summaryprogress = summary.transform.Find("progress").gameObject;
		summarytotlenum = summaryprogress.transform.Find("totlenum").GetComponent<TMP_Text>();
		summarycurrentnum = summaryprogress.transform.Find("currentnum").GetComponent<TMP_Text>();
		summaryfilledimg = summaryprogress.transform.Find("filledimg").GetComponent<Image>();
		rewardcontent = rewardtable.transform.Find("view/content").gameObject;

		// 里程碑任务列表
		listmilestone = tableinfo.transform.Find("milestone").gameObject;
		milestonetableview = tableinfo.transform.Find("milestone/tableview").gameObject;
		taskitem = milestonetableview.transform.Find("view/taskitem").gameObject;
		rightcontent = milestonetableview.transform.Find("view/content").gameObject;
		taskprogress = taskitem.transform.Find("progress").GetComponent<Image>();
		taskfilledimg = taskprogress.transform.Find("filledimg").GetComponent<Image>();
		milestonetotlenum = taskprogress.transform.Find("totlenum").gameObject;
		currentnum = taskprogress.transform.Find("currentnum").gameObject;
		// m_rightbtns = taskitem.transform.Find("rightbtns").gameObject;
		// m_getrewardbtn = m_rightbtns.transform.Find("getrewardbtn").gameObject;
		// m_jumpbtn = m_rightbtns.transform.Find("jumpbtn").gameObject;
		// m_intask = m_rightbtns.transform.Find("intask").gameObject;
		// m_isreceived = m_rightbtns.transform.Find("isreceived").gameObject;

		// 每日任务列表
		listdailyactivity = tableinfo.transform.Find("dailyactivity").gameObject;
		dailytableview = tableinfo.transform.Find("dailyactivity/tableview").gameObject;
		d_topprogress = tableinfo.transform.Find("dailyactivity/topprogress").gameObject;
		dailycontent = dailytableview.transform.Find("view/content").gameObject;
		dailyfill = d_topprogress.transform.Find("filled").gameObject;
		d_lines = d_topprogress.transform.Find("lines").gameObject;
		d_getrewardbtn = d_topprogress.transform.Find("getrewardbtn").gameObject;
		d_refreshtime = listdailyactivity.transform.Find("refreshtime").GetComponent<TMP_Text>();

		// 每周任务列表
		listweeklyactivity = tableinfo.transform.Find("weeklyactivity").gameObject;
		weeklytableview = tableinfo.transform.Find("weeklyactivity/tableview").gameObject;
		w_topprogress = tableinfo.transform.Find("weeklyactivity/topprogress").gameObject;
		weeklycontent = weeklytableview.transform.Find("view/content").gameObject;
		weeklyfill = w_topprogress.transform.Find("filled").gameObject;
		w_lines = w_topprogress.transform.Find("lines").gameObject;
		w_getrewardbtn = w_topprogress.transform.Find("getrewardbtn").gameObject;
		w_refreshtime = listweeklyactivity.transform.Find("refreshtime").GetComponent<TMP_Text>();

		m_selectobg = milestone.transform.Find("select").gameObject;
		d_selectobg = dailyactivity.transform.Find("select").gameObject;
		w_selectobg = weeklyactivity.transform.Find("select").gameObject;
		m_notselectobg = milestone.transform.Find("notselect").gameObject;
		d_notselectobg = dailyactivity.transform.Find("notselect").gameObject;
		w_notselectobg = weeklyactivity.transform.Find("notselect").gameObject;

		Set_Onclick_Event();

	}
	private void Set_Onclick_Event()
	{
		m_select.onClick.AddListener(m_select_Click);
		m_notselect.onClick.AddListener(m_notselect_Click);

		d_select.onClick.AddListener(d_select_Click);
		d_notselect.onClick.AddListener(d_notselect_Click);

		w_select.onClick.AddListener(w_select_Click);
		w_notselect.onClick.AddListener(w_notselect_Click);

	}
	partial void m_select_Click();
	partial void m_notselect_Click();
	partial void d_select_Click();
	partial void d_notselect_Click();
	partial void w_select_Click();
	partial void w_notselect_Click();
	partial void Getrewardbtn_Click();

}
