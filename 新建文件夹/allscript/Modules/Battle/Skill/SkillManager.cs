using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技能管理器
/// </summary>
public class SkillManager 
{
    private static SkillManager Ins;
    public static SkillManager ins
    {
        get
        {
            if (Ins == null)
            {
                Ins = new SkillManager();
            }
            return Ins;
        }
    }

    //当前释放技能的角色
    public BaseHero curHero;

    private GameObject dragBox;//当前拖拽的技能格子


    ///////////////////////////////////////////===> 普通攻击 <===///////////////////////////////////////////
    /// <summary>
    /// 获得普通攻击的索敌列表——近战
    /// </summary>
    /// <param name="attacker">攻击者</param>
    /// <param name="objList">进入检测范围的对象列表</param>
    /// <param name="range">索敌范围参数</param>
    public List<BaseObject> GetBaseSkillTarget_1(GameObject attacker, List<BaseObject> objList, string range)
    {
        List<BaseObject> targetList = new List<BaseObject>();
        string[] rangeArray = range.Split(';');
        //英雄正前方向量
        Vector3 heroNor = attacker.transform.rotation * Vector3.forward;
        Vector3 temNor;
        float angle;
        for (int i = 0; i < objList.Count; i++)
        {
            if (objList[i].prefabObj == attacker)//如果攻击对象是自己
            {
                targetList.Add(objList[i]);
                continue;
            }
            //英雄与敌人的方向向量
            temNor = objList[i].prefabObj.transform.position - attacker.transform.position;
            //向量夹角
            angle = Mathf.Acos(Vector3.Dot(heroNor.normalized, temNor.normalized)) * Mathf.Rad2Deg;
            if (angle <= int.Parse(rangeArray[1])*0.5f)
            {
                targetList.Add(objList[i]);
            }
        }
        return targetList;
    }

