using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleEventName 
{
    public const string battle_monsterDie = nameof(battle_monsterDie);//怪物死亡
    public const string battle_heroDie = nameof(battle_heroDie);//英雄死亡
    public const string battle_removeCardByHeroDie = nameof(battle_removeCardByHeroDie);//英雄死亡移除卡片

    public const string battle_upskillStar = nameof(battle_upskillStar);//添加支援状态技能
    public const string battle_upskillEnd = nameof(battle_upskillEnd);//支援状态技能结束
    public const string battle_attrChange = nameof(battle_attrChange);//战斗内属性改变
    public const string battle_cardCostChange = nameof(battle_cardCostChange);//战斗内卡牌费用改变
    public const string battle_energyChange = nameof(battle_energyChange);//算力改变


    public const string battle_end = nameof(battle_end);//战斗结束(显示结算界面，未退出战斗场景)
}
