using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 战斗机关管理器
/// </summary>
public class BattleTrapManager : SingletonNotMono<BattleTrapManager>
{
    // uid
    public long guidTime = 700000;
    // 关卡id
    private long m_MissionID;
    // 关卡数据
    private MissionData m_MissionData;
    // 关卡创建的机关实例列表
    private List<BattleTrapItem> battleTrapItems = new List<BattleTrapItem>();
    // 关卡需要的机关配置列表
    private List<BattleTrapCfgData> battleTrapCfgDatas = new List<BattleTrapCfgData>();
    // 机关对象的根几点
    private GameObject m_TrapRoot;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="missionID"></param>
    /// <param name="missionData"></param>
    public void Init(long missionID, MissionData missionData)
    {
        m_MissionID = missionID;
        m_MissionData = missionData;
    }

    /// <summary>
    /// 初始化机关对象和本关的空气墙
    /// </summary>
    public void InitObjs()
    {
        // 初始化trap节点
        if (m_TrapRoot == null)
            m_TrapRoot = GameObject.Find("BattleRoot(Clone)/TrapRoot");
        if (m_TrapRoot == null)
        {
            GameObject tr = new GameObject("TrapRoot");
            tr.transform.parent = GameObject.Find("BattleRoot(Clone)").transform;
            tr.transform.localScale = Vector3.one;
            tr.transform.localPosition = Vector3.zero;
            tr.name = "TrapRoot";
            m_TrapRoot = tr;
        }

        InitAirWall();

        if (battleTrapCfgDatas == null || battleTrapCfgDatas.Count <= 0)
        {
            battleTrapCfgDatas = GameCenter.mIns.m_CfgMgr.JsonToListClass<BattleTrapCfgData>("t_skill_trap");
        }
        CreateMissionTraps(m_MissionData);
    }

    /// <summary>
    /// 获取有学血量的机关列表
    /// </summary>
    /// <returns></returns>
    public List<BattleTrapItem> GetTrapObjList()
    {
        List<BattleTrapItem> retItems = battleTrapItems.FindAll(item => item.HasHP());
        return retItems;
    }

    /// <summary>
    /// 获取某个机关的配置
    /// </summary>
    /// <param name="trapId"></param>
    /// <returns></returns>
    public BattleTrapCfgData GetTrapConfig(long trapId)
    {
        return battleTrapCfgDatas.Find(delegate (BattleTrapCfgData cfg)
        {
            if (cfg != null)
                return cfg.trapid == trapId;
            return false;
        });
    }

    /// <summary>
    /// 获取TrapItem
    /// </summary>
    /// <param name="trapId"></param>
    /// <returns></returns>
    public BattleTrapItem GetTrapItem(long trapId)
    {
        return battleTrapItems.Find(delegate (BattleTrapItem item)
        {
            if (item != null)
                return item.battleTrapCfgData.trapid == trapId;
            return false;
        });
    }

    /// <summary>
    /// 创建关卡机关
    /// </summary>
    /// <param name="missionData"></param>
    private void CreateMissionTraps(MissionData missionData)
    {
        Clear();
        // 路径点上的机关
        foreach (PathData pathData in missionData.pathDatas)
        {
            foreach (PointData pd in pathData.pointDatas)
            {
                if (pd.trapId > 0)
                {
                    AddBattleTrapItem(pd.trapId, pd.index, pd.pos,pd.point);
                }
            }
        }
        if (missionData.trapPointList != null)
        {
            // 非路径点上的机关
            foreach (TrapPointData trapPointData in missionData.trapPointList)
            {
                AddBattleTrapItem(trapPointData.trapId, trapPointData.index, trapPointData.pos);
            }
        }
    }

    /// <summary>
    /// 添加一个机关
    /// </summary>
    /// <param name="id"></param>
    /// <param name="index"></param>
    /// <param name="pos"></param>
    private void AddBattleTrapItem(long id, V2 index, V3 pos,int point = -1)
    {
        /*foreach (var item in battleTrapItems)
        {
            if (item.index.x == index.x && item.index.y == index.y)
            {
                return;
            }
        }*/
        if (point > 0)
        {
            BattleTrapItem battleTrapItem = battleTrapItems.Find(x => x.index.x == index.x && x.index.y == index.y);
            if (battleTrapItem == null)
            {
                battleTrapItems.Add(new BattleTrapItem(id, index, pos, m_TrapRoot.transform, guidTime++, point));
            }
        }
        else
        {
            battleTrapItems.Add(new BattleTrapItem(id, index, pos, m_TrapRoot.transform, guidTime++));
        }
       
    }

