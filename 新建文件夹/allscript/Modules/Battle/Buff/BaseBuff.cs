using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// buff基类
/// </summary>
public  class BaseBuff
{
    public long guid;
    public BaseObject target;
    public BuffMain mainBuff;//主buff
    public float remainingtime;//剩余时间
    public int curstackcount;//叠加层数
    public bool bPermanentb;//是否永久

    public List<GameObject> buffEffect;//buff生效时特效

    /// <summary>
    /// 添加buff
    /// </summary>
    /// <param name="parm"></param>
    public bool OnBuffStart(params object[] parm)
	{
        return OnChlidStart(parm);
    }


    /// <summary>
    /// 检测buff效果 行为节点时检测目标身上buff 依次执行效果
    /// </summary>
    /// <param name="buffData"></param>
    public void OnCheckBuff(params object[] parm)
	{
		OnChildCheckBuff(parm);

    }

    public void RemoveBuff(BaseObject target)
    {
        RemoveBuffChild(target);
    }


    public virtual bool OnChlidStart(params object[] parm) { return false; }
    public virtual void RemoveBuffChild(BaseObject target) { }
    public virtual void OnChildCheckBuff(params object[] parm) { }

}




