using System;
using UnityEngine;
using Managers;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 怪物控制器
/// </summary>
public class MonsterController:MonoBehaviour
{
    public BaseMonster monsterData;//怪物数据

    private BattleManager battleManager;

    private bool bForward;//是否正向行走

    private Dictionary<int, int> selfMoveData;//记录自身的行走数据 int-格子point int-进入次数

    private int prevPoint;

    private int curPoint;//当前point

    private int nextPoint;//下一个点位的序号

    private Vector3 curPos;//当前pos

    private Vector3 nextPos;//下一个点位的pos

    private Vector3 targetPos;//目标点位

    public bool isMove = false;//是否运动

    public bool isAi = false;//是否开启ai
    public bool isBreath = false;//是否处于疲劳期，无法执行下一个ai

    private Vector3 selfNormalize;//方向

    public int curPath;//行走路线

    private List<PointData> curPointDatas;//当前路线点位

    public bool isEnd;//是否是终点点位

    private Rigidbody body;

    private float atkInterval;//攻击间隔
    private float atkTimer;//攻击计时器
    private List<BaseHero> depolyHeroList;//场上英雄列表

    private  List<MonsterAIData> monsterAIDataCfg;

    private List<BaseObject> aiTargetObj;//ai索敌列表

   

    private void Awake()
    {

        atkInterval = 5;
        atkTimer = 0;
        depolyHeroList = BattleHeroManager.Instance.depolyHeroList;

        selfMoveData = new Dictionary<int, int>();
        curPointDatas = new List<PointData>();

        battleManager = GameCenter.mIns.m_BattleMgr;

        body = this.gameObject.GetComponent<Rigidbody>();
        if (body == null)
        {
            body = this.gameObject.AddComponent<Rigidbody>();
        }
        body.useGravity = false;


        curPos = this.gameObject.transform.position;

        nextPos = Vector3.zero;
        curPoint = 0;
        isEnd = false;

    }


    //获得当前点位格子的坐标
    public Vector3 GetCurPointPos()
    {
        V3 p = curPointDatas[curPoint].pos;
        return new Vector3((float)p.x, (float)p.y, (float)p.z);
    }

    /// <summary>
    /// 开始运动
    /// </summary>
    public void StartMove()
    {
        ComputeNextPoint();
        StartAI();
        isMove = true;
        if (monsterData.aniState == 1)
        {
            monsterData.animationController.PlayAnimatorByName(monsterData.animatorEventData.move.actname[0].name);
        }
        else if (monsterData.aniState == 2)
        {
            monsterData.animationController.PlayAnimatorByName(monsterData.animatorEventData.move_2.actname[0].name);
        }
    }


    public void StartAI()
    {
        monsterAIDataCfg = MonsterAIManager.ins.GetMonsterAIByAIID(MonsterDataManager.Instance.GetMonsterCfgByMonsterID(monsterData.objID).aiid);
        isAi = true;
        CheckOneAICondition();
    }

    private void Update()
    {
        atkTimer += Time.deltaTime;
        if (atkTimer >= atkInterval)
        {
            atkTimer = 0;
        }
    }

    //计算离自己最近的英雄
    private BaseHero ComputeLatelyHero()
    {
        float dis = 999;
        int index = -1;
        for (int i = 0; i < depolyHeroList.Count; i++)
        {
           float newdis = Vector3.Distance(depolyHeroList[i].prefabObj.transform.position, monsterData.prefabObj.transform.position);
            if (newdis< dis)
            {
                dis = newdis;
                index = i;
            }
        }
        return depolyHeroList[index];
    }

    /// <summary>
    /// 检测怪物状态
    /// </summary>
    public void MonstateStateCheck()
    {
        if (monsterData.buffStackCompent.dicCurBuff.ContainsKey(102))
        {
            for (int i = 0; i < monsterData.buffStackCompent.dicCurBuff[102].Count; i++)
            {
                monsterData.buffStackCompent.dicCurBuff[102][i].OnCheckBuff(monsterData);
            }
        }
    }

    public void ChangeIsMove(bool bMove)
    {
        this.isMove = bMove;
    }

    public void ChangeIsAI(bool bAI)
    {
        this.isAi = bAI;
    }


