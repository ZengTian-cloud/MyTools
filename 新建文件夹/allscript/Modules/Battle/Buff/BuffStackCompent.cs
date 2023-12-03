using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// buff队列管理组件
/// </summary>
public class BuffStackCompent : MonoBehaviour
{
	public BaseObject baseObj;

    public List<BaseBuff> curBuffList = new List<BaseBuff>();

    //当前buff列表，分类( buff类型 0-每帧检测 1-造成伤害时，2-受到伤害时，3-死亡时 4-每秒检测 5-层数改变时检测一次 6-添加buff时检测  7-索敌时检测 11-释放技能时检测（抬手时）
    /// 101 - 修改属性   102-行动限制  103-限时护盾) 
    public Dictionary<int, List<BaseBuff>> dicCurBuff = new Dictionary<int, List<BaseBuff>>();

    private float timer = 1;

    private void Awake()
    {
        BattleEventManager.RegisterEvent(BattleEventName.battle_upskillEnd, RemoveUpSkill, true);
        BattleEventManager.RegisterEvent(BattleEventName.battle_heroDie, RemoveUpSkillbyHerodie, true);
    }

    private void Update()
    {
        RefreshBuffTime();

        //每秒检测
        if (GameCenter.mIns.m_BattleMgr.curBattleState == Managers.EBattleState.Start)
        {
            if (timer >= 1)
            {
                //每秒检测的buff
                if (dicCurBuff.ContainsKey(4) && dicCurBuff[4].Count > 0)
                {
                    for (int i = 0; i < dicCurBuff[4].Count; i++)
                    {
                        dicCurBuff[4][i].OnCheckBuff(baseObj);
                    }
                }
                timer = 0;
            }
            timer += Time.deltaTime;
        }        
    }

    /// <summary>
    /// 造成伤害时检测
    /// </summary>
    /// <param name="hoder"></param>
    /// <param name="atkTarget"></param>
    /// <param name="element">伤害属性</param>
    public void OnElementDamage(BaseObject hoder, BaseObject atkTarget,string element)
    {
        if (dicCurBuff.ContainsKey(1) && dicCurBuff[1].Count > 0)
        {
            for (int i = 0; i < dicCurBuff[1].Count; i++)
            {
                dicCurBuff[1][i].OnCheckBuff(hoder,atkTarget, element);
            }
        }
    }

    /// <summary>
    /// 释放技能后检测
    /// </summary>
    /// <param name="skillCfg">技能配置信息</param>
    /// <param name="holderID">释放者id</param>
    public void OnLastDoSkill(BattleSkillCfg skillCfg,BaseObject skillHolder = null)
    {
        if (dicCurBuff.ContainsKey(11) && dicCurBuff[11].Count > 0 && skillCfg!= null)
        {
            for (int i = 0; i < dicCurBuff[11].Count; i++)
            {
                dicCurBuff[11][i].OnCheckBuff(this.baseObj, skillCfg, skillHolder);
            }
        }
    }


    /// <summary>
    /// 受到伤害时检测
    /// </summary>
    /// <param name="target">攻击者</param>
    public void OnHurtCheck(BaseObject target)
    {
        if (dicCurBuff.ContainsKey(2) && dicCurBuff[2].Count > 0)
        {
            for (int i = 0; i < dicCurBuff[2].Count; i++)
            {
                dicCurBuff[2][i].OnCheckBuff(target);
            }
        }
    }

    /// <summary>
    /// 攻击前检测（索敌时检测）
    /// </summary>
    public void BeforAtkCheck(BaseObject target) 
    {
        if (dicCurBuff.ContainsKey(7) && dicCurBuff[7].Count > 0)
        {
            for (int i = 0; i < dicCurBuff[7].Count; i++)
            {
                dicCurBuff[7][i].OnCheckBuff(target);
            }
        }
    }

    public void OnDieCheck(BaseObject target)
    {
        if (dicCurBuff.ContainsKey(3) && dicCurBuff[3].Count > 0)
        {
            for (int i = 0; i < dicCurBuff[3].Count; i++)
            {
                dicCurBuff[3][i].OnCheckBuff(target);
            }
        }
    }


    /// <summary>
    /// 刷新buff剩余时间
    /// </summary>
    public void RefreshBuffTime()
    {
        if (curBuffList.Count > 0)
        {
            BaseBuff onedata;
            for (int i = 0; i < curBuffList.Count; i++)
            {
                onedata = curBuffList[i];
                if (!onedata.bPermanentb)
                {
                    onedata.remainingtime -= Time.deltaTime;
                    if (onedata.remainingtime <= 0)
                    {
                        //buff结束
                        RemoveOneBuff(onedata);
                    }
                }   
            }
        }
    }




