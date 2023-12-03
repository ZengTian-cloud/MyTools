using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 战斗技能子弹表
/// </summary>
public class BattleSkillBulletCfg
{
	public string describe;

    public long bulletid;//子弹ID

	public int tracktype;//弹道类型 1-抛物线到脚下 2-直线飞行 3-射线 4-抛物线到头顶

	public string trackoffset;//落点偏移

	public int count;//子弹数量

	public string delay;//延迟ms

	public int speed;//子弹飞行速度

	public string flyres;//子弹飞行资源

	public string logicid;//技能逻辑

	public string flyrespoint;//子弹创建点位

	public string endpoint;//子弹到达点位

    public int disappeardelay;//消失延时
	//public float totalTime;//到达终点的时间，只有范围技能并且子弹散射导致不能同时到达终点时使用
}

