using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using LitJson;


/// <summary>
/// 战斗剧情对话界面
/// </summary>
public partial class battlestorywnd
{
    public override UILayerType uiLayerType => UILayerType.Pop;

    public override string uiAtlasName => "";

    private BattleInteractCfg curInteract;

    private string curText;

    private Tweener textDot;//文本的dot动画

    private List<Button> btnList;

    private bool bSend;//是否发起战斗通信
    private JsonData randse;
    private long mission;

    protected override void OnInit()
    {
        btn_interact.gameObject.SetActive(false);
        btn_List.SetActive(false);
        btnList = new List<Button>();
    }


    /// <summary>
    /// 打开时
    /// </summary>
    protected override void OnOpen()
    {
        curText = "";
        bSend = false;
        if (openArgs !=  null && openArgs.Length > 0)
        {
            curInteract = (BattleInteractCfg)openArgs[0];
            bSend = curInteract.param == 3;//不进入战斗的剧情节点，在对话完成后需要发起战斗结算
            if (curInteract.param == 3)
            {
                mission = curInteract.mission;
                randse = (JsonData)openArgs[1];
            }
        }

        if (curInteract != null)
        {

            HandleTalk(curInteract);
        }
    }


    /// <summary>
    /// 处理对话数据
    /// </summary>
    private void HandleTalk(BattleInteractCfg curInteract)
    {
        //随机一个对话开始演播
        this.curInteract = curInteract;
        string[] dialogyeIDs = curInteract.dialogueId.Split(';');
        if (dialogyeIDs.Length == 1)
        {
            OnTalkStart(long.Parse(dialogyeIDs[0]));
        }
        else
        {
            int index = UnityEngine.Random.Range(0, dialogyeIDs.Length);
            OnTalkStart(long.Parse(dialogyeIDs[index]));
        }
    }

    /// <summary>
    /// 开始对话
    /// </summary>
    private void OnTalkStart(long talkID)
    {
        //获得当前对话配置
        BattleTalkCfg talkCfg = BattleCfgManager.Instance.GetBattleTalkCfgByID(talkID);
        if (!string.IsNullOrEmpty(talkCfg.background) && talkCfg.background != "-1")
        {
            SpriteManager.Instance.GetTextureSpriteAsync($"common/{talkCfg.background}", (s) =>
            {
                this.bgicon.sprite = s;
                this.bgicon.gameObject.SetActive(true);
            });
        }
        else
        {
            this.bgicon.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(talkCfg.picture1) && talkCfg.picture1 != "-1")//立绘左
        {
            SpriteManager.Instance.GetTextureSpriteAsync($"heroIcon_fullbody/{talkCfg.picture1}", (s) =>
            {
                this.speaker_l.sprite = s;
                ChangeSpeakerIconState(this.speaker_l, talkCfg.effects1);
                this.speaker_l.gameObject.SetActive(true);
            });
        }
        else
        {
            this.speaker_l.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(talkCfg.picture2) && talkCfg.picture2 != "-1")//立绘中
        {
            SpriteManager.Instance.GetTextureSpriteAsync($"heroIcon_fullbody/{talkCfg.picture2}", (s) =>
            {
                this.speaker_m.sprite = s;
                ChangeSpeakerIconState(this.speaker_m, talkCfg.effects2);
                this.speaker_m.gameObject.SetActive(true);
            });
        }
        else
        {
            this.speaker_m.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(talkCfg.picture3) && talkCfg.picture3 != "-1")//立绘右
        {
            SpriteManager.Instance.GetTextureSpriteAsync($"heroIcon_fullbody/{talkCfg.picture3}", (s) =>
            {
                this.speaker_r.sprite = s;
                ChangeSpeakerIconState(this.speaker_r, talkCfg.effects3);
                this.speaker_r.gameObject.SetActive(true);
            });
        }
        else
        {
            this.speaker_r.gameObject.SetActive(false);
        }

        SetTalkRootInfo(talkCfg);
    }

