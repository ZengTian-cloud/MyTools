using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 战斗天赋技能表
/// </summary>
public class BattleSkillTalentCfg
{
    public long talentid;//天赋ID

    public string describe;//备注

    public string name;

    public string note;//天赋描述

    public int triger;//触发条件 1= 进入战斗后持续监测（每0.5秒检测一次） 2= 战斗开始时触发 3=释放技能后触发 4=伤害计算前触发(元素) 5=伤害计算前触发(技能）

    public string trigerpm;//触发参数

    public long condition;//触发条件

    public int trigerlimit;//触发次数

    public string addbuff;//添加buff

    public string removebuff;//清除buff

    public string skilllevelup;//指定技能等级提高 1-普攻 2-战技 3-秘技 4-终结技

    public string exchangeskill;//修改英雄技能 1-普攻 2-战技 3-秘技 4-终结技

    public string useskill;//释放技能 英雄ID;技能类型

    public string cooldown;//主角技cd减少

    public string attrbonus;//属性增加 属性id;增加值

    public int addenergy;//算力回复

}

