using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
/// <summary>
/// 战斗界面-怪物信息部分
/// </summary>
public class basebattle_monsterInfo
{
    private List<long> allMonster;

    //public List<Button> infoBtns;

    //public List<GameObject> infoLists;

    private List<MonsterDataCfg> normalMonster;//普通小怪

    private List<MonsterDataCfg> eliteMonster;//精英小怪

    private List<MonsterDataCfg> boosMonster;//boss

    private int curTag;

    public int monsterCount;

    public GameObject monsterInfoUIObj;
    private Transform root;
    private RectTransform rootrt;
    private Transform btnsRoot;
    private List<Button> btns = new List<Button>();
    private Transform listsRoot;
    private Transform normalMonsterRoot;
    private VerticalLayoutGroup normalMonsterTV;
    private Transform eliteMonsterRoot;
    private VerticalLayoutGroup eliteMonsterTV;
    private GameObject monsterItem;

    private Transform bossInfoRoot;
    private GameObject bossList;
    private GameObject bossItem;
    private TMP_Text txBossName;
    private TMP_Text txBossDes;
    private TMP_Text txSkillTitle;
    private Transform skillList;
    private GameObject bossSkillItem;

    private List<MonsterItem> normalItemList = new List<MonsterItem>();
    private List<MonsterItem> eliteItemList = new List<MonsterItem>();
    private List<BossItem> bossItemList = new List<BossItem>();

    private Button btnMask;

    public basebattle_monsterInfo(GameObject monsterInfoUIObj)
    {
        btns.Clear();
        this.monsterInfoUIObj = monsterInfoUIObj;
        root = monsterInfoUIObj.transform.Find("root");
        rootrt = root.GetComponent<RectTransform>();
        btnsRoot = root.transform.Find("btns");
        btnMask = monsterInfoUIObj.GetComponent<Button>();
        for (int i = 0; i < btnsRoot.childCount; i++)
        {
            btns.Add(btnsRoot.GetChild(i).GetComponent<Button>());
        }

        listsRoot = root.transform.Find("lists");
        normalMonsterRoot = listsRoot.transform.FindHideInChild("scroll_1");
        normalMonsterTV = normalMonsterRoot.transform.Find("view/conten_1").GetComponent<VerticalLayoutGroup>();
        eliteMonsterRoot = listsRoot.transform.FindHideInChild("scroll_2");
        eliteMonsterTV = eliteMonsterRoot.transform.Find("view/conten_1").GetComponent<VerticalLayoutGroup>();

        monsterItem = root.transform.FindHideInChild("infoItem").gameObject;

        bossInfoRoot = root.transform.FindHideInChild("bossInfo");
        bossList = bossInfoRoot.transform.Find("bossList").gameObject;
        bossItem = bossList.transform.Find("content").transform.FindHideInChild("bossItem").gameObject;
        txBossName = bossInfoRoot.transform.Find("txBossName").GetComponent<TMP_Text>();
        txBossDes = bossInfoRoot.transform.Find("txBossDes").GetComponent<TMP_Text>();
        txSkillTitle = bossInfoRoot.transform.Find("txSkillTitle").GetComponent<TMP_Text>();
        skillList = bossInfoRoot.transform.Find("skillList");
        bossSkillItem = skillList.transform.Find("content").FindHideInChild("item").gameObject;

        btns[0].AddListenerBeforeClear(() => {
            SwitchTag(0);
        });
        btns[1].AddListenerBeforeClear(() => {
            SwitchTag(1);
        });

        btns[2].AddListenerBeforeClear(() => {
            SwitchTag(2);
        });

        btnMask.AddListenerBeforeClear(() => {
            Hide();
        });

        SetData();
    }

    public void Display()
    {
        btnMask.enabled = true;
        btnMask.gameObject.SetActive(true);
        rootrt.DOKill();
        rootrt.DOAnchorPosX(0, 0.3f);
        Clear();
        SetData();
    }

    public void Hide()
    {
        btnMask.enabled = false;
        rootrt.DOKill();
        rootrt.DOAnchorPosX(710, 0.3f).OnComplete(() => { monsterInfoUIObj.SetActive(false); });
    }