    /// <summary>
    /// 移除一个机关
    /// </summary>
    /// <param name="battleTrapItem"></param>
    public void RemoveBattleTrapItem(BattleTrapItem battleTrapItem)
    {
        for (var i = battleTrapItems.Count - 1; i >= 0; i--)
        {
            if (battleTrapItems[i] == battleTrapItem)
            {
                battleTrapItems[i].Destroy();
                battleTrapItems.RemoveAt(i);
                break;
            }
        }
    }


    /// <summary>
    /// 设置所有机关的碰撞
    /// </summary>
    /// <param name="b"></param>
    public void SetTrapsBoxColliderActive(bool b)
    {
        for (var i = battleTrapItems.Count - 1; i >= 0; i--)
        {
            Debug.LogError("~~ battleTrapItems[i]:" + battleTrapItems[i].trapObj.name);
           battleTrapItems[i].SetBoxColliderActive(b);
        }
    }

    /// <summary>
    /// 添加一个受机关影响的战斗对象到机关item缓存中
    /// </summary>
    /// <param name="index"></param>
    /// <param name="baseObject"></param>
    /// <param name="battleTrapCfgData"></param>
    /// <param name="doTriggerResult"></param>
    private void AddBaseObjToBattleTrapItem(V2 index, BaseObject baseObject, BattleTrapCfgData battleTrapCfgData, string doTriggerResult)
    {
        foreach (var item in battleTrapItems)
        {
            if (item.index.x == index.x && item.index.y == index.y)
            {
                item.AddCacheTrapDoObj(baseObject, battleTrapCfgData, doTriggerResult);
                return;
            }
        }
    }

    /// <summary>
    /// 刷新一个受机关影响的战斗对象在机关item缓存中(可以是从一个机关的缓存列表到另一个机关的缓存列表)
    /// </summary>
    /// <param name="index"></param>
    /// <param name="baseObject"></param>
    /// <param name="battleTrapCfgData"></param>
    /// <param name="newv2"></param>
    private void RefrehBaseObjFromBattleTrapItem(V2 index, BaseObject baseObject, BattleTrapCfgData battleTrapCfgData, V2 newv2)
    {
        TrapDoBattleObj old = null;
        foreach (var item in battleTrapItems)
        {
            if (item.index.x == index.x && item.index.y == index.y)
            {
                old = item.GetCacheTrapDoObj(baseObject);
                if (old != null)
                {
                    AddBaseObjToBattleTrapItem(newv2, baseObject, battleTrapCfgData, old.doTriggerResult);
                };

                item.RemoveCacheTrapDoObj(baseObject);
                return;
            }
        }
    }

