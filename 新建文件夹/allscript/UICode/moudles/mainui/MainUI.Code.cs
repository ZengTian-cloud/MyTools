using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UIManager;
using NetWork.Http;
using System.IO;
using Cysharp.Threading.Tasks;
using static UnityEngine.Rendering.DebugUI;
using DG.Tweening;
using System.Resources;
using GameTiJi;
using System.Text;

public partial class mainui
{
    #region 定义和生命周期
    public override UILayerType uiLayerType => UILayerType.Backgroud;
    public override string uiAtlasName => "mainui";

    Sequence hideTween;

    MiniMap miniMap;

    //本地存储的当前追踪任务的key值
    private string curTargetTaskKey = $"{GameCenter.mIns.userInfo.Uid}_curTargetTask";

    protected override void OnInit()
    {
        //检测热更
        GameCenter.mIns.m_HttpMgr.SendData("POST", 30, "", "", (state, content, val) =>
        {

        });

        GameEventMgr.Register(GEKey.NpcInteracton_OnDialogueStart, (args) =>
        {
            Debug.Log("mainui 响应了 StarDialogue 事件");

            if (hideTween == null)
            {
                hideTween = DOTween.Sequence().SetAutoKill(false); //TODO:动画改个通用方法

                hideTween.Insert(0, nodeLeft.GetComponent<RectTransform>().DOAnchorPosX(nodeLeft.GetComponent<RectTransform>().anchoredPosition.x - (UnityEngine.Screen.width / 2), 0.4f));

                hideTween.Insert(0, nodeRight.GetComponent<RectTransform>().DOAnchorPosX(nodeRight.GetComponent<RectTransform>().anchoredPosition.x + (UnityEngine.Screen.width / 2), 0.4f));

                hideTween.Insert(0, nodeBottom.GetComponent<RectTransform>().DOAnchorPosY(nodeBottom.GetComponent<RectTransform>().anchoredPosition.y - (UnityEngine.Screen.height / 2), 0.4f));

                hideTween.Insert(0, nodeLeft.GetComponent<CanvasGroup>().DOFade(0, 0.2f));

                hideTween.Insert(0, nodeRight.GetComponent<CanvasGroup>().DOFade(0, 0.2f));

                hideTween.Insert(0, nodeBottom.GetComponent<CanvasGroup>().DOFade(0, 0.2f));
            }
            else
            {
                hideTween.PlayForward();
            }
        });

        GameEventMgr.Register(GEKey.NpcInteracton_OnDialogueEnd, (args) =>
        {
            Debug.Log("mainui 响应了 FinishDialogue 事件");

            hideTween.PlayBackwards();
        });

        GameEventMgr.Register(GEKey.OnEnterMainScene, (gEventArgs) =>
        {
            if (gEventArgs == null || miniMap != null) return;

            GameObject player = gEventArgs.args[0] as GameObject;

            Camera camera = gEventArgs.args[1] as Camera;

            LoadMiniMap(player.transform,camera.transform);
        });

        //任务完成的事件派发监听
        GameEventMgr.Register(NetCfg.PUSH_TASK_END_NOTIF.ToString(), OnTaskSuccessPush);
        //主线任务自动领取的事件
        GameEventMgr.Register(GEKey.Task_Main_AutoReward, OnMainTaskAutoReward);

#if UNITY_EDITOR
        //gm面板
        this.gmPanel.gameObject.SetActive(true);
#endif
    }

    protected override void OnOpen()
    {
        this.CheckTargetTask();
        base.OnOpen();
        GameCenter.mIns.m_UIMgr.AddUpdateWin(this);

        MainSceneManager.Instance.OnEnter();

        SetPlayerInfo();

        SetSystemInfo();
    }

