using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using System;
using TMPro;
using Cysharp.Threading.Tasks;

public partial class basebattle
{
    public override UILayerType uiLayerType => UILayerType.Normal;
    public override string uiAtlasName => "";

    //记录站位台和英雄的字典
    private Dictionary<GameObject, BaseHero> DicDeployHero;

    //当前手指检测到的点位
    private GameObject CurPoint;

    //英雄卡牌列表
    private List<HeroData> cardHeroDatas;
    public int CurrCardCount
    {
        get
        {
            if (basebattle_skillItemList != null)
            {
                return basebattle_skillItemList.BattleSkillIconsCount;
            }
            return 0;
        }
    }//英雄卡牌列表

    //是否在拖拽ui
    private bool isDragUI;
    //抽卡计时器
    private float drawCardTimer = 0;
    //是否吸附
    public bool isadsorb;

    public int curEnergy = 0;//当前能量
    public int maxEnergy = 10;//最大能量

    private CardInputHandler curSkillCardInput;//当前正在拖拽的技能卡
    private BaseHero curSkillCardHolder;//当前技能卡拥有者

    public Tweener energyDot;

    private Tweener heroListDot;
    // 英雄信息面板
    public basebattle_heroInfo basebattle_heroInfo;
    // 技能信息面板
    public basebattle_skillInfo basebattle_skillInfo;
    // 能量条
    public basebattle_energySlider basebattle_energySlider;
    // 技能列表
    public basebattle_skillItemList basebattle_skillItemList;
    // 怪物面板
    public basebattle_monsterInfo basebattle_monsterInfo;


    // 120f;  1125->200 -- > 720=125  ==> 720~1125：屏幕高度像素没涨1，对应高度涨0.185
    private float fingerYPlaySkill
    {
        get
        {
            return Screen.height < 720 ? 125 : ((Screen.height - 720) * 0.185f + 125);
        }
    }

    private float fingerYSetHero = 120f;

    /// <summary>
    /// 打开战斗ui界面 战斗管理器调用
    /// </summary>
    public void OpenBaseBattle()
    {
        DicDeployHero = new Dictionary<GameObject, BaseHero>();
        InitView();
        //注册ui界面的update函数
        GameCenter.mIns.m_UIMgr.AddUpdateWin(this);
    }

    /// <summary>
    /// 初始化界面
    /// </summary>
    private void InitView()
    {
        SwitchRoot();
        CreatUserHeroList();
        RefreshLeaderHPText();
        basebattle_heroInfo = new basebattle_heroInfo();
        basebattle_skillInfo = new basebattle_skillInfo();
        basebattle_energySlider = new basebattle_energySlider();
        curEnergy = GameCenter.mIns.m_BattleMgr.curMissionParamCfg.initialenergy;
        basebattle_skillItemList = new basebattle_skillItemList(rightDown.GetComponent<CanvasGroup>(), backPanel1, infoPanel, () => {
            basebattle_skillItemList.CurrEnergy = curEnergy;
        }, this);
        basebattle_energySlider.InitEnergySlider(energySlider, maxEnergy, maxEnergy, (newCur) =>
        {
            if ((int)newCur != curEnergy)
            {
                curEnergy = (int)newCur;
            }

        }, curEnergy);


        if (basebattle_monsterInfo != null)
        {
            basebattle_monsterInfo.Clear();
        }
        basebattle_monsterInfo = new basebattle_monsterInfo(monsterInfoPanel);

        monsterNumber.text = basebattle_monsterInfo.monsterCount.ToString();
    }

    /// <summary>
    /// 生成玩家的英雄列表
    /// </summary>
    private void CreatUserHeroList()
    {
        //复制玩家英雄数据到战斗内使用
        List<HeroData> userHeroDatas = HeroDataManager.Instance.GetHeroDataList();
        cardHeroDatas = new List<HeroData>();
        for (int i = 0; i < userHeroDatas.Count; i++)
        {
            //获得配置投放到战斗的英雄
            if (GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(userHeroDatas[i].heroID).open == 1)
            {
                cardHeroDatas.Add(userHeroDatas[i]);
            }
        }
        //英雄卡牌了列表
        cardHeroDatas.Sort(new BattleHeroListSort());
        content.SetDatas(cardHeroDatas.Count, false);
        content.onItemRender = OneItemRender;

        // 推荐人数设置 t_mission_battle.limit字段
        BattleMissionParamCfg battleMissionParamCfg = GameCenter.mIns.m_BattleMgr.curMissionParamCfg;
        int limit = battleMissionParamCfg != null ? battleMissionParamCfg.limit : 0;
        txNeedNum.SetTextExt($"必须上阵");
        txNeedNumVal.SetTextExt($"<size=27><color=#F89808>{limit}</color></size>");
        txNeedNum2.SetTextExt($"人");

        // “推荐等级”功能暂不制作，强制写文本：推荐等级：3
        int suggestlv = 0;
        MissionCfgData missionCfg = MissionCfgManager.Instance.GetMissionCfgByMissionID(GameCenter.mIns.m_BattleMgr.missionID);//ChapterManager.Instance.GetMissionNodeData(GameCenter.mIns.m_BattleMgr.missionID);
        if (missionCfg != null)
            suggestlv = missionCfg.suggestlv;

        txRecommendLv.SetTextExt($"推荐等级：{3}");

        // 标题设置
        missionIdText.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(missionCfg.name3));
        missionName.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(missionCfg.name2));
    }

    /// <summary>
    /// 注册战斗内的事件监听
    /// </summary>
    /// <param name="register"></param>
    protected override void OnRegister(bool register)
    {
        BattleEventManager.RegisterEvent(BattleEventName.battle_removeCardByHeroDie, OnRemoveCardByHeroDie, register);
        BattleEventManager.RegisterEvent(BattleEventName.battle_heroDie, OnHerodie, register);
        BattleEventManager.RegisterEvent(BattleEventName.battle_monsterDie, OnMonsterDie, register);
        BattleEventManager.RegisterEvent(BattleEventName.battle_energyChange, OnEnergyChange, register);
        BattleEventManager.RegisterEvent(BattleEventName.battle_end, OnBattleEnd, register);
    }

    /// <summary>
    /// 战斗结束，此时未退出战斗场景，打开结算界面
    /// </summary>
    private void OnBattleEnd()
    {
        // 重置一些数据
        Time.timeScale = 1;
        BattleSelectedHelper.Instance.Clear();
        //统一关闭技能详情界面
        RefreshSkillInfoPanel(false);
        backPanel.SetActive(false);
        //统一删除技能指示器
        if (skillRange_inputRoot != null)
            GameObject.Destroy(skillRange_inputRoot);

        if (basebattle_skillItemList != null)
            basebattle_skillItemList.RestCardAddObjPosition();
    }

    /// <summary>
    /// 退出战斗场景，关闭结算，返回关卡界面
    /// </summary>
    public void OnBattleExit()
    {
        // 清理ui数据
        curEnergy = 0;
        DicDeployHero.Clear();
        basebattle_heroInfo.Clear();
        basebattle_skillInfo.Clear();
        basebattle_energySlider.Clear();
        basebattle_skillItemList.Clear();

        this.Close();
    }


    /// <summary>
    /// update函数
    /// </summary>
    private GameObject curSelectHeroObj;//当前选中的英雄预支体
    private Vector3 oldPos;
    private GameObject targetRolePointObj;//目标站位台
    private Vector3 shootPos;
    public override void UpdateWin()
    {
#if UNITY_EDITOR
        if (!Input.GetMouseButton(0) && Time.timeScale != 1)
#else
        if (Input.touchCount <= 0 && Time.timeScale != 1)
#endif
        {
            if (screenActive)
            {
                btn_screen_Click();
            }
            //是否打开了卡库界面
            if (basebattle_skillItemList != null && basebattle_skillItemList.IsOpnCardLib())
            {
            }
            else
            {
                Time.timeScale = 1;
            }
        }

        //设置英雄头部点位的显示
#if UNITY_EDITOR
        if (!Input.GetMouseButton(0))
#else
        if (Input.touchCount <= 0)
#endif
        {
            if (basebattle_skillItemList != null)
            {
                basebattle_skillItemList.SetHeroHeadPointerActive(false, Vector3.zero, null);
            }
        }

        // 能量update
        if (basebattle_energySlider != null)
            basebattle_energySlider.DoUpdate();
        // 技能列表update
        if (basebattle_skillItemList != null)
            basebattle_skillItemList.DoUpdate();
        //刷新技能卡cd
        RefreshSkillCardCD();
        //刷新技能卡的状态（算力是否足够）
        RefreshCardSkillState();

        //拖拽已上阵英雄的操作
        if (!isDragUI && GameCenter.mIns.m_BattleMgr.curBattleState == EBattleState.Readly)
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))//开始拖拽
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
#endif
            {
                if (curSelectHeroObj == null)
                {
                    ZCamera.shoottouchray_check(GameCenter.mIns.m_BattleMgr.lookCamer, 1 << LayerMask.NameToLayer("HeroRoot"), (obj) =>
                    {
                        if (screenPanel.activeSelf)
                        {
                            return;
                        }
                        if (obj != null)
                        {
                            curSelectHeroObj = obj;
                            oldPos = obj.transform.position;//记录英雄当前位置
                            if (heroListDot != null)
                            {
                                heroListDot.Kill();
                            }
                            heroListDot = readlyRoot.GetComponent<RectTransform>().DOAnchorPosY(-308, 0.1f).OnComplete(() =>
                            {
                                backPanel.SetActive(true);
                                backPanel.transform.FindHideInChild("text").GetComponent<TMP_Text>().SetText("回收");
                                backPanelCanvasGroup.alpha = 1.0f;
                            });
                            RefreshHeroInfoPanel(true, curSelectHeroObj.GetComponent<HeroController>().baseHero);
                        }
                    });

                }
            }
