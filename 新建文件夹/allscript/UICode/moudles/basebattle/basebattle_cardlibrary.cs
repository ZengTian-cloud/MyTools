using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using LitJson;
using Managers;

public class basebattle_cardLibrary
{
    private GameObject uiPrefab;
    private Transform root;
    private Image imgMask;
    private TMP_Text txTitle;
    private Button btnClose;

    private Transform nodeLeft;
    private Transform nl_skillEnergyDes;
    private TMP_Text nl_skillEnergyDes_des;
    private TMP_Text nl_skillEnergyDes_value;
    private Transform nl_skillCardDes;
    private TMP_Text nl_skillCardDes_des1;
    private TMP_Text nl_skillCardDes_value1;
    private TMP_Text nl_skillCardDes_des2;
    private TMP_Text nl_skillCardDes_value2;
    private Transform nl_noteDes1;
    private Transform nl_noteDes2;
    private Button nl_btnPrompt;
    private TMP_Text nl_txPrompt;

    private Transform nodeRight;
    private ScrollRect nr_sr;
    private RectTransform nr_content;
    private GameObject nr_rowlist;
    private GameObject nr_item;
    private Scrollbar nr_srBar;
    private Toggle nr_togglePause;
    private TMP_Text nr_toggleTx;

    private List<BaseHero> m_HeroDatas = new List<BaseHero>();
    private MissionCfgData missionCfg;
    private List<CardLibraryItem> m_CardLibraryItems = new List<CardLibraryItem>();

    private GameObject popObj;
    private basebattle_cardLibrary_pop basebattle_cardLibrary_pop;
    public basebattle_cardLibrary(Transform parent, List<long> heroIds, long missonId, basebattle _basebattle, float energyRecoverySpeed)
    {
        if (parent == null)
        {
            return;
        }

        if (uiPrefab != null)
        {
            Display(heroIds, missonId, _basebattle, energyRecoverySpeed);
        }
        else
        {
            loadUiPrefab( parent);
            OnOpen(uiPrefab, heroIds, missonId, _basebattle, energyRecoverySpeed);
        }
    }

    private async void loadUiPrefab(Transform parent)
    {
        uiPrefab = await ResourcesManager.Instance.LoadUIPrefabSync("basebattlecardlibrary", parent, true);
    }

    public void Display(List<long> heroIds, long missonId, basebattle _basebattle, float energyRecoverySpeed)
    {
        Clear();
        this._basebattle = _basebattle;
        this.energyRecoverySpeed = energyRecoverySpeed;
        uiPrefab.SetActive(true);
        SetData(heroIds, missonId);
    }

    public void OnClose()
    {
        if (uiPrefab != null)
        {
            uiPrefab.SetActive(false);
        }
    }

    public void OnDestroy()
    {
        if (uiPrefab != null)
        {
            GameObject.Destroy(uiPrefab);
        }
    }