    /// <summary>
    /// 设置对话框信息
    /// </summary>
    /// <param name="talkCfg"></param>
    private void SetTalkRootInfo(BattleTalkCfg talkCfg)
    {
        if (!string.IsNullOrEmpty(talkCfg.speakerName))
        {
            spekerName.text = talkCfg.speakerName;//讲话者名字
        }
        if (!string.IsNullOrEmpty(talkCfg.speakerTitle))
        {
            spekerTitle.text = talkCfg.speakerTitle;//讲话者称号
        }
        if (!string.IsNullOrEmpty(talkCfg.note))
        {
            curText = GameCenter.mIns.m_LanMgr.GetLan(talkCfg.note);
            textDot = DOTween.To(() => string.Empty, value => this.speakText.text = value, curText, curText.Length * 0.1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                textDot = null;
                OpenInteractList(talkCfg);
            });

            this.btn_next.AddListenerBeforeClear(() =>
            {
                if (textDot != null)//打字动画正在播放
                {
                    textDot.Kill();
                    textDot = null;
                    this.speakText.text = curText;
                }
                else//已经播完
                {
                    if (talkCfg.nextId!= -1)//有下一句
                    {
                        OnTalkStart(talkCfg.nextId);
                    }
                    else if (!string.IsNullOrEmpty(talkCfg.interactionId) && talkCfg.interactionId != "-1")
                    {
                        OpenInteractList(talkCfg);
                    }
                    else
                    {
                        SendSaveInteractID(this.curInteract.interact);
                        //Debug.Log("-------------没了");
                        if (bSend)
                        {
                            //获取关卡信息-需要拿到当前处于的模块才能开始加载
                            JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
                            jsonData["missionid"] = mission;
                            jsonData["star"] = 0;
                            jsonData["randnum"] = randse;
                            GameCenter.mIns.m_NetMgr.SendData(NetCfg.CHAPTER_MISSION_RESULT, jsonData, (eqid, code, content) =>
                            {
                                if (code == 0)
                                {
                                    GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                                    {
                                        GameEventMgr.Distribute(NetCfg.NPC_SAVE_INTERACT_ID.ToString());
                                    });
                                }
                            });
                        }
                        this.Close();
                    }
                    
                }
            });
        }
    }

    /// <summary>
    /// 开启选择列表
    /// </summary>
    private void OpenInteractList(BattleTalkCfg talkCfg)
    {
        if (!string.IsNullOrEmpty(talkCfg.interactionId) && talkCfg.interactionId != "-1")//有列表配置
        {
            for (int i = 0; i < this.btnList.Count; i++)
            {
                this.btnList[i].gameObject.SetActive(false);
            }
            this.btn_next.enabled = false;

            string[] interactionIds = talkCfg.interactionId.Split(';');
            for (int i = 0; i < interactionIds.Length; i++)
            {
                if (i <= this.btnList.Count - 1)
                {
                    RefreshBtnInfo(this.btnList[i], long.Parse(interactionIds[i]));
                }
                else
                {
                    Button btn = GameObject.Instantiate(this.btn_interact, this.btn_List.transform);
                    RefreshBtnInfo(btn, long.Parse(interactionIds[i]));
                    this.btnList.Add(btn);
                }
            }
            this.btn_List.SetActive(true);
        }
    }

    /// <summary>
    /// 刷新按钮信息
    /// </summary>
    /// <param name="btn"></param>
    /// <param name="interactID"></param>
    private void RefreshBtnInfo(Button btn,long interactID)
    {
        BattleInteractCfg interactCfg = BattleCfgManager.Instance.GetBattleInteractByInteract(interactID);
        if (interactCfg != null)
        {
            btn.transform.Find("title").GetComponent<TextMeshProUGUI>().text = interactCfg.caption;// GameCenter.mIns.m_LanMgr.GetLan(interactCfg.caption);//标题
            btn.AddListenerBeforeClear(() =>
            {
                SendSaveInteractID(curInteract.interact);
                this.btn_List.SetActive(false);
                if (!string.IsNullOrEmpty(interactCfg.dialogueId))
                {
                    HandleTalk(interactCfg);
                    this.btn_next.enabled = true;
                }
            });
            btn.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 改变说话者立绘的图片状态
    /// </summary>
    private void ChangeSpeakerIconState(Image icon,int state)
    {
        switch (state)
        {
            case -1://无效果
                icon.color = new Color(1, 1, 1);
                break;
            case 1://淡化（变暗）
                icon.color = new Color(0.7f, 0.7f, 0.7f);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 保存交互id
    /// </summary>
    private void SendSaveInteractID(long interact)
    {
        //获取关卡信息-需要拿到当前处于的模块才能开始加载
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["id"] = interact;
        jsonData["type"] = 2;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.NPC_SAVE_INTERACT_ID, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {
                    GameEventMgr.Distribute(NetCfg.NPC_SAVE_INTERACT_ID.ToString());
                });
            }
        });
    }
}

