using LitJson;

public enum EnumContactType
{
    // stranger
    Stranger = 0,
    // friend
    Friend = 1,
    // blacklist
    Blacklist = 2,
    // apply
    Apply = 3,
}

/// <summary>
/// 联系人数据
/// </summary>
public class ContactData
{
    /// 必须
    // 关系
    public int contactType { get; private set; }

    // 帐号id
    public string uuid { get; private set; }
    // 玩家id
    public string roleid { get; private set; }
    // 头像
    public string avatar { get; private set; }
    // 名字
    public string nickname { get; private set; }
    // 等级
    public int level { get; private set; }
    // 渠道
    public string cid { get; private set; }
    // 系统
    public int os { get; private set; }
    // 多语言 cn=中文 en=英文
    public string lan { get; private set; }
    // viplevel
    public int viplevel { get; private set; }

    /// 可选
    // 是否在线 1=在线 0=离线"
    public int line { get; private set; }
    // 最后在线时间戳
    public long lasttime { get; private set; }

    // 最后聊天时间(客户端用)
    public long lastchattime { get; set; }
    // 最后聊天文本(客户端用)
    public string lastchattext{ get; set; }

    // 若需要json构建，这必须有空构造函数!
    public ContactData()
    {

    }

    public ContactData(EnumContactType _contactTyp, JsonData jsonData)
    {
        if (jsonData == null)
        {
            zxlogger.logerror($"Error: use jsondata create contact data error! the jsondata is null!");
            return;
        }

        ContactData cd = JsonMapper.ToObject<ContactData>(JsonMapper.ToJson(jsonData));
        if (cd != null)
        {
            cd.contactType = (int)_contactTyp;
            Paste(cd);
        }
    }

    public ContactData Copy()
    {
        ContactData cd = commontool.DeepCopy<ContactData>(this);
        return cd;
    }

    public bool Paste(ContactData cd)
    {
        if (cd == null)
        {
            zxlogger.logerror($"Error: Paste contact data error! the contact data is null!");
            return false;
        }
        contactType = cd.contactType;
        uuid = cd.uuid;
        roleid = cd.roleid;
        line = cd.line;
        avatar = cd.avatar;
        nickname = cd.nickname;
        level = cd.level;
        cid = cd.cid;
        os = cd.os;
        lan = cd.lan;
        viplevel = cd.viplevel;
        lasttime = cd.lasttime;
        return true;
    }

    public JsonData ToJson()
    {
        JsonData jsonData = JsonMapper.ToObject(JsonMapper.ToJson(this));
        return jsonData;
    }

    public override string ToString()
    {
        return $"contact contactType:{contactType}, uuid:{uuid}, roleid:{roleid}, nickname:{nickname}, level:{level}, line:{line}, lasttime:{lasttime}";
    }

    /*
     * 可能变动的数据:
     contactType-> 特殊处理，需要在不同列表中转换 

     avatar
     name
     lv
     line
     lastOnline
     */
    public void OnContactType(EnumContactType contactType)
    {
        this.contactType = (int)contactType;
    }

    public void OnUpdateAvatar(string avatar)
    {
        this.avatar = avatar;
    }

    public void OnUpdateName(string name)
    {
        this.nickname = nickname;
    }

    public void OnUpdateLv(int lv)
    {
        this.level = level;
    }

    public void OnUpdateLine(int line)
    {
        this.line = line;
    }

    public void OnUpdateLastOnline(long lastOnline)
    {
        this.lasttime = lasttime;
    }
}