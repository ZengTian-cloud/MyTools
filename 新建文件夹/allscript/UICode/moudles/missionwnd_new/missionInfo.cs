using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LitJson;
using DG.Tweening;

public class missionInfo
{
    public GameObject uiRoot;


    public TextMeshProUGUI missionName;//关卡名字
    public GameObject missionCondition;//关卡条件
    public GameObject eventRoot;//事件
    public GameObject collectible;//藏品收集
    public GameObject reward;//奖励
    public GameObject cost;//消耗品
    public Button battleBtn;//战斗按钮

    public Button testBattleBtn;//gm通关按钮

    public Button mask;

    private MissionCfgData cfgData;
    private MissionMsg msgData;

    private bool isChallage = false;

    private missionwnd_new wnd;
    private JsonData random;//战斗通信所需要的随机种子

    private Tweener tweener;
    public missionInfo(GameObject obj, missionwnd_new missionwnd_New)
    {
        wnd = missionwnd_New;
        uiRoot = obj;
        missionName = obj.transform.Find("bg/title/missionName").GetComponent<TextMeshProUGUI>();
        missionCondition = obj.transform.Find("bg/missionCondition").gameObject;
        eventRoot = obj.transform.Find("bg/event").gameObject;
        collectible = obj.transform.Find("bg/collectible").gameObject;
        reward = obj.transform.Find("bg/reward").gameObject;
        cost = obj.transform.Find("bg/cost").gameObject;
        battleBtn = obj.transform.Find("bg/battleBtn").GetComponent<Button>();
        testBattleBtn = obj.transform.Find("bg/testBattleBtn").GetComponent<Button>();
        mask = obj.transform.Find("mask").GetComponent<Button>();

        mask.gameObject.SetActive(false);

        //事件和藏品暂不开发-隐藏
        eventRoot.SetActive(false);
        collectible.SetActive(false);

        mask.AddListenerBeforeClear(() =>
        {
            Close();
        });

        battleBtn.AddListenerBeforeClear(() => {

            if (isChallage)
            {
                GameCenter.mIns.m_UIMgr.PopMsg("测试挑战中，无法再次战斗！只能结算!");
                return;
            }
            if (cfgData != null)
            {

                switch (cfgData.type)
                {
                    default:
                        break;
                }
                if (cfgData.type == 2)//是剧情关
                {
                    //判断是否有剧情模块
                    BattleInteractCfg interactCfg = BattleCfgManager.Instance.GetBattleInteractByMissionAndTimer(cfgData.mission, 3);
                    if (interactCfg != null)
                    {
                        //发起战斗请求
                        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
                        jsonData["missionid"] = cfgData.mission;
                        GameCenter.mIns.m_NetMgr.SendData(NetCfg.CHAPTER_MISSION_CHALLAGE, jsonData, (eqid, code, content) =>
                        {
                            if (code == 0)
                            {
                                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                                {

                                    JsonData msg = jsontool.newwithstring(content);
                                    if (msg.ContainsKey("random"))
                                    {
                                        this.random = msg["random"];
                                        GameCenter.mIns.m_UIMgr.Open<battlestorywnd>(interactCfg, this.random);
                                    }
                                });
                            }
                        });
                    }
                }
                else if (cfgData.type == 4)//解密关卡
                {
                    BattleDecodeManager.Instance.EnterBattleDecode(cfgData.mission);
                }
                else//战斗关卡
                {
                    GameSceneManager.Instance.EnterBattleScene(cfgData.mission);
                }

            }

        });

