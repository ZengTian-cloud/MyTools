 using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;
using DG.Tweening;
/// <summary>
/// 战斗对象基类   英雄/怪物继承此类
/// </summary>
public class BaseObject
{
    public GameObject prefabObj { get; set; }//root节点

    public GameObject roleObj { get; set; }//模型预制

    public GameObject skillRange { get; set; }//攻击范围指示器

    public long objID { get; set; }//对象id 英雄id/怪物id

    public long GUID { get; set; }//战斗内唯一id

    public int lv { get; set; }

    public float curHp { get; set; }//当前血量

    public bool isOnSkill { get; set; }//是否正在释放技能

    public BaseObject curSkillTarget { get; set; }//当前技能目标

    public BaseObject PriorityTarget { get; set; }//优先级目标（无视索敌，优先级最高，一般用于嘲讽等强制改变攻击目标的情况）

    public float skillTotalDurtion { get; set; }//当前技能表现总持续时间（用于释放技能的共cd）

    public float skillDurtion { get; set; }//当前基技能表现已持续时间（用于释放技能的共cd）

    public int objType { get; set; }//对象类型 1-英雄 2-怪物

    public int aniState { get; set; }//动画状态参数

    public int bFly { get; set; }//是否飞行 0=否  1=是

    public bool bImmunedDeath { get; set; }//是否可免疫死亡

    public bool bRecycle { get; set; }//是否被回收

    public List<SkillDataAgain> willDoSkill { get; set; }//预存储技能释放列表 当英雄不处于释放技能并且可释放技能时，优先依次释放该列表的技能

    public V2 rolePoint { get; private set; }//当前对象所在的位置 对应地图表中的index

    private Dictionary<long, ShieldData> dicShield = new Dictionary<long, ShieldData>();//护盾列表 k-buffguid  根据护盾值从大到小排序

    public BaseObject()
    {

    }

    /// <summary>
    /// 设置站位台坐标
    /// </summary>
    public void SetRolePoint(V2 point)
    {
        this.rolePoint = point;
    }

    /// <summary>
    /// 特效节点
    /// </summary>
    private Transform _effectRoot;
    public Transform effectRoot { get {
            if (_effectRoot == null)
            {
                _effectRoot = roleObj.transform.Find("effectRoot");
                if (_effectRoot == null)
                {
                    _effectRoot = new GameObject("effectRoot").transform;
                    _effectRoot.SetParent(roleObj.transform);
                    _effectRoot.transform.localPosition = Vector3.zero;
                    _effectRoot.transform.localRotation = Quaternion.Euler(Vector3.zero);
                }
            }
            return _effectRoot;
        }
    }

    /// <summary>
    /// buff管理组件
    /// </summary>
    private BuffStackCompent _buffstackCompent;
    public BuffStackCompent buffStackCompent {
        get {
            if (_buffstackCompent == null && prefabObj != null)
            {
                _buffstackCompent = prefabObj.GetOrAddCompoonet<BuffStackCompent>();
                _buffstackCompent.baseObj = this;
            }
            return _buffstackCompent;
        }
    }

    /// <summary>
    /// 动画动画控制器
    /// </summary>
    private AnimationController _animationController;
    public AnimationController animationController {//动画控制器
        get
        {
            if (_animationController == null)
            {
                _animationController = prefabObj.GetComponentInChildren<AnimationController>();
            }
            return _animationController;
        }
    }

    /// <summary>
    /// 模型节点访问组件
    /// </summary>
    private FBXBindBonesHelper _pointHelper;
    public FBXBindBonesHelper pointHelper {
        get {
            if (_pointHelper == null)
            {
                _pointHelper = prefabObj.GetComponentInChildren<FBXBindBonesHelper>();
                if (_pointHelper == null)
                {
                    Debug.LogError($"角色{objID}的模型获取挂点组件为空，请检查！");
                }
            }
            return _pointHelper;
        }
    }

    /// <summary>
    /// 特效管理组件
    /// </summary>
    private EffectStackCompent _effectStackCompent;
    public EffectStackCompent effectStackCompent {
        get {
            if (_effectStackCompent == null)
            {
                _effectStackCompent = prefabObj.GetOrAddCompoonet<EffectStackCompent>();
                _effectStackCompent.Oninit(this.GUID);
            }
            return _effectStackCompent;
        }
    }

