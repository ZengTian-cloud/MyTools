using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Time = UnityEngine.Time;

/// <summary>
/// 英雄控制器
/// </summary>
public class HeroController : MonoBehaviour
{
    public BaseHero baseHero;

    //进入检测范围的对象列表
    public List<BaseObject> checkList = new List<BaseObject>();

    //索敌列表（群体）
    public List<BaseObject> targetList = new List<BaseObject>();

    //攻击间隔计时器
    private float atkTimer;

    public float defultInterval = 1f;

    public GameObject hero;

    //技能数据 控制器只管理普通攻击
    public BattleSkillCfg baseSkill;

    //索敌目标类型判定 1-敌人 2-友军 3-自己
    public string[] targetTypeArray;
    //是否检测怪物
    public bool isCheckMonster;
    //是否检测英雄
    public bool isCheckHero;
    //是否检测自己
    public bool isCheckSelf;

    //范围参数
    private string rangeParm;

    //十字攻击范围的方向参数（特有）
    private int shizistate;


    private int aniState = 0;

    private Vector3 lookAtPos;
 
    /// <summary>
    /// 初始化信息
    /// </summary>
    /// <param name="baseHero"></param>
    /// <param name="heroObj"></param>
    public void Oninit(BaseHero baseHero,GameObject heroObj)
    {
        //注册监听对象死亡
        BattleEventManager.RegisterEvent(BattleEventName.battle_monsterDie, ListenMonserDie, true);
        //监听属性改变 -更新攻速
        BattleEventManager.RegisterEvent(BattleEventName.battle_attrChange, OnAttrChange, true);

        this.baseHero = baseHero;
        this.hero = heroObj;
        InitSkillcfgData();
        defultInterval = 1 / ((this.baseHero.GetBattleAttr(EAttr.Hit_Rate) / 1000) * (1 - this.baseHero.GetBattleAttr(EAttr.Hit_Rate_Per)/10000));
        atkTimer = defultInterval;
    }

    private void Update()
    {
        if (baseHero.isOnSkill)
        {
            if (baseHero.curSkillTarget!= null)
            {
                hero.transform.LookAt(baseHero.curSkillTarget.prefabObj.transform);
            }
        }
        else
        {
            if (baseHero.willDoSkill!= null && baseHero.willDoSkill.Count > 0)
            {
                baseHero.isOnSkill = true;
                if (baseHero.willDoSkill[0].SkillCfg.guidetype != 2 && baseHero.willDoSkill[0].SkillCfg.guidetype != 5)//非单体
                {
                    baseHero.OnSkillBase(baseHero.willDoSkill[0].targetPos, baseHero.willDoSkill[0].SkillCfg,false);
                }
                else
                {
                    if (baseHero.willDoSkill[0].baseObjects[0] != null && !baseHero.willDoSkill[0].baseObjects[0].bRecycle)
                    {
                        baseHero.curSkillTarget = baseHero.willDoSkill[0].baseObjects[0];
                        baseHero.OnSkillBase(baseHero.willDoSkill[0].baseObjects, baseHero.willDoSkill[0].SkillCfg,false);
                    }
                    else
                    {
                        baseHero.isOnSkill = false;
                    }
                }
                baseHero.willDoSkill.RemoveAt(0);
            }
            CheckAtk();
        }

        // 盲区刷新
        if (baseHero.cfgdata.atktype == 2)
        {
            checkDeadZoneTimer += Time.deltaTime;
            if (checkDeadZoneTimer >= checkDeadZoneTimeLimit)
            {
                bool notHasObjInDeadZone = CheckDeadZoneByHero(baseHero);
                if (skillDeadZone == null)
                {
                    LoadSkillDeadZone();
                }
                DoSkillDeadZoneAnim(!notHasObjInDeadZone);
                checkDeadZoneTimer = 0;
            }
        }
    }