        testBattleBtn.AddListenerBeforeClear(() =>
        {
            if (cfgData != null)
            {
                // 测试直接通过关卡
                // 发送挑战数据
                //ChapterManager.Instance.SendMissionChallageMsg(cfgData.chapter, cfgData.mission);
                JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
                jsonData["missionid"] = this.cfgData.mission;
                GameCenter.mIns.m_NetMgr.SendData(NetCfg.CHAPTER_MISSION_CHALLAGE, jsonData, (eqid, code, content) =>
                {
                    if (code == 0)
                    {
                        GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                        {
                            //章节数据
                            JsonData msg = jsontool.newwithstring(content);
                            if (msg.ContainsKey("random"))
                            {

                                random = msg["random"];

                                //发起结算请求
                                JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
                                jsonData["missionid"] = this.cfgData.mission;
                                jsonData["star"] = UnityEngine.Random.Range(1, 3);
                                jsonData["randnum"] = random;
                                jsonData["story"] = "";
                                GameCenter.mIns.m_NetMgr.SendData(NetCfg.CHAPTER_MISSION_RESULT, jsonData, (eqid, code, content) =>
                                {
                                    if (code == 0)
                                    {
                                        GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                                        {
                                            MissionHelper.Instance.SendMissionMsg(wnd.curChapterData.chapterId, (missionMsgs, areaMsgs) =>
                                            {
                                                //根据下发关卡获取到最近通关关卡的所在模块
                                                int curAreaID = wnd.GetLastMissionOfMoudle(missionMsgs);
                                                wnd.RefreshMissionMoudles(curAreaID);

                                                wnd.missionRoot.chapterBtn.AddListenerBeforeClear(() =>
                                                {
                                                    wnd.areaInfo.Display(wnd.curChapterData, areaMsgs, wnd.curArea.cfgData.areaid);
                                                });
                                            });
                                        });

                                    }
                                });
                            }
                        });

                    }
                });
            }

        });

        /*

 

        btnUnlock.AddListenerBeforeClear(() =>
        {
            if (cfgData != null)
            {
                int unlock = msgData.unlock;
                // 需要解锁条件解锁
                if (unlock > 0)
                {
                    if (unlock == 201)
                    {
                        // ”与“通过a关卡b星解锁（关卡id;星数
                    }
                    else if (unlock == 202)
                    {
                        // “或”通过a关卡b星解锁（关卡id;星数）
                    }
                    else if (unlock == 301)
                    {
                        // 拥有a道具b数量解锁（道具id;数量|道具id;数量）
                    }
                    else if (unlock == 302)
                    {
                        // 消耗a道具b数量解锁（道具id;数量|道具id;数量）
                    }
                    else if (unlock == 501)
                    {
                        // 剧情选择解锁（剧情选项id）
                    }
                    else if (unlock == 601)
                    {
                        // 章节探索度解锁（探索度百分比）
                    }
                    else if (unlock == 701)
                    {
                        // 世界等级达到解锁（世界等级）
                    }
                    else if (unlock == 801)
                    {
                        // 达成成就解锁
                    }

                    //ChapterManager.Instance.SendUnlockMissionMsg(ChapterManager.Instance.GetCurrChapterId(), cfgData.mission);
                }
                else
                {
                    GameCenter.mIns.m_UIMgr.PopMsg("该关卡解锁条件为0，不需要手动解锁!");
                }
            }
            Close();
            isChallage = false;
        });*/

    }


    public void Display(MissionCfgData cfg, MissionMsg msg)
    {

        cfgData = cfg;
        msgData = msg;
        missionName.SetTextExt(cfgData.mission + " - " + GameCenter.mIns.m_LanMgr.GetLan(cfgData.name2));
        missionCondition.SetActive(cfg.type == 1);
        if (cfg.type == 1)
        {
            if (msgData != null)
            {
                SetStarInfo(msgData.star, cfgData.mission);
            }
            else
            {
                SetStarInfo(0, cfgData.mission);
            }

        }

        if (uiRoot != null)
        {
            if (tweener != null)
            {
                tweener.Kill();
            }
            tweener = uiRoot.transform.DOLocalMoveX(843, 0.2f);
            mask.gameObject.SetActive(true);
        }
    }

    public void SetStarInfo(int curStar, long mission)
    {
        BattleMissionParamCfg paramCfg = BattleCfgManager.Instance.GetMissionParamCfg(mission);
        for (int i = 1; i <= 3; i++)
        {
            Transform cond = missionCondition.transform.Find($"Condition_{i}");
            cond.Find("Image").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(curStar >= i ? "ui_e_icon_xing_xiao_01" : "ui_e_icon_xing_xiao_hui");
            switch (i)
            {
                case 1:
                    cond.Find("condotionText").GetComponent<TextMeshProUGUI>().text = GetParmText(paramCfg.star1, paramCfg.star1param);
                    break;
                case 2:
                    cond.Find("condotionText").GetComponent<TextMeshProUGUI>().text = GetParmText(paramCfg.star2, paramCfg.star2param);
                    break;
                case 3:
                    cond.Find("condotionText").GetComponent<TextMeshProUGUI>().text = GetParmText(paramCfg.star3, paramCfg.star3param);
                    break;
                default:
                    break;
            }
        }
    }

    public string GetParmText(int starType,string parm)
    {
        string note = string.Empty;
        switch (starType)
        {
            case 1://主角血量
                note = $"主角血量不低于{parm}";
                break;
            case 2://关卡某个事件
                break;
            default:
                break;
        }
        return note;
    } 


    public void Close()
    {
        if (uiRoot != null)
        {
            if (tweener != null)
            {
                tweener.Kill();
            }
            tweener = uiRoot.transform.DOLocalMoveX(1700, 0.2f);
            mask.gameObject.SetActive(false);
        }
    }

 
}