    protected override void OnClose()
    {
        MainSceneManager.Instance.OnLeave();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    #endregion

    /// <summary>
    /// 检测是否有正在追踪的任务
    /// </summary>
    private void CheckTargetTask()
    {
        if (PlayerPrefs.HasKey(this.curTargetTaskKey))
        {
            this.targetTask.gameObject.SetActive(true);
            this.RefreshTargetTaskPanel(PlayerPrefs.GetString(this.curTargetTaskKey));
        }
        else
        {
            this.targetTask.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 任务完成的下发监听
    /// </summary>
    private void OnTaskSuccessPush(GEventArgs gEventArgs)
    {
        GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
        {
            //任务模块
            int model = (int)gEventArgs.args[0];
            //完成的任务列表
            List<long> taskIDs = (List<long>)gEventArgs.args[1];
            if (model <= 4)//这里暂时只处理前四个模块
            {
                if (PlayerPrefs.HasKey(this.curTargetTaskKey))
                {
                    //获得缓存数据
                    string[] prefs = PlayerPrefs.GetString(this.curTargetTaskKey).Split(';');
                    for (int i = 0; i < taskIDs.Count; i++)
                    {
                        if (taskIDs[i] == long.Parse(prefs[0]))
                        {
                            prefs[4] = "1";
                            break;
                        }
                    }
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < prefs.Length; i++)
                    {
                        builder.Append(prefs[i]);
                        if (i < prefs.Length - 1)
                        {
                            builder.Append(";");
                        }
                    }
                    PlayerPrefs.SetString(this.curTargetTaskKey, builder.ToString());
                    RefreshTargetTaskPanel(builder.ToString());
                }
            }
        });
    }

    /// <summary>
    /// 刷新任务面板
    /// </summary>
    /// <param name="key"></param>
    private void RefreshTargetTaskPanel(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            string[] keys = key.Split(';');
            long taskID = long.Parse(keys[0]);
            string note = keys[1];
            string name = keys[2];
            int model = int.Parse(keys[3]);
            int state = int.Parse(keys[4]);
            targetTaskName.text = name;
            targetTaskCondition.text = GameCenter.mIns.m_LanMgr.GetLan(note);
            taskIcon.gameObject.SetActive(state == 1);
            switch (model)
            {
                case 1://主线
                    targetTaskIcon.sprite = SpriteManager.Instance.GetSpriteSync("ui_h_icon_renwu_zhuxian");
                    break;
                case 2://每日
                    targetTaskIcon.sprite = SpriteManager.Instance.GetSpriteSync("ui_h_icon_renwu_tansuo");
                    break;
                case 3://冒险
                    targetTaskIcon.sprite = SpriteManager.Instance.GetSpriteSync("ui_h_icon_renwu_maoxian");
                    break;
                case 4://同伴
                    targetTaskIcon.sprite = SpriteManager.Instance.GetSpriteSync("ui_h_icon_renwu_tongban");
                    break;
                default:
                    break;
            }
            TaskBaseCfg baseCfg = TaskCfgManager.Instance.GetTaskBaseCfgByTaskID(taskID);
            targetTask.AddListenerBeforeClear(() =>
            {
                JumpManger.Instance.DoJumpByTask(taskID);//点击跳转
            });
        }       
    }