    /// <summary>
    /// 天赋被动管理组件
    /// </summary>
    private TalentCompent _talentCompent;
    public TalentCompent talentCompent {
        get {
            _talentCompent = null;
            if (_talentCompent == null && prefabObj != null)
            {
                _talentCompent = prefabObj.GetOrAddCompoonet<TalentCompent>();
                _talentCompent.baseObject = this;
            }
            return _talentCompent;
        }
    }

    /// <summary>
    /// 动画事件配置数据
    /// </summary>
    public AnimatorEventDataCfg animatorEventData
    {
        get
        {
            if (objType == 1)//英雄
            {
                return AnimatorCfgManager.ins.GetAnimatorCfgByObjID(objID);
            }
            else if (objType == 2)//怪物取模型id
            {
                return AnimatorCfgManager.ins.GetAnimatorCfgByObjID(MonsterDataManager.Instance.GetMonsterCfgByMonsterID(objID).modelid);
            }
            else
            {
                return null;
            }
        }
    }

    //战斗内属性（会改变）
    public Dictionary<long, float> battleAttr;

    public float GetBattleAttr(long attrID)
    {
        if (battleAttr != null && battleAttr.ContainsKey(attrID))
        {
            return battleAttr[attrID];
        }
        return 0;
    }
    public float GetBattleAttr(EAttr attr)
    {
        return GetBattleAttr((long)attr);
    }

    public bool HasHP()
    {
        return battleAttr.ContainsKey((long)EAttr.HP);
    }

    /// <summary>
    /// 改变对象战斗内属性
    /// </summary>
    /// <param name="attrid">属性id</param>
    /// <param name="value">改变值</param>
    public void AttrChange(long attrid, float value)
    {

        if (battleAttr != null)
        {
            if (!battleAttr.ContainsKey(attrid))
            {
                battleAttr.Add(attrid, 0);
            }
            battleAttr[attrid] = battleAttr[attrid] + value;
        }
        BattleLog.Log("ObjID:", this.objID, ",属性改变:", (EAttr)attrid, ",变换值:", value, ",变换后:", battleAttr[attrid]);
        BattleEventManager.Dispatch(BattleEventName.battle_attrChange);
    }

    /// <summary>
    /// 改变对象战斗内属性
    /// </summary>
    /// <param name="attr">属性枚举</param>
    /// <param name="value">改变值</param>
    public void AttrChange(EAttr attr, float value)
    {
        AttrChange((long)attr, value);
    }

    



    /// <summary>
    /// 普通攻击时
    /// </summary>
    /// <param name="objList">目标列表</param>
    /// <param name="skillCfg">技能配置</param>
    /// <param name="action">动作</param>
    /// <param name="range">范围参数 有则复算</param>
    /// <param name="state"></param>
    public void OnAtkBase(List<BaseObject> objList, string range = null, int state = 0)
    {
        if (this.bRecycle)//对象已被回收
        {
            return;
        }
        //快照记录，该值参与计算
        BaseObject computeBase = CreatNewBaseByCompute();

        if (this.objType == 1)//英雄
        {
            BaseHero baseHero = (BaseHero)this;
            //普通攻击动画事件配置
            if (animatorEventData != null)
            {
                //获得普通攻击动画事件配置
                AnimatorEventData attack = AnimatorCfgManager.ins.GetAnimatorEventDataByType(animatorEventData, baseHero.baseSkillCfgData.action);
                if (attack == null)
                {
                    BattleLog.LogError($"未找到id为{objID}的配置文件中找到{baseHero.baseSkillCfgData.action}事件，请检查！");
                }
                //是否需要二次验算
                if (range == null)
                {
                    CheckAnimationEvent(attack, () => { BaseAtkOnce(objList, baseHero.baseSkillCfgData, computeBase); });
                }
                else
                {

                    CheckAnimationEvent(attack, () => {
                        BaseHero self = (BaseHero)this;
                        if (self.cfgdata.atktype == 3)//普通攻击仅类型3需要根据攻击时方向进行一次验算
                        {
                            List<BaseObject> targetList = targetList = SkillManager.ins.GetBaseSkillTarget_3_yansuan(this.prefabObj, objList, range, state);
                            BaseAtkOnce(targetList, baseHero.baseSkillCfgData, computeBase);
                        }
                        else
                        {
                            int curState;
                            List<BaseObject> targetList = self.GetBaseSkillMonsterList(self.Controller.checkList, range, out curState);
                            BaseAtkOnce(targetList, baseHero.baseSkillCfgData, computeBase);

                        }
                    });
                }

            }
            else
            {
                BaseAtkOnce(objList, baseHero.baseSkillCfgData, computeBase);
            }

        }


    }

