using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using LitJson;
using UnityEngine.Rendering;

public partial class HeroGrow
{
    public override string uiAtlasName => "grow";


    private HeroLeftMenu leftMenu = null;
    private float uiDuration = 0.2f;

    public HeroData hero;
    public HeroInfoCfgData heroInfo;

    private HeroGrowPanel heroGrowPanel = null;
    private HeroSkillPanel skillPanel = null;
    private HeroWeaponPanel weaponPanel = null;
    private HeroTalentPanel talentPanel = null;
    private HeroProfilePanel profilePanel = null;

    private GameObject heroSenceRoot;//角色3d展示节点
    private GameObject main_fire_01;//火属性节点
    private GameObject main_mine_01;//雷属性节点
    private GameObject main_water_01;//水属性节点
    private GameObject main_wind_01;//风属性节点

    private Dictionary<long, GameObject> heroPrefabList = new Dictionary<long, GameObject>();
    private GameObject curHeroPrefab;
    private AnimatorEventDataCfg curHeroAnimatorCfg;
    private TabType curTab;

    private GameObject heroScenceCamera;

    private float lastValue = 0;
    private float laseRotate = 0;

    //切换英雄展示 模型/立绘
    private Button btn_switchShow;
    //当前展示类型 1-模型 2-立绘
    private int curShowState = 1;
    //英雄立绘
    private Image heroIcon;
    private GameObject shadow;//阴影
    private List<int> delayIDlist;
    private Dictionary<string, GameObject> effectPool;

    private GameObject dragRoot;
    private bool canDrag;

    protected override void OnInit()
    {
        effectPool = new Dictionary<string, GameObject>();
    }

    public override void UpdateWin()
    {
        if (curShowState == 1 && !bPlay && curHeroPrefab != null && curHeroAnimatorCfg != null) 
        {
            aniTimer += Time.deltaTime;
            if (aniTimer >= 2 && bfirst)//第一次进入 两秒后播放idle
            {
                ShowIdle();
                bPlay = true;//进入动画播放状态
                bfirst = false;

                aniTimer = 0;//计时器归零

                //idle动画播放完成后，还原动画播放状态，继续播放loop
                delayIDlist.Add(GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke((int)curHeroAnimatorCfg.showidle.actname[0].anilen, () =>
                {
                    curHeroPrefab.GetComponent<Animator>().Play(curHeroAnimatorCfg.showloopidle.actname[0].name);
                    bPlay = false;
                }));
            }
            else if (aniTimer >= 6)//重复进入 6秒播放一次idle
            {
                ShowIdle();
                bPlay = true;//进入动画播放状态
                aniTimer = 0;//计时器归零

                delayIDlist.Add(GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke((int)curHeroAnimatorCfg.showidle.actname[0].anilen, () =>
                {
                    curHeroPrefab.GetComponent<Animator>().Play(curHeroAnimatorCfg.showloopidle.actname[0].name);
                    bPlay = false;
                }));
            }
        }
    }