    public void SetData()
    {
        GetAllMonster();
        ClassifyMonster();

        // 普通怪
        CreateNormalMonster();
        // 高级怪
        CreateEliteMonster();
        // boss
        CreateBoss();
        //EventManager.RegisterEvent(EventName.battle_monsterDie, () => {
        //    monsterNumber.SetTextExt((--monsterCount).ToString());
        //}, true);

        GameCenter.mIns.m_CoroutineMgr.DelayNextFrame(() =>
        {
            for (int i = 0; i < normalItemList.Count; i++)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(normalItemList[i].obj.GetComponent<RectTransform>());
            }
            for (int i = 0; i < eliteItemList.Count; i++)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(eliteItemList[i].obj.GetComponent<RectTransform>());
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(normalMonsterTV.gameObject.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(eliteMonsterTV.gameObject.GetComponent<RectTransform>());

        });

        for (int i = 0; i<= btns.Count - 1; i++)
        {
            string btnName = i == 0 ? "小怪" : (i == 1 ? "精英" : "BOSS");
            btns[i].transform.FindHideInChild("text").GetComponent<TMP_Text>().SetText(btnName);
            if (i == 0)
                btns[i].gameObject.SetActive(normalMonster.Count > 0);
            else if (i == 1)
                btns[i].gameObject.SetActive(eliteMonster.Count > 0);
            else if (i == 2)
                btns[i].gameObject.SetActive(boosMonster.Count > 0);
        }

        if (normalItemList.Count > 0)
        {
            curTag = 0;
            SwitchTag(curTag);
        }
        else if (eliteItemList.Count > 0)
        {
            curTag = 1;
            SwitchTag(curTag);
        }
        else if (bossItemList.Count > 0)
        {
            curTag = 2;
            SwitchTag(curTag);
        }
    }


    public void Clear()
    {
        normalMonster.Clear();
        eliteMonster.Clear();
        boosMonster.Clear();

        foreach (var item in normalItemList)
        {
            item.Destroy();
        }
        normalItemList.Clear();
        foreach (var item in eliteItemList)
        {
            item.Destroy();
        }
        eliteItemList.Clear();
        foreach (var item in bossItemList)
        {

            item.Destroy();
        }
        bossItemList.Clear();
    }

    private void SwitchTag(int index)
    {
        curTag = index;
        normalMonsterRoot.gameObject.SetActive(index == 0);
        eliteMonsterRoot.gameObject.SetActive(index == 1);
        bossInfoRoot.gameObject.SetActive(index == 2);
        foreach (var item in btns)
        {
            item.transform.FindHideInChild("select").gameObject.SetActive(item.gameObject.name.Contains((index + 1).ToString()));
        }
        if (index == 2 && bossItemList.Count > 0)
        {
            bossItemList[0].OnClick();
        }
    }

    private void CreateNormalMonster()
    {
        foreach (var item in normalItemList)
        {
            item.Destroy();
        }
        normalItemList.Clear();

        foreach (var mcfg in normalMonster)
        {
            GameObject o = GameObject.Instantiate(monsterItem);
            o.name = mcfg.monsterid.ToString();
            o.transform.parent = normalMonsterTV.transform;
            o.transform.localPosition = Vector3.zero;
            o.transform.localRotation = Quaternion.identity;
            o.transform.localScale = Vector3.one;
            MonsterItem _monsterItem = new MonsterItem(o, mcfg);
            normalItemList.Add(_monsterItem);
            o.SetActive(true);
        }
    }

    private void CreateEliteMonster()
    {
        foreach (var item in eliteItemList)
        {
            item.Destroy();
        }
        eliteItemList.Clear();

        foreach (var mcfg in eliteMonster)
        {
            GameObject o = GameObject.Instantiate(monsterItem);
            o.name = mcfg.monsterid.ToString();
            o.transform.parent = eliteMonsterTV.transform;
            o.transform.localPosition = Vector3.zero;
            o.transform.localRotation = Quaternion.identity;
            o.transform.localScale = Vector3.one;
            MonsterItem _monsterItem = new MonsterItem(o, mcfg);
            eliteItemList.Add(_monsterItem);
            o.SetActive(true);
        }
    }

    private void CreateBoss()
    {
        foreach (var item in bossItemList)
        {
            item.Destroy();
        }
        bossItemList.Clear();

        foreach (var mcfg in boosMonster)
        {
            GameObject o = GameObject.Instantiate(bossItem);
            o.name = mcfg.monsterid.ToString();
            o.transform.parent = bossList.transform.GetChild(0);
            o.transform.localPosition = Vector3.zero;
            o.transform.localRotation = Quaternion.identity;
            o.transform.localScale = Vector3.one;
            BossItem _bossItem = new BossItem(o, mcfg, (bi) =>
            {
                RefreshBossInfo(bi);
                foreach (var _bi in bossItemList)
                {
                    _bi.OnSelected(_bi.cfg.monsterid == bi.cfg.monsterid);
                }
            });
            bossItemList.Add(_bossItem);
            o.SetActive(true);
        }
    }

