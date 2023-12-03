using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public partial class basebattle : BaseWin
{
    public basebattle() { }
    public override string Prefab => "basebattle";
    public GameObject _Root;
    public Button btn_setting;
    public TMP_Text missionIdText;
    public TMP_Text missionName;
    public Image isnew;
    public Button monster;
    public TMP_Text monsterNumber;
    public TMP_Text leaderhptext;
    public Image leaderhp;
    public Image title;
    public Button btn_changeSpeed;
    public Button btn_resonance1;
    public Button btn_pause;
    public Button btn_back;
    public Button btn_resonance;
    public GameObject top;
    public GameObject cardItem;
    public GameObject cardList;
    public Image points;
    public GameObject energySlider;
    public GameObject rightDown;
    public Button leaderSkill;
    public Image buffItem;
    public GameObject buffList;
    public GameObject leftDown;
    public GameObject bosshp;
    public GameObject battleRoot;
    public TableView content;
    public ScrollRect roleList;
    public Button btn_start;
    public Button btn_skill;
    public Button btn_weapon;
    public Button btn_screen;
    public GameObject team;
    public GameObject yuansu;
    public GameObject zhiye;
    public GameObject screenPanel;
    public Button screenMask;
    public GameObject item_shui;
    public GameObject item_huo;
    public GameObject item_feng;
    public GameObject item_lei;
    public GameObject item_def;
    public Button resonancePanel;
    public GameObject monsterInfoPanel;
    public GameObject btns;
    public GameObject lists;
    public GameObject readlyRoot;
    public GameObject infoPanel;
    public GameObject heroInfo;
    public GameObject skillInfo;
    public GameObject backPanel;
    public GameObject backPanel1;
    public CanvasGroup backPanelCanvasGroup;
    public CanvasGroup backPanelCanvasGroup1;

    public GameObject imgListRightMask;
    public GameObject noScreenHero;
    public GameObject needNum;
    public TMP_Text txNeedNum;
    public TMP_Text txNeedNumVal;
    public TMP_Text txNeedNum2;
    public TMP_Text txRecommendLv;

    public Transform heroHeadPointer;

    //筛选界面的预设编队按钮
    public List<Button> screenPanelTeambtns;
    //筛选界面的元素按钮
    public List<Button> screenPanelYuanSubtns;
    //筛选界面的职业按钮
    public List<Button> screenPanelZhiyebtns;


    protected override void InitUI()
    {
        _Root = uiRoot;
        btn_setting = _Root.transform.Find("top/btn_setting").GetComponent<Button>();
        missionIdText = _Root.transform.Find("top/title/missionIdText").GetComponent<TMP_Text>();
        missionName = _Root.transform.Find("top/title/missionName").GetComponent<TMP_Text>();
        isnew = _Root.transform.Find("top/title/monster/isnew").GetComponent<Image>();
        monster = _Root.transform.Find("top/title/monster").GetComponent<Button>();
        monsterNumber = monster.transform.Find("text").GetComponent<TMP_Text>();
        leaderhp = _Root.transform.Find("top/title/leaderhp").GetComponent<Image>();
        leaderhptext = leaderhp.transform.Find("text").GetComponent<TMP_Text>();
        title = _Root.transform.Find("top/title").GetComponent<Image>();
        btn_changeSpeed = _Root.transform.Find("top/btn_changeSpeed").GetComponent<Button>();
        btn_resonance1 = _Root.transform.Find("top/btn_resonance1").GetComponent<Button>();
        btn_pause = _Root.transform.Find("top/btn_pause").GetComponent<Button>();
        btn_back = _Root.transform.Find("top/btn_back").GetComponent<Button>();
        btn_resonance = _Root.transform.Find("top/btn_resonance").GetComponent<Button>();
        top = _Root.transform.Find("top").gameObject;
        cardItem = _Root.transform.Find("battleRoot/rightDown/cardItem").gameObject;
        cardList = _Root.transform.Find("battleRoot/rightDown/cardList").gameObject;
        points = _Root.transform.Find("battleRoot/rightDown/points").GetComponent<Image>();
        energySlider = _Root.transform.Find("battleRoot/slider").gameObject;
        rightDown = _Root.transform.Find("battleRoot/rightDown").gameObject;
        leaderSkill = _Root.transform.Find("battleRoot/leftDown/leaderSkill").GetComponent<Button>();
        buffItem = _Root.transform.Find("battleRoot/leftDown/buffList/buffItem").GetComponent<Image>();
        buffList = _Root.transform.Find("battleRoot/leftDown/buffList").gameObject;
        leftDown = _Root.transform.Find("battleRoot/leftDown").gameObject;
        bosshp = _Root.transform.Find("battleRoot/bosshp").gameObject;
        battleRoot = _Root.transform.Find("battleRoot").gameObject;
        content = _Root.transform.Find("readlyRoot/roleList/view/content").gameObject.GetComponent<TableView>();
        roleList = _Root.transform.Find("readlyRoot/roleList").GetComponent<ScrollRect>();
        btn_start = _Root.transform.Find("readlyRoot/btn_start").GetComponent<Button>();
        btn_skill = _Root.transform.Find("readlyRoot/btn_skill").GetComponent<Button>();
        btn_weapon = _Root.transform.Find("readlyRoot/btn_weapon").GetComponent<Button>();
        btn_screen = _Root.transform.Find("readlyRoot/btn_screen").GetComponent<Button>();
        team = _Root.transform.Find("readlyRoot/screenPanel/bg/team").gameObject;
        imgListRightMask = content.transform.parent.FindHideInChild("imgListRightMask").gameObject;
        screenPanelTeambtns = new List<Button>();
        for (int i = 0; i < 5; i++)
        {
            screenPanelTeambtns.Add(team.transform.Find($"team_{i + 1}").GetComponent<Button>());
        }

        yuansu = _Root.transform.Find("readlyRoot/screenPanel/bg/yuansu").gameObject;
        screenPanelYuanSubtns = new List<Button>();
        for (int i = 0; i < 4; i++)
        {
            screenPanelYuanSubtns.Add(yuansu.transform.Find($"yuansu_{i + 1}").GetComponent<Button>());
        }

        zhiye = _Root.transform.Find("readlyRoot/screenPanel/bg/zhiye").gameObject;
        screenPanelZhiyebtns = new List<Button>();
        for (int i = 0; i < 4; i++)
        {
            screenPanelZhiyebtns.Add(zhiye.transform.Find($"zhiye_{i + 1}").GetComponent<Button>());
        }

        screenPanel = _Root.transform.Find("readlyRoot/screenPanel").gameObject;
        noScreenHero = screenPanel.transform.FindHideInChild("noScreenHero").gameObject;
        screenMask = _Root.transform.Find("readlyRoot/screenPanel/screenMask").GetComponent<Button>();
        item_shui = _Root.transform.Find("resonancePanel/bg/item_shui").gameObject;
        item_huo = _Root.transform.Find("resonancePanel/bg/item_huo").gameObject;
        item_feng = _Root.transform.Find("resonancePanel/bg/item_feng").gameObject;
        item_lei = _Root.transform.Find("resonancePanel/bg/item_lei").gameObject;
        item_def = _Root.transform.Find("resonancePanel/bg/item_def").gameObject;
        resonancePanel = _Root.transform.Find("resonancePanel").GetComponent<Button>();
        monsterInfoPanel = _Root.transform.Find("monsterInfoPanel").gameObject;
        //btns = _Root.transform.Find("monsterInfoPanel/bg/btns").gameObject;
        //lists = _Root.transform.Find("monsterInfoPanel/bg/lists").gameObject;
        readlyRoot = _Root.transform.Find("readlyRoot").gameObject;
        backPanel = _Root.transform.Find("backPanel").gameObject;
        backPanel1 = _Root.transform.Find("backPanel_1").gameObject;
        backPanelCanvasGroup = backPanel.GetComponent<CanvasGroup>();
        backPanelCanvasGroup1 = backPanel1.GetComponent<CanvasGroup>();
        infoPanel = _Root.transform.Find("infoPanel").gameObject;
        heroInfo = _Root.transform.Find("infoPanel/bg/heroInfo").gameObject;
        skillInfo = _Root.transform.Find("infoPanel/bg/skillInfo").gameObject;
        needNum = _Root.transform.Find("readlyRoot/needNum").gameObject;
        txNeedNum = _Root.transform.Find("readlyRoot/needNum/txNeedNum").GetComponent<TMP_Text>();
        txNeedNumVal = _Root.transform.Find("readlyRoot/needNum/txNeedNum/txNeedNumVal").GetComponent<TMP_Text>();
        txNeedNum2 = _Root.transform.Find("readlyRoot/needNum/txNeedNum/txNeedNum2").GetComponent<TMP_Text>();
        txRecommendLv = _Root.transform.Find("readlyRoot/needNum/txRecommendLv").GetComponent<TMP_Text>();
        heroHeadPointer = _Root.transform.FindHideInChild("heroHeadPointer");

        btn_setting.onClick.AddListener(btn_setting_Click);
        monster.onClick.AddListener(monster_Click);
        btn_changeSpeed.onClick.AddListener(btn_changeSpeed_Click);
        btn_resonance1.onClick.AddListener(btn_resonance1_Click);
        btn_pause.onClick.AddListener(btn_pause_Click);
        btn_back.onClick.AddListener(btn_back_Click);
        btn_resonance.onClick.AddListener(btn_resonance_Click);
        leaderSkill.onClick.AddListener(leaderSkill_Click);
        btn_start.onClick.AddListener(btn_start_Click);
        btn_skill.onClick.AddListener(btn_skill_Click);
        btn_weapon.onClick.AddListener(btn_weapon_Click);
        btn_screen.onClick.AddListener(btn_screen_Click);
        screenMask.onClick.AddListener(btn_screenMask_Click);
        //monsterInfoPanel.onClick.AddListener(btn_monsterInfoPanel_Click);
        resonancePanel.onClick.AddListener(btn_resonancePanel_Click);

        RefreshScreenPanelBtnsClick();
        //InitMonsterInfoCompent();
    }
    partial void btn_setting_Click();
    partial void monster_Click();
    partial void btn_changeSpeed_Click();
    partial void btn_resonance1_Click();
    partial void btn_pause_Click();
    partial void btn_back_Click();
    partial void btn_resonance_Click();
    partial void leaderSkill_Click();
    partial void btn_start_Click();
    partial void btn_skill_Click();
    partial void btn_weapon_Click();
    partial void btn_screen_Click();
    partial void btn_screenMask_Click();
    partial void btn_monsterInfoPanel_Click();
    partial void btn_resonancePanel_Click();

}