    protected override async void OnOpen()
    {
        base.OnOpen();

        dragRoot = _Root.transform.Find("dragRoot").gameObject;
        dragRoot.GetOrAddCompoonet<DragRoot>().onDragCB = () =>
        {
            if (curTab == TabType.Info && curShowState == 1)
            {
                if (lastValue == 0)
                {
                    lastValue = touchcstool.onefingerpos.x;
                    return;
                }
                laseRotate = heroSenceRoot.transform.rotation.eulerAngles.y;
                if (Math.Abs(touchcstool.onefingerpos.x - lastValue) > 50)
                {
                    canDrag = true;
                }
                if (canDrag)
                {
                    if (touchcstool.onefingerpos.x < lastValue)
                    {
                        heroSenceRoot.transform.rotation = Quaternion.Euler(0, laseRotate + (Time.deltaTime * 200f), 0);
                    }
                    else if (touchcstool.onefingerpos.x > lastValue)
                    {
                        heroSenceRoot.transform.rotation = Quaternion.Euler(0, laseRotate + (Time.deltaTime * 200f) * -1, 0);
                    }
                    lastValue = touchcstool.onefingerpos.x;
                }

            }
        };

        dragRoot.GetOrAddCompoonet<DragRoot>().onEndCB = () =>
        {
            lastValue = 0;
            canDrag = false;
        };

        heroIcon = _Root.transform.Find("heroIcon").GetComponent<Image>();
        btn_switchShow = heroLeft.Find("switch").GetComponent<Button>();
        shadow = _Root.transform.Find("shadow").gameObject;
        btn_switchShow.AddListenerBeforeClear(OnSwitchShowClick);

        if (heroScenceCamera == null)
        {
            heroScenceCamera = GameObject.Find("mainCamera").transform.FindHideInChild("heroScenceCamera").gameObject;
        }
        heroScenceCamera.SetActive(true);
        if (heroSenceRoot == null)
        {
            heroSenceRoot = await ResourcesManager.Instance.LoadPrefabSync("widget", "HeroGrowSecnceRoot");
            heroSenceRoot.transform.localPosition = Vector3.zero;
            main_fire_01 = heroSenceRoot.transform.Find("main_fire_01").gameObject;
            main_mine_01 = heroSenceRoot.transform.Find("main_mine_01").gameObject;
            main_water_01 = heroSenceRoot.transform.Find("main_water_01").gameObject;
            main_wind_01 = heroSenceRoot.transform.Find("main_wind_01").gameObject;


            main_fire_01.transform.Find("vol/Global Volume").GetComponent<Volume>().weight = 0.2f;
            main_mine_01.transform.Find("vol/Global Volume").GetComponent<Volume>().weight = 0.2f;
            main_water_01.transform.Find("vol/Global Volume").GetComponent<Volume>().weight = 0.2f;
            main_wind_01.transform.Find("vol/Global Volume").GetComponent<Volume>().weight = 0.2f;
        }
        else
        {
            heroSenceRoot.SetActive(true);
        }

        GameCenter.mIns.m_UIMgr.AddUpdateWin(this);

        TopResourceBar topResBar = new TopResourceBar(_Root, this, () =>
        {

            switch (HeroGrowUtils.backType)
            {
                case BackType.Info:
                default:
                    this.Close();
                    topResBar = null;
                    return true;
                case BackType.Sj:
                    heroGrowPanel.goInfoBySj();
                    HeroGrowUtils.backType = BackType.Info;
                    return false;
                case BackType.Tp:
                    heroGrowPanel.goInfoByTp();
                    HeroGrowUtils.backType = BackType.Info;
                    return false;
                case BackType.Jn:
                    skillPanel.BackSkillPanelByUpLevel();
                    HeroGrowUtils.backType = BackType.Info;
                    return false;
                case BackType.Wq:
                case BackType.WqSj:
                case BackType.WqTp:
                case BackType.WqXh:
                    weaponPanel.BackWeaponPanelByUpLevel();
                    HeroGrowUtils.backType = BackType.Info;
                    return false;
                case BackType.TF:
                    talentPanel.BackTalentByTalentInfo();
                    HeroGrowUtils.backType = BackType.Info;
                    return false;
            }
        }, GameCenter.mIns.m_LanMgr.GetLan("grow_title"));


        leftMenu = new HeroLeftMenu(heroLeft,uiDuration, (oldtype,newtype) => {
            onSwitchTab(oldtype,newtype);
        }, (HeroData, HeroInfoCfgData) => {
            onSwitchHero(HeroData, HeroInfoCfgData);
        });
        leftMenu.init();

        //showHeroGrow();

    }
    private void onSwitchTab(TabType oldtype,TabType newtype)
    {
      
        if(oldtype!= newtype)
        { 
            switch (oldtype)
            {
                case TabType.Info:
                    heroGrowPanel.close();
                    break;
                case TabType.Skill:
                    skillPanel.close();
                    break;
                case TabType.Talent:
                    talentPanel.close();
                    break;
                case TabType.Weapon:
                    weaponPanel.close();
                    break;
                case TabType.Profile:
                    profilePanel.close(); 
                    break;
            }
        }

       
        switch (newtype)
        {
            case TabType.Info:
                showHeroGrow();
                break;
            case TabType.Skill:
                showHeroSkill();
                break;
            case TabType.Talent:
                showHeroTalent();
                break;
            case TabType.Weapon:
                showHeroWeapon();
                break;
            case TabType.Profile:
                showHeroProfile();
                break;
        }
        curTab = newtype;

        /*btn_switchShow.gameObject.SetActive(newtype == TabType.Info);
        heroIcon.gameObject.SetActive(newtype == TabType.Info && curShowState == 2);
        if (curHeroPrefab != null)
        {
            curHeroPrefab.SetActive(newtype == TabType.Info && curShowState == 1);
        }
        if (newtype == TabType.Info)
        {
            if (curShowState == 1)
            {
                ShowHeroPrefab();
            }
            else
            {
                heroIcon.sprite = SpriteManager.Instance.GetTextureSpriteSync($"heroIcon_fullbody/{heroInfo.picture3}");
            }
        }
        heroSenceRoot.transform.localRotation = Quaternion.Euler(Vector3.zero);*/
        btn_switchShow.gameObject.SetActive(newtype == TabType.Info);
        if (this.heroSenceRoot != null)
            SwitchHeroSenceByElement(heroInfo.element);
        if (curHeroPrefab != null)
        {
            curHeroPrefab.SetActive(newtype == TabType.Info && curShowState == 1);
        }
        heroIcon.gameObject.SetActive(newtype == TabType.Info && curShowState == 2);
        shadow.gameObject.SetActive(newtype == TabType.Info && curShowState == 2);

    }
    private void onSwitchHero(HeroData hero,HeroInfoCfgData heroInfo)
    { 
        this.hero = hero;
        this.heroInfo = heroInfo;
        if (this.heroSenceRoot != null)
        {
            SwitchHeroSenceByElement(heroInfo.element);
        }
    }

