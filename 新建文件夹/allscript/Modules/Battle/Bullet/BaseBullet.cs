using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹基类 
/// </summary>
public class BaseBullet 
{
    public long bulletUID;//单场战斗内唯一id

    public long bulletID;//子弹id

    public GameObject bulletObj;

    public int bulletType;//子弹类型 1-锁定目标 2-地块范围

    public BaseObject holder;//子弹持有者

    public List<BaseObject> targetObj;//目标（1）

    public Vector3 targetPos;//目标范围（2）

    public BattleSkillBulletCfg battleSkillBulletCfg;
}