    private basebattle _basebattle;
    private float energyRecoverySpeed;
    public void OnOpen(GameObject go, List<long> heroIds, long missonId, basebattle _basebattle, float energyRecoverySpeed)
    {
        this._basebattle = _basebattle;
        this.energyRecoverySpeed = energyRecoverySpeed;
        Clear();
        root = go.transform.Find("root");
        popObj = go.transform.FindHideInChild("pop").gameObject;
        imgMask = go.transform.Find("imgMask").GetComponent<Image>();

        txTitle = root.transform.Find("txTitle").GetComponent<TMP_Text>();
        btnClose = root.transform.Find("btnClose").GetComponent<Button>();

        nodeLeft = root.transform.Find("nodeLeft");

        nl_skillEnergyDes = nodeLeft.transform.Find("skillEnergyDes");
        nl_skillEnergyDes_des = nl_skillEnergyDes.transform.Find("txTitle").GetComponent<TMP_Text>();
        nl_skillEnergyDes_value = nl_skillEnergyDes.transform.Find("txValue").GetComponent<TMP_Text>();

        nl_skillCardDes = nodeLeft.transform.Find("skillCardDes");
        nl_skillCardDes_des1 = nl_skillCardDes.transform.Find("txTitle1").GetComponent<TMP_Text>();
        nl_skillCardDes_value1 = nl_skillCardDes.transform.Find("txValue1").GetComponent<TMP_Text>();
        nl_skillCardDes_des2 = nl_skillCardDes.transform.Find("txTitle2").GetComponent<TMP_Text>();
        nl_skillCardDes_value2 = nl_skillCardDes.transform.Find("txValue2").GetComponent<TMP_Text>();

        nl_noteDes1 = nodeLeft.transform.Find("noteDes1");
        nl_noteDes2 = nodeLeft.transform.Find("noteDes2");
        nl_btnPrompt = nodeLeft.transform.Find("btnPrompt").GetComponent<Button>();
        nl_txPrompt = nodeLeft.transform.Find("txPrompt").GetComponent<TMP_Text>();

        nodeRight = root.transform.Find("nodeRight");
        nr_sr = nodeRight.transform.Find("sr").GetComponent<ScrollRect>();
        nr_content = nodeRight.transform.Find("sr/content").GetComponent<RectTransform>();
        //nr_rowlist = nr_content.transform.FindHideInChild("rowlist").gameObject;
        nr_item = nr_content.transform.FindHideInChild("item").gameObject;
        nr_srBar = nodeRight.transform.Find("srBar").GetComponent<Scrollbar>();
        nr_togglePause = nodeRight.transform.Find("lookPause/togglePause").GetComponent<Toggle>();
        nr_toggleTx = nr_togglePause.transform.Find("Label").GetComponent<TMP_Text>();

        txTitle.SetText("技能库");
        btnClose.AddListenerBeforeClear(() =>
        {
            OnClose();
            Time.timeScale = 1;
        });

        nl_btnPrompt.AddListenerBeforeClear(() =>
        {
            //root.gameObject.SetActive(false);
            basebattle_cardLibrary_pop = new basebattle_cardLibrary_pop(popObj, 0, () => { root.gameObject.SetActive(true); });
        });

        nl_skillEnergyDes_des.SetText("平均算力消耗");
        nl_skillEnergyDes_value.SetText("0");

        nl_skillCardDes_des1.SetText("当前手牌数");
        nl_skillCardDes_value1.SetText("0");
        nl_skillCardDes_des2.SetText("算力回复速度");
        nl_skillCardDes_value2.SetText("0");

        nl_txPrompt.SetText("每回合抽取技能牌的概率都会动态计算");
        nr_toggleTx.SetText("查阅时暂停游戏");

        new CardLibraryNode(nl_noteDes1.gameObject);
        new CardLibraryNode(nl_noteDes2.gameObject);

        // 打卡暂停?
        if (!LocalDataManager.Instance.HasLocalData("opencardlibrarypause"))
        {
            // 第一次, 默认暂停游戏
            LocalDataManager.Instance.SetLocalData("opencardlibrarypause", "true");
        }

        nr_togglePause.onValueChanged.RemoveAllListeners();
        nr_togglePause.onValueChanged.AddListener((v) =>
        {
            bool currOpenCardLibraryPause = bool.Parse(LocalDataManager.Instance.GetLocalData("opencardlibrarypause").ToString());
            if (currOpenCardLibraryPause != v)
            {
                LocalDataManager.Instance.SetLocalData("opencardlibrarypause", v ? "true" : "false");
            }
        });
        SetData(heroIds, missonId);
    }

    public void SetData(List<long> heroIds, long missonId)
    {
        if (heroIds.Count <= 0)
        {
            return;
        }
        foreach (var id in heroIds)
        {
            m_HeroDatas.Add(BattleHeroManager.Instance.GetBaseHeroByHeroID(id));
        }
        missionCfg = MissionCfgManager.Instance.GetMissionCfgByMissionID(GameCenter.mIns.m_BattleMgr.missionID);
        nr_item.SetActive(false);
        int totalEnergy1 = CreateHeroSkillIcons();
        int totalEnergy2 = CreateMissionIcons();
        string averageEnergy = "";
        if (m_HeroDatas.Count > 0)
        {
            averageEnergy = ((float)totalEnergy1 / (m_HeroDatas.Count * 3)).ToString("#0.0");
        }
        nl_skillEnergyDes_value.SetText(averageEnergy);
        nl_skillCardDes_value1.SetText((_basebattle != null ? _basebattle.CurrCardCount : 0).ToString());
        nl_skillCardDes_value2.SetText(energyRecoverySpeed.ToString() + "/秒");

        nr_srBar.value = 0;
        nr_srBar.size = 0;
        SetSrBarActive(m_CardLibraryItems.Count >= 16);
        nr_content.sizeDelta = new Vector2(nr_content.sizeDelta.x, m_CardLibraryItems.Count >= 16 ? 1230 : 820);

        bool openCardLibraryPause = bool.Parse(LocalDataManager.Instance.GetLocalData("opencardlibrarypause").ToString());
        nr_togglePause.isOn = openCardLibraryPause;
        if (openCardLibraryPause)
        {
            Time.timeScale = 0;
        }

        nl_noteDes2.gameObject.SetActive(m_CardLibraryItems.Count >= 9);
    }

