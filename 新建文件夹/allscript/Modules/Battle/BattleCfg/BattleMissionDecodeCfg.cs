using System;

/// <summary>
/// 战斗解密关卡配置表
/// </summary>
public class BattleMissionDecodeCfg
{
	public long mission;//关卡id

	public long mapid;//地图id

	public string passtype1;//通关目标1类型1=交互id完成（1=与，2=或）

	public string passcond1;//通关目标1参数

    public string passtype2;

    public string passcond2;

    public string passtype3;

    public string passcond3;
}

