using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 主buff 所有buff执行主buff表现逻辑、更新索敌等操作后， 再派发到各自效果的子buff上
/// </summary>
public class BuffMain
{

	public BattleSkillBuffCfg buffCfg;

    public bool bCompute;

    public BattleSkillCfg skillCfg;

    public Vector3 targetPos;

    public BaseObject holder;//释放者

    public BaseObject computeBase;//快照数据

    public List<BaseObject> targetList;

    public bool isSkill;

    public object[] parm;
    public bool isinitiative;//是否主动释放



    /// <summary>
    /// 主buff进入
    /// </summary>
    /// <param name="buffid"></param>
    /// <param name="atacker"></param>
    /// <param name="target"></param>
    /// <param name="isskill"></param>
    /// <param name="boomPos"></param>
    /// <param name="battleSkillCfg"></param>
    /// <param name="bCompute"></param>
    /// <param name="parm"></param>
    public void OnStart(long buffid, BaseObject atacker, BaseObject computeBase, List<BaseObject> target, bool isskill, Vector3 boomPos, BattleSkillCfg battleSkillCfg, bool bCompute ,bool isinitiative, params object[] parm)
	{
		this.buffCfg = BattleCfgManager.Instance.GetBuffCfg(buffid);
        if (buffCfg == null)
        {
            Debug.LogError($"未找到buffid：{buffid},请检查！");
        }
        this.bCompute = bCompute;
        this.skillCfg = battleSkillCfg;
        this.targetPos = boomPos;
        this.holder = atacker;
        this.computeBase = computeBase;
        this.targetList = target;
        this.isSkill = isskill;
        this.parm = parm;
        this.isinitiative = isinitiative;

        OnShow();
    }

	/// <summary>
	/// 表现阶段
	/// </summary>
	public void OnShow()
	{
        int boomIndex;
        //爆炸延时
        string[] boomDelay = buffCfg.boomdelay.Split('|');
		//爆炸资源
		if (!string.IsNullOrEmpty(buffCfg.boomres))
		{
			string[] boomRes = buffCfg.boomres.Split('|');
			int resIndex;
            for (int i = 0; i < boomRes.Length; i++)
			{

                resIndex = i > boomRes.Length - 1 ? i % boomRes.Length : i;


                boomIndex = i > boomDelay.Length - 1 ? i % boomDelay.Length : i;

                if (resIndex < boomRes.Length)
                {
                    GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(int.Parse(boomDelay[boomIndex]), () =>
                    {
                        holder.effectStackCompent.ShowEffectOnPanel(boomRes[resIndex], targetPos);
                    });
                }
            }
		}
		int atkCount = buffCfg.count;

        for (int i = 0; i < atkCount; i++)
        {
            boomIndex = i > boomDelay.Length - 1 ? i % boomDelay.Length : i;
            bool isend = i == atkCount - 1;
            GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(buffCfg.hitdelay + int.Parse(boomDelay[boomIndex]), () => {
                OnToDo();
                if (isSkill && isend)
                {
                    holder.curSkillTarget = null;
                }
            });
        }
    }

	/// <summary>
	/// 执行阶段
	/// </summary>
	public void OnToDo()
	{
        if (this.bCompute)
        {
            targetList = ComputeRangeAgain(targetList,skillCfg, targetPos, holder);
        }

        if (isSkill && holder.talentCompent != null)
        {
            holder.talentCompent.OnDoSkill(skillCfg.skilltype, targetList, isinitiative);
        }
        DispatchChildBuff();

    }

    private async void ShowEffect(BaseBuff buff)
    {
        //buff特效
        if (!string.IsNullOrEmpty(this.buffCfg.buffres))
        {
            GameObject eff = await buff.target.effectStackCompent.ShowEffect(this.buffCfg.buffres, buff.target.effectRoot, buff.target.pointHelper.GetBoneByString(this.buffCfg.buffrespoint).position, default, true);
            if (buff.buffEffect == null)
            {
                buff.buffEffect = new List<GameObject>();
                buff.buffEffect.Add(eff);
            }
        }

        //受击特效
        if (!string.IsNullOrEmpty(this.buffCfg.hitres))
        {
            buff.target.effectStackCompent.ShowEffect(this.buffCfg.hitres, buff.target.effectRoot, buff.target.pointHelper.GetBoneByString(this.buffCfg.hitrespoint).position);
        }
    }


