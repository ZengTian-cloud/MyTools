using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 战斗技能逻辑配置表
/// </summary>
public class BattleSkillLogicCfg
{

    public string describe;//描述

    public long effectid;//效果ID

	public int position;//释放位置 1-角色位置 2-玩家自选

	public int rangetype;//范围类型 0-全局 1-扇形 2-环形 3-射线 4-矩形

	public string rangepm;//范围参数

	public string targettype;//目标类型 1-敌方 2-友方 3-自己 4-机关

	public string flytarget;//可攻击空中目标 0-地面单位 1-空中单位

	public int filtertype;//筛选参数 0-不筛选 1-指定元素 2-指定职业

    public int flitertypepm;//筛选数量 flitertypepm = 1时，1-水 2-火 3-风

    public int filtercount;//筛选数量

    public string bufferids;//buff数组

	public string effectfunction;//特殊效果

	public string functionpm;//效果参数

	public string conditionid1;//条件A

	public string bufferids1;//满足条件A目标施加buff数组

	public string effectfunction1;//特殊效果 1-子弹 2-effect

	public string functionpm1;//效果参数

    public string conditionid2;//条件B

    public string bufferids2;//满足条件B目标施加buff数组

    public string effectfunction2;//特殊效果 1-子弹 2-effect

    public string functionpm2;//效果参数

}

