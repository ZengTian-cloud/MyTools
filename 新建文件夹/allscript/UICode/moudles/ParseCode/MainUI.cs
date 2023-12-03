using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitJson;

public partial class mainui : BaseWin
{
    public mainui() { }
    public override string Prefab => "mainui";

    Transform nodeRight;
    Transform nodeLeft;
    Transform nodeBottom;

    Image playerAvatar;
    TMP_Text playerLv;

    RectTransform systemInfoLayout;
    GameObject network;
    GameObject network_LocalArea;//wifi
    GameObject network_CarrierData;//流量

    Slider battery;
    GameObject chargingTag;
    TMP_Text time;
    public Transform btnWareHouse1;
    public Transform btnGameLevel1;
    public Transform btnHeroGrow1;
    public Transform btnDrawCard1;
    public Transform btnShop1;
    public TMP_Text btnWareHousename;
    public TMP_Text btnGameLevelname;
    public TMP_Text btnHeroGrowname;
    public TMP_Text btnDrawCardname;
    public TMP_Text btnShopname;


    /// <summary>
    /// 切换（走-跑）
    /// </summary>
    Button btnSwitch;
    /// <summary>
    /// 好友
    /// </summary>
    Button btnFriend;
    /// <summary>
    /// 玩家信息
    /// </summary>
    Button btnPlayerInfo;
    /// <summary>
    /// 玩法
    /// </summary>
    Button btnGameLevel;
    /// <summary>
    /// 英雄养成
    /// </summary>
    Button btnHeroGrow;
    /// <summary>
    /// 仓库
    /// </summary>
    Button btnWareHouse;
    /// <summary>
    /// 抽卡
    /// </summary>
    Button btnDrawCard;
    /// <summary>
    /// 商店
    /// </summary>
    Button btnShop;
    /// <summary>
    /// 编队
    /// </summary>
    Button btnFormation;
    /// <summary>
    /// 邮件
    /// </summary>
    Button btnMail;
    /// <summary>
    /// 任务
    /// </summary>
    Button btnMission;

    Button btnlogbook;
  

    Transform joystickTran;
    Transform joystickThumbTran;
    Transform joystickArrowTran;
    Button btnResetLens;

    Transform gmPanel;
    Button btn_gm;
    Transform gmTip;
    TMP_InputField input_pid;
    TMP_InputField input_count;
    Button getBtn;

    Button targetTask;//追踪任务
    Image targetTaskIcon;
    TextMeshProUGUI targetTaskName;
    TextMeshProUGUI targetTaskCondition;
    Image taskIcon;