    /// <summary>
    /// 派发到各个子buff
    /// </summary>
    public void DispatchChildBuff()
    {
        if (buffCfg.functiontype == 24)//全局检测buff类型
        {
            BaseBuff deriverd = BuffManager.ins.GetBuffClassByType(buffCfg.functiontype);
            deriverd.mainBuff = this;
            deriverd.target = GameCenter.mIns.m_BattleMgr.baseGod;
            deriverd.remainingtime = this.buffCfg.time / 1000f;
            deriverd.bPermanentb = this.buffCfg.time == -1;
            deriverd.guid = BuffManager.ins.buffguid;
            deriverd.OnBuffStart(parm);
        }
        else
        {
            for (int i = 0; i < targetList.Count; i++)
            {
                BaseBuff deriverd = BuffManager.ins.GetBuffClassByType(buffCfg.functiontype);
                if (deriverd != null)
                {
                    deriverd.mainBuff = this;
                    deriverd.target = targetList[i];
                    deriverd.remainingtime = this.buffCfg.time / 1000f;
                    deriverd.bPermanentb = this.buffCfg.time == -1;
                    deriverd.guid = BuffManager.ins.buffguid;
                    bool bTry = deriverd.OnBuffStart(parm);
                    if (bTry)
                    {
                        ShowEffect(deriverd);
                    }
                   
                }
            }
        }
        
    }



    /// <summary>
    /// 再次计算索敌列表 防止延时过后表现与实际效果偏差
    /// </summary>
    /// <param name="battleSkillCfg"></param>
    /// <param name="targetPos"></param>
    /// <param name="holder"></param>
    public List<BaseObject> ComputeRangeAgain(List<BaseObject> oldList,BattleSkillCfg battleSkillCfg, Vector3 targetPos, BaseObject holder = null)
    {
        List<BaseObject> targetList = null;
        if (battleSkillCfg != null)
        {

            GameObject inputRoot = new GameObject();
            inputRoot.transform.position = targetPos;
            inputRoot.SetActive(false);
            switch (battleSkillCfg.guidetype)
            {
                case 0://支援状态 参数0：自己  参数1:友方全体
                    int rangePar = int.Parse(battleSkillCfg.guiderange);
                    targetList = SkillManager.ins.GetBaseSkillTarget_0(rangePar, battleSkillCfg.hightlight, holder);
                    break;
                case 4://矩形范围
                    string[] rangeParm = battleSkillCfg.guiderange.Split(';');
                    targetList = SkillManager.ins.GetBaseSkillTarget_4(int.Parse(rangeParm[0]) / 130, int.Parse(rangeParm[1]) / 130, inputRoot, battleSkillCfg.hightlight, holder, false);

                    break;
                case 6://获得技能索敌范围-十字
                    float s = 0.16f * int.Parse(battleSkillCfg.guiderange) / 100;
                    targetList = SkillManager.ins.GetBaseSkillTarget_6(battleSkillCfg.guiderange, inputRoot, battleSkillCfg.hightlight, holder, false);
                    break;
                case 7:
                    int rangeRadius = int.Parse(battleSkillCfg.guiderange);
                    if (inputRoot == null)
                    {
                        inputRoot = new GameObject();
                    }
                    targetList = SkillManager.ins.GetBaseSkillTarget_7(rangeRadius, inputRoot, battleSkillCfg.hightlight, holder);
                    break;
                default:
                    GameObject.Destroy(inputRoot);
                    return oldList;
            }
            GameObject.Destroy(inputRoot);
        }
        else
        {
            return oldList; 
        }
        return targetList;
    }
}

