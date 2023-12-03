using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcInteractionWidget : BaseWin
{
    public override UILayerType uiLayerType => UILayerType.Pop;

    public override string uiAtlasName => "mainui";

    public override string Prefab => "npcinteractionwidget";

    Transform dialogueRoot;//对话组件根节点

    Transform interactionRoot;//交互按钮根节点

    GameObject interactionBtnPrefab;

    TMP_Text npcName;

    GameObject titleLayout;

    TMP_Text npcTitle;

    TMP_Text content;

    Button mask;//遮罩 or 下一句

    /// <summary>
    /// 当前交互的npc
    /// </summary>
    private Npc currentNpc;
    /// <summary>
    /// 当前进行的交互
    /// </summary>
    private InteractionConfig currentInteraction;
    /// <summary>
    /// 当前进行的对话队列
    /// </summary>
    private Queue<DialogueConfig> currentDialogueQueue = new Queue<DialogueConfig>();
    /// <summary>
    /// 下一组交互事件 (当前对话进行到最后时展示的交互事件)
    /// </summary>
    InteractionConfig[] nextInteractions;
    /// <summary>
    /// 已经完成了当前事件
    /// </summary>
    bool isCompletedCurrentInteraction;
    /// <summary>
    /// 交互Btn
    /// </summary>
    private Dictionary<InteractionConfig, GameObject> interactionBtnDic = new Dictionary<InteractionConfig, GameObject>();
    /// <summary>
    /// Btn回收池
    /// </summary>
    private Stack<GameObject> btnObjPool = new Stack<GameObject>();

    protected override void InitUI()
    {
        base.InitUI();
        dialogueRoot = uiRoot.transform.Find("dialogueRoot");
        npcName = Utils.Find<TMP_Text>(dialogueRoot, "infoLayout/name");
        titleLayout = npcName.transform.Find("titleLayout").gameObject;
        npcTitle = Utils.Find<TMP_Text>(npcName.transform, "titleLayout/title");
        content = Utils.Find<TMP_Text>(dialogueRoot, "content");
        mask = Utils.Find<Button>(dialogueRoot, "mask");
        interactionRoot = uiRoot.transform.Find("interactionRoot");
        interactionBtnPrefab = uiRoot.transform.Find("interactionBtnPrefab").gameObject;

        dialogueRoot.gameObject.SetActive(false);
        btnObjPool = new Stack<GameObject>();
    }
    /// <summary>
    /// 显示交互按钮
    /// </summary>
    public void ShowInteractionBtn(InteractionConfig interaction, Npc npc = null)
    {
        GameObject obj;

        if (btnObjPool.Count > 0)
        
            obj = btnObjPool.Pop();
        else
            obj = Utils.AddChiled(interactionRoot, interactionBtnPrefab);

        for (int i = 0; i < interactionRoot.childCount; i++)
        {   //hierarchy中的层级位置设置 放在最后一个active的gameobject后面
            if (!interactionRoot.GetChild(i).gameObject.activeSelf)
            {
                obj.transform.SetSiblingIndex(i);

                break;
            }
        }
        obj.SetActive(true);

        interactionBtnDic[interaction] = obj;

        SetInteractionBtn(obj.transform, interaction, npc);

        //LayoutRebuilder.ForceRebuildLayoutImmediate(interactionRoot.GetComponent<RectTransform>());
    }

    private void SetInteractionBtn(Transform btn, InteractionConfig interaction, Npc npc = null)
    {
        btn.Find("caption").GetComponent<TMP_Text>().SetText(GameCenter.mIns.m_LanMgr.GetLan(interaction.note));

        btn.Find("icon").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(GetSpritePath(interaction.type));

        btn.GetComponent<Button>().AddListenerBeforeClear(() =>
         {
             if (currentInteraction != null && !isCompletedCurrentInteraction)
             {
                 GameEventMgr.Distribute(GEKey.NpcInteracton_OnInteractionEcd, currentInteraction, true);

                 isCompletedCurrentInteraction = true;
             }
             currentInteraction = interaction;

             isCompletedCurrentInteraction = false;

             if (npc != null)

                 currentNpc = npc;

             GameEventMgr.Distribute(GEKey.NpcInteracton_OnDialogueStart, interaction, currentNpc);//开始对话
         });
    }
    public void ShowDialogue(Queue<DialogueConfig> dialogQueue, InteractionConfig[] interactionList)
    {
        Debug.Log("播放对话");

        mask.enabled = true;

        currentDialogueQueue = dialogQueue;

        dialogueRoot.gameObject.SetActive(true);

        nextInteractions = interactionList;

        RemoveInteractionBtn();

        NextDialogue();
    }

    private void NextDialogue()
    {
        Debug.Log("有效点击了屏幕");

        if (currentDialogueQueue.Count > 0) //下一句
        {
            Debug.Log("播放下一句");

            DialogueConfig cfg = currentDialogueQueue.Dequeue();

            Action callBack = null;

            if (cfg.dropId > 0) //有奖励ID
            {
                Debug.Log("有奖励ID");

                callBack = () =>
                {
                    GameEventMgr.Distribute(GEKey.NpcInteracton_OnReachedRewardNode, currentInteraction, cfg, new Action(() =>
                    {
                        //奖励领取后的回调
                        isCompletedCurrentInteraction = true;
                    }));
                };
            }
            SetDialogue(cfg, callBack);
        }
        else
        {
            Debug.Log("没有下一句了");

            if (nextInteractions != null && nextInteractions.Length > 0)  //有选项 则要在选择之后记录上一事件完成
            {
                Debug.Log("有选项");

                mask.enabled = false;//Tips : 有选项按钮的时候 它就只是mask

                for (int i = 0; i < nextInteractions.Length; i++)
                {
                    ShowInteractionBtn(nextInteractions[i]);
                }
            }
            else
            {
                Debug.Log("没有选项");

                GameEventMgr.Distribute(GEKey.NpcInteracton_OnInteractionEcd, currentInteraction);

                isCompletedCurrentInteraction = true; //这里不管通信结果 都记为完成  表现上要和成功了一样
            }
        }
    }
    private  async void SetDialogue(DialogueConfig cfg, Action callBack = null)
    {
     

        dialogueRoot.Find("infoLayout/name/titleLayout/arrowL").gameObject.SetActive(cfg.showType == 1);
        dialogueRoot.Find("infoLayout/name/titleLayout/arrowR").gameObject.SetActive(cfg.showType == 1);
        dialogueRoot.Find("infoLayout/lineL").gameObject.SetActive(cfg.showType == 1);
        dialogueRoot.Find("infoLayout/lineR").gameObject.SetActive(cfg.showType == 1);

     


        AudioManager.Instance.PlaySingleSound("voice1");


        dialogueRoot.Find("next").gameObject.SetActive(false);

        if (GameCenter.mIns.m_LanMgr.GetLan(cfg.name) == "{name}")
        {
            npcName.SetText(GameCenter.mIns.userInfo.NickName);
        }
        else
        {
            npcName.SetText(GameCenter.mIns.m_LanMgr.GetLan(cfg.name));
        }

        titleLayout.SetActive(!string.IsNullOrEmpty(GameCenter.mIns.m_LanMgr.GetLan(cfg.name1)));
        npcTitle.SetText(GameCenter.mIns.m_LanMgr.GetLan(cfg.name1));

        content.SetText(cfg._Content);

        ContentDoAnimation(callBack);

        await UniTask.DelayFrame(1);

        LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueRoot.Find("infoLayout/name/titleLayout").GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueRoot.Find("infoLayout/name").GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueRoot.Find("infoLayout").GetComponent<RectTransform>());

    }
    //打字机动画
    private void ContentDoAnimation(Action callback = null)
    {
        int count = content.GetTextInfo(content.text).characterCount;

        content.maxVisibleCharacters = -1;

        Tweener tweener = DOTween.To(() => content.maxVisibleCharacters, value => content.maxVisibleCharacters = value, count, count * 0.04f).SetEase(Ease.Linear).OnComplete(() =>
        {
            mask.AddListenerBeforeClear(() => { NextDialogue(); });

            dialogueRoot.Find("next").gameObject.SetActive(true);

            callback?.Invoke();
        });
        mask.AddListenerBeforeClear(() => { tweener.Complete(); });
    }
    /// <summary>
    /// 移除所有交互按钮
    /// </summary>
    private void RemoveInteractionBtn()
    {
        foreach (var item in interactionBtnDic)
        {
            item.Value.SetActive(false);

            btnObjPool.Push(item.Value);
        }
        interactionBtnDic.Clear();
    }
    /// <summary>
    /// 移除交互事件对应的交互按钮
    /// </summary>
    /// <param name="cfg"></param>
    public void RemoveInteractionBtn(InteractionConfig cfg)
    {
        if (interactionBtnDic.TryGetValue(cfg, out GameObject obj))
        {
            obj.SetActive(false);

            btnObjPool.Push(obj);

            interactionBtnDic.Remove(cfg);
        }
    }
    /// <summary>
    /// 衔接新的交互
    /// </summary>
    public void LinkNewInteraction(InteractionConfig interaction, Queue<DialogueConfig> dialogueQueue, InteractionConfig[] interactionList)
    {
        currentInteraction = interaction;

        isCompletedCurrentInteraction = false;

        currentDialogueQueue = dialogueQueue;

        nextInteractions = interactionList;

        NextDialogue();
    }
    /// <summary>
    /// 强制开始交互
    /// </summary>
    public void ForceStarInteraction(Npc npc ,InteractionConfig interaction, Queue<DialogueConfig> dialogueQueue, InteractionConfig[] interactionList)
    {
        currentNpc = npc;

        currentInteraction = interaction;

        isCompletedCurrentInteraction = false;

        currentDialogueQueue = dialogueQueue;

        nextInteractions = interactionList;

        ShowDialogue(dialogueQueue, interactionList);
    }


    private string GetSpritePath(int type)
    {
        switch (type)
        {
            case 1: return "ui_h_icon_duihua_duihua";

            case 2: return "ui_h_icon_duihua_tuijin";

            case 3: return "ui_h_icon_duihua_tuichu";

            case 4: return "ui_h_icon_duihua_fanhui";

            case 5: return "ui_h_icon_duihua_baoxiang";

            case 6: return "ui_h_icon_duihua_diaocha";

            case 7: return "ui_h_icon_duihua_xiufu";

            case 8: return "ui_h_icon_renwu_zhuxian01";

            case 9: return "ui_h_icon_renwu_tongban01";

            case 10: return "ui_h_icon_renwu_maoxian01";

            case 11: return "ui_h_icon_renwu_tansuo01";

            case 12: return "ui_h_icon_duihua_shangcheng";

            default: return "";
        }
    }

    protected override void OnOpen()
    {
        dialogueRoot.gameObject.SetActive(false);
    }


    protected override void OnClose()
    {
        GameEventMgr.Distribute(GEKey.NpcInteracton_OnDialogueEnd, currentNpc);
        RemoveInteractionBtn();
    }

}