#if UNITY_EDITOR
            else if (Input.GetMouseButton(0))//拖拽中
#else
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
#endif

            {
                if (curSelectHeroObj != null)
                {
                    RefreshPanelPos();
                    curSelectHeroObj.GetComponent<HeroController>().baseHero.skillRange.SetActive(true);//拖拽时显示技能指示器
                    shootPos = ZCamera.shoottouchray_check(GameCenter.mIns.m_BattleMgr.lookCamer, LayerMask.GetMask(new string[] { "terrain", "trap" }), (obj) =>
                    {
                        RefreshRolePointState(obj, curSelectHeroObj);
                    });
                    if (!isadsorb)
                    {
                        curSelectHeroObj.transform.position = shootPos;
                    }
                    curSelectHeroObj.GetComponent<HeroController>().baseHero.roleObj.transform.LookAt(GameCenter.mIns.m_BattleMgr.startPos[0]);
                    backPanel.transform.Find("select").gameObject.SetActive(touchcstool.onefingerpos.y <= fingerYSetHero);
                    backPanelCanvasGroup.alpha = 1.0f;
                }

            }
#if UNITY_EDITOR
            else if (Input.GetMouseButtonUp(0))//拖拽结束
#else
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
#endif
            {
                if (curSelectHeroObj != null)
                {
                    RefreshHeroInfoPanel(false);
                    backPanel.SetActive(false);
                    if (heroListDot != null)
                    {
                        heroListDot.Kill();
                    }
                    readlyRoot.GetComponent<RectTransform>().DOAnchorPosY(0, 0.1f);
                    curSelectHeroObj.GetComponent<HeroController>().baseHero.skillRange.SetActive(false);//完成后隐藏技能指示器
                    if (touchcstool.onefingerpos.y <= fingerYSetHero)//判断手指区域，回收英雄
                    {
                        var data = DicDeployHero.FirstOrDefault(v => v.Value.prefabObj == curSelectHeroObj);
                        RecycleOneHero(data.Key, data.Value);
                    }
                    else
                    {

                        //检测结束拖拽时鼠标停留的位置
                        targetRolePointObj = ZCamera.shootray_checkobj(GameCenter.mIns.m_BattleMgr.lookCamer, LayerMask.GetMask(new string[] { "terrain", "trap" }), null);
                        //如果是可放置格子
                        if (CurPoint != null && CurPoint.tag == "rolePoint")
                        {
                            //变换坐标到目标点
                            curSelectHeroObj.transform.position = CurPoint.transform.position;
                            //判断目标点是否已有英雄
                            if (DicDeployHero.ContainsKey(targetRolePointObj))
                            {
                                //目标点的角色交换位置
                                DicDeployHero[targetRolePointObj].prefabObj.transform.position = oldPos;
                            }
                            DeployOneHero(CurPoint, DicDeployHero.FirstOrDefault(v => v.Value.prefabObj == curSelectHeroObj).Value);
                        }
                        else
                        {
                            curSelectHeroObj.transform.position = oldPos;
                        }
                    }
                }
                curSelectHeroObj = null;
                CurPoint = null;
                RefreshRolePointState(null, null);
            }
        }



        //游戏处于战斗阶段
        if (GameCenter.mIns.m_BattleMgr.curBattleState == EBattleState.Start)
        {
            //如果手牌未满
            if (CardManager.Instance.curCardList.Count < CardManager.Instance.maxCount)
            {
                drawCardTimer += Time.deltaTime;
                if (drawCardTimer >= CardManager.Instance.drawCardTime)
                {
                    //抽一张卡
                    CardManager.Instance.DrawOneCard();
                    drawCardTimer = 0;
                }
            }
        }
    }

    /// <summary>
    /// 单个英雄卡片刷新回调
    /// </summary>
    /// <param name="item"></param>
    /// <param name="index"></param>
    private async void OneItemRender(GameObject item, int index)
    {

        //玩家英雄数据
        HeroData curDate = cardHeroDatas[index - 1];
        //英雄配置数据
        HeroInfoCfgData heroCfgData = GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(curDate.heroID);
        item.transform.Find("leveldes").GetComponent<Text>().text = "LV.";

        Image lvsp_1 = item.transform.Find("imgLevelValue").GetComponent<Image>();
        Image lvsp_2 = lvsp_1.transform.FindHideInChild("imgLevelValue_1").GetComponent<Image>();
        lvsp_2.gameObject.SetActive(curDate.level >= 10);
        if (curDate.level < 10)
        {
            lvsp_1.sprite = SpriteManager.Instance.GetSpriteSync("lv_" + curDate.level);
        }
        else
        {
            int[] nums = commontool.GetNumberDigits(curDate.level);
            lvsp_1.sprite = SpriteManager.Instance.GetSpriteSync("lv_" + nums[0]);
            lvsp_2.sprite = SpriteManager.Instance.GetSpriteSync("lv_" + nums[1]);
        }

        item.transform.Find("leveldes").GetComponent<Text>().text = "LV.";
        item.transform.Find("name").GetComponent<Text>().text = GameCenter.mIns.m_LanMgr.GetLan(heroCfgData.name);
        item.transform.Find("yuansu/icon").GetComponent<Image>().sprite = curDate.GetYuansuIcon(heroCfgData.element);
        item.transform.Find("yuansu/icon").GetComponent<Image>().SetNativeSize();
        item.transform.Find("zhiye/icon").GetComponent<Image>().sprite = curDate.GetZhiYeIcon(heroCfgData.profession);
        item.transform.Find("zhiye/icon").GetComponent<Image>().SetNativeSize();
        item.transform.Find("heroicon").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(heroCfgData.picture1);

        // 队伍标签
        Transform teamRoot = item.transform.FindHideInChild("team");
        teamRoot.gameObject.SetActive(curDate.teamId > 0);
        if (curDate.teamId > 0)
        {
            teamRoot.FindHideInChild("text").GetComponent<Text>().SetTextExt("队伍");
            //imgTeamValue
            teamRoot.FindHideInChild("imgTeamValue").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(commontool.GetArtNumberString_DuiWu_SpriteName(curDate.teamId, 1));
            SpriteManager.Instance.DebugAtlas();
        }

        // 英雄品质条
        // 资源为: 品质3=ui_fight_pnl_renwu_lan, 品质4=ui_fight_pnl_renwu_zi, 品质5=ui_fight_pnl_renwu_cheng, 品质6=ui_fight_pnl_renwu_hong
        string qualityResName = "";
        switch (heroCfgData.quality)
        {
            case 3: qualityResName = "ui_fight_pnl_renwu_lan"; break;
            case 4: qualityResName = "ui_fight_pnl_renwu_zi"; break;
            case 5: qualityResName = "ui_fight_pnl_renwu_cheng"; break;
            case 6: qualityResName = "ui_fight_pnl_renwu_hong"; break;
        }
        item.transform.Find("quality").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(qualityResName);

        // 星级
        BaseHero baseHero = null;
        for (int i = 0; i < 6; i++)
        {
            item.transform.Find($"star/star_{i + 1}/active").gameObject.SetActive(curDate.star >= i + 1);
        }


        //添加拖拽检测脚本
        CardInputHandler cardInput = item.GetOrAddCompoonet<CardInputHandler>();
        cardInput.isHero = true;
        cardInput.heroId = curDate.heroID;
        cardInput.rootScroll = roleList;
        cardInput.dradDir = 2;
        cardInput.uiPrefab = item;
        cardInput.enabled = true;

        cardInput.onPointDownCB = () =>
        {//按下回调
            RefreshPanelPos();
            RefreshHeroInfoPanel(true, null, curDate.heroID);
        };

        cardInput.onPointUpCB = () =>
        {//抬起回调
            RefreshHeroInfoPanel(false);
        };

        cardInput.onBeginCB = async () =>
        {//开始拖拽
            if (screenPanel.activeSelf)
            {
                screenPanel.SetActive(false);
                return;
            }
            isDragUI = true;
            //ui-英雄列表 向下隐藏
            if (heroListDot != null)
            {
                heroListDot.Kill();
            }
            heroListDot = readlyRoot.GetComponent<RectTransform>().DOAnchorPosY(-508, 0.3f);
            baseHero = await BattleHeroManager.Instance.CreatOneHero(curDate, GameCenter.mIns.m_BattleMgr.heroListRoot, heroCfgData);
            if (baseHero != null && !baseHero.bRecycle)
            {
                baseHero.roleObj.transform.LookAt(GameCenter.mIns.m_BattleMgr.startPos[0]);
                //加载攻击范围指示器
                GameObject skillZhishi = await LoadBaseAtkRange(baseHero.baseSkillCfgData, baseHero.prefabObj);
                baseHero.skillRange = skillZhishi;
                RefreshHeroInfoPanel(true, baseHero);
                backPanel.SetActive(true);
                backPanel.transform.FindHideInChild("text").GetComponent<TMP_Text>().SetText("回收");
                backPanelCanvasGroup.alpha = 1.0f;

                // 添加辅助脚本
                baseHero.roleObj.GetOrAddCompoonet<ModelRenderQueueHelper>();
            }

            DisplayPlatformEffect(1);
        };

        cardInput.onDragCB = () =>
        {//拖拽中
            if (baseHero != null && !baseHero.bRecycle)
            {
     
                RefreshPanelPos();
                //获得点击实时位置
                Vector3 shiliPos = ZCamera.shoottouchray_check(GameCenter.mIns.m_BattleMgr.lookCamer, LayerMask.GetMask(new string[] { "terrain", "trap" }), (obj) =>
                {
                    RefreshRolePointState(obj, baseHero.prefabObj);
                });
                if (!isadsorb)
                {
                    baseHero.prefabObj.transform.position = shiliPos;
                }

                baseHero.roleObj.transform.LookAt(GameCenter.mIns.m_BattleMgr.startPos[0]);
                if (baseHero.skillRange)
                {
                    baseHero.skillRange.SetActive(true);
                }
                backPanel.transform.Find("select").gameObject.SetActive(touchcstool.onefingerpos.y <= fingerYSetHero);
                backPanelCanvasGroup.alpha = 1.0f;
            }

        };

        cardInput.onEndCB = () =>//拖拽结束
        {
            if (baseHero != null && !baseHero.bRecycle)
            {
                backPanel.SetActive(false);
                RefreshHeroInfoPanel(false);
                if (baseHero.skillRange)
                {
                    baseHero.skillRange.SetActive(false);
                }
                if (CurPoint != null && CurPoint.tag == "rolePoint")
                {
                    //上阵一个英雄
                    DeployOneHero(CurPoint, baseHero);
                    //设置位置
                    baseHero.prefabObj.transform.position = CurPoint.transform.position;
                    baseHero.roleObj.transform.LookAt(GameCenter.mIns.m_BattleMgr.startPos[0]);
                }
                else
                {
                    BattlePoolManager.Instance.InPool(ERootType.Hero, baseHero.prefabObj, curDate.heroID.ToString());
                    HpSliderManager.ins.OnOneObjDisappear(baseHero);
                }

                if (heroListDot != null)
                {
                    heroListDot.Kill();
                }
                heroListDot = readlyRoot.GetComponent<RectTransform>().DOAnchorPosY(0, 0.1f);
                isDragUI = false;
                CurPoint = null;
                RefreshRolePointState(null, null);
                if (baseHero != null)
                {
                    TalentManager.Instance.CheckTalentBeforBattleByHero(baseHero);
                }
            }
            DisplayPlatformEffect(-1);
        };
    }


    /// <summary>
    /// 英雄死亡事件监听
    /// </summary>
    /// <param name="heroid"></param>
    private void OnHerodie(long heroid)
    {
        if (curSkillCardInput != null)
        {
            curSkillCardInput.onEndCB?.Invoke();
            RefreshSkillInfoPanel(false);
        }
    }

    /// <summary>
    /// 怪物死亡事件监听
    /// </summary>
    private void OnMonsterDie()
    {
        basebattle_monsterInfo.monsterCount -= 1;
        monsterNumber.text = basebattle_monsterInfo.monsterCount.ToString();
    }

    /// <summary>
    /// 算力改变事件监听
    /// </summary>
    /// <param name="value"></param>
    private void OnEnergyChange(long value)
    {
        RefreshEnergy(value, true);
        BattleLog.Log($"算力恢复，数值：{value}");
    }

    /// <summary>
    /// 英雄死亡移除卡牌监听
    /// </summary>
    /// <param name="cardUID"></param>
    private void OnRemoveCardByHeroDie(long cardUID)
    {
        this.basebattle_skillItemList.RemoveSkillIcon(cardUID);
    }

    /// <summary>
    /// 购买卡槽
    /// </summary>
    /// <param name="need"></param>
    public void OnBuyCardSucc(int need)
    {
        // 购买卡槽成功, 更新能量
        RefreshEnergy(need, false);
        //Debug.LogError("OnBuyCardSucc cur:" + curEnergy);

        // 卡槽上限加1, 并立即抽一张卡
        CardManager.Instance.AddCardLimit(1);
    }

   
    protected override void OnClose()
    {
        base.OnClose();
    }

   
    /// <summary>
    /// 播放站位台特效
    /// </summary>
    /// <param name="index">index == 1 == 选中英雄所有站位台特效 || index == 2 == 放置英雄对应站位台特效</param>
    private void DisplayPlatformEffect(int index = -1, GameObject setPlatformObj = null)
    {
       Dictionary<V2,GameObject> platforms = GameCenter.mIns.m_BattleMgr.rolePointObjList;
        if (platforms.Count <= 0)
        {
            return;
        }
        if (index == 1)
        {
            foreach (var item in platforms)
            {
                item.Value.transform.GetChild(index).gameObject.SetActive(true);
            }
        }
        else if (index == 2)
        {
            if (setPlatformObj != null && index < setPlatformObj.transform.childCount)
            {
                GameObject effObj = setPlatformObj.transform.GetChild(index).gameObject;
                effObj.transform.localPosition = Vector3.zero;
                effObj.SetActive(true);
                effObj.transform.DOLocalMoveY(0.1f, 3f).OnComplete(() =>
                {
                    effObj.transform.localPosition = Vector3.zero;
                    effObj.SetActive(false);
                });
            }
        }
        else if (index == -1)
        {
            foreach (var item in platforms)
            {
                if (item.Value != null)
                {
                    item.Value.transform.GetChild(1).gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 加载普通攻击范围指示器
    /// </summary>
    private async UniTask<GameObject> LoadBaseAtkRange(BattleSkillCfg cfg, GameObject parent)
    {
        GameObject indicator = null;
        string[] range = cfg.guiderange.Split(';');
        if (cfg.guidetype == 1)//近战
        {
            indicator = await ResourcesManager.Instance.LoadAssetSyncByName("skillIndicator.prefab");
            indicator.transform.SetParent(parent.transform);
            indicator.transform.localPosition = new Vector3(0, 0.1f, 0);
            float value = float.Parse(range[0]) / 130 * 1.3f * 2f;
            indicator.transform.localScale = new Vector3(value, value, value);
        }
        else if (cfg.guidetype == 2)//单体
        {
            indicator = await ResourcesManager.Instance.LoadAssetSyncByName("skillIndicator.prefab");
            indicator.transform.SetParent(parent.transform);
            indicator.transform.localPosition = new Vector3(0, 0.1f, 0);
            float value_1 = float.Parse(range[0]) / 130;
            float value_2 = float.Parse(range[1]) / 130;
            indicator.transform.localScale = new Vector3(value_2 * 2, value_2 * 2, value_2 * 2);
            indicator.GetComponent<HollowOut2DSpriteHelper>().radius = value_1 / value_2;
        }
        else if (cfg.guidetype == 3)//十字
        {
            indicator = await ResourcesManager.Instance.LoadAssetSyncByName("skillIndicator_1.prefab");
            indicator.transform.SetParent(parent.transform);
            indicator.transform.localPosition = new Vector3(0, 0, 0);
        }
        indicator.SetActive(false);
        return indicator;
    }

    /// <summary>
    /// 刷新角色列表
    /// </summary>
    /// <param name="heroData">英雄数据</param>
    /// <param name="isAdd"></param>
    private void RefreshRoleList(BaseHero baseHero, bool isAdd)
    {
        if (isAdd)
        {
            cardHeroDatas.Add(baseHero.heroData);
        }
        else
        {
            cardHeroDatas.Remove(baseHero.heroData);
        }

        cardHeroDatas.Sort(new BattleHeroListSort());
        content.SetDatas(cardHeroDatas.Count, false);
    }

    /// <summary>
    /// 上阵一个英雄or交换位置
    /// </summary>
    /// <param name="heroPoint">point点位</param>
    /// <param name="baseHero">英雄数据</param>
    private void DeployOneHero(GameObject heroPoint, BaseHero baseHero)
    {
        baseHero.SetRolePoint(GameCenter.mIns.m_BattleMgr.GetRolePointByObj(heroPoint));
        GameObject curPoint;//本次操作角色的目标位置
        DisplayPlatformEffect(2, heroPoint);
        BaseHero targetHero;//目标点位的数据
        if (DicDeployHero.ContainsValue(baseHero))//如果这个角色已上阵
        {
            //获得本次操作角色的原本位置
            curPoint = DicDeployHero.FirstOrDefault(p => p.Value == baseHero).Key;

            //判断目标点位是否有英雄存在
            if (DicDeployHero.ContainsKey(heroPoint))
            {
                //获得目标点位已存在的英雄数据
                targetHero = DicDeployHero[heroPoint];
                //交换位置
                DicDeployHero[heroPoint] = baseHero;
                DicDeployHero[curPoint] = targetHero;
            }
            else//没有英雄
            {
                DicDeployHero.Remove(curPoint);
                DicDeployHero.Add(heroPoint, baseHero);
            }
        }
        else//如果这个本次操作角色没有上阵
        {
            //如果这个点位上有英雄
            if (DicDeployHero.ContainsKey(heroPoint))
            {
                //获得该点位英雄，进入缓存池
                BaseHero baseHero1 = DicDeployHero[heroPoint];
                BattlePoolManager.Instance.InPool(ERootType.Hero, baseHero1.prefabObj, baseHero1.objID.ToString());
                HpSliderManager.ins.OnOneObjDisappear(baseHero1);
                //覆盖该点位英雄
                DicDeployHero[heroPoint] = baseHero;

                RefreshRoleList(baseHero, false);
                RefreshRoleList(baseHero1, true);
            }
            else
            {
                //添加数据
                DicDeployHero.Add(heroPoint, baseHero);
                RefreshRoleList(baseHero, false);
            }
        }
    }

    /// <summary>
    /// 下阵（回收）一个英雄
    /// </summary>
    /// <param name="heroPoint"></param>
    /// <param name="baseHero"></param>
    public void RecycleOneHero(GameObject heroPoint, BaseHero baseHero)
    {
        BattlePoolManager.Instance.InPool(ERootType.Hero, baseHero.prefabObj, baseHero.objID.ToString());
        HpSliderManager.ins.OnOneObjDisappear(baseHero);
        if (DicDeployHero.ContainsValue(baseHero))
        {
            DicDeployHero.Remove(DicDeployHero.FirstOrDefault(v => v.Value == baseHero).Key);
            RefreshRoleList(baseHero, true);
        }
    }

    /// <summary>
    /// 刷新可放置角色格子的状态
    /// </summary>
    private void RefreshRolePointState(GameObject curPoint, GameObject prefab)
    {
        bool isRole = false;
        if (curPoint == null)
        {
            CurPoint = null;
            return;
        }

        if (CurPoint == curPoint)
        {
            prefab.transform.position = CurPoint.transform.position;
            isadsorb = true;
            return;
        }
        foreach (var item in GameCenter.mIns.m_BattleMgr.rolePointObjList)
        {
            if (curPoint == item.Value)
            {
                CurPoint = curPoint;
                prefab.transform.position = CurPoint.transform.position;
                isRole = true;
                isadsorb = true;
            }
            else
            {
                isadsorb = false;
            }

        }
        if (!isRole)
        {
            CurPoint = null;
        }
    }

    /// <summary>
    /// 切换界面root
    /// </summary>
    private void SwitchRoot()
    {
        readlyRoot.SetActive(GameCenter.mIns.m_BattleMgr.curBattleState == EBattleState.Readly);
        battleRoot.SetActive(GameCenter.mIns.m_BattleMgr.curBattleState == EBattleState.Start);

        GameCenter.mIns.m_BattleMgr.lookCamer.gameObject.SetActive(GameCenter.mIns.m_BattleMgr.curBattleState == EBattleState.Readly);
        GameCenter.mIns.m_BattleMgr.battleCamer.gameObject.SetActive(GameCenter.mIns.m_BattleMgr.curBattleState == EBattleState.Start);

        btn_resonance.gameObject.SetActive(GameCenter.mIns.m_BattleMgr.curBattleState == EBattleState.Readly);
        btn_resonance1.gameObject.SetActive(GameCenter.mIns.m_BattleMgr.curBattleState == EBattleState.Start);
        btn_changeSpeed.gameObject.SetActive(GameCenter.mIns.m_BattleMgr.curBattleState == EBattleState.Start);
        btn_pause.gameObject.SetActive(GameCenter.mIns.m_BattleMgr.curBattleState == EBattleState.Start);
    }


    /// <summary>
    /// 刷新主角血量的ui显示
    /// </summary>
    public void RefreshLeaderHPText()
    {
        leaderhp.GetComponentInChildren<TextMeshProUGUI>().text = LeaderManager.ins.GetLeaderCurHP().ToString();
    }

    /// <summary>
    /// 开始战斗按钮
    /// </summary>
    partial void btn_start_Click()
    {
        if (DicDeployHero.Count <= 0)
        {
            GameCenter.mIns.m_UIMgr.PopMsg("未上阵英雄！");
            return;
        }

 

        GameCenter.mIns.m_BattleMgr.StartBattle(this.DicDeployHero);
        SwitchRoot();
        BattleTrapManager.Instance.SetTrapsBoxColliderActive(true);
        RefreshCurCardList();
        RefreshEnergy();

        // 显示技能列表(有可能第二场列表alpha == 0)
        rightDown.GetComponent<CanvasGroup>().alpha = 1;
        // 隐藏技能信息列表
        RefreshSkillInfoPanel(false);
        //infoPanel.SetActive(false);
    }

    /// <summary>
    /// 刷新能源
    /// </summary>
    /// <param name="changevalue">改变值</param>
    /// <param name="isAdd">是否是增值</param>
    private void RefreshEnergy(float changevalue = 0, bool isAdd = false)
    {
        if (basebattle_energySlider != null)
            basebattle_energySlider.RefreshEnergy(curEnergy, maxEnergy, isAdd ? changevalue : -changevalue, isAdd);
    }


    /// <summary>
    /// 刷新当前卡牌列表
    /// </summary>
    private void RefreshCurCardList()
    {
        if (cardList.transform.childCount > 1)
        {
            for (int i = cardList.transform.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(cardList.transform.GetChild(i).gameObject);
            }
        }

        DrawCardData drawCardData;
        GameObject inputRoot = GameObject.CreatePrimitive(PrimitiveType.Plane);
        inputRoot.SetActive(false);
        for (int i = 0; i < CardManager.Instance.curCardList.Count; i++)
        {
            drawCardData = CardManager.Instance.curCardList[i];
            GameObject card = GameObject.Instantiate(cardItem, cardList.transform);
            basebattle_skillItemList.AddSkillIcon(drawCardData, card, drawCardData.skillid, drawCardData.heroid, -1, true);
            drawCardData.item = card;

            CardInputHandler cardInput = card.GetOrAddCompoonet<CardInputHandler>();
            cardInput.isHero = false;
            //cardInput.rootScroll = roleList;
            cardInput.dradDir = 2;
            cardInput.uiPrefab = card;
            cardInput.enabled = true;

            RefreshSkillCardInputCB(cardInput, drawCardData);
        }
    }

    /// <summary>
    /// 添加单张技能卡牌
    /// </summary>
    /// <param name="data"></param>
    public void AddOneSkillCard(DrawCardData data)
    {
        //Debug.LogError("~~ AddOneSkillCard:" + data.uid);
        GameObject carditem = BattlePoolManager.Instance.OutPool(ERootType.SkillCard);
        if (carditem == null)
        {
            carditem = GameObject.Instantiate(cardItem);
            carditem.name = data.skillCfgData.skillid.ToString();
        }

        carditem.transform.SetParent(cardList.transform);
        carditem.transform.localScale = Vector3.one;

        if (basebattle_skillItemList != null)
        {
            basebattle_skillItemList.AddSkillIcon(data, carditem, data.skillid, data.heroid, -1, false);
        }

        data.item = carditem;
        //添加卡片拖拽脚本
        CardInputHandler cardInput = carditem.GetOrAddCompoonet<CardInputHandler>();
        cardInput.isHero = false;
        cardInput.dradDir = 2;
        cardInput.uiPrefab = carditem;
        cardInput.enabled = true;

        RefreshSkillCardInputCB(cardInput, data);


        BaseHero curhero = BattleHeroManager.Instance.GetBaseHeroByHeroID(data.heroid);
        GameObject cd = data.item.transform.Find("bg/cd").gameObject;
        if (curhero != null && curhero.skillDurtion >= 0)
        {
            cd.SetActive(true);
            cd.GetComponent<Image>().fillAmount = curhero.skillDurtion / curhero.skillTotalDurtion;
            data.item.GetComponent<CardInputHandler>().SetSelfLock(true);
            if (curhero.skillDurtion == 0)
            {
                cd.SetActive(false);
                cd.GetComponent<Image>().fillAmount = 1;
                data.item.GetComponent<CardInputHandler>().SetSelfLock(false);

            }
        }
    }

    /// <summary>
    /// 移除一张技能卡牌
    /// </summary>
    public void RemoveOneSkillCard(GameObject carditem, DrawCardData drawCardData)
    {
        CardManager.Instance.RemoveOneCard(drawCardData, carditem);

    }

    /// <summary>
    /// 目标为己方时，添加一个指示器
    /// </summary>
    public async void AddRangeOnSelf(List<BaseObject> targetList)
    {
        if (targetList != null && selectSelfRangeRoot == null)
        {
            selectSelfRangeRoot = new List<GameObject>();
            for (int i = 0; i < targetList.Count; i++)
            {
                if (targetList[i] != null && !targetList[i].bRecycle)
                {
                    GameObject root = await ResourcesManager.Instance.LoadPrefabSync("skillIputRoot.prefab");
                    root.transform.SetParent(targetList[i].prefabObj.transform);
                    root.transform.Find("sprite").GetComponent<SpriteRenderer>().size = new Vector2(1.3f, 1.3f);
                    root.transform.localPosition = new Vector3(0, 0.1f, 0);
                    ModelRenderQueueHelper modelRenderQueueHelper = root.GetOrAddCompoonet<ModelRenderQueueHelper>();
                    if (modelRenderQueueHelper != null)
                    {
                        modelRenderQueueHelper.ToRenderQueue(3020);
                    }
                    selectSelfRangeRoot.Add(root);
                }
            }
        }
    }


    private List<BaseObject> lastTargetList;//上一次索敌列表
    private List<GameObject> removeHightLightList;//需要移除高亮的列表
    private Vector2 tempRecordOneFingerPos = default;

    private List<GameObject> selectSelfRangeRoot;//选中友方时的指示预制体（仅技能目标为友方全体时使用）
    private GameObject skillRange_inputRoot;// 技能范围obj
    /// <summary>
    /// 刷新卡片的拖拽监听回调
    /// </summary>
    private void RefreshSkillCardInputCB(CardInputHandler cardInput, DrawCardData drawCardData)
    {
        GameObject inputRoot = null;
        GameObject skillPoint = null;//技能中心点，跟拖拽范围有偏移
        List<BaseObject> targetList = new List<BaseObject>();
        BaseHero curHero = null;

        cardInput.onPointDownCB = () =>
        {//按下回调
            if (!basebattle_skillItemList.IconCanDrag(drawCardData))
            {
                zxlogger.log("等待其他卡动画完成才能拖动");
                return;
            }
            curHero = SkillManager.ins.GetReleasSkillHeroByID(drawCardData.heroid);
            if (curHero != null && !curHero.bRecycle)
            {
                RefreshPanelPos();
                RefreshSkillInfoPanel(true, drawCardData, curEnergy >= drawCardData.skillCfgData.energycost);

                if (basebattle_skillItemList != null)
                {
                    basebattle_skillItemList.OnPointerDown(drawCardData, curHero);
                }
            }
            else
            {
                // 非英雄卡
                if (basebattle_skillItemList != null)
                {
                    basebattle_skillItemList.OnPointerDown(drawCardData);
                }
            }
            BattleSelectedHelper.Instance.OnSetBattleGlobalMaskActive(true, drawCardData, curHero);
        };

        cardInput.onPointUpCB = () =>
        {
            RefreshSkillInfoPanel(false);
            RefreshPanelPos();

            if (basebattle_skillItemList != null)
            {
                basebattle_skillItemList.OnPointerUp(drawCardData);
            }

            BattleSelectedHelper.Instance.OnSetBattleGlobalMaskActive(false);
        };


        cardInput.onBeginCB = () =>
        {//开始拖拽
            if (curSkillCardInput != null)
            {
                return;
            }
            curSkillCardInput = cardInput;

            if (!basebattle_skillItemList.IconCanDrag(drawCardData) && basebattle_skillItemList.rightDownCanvasGroup.alpha != 0)
            {
                zxlogger.log("等待其他卡动画完成才能拖动");
                return;
            }
            lastTargetList = null;
            //获得当前技能的所属英雄
            curHero = SkillManager.ins.GetReleasSkillHeroByID(drawCardData.heroid);
            if (curHero != null && !curHero.bRecycle)
            {
                curSkillCardHolder = curHero;
                if (basebattle_skillItemList != null)
                {
                    basebattle_skillItemList.OnBeginDrag(drawCardData);
                }
            }
            else
            {
                // 非英雄卡
                if (basebattle_skillItemList != null)
                {
                    basebattle_skillItemList.OnBeginDrag(drawCardData);
                }
            }
            // TODO: 角色面板出来后再减速
            // Time.timeScale = 0.1f;
        };

        cardInput.onDragCB =async () =>
        {//拖拽中
            if (!basebattle_skillItemList.IconCanDrag(drawCardData) && basebattle_skillItemList.rightDownCanvasGroup.alpha != 0)
            {
                zxlogger.log("等待其他卡动画完成才能拖动");
                return;
            }

            RefreshPanelPos();
            tempRecordOneFingerPos = touchcstool.onefingerpos;
            //if (curHero != null && !curHero.bRecycle)
            //{
                if (!backPanel1.GetComponent<skillHander>().onPoint)
                {
                if (inputRoot != null)
                {
                    inputRoot.SetActive(true);
                }
                //BattleSelectedHelper.Instance.OnSetBattleGlobalMaskActive(true);
                if (curHero != null && !curHero.bRecycle)//英雄技能
                {
                    switch (drawCardData.skillCfgData.guidetype)
                    {
                        case 0://支援状态 参数0：自己  参数1:友方全体
                            int rangePar = int.Parse(drawCardData.skillCfgData.guiderange);
                            targetList = SkillManager.ins.GetBaseSkillTarget_0(rangePar, drawCardData.skillCfgData.hightlight, curHero);
                            if (inputRoot == null)
                            {
                                inputRoot = await ResourcesManager.Instance.LoadPrefabSync("skillIputRoot.prefab");
                                skillPoint = inputRoot.transform.Find("sprite/inputroot").gameObject;
                            }
                            inputRoot.SetActive(false);
                            inputRoot.transform.position = ZCamera.shoottouchray_check(GameCenter.mIns.m_BattleMgr.battleCamer, 1 << LayerMask.NameToLayer("terrain"));

                            //目标为自身或者友方全体时，给目标下方添加一个指示器
                            AddRangeOnSelf(targetList);
                            break;
                        case 4://获得技能索敌范围-矩形
                            string[] rangeParm = drawCardData.skillCfgData.guiderange.Split(';');
                            if (inputRoot == null)
                            {
                                inputRoot = await ResourcesManager.Instance.LoadPrefabSync("skillIputRoot.prefab");
                                SpriteRenderer renderer = inputRoot.GetComponentInChildren<SpriteRenderer>();
                                renderer.sprite = SpriteManager.Instance.GetSpriteSync("ui_fight_pnl_fanwei01");
                                float width = float.Parse(rangeParm[0]) / 100f;//长
                                float height = float.Parse(rangeParm[1]) / 100f;//宽

                                // 1.269==>之前定义的一个格子单位为1.3, 现在需要转换为1.65, 差1.269倍， --若格子数大于2，格子之间的间距为0.2, 需要减去格子数*0.2--, 只适用于矩形范围
                                renderer.size = new Vector2(width * 1.269f - ((width / 1.3f) > 1 ? (width / 1.3f * 0.02f) : 0), height * 1.269f - ((height / 1.3f) > 1 ? (height / 1.3f * 0.02f) : 0));
                                renderer.gameObject.transform.localPosition = new Vector3(-renderer.size.x / 2 + 0.05f, 0.1f, renderer.size.y / 2 - 0.05f);
                                skillPoint = inputRoot.transform.Find("sprite/inputroot").gameObject;
                                inputRoot.SetActive(true);
                            }
                            targetList = SkillManager.ins.GetBaseSkillTarget_4(int.Parse(rangeParm[0]) / 130, int.Parse(rangeParm[1]) / 130, inputRoot, drawCardData.skillCfgData.hightlight, curHero);
                            break;
                        case 5://获得技能索敌范围-单体-无限
                            targetList = SkillManager.ins.GetBaseSkillTarget_5(GameCenter.mIns.m_BattleMgr.battleCamer, drawCardData.skillCfgData.hightlight, curHero);
                            BattleSelectedHelper.Instance.CreateSingleUIIndicator(battleRoot);
                            break;
                        case 6://获得技能索敌范围-十字
                            if (inputRoot == null)
                            {
                                inputRoot = await ResourcesManager.Instance.LoadPrefabSync("skillInputRoot_1.prefab");
                                inputRoot.GetComponent<SkillInputRoot>().InitRoot(float.Parse(drawCardData.skillCfgData.guiderange));
                                skillPoint = inputRoot.transform.Find("sprite/inputroot").gameObject;
                                inputRoot.SetActive(true);
                            }
                            targetList = SkillManager.ins.GetBaseSkillTarget_6(drawCardData.skillCfgData.guiderange, inputRoot, drawCardData.skillCfgData.hightlight, curHero);
                            break;
                        case 7://场景中心
                            //范围半径
                            int rangeRadius = int.Parse(drawCardData.skillCfgData.guiderange);
                            if (inputRoot == null)
                            {
                                inputRoot = new GameObject();
                                skillPoint = inputRoot;
                            }
                            targetList = SkillManager.ins.GetBaseSkillTarget_7(rangeRadius, inputRoot, drawCardData.skillCfgData.hightlight, curHero);
                            AddRangeOnSelf(targetList);
                            break;
                        default:
                            break;
                    }

                }
                else//非英雄技能
                {
                    switch (drawCardData.skillCfgData.guidetype)
                    {
                        case 7://场景中心（全体）
                            targetList = BattleTrapManager.Instance.GetTargetListByTrap_7(drawCardData.skillCfgData);
                            break;
                        default:
                            break;
                    }
                }
                if (skillRange_inputRoot = null)
                {
                    GameObject.Destroy(skillRange_inputRoot.gameObject);
                }
                    if (inputRoot != null)
                    {
                        ModelRenderQueueHelper modelRenderQueueHelper = inputRoot.GetOrAddCompoonet<ModelRenderQueueHelper>();
                        if (modelRenderQueueHelper != null)
                        {
                            modelRenderQueueHelper.ToRenderQueue(3020);
                        }
                    skillRange_inputRoot = inputRoot;
                    }
                }
                else
                {
                if (inputRoot!=null)
                {
                    inputRoot.SetActive(false);
                }
                }

                //设置高亮
                if (lastTargetList == null || targetList == null)
                {
                    lastTargetList = targetList;
                }
                else
                {
                    removeHightLightList = GetRemoveHightLightList(lastTargetList, targetList);
                    BattleSelectedHelper.Instance.RemoveFromHighlights(removeHightLightList);
                    lastTargetList = targetList;
                }
                if (targetList != null)
                {
                    for (int i = 0; i < targetList.Count; i++)
                    {
                        BattleSelectedHelper.Instance.AddHighlightObj(targetList[i].roleObj);
                    }
                }
                // 加入释放者自己
                if (curHero != null)
                    BattleSelectedHelper.Instance.AddHighlightObj(curHero.roleObj);
                if (basebattle_skillItemList != null)
                {
                    basebattle_skillItemList.OnDrag(drawCardData);
                }
                // backPanel.transform.Find("select").gameObject.SetActive(touchcstool.onefingerpos.y <= fingerYPlaySkill);
            // }
        };

        cardInput.onEndCB = () =>//拖拽结束
        {
            RefreshSkillInfoPanel(false);
            Time.timeScale = 1f;
            curSkillCardInput = null;
            curSkillCardHolder = null;
            if (basebattle_skillItemList != null)
            {
                basebattle_skillItemList.OnCancelDrag(drawCardData);
            }
            BattleSelectedHelper.Instance.OnSetBattleGlobalMaskActive(false);
            RefreshPanelPos();
            if (inputRoot != null)
            {
                inputRoot.SetActive(false);
            }

            if (selectSelfRangeRoot!=null)
            {
                for (int i = 0; i < selectSelfRangeRoot.Count; i++)
                {
                    if (selectSelfRangeRoot[i]!=null)
                    {
                        GameObject.Destroy(selectSelfRangeRoot[i].gameObject);
                    }
                }
                selectSelfRangeRoot = null;
            }

            if (!backPanel1.GetComponent<skillHander>().onPoint)
            {
                bool isReleaseSucc = true;
                if (curHero != null)
                {

                    if (!curHero.bRecycle)
                    {
                        if (drawCardData.skillCfgData.guidetype == 5)//单体技能必须索到敌人才能释放
                        {
                            if (targetList != null && targetList.Count > 0)
                            {
                                curHero.isOnSkill = true;
                                if (targetList[0] != curHero)
                                {
                                    curHero.curSkillTarget = targetList[0];
                                }
                                //攻击者
                                BaseHero atker = BattleHeroManager.Instance.GetBaseHeroByHeroID(drawCardData.heroid);
                                switch (drawCardData.skillCfgData.skilltype)
                                {
                                    case 2://战技
                                        curHero.OnSkillBase(targetList, drawCardData.skillCfgData);
                                        break;
                                    case 3://秘技
                                        curHero.OnSkillBase(targetList, drawCardData.skillCfgData);
                                        break;
                                    case 4://终结技
                                        curHero.OnSkillBase(targetList, drawCardData.skillCfgData);
                                        break;
                                    default:
                                        break;
                                }
                                //释放技能
                                RemoveOneSkillCard(cardInput.uiPrefab, drawCardData);
                                RefreshSkillCardstate(curHero);
                                RefreshEnergy(drawCardData.energy, false);
                                // RefreshSkillCardstate(atker);
                            }
                            else
                            {
                                // 回手牌
                                isReleaseSucc = false;
                            }
                        }
                        else//范围敌人
                        {
                            //Vector3 targetPos = ZCamera.shoottouchray_check(battleManager.lookCamer, 1 << LayerMask.NameToLayer("terrain"));
                            curHero.isOnSkill = true;
                            //攻击者
                            BaseHero atker = BattleHeroManager.Instance.GetBaseHeroByHeroID(drawCardData.heroid);
                            switch (drawCardData.skillCfgData.skilltype)
                            {
                                case 2://战技
                                    curHero.OnSkillBase(skillPoint.transform.position, drawCardData.skillCfgData);
                                    break;
                                case 3://秘技
                                    curHero.OnSkillBase(skillPoint.transform.position, drawCardData.skillCfgData);
                                    break;
                                case 4://终结技
                                    curHero.OnSkillBase(skillPoint.transform.position, drawCardData.skillCfgData);
                                    break;
                                default:
                                    break;
                            }
                            //释放技能
                            RemoveOneSkillCard(cardInput.uiPrefab, drawCardData);
                            RefreshSkillCardstate(curHero);
                            RefreshEnergy(drawCardData.energy, false);
                        }
                    }
                    
                }
                else//非英雄技能
                {
                    GameCenter.mIns.m_BattleMgr.baseGod.OnSkillBase(targetList, drawCardData.skillCfgData);
                    RemoveOneSkillCard(cardInput.uiPrefab, drawCardData);
                    RefreshEnergy(drawCardData.energy, false);
                }
                if (basebattle_skillItemList != null && isReleaseSucc)
                {
                    basebattle_skillItemList.RemoveSkillIcon(drawCardData.uid);
                }
            }
            else
            {
                // 卡牌回原位
            }
            GameObject.Destroy(inputRoot);
            BattleSelectedHelper.Instance.Clear();
            lastTargetList = null;
            tempRecordOneFingerPos = default;
        };
    }

    private List<GameObject> GetRemoveHightLightList(List<BaseObject> oldList, List<BaseObject> newList)
    {

        List<GameObject> removeList = new List<GameObject>();
        if (oldList.Count < 1 && newList.Count < 1)
        {
            return removeList;
        }
        bool ishavs;
        for (int w = 0; w < oldList.Count; w++)
        {
            ishavs = false;
            for (int i = 0; i < newList.Count; i++)
            {
                if (oldList[w] == newList[i])
                {
                    ishavs = true;
                    break;
                }
            }
            if (!ishavs)
            {
                removeList.Add(oldList[w].roleObj);
            }
        }
        return removeList;
    }

    /// <summary>
    /// 刷新技能卡牌状态 同以英雄释放技能的公共cd 等于本次技能的表演时长+0.2s
    /// </summary>
    private void RefreshSkillCardstate(BaseObject hero)
    {
        float dur = hero.skillDurtion / 1000f;
        DOTween.To(() => hero.skillDurtion, t => hero.skillDurtion = t, 0, dur + 0.2f).SetEase(Ease.Linear);
    }

    /// <summary>
    /// 刷新技能卡的cd（英雄处于释放技能时，该英雄的其他卡牌回进入一个时长为英雄技能动画表现的cd）
    /// </summary>
    private void RefreshSkillCardCD()
    {
        for (int i = 0; i < CardManager.Instance.curCardList.Count; i++)
        {
            DrawCardData onecard = CardManager.Instance.curCardList[i];
            //获得技能卡片的持有者
            BaseHero curhero = BattleHeroManager.Instance.GetBaseHeroByHeroID(onecard.heroid);
            GameObject cd = onecard.item.transform.Find("bg/cd").gameObject;
            if (curhero != null)
            {
                if (curhero.skillDurtion >= 0)
                {
                    cd.SetActive(true);
                    cd.GetComponent<Image>().fillAmount = curhero.skillDurtion / curhero.skillTotalDurtion;
                    onecard.item.GetComponent<CardInputHandler>().SetSelfLock(true);
                    if (curhero.skillDurtion == 0)
                    {
                        cd.SetActive(false);
                        cd.GetComponent<Image>().fillAmount = 1;
                        onecard.item.GetComponent<CardInputHandler>().SetSelfLock(false);

                    }
                }
            }

        }
    }


    /// <summary>
    /// 刷新卡牌列表状态（根据算力）
    /// </summary>
    private async void RefreshCardSkillState()
    {
        for (int i = 0; i < CardManager.Instance.curCardList.Count; i++)
        {
            DrawCardData onecard = CardManager.Instance.curCardList[i];
            BattleSkillIcon battleSkillIcon = basebattle_skillItemList.GetBattleSkillIcon(onecard);


            if (onecard.skillCfgData.energycost > this.curEnergy)
            {
                if (battleSkillIcon != null && !battleSkillIcon.bGary)
                {
                    commontool.SetGary(battleSkillIcon.imgIcon, true);
                    battleSkillIcon.bGary = true;
                }
            }
            else if (onecard.skillCfgData.energycost <= this.curEnergy )
            {
                if (battleSkillIcon != null && battleSkillIcon.bGary)
                {
                    battleSkillIcon.bGary = false;
                    battleSkillIcon.imgIcon.material = await ResourcesManager.Instance.LoadAssetSync<Material>("Assets/allres/materials/UIDefaultExt.mat");
                    battleSkillIcon.InitHighLightParam();
                    battleSkillIcon.DoHighLight();
                }
                
            }
            onecard.item.GetComponent<CardInputHandler>().SetSelfLock(onecard.skillCfgData.energycost > this.curEnergy);
        }
    }


    /// <summary>
    /// 英雄信息界面
    /// </summary>
    /// <param name="bActive"></param>
    /// <param name="baseHero"></param>
    private void RefreshHeroInfoPanel(bool bActive, BaseHero baseHero = null, long heroId = 0)
    {
        infoPanel.SetActive(bActive);
        if (bActive)
        {
            heroInfo.SetActive(true);
            skillInfo.SetActive(false);
            if (basebattle_heroInfo != null)
            {
                basebattle_heroInfo.InitHeroInfo(heroInfo, baseHero != null ? baseHero.objID : heroId);
            }
        }
    }

    /// <summary>
    /// 技能信息界面
    /// </summary>
    /// <param name="bActive"></param>
    /// <param name="baseHero"></param>
    private void RefreshSkillInfoPanel(bool bActive, DrawCardData cardData = null, bool isChangedTimeScale = true)
    {
        if (bActive)
        {
            RectTransform infoBgRt = skillInfo.transform.parent.GetComponent<RectTransform>();
            CanvasGroup cg = infoPanel.GetComponent<CanvasGroup>();
            infoBgRt.anchoredPosition = infoBgRt.anchoredPosition.x > 0 ? new Vector2(-500, infoBgRt.anchoredPosition.y) : new Vector2(500, infoBgRt.anchoredPosition.y);
            Vector2 to = infoBgRt.anchoredPosition.x == -500 ? new Vector2(344.5f, infoBgRt.anchoredPosition.y) : new Vector2(-344.5f, infoBgRt.anchoredPosition.y);
            infoBgRt.DOKill();
            infoBgRt.DOAnchorPos(to, 0.2f).OnComplete(() => { if(isChangedTimeScale && GameCenter.mIns.m_BattleMgr.curBattleState == EBattleState.Start) Time.timeScale = 0.1f; });
            cg.alpha = 0;
            cg.DOKill();
            cg.DOFade(1, 0.2f);
            infoPanel.SetActive(true);
            heroInfo.SetActive(false);
            skillInfo.SetActive(true);
            if (basebattle_skillInfo != null)
                basebattle_skillInfo.InitSkillInfo(skillInfo, cardData.skillid, cardData.heroid);
        }
        else
        {
            Time.timeScale = 1;
            RectTransform infoBgRt = skillInfo.transform.parent.GetComponent<RectTransform>();
            CanvasGroup cg = infoPanel.GetComponent<CanvasGroup>();
            infoBgRt.DOKill();
            infoBgRt.DOAnchorPos(infoBgRt.anchoredPosition.x > 0 ? new Vector2(-500, infoBgRt.anchoredPosition.y) : new Vector2(500, infoBgRt.anchoredPosition.y), 0.2f);
            cg.alpha = 1;
            cg.DOKill();
            cg.DOFade(0, 0.2f).OnComplete(() =>
            {
                infoPanel.SetActive(false);
            });
        }
    }

    private void RefreshPanelPos()
    {
        // -1 左， 1：右
        int lorr = 0;
        if (touchcstool.onefingerpos.x > Screen.width / 2)
        {
            RectTransform rect = infoPanel.transform.Find("bg").GetComponent<RectTransform>();
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector3(344.5f, 0, 0);
            lorr = -1;
        }
        else
        {
            RectTransform rect = infoPanel.transform.Find("bg").GetComponent<RectTransform>();
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchoredPosition = new Vector3(-344.5f, 0, 0);
            lorr = 1;
        }
        if (basebattle_heroInfo != null)
            basebattle_heroInfo.UpdateFadeBody(lorr);
        if (basebattle_skillInfo != null)
            basebattle_skillInfo.UpdateFadeBody(lorr);
    }

    //改变速度
    partial void btn_changeSpeed_Click()
    {
    }

    //暂停
    partial void btn_pause_Click()
    {
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
    }

    //设置
    partial void btn_setting_Click()
    {
        throw new System.NotImplementedException();
    }

    partial void btn_skill_Click()
    {
        throw new NotImplementedException();
    }

    partial void btn_back_Click()
    {
        GameCenter.mIns.m_UIMgr.PopWindow("", "确认退出战斗？", "否", () =>
        {

        }, "是", () =>
        {
            GameCenter.mIns.m_BattleMgr.QuitBattle();
        });
       
    }

    partial void btn_resonance1_Click()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 势能共振
    /// </summary>
    partial void btn_resonance_Click()
    {
        resonancePanel.gameObject.SetActive(true);
    }

    partial void btn_resonancePanel_Click()
    {
        resonancePanel.gameObject.SetActive(false);
    }

    //筛选界面的显隐状态
    private bool screenActive = false;
    //筛选英雄
    partial void btn_screen_Click()
    {
        screenActive = !screenActive;
        screenPanel.SetActive(screenActive);
    }
    //筛选英雄（防误触）
    partial void btn_screenMask_Click()
    {
        screenActive = false;
        screenPanel.SetActive(screenActive);
    }

    /// <summary>
    /// 刷行筛选界面按钮的点击回调
    /// </summary>
    public void RefreshScreenPanelBtnsClick()
    {
        //编队按钮
        for (int i = 0; i < screenPanelTeambtns.Count; i++)
        {
            int teamIndex = i + 1;
            screenPanelTeambtns[i].onClick.AddListener(() =>
            {
                OnScreenPanelTeambtnOnClick(teamIndex);
            });
        }

        //元素按钮
        for (int i = 0; i < screenPanelYuanSubtns.Count; i++)
        {
            int yuansuIndex = i + 1;
            screenPanelYuanSubtns[i].onClick.AddListener(() =>
            {
                OnScreenPanelYuansubtnOnClick(yuansuIndex);
            });
        }

        //职业按钮
        for (int i = 0; i < screenPanelZhiyebtns.Count; i++)
        {
            int zhiyeIndex = i + 1;
            screenPanelZhiyebtns[i].onClick.AddListener(() =>
            {
                OnScreenPanelZhiyebtnOnClick(zhiyeIndex);
            });
        }

    }

    //当前选中的队伍id
    private int curDuiwu = 0;

    //当前选中的元素id
    private int curYuansu = 0;

    //当前选中的职业id
    private int curZhiye = 0;

    /// <summary>
    /// 筛选界面编队按钮回调
    /// </summary>
    /// <param name="index"></param>
    public void OnScreenPanelTeambtnOnClick(int index)
    {
        curDuiwu = curDuiwu == index ? 0 : index;
        RefreshDuiwubtnState(curDuiwu - 1);

        curYuansu = 0;
        curZhiye = 0;
        RefreshYuansubtnState(curYuansu - 1);
        RefreshZhiyebtnState(curZhiye - 1);

        RefreshDataListByCondition(0, 0, curDuiwu);
    }

    /// <summary>
    /// 筛选界面元素按钮回调
    /// </summary>
    /// <param name="index"></param>
    public void OnScreenPanelYuansubtnOnClick(int index)
    {
        curYuansu = curYuansu == index ? 0 : index;
        RefreshYuansubtnState(curYuansu - 1);

        curDuiwu = 0;
        RefreshDuiwubtnState(curDuiwu - 1);

        RefreshDataListByCondition(curYuansu, curZhiye, 0);
    }

    /// <summary>
    /// 筛选界面职业按钮回调
    /// </summary>
    /// <param name="index"></param>
    public void OnScreenPanelZhiyebtnOnClick(int index)
    {
        curZhiye = curZhiye == index ? 0 : index;
        RefreshZhiyebtnState(curZhiye - 1);

        curDuiwu = 0;
        RefreshDuiwubtnState(curDuiwu - 1);

        RefreshDataListByCondition(curYuansu, curZhiye, 0);
    }

    /// <summary>
    ///  刷新队伍按钮选中状态
    /// </summary>
    /// <param name="index">选中的下标</param>
    private void RefreshDuiwubtnState(int index)
    {
        for (int i = 0; i < screenPanelTeambtns.Count; i++)
        {
            screenPanelTeambtns[i].transform.Find("select").gameObject.SetActive(index == i);
        }
    }

    /// <summary>
    ///  刷新元素按钮选中状态
    /// </summary>
    /// <param name="index">选中的下标</param>
    private void RefreshYuansubtnState(int index)
    {
        for (int i = 0; i < screenPanelYuanSubtns.Count; i++)
        {
            screenPanelYuanSubtns[i].transform.Find("select").gameObject.SetActive(index == i);
        }
    }

    /// <summary>
    ///  刷新职业按钮选中状态
    /// </summary>
    /// <param name="index">选中的下标</param>
    private void RefreshZhiyebtnState(int index)
    {
        for (int i = 0; i < screenPanelZhiyebtns.Count; i++)
        {
            screenPanelZhiyebtns[i].transform.Find("select").gameObject.SetActive(index == i);
        }
    }

    /// <summary>
    /// 根据选中的条件筛选英雄
    /// </summary>
    /// <param name="yuansuID"></param>
    /// <param name="zhiyeID"></param>
    private void RefreshDataListByCondition(int yuansuID, int zhiyeID, int duiwuID)
    {
        List<HeroData> userHeroDatas = HeroDataManager.Instance.GetHeroDataList();
        cardHeroDatas = new List<HeroData>();
        for (int i = 0; i < userHeroDatas.Count; i++)
        {
            if (GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(userHeroDatas[i].heroID).open == 1)
            {
                cardHeroDatas.Add(userHeroDatas[i]);
            }
        }

        // 队伍, 与职业和元素互斥
        if (duiwuID > 0) // 点击了队伍
        {
            List<HeroData> heroList_1 = new List<HeroData>();
            List<HeroData> heroList_2 = new List<HeroData>();
            foreach (var hd in cardHeroDatas)
            {
                if (hd.teamId == duiwuID)
                    heroList_1.Add(hd);
                else
                    heroList_2.Add(hd);
            }

            heroList_1.Sort(new BattleHeroListSort());
            heroList_2.Sort(new BattleHeroListSort());
            foreach (var hd in heroList_2)
            {
                heroList_1.Add(hd);
            }
            cardHeroDatas = heroList_1;
            content.SetDatas(cardHeroDatas.Count, false);
            noScreenHero.SetActive(heroList_1.Count <= 0);
            heroList_1 = null;
            heroList_2 = null;
            return;
        }
        else // 点击了元素或职业队伍
        {
            curDuiwu = 0;
        }

        List<HeroData> heroList = new List<HeroData>();
        HeroInfoCfgData heroCfgData;
        for (int i = 0; i < cardHeroDatas.Count; i++)
        {
            heroCfgData = GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(cardHeroDatas[i].heroID);
            HeroData hd = HeroDataManager.Instance.GetHeroData(heroCfgData.heroid);
            if (hd == null)
            {
                zxlogger.logerror($"Error: 'RefreshDataListByCondition' failed! the heroid={heroCfgData.heroid} not find hero data in HeroCfgManager's heroDataList!");
                return;
            }

            /*if (yuansuID > 0 && zhiyeID > 0 && duiwuID > 0)
            {
                if (heroCfgData.element == yuansuID && int.Parse(heroCfgData.profession) == zhiyeID && hd.teamId == duiwuID)
                {
                    heroList.Add(cardHeroDatas[i]);
                }
            }
            else*/
            if (yuansuID > 0 && zhiyeID > 0)
            {
                if (heroCfgData.element == yuansuID && heroCfgData.profession == zhiyeID)
                {
                    heroList.Add(cardHeroDatas[i]);
                }
            }
            //else if (zhiyeID > 0 && duiwuID > 0)
            //{
            //    if (int.Parse(heroCfgData.profession) == zhiyeID && hd.teamId == duiwuID)
            //    {
            //        heroList.Add(cardHeroDatas[i]);
            //    }
            //}
            //else if (yuansuID > 0 && duiwuID > 0)
            //{
            //    if (heroCfgData.element == yuansuID && hd.teamId == duiwuID)
            //    {
            //        heroList.Add(cardHeroDatas[i]);
            //    }
            //}
            else if (yuansuID > 0)
            {
                if (heroCfgData.element == yuansuID)
                {
                    heroList.Add(cardHeroDatas[i]);
                }
            }
            else if (zhiyeID > 0)
            {
                if (heroCfgData.profession == zhiyeID)
                {
                    heroList.Add(cardHeroDatas[i]);
                }
            }
            //else if (duiwuID > 0)
            //{
            //    if (hd.teamId == duiwuID)
            //    {
            //        heroList.Add(cardHeroDatas[i]);
            //    }
            //}
            else
            {
                heroList.Add(cardHeroDatas[i]);
            }
        }
        cardHeroDatas = heroList;
        content.SetDatas(cardHeroDatas.Count, false);

        noScreenHero.SetActive(heroList.Count <= 0);
    }


    partial void btn_weapon_Click()
    {
        throw new NotImplementedException();
    }
    partial void leaderSkill_Click()
    {
        throw new NotImplementedException();
    }

    private bool monsterInfoActive = false;

    //怪物信息按钮
    partial void monster_Click()
    {
        if (basebattle_monsterInfo != null)
            basebattle_monsterInfo.Display();


        //monsterInfoActive = !monsterInfoActive;
        //if (monsterInfoActive)
        //{
        //    monsterInfoPanel.gameObject.SetActive(monsterInfoActive);
        //    monsterInfoPanel.transform.Find("bg").GetComponent<RectTransform>().DOAnchorPosX(-350, 0.1f).SetEase(Ease.Linear);
        //}
        //else
        //{
        //    monsterInfoPanel.transform.Find("bg").GetComponent<RectTransform>().DOAnchorPosX(700, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
        //    {
        //        monsterInfoPanel.gameObject.SetActive(monsterInfoActive);
        //    });
        //}
    }
    partial void btn_monsterInfoPanel_Click()
    {
        monsterInfoActive = false;
        monsterInfoPanel.transform.Find("bg").GetComponent<RectTransform>().DOAnchorPosX(700, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            monsterInfoPanel.gameObject.SetActive(monsterInfoActive);
        });
    }

    //支援状态管理的列表以及时间回调
    private Dictionary<long, GameObject> upSkillitem = new Dictionary<long, GameObject>();
    private Dictionary<long, Tween> UpSkillDic = new Dictionary<long, Tween>();
    /// <summary>
    /// 移除一个支援状态技能
    /// </summary>
    public void RemoveUpSkill(BattleSkillCfg battleSkillCfg)
    {
        if (!UpSkillDic.ContainsKey(battleSkillCfg.skillid) || !upSkillitem.ContainsKey(battleSkillCfg.skillid))
        {
            return;
        }
        UpSkillDic[battleSkillCfg.skillid].Kill();
        UpSkillDic.Remove(battleSkillCfg.skillid);


        upSkillitem[battleSkillCfg.skillid].SetActive(false);
        GameObject.Destroy(upSkillitem[battleSkillCfg.skillid]);
        upSkillitem.Remove(battleSkillCfg.skillid);

        BuffManager.ins.RemoveOneUpSkill(battleSkillCfg.skillid);
    }
    /// <summary>
    /// 添加一个支援状态到buff列表
    /// </summary>
    public void AddOneBuffToBuffList(BattleSkillCfg battleSkillCfg)
    {
        if (upSkillitem.ContainsKey(battleSkillCfg.skillid) && UpSkillDic.ContainsKey(battleSkillCfg.skillid))
        {
            GameObject item = upSkillitem[battleSkillCfg.skillid];
            Image mask = item.transform.Find("mask").GetComponent<Image>();
            TextMeshProUGUI time = item.transform.Find("timer").GetComponent<TextMeshProUGUI>();
            mask.fillAmount = 0;

            UpSkillDic[battleSkillCfg.skillid].Kill();
            UpSkillDic[battleSkillCfg.skillid] = mask.DOFillAmount(1, battleSkillCfg.suptime / 1000).SetEase(Ease.Linear).OnUpdate(() =>
            {
                time.text = ((int)((1 - mask.fillAmount) * (battleSkillCfg.suptime / 1000))).ToString();
            }).OnComplete(() =>
            {
                item.SetActive(false);
                GameObject.Destroy(item);
                if (upSkillitem.ContainsKey(battleSkillCfg.skillid))
                {
                    upSkillitem.Remove(battleSkillCfg.skillid);
                }
                if (UpSkillDic.ContainsKey(battleSkillCfg.skillid))
                {
                    UpSkillDic.Remove(battleSkillCfg.skillid);
                }
                BuffManager.ins.RemoveOneUpSkill(battleSkillCfg.skillid);
            });
        }
        else
        {
            GameObject item = GameObject.Instantiate(buffItem.gameObject);
            item.transform.SetParent(buffList.transform);
            item.transform.Find("icon").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(battleSkillCfg.supicon);
            Image mask = item.transform.Find("mask").GetComponent<Image>();
            TextMeshProUGUI time = item.transform.Find("timer").GetComponent<TextMeshProUGUI>();
            mask.fillAmount = 0;
            Tween tween = mask.DOFillAmount(1, battleSkillCfg.suptime / 1000).SetEase(Ease.Linear).OnUpdate(() =>
            {
                time.text = ((int)((1 - mask.fillAmount) * (battleSkillCfg.suptime / 1000))).ToString();
            }).OnComplete(() =>
            {
                item.SetActive(false);
                GameObject.Destroy(item);
                if (upSkillitem.ContainsKey(battleSkillCfg.skillid))
                {
                    upSkillitem.Remove(battleSkillCfg.skillid);
                }
                if (UpSkillDic.ContainsKey(battleSkillCfg.skillid))
                {
                    UpSkillDic.Remove(battleSkillCfg.skillid);
                }
                BuffManager.ins.RemoveOneUpSkill(battleSkillCfg.skillid);
            });
            item.SetActive(true);
            UpSkillDic.Add(battleSkillCfg.skillid, tween);
            upSkillitem.Add(battleSkillCfg.skillid, item);
        }
    }

    /*
     	英雄列表排序功能如下			
		基础规则		
			等级高的英雄靠前排	
				等级相同时，品质高的英雄靠前排
				
		当有队伍筛选时		
			被筛选的队伍靠前	
				同队中根据基础规则排序
			未被筛选的队伍靠后	
				未被筛选的英雄根据基础规则排序
				
		当有元素/职业筛选时		
			去除不符合筛选条件的角色	
			剩余角色根据基础规则排序	
				
		队伍筛选与元素/职业筛选互斥		
			启用队伍筛选时，自动取消元素/职业筛选条件	
			启用元素/职业筛选时，自动移除队伍筛选	
		元素与职业筛选可以共存		
			元素筛选只能单选	
			职业筛选只能单选	
     */
    private class BattleHeroListSort : IComparer<HeroData>
    {
        public int Compare(HeroData a, HeroData b)
        {
            int quality_a = GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(a.heroID).quality;
            int quality_b = GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(b.heroID).quality;
            if (a.level == b.level)
            {
                return quality_b.CompareTo(quality_a);
            }
            return b.level.CompareTo(a.level);
        }
    }

}
