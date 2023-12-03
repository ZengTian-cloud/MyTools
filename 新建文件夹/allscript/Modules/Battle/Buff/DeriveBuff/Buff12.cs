using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 由其他伤害buff传入伤害->计算->打出 传入伤害的buff本次不再执行伤害，由本buff代为执行 
/// </summary>
public class Buff12:BaseBuff
{
    public override bool OnChlidStart(params object[] parm)
    {
        return false;
    }
}

