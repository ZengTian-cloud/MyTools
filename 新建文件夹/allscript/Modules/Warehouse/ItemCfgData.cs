/// <summary>
/// 物品是配置类
/// </summary>
public class ItemCfgData
{
    public int pid = 0;
    //服务器使用 分类翠芳
    public int pakge = 0;
    //客户端使用 页签
    public int type = 0;
    public string describe = "";
    //名称
    public string name = "";
    //描述
    public string note = "";
    //详细描述
    public string note2 = "";
    //图标
    public string icon = "";
    //品质
    public int quality = 0;
    //来源途径
    public string source = "";
    //来源id
    public string sourceid = "";

    /// <summary>
    /// 构造函数
    /// </summary>
    public ItemCfgData() { }

    public override string ToString()
    {
        return $"item:pid:{pid}-type:{type}-pakge:{pakge}-quality:{quality}-name:{name}";
    }
}
