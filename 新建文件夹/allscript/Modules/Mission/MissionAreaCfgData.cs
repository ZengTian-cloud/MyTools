using System;

/// <summary>
/// 关卡区块表
/// </summary>
public class MissionAreaCfgData
{
    public int areaid;//区块id

    public string left;//左侧链接（起始链接点；链接区块id；被链接的节点）

    public string right;//左侧链接（起始链接点；链接区块id；被链接的节点）

    public string up;//左侧链接（起始链接点；链接区块id；被链接的节点）

    public string down;//左侧链接（起始链接点；链接区块id；被链接的节点）

    public int list;//序号

    public int chapter;//章节id

    public int unlock;//区块解锁条件

    public string unlockparam;//区块解锁参数

    public string name;//区块名

    public string size;//区块大小
}