    private void SetSrBarActive(bool bActive)
    {
        Image barImg = nr_srBar.GetComponent<Image>();
        GameObject barHand = nr_srBar.transform.Find("Sliding Area/Handle").gameObject;
        barImg.enabled = bActive;
        barHand.SetActive(bActive);
    }

    private class TempSort
    {
        public long heroId;
        public long skillId;
        public float prop;
    }

    private Dictionary<DrawCardData, float> newProps = new Dictionary<DrawCardData, float>();
    private float GetNewProp(DrawCardData drawCardData)
    {
        foreach (var dcd in newProps)
        {
            if (dcd.Key.uid == drawCardData.uid)
            {
                return dcd.Value;
            }
        }
        return 0;
    }

    private int CreateHeroSkillIcons()
    {
        newProps = DrawCardMgr.Instance.GetNextProp();

        int totalEnergy = 0;
        List<TempSort> tempSorts = new List<TempSort>();
        foreach (var hd in m_HeroDatas)
        {
            TempSort tempSort1 = new TempSort();
            tempSort1.heroId = hd.heroData.heroID;
            tempSort1.skillId = hd.skill1;
            DrawCardData drawCardData1 = DrawCardMgr.Instance.GetDrawCardDataBySkillId(hd.skill1);
            tempSort1.prop = drawCardData1 != null ? GetNewProp(drawCardData1) : 0;
            tempSorts.Add(tempSort1);
            TempSort tempSort2 = new TempSort();
            tempSort2.heroId = hd.heroData.heroID;
            tempSort2.skillId = hd.skill2;
            DrawCardData drawCardData2 = DrawCardMgr.Instance.GetDrawCardDataBySkillId(hd.skill2);
            tempSort2.prop = drawCardData2 != null ? GetNewProp(drawCardData2) : 0;
            tempSorts.Add(tempSort2);
            TempSort tempSort3 = new TempSort();
            tempSort3.heroId = hd.heroData.heroID;
            tempSort3.skillId = hd.skill3;
            DrawCardData drawCardData3 = DrawCardMgr.Instance.GetDrawCardDataBySkillId(hd.skill3);
            tempSort3.prop = drawCardData3 != null ? GetNewProp(drawCardData3) : 0;
            tempSorts.Add(tempSort3);
        }

        // 非英雄卡牌
        List<DrawCardData> notHeroCards = DrawCardMgr.Instance.GetNotHeroCrads();
        if (notHeroCards != null)
        {
            foreach (var nhc in notHeroCards)
            {
                TempSort tempSort1 = new TempSort();
                tempSort1.heroId = nhc.heroid;
                tempSort1.skillId = nhc.skillid;
                tempSort1.prop = nhc != null ? GetNewProp(nhc) : 0;
                tempSorts.Add(tempSort1);
            }
        }

        tempSorts.Sort(new CardLibrarySort());

        foreach (var hd in tempSorts)
        {
            /*
                this.baseSkill = this.cfgdata.baseskill;
                this.skill1 = this.cfgdata.cardskill1;
                this.skill2 = this.cfgdata.cardskill2;
                this.skill3 = this.cfgdata.cardskill3;
             */
            // m_CardLibraryItems.Add(CreateCardLibraryItem(hd.baseSkill, hd.heroData.heroID));
            m_CardLibraryItems.Add(CreateCardLibraryItem(hd.skillId, hd.heroId, hd.prop));
            //m_CardLibraryItems.Add(CreateCardLibraryItem(hd.skill2, hd.heroData.heroID));
            //m_CardLibraryItems.Add(CreateCardLibraryItem(hd.skill3, hd.heroData.heroID));
            totalEnergy += BattleCfgManager.Instance.GetSkillCfgBySkillID(hd.skillId).energycost;
        }
        tempSorts.Clear();
        return totalEnergy;
    }

    private class CardLibrarySort : IComparer<TempSort>
    {
        public int Compare(TempSort a, TempSort b)
        {
            return b.prop.CompareTo(a.prop);
        }
    }