    private void FixedUpdate()
    {

        if (isMove && !monsterData.isOnSkill)
        {
            monsterData.roleObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            if (monsterData.aniState == 1)
            {
                if (monsterData.animationController.curName != monsterData.animatorEventData.move.actname[0].name)
                {
                    monsterData.animationController.PlayAnimatorByName(monsterData.animatorEventData.move.actname[0].name);
                }
            }
            else if (monsterData.aniState == 2)
            {
                if (monsterData.animationController.curName != monsterData.animatorEventData.move_2.actname[0].name)
                {
                    monsterData.animationController.PlayAnimatorByName(monsterData.animatorEventData.move_2.actname[0].name);
                }
            }
            targetPos = Vector3.MoveTowards(this.transform.position, nextPos, monsterData.GetBattleAttr(EAttr.Speed) / 100f * (1 + monsterData.GetBattleAttr(EAttr.Speed_Per) / 10000) * Time.deltaTime);
            //转向
            if (targetPos - this.transform.position != Vector3.zero)
            {
                var tarqt = Quaternion.LookRotation(targetPos - this.transform.position);
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, tarqt, 5);
            }

            this.transform.position = targetPos;

            // 检查路径格子机关类触发
            if (curPoint < curPointDatas.Count || prevPoint < curPointDatas.Count)
                BattleTrapManager.Instance.CheckTrapPathGridTrigger(curPointDatas[curPoint], monsterData, curPointDatas[prevPoint]);

            if (Vector3.Distance(this.transform.position, nextPos) <= 0.3f && !isEnd) 
            {
                SetNewPoint();
            }
            if (isEnd)
            {
                isMove = false;
                monsterData.Disappear();
                selfMoveData.Clear();
                Destroy(this.gameObject.GetComponent<MonsterController>());
            }
        }

