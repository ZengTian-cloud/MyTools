using System;
using System.Collections.Generic;
using UnityEngine;


public class BaseMonster : BaseObject
{
    //控制器数据
    public MonsterController Controller { get; set; }
    //配置数据
    public MonsterDataCfg cfgdata;

    public BaseMonster(MonsterDataCfg cfgdata, MonsterController Controller, GameObject root, GameObject monster,long guid)
    {
        this.objID = cfgdata.monsterid;
        this.Controller = Controller;
        this.cfgdata = cfgdata;
        this.objType = 2;
        this.prefabObj = root;
        this.roleObj = monster;
        this.GUID = guid;
        bdie = false;
        this.lv = GameCenter.mIns.m_BattleMgr.curMissionParamCfg.monsterlevel + (int)GameCenter.mIns.userInfo.Level;
        this.lv = Mathf.Clamp(this.lv, GameCenter.mIns.m_BattleMgr.curMissionParamCfg.lvmin, GameCenter.mIns.m_BattleMgr.curMissionParamCfg.lvmax);
        this.battleAttr = new Dictionary<long, float>();
        this.aniState = 1;
        this.bFly = this.cfgdata.isfly;
        this.bRecycle = false;
        foreach (var item in this.cfgdata.attrs)
        {
            battleAttr.Add(item.Key, item.Value );
        }
        float curMaxHp = GetBattleAttr(EAttr.HP);
        AttrChange(EAttr.HP, curMaxHp * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(this.lv, 10100103) - 10000) / 10000);
        this.curHp = GetBattleAttr(EAttr.HP);
        //this.curHp = MonsterDataManager.Instance.GetMonsterAttrByMonsterIDAndAttrID(this.objID, (long)EAttr.HP) / 100f;
        //移动速度
    }

    public override void OnStop()
    {
        if (prefabObj!=null && prefabObj.GetComponent<MonsterController>()!= null)
        {
            prefabObj.GetComponent<MonsterController>().isMove = false;
        }

    }

    /// <summary>
    /// 怪物活着走到终点时执行
    /// </summary>
    public virtual void Disappear()
	{
        //移除控制器
        GameObject.Destroy(prefabObj.GetComponent<MonsterController>());
        //回收对象池
        BattlePoolManager.Instance.InPool(ERootType.Monster, prefabObj, objID.ToString());
        this.bRecycle = true;
        //清理对象下特效
        for (int i = 0; i < this.effectRoot.childCount; i++)
        {
            GameObject.Destroy(this.effectRoot.GetChild(i).gameObject);
        }
        //回收血条
        HpSliderManager.ins.OnOneObjDisappear(this);
        BattleMonsterManager.Instance.MonsterDie(GUID);
        BattleEventManager.Dispatch(BattleEventName.battle_monsterDie, prefabObj);

        //主角受伤
        LeaderManager.ins.LeaderHurt(this.cfgdata.crash);
    }

    /// <summary>
    /// 怪物死亡
    /// </summary>
    public override void OnChildDie()
    {
        OnStop();
        BattleEventManager.Dispatch(BattleEventName.battle_monsterDie, prefabObj);
        //死亡动画配置
        AnimatorEventData dataCfg = animatorEventData.death;
        base.CheckAnimationEvent(dataCfg, null, () => {
            //移除怪物列表
            BattleMonsterManager.Instance.MonsterDie(GUID);
            //移除控制器
            GameObject.Destroy(prefabObj.GetComponent<MonsterController>());
            //回收对象池
            BattlePoolManager.Instance.InPool(ERootType.Monster, prefabObj, objID.ToString());
            this.bRecycle = true;
            //清理对象下特效
            for (int i = 0; i < this.effectRoot.childCount; i++)
            {
                GameObject.Destroy(this.effectRoot.GetChild(i).gameObject);
            }
            //回收血条
            HpSliderManager.ins.OnOneObjDisappear(this);
        });
    }
}

