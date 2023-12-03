using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTrapCfgData
{
    // 机关id
    public long trapid;
    //机关类型
    public int subtargettype;
    // 备注
    public string describe;
    // 模型id
    public string modelid;
    // 机关属性
    public string attr;
    // 能否被摧毁（被伤害）0=不能，1=能
    public int harm;
    // 摧毁后结果，-1=无，1=从地图上消失，2=切换机关id
    public int destroy;
    // 摧毁结果参数
    public long destroyparam;
    // 能否通行，0=不能，1=能
    public int pass;
    // 方向，1=上，2=下，3=左，4=右
    public string direction;
    // 检测时机，-1=不检测，1=每0.5s检测
    public int time;
    // 触发条件，1=机关所在格子上有某单位（1=所有怪物，2=飞行怪物，3=非飞行怪物）
    public int cond1;
    // 条件参数
    public string cond1param;
    // 触发结果1，1=变速带类型（行进方向相同时的buffid;不同时的buffid
    public int result1;
    // 结果1参数
    public string param1;

    public int timing;//添加技能卡时机//1-战斗开始时 2-机关被摧毁时 

    public string addplace;//添加参数 1-添加到牌库 2-添加到手牌；数量

    public string addskill;//

    public int remove;//移除方式

    public int special;//机制特殊参数
}