    /// <summary>
    /// 根据角色元素切换3d节点//元素属性 1=水 2=火 3=风 4=雷
    /// </summary>
    /// <param name="element"></param>
    private async void SwitchHeroSenceByElement(int element)
    {
        main_water_01.SetActive(element == 1);
        main_fire_01.SetActive(element == 2);
        main_wind_01.SetActive(element == 3);
        main_mine_01.SetActive(element == 4);

        if (curTab == TabType.Info)
        {
            if (curShowState == 1)
            {
                //加载英雄模型
                foreach (var item in heroPrefabList)
                {
#if UNITY_EDITOR
                    if (item.Value == null)
                    {
                        Debug.LogError("角色资源没有加载到====================");
                    }
#endif

                    item.Value?.SetActive(item.Key == hero.heroID);
                }
                ShowHeroPrefab();
            }
            else
            {
                Sprite s = await SpriteManager.Instance.GetTextureSpriteSync($"heroIcon_fullbody/{heroInfo.picture3}");
                heroIcon.sprite = s;
                shadow.transform.Find("icon").GetComponent<Image>().sprite = s;
                heroIcon.gameObject.SetActive(true);

            }
        }
        

        
        // 加载环境配置
        string sceneCfgName = element == 1 ? "UI_main_water" :
                    (element == 2 ? "UI_main_fire" :
                    (element == 3 ? "UI_main_wind" : "UI_main_mine"));
        GameCenter.mIns.m_FogManager.SetSceneSetting(sceneCfgName);
    }

    private async void ShowHeroPrefab()
    {
        if (curHeroPrefab!= null)
        {
            curHeroPrefab.SetActive(false);
        }
        if (curShowState == 1)
        {
            if (!heroPrefabList.ContainsKey(hero.heroID))
            {
                heroPrefabList.Add(hero.heroID, await BattleHeroManager.Instance.LoadHeroModelByHeroID(null, hero.heroID));
            }
            curHeroAnimatorCfg = AnimatorCfgManager.ins.GetAnimatorCfgByObjID(hero.heroID);
            curHeroPrefab = heroPrefabList[hero.heroID];
            if (curHeroPrefab != null) {
                curHeroPrefab.transform.SetParent(heroSenceRoot.transform);
                curHeroPrefab.transform.localRotation = Quaternion.Euler(0, 180, 0);
                curHeroPrefab.transform.localPosition = Vector3.zero;
                lastValue = 0;
                heroSenceRoot.transform.localRotation = Quaternion.Euler(Vector3.zero);
                curHeroPrefab.SetActive(true);
                DoAnimationCB();
            }
        }
    }

    private bool bPlay;
    private float aniTimer;
    private bool bfirst;
    private void DoAnimationCB()
    {
        curHeroPrefab.GetComponent<Animator>().Play(curHeroAnimatorCfg.showloopidle.actname[0].name);
        if (delayIDlist != null)//清理前一个英雄的延时回调
        {
            for (int i = 0; i < delayIDlist.Count; i++)
            {
                GameCenter.mIns.m_CoroutineMgr.StopDelayInvoke(delayIDlist[i]);
            }
        }
        else
        {
            delayIDlist = new List<int>();
        }
        foreach (var item in effectPool)
        {
            if (item.Value!= null)
            {
               item.Value.SetActive(false);
            }
        }
        aniTimer = 0;
        bfirst = true;
        bPlay = false;
    }

