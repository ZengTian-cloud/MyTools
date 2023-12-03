using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// buff类型5：水泡 获得此BUFF的角色Z轴坐标略微提升，播放待机动作，无法攻击，无法移动
/// </summary>
public class Buff5 : BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            target.buffStackCompent.AddOneBuff(this, 0);
            if (target.objType == 2)//怪物
            {
                BaseMonster baseMonster = (BaseMonster)target;
                baseMonster.Controller.isMove = false;
                baseMonster.Controller.isAi = false;
               
                baseMonster.animationController.PlayAnimatorByName(baseMonster.animatorEventData.idle.actname[0].name);
                baseMonster.prefabObj.transform.DOMoveY(0.5f, 1).SetEase(Ease.Linear);
                return true;
            }

        }
        return false;
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
                BaseMonster baseMonster = (BaseMonster)target;
                baseMonster.prefabObj.transform.DOMoveY(0, 1).SetEase(Ease.Linear).OnComplete(() => {
                    baseMonster.Controller.isMove = true;
                    baseMonster.Controller.isAi = true;
                });
            }
        }
        
    }
}