    /// <summary>
    /// 获得普通攻击的索敌对象——远程
    /// </summary>
    public BaseObject GetBaseSkillTarget_2(GameObject attacker, List<BaseObject> objList, string range)
    {
        string[] rangeArray = range.Split(';');
        
        for (int i = 0; i < objList.Count; i++)
        {
            if (objList[i].prefabObj == attacker)//如果攻击对象是自己
            {
                return objList[i];
            }
            //计算两点距离 判断是否在攻击盲区
            float dis = Vector3.Distance(objList[i].prefabObj.transform.position, attacker.transform.position);
            if (dis >= (float.Parse(rangeArray[0]) / 100)) 
            {
                return objList[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 获得普通攻击的索敌列表——十字范围--取一边
    /// </summary>
    public List<BaseObject> GetBaseSkillTarget_3(GameObject attacker, List<BaseObject> objList, string range,out int curState)
    {
        float rangeValue = float.Parse(range);

        Vector3 temNor;
        float forawardDistacne;
        float rightDistance;
        int state = 0;//选择哪一遍 1-上 2-右 3-下 4-左
        List<BaseObject> targetList = new List<BaseObject>();
        for (int i = 0; i < objList.Count; i++)
        {
            if (objList[i].prefabObj == attacker)
            {
                targetList.Add(objList[i]);
            }

            //玩家与敌人的方向向量
            temNor = objList[i].prefabObj.transform.position - attacker.transform.position;
            forawardDistacne =  Vector3.Dot(temNor, attacker.transform.forward.normalized);
            rightDistance = Vector3.Dot(temNor, attacker.transform.right.normalized);
            if (forawardDistacne > 0 && rightDistance >= -0.65f && rightDistance <= 0.65f)//上边
            {
                if (state == 0 || state == 1)
                {
                    state = 1;
                    targetList.Add(objList[i]);
                }
 
            }
            else if (rightDistance > 0 && forawardDistacne >= -0.65f && forawardDistacne <= 0.65f)//右边
            {
                if (state == 0 || state == 2)
                {
                    state = 2;
                    targetList.Add(objList[i]);
                }
            }
            else if (forawardDistacne < 0 && rightDistance >= -0.65f && rightDistance <= 0.65f)//下边
            {
                if (state == 0 || state == 3)
                {
                    state = 3;
                    targetList.Add(objList[i]);
                }
            }
            else if (rightDistance < 0 && forawardDistacne >= -0.65f && forawardDistacne <= 0.65f)//左边
            {
                if (state == 0 || state == 4)
                {
                    state = 4;
                    targetList.Add(objList[i]);
                }
            }
        }
        curState = state;
        return targetList;
    }

    //验算一条边的敌人索敌 仅射线类型
    public List<BaseObject> GetBaseSkillTarget_3_yansuan(GameObject attacker, List<BaseObject> objList, string range, int curState)
    {
        float rangeValue = float.Parse(range);
        Vector3 temNor;
        float forawardDistacne;
        float rightDistance;
        int state = curState;//选择哪一遍 1-上 2-右 3-下 4-左
        List<BaseObject> targetList = new List<BaseObject>();
        for (int i = 0; i < objList.Count; i++)
        {
            if (objList[i].prefabObj == attacker)
            {
                targetList.Add(objList[i]);
            }

            //玩家与敌人的方向向量
            temNor = objList[i].prefabObj.transform.position - attacker.transform.position;
            forawardDistacne = Vector3.Dot(temNor, attacker.transform.forward.normalized);
            rightDistance = Vector3.Dot(temNor, attacker.transform.right.normalized);
            if (forawardDistacne > 0 && rightDistance >= -0.65f && rightDistance <= 0.65f)//上边
            {
                if (state == 0 || state == 1)
                {
                    state = 1;
                    targetList.Add(objList[i]);
                }

            }
            else if (rightDistance > 0 && forawardDistacne >= -0.65f && forawardDistacne <= 0.65f)//右边
            {
                if (state == 0 || state == 2)
                {
                    state = 2;
                    targetList.Add(objList[i]);
                }
            }
            else if (forawardDistacne < 0 && rightDistance >= -0.65f && rightDistance <= 0.65f)//下边
            {
                if (state == 0 || state == 3)
                {
                    state = 3;
                    targetList.Add(objList[i]);
                }
            }
            else if (rightDistance < 0 && forawardDistacne >= -0.65f && forawardDistacne <= 0.65f)//左边
            {
                if (state == 0 || state == 4)
                {
                    state = 4;
                    targetList.Add(objList[i]);
                }
            }
        }
        return targetList;
    }



    ////////////////////////////////////////////===> 技能索敌 <===//////////////////////////////////////////////
    /// <summary>
    /// 获得技能索敌列表 
    /// </summary>
    /// <param name="range">范围 0-无限</param>
    /// <param name="checkTarget">目标数组 1-怪物 2-英雄 3-自己 4-阻碍物</param>
    /// <param name="self"></param>
    /// <returns></returns>
    public List<BaseObject> GetBaseSkillTarget_0(int range,string checkTarget,BaseObject self)
    {
        List<BaseObject> target = new List<BaseObject>();
        string[] parm = checkTarget.Split('|');
        for (int i = 0; i < parm.Length; i++)
        {
            int type = int.Parse(parm[i]);
            switch (type)
            {
                case 1:
                    foreach (var item in BattleMonsterManager.Instance.dicMonsterList)
                    {
                        target.Add(item.Value);
                    }
                    break;
                case 2:
                    foreach (var item in BattleHeroManager.Instance.depolyHeroList)
                    {
                        if (item != self)
                        {
                            target.Add(item);
                        }
                    }
                    break;
                case 3:
                    target.Add(self);
                    break;
                case 4:
                    break;
                default:
                    break;
            }
        }
        return target;
    }


    /// <summary>
    /// 获得技能索敌范围-场景中心-半径
    /// </summary>
    /// <param name="range"></param>
    /// <param name="inputRoot"></param>
    /// <param name="checkTarget"></param>
    /// <param name="self"></param>
    /// <returns></returns>
    public List<BaseObject> GetBaseSkillTarget_7(int range, GameObject inputRoot, string checkTarget, BaseObject self)
    {
        List<BaseObject> targetList = new List<BaseObject>();

        //获得技能目标类型 获得检测列表
        string[] targetType = checkTarget.Split('|');
        List<BaseObject> checkList = new List<BaseObject>();
        for (int i = 0; i < targetType.Length; i++)
        {
            if (targetType[i] == "1")//敌人
            {
                foreach (var kv in BattleMonsterManager.Instance.dicMonsterList)
                {
                    checkList.Add(kv.Value);
                }
            }
            else if (targetType[i] == "2")//友军
            {
                foreach (var kv in BattleHeroManager.Instance.depolyHeroList)
                {
                    checkList.Add(kv);
                }
            }
            else if (targetType[i] == "3")//自己
            {
                if (!checkList.Contains(self))
                {
                    checkList.Add(self);
                }
            }
        }

        var touchpos = new Vector2(Screen.width / 2, Screen.height / 2);
        ZCamera.shootray_check(GameCenter.mIns.m_BattleMgr.battleCamer, 1 << LayerMask.NameToLayer("terrain"), touchpos, (obj) => {
            if (obj != null)
            {
                inputRoot.transform.position = new Vector3(obj.transform.position.x, 0.1f, obj.transform.position.z);
            }
        });


        float distacne;
        for (int i = 0; i < checkList.Count; i++)
        {
            //获得检测对象与点击位置的方向
            distacne = Vector3.Distance(checkList[i].prefabObj.transform.position, inputRoot.transform.position);
            if (distacne <= range)
            {
                targetList.Add(checkList[i]);
            }
        }

        return targetList;
    }


    /// <summary>
    /// 获得技能索敌列表-矩形-长宽
    /// </summary>
    /// <param name="range1">参数1</param>
    /// <param name="range2">参数2</param>
    /// <param name="inputRoot">拖拽技能范围</param>
    /// <param name="checkTarget">检测对象区间</param>
    /// <param name="self">自己释放者</param>
    /// <param name="needCheck">是否需要检测</param>
    /// <returns></returns>
    public List<BaseObject> GetBaseSkillTarget_4(float range1,float range2,GameObject inputRoot,string checkTarget,BaseObject self,bool needCheck = true)
    {
        bool isadsorb = false;
        //技能中心点
        GameObject pointRoot = null;
        if (inputRoot != null && inputRoot.transform.childCount > 0)
        {
            pointRoot = inputRoot.transform.Find("sprite/inputroot").gameObject;
        }
        else
        {
            pointRoot = inputRoot;
        }


        string[] targetType = checkTarget.Split('|');
        //获得技能目标类型 获得检测列表
        List<BaseObject> checkList = new List<BaseObject>();
        for (int i = 0; i < targetType.Length; i++)
        {
            if (targetType[i] == "1")//敌人
            {
                foreach (var kv in BattleMonsterManager.Instance.dicMonsterList)
                {
                    checkList.Add(kv.Value);
                }
            }
            else if (targetType[i] == "2")//友军
            {
                foreach (var kv in BattleHeroManager.Instance.depolyHeroList)
                {
                    checkList.Add(kv);
                }
            }
            else if (targetType[i] ==  "3")//自己
            {
                if (!checkList.Contains(self))
                {
                    checkList.Add(self);
                }
            }
        }
       


        //获得鼠标实时位置
        if (inputRoot != null && needCheck)
        {
            
             Vector3 xifuPos = ZCamera.shoottouchray_check(GameCenter.mIns.m_BattleMgr.battleCamer, 1 << LayerMask.NameToLayer("terrain"), (obj) => {
                if (obj!=null)
                {
                    dragBox = obj;
                }
                isadsorb = obj == dragBox;
                if (isadsorb)
                {
                    inputRoot.transform.position = new Vector3(obj.transform.position.x + 0.6f * 1.3f, 0.1f, obj.transform.position.z - 0.6f * 1.3f);
                }  
            });
            if (!isadsorb)
            {
                xifuPos.y = 0.1f;
                xifuPos.x = xifuPos.x + 0.6f * 1.3f;
                xifuPos.z = xifuPos.z - 0.6f * 1.3f;
                inputRoot.transform.position = xifuPos;
            }
            
        }


        Vector3 temNor;
        float forawardDistacne;
        float rightDistance;
        List<BaseObject> baseMonsters = new List<BaseObject>();
        for (int i = 0; i < checkList.Count; i++)
        {
            //获得检测对象与点击位置的方向
            temNor = checkList[i].prefabObj.transform.position - pointRoot.transform.position;
            forawardDistacne = Vector3.Dot(temNor,Vector3.forward);
            rightDistance = Vector3.Dot(temNor, Vector3.right);
            if (Mathf.Abs(rightDistance) <= (range1 * 1.3f / 2) && Mathf.Abs(forawardDistacne) <= (range2 * 1.3f / 2)) 
            {
                baseMonsters.Add(checkList[i]);
            }
        }
        return baseMonsters;
    }

    /// <summary>
    /// 获得技能索敌范围-单体-无限
    /// </summary>
    public List<BaseObject> GetBaseSkillTarget_5(Camera battleCamera, string checkTarget, BaseObject self)
    {
        List<BaseObject> targetList = new List<BaseObject>();
        //获得技能目标类型 获得检测列表
        string[] targetType = checkTarget.Split('|');
        //默认不检测层数
        int layerMask = 0;

        List<string> layerName = new List<string>();
        for (int i = 0; i < targetType.Length; i++)
        {
            if (targetType[i] == "1")//敌人
            {
                layerName.Add("MonsterItem");
            }
            else if (targetType[i] == "2")//友军
            {
                layerName.Add("HeroItem");
            }
            else if (targetType[i] == "3" && targetType.Length <= 1)//只有自己
            {
                targetList.Add(self);
                return targetList;
            }
        }
        if (layerName.Count > 0)
        {
            layerMask = LayerMask.GetMask(layerName.ToArray());
        }
        
        GameObject target =  ZCamera.shootray_checkobj(battleCamera, layerMask , (obj) =>
        {
        });

        if (target != null)
        {
            if (target.GetComponent<MonsterController>() != null )
            {
                targetList.Add(target.GetComponent<MonsterController>().monsterData);
                return targetList;
            }
            else if (target.transform.parent.GetComponentInParent<HeroController>() != null)
            {
                targetList.Add(target.transform.parent.GetComponentInParent<HeroController>().baseHero);
                return targetList;
            }
        }
        return targetList;
    }


    /// <summary>
    /// 获得技能索敌范围-十字-半径-不包含中心点
    /// </summary>
    public List<BaseObject> GetBaseSkillTarget_6(string range,GameObject inputRoot, string checkTarget, BaseObject self, bool needCheck = true)
    {
        bool isadsorb = false;
        string[] targetType = checkTarget.Split('|');
        //获得技能目标类型 获得检测列表
        List<BaseObject> checkList = new List<BaseObject>();
        for (int i = 0; i < targetType.Length; i++)
        {
            if (targetType[i] == "1")//敌人
            {
                foreach (var kv in BattleMonsterManager.Instance.dicMonsterList)
                {
                    checkList.Add(kv.Value);
                }
            }
            else if (targetType[i] == "2")//友军
            {
                foreach (var kv in BattleHeroManager.Instance.depolyHeroList)
                {
                    checkList.Add(kv);
                }
            }
            else if (targetType[i] == "3")//自己
            {
                checkList.Add(self);
            }
        }

        List<BaseObject> targetList = new List<BaseObject>();

        //获得鼠标实时位置
        if (inputRoot != null && needCheck)
        {

            Vector3 xifuPos = ZCamera.shoottouchray_check(GameCenter.mIns.m_BattleMgr.battleCamer, 1 << LayerMask.NameToLayer("terrain"), (obj) => {
                if (obj != null)
                {
                    dragBox = obj;
                }
                isadsorb = obj == dragBox;
                if (isadsorb)
                {
                    inputRoot.transform.position = obj.transform.position;
                }
            });

            if (!isadsorb)
            {
                inputRoot.transform.position = xifuPos;
            }

        }

        Vector3 temNor;
        float forawardDistacne;
        float rightDistance;
        for (int i = 0; i < checkList.Count; i++)
        {
            temNor = checkList[i].prefabObj.transform.position - inputRoot.transform.position;
            forawardDistacne = Vector3.Dot(temNor, inputRoot.transform.forward.normalized);
            rightDistance = Vector3.Dot(temNor, inputRoot.transform.right.normalized);

            if ((Mathf.Abs(forawardDistacne) <= float.Parse(range) / 100 + 0.65f && Mathf.Abs(rightDistance) <= 0.65f) || (Mathf.Abs(forawardDistacne) <= 0.65 && Mathf.Abs(rightDistance) <= float.Parse(range) / 100 + 0.65f))
            {
                targetList.Add(checkList[i]);
            }
        }
        return targetList;
    }


    /// <summary>
    /// 获得释放技能的英雄
    /// </summary>
    /// <param name="heroid"></param>
    public BaseHero GetReleasSkillHeroByID(long heroid)
    {
        BaseHero curHero = BattleHeroManager.Instance.depolyHeroList.Find(h => h.objID == heroid);
        return curHero;
    }

    /// <summary>
    /// 获得单个子弹的所有逻辑配置
    /// </summary>
    /// <param name="battleSkillBulletCfg"></param>
    /// <returns></returns>
    public List<BattleSkillLogicCfg> GetAllLogicCfgByBullet(BattleSkillBulletCfg battleSkillBulletCfg)
    {
        List<BattleSkillLogicCfg> battleSkillLogicCfgs = new List<BattleSkillLogicCfg>();
        if (!string.IsNullOrEmpty(battleSkillBulletCfg.logicid))//有逻辑
        {
            string[] logicIDs = battleSkillBulletCfg.logicid.Split('|');
            for (int l = 0; l < logicIDs.Length; l++)
            {
                battleSkillLogicCfgs.Add(BattleCfgManager.Instance.GetLogicCfg((long.Parse(logicIDs[l]))));
            }
            return battleSkillLogicCfgs;
        }
        return null;
    }


    ////////////////////////////////////////////////////////=====怪物ai索敌=====////////////////////////////////////////////////////////

    public List<BaseObject> GetMonsterAITarget_1(BaseMonster monster, BattleSkillCfg skillCfg)
    {
        List<BaseObject> checkList = new List<BaseObject>();
        string[] target = skillCfg.hightlight.Split('|');

        string[] range = skillCfg.guiderange.Split(';');
        for (int i = 0; i < target.Length; i++)
        {
            switch (target[i])
            {
                case "1"://怪物
                    foreach (var item in BattleMonsterManager.Instance.dicMonsterList)
                    {
                        float dis = Vector3.Distance(monster.prefabObj.transform.position, item.Value.prefabObj.transform.position);
                        if (dis < float.Parse(range[0]) / 100) 
                        {
                            checkList.Add(item.Value);
                        }
                    }
                    break;
                case "2"://英雄
                    foreach (var item in BattleHeroManager.Instance.depolyHeroList)
                    {
                        float dis = Vector3.Distance(monster.prefabObj.transform.position, item.prefabObj.transform.position);
                        if (dis < float.Parse(range[0]) / 100)
                        {
                            checkList.Add(item);
                        }
                    }
                    break;
                case "3"://自己
                    checkList.Add(monster);
                    break;
                case "4"://阻碍物
                    foreach (var item in BattleTrapManager.Instance.GetTrapObjList())
                    {
                        float dis = Vector3.Distance(monster.prefabObj.transform.position, item.roleObj.transform.position);
                        if (dis < float.Parse(range[0]) / 100)
                        {
                            checkList.Add(item);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        return GetBaseSkillTarget_1(monster.prefabObj, checkList, skillCfg.guiderange);
    }

    /// <summary>
    /// 获得怪物的ai索敌列表 单体
    /// </summary>
    /// <param name="monster"></param>
    /// <param name="range"></param>
    public BaseObject GetMonsterAITarget_2(BaseMonster monster,float range, BattleSkillCfg skillCfg)
    {
        //索敌目标
        string[] target = skillCfg.hightlight.Split('|');
        float distance = 999;
        BaseObject targetObj = null;
        for (int i = 0; i < target.Length; i++)
        {
            switch (target[i])
            {
                case "1"://怪物
                    foreach (var item in BattleMonsterManager.Instance.dicMonsterList)
                    {
                        if (monster.GUID != item.Value.GUID)
                        {
                            float dis = Vector3.Distance(monster.prefabObj.transform.position, item.Value.prefabObj.transform.position);
                            if (dis <= range && dis < distance)
                            {
                                distance = dis;
                                targetObj = item.Value;
                            }
                        }
                    }
                    break;
                case "2"://英雄
                    foreach (var item in BattleHeroManager.Instance.depolyHeroList)
                    {
                        float dis = Vector3.Distance(monster.prefabObj.transform.position, item.prefabObj.transform.position);
                        if (dis <= range && dis < distance)
                        {
                            distance = dis;
                            targetObj = item;
                        }
                    }
                    break;
                case "3"://自己
                    targetObj = monster;
                    break;
                case "4"://阻碍物
                    foreach (var item in BattleTrapManager.Instance.GetTrapObjList())
                    {
                        float dis = Vector3.Distance(monster.prefabObj.transform.position, item.roleObj.transform.position);
                        if (dis <= range && dis < distance)
                        {
                            distance = dis;
                            targetObj = item;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        return targetObj;
    }
}
