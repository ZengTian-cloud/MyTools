using LitJson;

public enum EnumMailSpecial
{
    // 非特殊邮件
    NonSpecial = 0,
    // 特殊邮件(不可一键删除)
    Special = 1,
}

public enum EnumMailState
{
    // 未读
    UnRead = 0,
    // 已读
    Readed = 1,
    // 已领取
    AlreadyReceived = 2,
    // 未领取
    UnReceived = 3,
}

public class MailData
{
    // 邮件id
    public int id { get; private set; }
    // 标题
    public string title { get; private set; }
    // 发送者name
    public string sender { get; private set; }
    // 显示文本内容
    public string content { get; private set; }
    // 显示物品内容
    public string reward { get; private set; }
    // 特殊文件标记(0=非特殊邮件, 1=特殊邮件(不可一键删除))
    public int special { get; private set; }
    // 发送时间戳
    public long sendtime { get; private set; }
    // 过期时间戳(-1=永久)
    public long expiretime { get; private set; }
    // 物品状态(0=未读, 1=已读, 2=已领取, 3=未领取)
    public int state { get; private set; }

    // 若需要json构建，这必须有空构造函数!
    public MailData()
    {

    }

    public MailData(int id, string title, string sender, string content, string reward, int special, long expire, long sendtime, int state)
    {
        if (id <= 0)
        {
            zxlogger.logerror($"Error: create mail data error! the id <= 0 --> curr id:{id}!");
            return;
        }
        this.id = id;
        this.title = title;
        this.sender = sender;
        this.content = content;
        this.reward = reward;
        this.special = special;
        this.sendtime = sendtime;
        this.expiretime = expire;
        SetState(state);
        // this.state = state;
    }

    public MailData(JsonData jsonData)
    {
        if (jsonData == null)
        {
            zxlogger.logerror($"Error: use jsondata create mail data error! the jsondata is null!");
            return;
        }

        MailData mailData = JsonMapper.ToObject<MailData>(JsonMapper.ToJson(jsonData));
        if (mailData != null)
        {
            Paste(mailData);
        }
    }

    public MailData Copy()
    {
        MailData mailData = commontool.DeepCopy<MailData>(this);
        return mailData;
    }

    public bool Paste(MailData mailData)
    {
        if (mailData == null)
        {
            zxlogger.logerror($"Error: Paste mail data error! the mail data is null!");
            return false;
        }

        id = mailData.id;
        title = mailData.title;
        sender = mailData.sender;
        content = mailData.content;
        reward = mailData.reward;
        special = mailData.special;
        sendtime = mailData.sendtime;
        expiretime = mailData.expiretime;
        // state = mailData.state;
        SetState(state);
        return true;
    }

    public void ChangedMailSpecial(EnumMailSpecial enumMailSpecial)
    {
        if (enumMailSpecial != (EnumMailSpecial)special)
        {
            special = (int)enumMailSpecial;
        }
    }

    public void ChangedMailState(EnumMailState enumMailState)
    {
        if (enumMailState != (EnumMailState)state)
        {
            SetState((int)enumMailState);
            UnityEngine.Debug.Log("~~ ChangedMailState: data:" + ToString());
        }
    }

    public void InitState()
    {
        SetState(state);
    }

    public void SetState(int newState)
    {
        // 非已领取状态，若存在物品，这标记为未领取状态
        if (!string.IsNullOrEmpty(reward) && !"{}".Equals(reward))
        {
            if (newState != (int)EnumMailState.AlreadyReceived)
            {
                newState = (int)EnumMailState.UnReceived;
            }
        }
        // UnityEngine.Debug.Log("~~ SetState: newState:" + newState + " - reward:" + reward);
        state = newState;
    }

    public JsonData ToJson()
    {
        JsonData jsonData = JsonMapper.ToObject(JsonMapper.ToJson(this));
        return jsonData;
    }

    public override string ToString()
    {
        return $"mail id:{id}, title:{title}, sender:{sender}, special:{special}, sendtime:{sendtime}, expiretime:{expiretime}, state:{state}, reward:{reward}, content:{content}";
    }
}

