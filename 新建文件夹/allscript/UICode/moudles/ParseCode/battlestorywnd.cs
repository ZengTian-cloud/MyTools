using UnityEngine;
using UnityEngine.UI;
using TMPro;
public partial class battlestorywnd : BaseWin
{
	public battlestorywnd() { }
	public override string Prefab => "battlestorywnd";
	public GameObject _Root;
	public Image bgicon;
	public Image speaker_l;
	public Image speaker_m;
	public Image speaker_r;
	public TextMeshProUGUI spekerName;
	public TextMeshProUGUI spekerTitle;
	public TextMeshProUGUI speakText;
	public Button btn_next;
	public Button btn_interact;
	public GameObject btn_List;

	protected override void InitUI()
	{
		_Root = uiRoot;
        bgicon = _Root.transform.Find("bg/bgicon").GetComponent<Image>();
        speaker_l = _Root.transform.Find("bg/speaker_l").GetComponent<Image>();
        speaker_m = _Root.transform.Find("bg/speaker_m").GetComponent<Image>();
        speaker_r = _Root.transform.Find("bg/speaker_r").GetComponent<Image>();
        spekerName = _Root.transform.Find("bg/talkRoot/bg/spekerName").GetComponent<TextMeshProUGUI>();
		spekerTitle = _Root.transform.Find("bg/talkRoot/bg/spekerTitle").GetComponent<TextMeshProUGUI>(); 
		speakText = _Root.transform.Find("bg/talkRoot/bg/speakText").GetComponent<TextMeshProUGUI>();
		btn_next = _Root.transform.Find("bg/talkRoot/btn_next").GetComponent<Button>();
        btn_interact = _Root.transform.Find("bg/btn_List/btn_interact").GetComponent<Button>();
		btn_List = _Root.transform.Find("bg/btn_List").gameObject;
	}
}