    protected override void InitUI()
    {
        nodeRight = uiRoot.transform.Find("nodeRight");
        nodeLeft = uiRoot.transform.Find("nodeLeft");
        nodeBottom = uiRoot.transform.Find("nodeBottom");

        Transform leftTop = nodeLeft.Find("top");
        Transform rightTop = nodeRight.Find("top");

        playerAvatar = Utils.Find<Image>(rightTop, "btn_PlayerInfo/avatar");
        playerLv = Utils.Find<TMP_Text>(rightTop, "btn_PlayerInfo/level/lvNum");

        systemInfoLayout = Utils.Find<RectTransform>(leftTop, "systemInfo");
        network = systemInfoLayout.transform.Find("network").gameObject;
        network_LocalArea = network.transform.Find("localArea").gameObject;
        network_CarrierData = network.transform.Find("carrierData").gameObject;
        battery = Utils.Find<Slider>(systemInfoLayout.transform, "battery");
        chargingTag = battery.transform.Find("charging").gameObject;
        time = Utils.Find<TMP_Text>(systemInfoLayout.transform, "time");
        btnSwitch = Utils.Find<Button>(nodeRight, "bottom/btn_Switch");
        btnFriend = Utils.Find<Button>(nodeBottom, "btn_Friend");
        btnPlayerInfo = Utils.Find<Button>(rightTop, "btn_PlayerInfo");
        btnGameLevel = Utils.Find<Button>(rightTop, "btnRoot/btn_GameLevel");
        btnHeroGrow = Utils.Find<Button>(rightTop, "btnRoot/btn_HeroGrow");
        btnWareHouse = Utils.Find<Button>(rightTop, "btnRoot/btn_WareHouse");
        btnDrawCard = Utils.Find<Button>(rightTop, "btnRoot/btn_DrawCard");
        btnShop = Utils.Find<Button>(rightTop, "btnRoot/btn_Shop");
        btnFormation = Utils.Find<Button>(leftTop, "btnRoot/btn_Formation");
        btnMail = Utils.Find<Button>(leftTop, "btnRoot/btn_Mail");
        btnlogbook = Utils.Find<Button>(leftTop, "btnRoot/btn_logbookwnd");
        btnMission = Utils.Find<Button>(leftTop, "btnRoot/btn_Mission");
        targetTask = Utils.Find<Button>(leftTop, "targetTask");
        targetTaskIcon = Utils.Find<Image>(leftTop, "targetTask/targetTaskIcon");
        targetTaskName = Utils.Find<TextMeshProUGUI>(leftTop, "targetTask/targetTaskName");
        targetTaskCondition = Utils.Find<TextMeshProUGUI>(leftTop, "targetTask/targetTaskCondition");
        taskIcon = Utils.Find<Image>(leftTop, "targetTask/icon");

        //按钮赋值索引
        btnGameLevel1 = rightTop.Find("btnRoot/btn_GameLevel").transform;
        btnGameLevelname = Utils.Find<TMP_Text>(btnGameLevel1, "name");
        btnHeroGrow1 = rightTop.Find("btnRoot/btn_HeroGrow").transform;
        btnHeroGrowname = Utils.Find<TMP_Text>(btnHeroGrow1, "name");
        btnWareHouse1 = rightTop.Find("btnRoot/btn_WareHouse").transform;
        btnWareHousename = Utils.Find<TMP_Text>(btnWareHouse1, "name");
        btnDrawCard1 = rightTop.Find("btnRoot/btn_DrawCard").transform;
        btnDrawCardname = Utils.Find<TMP_Text>(btnDrawCard1, "name");
        btnShop1 = rightTop.Find("btnRoot/btn_Shop").transform;
        btnShopname = Utils.Find<TMP_Text>(btnShop1, "name");

        //按钮赋值文本
        btnGameLevelname.SetText(GameCenter.mIns.m_LanMgr.GetLan("MainUI_1"));
        btnHeroGrowname.SetText(GameCenter.mIns.m_LanMgr.GetLan("MainUI_2"));
        btnWareHousename.SetText(GameCenter.mIns.m_LanMgr.GetLan("MainUI_3"));
        btnDrawCardname.SetText(GameCenter.mIns.m_LanMgr.GetLan("MainUI_4"));
        btnShopname.SetText(GameCenter.mIns.m_LanMgr.GetLan("MainUI_5"));

        //摇杆
        joystickTran = nodeLeft.Find("bottom/EasyTouchCanvas/Joystick");
        joystickThumbTran = joystickTran.transform.Find("Thumb");
        joystickArrowTran = joystickTran.transform.Find("Arrow");
        btnResetLens = uiRoot.transform.FindHideInChild("btnResetLens").GetComponent<Button>();
        //GM面板
        gmPanel = uiRoot.transform.Find("gmPanel");
        btn_gm = gmPanel.Find("btn_gm").GetComponent<Button>();
        gmTip = gmPanel.Find("gmTip");
        input_pid = gmTip.Find("bg/input_pid").GetComponent<TMP_InputField>();
        input_count = gmTip.Find("bg/input_count").GetComponent<TMP_InputField>();
        getBtn = gmTip.Find("bg/getBtn").GetComponent<Button>();
        btnResetLens.onClick.AddListener(btnResetLens_Click);

        RegisterBtnClickEvent();
        InitGM();
       
    }



    private void RegisterBtnClickEvent()
    {
        btnSwitch.onClick.AddListener(OnClickSwitchBtn);
        btnFriend.onClick.AddListener(OnClickFriendBtn);
        btnPlayerInfo.onClick.AddListener(OnClickPlayerInfoBtn);
        btnGameLevel.onClick.AddListener(OnClickGameLevelBtn);
        btnHeroGrow.onClick.AddListener(OnClickHeroGrowBtn);
        btnWareHouse.onClick.AddListener(OnClickWareHouseBtn);
        btnDrawCard.onClick.AddListener(OnClickDrawCardBtn);
        btnShop.onClick.AddListener(OnClickShopBtn);
        btnFormation.onClick.AddListener(OnClickFormationBtn);
        btnMail.onClick.AddListener(OnClickMailBtn);
        btnMission.onClick.AddListener(OnClickTaskBtn);
        btnlogbook.onClick.AddListener(OnClickLogbookBtn);
    }

    private void InitGM()
    {
        btn_gm.onClick.AddListener(() =>
        {

            gmTip.gameObject.SetActive(true);
        });
        gmTip.GetComponent<Button>().onClick.AddListener(() =>
        {
            gmTip.gameObject.SetActive(false);
        });

        getBtn.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(input_pid.text) && !string.IsNullOrEmpty(input_count.text))
            {
                JsonData data = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
                data["pid"] = long.Parse(input_pid.text);
                data["num"] = long.Parse(input_count.text);
                GameCenter.mIns.m_NetMgr.SendData(NetCfg.GM_CHANGEPROP, data, (seqid, code, data) =>
                {
                    if (code == 0)
                    {
                        GameCenter.mIns.m_UIMgr.PopMsg("刷取成功！");
                    }
                });
            }
        });
    }
}