    /// <summary>
    ///  移除n层buff
    /// </summary>
    /// <param name="buff"></param>
    /// <param name="stack"></param>
    public async void RemoveOneBuff(BaseBuff buff, int stack = 1)
    {
        buff.curstackcount -= stack;
        BattleLog.Log($"移除{stack}层buff，buffid:{buff.mainBuff.buffCfg.buffid},移除对象：{this.baseObj.objID},剩余层数：{buff.curstackcount}");
        if (buff.curstackcount <= 0)
        {
            curBuffList.Remove(buff);
            foreach (var item in dicCurBuff)
            {
                if (item.Value.Contains(buff))
                {
                    item.Value.Remove(buff);
                }
            }
            //支援技能
            if (buff.mainBuff.buffCfg.showinsup == 1)
            {
                basebattle bb = GameCenter.mIns.m_UIMgr.Get<basebattle>();
                bb.RemoveUpSkill(buff.mainBuff.skillCfg);
            }
            BuffManager.ins.RemoveBuff(buff, baseObj);
        }
    }

    /// <summary>
    /// 移除一个buff 
    /// </summary>
    /// <param name="buff">buff对象</param>
    /// <param name="stack">层数</param>
    public void RemoveOneBuffByBuffID(BaseBuff buff,int stack = 1)
    {
        BaseBuff data = curBuffList.Find(x => x.mainBuff.buffCfg.buffid == buff.mainBuff.buffCfg.buffid);
        if (data!=null)
        {
            RemoveOneBuff(data, stack);
        }
    }

    /// <summary>
    /// 移除一个buff
    /// </summary>
    /// <param name="buffid"></param>
    /// <param name="stack">层数</param>
    public void RemoveOneBuffByBuffID(long buffid, int stack = 1)
    {
        BaseBuff data = curBuffList.Find(x => x.mainBuff.buffCfg.buffid == buffid);
        if (data != null)
        {
            RemoveOneBuff(data, stack);
        }
    }

    /// <summary>
    /// 移除一个支援状态技能下的所有buff
    /// </summary>
    /// <param name="skillid"></param>
    public void RemoveUpSkill(long skillid)
    {
        if (BuffManager.ins.upSkills.ContainsKey(skillid))
        {
            UpSkill upSkill = BuffManager.ins.upSkills[skillid];
            List<string> buffs = upSkill.buffids;
            for (int i = 0; i < buffs.Count; i++)
            {
                RemoveOneBuffByBuffID(long.Parse(buffs[i]));
            }
        }
    }

    public void RemoveUpSkillbyHerodie(long heroID)
    {
        if (BuffManager.ins.upSkills!= null)
        {
            foreach (var item in BuffManager.ins.upSkills)
            {
                if (item.Value.holder.objID == heroID)
                {
                    GameCenter.mIns.m_UIMgr.Get<basebattle>().RemoveUpSkill(BattleCfgManager.Instance.GetSkillCfgBySkillID(item.Key));
                }
            }
        }
        
    }

    /// <summary>
    /// 刷新一个技能的持续时间
    /// </summary>
    /// <param name="buffData"></param>
    public void RefreshSkillTime(BaseBuff baseBuff)
    {
        BaseBuff buff = curBuffList.Find(x => x.mainBuff.buffCfg.buffid == baseBuff.mainBuff.buffCfg.buffid);
        if (buff != null)
        {
            buff.remainingtime = baseBuff.remainingtime;
        }
        foreach (var item in dicCurBuff)
        {
            BaseBuff _buff = item.Value.Find(x => x.mainBuff.buffCfg.buffid == baseBuff.mainBuff.buffCfg.buffid);
            if (_buff != null)
            {
                _buff.remainingtime = baseBuff.remainingtime;
                break;
            }
        }
    }


    /// <summary>
    /// 检测同buff是否已经存在
    /// </summary>
    /// <param name="buffid"></param>
    public bool CheckBuffIsHas(BaseBuff basebuff)
    {
        bool ishas = false;
        BaseBuff buff = curBuffList.Find(x => x.mainBuff.buffCfg.buffid == basebuff.mainBuff.buffCfg.buffid);
        ishas = buff != null;
        foreach (var item in dicCurBuff)
        {
            BaseBuff _buff = item.Value.Find(x => x.mainBuff.buffCfg.buffid == basebuff.mainBuff.buffCfg.buffid);
            if (_buff != null)
            {
                ishas = true;
                break;
            }
        }
        return ishas;
    }

    /// <summary>
    /// 检测同buff是否已经存在
    /// </summary>
    /// <param name="buffid"></param>
    public bool CheckBuffIsHas(long buffid)
    {
        bool ishas = false;
        BaseBuff buff = curBuffList.Find(x => x.mainBuff.buffCfg.buffid == buffid);
        ishas = buff != null;
        foreach (var item in dicCurBuff)
        {
            BaseBuff _buff = item.Value.Find(x => x.mainBuff.buffCfg.buffid == buffid);
            if (_buff != null)
            {
                ishas = true;
                break;
            }
        }
        return ishas;
    }