        if (isAi && !isBreath && !monsterData.isOnSkill)
        {
            CheckDOAi();
        }
        if (monsterData.isOnSkill && aiTargetObj != null && aiTargetObj.Count > 0 && aiTargetObj[0] != null) 
        {
            if (aiTargetObj[0].roleObj != null)
            {
                monsterData.roleObj.transform.LookAt(aiTargetObj[0].roleObj.transform);
            }
        }
    }

    /// <summary>
    /// 刷新路线点位
    /// </summary>
    /// <param name="pathIndex">路线下标</param>
    public void RefreshCurPath(int pathIndex)
    {
        if (pathIndex >= battleManager.curMissionData.pathDatas.Count)
        {
            Debug.LogError($"切换路线失败，{battleManager.curMissionData.missionName}没有{pathIndex}路线,请检查！");
            return;
        }
        curPointDatas = battleManager.curMissionData.pathDatas[pathIndex].pointDatas;
        curPath = pathIndex;
    }


    /// <summary>
    /// 添加数据到我的记录
    /// </summary>
    private void AddToMyMoveData(int point)
    {
        if (selfMoveData.ContainsKey(point))
        {
            selfMoveData[point] += 1;
        }
        else
        {
            selfMoveData.Add(point, 1);
        }
    }


    /// <summary>
    /// 设置新的坐标和点位数据
    /// </summary>
    private void SetNewPoint()
    {
        prevPoint = curPoint;
        curPoint = nextPoint;
        AddToMyMoveData(curPoint);
        ComputeNextPoint();

     
    }

    /// <summary>
    /// 计算下一个坐标
    /// </summary>
    private void ComputeNextPoint()
    {
        if (curPoint < curPointDatas.Count && curPointDatas[curPoint] != null)
        {
            List<NextPoint> nextPoints = curPointDatas[curPoint].nextPoints;
            if (nextPoints.Count == 1)
            {
                nextPoint = nextPoints[0].point;
                RefreshNextPoint(nextPoint);
                return;
            }
            else
            {
                for (int i = 0; i < nextPoints.Count; i++)
                {
                    if (selfMoveData.ContainsKey(curPoint))
                    {
                        if (selfMoveData[curPoint] == nextPoints[i].limit)//当前格子进入次数匹配下个格子的条件次数 
                        {
                            nextPoint = nextPoints[i].point;
                            RefreshNextPoint(nextPoint);
                            return;
                        }
                    }               
                }
            }
        }
        isEnd = true;
    }

    /// <summary>
    /// 刷新下一个坐标
    /// </summary>
    /// <param name="point"></param>
    private void RefreshNextPoint(int point)
    {
        nextPos = new Vector3((float)curPointDatas[point].pos.x, 0, (float)curPointDatas[point].pos.z);
    }



    private List<MonsterAIData> checkMonsterAiList;
    //检测一次生效条件 确认一组生效的ai行为
    private void CheckOneAICondition()
    {
        //如果处于疲劳期 不进行ai操作
        if (isBreath)
        {
            checkMonsterAiList = null;
            return;
        }
        checkMonsterAiList = new List<MonsterAIData>();
        MonsterAIData aIData;
        for (int i = 0; i < monsterAIDataCfg.Count; i++)
        {
            aIData = monsterAIDataCfg[i];
            if (!aIData.isCoolDown)
            {
                continue;
            }
            switch (aIData.cfg.condition)
            {
                case -1://无条件
                    checkMonsterAiList.Add(aIData);
                    break;
                case 1://是否拥有某个buff
                    string[] parma = aIData.cfg.param.Split(';');
                    bool ishas = monsterData.buffStackCompent.CheckBuffIsHas(long.Parse(parma[1]));
                    if ((parma[0] == "1" && ishas) || (parma[0] == "2" && !ishas))//需要拥有 or 需要没有
                    {
                        checkMonsterAiList.Add(aIData);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 在生效的ai中筛选一条ai执行
    /// </summary>
    List<MonsterAIData> doAIList = new List<MonsterAIData>();
    MonsterAIData willDOAi;
    private void  CheckDOAi()
    {
        willDOAi = null;
        float importance = 999;
        doAIList.Clear();
        for (int i = 0; i < checkMonsterAiList.Count; i++)
        {
            MonsterAIData aIData = checkMonsterAiList[i];
            switch (aIData.cfg.checktype)
            {
                case 1://固定时间检测一次，是否锁定目标
                    {
                        if (aIData.cfg.skill == 50201232)
                        {

                        }
                        aIData.checkTimer += Time.deltaTime;
                        if (aIData.checkTimer >= aIData.checkInterval)
                        {
                            if (aIData.cfg.skill == 50201232)
                            {

                            }
                            float range = float.Parse(aIData.cfg.checkparam.Split(';')[1])/100;
                            List<BaseObject> target = MonsterAIManager.ins.CheckMonsterAISkillRange(aIData.cfg.skill, range, monsterData);
                            if (target != null && target.Count > 0 && aIData.cfg.importance < importance)
                            {
                                willDOAi = aIData;
                                aiTargetObj = target;
                                importance = aIData.cfg.importance;
                            }
                        }
                    }
                    break;
                case 2://xx行为处于cd时，固定时间检测一次，是否锁定目标
                    {
                        if (MonsterAIManager.ins.GetMonsterAIDataByMoveid(long.Parse( aIData.cfg.checkparam)).isCoolDown)
                        {
                            aIData.checkTimer += Time.deltaTime;
                            if (aIData.checkTimer >= aIData.checkInterval)
                            {
                                float range = float.Parse(aIData.cfg.checkparam.Split(';')[1]) / 100;
                                List<BaseObject> target = MonsterAIManager.ins.CheckMonsterAISkillRange(aIData.cfg.skill, range, monsterData);
                                if (target != null && target.Count > 0 && aIData.cfg.importance < importance)
                                {
                                    willDOAi = aIData;
                                    aiTargetObj = target;
                                    importance = aIData.cfg.importance;
                                }
                            }
                        }     
                    }
                    break;
                case 3://固定时间直接执行一次，无需目标
                    if (aIData.cfg.importance < importance)
                    {
                        willDOAi = aIData;
                        importance = aIData.cfg.importance;
                    }
                    break;
                default:
                    break;
            }     
        }
        if (willDOAi!=null)
        {
            DOAi(willDOAi);
        }
    }


    private BattleSkillCfg aiSkillCfg;//ai技能数据
    /// <summary>
    /// 执行一个ai
    /// </summary>
    /// <param name="aIData"></param>
    private void DOAi(MonsterAIData aIData)
    {
        RefreshAISkillCfg(aIData.cfg.skill);
        monsterData.buffStackCompent.BeforAtkCheck(monsterData);
        if (monsterData.PriorityTarget != null)
        {
            aiTargetObj.Clear();
            aiTargetObj.Add(monsterData.PriorityTarget);
        }
        monsterData.isOnSkill = true;
        if (aiSkillCfg.skillid == 50201233)
        {

        }
        monsterData.OnSkillBase(aiTargetObj, aiSkillCfg);
        isBreath = true;
        GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(aIData.cfg.breath, () => { isBreath = false; });
        aIData.isCoolDown = false;
        GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(aIData.cfg.cooldown, () => { aIData.isCoolDown = true; });
        
    }

    /// <summary>
    /// 刷新当前ai的所有技能配置数据
    /// </summary>
    private void RefreshAISkillCfg(long skillid)
    {
        aiSkillCfg = BattleCfgManager.Instance.GetSkillCfgBySkillID(skillid);
    }


    /// <summary>
    /// 强制位移
    /// </summary>
    /// <param name="targetPathIndex">新路线</param>
    /// <param name="targetPoint">新点</param>
    /// <param name="targetPos">新pos</param>
    /// <param name="hiddenTime">隐藏时间</param>
    public void ForcedDisplacement(int targetPoint, int targetPathIndex ,V3 targetPos, int hiddenTime)
    {
        //需要保证对象生命周期，所以这里隐藏是通过改变层级不被摄像机渲染
        monsterData.prefabObj.layer = 19;//战斗隐藏层

        GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(hiddenTime, () =>
        {
            monsterData.prefabObj.layer = 8;
            RefreshCurPath(targetPathIndex);
            curPoint = targetPoint;
            nextPoint = curPoint;
            this.gameObject.transform.position = new Vector3((float)targetPos.x, (float)targetPos.y, (float)targetPos.z);
            SetNewPoint();
        });
    }
}

