using Cysharp.Threading.Tasks;
using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

public class NpcInteractionManager : SingletonNotMono<NpcInteractionManager>
{
    /// <summary>
    /// 已经完成的交互ID
    /// </summary>
    private List<long> completedInteractions;
    /// <summary>
    /// 当前触发的npc交互事件
    /// </summary>
    private Dictionary<Npc, InteractionConfig> npcInteractionDic;
    //ui界面引用
    NpcInteractionWidget widget;
    /// <summary>
    /// 取数据 注册事件 
    /// </summary>
    public async UniTask Init()
    {
        await GetCompletedInteraction(() => {
            RegisterEvent();
            npcInteractionDic = new Dictionary<Npc, InteractionConfig>();
        });
        
    }
    private void RegisterEvent()
    {
        GameEventMgr.Register(GEKey.NpcInteracton_OnNpcTriggerEnter, OnNpcTriggerEnter);

        GameEventMgr.Register(GEKey.NpcInteracton_OnNpcTriggerExit, OnNpcTriggerExist);

        GameEventMgr.Register(GEKey.NpcInteracton_OnDialogueStart, OnStartDialogue);

        GameEventMgr.Register(GEKey.NpcInteracton_OnInteractionEcd, OnInteractionEnd);

        GameEventMgr.Register(GEKey.NpcInteracton_OnReachedRewardNode, OnReachedRewardNode);

        GameEventMgr.Register(GEKey.OnUnLoadScene, (args) =>
        {
            if (widget != null)
            {
                widget.Close();
            }
        });
        GameEventMgr.Register(GEKey.OnLeaveMainScene, (args) =>
        {
            if (widget != null)
            {
                widget.Close();
            }
        });
    }

    #region Net
    /// <summary>
    /// 获取已完成的交互
    /// </summary>
    private async UniTask GetCompletedInteraction(Action callback)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();

        Debug.LogError("<color=#ff0000> 开始拉NPC数据 </color>");