    /// <summary>
    /// 添加一个buff
    /// </summary>
    /// <param name="type">
    /// return int ----- 1-改变属性 -1-不改变属性
    public int AddOneBuff(BaseBuff baseBuff,int type)
    {
        //如果是支援状态技能的附属buff并且已经挂载在人物身上 则只刷新持续时间
        if (baseBuff.mainBuff.buffCfg.showinsup == 1 && CheckBuffIsHas(baseBuff))
        {
            RefreshSkillTime(baseBuff);
            //return -1;
        }
        //else
        {
            if (!dicCurBuff.ContainsKey(type))
            {
                dicCurBuff.Add(type, new List<BaseBuff>());
            }
            switch (baseBuff.mainBuff.buffCfg.cover)//叠加
            {
                case 0://无法再次添加
                    {
                        if (!CheckBuffIsHas(baseBuff))
                        {
                            curBuffList.Add(baseBuff);
                            dicCurBuff[type].Add(baseBuff);
                            return 1;
                        }
                        return 0;
                    }
                case 1://独立计算，直接添加
                    {
                        curBuffList.Add(baseBuff);
                        dicCurBuff[type].Add(baseBuff);
                        return 1;
                    }
                case 2://效果叠加，剩余持续时间刷新
                    {
                        if (CheckBuffIsHas(baseBuff))
                        {
                            BaseBuff temp = curBuffList.Find(buff => buff.mainBuff.buffCfg.buffid == baseBuff.mainBuff.buffCfg.buffid);
                            temp.curstackcount = Mathf.Clamp(temp.curstackcount + 1, 1, temp.mainBuff.buffCfg.stack);//叠加一次层数
                            temp.remainingtime = temp.mainBuff.buffCfg.time/1000f;//刷新时间


                            temp = dicCurBuff[type].Find(buff => buff.mainBuff.buffCfg.buffid == baseBuff.mainBuff.buffCfg.buffid);
                            temp.curstackcount = Mathf.Clamp(temp.curstackcount + 1, 1, temp.mainBuff.buffCfg.stack);//叠加一次层数
                            temp.remainingtime = temp.mainBuff.buffCfg.time/1000f;//刷新时间
                        }
                        else
                        {
                            curBuffList.Add(baseBuff);
                            dicCurBuff[type].Add(baseBuff);
                        }
                        return 1;
                    }
                case 3://效果叠加，持续时间不刷新
                    {
                        if (CheckBuffIsHas(baseBuff))
                        {
                            BaseBuff temp = curBuffList.Find(buff => buff.mainBuff.buffCfg.buffid == baseBuff.mainBuff.buffCfg.buffid);
                            temp.curstackcount = Mathf.Clamp(temp.curstackcount + 1, 1, temp.mainBuff.buffCfg.stack);//叠加一次层数

                            temp = dicCurBuff[type].Find(buff => buff.mainBuff.buffCfg.buffid == baseBuff.mainBuff.buffCfg.buffid);
                            temp.curstackcount = Mathf.Clamp(temp.curstackcount + 1, 1, temp.mainBuff.buffCfg.stack);//叠加一次层数
                        }
                        else
                        {
                            curBuffList.Add(baseBuff);
                            dicCurBuff[type].Add(baseBuff);
                        }
                        
                        return 1;
                    }
                case 4://效果不叠加，持续时间累加
                    {
                        if (CheckBuffIsHas(baseBuff))
                        {
                            BaseBuff temp = curBuffList.Find(buff => buff.mainBuff.buffCfg.buffid == baseBuff.mainBuff.buffCfg.buffid);
                            temp.remainingtime += temp.mainBuff.buffCfg.time/1000f;//累加时间

                            temp = dicCurBuff[type].Find(buff => buff.mainBuff.buffCfg == baseBuff.mainBuff.buffCfg);
                            temp.remainingtime += temp.mainBuff.buffCfg.time/1000f;//累加时间
                        }
                        else
                        {
                            curBuffList.Add(baseBuff);
                            dicCurBuff[type].Add(baseBuff);
                            return 1;
                        }
                        return -1;
                    }
                case 5://效果不叠加，持续时间刷新
                    {
                        if (CheckBuffIsHas(baseBuff))
                        {
                            BaseBuff temp = curBuffList.Find(buff => buff.mainBuff.buffCfg.buffid == baseBuff.mainBuff.buffCfg.buffid);
                            temp.remainingtime = temp.mainBuff.buffCfg.time/1000f;//刷新时间

                            temp = dicCurBuff[type].Find(buff => buff.mainBuff.buffCfg.buffid == baseBuff.mainBuff.buffCfg.buffid);
                            temp.remainingtime = temp.mainBuff.buffCfg.time/1000f;//刷新时间
                        }
                        else
                        {
                            curBuffList.Add(baseBuff);
                            dicCurBuff[type].Add(baseBuff);
                            return 1;
                        }
                        return -1;
                    }
                case 6://完全代替
                    if (CheckBuffIsHas(baseBuff))
                    {
                        RemoveOneBuffByBuffID(baseBuff);
                    }
                    curBuffList.Add(baseBuff);
                    dicCurBuff[type].Add(baseBuff);
                    return 1;
                default:
                    return -1;
            }

        }

    }


    private void OnDestroy()
    {
        BattleEventManager.RegisterEvent(BattleEventName.battle_upskillEnd, RemoveUpSkill, false);
        BattleEventManager.RegisterEvent(BattleEventName.battle_heroDie, RemoveUpSkillbyHerodie, false);
    }
}