    private int CreateMissionIcons()
    {
        int totalEnergy = 0;
        if (missionCfg != null)
        {
            // todo
        }
        return totalEnergy;
    }

    private CardLibraryItem CreateCardLibraryItem(long skillId, long heroId, float prop)
    {
        GameObject gameObject = GameObject.Instantiate(nr_item);
        gameObject.transform.parent = nr_content.transform;
        gameObject.transform.localScale = Vector3.one;
        CardLibraryItem cardLibraryItem = new CardLibraryItem(gameObject.transform.Find("skillicon").gameObject, skillId, heroId, (battleSkillCfg) =>
        {
            // click
            foreach (var item in m_CardLibraryItems)
            {
                if (item.m_SkillCfg.skillid == battleSkillCfg.skillid)
                {
                    //root.gameObject.SetActive(false);
                    basebattle_cardLibrary_pop = new basebattle_cardLibrary_pop(popObj, 1, () => { root.gameObject.SetActive(true); }, skillId, heroId, item);
                    break;
                }
            }
        });

        cardLibraryItem.SetObj(gameObject, gameObject.transform.Find("notes").gameObject, heroId, skillId, prop);
        return cardLibraryItem;
    }


    private void Clear()
    {
        foreach (var item in m_CardLibraryItems)
        {
            item.Destroy();
        }
        m_CardLibraryItems.Clear();
        m_HeroDatas.Clear();
        missionCfg = null;
    }


}

public class CardLibraryItem : CommonSkillIcon
{
    // 1: 英雄， 2：关卡
    public int cardFromType = 0;

    public GameObject customRoot;
    public GameObject noteObj;
    public float fprobability;
    public string prop;
    public CardLibraryItem(GameObject o, long skillId, long heroId, Action<BattleSkillCfg> cliclCallback) : base(o, skillId, heroId, cliclCallback)
    {
    }
    public CardLibraryItem(GameObject o, long skillId, long heroId, Action<BattleSkillCfg> cliclCallback, bool isInit) : base(o, skillId, heroId, cliclCallback, isInit)
    {
    }


    public void SetObj(GameObject customRoot, GameObject noteObj, long heroId, long skillId, float _prop)
    {
        this.customRoot = customRoot;
        this.noteObj = noteObj;
        customRoot.SetActive(true);

        //DrawCardData drawCardData = DrawCardMgr.Instance.GetDrawCardDataBySkillId(skillId);
        float probability = _prop;//drawCardData != null ? drawCardData.probability : 0;
        fprobability = probability;

        string strProbability;
        DrawCardData cardData = DrawCardMgr.Instance.drawCardDatas.Find(x => x.skillid == skillId);
        //支援状态下卡牌无法抽取
        if (cardData != null && cardData.place == 3)
        {
             strProbability = GameCenter.mIns.m_LanMgr.GetLan("battle_cannotDraw");
        }
        else
        {
             strProbability = (probability * 100).ToString("#0.00") + "%";
        }
        string heroName = "";
        if (heroId > 0)
            heroName = GameCenter.mIns.m_LanMgr.GetLan(BattleHeroManager.Instance.GetBaseHeroByHeroID(heroId).cfgdata.name);
        else
            heroName = GameCenter.mIns.m_LanMgr.GetLan(BattleCfgManager.Instance.GetSkillCfgBySkillID(skillId).name);

        string skillDis = GameCenter.mIns.m_LanMgr.GetLan(BattleCfgManager.Instance.GetSkillCfgBySkillID(skillId).note1);
        new CardLibraryNode(noteObj, strProbability, heroName, skillDis);

        prop = strProbability;
        desc2.gameObject.SetActive(false);
    }

    public void SetCardFromType(int cardFromType = 0)
    {
        this.cardFromType = cardFromType;
    }

    public void Destroy()
    {
        if (customRoot != null)
        {
            GameObject.Destroy(customRoot);
        }
    }
}

public class CardLibraryNode
{
    public CardLibraryNode(GameObject o, string probability = "", string belongHero = "", string skillDes = "")
    {
        o.transform.Find("note1/tx").GetComponent<TMP_Text>().SetText(probability == "" ? "抽卡概率" : probability.ToString());
        o.transform.Find("note2/tx").GetComponent<TMP_Text>().SetText(belongHero == "" ? "所属" : belongHero.ToString());
        o.transform.Find("note3/tx").GetComponent<TMP_Text>().SetText(skillDes == "" ? "技能描述" : skillDes.ToString());
    }
}
public class basebattle_cardLibrary_pop
{
    public GameObject popObj;
    public Button btnMask;
    public Transform popRoot;