        await GameCenter.mIns.m_NetMgr.SendDataSync(NetCfg.NPC_GET_INTERACT_LIST, jsonData, (eqid, code, content) =>
        {
            Debug.LogError("<color=#ff0000> NPC数据返回 </color>");

            if (code == 0 && !string.IsNullOrEmpty(content))
            {
                Debug.Log($"<color=#ff0000> 取得npc交互数据  <color=#00ff00> {content} </color></color>");

                Debug.LogError("<color=#ff0000> 成功拉到了NPC数据 </color>");

                JsonData jd = jsontool.newwithstring(content);

                JsonData idArray = jd["npcIds"];

                completedInteractions = new List<long>();

                for (int i = 0; i < idArray.Count; i++)
                {
                    completedInteractions.Add(idArray[i].ToInt64());
                }
                callback?.Invoke();
            }
            else
            {
                Debug.Log(content);

                GameCenter.mIns.m_UIMgr.PopMsg($"请求超时：code:{code}");

                Debug.LogError("<color=#ff0000> 没有拉到NPC数据 </color>");
            }
        });
    }
    public bool CheckInteractionIsCompleted(long interactionId)
    {
        if (completedInteractions == null)
        {
            return false;
        }
        return completedInteractions.Contains(interactionId);
    }

    /// <summary>
    /// 记录交互完成
    /// </summary>
    private void RecordInteractionComplete(InteractionConfig interaction, Action callBack)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();

        jsonData["id"] = interaction.id;

        jsonData["type"] = 1;

        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.NPC_SAVE_INTERACT_ID, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {
                    completedInteractions.Add(interaction.id);

                    callBack?.Invoke();

                    GameEventMgr.Distribute(GEKey.NpcInteracton_OnInteractionCompleted);
                });
            }
            else
            {
                Debug.Log(content);

                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {
                    GameCenter.mIns.m_UIMgr.PopMsg(content);
                });
            }
        });
    }
    /// <summary>
    /// 领取交互奖励
    /// </summary>
    private void ReceiveInteractionAward(InteractionConfig interaction, DialogueConfig dialogue, Action callBack)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();

        jsonData["id"] = interaction.id;

        jsonData["talkid"] = dialogue.id;

        jsonData["type"] = 1;

        GameCenter.mIns.m_NetMgr.SendData((int)NetCfg.NPC_GET_INTERACT_REWORD, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {
                    completedInteractions.Add(interaction.id);
                    //TODO:解析奖励信息 传到回调里
                    callBack?.Invoke();
                });
            }
            else
            {
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {
                    Debug.Log(content);

                    GameCenter.mIns.m_UIMgr.PopMsg(content);
                });
            }

        });
    }
    #endregion

    #region 交互事件
    /// <summary>
    /// 进入npc交互范围
    /// </summary>
    /// <param name="gEventArgs"></param>
    private void OnNpcTriggerEnter(GEventArgs gEventArgs)
    {
        if (gEventArgs == null) return;

        Npc npc = gEventArgs.args[0] as Npc;

        NpcConfig npcConfig = gEventArgs.args[1] as NpcConfig;

        InteractionConfig cfg = GetNpcInteraction(npcConfig);

        if (cfg == null) return;

        npcInteractionDic[npc] = cfg;

        widget = GameCenter.mIns.m_UIMgr.Open<NpcInteractionWidget>();

        if (npcConfig.interactTriggerType == 1) //点击交互按钮开始交互
        {
            widget.ShowInteractionBtn(cfg, npc);
        }
        else//进入范围自动开始交互
        {
            Queue<DialogueConfig> dialogueQueue = GetDialogueQueue(cfg.id, out InteractionConfig[] interactions);

            widget.ForceStarInteraction(npc, cfg, dialogueQueue, interactions);
        }
    }
    /// <summary>
    /// 退出npc交互范围
    /// </summary>
    /// <param name="gEventArgs"></param>
    private void OnNpcTriggerExist(GEventArgs gEventArgs)
    {
        if (gEventArgs == null) return;

        Npc npc = gEventArgs.args[0] as Npc;

        if (npcInteractionDic.ContainsKey(npc))
        {
            widget.RemoveInteractionBtn(npcInteractionDic[npc]);

            npcInteractionDic.Remove(npc);
        }
        widget.Close();
    }
    /// <summary>
    /// 开始对话
    /// </summary>
    /// <param name="gEventArgs"></param>
    private void OnStartDialogue(GEventArgs gEventArgs)
    {
        if (gEventArgs == null) return;

        InteractionConfig interaction = gEventArgs.args[0] as InteractionConfig;

        Queue<DialogueConfig> dialogueQueue = GetDialogueQueue(interaction.id, out InteractionConfig[] interactionList);

        widget.ShowDialogue(dialogueQueue, interactionList);
    }
    /// <summary>
    /// 到达奖励节点
    /// </summary>
    private void OnReachedRewardNode(GEventArgs gEventArgs)
    {
        if (gEventArgs == null)

            return;

        InteractionConfig interaction = gEventArgs.args[0] as InteractionConfig;

        DialogueConfig dialogue = gEventArgs.args[1] as DialogueConfig;

        Action callback = gEventArgs.args[2] as Action;

        Debug.Log($"<color=#ff0000> 领取 <color=#00FF00>{dialogue.dropId}</color> 奖励</color>");

        ReceiveInteractionAward(interaction, dialogue, () =>
        {
            //回调里传进来奖励信息  展示奖励
            callback?.Invoke();
        });
    }
    /// <summary>
    /// 完成交互
    /// </summary>
    private void OnInteractionEnd(GEventArgs gEventArgs)
    {
        if (gEventArgs == null) return;

        InteractionConfig interaction = gEventArgs.args[0] as InteractionConfig;

        bool isHaveOption = (gEventArgs.args.Length > 1 && (bool)gEventArgs.args[1]);

        if (interaction.recordType == 1) //走对话逻辑记录
        {
            RecordInteractionComplete(interaction, () =>
            {
                Debug.Log($"<color=#ff0000> 记录事件 <color=##00FF00>{interaction.id}</color>完成 </color>");
            });

            TriggerInteractionEvent(interaction, !isHaveOption);
        }
        else if (interaction.recordType == 2)//走触发事件记录  
        {
            TriggerInteractionEvent(interaction, !isHaveOption, true);
        }
        else//不记录
        {
            Debug.Log($"<color=#ff0000> 不记录 <color=##00FF00>{interaction.id}</color> 事件 </color>");

            TriggerInteractionEvent(interaction, !isHaveOption);
        }
    }
    /// <summary>
    /// 触发事件
    /// </summary>
    /// <param name="interaction"></param>
    /// <param name="isNeedRecord"></param>
    private void TriggerInteractionEvent(InteractionConfig interaction, bool isFinishDialogue, bool isNeedRecord = false)
    {
        switch (interaction.interactionEventType)
        {
            case NpcInteractionTriggerEventCondition.None://没有触发事件

                Debug.Log($"<color=#00FF00>没有触发事件</color>");

                break;
            case NpcInteractionTriggerEventCondition.Battle://触发战斗 在战斗胜利后记录事件完成

                Debug.Log($"<color=#ff0000> 进入 <color=#00FF00>{interaction.interactionEventParameter}</color> 战斗关卡</color>");


                GameSceneManager.Instance.EnterBattleScene(interaction.interactionEventParameter);

                //TODO:战斗胜利后记录  加回调或者抛事件

                if (isNeedRecord)
                {
                    Debug.Log($"<color=#ff0000> 记录事件 <color=#00FF00>{interaction.id}</color>完成 </color>");
                }

                break;
            case NpcInteractionTriggerEventCondition.Transfer://切换场景 不记录事件

                NpcMapConfig cfg = GameConfig.Get<NpcMapConfig>(interaction.interactionEventParameter);

                GameSceneManager.Instance.LoadScene(cfg.mapasset, (obj) =>
                {
                    Debug.Log($"<color=#ff0000> 切换 <color=#00FF00>{interaction.interactionEventParameter}</color> 场景</color>");
                });
                break;
            case NpcInteractionTriggerEventCondition.DecryptionGame://解密玩法 在解密成功后记录事件完成

                Debug.Log($"<color=#ff0000> 进入 <color=#00FF00>{interaction.interactionEventParameter}</color> 解密关卡</color>");

                if (isNeedRecord)
                {
                    Debug.Log($"<color=#ff0000> 记录事件 <color=#00FF00>{interaction.id}</color>完成 </color>");
                }
                break;
            case NpcInteractionTriggerEventCondition.LinkInteraction://衔接交互 对话结束时记录 (配1就在对话结束的时候记录 配2就在这里记录 结果相同)

                if (isNeedRecord)
                {
                    RecordInteractionComplete(interaction, () =>
                    {
                        Debug.Log($"<color=#ff0000> 记录事件 <color=##00FF00>{interaction.id}</color>完成 </color>");
                    });
                }
                Debug.Log($"<color=#ff0000> 衔接 <color=#00FF00>{interaction.interactionEventParameter}</color> 交互事件</color>");

                InteractionConfig linkInteraction = GameConfig.Get<InteractionConfig>(interaction.interactionEventParameter);  //TODO : 这里和 OnStartDialogue 非常相似 可以提一个方法出来

                Queue<DialogueConfig> dialogueQueue = GetDialogueQueue(linkInteraction.id, out InteractionConfig[] interactionList);

                widget.LinkNewInteraction(linkInteraction, dialogueQueue, interactionList);

                isFinishDialogue = false;
                break;
            case NpcInteractionTriggerEventCondition.GetTask://接取任务 接到任务后记录

                Debug.Log($"<color=#ff0000> 领取 <color=#00FF00>{interaction.interactionEventParameter}</color> 任务</color>");

                if (isNeedRecord)
                {
                    Debug.Log($"<color=#ff0000> 记录事件 <color=#00FF00>{interaction.id}</color>完成 </color>");
                }
                break;

            default:

                break;
        }
        if (isFinishDialogue) // 结束对话

            widget.Close();
    }
    #endregion

    #region 数据处理
    /// <summary>
    /// 获取靠近NPC时的交互选项
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public InteractionConfig GetNpcInteraction(NpcConfig npc)
    {
        if (npc.defaultInteractionId == -1)
        {
            return null;
        }
        InteractionConfig config;

        if (npc.firstInteractionId > 0 && !completedInteractions.Contains(npc.firstInteractionId))
        {
            config = GameConfig.Get<InteractionConfig>(npc.firstInteractionId);

            if (ConditionCheck.Check(config.showCondition, config.showCheck == 1))
            {
                return config;
            }
        }
        config = GameConfig.Get<InteractionConfig>(npc.defaultInteractionId);
        if (ConditionCheck.Check(config.showCondition, config.showCheck == 1))
        {
            return config;
        }
        return null;
    }
    /// <summary>
    /// 获得一个交互事件的对话队列
    /// </summary>
    /// <param name="interactionId"></param>
    /// <returns></returns>
    public Queue<DialogueConfig> GetDialogueQueue(long interactionId, out InteractionConfig[] interactionList)
    {
        InteractionConfig interactionCfg = GameConfig.Get<InteractionConfig>(interactionId);

        long dialogueId;

        if (interactionCfg.DialogueId.Length > 1)

            dialogueId = interactionCfg.DialogueId[UnityEngine.Random.Range(0, interactionCfg.DialogueId.Length)];
        else
            dialogueId = interactionCfg.DialogueId[0];

        Queue<DialogueConfig> dialogueQueue = new Queue<DialogueConfig>();

        interactionList = null;
        do
        {
            DialogueConfig dialogueCfg = GameConfig.Get<DialogueConfig>(dialogueId);

            dialogueQueue.Enqueue(dialogueCfg);

            dialogueId = dialogueCfg.nextId;

            if (dialogueCfg.InteractionId != null && dialogueCfg.InteractionId.Length > 0)
            {
                interactionList = GetInteractionGroup(dialogueCfg.InteractionId);
            }

        } while (dialogueId > 0);

        return dialogueQueue;
    }
    /// <summary>
    /// 获取NPC对话时的交互选项
    /// </summary>
    /// <param name="interactionIdArray"></param>
    /// <returns></returns>
    public InteractionConfig[] GetInteractionGroup(long[] interactionIdArray)
    {
        List<InteractionConfig> configList = new List<InteractionConfig>();

        for (int i = 0; i < interactionIdArray.Length; i++)
        {
            InteractionConfig cfg = GameConfig.Get<InteractionConfig>(interactionIdArray[i]);

            if (ConditionCheck.Check(cfg.showCondition, cfg.showCheck == 1))
            {
                configList.Add(cfg);
            }
        }
        return configList.ToArray();
    }
    #endregion
}

/// <summary>
/// 交互条件枚举 （NPC是否出现 NPC是否可交互 交互事件是否显示） 
/// </summary>
public enum NpcInteractionConditionType
{
    /// <summary>
    /// 等级
    /// </summary>
    PlayerLevel = 1,
    /// <summary>
    /// 交互事件ID
    /// </summary>
    InteractionId,
    /// <summary>
    /// 任务ID
    /// </summary>
    TaskId,
    /// <summary>
    /// 关卡ID
    /// </summary>
    GameLevelId,
    /// <summary>
    /// 解密ID
    /// </summary>
    DecryptionId,
}
/// <summary>
/// 交互触发事件枚举
/// </summary>
public enum NpcInteractionTriggerEventCondition
{
    None = -1,
    /// <summary>
    /// 战斗
    /// </summary>
    Battle = 1,
    /// <summary>
    /// 传送（切换地图）
    /// </summary>
    Transfer,
    /// <summary>
    /// 解密
    /// </summary>
    DecryptionGame,
    /// <summary>
    /// 衔接交互
    /// </summary>
    LinkInteraction,
    /// <summary>
    /// 领取任务
    /// </summary>
    GetTask,
}