    private async void ShowIdle()
    {


        AnimatorEventData eventData = curHeroAnimatorCfg.showidle;
        curHeroPrefab.GetComponent<Animator>().Play(eventData.actname[0].name);
        //特效列表
        if (eventData.effects != null && eventData.effects.Count > 0)
        {
            string effectName;
            for (int i = 0; i < eventData.effects.Count; i++)
            {
                effectName = eventData.effects[i].effabname;
                Transform root = curHeroPrefab.GetComponent<FBXBindBonesHelper>().GetBoneByString(eventData.effects[i].point);
                Vector3 pos = new Vector3((float)eventData.effects[i].pos.x, (float)eventData.effects[i].pos.y, (float)eventData.effects[i].pos.z);
                GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke((int)eventData.effects[i].time, () => {
                    if (effectPool.ContainsKey(effectName))
                    {
                        effectPool[effectName].SetActive(false);
                        effectPool[effectName].transform.SetParent(root);
                        effectPool[effectName].transform.position = pos;
                        effectPool[effectName].transform.localRotation = Quaternion.Euler(Vector3.zero);
                        effectPool[effectName].SetActive(true);
                    }
                    else
                    {
                        ResourcesManager.Instance.LoadAssetAsyncByName($"{effectName}.prefab", (effItem) => {
                            effItem.transform.SetParent(root);
                            effItem.transform.position = pos;
                            effItem.transform.localRotation = Quaternion.Euler(Vector3.zero);
                            effItem.SetActive(true);
                            effectPool.Add(effectName, effItem);
                        });
                    }
                    
                });
            }
        }
       
    }


    /// <summary>
    /// 切换显示 3d/立绘
    /// </summary>
    private async void OnSwitchShowClick()
    {
        curShowState = curShowState == 1 ? 2 : 1;
        btn_switchShow.transform.Find("3d").gameObject.SetActive(curShowState == 1);
        if (curHeroPrefab != null)
        {
            curHeroPrefab.SetActive(curShowState == 1);
        }


        btn_switchShow.transform.Find("icon").gameObject.SetActive(curShowState == 2);
        heroIcon.gameObject.SetActive(curShowState == 2);
        shadow.gameObject.SetActive(curShowState == 2);
        if (curShowState == 2)
        {
            Sprite s = await SpriteManager.Instance.GetTextureSpriteSync($"heroIcon_fullbody/{heroInfo.picture3}");
            heroIcon.sprite = s;
            shadow.transform.Find("icon").GetComponent<Image>().sprite = s;
            heroIcon.gameObject.SetActive(true);
            shadow.SetActive(true);
        }
        else
        {
            foreach (var item in heroPrefabList)
            {
                item.Value.SetActive(item.Key == hero.heroID);
            }
            if (!heroPrefabList.ContainsKey(heroInfo.heroid))
            {
                heroPrefabList.Add(hero.heroID,await BattleHeroManager.Instance.LoadHeroModelByHeroID(null, hero.heroID));
            }
            ShowHeroPrefab();
        }
    }

    private void showHeroGrow()
    {
        if (heroGrowPanel != null)
        {
            heroGrowPanel.OnDestroy();
        }
        heroGrowPanel = new HeroGrowPanel(panelHeroGrow, leftMenu, uiDuration);
        heroGrowPanel.show(true);
        HeroGrowUtils.backType = BackType.Info;
    }

    private void showHeroWeapon()
    {
        if (weaponPanel != null)
        {
            weaponPanel.Refresh(leftMenu);
        }
        else
        {
            weaponPanel = new HeroWeaponPanel(panelHeroWeapon, leftMenu, uiDuration);
            weaponPanel.show();
        }

        HeroGrowUtils.backType = BackType.Info;
    }
    private void showHeroTalent()
    {
        talentPanel = new HeroTalentPanel(panelHeroTalent, leftMenu, uiDuration);
        talentPanel.show();
        HeroGrowUtils.backType = BackType.Info;
    }
    private void showHeroProfile()
    {
        profilePanel = new HeroProfilePanel(panelHeroProfile, leftMenu, uiDuration);
        profilePanel.show();
        HeroGrowUtils.backType = BackType.Info;
    }
    private void showHeroSkill()
    {
        skillPanel = new HeroSkillPanel(panelHeroSkill, leftMenu, uiDuration);
        skillPanel.show();
        HeroGrowUtils.backType = BackType.Info;
    }

    protected override void OnClose()
    {
        heroScenceCamera.SetActive(false);
        foreach (var item in heroPrefabList)
        {
            GameObject.Destroy(item.Value);
        }
        heroPrefabList.Clear();

        foreach (var item in effectPool)
        {
            if (item.Value!= null)
            {
                GameObject.Destroy(item.Value);
            }
        }
        effectPool.Clear();

        base.OnClose();
        if (heroGrowPanel != null)
            heroGrowPanel.OnDestroy();

        ClearUI();
        leftMenu = null;

        if (heroSenceRoot != null)
        {
            heroSenceRoot.SetActive(false);
        }
        MainSceneManager.Instance.OnEnter();
        GameCenter.mIns.m_FogManager.SetSceneSetting("");
    }

    protected override void OnDestroy()
    {
        if (heroSenceRoot != null)
        {
            heroSenceRoot.SetActive(false);
        }
        base.OnDestroy();
    }


    public void ClearUI()
    {
        leftMenu.clearUI();

    }

   
}

