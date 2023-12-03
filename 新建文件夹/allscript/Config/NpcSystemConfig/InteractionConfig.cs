

public class InteractionConfig : Config
{
    /// <summary>
    /// 交互说明 // 靠近npc时出现的交互按钮上显示的文字
    /// </summary>
    public readonly string note;
    /// <summary>
    /// 显示条件
    /// </summary>
    public readonly string showCondition;
    /// <summary>
    /// 显示条件判断类型 （与 - 或）
    /// </summary>
    public readonly int showCheck;
    /// <summary>
    /// 类型
    /// </summary>
    public readonly int type;
    /// <summary>
    /// 对话ID组 （有多个的话 等概率随机）
    /// </summary>
    public readonly string dialogueId;
    /// <summary>
    /// 触发事件
    /// </summary>
    public readonly string interactEvent;
    /// <summary>
    /// 记录类型（控制和后台通信的时机） （1 走对话逻辑进行记录  2 走触发事件记录逻辑 3 不记录）
    /// </summary>
    public readonly int recordType;

    private long[] _dialogueId;

    public long[] DialogueId
    {
        get
        {
            if (_dialogueId == null) 
            {
                string[] strArr = dialogueId.Split(';');

                _dialogueId = new long[strArr.Length];

                for (int i = 0; i < strArr.Length; i++) 
                {
                    _dialogueId[i] = long.Parse(strArr[i]);
                }
            }
            return _dialogueId;
        }
    }

    public NpcInteractionTriggerEventCondition interactionEventType
    {
        get => (NpcInteractionTriggerEventCondition)int.Parse(interactEvent.Split(';')[0]);
    }
    public long interactionEventParameter
    {
        get => long.Parse(interactEvent.Split(';')[1]);
    }
}

