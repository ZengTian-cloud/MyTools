using System;
/// <summary>
/// 驱散buff
/// </summary>
public class Buff25: BaseBuff
{
    //移除buff
    private long beRemoveBuff;
    //移除层数
    private int beRemovbeStack;

    public override bool OnChlidStart(params object[] parm)
    {
        InitParam();

        target.buffStackCompent.RemoveOneBuffByBuffID(beRemoveBuff, beRemovbeStack);
        return true;
    }

    private void InitParam()
    {
        string[] funcPm = mainBuff.buffCfg.functionpm.Split(';');
        beRemoveBuff = long.Parse(funcPm[0]);
        beRemovbeStack = int.Parse(funcPm[1]);
    }
}

