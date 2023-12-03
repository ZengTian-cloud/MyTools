using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CommonSkillIcon
{
    #region base data
    private long m_SkillId;
    private long m_HeroId;
    public BattleSkillCfg m_SkillCfg;
    private HeroInfoCfgData m_HeroInfoCfg;
    #endregion

    #region ui component
    public CanvasGroup canvasGroup { get; private set; }
    public GameObject obj { get; private set; }
    public Transform nodebg { get; private set; }

    public Button btnClick { get; private set; }
    public Image imgIcon { get; private set; }
    public Image imgQuality { get; private set; }
    public Image imgElementIcon { get; private set; }
    public Image imgSkillTypeDes { get; private set; }

    public TMP_Text desc2 { get; private set; }
    public GameObject cd { get; private set; }
    public TMP_Text txCD { get; private set; }
    public GameObject expend { get; private set; }
    public TMP_Text txExpend { get; private set; }
    public GameObject selected { get; private set; }
    public GameObject nodeNull { get; private set; }

    public SequenceFrameHelper sequenceFrameHelper { get; private set; }
    #endregion

    private Action<BattleSkillCfg> cliclCallback;
    public CommonSkillIcon(GameObject o, long skillId, long heroId, Action<BattleSkillCfg> cliclCallback)
    {
        obj = o;
        m_SkillId = skillId;
        this.cliclCallback = cliclCallback;
        m_HeroId = heroId;
        SetCfg();
        BindUI();
        SetUIData();

    }

    public CommonSkillIcon(GameObject o, long skillId, long heroId, Action<BattleSkillCfg> cliclCallback, bool isInit)
    {
        obj = o;
        m_SkillId = skillId;
        this.cliclCallback = cliclCallback;
        m_HeroId = heroId;
        SetCfg();
        BindUI();
        SetUIData();

        // isInit
    }

    protected virtual void SetCfg()
    {
        m_SkillCfg = BattleCfgManager.Instance.GetSkillCfgBySkillID(m_SkillId);
        if (m_HeroId > 0)
            m_HeroInfoCfg = GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(m_HeroId);
    }

    protected virtual void BindUI()
    {
        btnClick = obj.GetComponent<Button>();
        nodebg = obj.transform.Find("bg");
        nodeNull = obj.transform.FindHideInChild("null").gameObject;
        canvasGroup = nodebg.GetComponent<CanvasGroup>();

        imgIcon = nodebg.Find("icon").GetComponent<Image>();
        imgQuality = nodebg.Find("quality").GetComponent<Image>();
        imgElementIcon = nodebg.Find("yuansu/icon").GetComponent<Image>();
        imgSkillTypeDes = nodebg.Find("imgSkillTypeDes").GetComponent<Image>();
        desc2 = nodebg.Find("desc2").GetComponent<TMP_Text>();
        cd = nodebg.FindHideInChild("cd").gameObject;
        txCD = cd.transform.FindHideInChild("text").GetComponent<TMP_Text>();
        expend = nodebg.FindHideInChild("expend").gameObject;
        txExpend = expend.transform.FindHideInChild("text").GetComponent<TMP_Text>();

        selected = nodebg.FindHideInChild("selected").gameObject;
        sequenceFrameHelper = selected.GetComponent<SequenceFrameHelper>();
        if (sequenceFrameHelper != null)
        {
            sequenceFrameHelper.SetOverActive(false);
        }
        //if (sequenceFrameHelper != null)
        //{
        //    sequenceFrameHelper.Init("uieffect_skill_xuanzhong", 1, 7, true);
        //}
        btnClick.AddListenerBeforeClear(() => {
            cliclCallback?.Invoke(m_SkillCfg);
        });


    }

    protected virtual void SetUIData()
    {
        // ����icon
        if (!string.IsNullOrEmpty(m_SkillCfg.icon))
        {
            imgIcon.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(m_SkillCfg.icon);
        }

        // Ʒ����
        if (m_HeroInfoCfg != null)
        {
            imgQuality.sprite = commontool.GetQualityBarIcon(GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(m_HeroInfoCfg.heroid).quality);
        }

        // Ԫ��icon
        if (m_SkillCfg.skillelement > 0)
        {
            // ����ʾ
            imgElementIcon.sprite = commontool.GetYuansuIcon(m_SkillCfg.skillelement);
        }
        imgElementIcon.transform.parent.gameObject.SetActive(m_SkillCfg.skillelement != 0);

        // ��������
        Sprite skillTypeDesIcon = commontool.GetSkillTypeDesIcon(m_SkillCfg.skilltype);
        imgSkillTypeDes.sprite = skillTypeDesIcon;
        imgSkillTypeDes.gameObject.SetActive(skillTypeDesIcon != null);

        // ������������
        desc2.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(m_SkillCfg.note1));
        // cd

        // ����
        expend.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_fight_icon_suanli_xiao01");
        txExpend.GetComponent<TextMeshProUGUI>().text = m_SkillCfg.energycost.ToString();

        // selected


    }

    
}