    // skillPop
    public GameObject skillPop;
    public GameObject sp_skillIcon;
    public CommonSkillIcon sp_commonSkillIcon;
    public TMP_Text sp_txSkillName;
    public GameObject sp_note1;
    public GameObject sp_note2;
    public GameObject sp_note3;
    public GameObject sp_note4;
    public TMP_Text sp_txDes;
    public GameObject sp_ext;
    public RectTransform sp_nodeContent;
    public RectTransform sp_nodeSkillExt;
    public GameObject sp_ori_extra;
    public GameObject sp_ori_empty;
    private List<ExtraAddItem> extraAddItems = new List<ExtraAddItem>();
    public RectTransform sp_textsrt;

    // desPop
    public GameObject desPop;
    public TMP_Text dp_txTitle;
    public TMP_Text dp_txContent;

    public basebattle_cardLibrary_pop(GameObject popObj, int popType, Action closeCallback, long skillId = 0, long heroId = 0, CardLibraryItem cardLibraryItem = null)
    {
        this.popObj = popObj;
        btnMask = popObj.transform.Find("imgMask").GetComponent<Button>();
        popRoot = popObj.transform.Find("popRoot").transform;

        skillPop = popRoot.transform.FindHideInChild("skillRoot").gameObject;
        sp_skillIcon = skillPop.transform.Find("skillicon").gameObject;
        sp_txSkillName = skillPop.transform.Find("txSkillName").GetComponent<TMP_Text>();
        sp_note1 = skillPop.transform.Find("note1").gameObject;
        sp_note2 = skillPop.transform.Find("note2").gameObject;
        sp_note3 = skillPop.transform.Find("note3").gameObject;
        sp_note4 = skillPop.transform.Find("note4").gameObject;
        sp_textsrt = skillPop.transform.Find("texts").GetComponent<RectTransform>();
        sp_txDes = skillPop.transform.Find("texts/txDes").GetComponent<TMP_Text>();
        sp_ext = skillPop.transform.Find("texts/ext").gameObject;
        sp_nodeContent = skillPop.transform.Find("texts/ext/nodeContent").GetComponent<RectTransform>();
        sp_nodeSkillExt = skillPop.transform.Find("texts/ext/nodeContent/nodeSkillExt").GetComponent<RectTransform>();
        sp_ori_extra = sp_nodeSkillExt.transform.FindHideInChild("ori_extra").gameObject;
        sp_ori_empty = sp_nodeSkillExt.transform.FindHideInChild("empty").gameObject;

        desPop = popRoot.transform.FindHideInChild("desRoot").gameObject;
        dp_txTitle = desPop.transform.Find("txTitle").GetComponent<TMP_Text>();
        dp_txContent = desPop.transform.Find("txContent").GetComponent<TMP_Text>();

        SetData(popType, skillId, heroId, cardLibraryItem);
        popObj.SetActive(true);

        btnMask.AddListenerBeforeClear(() =>
        {
            foreach (var item in extraAddItems)
            {
                item.Destroy();
            }
            extraAddItems.Clear();


            popObj.SetActive(false);
            closeCallback?.Invoke();
        });
    }

