using System;

/// <summary>
/// 战斗剧情交互表
/// </summary>
public class BattleInteractCfg
{
	public long interact;//交互id

	public string caption;//交互标题

	public int showCheck;//显示逻辑类型 1-与 2-或

	public string showCondition;//显示逻辑列表 类型+参数+判定方式

	public int type;//选项图标

	public long mission;//触发关卡

	public int param;//交互触发类型 1-战斗关卡开始前 2-战斗关卡结算前 3-剧情节点触发

	public string dialogueId;//对话列表 多个对话随机一个

	public int isImportant;//是否需要记录进数据库 1-对话逻辑记录 2-触发事件记录 -1-不记录

}