    private class BossSkillItem
    {
        public long skillId;
        public GameObject o;
        public Image icon;
        public TMP_Text txDes;
        public bool istalent;
        public BossSkillItem(GameObject o, long skillId, bool istalent = false)
        {
            this.o = o;
            this.skillId = skillId;
            this.istalent = istalent;
            icon = o.transform.Find("head/icon").GetComponent<Image>();
            txDes = o.transform.Find("txDes").GetComponent<TMP_Text>();

            // Debug.LogError("~~~~~~~skillId: " + skillId);
            // Debug.LogError("~~~~~~~istalent: " + istalent);
            if (istalent)
            {
                BattleSkillTalentCfg tcfg = BattleCfgManager.Instance.GetTalentCfg(skillId);
                // Debug.LogError("~~~~~~~tcfg: " + tcfg);
                if (tcfg != null)
                {
                    // TODO: note还未配置，后续使用note
                    txDes.SetText(GameCenter.mIns.m_LanMgr.GetLan(tcfg.name));
                }
            }
            else
            {
                BattleSkillCfg skillCfg = BattleCfgManager.Instance.GetSkillCfgBySkillID(skillId);
                if (skillCfg != null)
                {
                    // TODO: note还未配置，后续使用note
                    txDes.SetText(GameCenter.mIns.m_LanMgr.GetLan(skillCfg.name));
                }
            }
        
        }

        public void Destroy()
        {
            if (o != null)
            {
                GameObject.Destroy(o);
            }
        }
    }

    private List<BossSkillItem> bossSkillItemList = new List<BossSkillItem>();
    private void RefreshBossInfo(BossItem bossItem)
    {
        /*
           private TMP_Text txBossName;
    private TMP_Text txBossDes;
    private TMP_Text txSkillTitle;
    private Transform skillList;
    private GameObject bossSkillItem;
         */

        txBossName.SetText(GameCenter.mIns.m_LanMgr.GetLan(bossItem.cfg.name));
        txBossDes.SetText(GameCenter.mIns.m_LanMgr.GetLan(bossItem.cfg.note));
        txSkillTitle.SetText("技能介绍");

        txBossName.SetText(GameCenter.mIns.m_LanMgr.GetLan(bossItem.cfg.name));

        foreach (var item in bossSkillItemList)
        {
            item.Destroy();
        }
        bossSkillItemList.Clear();

        string[] skills = bossItem.cfg.showskill.Split('|');
        foreach (string skill in skills)
        {
            GameObject skillObj = GameObject.Instantiate(bossSkillItem);
            skillObj.transform.parent = bossSkillItem.transform.parent;
            skillObj.transform.localPosition = Vector3.zero;
            skillObj.transform.localRotation = Quaternion.identity;
            skillObj.transform.localScale = Vector3.one;
            BossSkillItem _bossSkillItem = new BossSkillItem(skillObj, long.Parse(skill));
            skillObj.SetActive(true);
            bossSkillItemList.Add(_bossSkillItem);
        }

        string[] showtalents = bossItem.cfg.showtalent.Split('|');
        foreach (string tid in showtalents)
        {
            GameObject skillObj = GameObject.Instantiate(bossSkillItem);
            skillObj.transform.parent = bossSkillItem.transform.parent;
            skillObj.transform.localPosition = Vector3.zero;
            skillObj.transform.localRotation = Quaternion.identity;
            skillObj.transform.localScale = Vector3.one;
            BossSkillItem _bossSkillItem = new BossSkillItem(skillObj, long.Parse(tid), true);
            skillObj.SetActive(true);
            bossSkillItemList.Add(_bossSkillItem);
        }
    }

    /// <summary>
    /// 怪物分类
    /// </summary>
    public void ClassifyMonster()
    {
        normalMonster = new List<MonsterDataCfg>();
        eliteMonster = new List<MonsterDataCfg>();
        boosMonster = new List<MonsterDataCfg>();
        for (int i = 0; i < allMonster.Count; i++)
        {
            MonsterDataCfg monsterDataCfg = MonsterDataManager.Instance.GetMonsterCfgByMonsterID(allMonster[i]);
            if (monsterDataCfg.type == 1)
            {
                normalMonster.Add(monsterDataCfg);
            }
            else if (monsterDataCfg.type == 2)
            {
                eliteMonster.Add(monsterDataCfg);
            }
            else if (monsterDataCfg.type == 3)
            {
                boosMonster.Add(monsterDataCfg);
            }
        }
    }