    public void SetData(int popType, long skillId = 0, long heroId = 0, CardLibraryItem cardLibraryItem = null)
    {
        if (popType == 1)
        {
            sp_textsrt.GetComponent<CanvasGroup>().alpha = 0;
            desPop.SetActive(false);
            BattleSkillCfg SkillCfg = BattleCfgManager.Instance.GetSkillCfgBySkillID(skillId);
            // skill pop
            sp_commonSkillIcon = new CommonSkillIcon(sp_skillIcon, skillId, heroId, (skillCfg) => { });
            sp_txSkillName.SetText(GameCenter.mIns.m_LanMgr.GetLan(SkillCfg.name));
            // node1 技能类型
            sp_note1.transform.Find("txName").GetComponent<TMP_Text>().SetText("技能类型");
            sp_note1.transform.Find("txValue").GetComponent<TMP_Text>().SetText(GameCenter.mIns.m_LanMgr.GetLan(SkillCfg.note1));
            // node2 抽卡概率
            sp_note2.transform.Find("txName").GetComponent<TMP_Text>().SetText("抽卡概率");
            sp_note2.transform.Find("txValue").GetComponent<TMP_Text>().SetText(cardLibraryItem.prop);
            // node3 算力消耗
            sp_note3.transform.Find("txName").GetComponent<TMP_Text>().SetText("算力消耗");
            sp_note3.transform.Find("txValue").GetComponent<TMP_Text>().SetText(SkillCfg.energycost.ToString());
            // node4 技能元素
            sp_note4.transform.Find("txName").GetComponent<TMP_Text>().SetText("技能元素");

            /*
             技能元素 0=无元素 1=水 2=火 3=风 4=雷
             */
            string eleName = SkillCfg.skillelement == 0 ? GameCenter.mIns.m_LanMgr.GetLan("basebattle_1") :
                (SkillCfg.skillelement == 1 ? GameCenter.mIns.m_LanMgr.GetLan("basebattle_2") :
                (SkillCfg.skillelement == 2 ? GameCenter.mIns.m_LanMgr.GetLan("basebattle_3") :
                (SkillCfg.skillelement == 3 ? GameCenter.mIns.m_LanMgr.GetLan("basebattle_4") : GameCenter.mIns.m_LanMgr.GetLan("basebattle_5"))));
            sp_note4.transform.Find("txValue").GetComponent<TMP_Text>().SetText(eleName);
            int lv = 0;
            if (heroId > 0)
                lv = BattleHeroManager.Instance.GetBaseHeroByHeroID(heroId).GetSkilllVBySkillID(skillId);
            sp_txDes.SetText(BattleCfgManager.Instance.GetSkillCfgNoteValue(SkillCfg, lv));

            if (!string.IsNullOrEmpty(SkillCfg.relationskills))
            {
                int extCount = 0;
                string[] idSplit = SkillCfg.relationskills.Split('|');
                extCount = idSplit.Length;
                if (extCount > 0)
                {
                    for (int i = 0; i < idSplit.Length; i++)
                    {
                        GameObject eObj = GameObject.Instantiate(sp_ori_extra);
                        eObj.transform.SetParent(sp_ori_extra.transform.parent);
                        eObj.transform.localPosition = Vector3.zero;
                        eObj.transform.localScale = Vector3.one;
                        eObj.name = "extra_" + i;
                        ExtraAddItem extraAddItem = new ExtraAddItem(eObj, SkillCfg);
                        extraAddItems.Add(extraAddItem);
                        extraAddItem.SetActive(true, long.Parse(idSplit[i]));
                        sp_ori_empty.transform.SetAsLastSibling();
                    }
                }
            }
            skillPop.SetActive(true);

            //LayoutRebuilder.ForceRebuildLayoutImmediate(sp_nodeContent.GetComponent<RectTransform>());
            //LayoutRebuilder.ForceRebuildLayoutImmediate(sp_ext.GetComponent<RectTransform>());
            //LayoutRebuilder.ForceRebuildLayoutImmediate(sp_txDes.GetComponent<RectTransform>());
            //LayoutRebuilder.ForceRebuildLayoutImmediate(sp_nodeContent.GetComponent<RectTransform>());
            //LayoutRebuilder.ForceRebuildLayoutImmediate(sp_textsrt);

            // 延迟再刷一下 --> TODO:渲染逻辑分开后再优化处理
            bool isStop = Time.timeScale == 0;
            if (isStop) Time.timeScale = 1;
            GameCenter.mIns.RunWaitCoroutine(() => {
                LayoutRebuilder.ForceRebuildLayoutImmediate(sp_nodeContent.GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(sp_ext.GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(sp_txDes.GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(sp_nodeContent.GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(sp_textsrt);
                if (isStop) Time.timeScale = 0;
                sp_textsrt.GetComponent<CanvasGroup>().alpha = 1;
            }, 0);

        }
        else
        {
            skillPop.SetActive(false);
            string name = "";
            string des = "";
            JsonData jd = GameCenter.mIns.m_CfgMgr.GetCfg("t_help_info");
            if (jd != null)
            {
                foreach (JsonData onejd in jd)
                {
                    if (onejd["helpid"].ToString() == "1")
                    {
                        name = GameCenter.mIns.m_LanMgr.GetLan(onejd["name"].ToString());
                        des = GameCenter.mIns.m_LanMgr.GetLan(onejd["note"].ToString());
                    }
                }
            }
            // des pop
            dp_txTitle.SetText(name);
            dp_txContent.SetText(des);
            desPop.SetActive(true);
        }
    }
}