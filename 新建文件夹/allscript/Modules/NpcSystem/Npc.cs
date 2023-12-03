using DG.Tweening;
using UnityEngine;

public class Npc :MonoBehaviour
{
    SphereCollider trigger;

    private NpcConfig config;

    private bool isHaveTriggered;

    Transform playerTrans;

    void Awake()
    {
        trigger = transform.AddMissingComponent<SphereCollider>();

        trigger.isTrigger = true;
    }
    private void Start()
    {
        RegisterEvent();
    }

    public void InitNpc(NpcConfig cfg)
    {
        this.config = cfg;

        transform.localPosition = cfg.Position;

        transform.localRotation = Quaternion.Euler(cfg.Rotation);

        trigger.radius = cfg.interoperableDistance;
    }

     private void RegisterEvent()
    {
        GameEventMgr.Register(GEKey.NpcInteracton_OnDialogueStart,OnStartDialogue);

        GameEventMgr.Register(GEKey.NpcInteracton_OnDialogueEnd, OnFinishDialogue);

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && IsFaceMe(other.transform))
        {
            Debug.Log($"<color=#ff0000> Player : <color=#26FF00>{other.name}</color> 进入了Npc: <color=#26FF00>{GameCenter.mIns.m_LanMgr.GetLan(config.name)}</color>的交互范围</color>");

            GameEventMgr.Distribute(GEKey.NpcInteracton_OnNpcTriggerEnter,this ,config);

            isHaveTriggered = true;

            playerTrans  = other.transform;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (IsFaceMe(other.transform))
            {
                if (isHaveTriggered) return;

                GameEventMgr.Distribute(GEKey.NpcInteracton_OnNpcTriggerEnter, this , config);

                isHaveTriggered = true;
            }
            else
            {
                if (!isHaveTriggered) return;

                GameEventMgr.Distribute(GEKey.NpcInteracton_OnNpcTriggerExit, this, config);

                isHaveTriggered = false;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log($"<color=#ff0000> Player : <color=#26FF00>{other.name}</color> 退出了Npc: <color=#26FF00>{GameCenter.mIns.m_LanMgr.GetLan(config.name)}</color>的交互范围</color>");

            GameEventMgr.Distribute(GEKey.NpcInteracton_OnNpcTriggerExit, this, config);
        }
    }
    /// <summary>
    /// 开始对话
    /// </summary>
    private void OnStartDialogue(GEventArgs gEventArgs)
    {
        if (gEventArgs == null) return;

        Npc npc = gEventArgs.args[1] as Npc;

        if(npc == this)
        {
            Debug.Log("开始对话  npc响应 转向 + 播放对话动画");
            //响应事件  是否要转向玩家 播放对话动画
            if (config.isFaceToPlayer==1)
            {
                FaceToPlayer();
            }
        }
    }
    /// <summary>
    /// 结束对话
    /// </summary>
    private void OnFinishDialogue(GEventArgs gEventArgs)
    {
        try
        {
            if (gEventArgs == null) return;

            Npc npc = gEventArgs.args[0] as Npc;

            if (npc == this)
            {
                Debug.Log("完成对话  npc还原状态");

                transform.DORotate(config.Rotation, 1);
            }
            isHaveTriggered = false;
        }
        catch (System.Exception ex)
        { 
        }
    }
    /// <summary>
    /// 是否面向我
    /// </summary>
    /// <param name="playerTrans"></param>
    private bool IsFaceMe(Transform playerTrans)
    {
        Vector3 playerToNpcDir = (transform.position - playerTrans.position).normalized;

        float cos = Vector3.Dot(playerToNpcDir, playerTrans.forward);

        return (cos > 0.25f);
    }
    /// <summary>
    /// 转向玩家
    /// </summary>
    private void FaceToPlayer()
    {
        if (playerTrans != null)
        {
            Vector3 targetDirection = playerTrans.position - transform.position;

            targetDirection.y = 0; // 为了之后得到不含Y轴旋转的方向

            Vector3 euler = Quaternion.LookRotation(targetDirection).eulerAngles;

            transform.DORotate(euler, 1);

            //角色的转向 先写在这里  后面挪到player里面

            Vector3 p2nDirection = transform.position - playerTrans.position;

            p2nDirection.y = 0;

            Vector3 euler1 = Quaternion.LookRotation(p2nDirection).eulerAngles;

            playerTrans.DORotate(euler1, 1);
        }
    }
}