    /// <summary>
    /// 获得本关卡所有怪物
    /// </summary>
    private void GetAllMonster()
    {
        allMonster = new List<long>();
        PathData onePathData;
        MonsterData oneMonsterData;
        monsterCount = 0;
        for (int i = 0; i < GameCenter.mIns.m_BattleMgr.curMissionData.pathDatas.Count; i++)
        {
            onePathData = GameCenter.mIns.m_BattleMgr.curMissionData.pathDatas[i];
            for (int m = 0; m < onePathData.monsterDatas.Count; m++)
            {
                oneMonsterData = onePathData.monsterDatas[m];
                for (int w = 0; w < oneMonsterData.waveDatas.Count; w++)
                {
                    monsterCount += int.Parse(oneMonsterData.waveDatas[w].Count);
                    if (!allMonster.Contains(long.Parse(oneMonsterData.waveDatas[w].MonsterID)))
                    {
                        allMonster.Add(long.Parse(oneMonsterData.waveDatas[w].MonsterID));
                    }
                }
            }
        }
    }

    private class MonsterItem
    {
        public GameObject obj;
        public Image icon;
        public Image iconSkill;
        public TMP_Text name;
        public TMP_Text desc1;
        public TMP_Text desc2;

        public MonsterDataCfg cfg;

        public GameObject skillitem;
        public List<GameObject> skillItems = new List<GameObject>();

        public MonsterItem(GameObject obj, MonsterDataCfg cfg)
        {
            this.obj = obj;
            this.cfg = cfg;
            icon = obj.transform.Find("head/icon").GetComponent<Image>();
            iconSkill = obj.transform.Find("desc/iconSkill").GetComponent<Image>();
            name = obj.transform.Find("desc/name").GetComponent<TMP_Text>();
            desc1 = obj.transform.Find("desc/desc1").GetComponent<TMP_Text>();
            desc2 = obj.transform.Find("desc/desc2").GetComponent<TMP_Text>();
            skillitem = obj.transform.Find("desc/content").transform.FindHideInChild("item").gameObject;

            icon.sprite = SpriteManager.Instance.GetSpriteSync(cfg.icon);
            // iconSkill.sprite = cfg.skill;
            name.SetText(GameCenter.mIns.m_LanMgr.GetLan(cfg.name));
            desc1.SetText(GameCenter.mIns.m_LanMgr.GetLan(cfg.note));

            if (skillItems != null)
            {
                foreach (var item in skillItems)
                {
                    if (item != null)
                    {
                        GameObject.Destroy(item);
                    }
                }
            }
            skillItems.Clear();

            // 技能的
            if (!string.IsNullOrEmpty(cfg.showskill))
            {
                string[] skillIds = cfg.showskill.Split('|');
                foreach (var sid in skillIds)
                {
                    BattleSkillCfg battleSkillCfg = BattleCfgManager.Instance.GetSkillCfgBySkillID(long.Parse(sid));
                    if (battleSkillCfg != null)
                    {
                        GameObject o = skillitem.CloneSelf(skillitem.transform.parent, sid);
                        o.transform.Find("txDes").GetComponent<TMP_Text>().SetText(GameCenter.mIns.m_LanMgr.GetLan(battleSkillCfg.note));
                        skillItems.Add(o);
                        o.SetActive(true);
                    }
                }
            }
            if (!string.IsNullOrEmpty(cfg.showtalent))
            {
                string[] tIds = cfg.showtalent.Split('|');
                foreach (var tid in tIds)
                {
                    BattleSkillTalentCfg skillTalentCfg = BattleCfgManager.Instance.GetTalentCfg(long.Parse(tid));
                    if (skillTalentCfg != null)
                    {
                        GameObject o = skillitem.CloneSelf(skillitem.transform.parent, tid);
                        o.transform.Find("txDes").GetComponent<TMP_Text>().SetText(GameCenter.mIns.m_LanMgr.GetLan(skillTalentCfg.note));
                        skillItems.Add(o);
                        o.SetActive(true);
                    }
                }
            }
        }

        public void Destroy()
        {
            if (obj != null)
            {
                GameObject.Destroy(obj);
            }
        }
    }


    private class BossItem
    {
        public GameObject obj;
        public Image icon;
        public GameObject selected;
        public Button click;

        public MonsterDataCfg cfg;
        public Action<BossItem> clickCallback;
        public BossItem(GameObject obj, MonsterDataCfg cfg, Action<BossItem> clickCallback)
        {
            this.obj = obj;
            this.cfg = cfg;
            this.clickCallback = clickCallback;
            icon = obj.transform.Find("icon").GetComponent<Image>();
            selected = obj.transform.FindHideInChild("selected").gameObject;
            click = obj.GetComponent<Button>();
            click.AddListenerBeforeClear(() => {
                clickCallback?.Invoke(this);
            });
            icon.sprite = SpriteManager.Instance.GetSpriteSync(cfg.icon);
        }

        public void Destroy()
        {
            if (obj != null)
            {
                GameObject.Destroy(obj);
            }
        }

        public void OnClick()
        {
            clickCallback?.Invoke(this);
        }

        public void OnSelected(bool bSelected)
        {

            selected.SetActive(bSelected);
        }
    }


}

