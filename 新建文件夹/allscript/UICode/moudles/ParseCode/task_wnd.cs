using UnityEngine;
using UnityEngine.UI;
using TMPro;
public partial class task_wnd : BaseWin
{
	public task_wnd() { }
	public override string Prefab => "task_wnd";

    public GameObject _Root;
	public GameObject groupItem;
	public ScrollRect scroll_group;
	public GameObject left;
	public GameObject contionItem;
	public GameObject conditionList;
    public TextMeshProUGUI desc;
	public ScrollRect scroll_target;
	public ScrollRect scroll_reward;
	public Button btn_go;
	public GameObject right;
	public GameObject nothing;
	public GameObject commonitem;


    protected override void InitUI()
	{
		_Root = uiRoot;
		groupItem = _Root.transform.Find("bg/left/scroll_group/view/content/groupItem").gameObject;
		scroll_group = _Root.transform.Find("bg/left/scroll_group").GetComponent<ScrollRect>();
		left = _Root.transform.Find("bg/left").gameObject;

        contionItem = _Root.transform.Find("bg/right/scroll_target/view/content/conditionList/contionItem").gameObject;
        conditionList = _Root.transform.Find("bg/right/scroll_target/view/content/conditionList").gameObject;
		desc = _Root.transform.Find("bg/right/scroll_target/view/content/desc").GetComponent<TextMeshProUGUI>() ;
		scroll_target = _Root.transform.Find("bg/right/scroll_target").GetComponent<ScrollRect>();
		scroll_reward = _Root.transform.Find("bg/right/scroll_reward").GetComponent<ScrollRect>();
		commonitem = scroll_reward.content.Find("commonitem").gameObject;
        btn_go = _Root.transform.Find("bg/right/btn_go").GetComponent<Button>();
		right = _Root.transform.Find("bg/right").gameObject;
		nothing = _Root.transform.Find("bg/nothing").gameObject;
	}
}