    /// <summary>
    /// 普通攻击一次
    /// </summary>
    /// <param name="objList"></param>
    /// <param name="skillCfg"></param>
    /// <param name="computeBase">快照数据</param>
    private void BaseAtkOnce(List<BaseObject> objList, BattleSkillCfg skillCfg, BaseObject computeBase)
    {
        List<BattleSkillBulletCfg> bulletCfgs = new List<BattleSkillBulletCfg>();
        string[] bullets = skillCfg.bulletids.Split('|');
        for (int i = 0; i < bullets.Length; i++)
        {
            bulletCfgs.Add(BattleCfgManager.Instance.GetBulletCfg(long.Parse(bullets[i])));
        }

        if (bulletCfgs.Count > 0)
        {
            CreatBullet(bulletCfgs, objList, false, skillCfg, true, computeBase);
        }
        if (this.talentCompent != null)
        {
            this.talentCompent.OnBaseSkill(objList);
        }
    }



    /// <summary>
    /// 释放技能时(索敌)
    /// </summary>
    /// <param name="objList">目标列表</param>
    /// <param name="isinitiative">是否主动释放</param>
    public void OnSkillBase(List<BaseObject> objList, BattleSkillCfg battleSkillCfg, bool isinitiative = true)
    {
        //快照记录，该值参与计算
        BaseObject computeBase = CreatNewBaseByCompute();
        //释放技能后（脱手后触发）
        if (buffStackCompent != null)
            this.buffStackCompent.OnLastDoSkill(battleSkillCfg);
        GameCenter.mIns.m_BattleMgr.baseGod.buffStackCompent.OnLastDoSkill(battleSkillCfg, this);
        //技能表现
        AnimatorEventData eventData = AnimatorCfgManager.ins.GetAnimatorEventDataByType(animatorEventData, battleSkillCfg.action);
        if (animatorEventData != null && eventData != null)
        {

            if (eventData != null)
            {
                CheckAnimationEvent(eventData, () => { SkillOnece(objList, battleSkillCfg, isinitiative, computeBase); }, null, true);
            }

        }
        else
        {
            SkillOnece(objList, battleSkillCfg, isinitiative, computeBase);
        }

    }

    /// <summary>
    /// 释放技能时(手指拖拽技能释放的坐标)
    /// </summary>
    /// <param name="targetPos">目标坐标</param>
    public void OnSkillBase(Vector3 targetPos, BattleSkillCfg battleSkillCfg, bool isinitiative = true)
    {
        //快照记录，该值参与计算
        BaseObject computeBase = CreatNewBaseByCompute();
        //释放技能后（脱手后触发）
        if (buffStackCompent != null)
            this.buffStackCompent.OnLastDoSkill(battleSkillCfg);
        GameCenter.mIns.m_BattleMgr.baseGod.buffStackCompent.OnLastDoSkill(battleSkillCfg, this);

        if (!string.IsNullOrEmpty(battleSkillCfg.effectrangeid))
        {
            ShowSkillEffectRange(targetPos, battleSkillCfg.effectrangeid);
        }

        Vector3 dir = targetPos - this.roleObj.transform.position;
        if (MathF.Abs(dir.x) > 0.5 || MathF.Abs(dir.z) > 0.5) 
        {
            this.roleObj.transform.rotation = Quaternion.LookRotation(dir);
        }
        if (animatorEventData != null)
        {
            AnimatorEventData eventData = AnimatorCfgManager.ins.GetAnimatorEventDataByType(animatorEventData, battleSkillCfg.action);
            if (eventData != null)
            {
                CheckAnimationEvent(eventData, () => { SkillOnece(targetPos, battleSkillCfg, isinitiative, computeBase); }, null, true, targetPos);
            }
        }
        else
        {
            SkillOnece(targetPos, battleSkillCfg, isinitiative, computeBase);
        }
    }

