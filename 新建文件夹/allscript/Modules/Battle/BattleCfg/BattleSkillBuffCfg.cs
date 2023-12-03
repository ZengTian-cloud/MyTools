using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 技能buff配置
/// </summary>
public class BattleSkillBuffCfg
{
    public string describe;//备注

    public string name;//buff名称

    public string note;//说明

    public long buffid;//buffid

    public long classid;//buff分类 1-治疗 2-减速 3-控制 3-减益 5-燃烧 6-增益

    public int functiontype;//功能类型 1-瞬时伤害 2-改变属性 3-回复生命 4-元素协战 5-水泡 6-持续伤害 7-受击特效 8-反击 10-嘲讽 11-降低消耗
    //12-运算伤害时属性瞬时改变（自己） 13-转换buff 14-动态属性提升 15-中心伤害提升 16-免疫一次死亡 17-回复生命

    public string functionpm;//类型参数

    public string value;//基础数值

    public string growvalueid;//成长数值

    public int count;//激活次数

    public string boomdelay;//爆炸延时 ms

    public int hitdelay;//受击延时 ms

    public int time;//激活时间ms  0-瞬时buff -1-永久

    public int stack;//叠加层数

    public int cover;//叠加形式 0=无法叠加（无法再次添加）1=独立计算（相同BUFFID分别计算）2=效果叠加；剩余持续时间刷新 3=效果叠加；剩余持续时间不刷新
    //4=效果不叠加；剩余持续时间累加 5=效果不叠加；剩余持续时间刷新 6=完全顶替；持续时间刷新

    public string boomres;//爆炸资源

    public string hitres;//受击资源

    public string hitrespoint;//受击资源挂点

    public string buffrespoint;//buff特效挂点

    public string buffres;//buff图标 显示在角色身上

    public string iconincard;//buff图标 显示在技能卡上

    public int showinsup;//进入状态栏
}
