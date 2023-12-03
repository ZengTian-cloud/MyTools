using System;
/// <summary>
/// 恢复算力
/// </summary>
public class Buff9 : BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        int value = int.Parse(mainBuff.buffCfg.value);
        BattleEventManager.Dispatch(BattleEventName.battle_energyChange, value);
        return true;
    }
}

