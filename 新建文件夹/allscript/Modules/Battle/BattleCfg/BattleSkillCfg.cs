using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 战斗技能总表
/// </summary>
public class BattleSkillCfg
{
    public long skillid;//技能ID

    public string describe;//备注

    public string name;//技能名称

    public string note;//战斗描述

    public string note1;//战斗简述

    public string action;//动画调用

    public int skilltype;//技能类型  1-普通攻击 2-战技 3-秘技 4-终结技

    public int issupsill;//是否是支援状态技能（状态栏）

    public int suptime;//支援状态时间

    public int energycost;//算力消耗

    public int skillelement;//技能元素 0-无 1-水 2-火 3-风 4-雷

    public int position;//释放位置 1-角色位置 2-玩家自选

    public int guidetype;//范围类型 0=支援状态(0=自己;1=友方全体) 1=扇形范围(半径; 角度) 2=环形范围(盲区半径; 射程半径) 3=射线(射程) 4=矩形范围(长; 宽) 5=单体锁定(无限) 6=十字(不含中心格半径) 9=左右扫射

    public string guiderange;//范围参数

    public string hightlight;//范围内高亮目标 1-敌人 2-友军 3-自己

    public string effectrangeid;

    public string bulletids;//子弹ID

    public string icon;//技能图标

    public string supicon;

    public string relationskills;//相关加成（仅做技能显示作用）填写天赋、被动ID
}