    /// <summary>
    /// 普通攻击攻击检测
    /// </summary>
    private void CheckAtk()
    {
        atkTimer += Time.deltaTime;
        //判断检测列表是否有对象
        if (checkList.Count > 0 && GameCenter.mIns.m_BattleMgr.curBattleState == Managers.EBattleState.Start)
        {
            //根据技能类型刷新索敌 对象/列表
            switch (baseSkill.guidetype)
            {
                case 1://近战
                    {
                        if (atkTimer >= defultInterval)
                        {
                            BattleHeroHelp.GetBaseSkillTartget_1(baseHero, checkList);
                            atkTimer = 0;
                        }
                        else
                        {
                            baseHero.roleObj.transform.LookAt(checkList[0].prefabObj.transform);
                        }    
                    }
                    break;
                case 2://单体-范围-盲区
                    {
                        if (atkTimer >= defultInterval)
                        {
                            BattleHeroHelp.GetBaseSkillTartget_2(baseHero, checkList);
                            atkTimer = 0;
                        }
                    }
                    break;
                case 3://十字范围 
                    {
                        if (atkTimer >= defultInterval)
                        {
                            BattleHeroHelp.GetBaseSkillTartget_3(baseHero, checkList);
                            atkTimer = 0;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (baseHero.animatorEventData != null && baseHero.animatorEventData.idle != null)
            {
                if (baseHero.animationController.curName != baseHero.animatorEventData.idle.actname[0].name)
                {
                    baseHero.animationController.PlayAnimatorByName(baseHero.animatorEventData.idle.actname[0].name);
                }
     
            }
            else
            {
                if (baseHero.animationController.curName != "loopidle")
                {
                    if (baseHero.animationController.curName != "loopidle")
                    {
                        baseHero.animationController.PlayAnimatorByName("loopidle");
                    }
                       
                }
            }
            
        }
    }

    /// <summary>
    /// 进入触发器范围
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
       
        if (other.gameObject.tag == "monster" && isCheckMonster)
        {
            if (checkList.Contains(other.gameObject.GetComponent<MonsterController>().monsterData))
            {
                return;
            }
            //检测列表新增
            checkList.Add(other.gameObject.GetComponent<MonsterController>().monsterData);
        }
        else if (other.gameObject.tag == "hero" && isCheckHero)
        {
            if (checkList.Contains(other.gameObject.GetComponent<HeroController>().baseHero))
            {
                return;
            }
            //检测列表新增
            checkList.Add(other.gameObject.GetComponent<HeroController>().baseHero);
        }

    }

    /// <summary>
    /// 离开触发器范围
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        
            if (other.gameObject.tag == "monster" && isCheckMonster)
            {
            if (checkList.Contains(other.gameObject.GetComponent<MonsterController>().monsterData))
            {
                //检测列表移除
                checkList.Remove(other.gameObject.GetComponent<MonsterController>().monsterData);
            }
            }
            else if (other.gameObject.tag == "hero" && isCheckHero)
            {
            if (checkList.Contains(other.gameObject.GetComponent<HeroController>().baseHero))
            {
                //检测列表移除
                checkList.Remove(other.gameObject.GetComponent<HeroController>().baseHero);
            }
            }
       
    }

    /// <summary>
    /// 战斗开始再设置攻击范围的触发器
    /// </summary>
    public void ChangeCollider()
    {
        if(this.gameObject.GetComponent<SphereCollider>() != null)
        {
            rangeParm = baseSkill.guiderange;
            switch (baseSkill.guidetype)
            {
                case 1://圆形范围
                    this.gameObject.GetComponent<SphereCollider>().radius = (int.Parse(rangeParm.Split(';')[0]) / 100f);
                    break;
                case 2://单体-盲区 设置检测范围
                    this.gameObject.GetComponent<SphereCollider>().radius = int.Parse(rangeParm.Split(';')[1])/100f;
                    break;
                case 3:// 射线
                    this.gameObject.GetComponent<SphereCollider>().radius = (int.Parse(rangeParm) / 100f);
                    break;
                default:
                    break;
            }
            
        }
    }

    /// <summary>
    /// 初始化技能数据
    /// </summary>
    private void InitSkillcfgData()
    {
        baseSkill = BattleCfgManager.Instance.GetSkillCfgBySkillID((int)baseHero.cfgdata.baseskill);
        //获得技能索敌 判断索敌目标类型
        targetTypeArray = baseSkill.hightlight.Split('|');
        for (int i = 0; i < targetTypeArray.Length; i++)
        {
            if (targetTypeArray[i] == "1")
            {
                isCheckMonster = true;
            }
            else if(targetTypeArray[i] == "2")
            {
                isCheckHero = true;
            }
            else if (targetTypeArray[i] == "3")
            {
                isCheckSelf = true;
                checkList.Add(baseHero);
            }
        }
    }

    //监听对象死亡
    private void ListenMonserDie(GameObject baseObject)
    {
       BaseObject obj = this.checkList.Find(b => b.prefabObj == baseObject);
        if (obj != null)
        {
            this.checkList.Remove(obj);
        }
    }

    private void OnAttrChange()
    {
        defultInterval = 1 / ((this.baseHero.GetBattleAttr(EAttr.Hit_Rate) / 1000)* (1+this.baseHero.GetBattleAttr(EAttr.Hit_Rate_Per)/10000));
    }


    private void OnDestroy()
    {
        BattleEventManager.RegisterEvent(BattleEventName.battle_monsterDie, ListenMonserDie, false);
        BattleEventManager.RegisterEvent(BattleEventName.battle_attrChange, OnAttrChange, false);
    }

    #region 盲区显示
    // timer
    private float checkDeadZoneTimer = 0;
    // 检查频率
    private float checkDeadZoneTimeLimit = 0.2f;
    // 范围obj
    private GameObject skillDeadZone;
    // 相同状态不反复设置
    private int skillDeadZoneState = -1;
    private async void LoadSkillDeadZone()
    {
        skillDeadZone = await ResourcesManager.Instance.LoadPrefabSync("widget", "skillDeadZone");
        if (skillDeadZone != null)
        {
            skillDeadZone.transform.SetParent(baseHero.roleObj.transform.parent);
            skillDeadZone.transform.localPosition = new Vector3(0, 0.1f, 0);
            skillDeadZone.transform.localRotation = Quaternion.Euler(90, 0, 0);
            skillDeadZone.transform.localScale = Vector3.one;
            skillDeadZone.name = "skillDeadZone_" + baseHero.heroData.heroID;
        }
    }

    private void DoSkillDeadZoneAnim(bool bActive)
    {
        int tempState = bActive ? 1 : 0;
        if (tempState == skillDeadZoneState)
        {
            return;
        }
        skillDeadZoneState = bActive ? 1 : 0;
        SpriteRenderer spriteRenderer = skillDeadZone.GetComponent<SpriteRenderer>();
        spriteRenderer.DOKill();
        Color toColor = spriteRenderer.color;
        string[] rangeArray = baseHero.baseSkillCfgData.guiderange.Split(';');
        spriteRenderer.size = new Vector2(float.Parse(rangeArray[0]) / 130 * 1.3f * 2, float.Parse(rangeArray[0]) / 130 * 1.3f * 2);
        if (bActive)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
            toColor = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.5f);
            skillDeadZone.SetActive(true);
        }
        else
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.5f);
            toColor = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
        spriteRenderer.DOColor(toColor, 1);
    }

    /// <summary>
    /// 检测英雄盲区范围是否没有敌人
    /// </summary>
    /// <param name="baseHero"></param>
    private bool CheckDeadZoneByHero(BaseHero baseHero)
    {
        string[] rangeArray = baseHero.baseSkillCfgData.guiderange.Split(';');
        List<BaseObject> objList = baseHero.Controller.checkList;
        for (int i = 0; i < objList.Count; i++)
        {
            //计算两点距离 判断是否在攻击盲区
            float dis = Vector3.Distance(objList[i].prefabObj.transform.position, baseHero.prefabObj.transform.position);
            if (dis < (float.Parse(rangeArray[0]) / 100))
            {
                return false;
            }
        }
        return true;
    }
    #endregion
}
