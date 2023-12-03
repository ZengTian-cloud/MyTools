using System;

/// <summary>
/// 禁锢（无法移动）
/// </summary>
public class Buff32 : BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            if (target.objType == 2)
            {
                target.buffStackCompent.AddOneBuff(this, 102);
                ((BaseMonster)target).Controller.MonstateStateCheck();
            }
        }
        return false;
    }

    public override void OnChildCheckBuff(params object[] parm)
    {
        BaseObject target = (BaseObject)parm[0];
        if (target != null && !target.bRecycle)
        {
            if (target.objType == 2)
            {
                ((BaseMonster)target).Controller.ChangeIsMove(false);
            }
        }
    }

    public override void RemoveBuffChild(BaseObject target)
    {
        if (this.buffEffect != null)
        {
            for (int i = 0; i < buffEffect.Count; i++)
            {
                if (buffEffect[i] != null)
                {
                    buffEffect[i].SetActive(false);
                    BattlePoolManager.Instance.InPool(ERootType.Effect, buffEffect[i], buffEffect[i].name);
                }
            }
            buffEffect.Clear();
        }

        if (target != null)
        {
            if (target.objType == 2)
            {
                ((BaseMonster)target).Controller.ChangeIsMove(true);
                ((BaseMonster)target).Controller.MonstateStateCheck();
            }
        }
    }
}

