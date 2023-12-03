using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// buff管理类
/// </summary>
public class BuffManager
{
    private static BuffManager Ins;
    public static BuffManager ins
    {
        get
        {
            if (Ins == null)
            {
                Ins = new BuffManager();
            }
            return Ins;
        }
    }

    public long buffguid;

    //全局buff
    public List<BaseBuff> globalBuff;

    public void InitBuffManager()
    {
        globalBuff = new List<BaseBuff>();
        buffguid = 30000;
    }


    /// <summary>
    /// 执行buff(索敌目标)
    /// </summary>
    /// <param name="computeBase">快照数据</param>
    /// <param name="objList"></param>
    /// <param name="holder"></param>
    /// <param name="baseSkillLogicCfgs"></param>
    /// <param name="battleSkillCfg"></param>
    /// <param name="isinitiative"></param>
    /// <param name="isSkill"></param>
    /// <param name="boomPos"></param>
    public void ExecuteBuff(BaseObject computeBase,List<BaseObject> objList,BaseObject holder,List<BattleSkillLogicCfg> baseSkillLogicCfgs, BattleSkillCfg battleSkillCfg , bool isinitiative, bool isSkill = false, Vector3 boomPos = default)
    {

        BattleSkillLogicCfg logicCfg;
        List<string> buffidList = new List<string>();
        for (int i = 0; i < baseSkillLogicCfgs.Count; i++)
        {
            //获得单个效果的配置
            logicCfg = baseSkillLogicCfgs[i];
            if (!string.IsNullOrEmpty(logicCfg.effectfunction))
            {
                string[] functions = logicCfg.effectfunction.Split('|');
                string[] functionPms = logicCfg.functionpm.Split('|');
                for (int f = 0; f < functions.Length; f++)
                {
                    switch (functions[f])
                    {
                        case "1001":
                            break;
                        case "1002":
                            holder.aniState = int.Parse(functionPms[f]);
                            break;
                        case "1003":
                            holder.bFly = int.Parse(functionPms[f]);
                            break;
                        case "1004":
                            break;
                        case "1005"://变速带切换
                            BattleTrapManager.Instance.SwitchVariableSpeedBelt(objList);
                            break;
                        default:
                            break;
                    }
                }
            }
            buffidList = buffidList.Concat(new List<string>(logicCfg.bufferids.Split('|'))).ToList<string>();

            //检测附加条件和添加buff
            if (!string.IsNullOrEmpty(logicCfg.conditionid1) && CheckLogicCondition(logicCfg.conditionid1, holder)) 
            {
                buffidList = buffidList.Concat(new List<string>(logicCfg.bufferids1.Split('|'))).ToList<string>();
            }
            if (!string.IsNullOrEmpty(logicCfg.conditionid2) && CheckLogicCondition(logicCfg.conditionid2, holder)) 
            {
                buffidList = buffidList.Concat(new List<string>(logicCfg.bufferids2.Split('|'))).ToList<string>();
            }
        }

        //如果是支援状态技能
        if (battleSkillCfg != null && battleSkillCfg.issupsill == 1)
        {
            
            //添加
            BuffManager.ins.AddUpSkill(battleSkillCfg, buffidList,holder);
        }

        if (buffidList.Count == 1 && string.IsNullOrEmpty(buffidList[0]))
        {
            holder.isOnSkill = false;
            return;
        }

        for (int i = 0; i < buffidList.Count; i++)
        {
            BuffManager.ins.DOBuff(long.Parse(buffidList[i]), holder, computeBase, objList, isSkill, boomPos, battleSkillCfg, isinitiative);
        }
       
    }

    /// <summary>
    /// 执行buff（手指拖拽技能坐标）
    /// </summary>
    /// <param name="computeBase">快照数据</param>
    /// <param name="targetPos"></param>
    /// <param name="holder"></param>
    /// <param name="baseSkillLogicCfgs"></param>
    /// <param name="battleSkillCfg"></param>
    /// <param name="isinitiative"></param>
    /// <param name="isSkill"></param>
    public void ExecuteBuff(BaseObject computeBase,Vector3 targetPos, BaseObject holder, List<BattleSkillLogicCfg> baseSkillLogicCfgs, BattleSkillCfg battleSkillCfg, bool isinitiative, bool isSkill = false)
    {
        List<BaseObject> targetList = new List<BaseObject>();

        ExecuteBuff(computeBase,targetList, holder,baseSkillLogicCfgs, battleSkillCfg, isinitiative, isSkill, targetPos);
    }

    /// <summary>
    /// 检测技能逻辑表中的条件
    /// </summary>
    public bool CheckLogicCondition(string condition, BaseObject holder)
    {
        string[] par = condition.Split('|');
        for (int i = 0; i < par.Length; i++)
        {
            if (!ConditionManager.ins.CheckCondition(holder, long.Parse(par[i])))
            {
                return false;
            }
        }
        return true;
    }

    //支援状态技能记录列表 k-技能总表id 
    public Dictionary<long, UpSkill> upSkills;

