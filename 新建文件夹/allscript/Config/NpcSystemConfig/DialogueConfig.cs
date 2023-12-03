using System.Text.RegularExpressions;

public class DialogueConfig : Config
{
    /// <summary>
    /// 下一句话的ID
    /// </summary>
    public readonly long nextId;
    /// <summary>
    /// 交互选项
    /// </summary>
    public readonly string interactionId;
    /// <summary>
    /// 奖励Id
    /// </summary>
    public readonly long dropId;
    /// <summary>
    /// 文本内容
    /// </summary>
    public readonly string note;
    /// <summary>
    /// 类型 （对话/内心独白/...）
    /// </summary>
    public readonly int showType;
    /// <summary>
    /// 对话的npc名称
    /// </summary>
    public readonly string name;
    /// <summary>
    /// 对话的npc称号
    /// </summary>
    public readonly string name1;

    private long[] _interactionId;

    public long[] InteractionId
    {
        get
        {
            if (_interactionId == null || _interactionId.Length==0)
            {
                if (interactionId.Equals("-1") || string.IsNullOrEmpty(interactionId))
                {
                    return null;
                }
                string[] str = interactionId.Split(';');

                _interactionId=new long[str.Length];

                for (int i = 0; i < str.Length; i++)
                {
                    _interactionId[i] = long.Parse(str[i]);
                }
            }
            return _interactionId;
        }
    }
    private string _content;

    public string _Content
    {
        get
        {
            if (string.IsNullOrEmpty(_content))
            {
                _content = Regex.Replace(GameCenter.mIns.m_LanMgr.GetLan(note), "{.*?}", GameCenter.mIns.userInfo.NickName); 
            }
            return _content;
        }
    }
}
