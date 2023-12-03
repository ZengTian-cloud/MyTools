using System;

/// <summary>
/// 战斗关卡剧情对话配置
/// </summary>
public class BattleTalkCfg
{
	public long id;

	public long nextId;//衔接文本id

	public string interactionId;//选择组list，对应剧情交互表

	public long dropId;//奖励id

	public string voice;//语音

	public string audio;//音效

	public int delay;//音效播放延时

	public int showType;//文本展示类型

	public string speakerName;//姓名

	public string speakerTitle;//称号

	public string note;

	public string background;//背景

    public int backgroundeffs;//背景特效

    public string picture1;//立绘左

	public int effects1;//立绘效果 -1-无效果 1-淡化

    public string picture2;//立绘中

    public int effects2;//立绘效果 -1-无效果 1-淡化

    public string picture3;//立绘右

    public int effects3;//立绘效果 -1-无效果 1-淡化
}