    /// <summary>
    /// 执行buff
    /// </summary>
    /// <param name="buffid"></param>
    /// <param name="atacker">攻击者</param>
    /// <param name="target">目标</param>
    /// <param name="isskill">是否是技能</param>
    /// <param name="battleSkillCfg">是否是技能</param>
    public void DOBuff(long buffid, BaseObject atacker, BaseObject computeBase, List<BaseObject> target, bool isskill, Vector3 boomPos,BattleSkillCfg battleSkillCfg,bool isinitiative, bool bCompute = true, params object[] parm)
    {
        BuffMain buffMain = new BuffMain();
        buffMain.OnStart(buffid, atacker, computeBase, target, isskill, boomPos, battleSkillCfg, bCompute, isinitiative, parm );
    }

    /// <summary>
    /// 从目标身上移除一个buff
    /// </summary>
    /// <param name="target"></param>
    public void RemoveBuff(BaseBuff basebuff, BaseObject target)
    {
        if (basebuff != null)
        {
            basebuff.RemoveBuff(target);
        }
    }



    /// <summary>
    /// 获得条件ID对应的派生类
    /// </summary>
    public BaseBuff GetBuffClassByType(int bufftype)
    {
        buffguid += 1;
        switch (bufftype)
        {
            case 0://空buff
                return new Buff0();
            case 1://瞬时伤害
                return new Buff1();
            case 2://改变属性值
                return new Buff2();
            case 3://回复生命值
                return new Buff3();
            case 4://元素协战
                return new Buff4();
            case 5://水泡
                return new Buff5();
            case 6://持续伤害
                return new Buff6();
            case 7://受击特效
                return new Buff7();
            case 8://反击
                return new Buff8();
            case 9://回复算力
                return new Buff9();
            case 10://嘲讽
                return new Buff10();
            case 11://降低消耗
                return new Buff11();
            case 12://运算伤害时属性瞬间改变
                return new Buff12();
            case 13://转换buff
                return new Buff13();
            case 14://动态属性提升
                return new Buff14();
            case 15://中心伤害提升
                    //return new Buff15();
            case 16://免疫一次死亡并触发buff
                return new Buff16();
            case 17://回复生命值（被回复者的最大生命值计算）
                return new Buff17();
            case 18://造成伤害（目标最大生命值计算）
                return new Buff18();
            case 19://免疫一次元素伤害
                return new Buff19();
            case 20://免疫指定类型的buff
                return new Buff20();
            case 23://每秒回复生命值（以释放者最大生命值计算）
                return new Buff23();
            case 24://全局buff 条件检测
                return new Buff24();
            case 25://驱散
                return new Buff25();
            case 28://改变属性 特定怪物额外加成
                return new Buff28();
            case 29://传送
                return new Buff29();
            case 30:
                return new Buff30();
            case 31://计数器
                return new Buff31();
            case 32://禁锢
                return new Buff32();
            case 34://伤害协战
                return new Buff34();
            case 36://释放一个技能
                return new Buff36();
            default:
                break;
        }
        Debug.LogError($"未找到类型为{bufftype}的buff，请检查配置！！！");
        return null;
    }

    /// <summary>
    /// 添加一个支援状态技能的计时器
    /// </summary>
    private void AddUpSkillTimer(BattleSkillCfg battleSkillCfg, List<string> buffidList, BaseObject holder)
    {
        if (upSkills == null)
        {
            upSkills = new Dictionary<long, UpSkill>();
        }


        //重复施加支援状态技能，时间刷新，子buff叠加状态不变
        if (upSkills.ContainsKey(battleSkillCfg.skillid))
        {
            upSkills[battleSkillCfg.skillid].timer = battleSkillCfg.suptime;

            AddOneUpSkillUI(battleSkillCfg);
        }
        else
        {
            //添加一个支援状态技能
            upSkills.Add(battleSkillCfg.skillid, new UpSkill());
            upSkills[battleSkillCfg.skillid].timer = battleSkillCfg.suptime;
            upSkills[battleSkillCfg.skillid].buffids = buffidList;
            upSkills[battleSkillCfg.skillid].holder = holder;
            AddOneUpSkillUI(battleSkillCfg);

        }
    }

    /// <summary>
    /// 添加一个支援状态技能
    /// </summary>
    /// <param name="battleSkillCfg"></param>
    /// <param name="buffidList"></param>
    public void AddUpSkill(BattleSkillCfg battleSkillCfg, List<string> buffidList,BaseObject holder)
    {
        BattleEventManager.Dispatch(BattleEventName.battle_upskillStar, battleSkillCfg.skillid);
        AddUpSkillTimer(battleSkillCfg, buffidList, holder);
    }

    /// <summary>
    /// 添加/刷新一个状态支援栏
    /// </summary>
    public void AddOneUpSkillUI(BattleSkillCfg battleSkillCfg)
    {
        basebattle bb = GameCenter.mIns.m_UIMgr.Get<basebattle>();
        bb.AddOneBuffToBuffList(battleSkillCfg);
    }

    /// <summary>
    /// 移除一个状态支援技能
    /// </summary>
    /// <param name="skillID"></param>
    public void RemoveOneUpSkill(long skillID)
    {
        if (upSkills.ContainsKey(skillID))
        {
            BattleEventManager.Dispatch(BattleEventName.battle_upskillEnd, skillID);
        }
    }
}

//支援状态技能 
public class UpSkill
{
    public long timer;

    public BaseObject holder;

    public  List<string> buffids;//技能包含的子buff
}