    /// <summary>
    /// 执行一次技能（目标）
    /// </summary>
    /// <param name="objList"></param>
    /// <param name="battleSkillCfg"></param>
    /// <param name="isinitiative">是否主动释放</param>
    /// <param name="computeBase">快照数据</param>
    private void SkillOnece(List<BaseObject> objList, BattleSkillCfg battleSkillCfg, bool isinitiative,BaseObject computeBase)
    {

        if (!this.bRecycle)
        {
            List<BattleSkillBulletCfg> bulletCfgs = new List<BattleSkillBulletCfg>();
            string[] bullets = battleSkillCfg.bulletids.Split('|');
            for (int i = 0; i < bullets.Length; i++)
            {
                bulletCfgs.Add(BattleCfgManager.Instance.GetBulletCfg(long.Parse(bullets[i])));
            }

            if (bulletCfgs.Count > 0)
            {
                //生成子弹
                CreatBullet(bulletCfgs, objList, true, battleSkillCfg, isinitiative, computeBase);
            }
        }
    }

    /// <summary>
    /// 执行一次技能（目标点位）
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="battleSkillCfg"></param>
    /// <param name="isinitiative">是否主动释放</param>
    /// <param name="computeBase">快照数据</param>
    private void SkillOnece(Vector3 targetPos, BattleSkillCfg battleSkillCfg, bool isinitiative, BaseObject computeBase)
    {
        List<BattleSkillBulletCfg> bulletCfgs = new List<BattleSkillBulletCfg>();
        string[] bullets = battleSkillCfg.bulletids.Split('|');
        for (int i = 0; i < bullets.Length; i++)
        {
            bulletCfgs.Add(BattleCfgManager.Instance.GetBulletCfg(long.Parse(bullets[i])));
        }
        if (bulletCfgs.Count > 0)
        {
            CreatBullet(bulletCfgs, targetPos, true, battleSkillCfg, isinitiative, computeBase);
        }
  
    }

    /// <summary>
    /// 根据当前面板生成一个临时对象来参与数值计算（快照机制）
    /// </summary>
    private BaseObject CreatNewBaseByCompute()
    {
        BaseObject newBaseObj = new BaseObject();
        newBaseObj.objID = this.objID;
        newBaseObj.GUID = this.GUID;
        newBaseObj.lv = this.lv;
        newBaseObj.curHp = this.curHp;
        if (this.battleAttr != null)
        {
            newBaseObj.battleAttr = new Dictionary<long, float>(this.battleAttr);
        }
        else
        {
            newBaseObj.battleAttr = new Dictionary<long, float>();
        }
       
        return newBaseObj;
    } 



    /// <summary>
    /// 表现技能范围特效
    /// </summary>
    public void ShowSkillEffectRange(Vector3 targetPos,string effectrangeid)
    {
        this.effectStackCompent.ShowEffectOnPanel(effectrangeid, new Vector3(targetPos.x, 0.01f, targetPos.z));
    }

