using System;
/// <summary>
/// 战斗关卡参数表
/// </summary>
public class BattleMissionParamCfg
{
	public long missionid;//关卡id

	public string mapid;//sence场景的名字

	public int star1;//一星胜利条件

	public string star1param;//一星条件参数

    public int star2;//二星胜利条件

    public string star2param;//二星条件参数

    public int star3;//三星胜利条件

    public string star3param;//三星条件参数

    public int limit;//上阵限定角色数

    public int quickmode;//客户端判断地图中当前波次怪物全部打死后，是否立即进入下一个出怪节点，1=加速，-1=不加速

    public int hp;//关卡生命

    public int monsterlevel;//怪物等级

    public int factor;//血量系数

    public int atkfactor;//伤害系数

    public int deffactor;//防御系数

    public int crashfactor;//入怪扣血量系数

    public int initialenergy;//初始算力

    public int energyregen;//能量回复系数

    public int cardcd;//基础补牌cd

    public int leadercdrate;//主角技能cd倍率

    public string ramcost;//购买手牌花费系数

    public int lvmin;//最小等级

    public int lvmax;//最大等级

    public long beforemapid;//战斗前交互地图

    public string endbefore;//战斗前的交互结束标记-交互id

    public long aftermapid;//战斗后交互地图

    public string endafter;//战斗后的交互结束标记-交互ID

}