    /// <summary>
    /// 移除一个受机关影响的战斗对象在机关item缓存中
    /// </summary>
    /// <param name="index"></param>
    /// <param name="baseObject"></param>
    /// <returns></returns>
    private string RemoveBaseObjFromBattleTrapItem(V2 index, BaseObject baseObject)
    {
        foreach (var item in battleTrapItems)
        {
            if (item.index.x == index.x && item.index.y == index.y)
            {
                return item.RemoveCacheTrapDoObj(baseObject);
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// 清理所有机关
    /// </summary>
    private void Clear()
    {
        if (battleTrapItems != null)
        {
            foreach (var item in battleTrapItems)
            {
                item.Destroy();
            }
        }
        battleTrapItems.Clear();
    }

    #region Trap

    /// <summary>
    /// 检查路径格子机关类触发
    /// </summary>
    /// <param name="pointData"></param>
    public void CheckTrapPathGridTrigger(PointData pointData, BaseObject baseObject, PointData prevPoint)
    {

        if (pointData != null && baseObject != null && prevPoint != null && pointData.trapId > 0)
        {
            bool isTrigger = false;
            BattleTrapCfgData battleTrapCfgData = battleTrapCfgDatas.Find(delegate (BattleTrapCfgData cfg)
            {
                if (cfg != null)
                    return cfg.trapid == pointData.trapId;
                return false;
            });
            /*// 前一个格子也是机关格子
            if (prevPoint.trapId != 0 && pointData.trapId != 0)
            {
                BattleTrapCfgData battleTrapCfgData_prev = battleTrapCfgDatas.Find(delegate (BattleTrapCfgData cfg)
                {
                    if (cfg != null)
                        return cfg.trapid == prevPoint.trapId;
                    return false;
                });

                // 针对路径格子, 前后point的格子是一样的，不再触发
                if (battleTrapCfgData.trapid == battleTrapCfgData_prev.trapid)
                {
                    RefrehBaseObjFromBattleTrapItem(prevPoint.index, baseObject, battleTrapCfgData, pointData.index);
                    return;
                }
            }

            if ((prevPoint.trapId > 0 && pointData.trapId != prevPoint.trapId) || pointData.trapId == 0)
            {
                // 移除前一个格子的buff
                if (prevPoint.trapId == 0 && pointData.trapId == 0)
                {
                    return;
                }
                string doTriggerResult = RemoveBaseObjFromBattleTrapItem(prevPoint.index, baseObject);
                if (!string.IsNullOrEmpty(doTriggerResult))
                {
                    BuffManager.ins.RemoveBuff(BuffManager.ins.CreatOneBuff(long.Parse(doTriggerResult), null), baseObject);
                }
                return;
            }*/

            if (battleTrapCfgData != null)
            {
                isTrigger = CheckTrapTriggerCondition(baseObject, battleTrapCfgData, prevPoint, pointData);
                if (isTrigger)
                {
                    // 触发了, 获取触发的东西
                    string triggerBuffStrId = GetTriggerResult(battleTrapCfgData, prevPoint, pointData);
                    List<BaseObject> targets = new List<BaseObject>() { baseObject };
                    if (!string.IsNullOrEmpty(triggerBuffStrId))
                    {
                        BuffManager.ins.DOBuff(long.Parse(triggerBuffStrId), null, null, targets, false, default, null, false);
                        AddBaseObjToBattleTrapItem(pointData.index, baseObject, battleTrapCfgData, triggerBuffStrId);
                    }
                }
            }
            else
            {
                zxlogger.logwarning($"Warning: trigger trap failed! the trapid=[{pointData.trapId}] not in battle trap config!");
            }
        }
    }

    /// <summary>
    /// 检查机关触发条件
    /// </summary>
    /// <param name="baseObject"></param>
    /// <param name="battleTrapCfgData"></param>
    public bool CheckTrapTriggerCondition(BaseObject baseObject, BattleTrapCfgData battleTrapCfgData, PointData prevData, PointData pointData)
    {
        if (baseObject == null || battleTrapCfgData == null) return false;


        if (battleTrapCfgData.cond1 == 1)
        {
            return DoCheckTrapTriggerCondition_1(baseObject, battleTrapCfgData, pointData);
        }


        return false;
    }

    /// 条件类型1机关触发检查
    /// </summary>
    /// <param name="baseObject"></param>
    /// <param name="battleTrapCfgData"></param>
    /// <param name="pointData"></param>
    /// <returns></returns>
    private bool DoCheckTrapTriggerCondition_1(BaseObject baseObject, BattleTrapCfgData battleTrapCfgData, PointData pointData)
    {
        if (pointData == null)
        {
            zxlogger.logwarning($"Warning: check trigger trap condition failed! this check point data is null!");
            return false;
        }

        // 触发条件 1 = 机关所在格子上有某单位（1 = 所有怪物，2 = 飞行怪物，3 = 非飞行怪物）
        // 触发参数
        switch (battleTrapCfgData.cond1param)
        {
            case "1":
                return baseObject != null;
            case "2":
                if ((BaseMonster)baseObject == null) return false;
                return ((BaseMonster)baseObject).cfgdata.isfly == 1;
            case "3":
                if ((BaseMonster)baseObject == null) return false;
                return ((BaseMonster)baseObject).cfgdata.isfly == 0;
        }

        return false;
    }

    /// <summary>
    /// 获取触发结果
    /// </summary>
    /// <param name="battleTrapCfgData"></param>
    /// <returns></returns>
    public string GetTriggerResult(BattleTrapCfgData battleTrapCfgData, PointData prevPoint, PointData currPoint)
    {
        if (battleTrapCfgData == null) return string.Empty;

        // result1 ： 触发结果1 1 = 变速带类型（行进方向相同时的buffid; 不同时的buffid）
        if (battleTrapCfgData.result1 == 1)
        {
            return GetTriggerResult_VSBelt(battleTrapCfgData, prevPoint, currPoint);
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取触发结果 -> variable speed belt:变速带类型
    /// </summary>
    /// <param name="battleTrapCfgData"></param>
    /// <returns></returns>
    public string GetTriggerResult_VSBelt(BattleTrapCfgData battleTrapCfgData, PointData prevPoint, PointData currPoint)
    {
        Vector3 prevPos = new Vector3((float)prevPoint.pos.x, (float)prevPoint.pos.y, (float)prevPoint.pos.z);
        Vector3 currPos = new Vector3((float)currPoint.pos.x, (float)currPoint.pos.y, (float)currPoint.pos.z);

        // 1:上, 2:下, 3:左, 4:右
        int moveDire = Vector3.Dot((currPos - prevPos).normalized, Vector3.right) == 0 ?
                        (prevPos.z < currPos.z ? 1 : 2) :   // 垂直方向
                        (prevPos.x > currPos.x ? 3 : 4);    // 水平方向
        // 方向 1=上 -- 2=下 -- 3=左 -- 4=右
        string vsBeltCfgDire = battleTrapCfgData.direction;

        // result1 ： 触发结果1 1 = 变速带类型（行进方向相同时的buffid; 不同时的buffid）
        if (battleTrapCfgData.result1 == 1)
        {
            string[] buffStrSplit = battleTrapCfgData.param1.Split("|");
            if (buffStrSplit.Length != 2) return string.Empty;

            int check = CheckTrapDireResult_1(moveDire, vsBeltCfgDire);
            if (check == 1)
            {
                // 加速buff
                return buffStrSplit[0];
            }
            else if (check == 2)
            {
                // 减速buff
                return buffStrSplit[1];
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// 判断传送带类型的结果
    /// </summary>
    /// <param name="baseDire">对象前进方向</param>
    /// <param name="trapDire">机关作用方向</param>
    private int CheckTrapDireResult_1(int baseDire,string trapDire)
    {
        int check = 0;//判断结果 1-加速 2-减速 0不受影响
        string[] dirs = trapDire.Split('|');
        for (int i = 0; i < dirs.Length; i++)
        {
            if (baseDire == int.Parse(dirs[i]))//满足机关的任意一个方向
            {
                check = 1;
                return check;
            }
            else//满足机关任意一个相反方向
            {
                switch (int.Parse(dirs[i]))
                {
                    case 1://上
                        if (baseDire == 2)
                        {
                            check = 2;
                            return check;
                        }
                        break;
                    case 2://下
                        if (baseDire == 1)
                        {
                            check = 2;
                            return check;
                        }
                        break;
                    case 3://左
                        if (baseDire == 4)
                        {
                            check = 2;
                            return check;
                        }
                        break;
                    case 4://右
                        if (baseDire == 3)
                        {
                            check = 2;
                            return check;
                        }
                        break;
                }
            }

        }
        return check;
    }

    #endregion

    #region Air Wall
    // 是否开启空气墙
    private bool m_UseAirWall = true;
    // 空气墙节点
    private Transform m_AirWallTran = null;
    /* --  未使用碰撞数据，目前均统一使用胶囊体碰撞
    private Vector3 m_DefBoxCollSize = Vector3.one * 0.2f;
    private Vector3 m_DefBoxCollCenter = new Vector3(0.0f, 0.25f, 0.0f);

    private float m_DefSphCollRadius = 0.2f;
    private Vector3 m_DefSphCollCenter = new Vector3(0.0f, 0.2f, 0.0f);
     */

    // 半径
    private float m_DefCapCollRadius = 0.15f;
    // 高度
    private float m_DefCapCollHeight = 0.5f;
    // 中心点
    private Vector3 m_DefCapCollCenter = new Vector3(0.0f, 0.25f, 0.0f);

    /// <summary>
    /// 初始化空气墙
    /// </summary>
    public void InitAirWall()
    {
        if (!m_UseAirWall) return;

        GameObject groudParent = GameObject.Find("maptest");
        Transform groudTran = null;
        if (groudParent != null)
        {
            groudTran = groudParent.transform.GetChild(0);
            m_AirWallTran = groudTran.FindHideInChild("airwall");
        }

        if (m_AirWallTran != null)
        {
            m_AirWallTran.gameObject.SetActive(true);
            Transform[] walls = m_AirWallTran.GetComponentsInChildren<Transform>();
            // 修正高度
            foreach (var item in walls)
            {
                item.localPosition = new Vector3(item.localPosition.x, 0, item.localPosition.z);
            }
        }
    }

    /// <summary>
    /// 给对象添加碰撞体(胶囊), 并给带有刚体的对象锁定y轴
    /// </summary>
    /// <param name="o"></param>
    public void AddBoxCollider(GameObject o)
    {
        if (o == null)
        {
            return;
        }

        CapsuleCollider objColl = o.GetComponent<CapsuleCollider>();
        if (objColl == null)
        {
            objColl = o.AddComponent<CapsuleCollider>();
        }
        // 先给默认值， TODO：后续可走配置
        objColl.radius = m_DefCapCollRadius;
        objColl.height = m_DefCapCollHeight;
        objColl.center = m_DefCapCollCenter;

        Rigidbody rigidbody = o.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rigidbody.drag = 100;
        }
    }

    #endregion


    //------------------处理机关额外机制
    /// <summary>
    /// 处理关卡的额外机制
    /// </summary>
    public void HandleTrapEffect()
    {
        for (int i = 0; i < battleTrapItems.Count; i++)
        {
            BattleTrapItem trapItem = battleTrapItems[i];
            HandleOneTrap(trapItem);
        }
    }

    public void HandleOneTrap(BattleTrapItem trapItem)
    {
        switch (trapItem.battleTrapCfgData.timing)
        {
            case 1://战斗开始时添加一张技能卡片
                BattleLog.Log($"{trapItem.trapId}机关触发，添加一张技能卡片");
                HandleTrapType_1(trapItem);
                break;
            case 2://机关被摧毁时触发
                HandleTrapType_2(trapItem);
                break;
            default:
                break;
        }

    }

    /// <summary>
    /// 机关类型1 - 战斗开始时添加一张技能牌
    /// </summary>
    private void HandleTrapType_1(BattleTrapItem trapItem)
    {
        if (!string.IsNullOrEmpty(trapItem.battleTrapCfgData.addplace ))
        {
            string[] pm = trapItem.battleTrapCfgData.addplace.Split(';');
            string[] effectPm = trapItem.battleTrapCfgData.addskill.Split('|');
            if (pm[0] == "1")
            {
                DrawCardMgr.Instance.OnAddNewCard((int)new Snowflake().GetId(), -999, long.Parse(effectPm[0]));
            }
            else if (pm[0] == "2")
            {
                
            }
        }
    }

    /// <summary>
    /// 机关类型2 - 被摧毁时执行
    /// </summary>
    /// <param name="trapItem"></param>
    private void HandleTrapType_2(BattleTrapItem trapItem)
    {

    }

    /// <summary>
    /// 获得索敌列表(类型7:场景中心 全体)
    /// </summary>
    public List<BaseObject> GetTargetListByTrap_7(BattleSkillCfg battleSkillCfg)
    {
        List<BaseObject> target = new List<BaseObject>();
        BattleSkillBulletCfg battleSkillBulletCfg;
        BattleSkillLogicCfg battleSkillLogicCfg;
        int filter = 0;
        if (!string.IsNullOrEmpty(battleSkillCfg.bulletids))
        {
            string[] bullets = battleSkillCfg.bulletids.Split('|');
            for (int i = 0; i < bullets.Length; i++)
            {
                battleSkillBulletCfg = BattleCfgManager.Instance.GetBulletCfg(long.Parse(bullets[i]));
                if (!string.IsNullOrEmpty(battleSkillBulletCfg.logicid))
                {
                    string[] logics = battleSkillBulletCfg.logicid.Split('|');
                    for (int g = 0; g < logics.Length; g++)
                    {
                        battleSkillLogicCfg = BattleCfgManager.Instance.GetLogicCfg(long.Parse(logics[g]));
                        filter = battleSkillLogicCfg.filtertype;
                        break;
                    }
                    break;
                }
            }
        }
        for (int i = 0; i < battleTrapItems.Count; i++)
        {
            if (battleTrapItems[i].battleTrapCfgData.subtargettype == filter)
            {
                target.Add(battleTrapItems[i]);
            }
        }
        return target;
    }

    /// <summary>
    /// 切换变速带
    /// </summary>
    public void SwitchVariableSpeedBelt(List<BaseObject> target)
    {
        //改模型
        for (int i = 0; i < target.Count; i++)
        {
            BattleTrapItem trapItem = (BattleTrapItem)target[i];
            BattleTrapCfgData newCfg = null;
            newCfg = this.battleTrapCfgDatas.Find(cfg => cfg.subtargettype == trapItem.battleTrapCfgData.subtargettype && trapItem.battleTrapCfgData.special == cfg.trapid);
            if (newCfg != null)
            {
                AddBattleTrapItem(newCfg.trapid, trapItem.index, trapItem.pos);
            }
            GameObject.Destroy(trapItem.prefabObj);
            battleTrapItems.Remove(trapItem);
        }

        //改数据
        for (int i = 0; i < GameCenter.mIns.m_BattleMgr.curMissionData.pathDatas.Count; i++)
        {
            PathData onePath = GameCenter.mIns.m_BattleMgr.curMissionData.pathDatas[i];
            for (int p = 0; p < onePath.pointDatas.Count; p++)
            {
                if (onePath.pointDatas[p].trapId > 0)
                {
                    BattleTrapCfgData cfg = GetTrapConfig(onePath.pointDatas[p].trapId);
                    if (cfg != null && cfg.subtargettype == 7001)
                    {
                        onePath.pointDatas[p].trapId = cfg.special;
                    }
                }
            }
        }
    }
}