    /// <summary>
    /// 检测动画事件
    /// </summary>
    /// <param name="eventData">动画事件对应技能配置</param>
    /// <param name="callback">事件回调</param>
    public void CheckAnimationEvent(AnimatorEventData eventData,Action callback,Action aciCallBack = null, bool isSkill =false,Vector3 targetPos = default)
    {
        float atktime = 1 / (GetBattleAttr(EAttr.Hit_Rate) / 1000);
        if (eventData != null)
        {
            //动作列表
            if (eventData.actname != null && eventData.actname.Count > 0)
            {
                long delayTime = 0;
                long totalTime = 0;
                for (int i = 0; i < eventData.actname.Count; i++)
                {
                    if (i == 0)
                    {
                        animationController.PlayAnimatorByName(eventData.actname[i].name);
                    }
                    else
                    {
                        delayTime += eventData.actname[i].anilen;
                        float speed = 1;
                        if (atktime < delayTime && !isSkill)
                        {
                            speed = 1 + (delayTime - atktime) / delayTime;
                        }
                        GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke((int)delayTime, () => { animationController.PlayAnimatorByName(eventData.actname[i].name, speed); });

                    }
                    totalTime = totalTime + eventData.actname[i].anilen; 
                }
                if (aciCallBack != null)
                {
                    GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke((int)totalTime, () =>
                    {
                        aciCallBack.Invoke();
                    });
                }
                if (isSkill)
                {
                    long skilltime = totalTime;
                    DOTween.To(() => skilltime, t => skilltime = t, 0, totalTime / 1000).OnComplete(() => { isOnSkill = false; });

                    skillTotalDurtion = totalTime;
                    skillDurtion = totalTime;
                }
            }
            else
            {
                aciCallBack?.Invoke();
            }

            //特效列表
            if (eventData.effects != null && eventData.effects.Count > 0)
            {
                string effectName;
                for (int i = 0; i < eventData.effects.Count; i++)
                {
                    effectName = eventData.effects[i].effabname;
                    Transform root = this.roleObj.GetComponent<FBXBindBonesHelper>().GetBoneByString(eventData.effects[i].point);
                    Vector3 pos = new Vector3((float)eventData.effects[i].pos.x,(float)eventData.effects[i].pos.y,(float)eventData.effects[i].pos.z);
                    GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke((int)eventData.effects[i].time, () => { this.effectStackCompent.ShowEffect(effectName, root, root.position, pos); });
                }
            }

            //事件节点 每一个节点执行一次攻击逻辑
            if (eventData.events != null && eventData.events.Count > 0)
            {
                for (int i = 0; i < eventData.events.Count; i++)
                {
                    GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke((int)eventData.events[i], () => { callback?.Invoke(); });
                }
            }

            //jump节点
            if (eventData.jump != null && eventData.jump.Count > 0)
            {
                for (int i = 0; i < eventData.jump.Count; i++)
                {
                    Vector3 oldPos = this.roleObj.transform.position;
                    float durtion = ((int)eventData.jump[i].time2 - (int)eventData.jump[i].time1) / 1000f;
                    GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke((int)eventData.jump[i].time1, () => { this.roleObj.transform.DOMove(targetPos, durtion).SetEase(Ease.Linear); });
                    float durtion2 = ((int)eventData.jump[i].time4 - (int)eventData.jump[i].time3) / 1000f;
                    GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke((int)eventData.jump[i].time3, () => { this.roleObj.transform.DOMove(oldPos, durtion2).SetEase(Ease.Linear); });
                }
            }
        }
    }

    /// <summary>
    /// 护盾改变
    /// </summary>
    /// <param name="guid">buffguid</param>
    /// <param name="value">护盾数值</param>
    /// <param name="bAdd">是否是添加</param>
    public void ShieldChange(ShieldData shield,bool bAdd)
    {
        if (this.dicShield != null)
        {
            if (bAdd)
            {
                this.dicShield.Add(shield.buff.guid, shield);
            }
            else
            {
                if (this.dicShield.ContainsKey(shield.buff.guid))
                {
                    this.dicShield.Remove(shield.buff.guid);
                }
            }
            SortDicShield();
            //刷新护盾显示
            HpSliderManager.ins.RefreshShieldSliderActive(this.GUID, this.dicShield.Count > 0);
            //刷新护盾值
            HpSliderManager.ins.RefreshShieldSldierValue(this.GUID, this.dicShield.First().Value.curValue);
        }
    }

    /// <summary>
    /// 护盾受伤
    /// </summary>
    /// <param name="value"></param>
    /// <param name="bAdd"></param>
    public float OnShieldHurt(float value,bool bAdd = false, int damageType = 0, int size = 5)
    {
        float excessValue = 0;

        if (this.dicShield == null || this.dicShield.Count <= 0)
        {
            return value;
        }
        ShieldData shield = this.dicShield.First().Value;
        if (!bAdd)
        {
            if (value > shield.curValue)
            {
                excessValue = value - shield.curValue;
            }
            shield.curValue = Mathf.Clamp(shield.curValue - value, 0, shield.maxValue);
            //刷新护盾值
            HpSliderManager.ins.RefreshShieldSldierValue(this.GUID, shield.curValue/ shield.maxValue);
            DamageTipTool.ins.ShowDamageTip(this.pointHelper.GetBone(FBXBoneType.point_headtop), value, damageType, size);
        }

        if (shield.curValue <= 0)
        {
            BuffManager.ins.RemoveBuff(shield.buff, this);
        }


        return excessValue;
    }