    /// <summary>
    /// 自动领取主线任务事件回调
    /// </summary>
    /// <param name="taskID"></param>
    private void OnMainTaskAutoReward(GEventArgs gEventArgs)
    {
        GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
        {
            long taskID = (long)gEventArgs.args[0];
            if (PlayerPrefs.HasKey(this.curTargetTaskKey))
            {
                string[] keys = PlayerPrefs.GetString(this.curTargetTaskKey).Split(';');
                if (taskID == long.Parse(keys[0]))
                {
                    PlayerPrefs.DeleteKey(this.curTargetTaskKey);
                    CheckTargetTask();
                }

            }
        });
    }


    private void SetPlayerInfo()
    {
        //playerAvatar.sprite

        playerLv.text = GameCenter.mIns.userInfo.Level.ToString();
    }

    private async void SetSystemInfo()
    {
        while (true)
        {
            SetNetWorkShow();
            SetBatterShow();
            time.text = System.DateTime.Now.ToString("HH:mm");
            LayoutRebuilder.ForceRebuildLayoutImmediate(systemInfoLayout);
            await UniTask.Delay(1000, true);
        }
    }

    private void SetNetWorkShow()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            network.SetActive(false);
            return;
        }
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable://无网络
                network_LocalArea.SetActive(false);
                network_CarrierData.SetActive(false);
                break;
            case NetworkReachability.ReachableViaCarrierDataNetwork://流量
                network_CarrierData.SetActive(true);
                network_LocalArea.SetActive(false);
                break;
            case NetworkReachability.ReachableViaLocalAreaNetwork://wifi
                network_LocalArea.SetActive(true);
                network_CarrierData.SetActive(false);
                break;
            default:
                break;
        }
    }
    private void SetBatterShow()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            battery.gameObject.SetActive(false);
            return;
        }
        battery.value = Mathf.Clamp01(SystemInfo.batteryLevel);

        chargingTag.SetActive(SystemInfo.batteryStatus == BatteryStatus.Charging);
    }

    private async void LoadMiniMap(Transform player , Transform camera)
    {
        GameObject go = await ResourcesManager.Instance.LoadUIPrefabSync("minimap");

        go.transform.SetParent(nodeLeft.transform.Find("top/miniMap"), false);

        MiniMap miniMap = go.AddComponent<MiniMap>();

        miniMap.Init(GameConfig.Get<NpcMapConfig>(1) , player , camera);
    }


    #region 点击事件
    private void OnClickPlayerInfoBtn()
    {

    }

    /// <summary>
    /// 玩法
    /// </summary>
    private void OnClickGameLevelBtn()
    {
        long curMission = GameCenter.mIns.userInfo.nowmissionid;
        curMission = curMission > 0 ? curMission : 1010010101;//默认第一关
        MissionCfgData missionCfg = MissionCfgManager.Instance.GetMissionCfgByMissionID(curMission);
        if (missionCfg.chapter > 0)
        {
            ChapterCfgData chapter = ChapterCfgManager.Instance.GetChapterCfgDataByChapterId(missionCfg.chapter);
            MainSceneManager.Instance.OnLeave();
            GameCenter.mIns.m_UIMgr.Open<missionwnd_new>(new chapterData(chapter.chapter, chapter, null));
        }
    }

    private void OnClickHeroGrowBtn()
    {
        MainSceneManager.Instance.OnLeave();
        GameCenter.mIns.m_UIMgr.Open<HeroGrow>();
    }
    private void OnClickWareHouseBtn()
    {
        MainSceneManager.Instance.OnLeave();
        GameCenter.mIns.m_UIMgr.Open<Warehouse>();
    }
    private void OnClickDrawCardBtn()
    {

    }
    private async void OnClickShopBtn()
    {
        await NpcInteractionManager.Instance.Init();
    }
    private void OnClickFormationBtn()
    {

    }


    private void OnClickTaskBtn()
    {
        
        MainSceneManager.Instance.OnLeave();
        GameCenter.mIns.m_UIMgr.Open<task_wnd>();

    }
    private void OnClickLogbookBtn()
    {
        MainSceneManager.Instance.OnLeave();
        GameCenter.mIns.m_UIMgr.Open<logbookwnd>();
    }
    private void OnClickMailBtn()
    {
        MainSceneManager.Instance.OnLeave();
        GameCenter.mIns.m_UIMgr.Open<SocialUI>(EnumSocialTabType.Mail);
    }
    private void OnClickFriendBtn()
    {
        MainSceneManager.Instance.OnLeave();
        GameCenter.mIns.m_UIMgr.Open<SocialUI>(EnumSocialTabType.Friend);
    }
    private void OnClickSwitchBtn()
    {

    }

    private void btnResetLens_Click()
    {
        MainSceneManager.Instance.ResetLens();
    }
    #endregion

    #region functions
    private TestMainSceneHeroCtrl heroCtrl;
    public override void UpdateWin()
    {
        base.UpdateWin();
        heroCtrl = MainSceneManager.Instance.GetHeroCtrl();
        if (heroCtrl != null)
        {
            float CurrJoystickAngle = heroCtrl.CurrJoystickAngle;

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButton(0))
#else
            if (Input.touchCount > 0)
#endif
            {
                if (CurrJoystickAngle >= 0)
                {
                    joystickArrowTran.rotation = Quaternion.Euler(0, 0, -CurrJoystickAngle);
                    if (!joystickArrowTran.gameObject.activeSelf)
                        joystickArrowTran.gameObject.SetActive(true);
                }
            }
            else
            {
                if (joystickArrowTran.gameObject.activeSelf)
                    joystickArrowTran.gameObject.SetActive(false);
            }
        }
    }
    #endregion
}