    /// <summary>
    /// 护盾列表排序
    /// </summary>
    public void SortDicShield()
    {
        List<KeyValuePair<long, ShieldData>> tempList = this.dicShield.ToList();
        tempList.Sort((x, y) => y.Value.maxValue.CompareTo(x.Value.maxValue));
        this.dicShield = tempList.ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    /// 受伤调用 
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="damageType">伤害类型</param>
    /// <param name="size">文字尺寸 </param>
    public void OnHurt(float damage,BaseObject atker,int damageType = 0,int size = 5)
    {
        if (bdie)
        {
            return;
        }
        damage = OnShieldHurt(damage);
        if (damage > 0)
        {
            DamageTipTool.ins.ShowDamageTip(this.pointHelper.GetBone(FBXBoneType.point_headtop), damage, damageType, size);
            this.curHp = Mathf.Clamp(this.curHp - damage, 0, GetBattleAttr(EAttr.HP));
            HpSliderManager.ins.RefreshHpSliderValue(GUID, curHp / GetBattleAttr(EAttr.HP));
            this.talentCompent.OnHurt();
            this.buffStackCompent.OnHurtCheck(atker);

            OnChildHurt();
            if (curHp <= 0)
            {
                bdie = true;
                OnDie();
            }
        }
    }

    /// <summary>
    /// 被治疗时调用
    /// </summary>
    /// <param name="value"></param>
    public void OnHeal(float value, int damageType = 0, int size = 5)
    {
        DamageTipTool.ins.ShowDamageTip(this.pointHelper.GetBone(FBXBoneType.point_headtop), value, damageType, size);
        this.curHp = Mathf.Clamp(this.curHp + value, 0, GetBattleAttr(EAttr.HP));
        //todo 刷新血条
        HpSliderManager.ins.RefreshHpSliderValue(GUID, curHp / GetBattleAttr(EAttr.HP));
    }

    //对象是否已经死亡
    public bool bdie = false;

    /// <summary>
    /// 死亡调用
    /// </summary>
    public void OnDie()
    {

        this.talentCompent.OnDie();
        this.buffStackCompent.OnDieCheck(this);
        //检测身上是否有免死类buff
        if (this.bImmunedDeath)
        {
            bdie = false;
            this.bImmunedDeath = false;
            return;
        }
        bdie = true;
        if (this.effectStackCompent!= null)
        {
            this.effectStackCompent.Clear();
        }
        OnChildDie(); 

    }

    /// <summary>
    /// 检测对象身上是否有指定buff
    /// </summary>
    public bool CheckBuff(params long[] buffs)
    {
        return false;
    }


    /// <summary>
    /// 生成子弹(索敌目标)
    /// </summary>
    /// <param name="baseSkillBulletCfgs"></param>
    /// <param name="target">目标列表</param>
    /// <param name="isSkill">是否技能</param>
    /// <param name="battleSkillCfg"></param>
    /// <param name="isinitiative">是否主动释放</param>
    /// <param name="computeBase">快照数据</param>
    public void CreatBullet(List<BattleSkillBulletCfg> baseSkillBulletCfgs, List<BaseObject> target,bool isSkill,BattleSkillCfg battleSkillCfg, bool isinitiative, BaseObject computeBase)
    {
        if (!this.bRecycle)
        {
            for (int i = 0; i < baseSkillBulletCfgs.Count; i++)
            {
                if (baseSkillBulletCfgs[i].tracktype != -1)//不是空子弹
                {
                    if (baseSkillBulletCfgs[i].tracktype == 6)//射线
                    {
                        BulletManager.ins.ShowRayButtle(computeBase,baseSkillBulletCfgs[i], this, isSkill, battleSkillCfg, isinitiative);
                    }
                    else
                    {
                        BulletManager.ins.CreatOneBullet(computeBase,baseSkillBulletCfgs[i], target, this, isSkill, battleSkillCfg, isinitiative);
                    }
                }
                else//直接执行逻辑
                {
                    List<BattleSkillLogicCfg> battleSkillLogicCfgs = SkillManager.ins.GetAllLogicCfgByBullet(baseSkillBulletCfgs[i]);
                    if (battleSkillLogicCfgs != null)
                    {
                        BuffManager.ins.ExecuteBuff(computeBase,target, this, battleSkillLogicCfgs, battleSkillCfg, isinitiative, isSkill);
                    }
                }

            }
        }
        else {
            this.isOnSkill = false;
        }
        
    }

    /// <summary>
    /// 生成子弹（手指拖拽技能坐标）
    /// </summary>
    /// <param name="baseSkillBulletCfgs"></param>
    /// <param name="targetPos"></param>
    /// <param name="callBack"></param>
    public void CreatBullet(List<BattleSkillBulletCfg> baseSkillBulletCfgs, Vector3 targetPos, bool isSkill, BattleSkillCfg battleSkillCfg, bool isinitiative, BaseObject computeBase)
    {
        for (int i = 0; i < baseSkillBulletCfgs.Count; i++)
        {
            float distacne = Vector3.Distance(prefabObj.transform.position, targetPos);
            //预算时间
            float totaltime = distacne / (baseSkillBulletCfgs[i].speed / 100);
            if (baseSkillBulletCfgs[i].tracktype != -1)
            {
                if (baseSkillBulletCfgs[i].tracktype == 6)//射线
                {
                    BulletManager.ins.ShowRayButtle(computeBase,baseSkillBulletCfgs[i], this, isSkill, battleSkillCfg, isinitiative);
                }
                else
                {
                    if (!this.bRecycle)
                    {
                        BulletManager.ins.CreatOneBullet(computeBase,baseSkillBulletCfgs[i], targetPos, this, totaltime, isSkill, battleSkillCfg, isinitiative);
                    }
                    
                }
            }
            else
            {
                List<BattleSkillLogicCfg> battleSkillLogicCfgs = SkillManager.ins.GetAllLogicCfgByBullet(baseSkillBulletCfgs[i]);
                if (battleSkillLogicCfgs != null)
                {
                    BuffManager.ins.ExecuteBuff(computeBase,targetPos, this, battleSkillLogicCfgs, battleSkillCfg, isinitiative, true);
                }
            }
        }
    }

 

    public void AddWillDoSkill(List<BaseObject> target, BattleSkillCfg skillCfg)
    {
        if (this.willDoSkill == null)
        {
            this.willDoSkill = new List<SkillDataAgain>();
        }
        this.willDoSkill.Add(new SkillDataAgain()
        {
            SkillCfg = skillCfg,
            baseObjects = target,
        });
    }

    public void AddWillDoSkill(Vector3 target, BattleSkillCfg skillCfg)
    {
        if (this.willDoSkill == null)
        {
            this.willDoSkill = new List<SkillDataAgain>();
        }
        this.willDoSkill.Add(new SkillDataAgain()
        {
            SkillCfg = skillCfg,
            targetPos = target,
        });
    }



    public virtual void OnChildHurt() { }
    public virtual void OnChildDie() { }
    public virtual void OnStop() { }
}

/// <summary>
/// 再次释放技能数据
/// </summary>
public class SkillDataAgain
{
   public BattleSkillCfg SkillCfg;
   public List<BaseObject> baseObjects;
   public Vector3 targetPos;
}

/// <summary>
/// 释放技能时的监听时间数据
/// </summary>
public class DoSkillEventData
{
    public long skillID;
    public long objID;
}

/// <summary>
/// 护盾数据
/// </summary>
public class ShieldData
{
    public BaseBuff buff;//所属buffguid
    public float maxValue;//最大值
    public float curValue;//当前值

    public ShieldData(BaseBuff buff, float _max, float _cur)
    {
        this.buff = buff;
        this.maxValue = _max;
        this.curValue = _cur;
    }
}

