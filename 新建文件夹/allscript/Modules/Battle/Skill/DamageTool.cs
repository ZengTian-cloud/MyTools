using System;
using UnityEngine;
/// <summary>
/// 伤害计算工具
/// </summary>
public static class DamageTool
{
    //是否暴击
    public static bool isCritical;
    //进攻方攻击数值
    public static float atkerValue;
    //防守方防御系数
    public static float defdenderValue;
    //进攻方技能系数
    public static float atkerSkillMultiplier;
    //元素乘区
    public static float ElementMultiplierValue;
    //爆伤系数
    public static float criticalHitMultiplier;
    //增伤系数
    public static float damageBoost;

    //技能系数固定值
    public static float skillValue;

    /// <summary>
    /// 计算本次技能所有伤害
    /// </summary>
    /// <param name="skillCfg"></param>
    /// <param name="buffCfg"></param>
    /// <param name="atker">攻击方</param>
    /// <param name="computeBase">攻击方快照数据</param>
    /// <param name="defender">防御方</param>
    /// <param name="state">技能伤害计算类型 1-自身攻击计算 2-自身防御计算 3-自己最大生命值计算 4-自己当前生命计算 5-目标最大生命值计算</param>
    /// <returns></returns>
    public static float ComputeBaseObjUltimateInjury(BattleSkillCfg skillCfg,BattleSkillBuffCfg buffCfg, BaseObject atker, BaseObject computeBase, BaseObject defender,int state)
    {
        //技能固定值
        skillValue = GetSkillValue(skillCfg, buffCfg, atker);
        //进攻方攻击数值
        atkerValue = ComputeBaseObjAtkMultiplier(atker,computeBase);
        //防守方防御系数
        defdenderValue = ComputeBaseObjDEFMultiplier(defender, atker);
        //进攻方技能系数
        atkerSkillMultiplier = ComputeBaseObjSkillMultiplier(atker,computeBase, defender, skillCfg, buffCfg);
        //元素乘区
        ElementMultiplierValue = ComputeBaseObjElementDamageMultiplier(atker, computeBase, defender, skillCfg.skillelement);
        //爆伤系数
        criticalHitMultiplier = ComputeBaseObjCriticalHitMultiplier(atker, computeBase, defender);
        //增伤系数
        damageBoost = ComputeBaseObjDamageBoostMultiplier(atker, computeBase, defender);

        float value = ComputeUltimateInjury(state, atker, computeBase, defender);
        return value;
    }

    /// <summary>
    /// 计算治疗技能最终治疗量
    /// </summary>
    /// <param name="skillCfg"></param>
    /// <param name="buffCfg"></param>
    /// <param name="atker"></param>
    /// <param name="defender"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static float ComputeBaseObjHealthValue(BattleSkillCfg skillCfg, BattleSkillBuffCfg buffCfg, BaseObject atker, BaseObject defender, int state)
    {
        //技能固定值
        float skillValue = 0;
        //技能系数
        float skillMultiplier = 0;
        float totalValue = 0;
        //1-自身攻击计算 2-自身防御计算 3-自己最大生命值计算 4-自己当前生命计算 5-目标最大生命值计算
        switch (state)
        {
            case 1:
                //固定值
                skillValue = GetSkillValue(skillCfg, buffCfg,atker);
                skillMultiplier = GetSkillValueMultipler(skillCfg, buffCfg, atker);
                totalValue = skillValue + atker.GetBattleAttr(EAttr.ATK) * skillMultiplier;
                break;
            case 2:
                break;
            case 3:
                skillValue = GetSkillValue(skillCfg, buffCfg, atker);
                skillMultiplier = GetSkillValueMultipler(skillCfg, buffCfg, atker);
                totalValue = skillValue + atker.GetBattleAttr(EAttr.HP) * skillMultiplier;
                break;
            case 4:
                break;
            case 5:
                skillValue = GetSkillValue(skillCfg, buffCfg, atker);
                skillMultiplier = GetSkillValueMultipler(skillCfg, buffCfg, atker);
                totalValue = skillValue + defender.GetBattleAttr(EAttr.HP) * skillMultiplier;
                break;
            default:
                break;
        }
        return totalValue;
    }

    /// <summary>
    /// 计算最终伤害
    /// </summary>
    /// <param name="state"></param>
    /// <param name="atker"></param>
    /// <param name="computeBase"></param>
    /// <param name="def"></param>
    /// <returns></returns>
    public static float ComputeUltimateInjury(int state,BaseObject atker, BaseObject computeBase, BaseObject def)
    {
        if (atker == null || atker.bRecycle || def == null || def.bRecycle)
        {
            return 0;
        }
        switch (state)
        {
            case 1://自身攻击计算
                if (isCritical)
                {
                    return (atkerValue * atkerSkillMultiplier + skillValue) * (1 - defdenderValue) * (1 + ElementMultiplierValue) * (1 + damageBoost) * (1 + criticalHitMultiplier);
                }
                else
                {
                    return (atkerValue * atkerSkillMultiplier + skillValue) * (1 - defdenderValue) * (1 + ElementMultiplierValue) * (1 + damageBoost);
                }
            case 2://自身防御计算
                if (isCritical)
                {
                    return (computeBase.GetBattleAttr(EAttr.DEF) * atkerSkillMultiplier + skillValue) * (1 - defdenderValue) * (1 + ElementMultiplierValue) * (1 + damageBoost) * (1 + criticalHitMultiplier);
                }
                else
                {
                    return (computeBase.GetBattleAttr(EAttr.DEF) * atkerSkillMultiplier + skillValue) * (1 - defdenderValue) * (1 + ElementMultiplierValue) * (1 + damageBoost);
                }
            case 3://自己最大生命值计算
                if (isCritical)
                {
                    return (computeBase.GetBattleAttr(EAttr.HP) * atkerSkillMultiplier + skillValue) * (1 - defdenderValue) * (1 + ElementMultiplierValue) * (1 + damageBoost) * (1 + criticalHitMultiplier);
                }
                else
                {
                    return (computeBase.GetBattleAttr(EAttr.HP) * atkerSkillMultiplier + skillValue) * (1 - defdenderValue) * (1 + ElementMultiplierValue) * (1 + damageBoost);
                }
            case 4://自己当前生命计算
                if (isCritical)
                {
                    return (computeBase.curHp * atkerSkillMultiplier + skillValue) * (1 - defdenderValue) * (1 + ElementMultiplierValue) * (1 + damageBoost) * (1 + criticalHitMultiplier);
                }
                else
                {
                    return (computeBase.curHp * atkerSkillMultiplier + skillValue) * (1 - defdenderValue) * (1 + ElementMultiplierValue) * (1 + damageBoost);
                }
            case 5://目标最大生命值计算
                if (isCritical)
                {
                    return (def.GetBattleAttr(EAttr.HP) * atkerSkillMultiplier + skillValue) * (1 - defdenderValue) * (1 + ElementMultiplierValue) * (1 + damageBoost) * (1 + criticalHitMultiplier);
                }
                else
                {
                    return (def.GetBattleAttr(EAttr.HP) * atkerSkillMultiplier + skillValue) * (1 - defdenderValue) * (1 + ElementMultiplierValue) * (1 + damageBoost);
                }
            default:
                return 0;
        }
    }



    /// <summary>
    /// 计算对象攻击乘区系数
    /// </summary>
    /// <param name="baseObject">对象</param>
    /// <param name="computeBase">对象快照数据，实际参与运算</param>
    /// <returns></returns>
    public static float ComputeBaseObjAtkMultiplier(BaseObject baseObject,BaseObject computeBase)
    {
        float objAtkMultiplierValue = 0;
        //判断对象类型 1-英雄 2-怪物
        //英雄
        if (baseObject.objType == 1)
        {
            BaseHero baseHero = (BaseHero)baseObject;
            if (baseHero == null || baseHero.bRecycle)
            {
                return 0;
            }
            //英雄基础攻击力
            float baseHeroAtk = computeBase.GetBattleAttr(EAttr.ATK);
            //装备基础攻击力
            float baseEquipmentAtk = 0;
            //天赋基础攻击力
            float baseTalentAtk = 0;
            //法宝基础攻击力
            float baseMagicWeaponAtk = 0;
            //buff基础攻击里
            float baseBuffAtk = 0;
            //瞬时buff基础攻击里
            float baseInstantBuffAtk = 0;
            //羁绊基础攻击力
            float baseFetterAtk = 0;

            //英雄百分比攻击力
            float percentHeroAtk = computeBase.GetBattleAttr(EAttr.ATK_Per) / 10000;
            //装备百分比攻击力
            float percentEquipmentAtk = 0;
            //天赋百分比攻击力
            float percentTalentAtk = 0;
            //法宝百分比攻击力
            float percentMagicWeaponAtk = 0;
            //buff百分比攻击力
            float percentBuffAtk = 0;
            //瞬时buff百分比攻击力
            float percentInstantBuffAtk = 0;
            //羁绊百分比攻击力
            float precentFetterAtk = 0;

            //本源攻击
            float sourceAtk = (baseHeroAtk + baseEquipmentAtk + baseBuffAtk) * (1 + percentHeroAtk + percentBuffAtk);

            //本源属性转换的其他属性
            float other = 0;

            //输入系数
            objAtkMultiplierValue = (baseHeroAtk + baseEquipmentAtk + baseTalentAtk + baseMagicWeaponAtk + baseBuffAtk + baseInstantBuffAtk + baseFetterAtk) * (1 + percentHeroAtk + percentEquipmentAtk + percentTalentAtk + percentMagicWeaponAtk + percentBuffAtk + percentInstantBuffAtk + precentFetterAtk) + other;

        }
        else if (baseObject.objType == 2)//怪物
        {
            BaseMonster baseMonster = (BaseMonster)baseObject;
            if (baseMonster == null || baseMonster.bRecycle)
            {
                return 0;
            }
            //怪物基础攻击力
            float baseMonsterAtk = computeBase.GetBattleAttr(EAttr.ATK) * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(computeBase.lv, 10100101) / 10000);
            //关卡攻击系数
            float baseMissionAtk = 0;
            //buff基础攻击
            float baseBuffAtk = 0;
            //瞬时buff基础攻击
            float baseInstantBuffAtk = 0;
            //怪物百分比攻击
            float percentMonsterAtk = computeBase.GetBattleAttr(EAttr.ATK_Per)/10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(computeBase.lv, 10200101) / 10000);
            //buff百分比攻击
            float percentBuffAtk = 0;
            //瞬时buff百分比攻击
            float percentInstantBuffAtk = 0;

            objAtkMultiplierValue = (baseMonsterAtk + baseMissionAtk + baseBuffAtk + baseInstantBuffAtk) * (1 + percentMonsterAtk + percentBuffAtk + percentInstantBuffAtk);
        }

        return objAtkMultiplierValue;
    }



    /// <summary>
    /// 计算对象防御乘区系数
    /// </summary>
    /// <param name="baseObject"></param>
    public static float ComputeBaseObjDEFMultiplier(BaseObject defer, BaseObject atker)
    {
        float objDEFMultiplierValue = 0;
        //判断对象类型 1-英雄 2-怪物
        //英雄
        if (defer.objType == 1)
        {
            BaseHero baseHero = (BaseHero)defer;
            if (baseHero == null || baseHero.bRecycle)
            {
                return 0;
            }
            //英雄基础防御力
            float baseHeroDEF = baseHero.GetBattleAttr(EAttr.DEF);
            //装备基础防御力
            float baseEquipmentDEF = 0;
            //天赋基础防御离
            float baseTalentDEF = 0;
            //法宝基础防御力
            float baseMagicWeaponDEF = 0;
            //buff基础防御力
            float baseBuffDEF = 0;
            //瞬时buff基础防御力
            float baseInstantBuffDEF = 0;
            //羁绊基础防御力
            float baseFetterDEF = 0;

            //英雄百分比防御力
            float percentHeroDEF = baseHero.GetBattleAttr(EAttr.DEF_Per)/10000;
            //装备百分比防御力
            float percentEquipmentDEF = 0;
            //天赋百分比防御离
            float percentTalentDEF = 0;
            //法宝百分比防御力
            float percentMagicWeaponDEF = 0;
            //buff百分比防御力
            float percentBuffDEF = 0;
            //瞬时buff百分比防御力
            float percentInstantBuffDEF = 0;
            //羁绊百分比防御力
            float precentFetterDEF = 0;

            //本源攻击
            float sourceDEF = (baseHeroDEF + baseEquipmentDEF + baseBuffDEF) * (1 + percentHeroDEF + percentBuffDEF);

            //本源属性转换的其他属性
            float other = 0;

            //输入系数
            float def  = objDEFMultiplierValue = (baseHeroDEF + baseEquipmentDEF + baseTalentDEF + baseMagicWeaponDEF + baseBuffDEF + baseInstantBuffDEF + baseFetterDEF) * (1 + percentHeroDEF + percentEquipmentDEF + percentTalentDEF + percentMagicWeaponDEF + percentBuffDEF + percentInstantBuffDEF + precentFetterDEF) + other;
            objDEFMultiplierValue = def / (def + 9 * atker.lv + 500);

        }
        else if (defer.objType == 2)//怪物
        {
            BaseMonster baseMonster = (BaseMonster)defer;
            if (baseMonster == null || baseMonster.bRecycle)
            {
                return 0;
            }
            //怪物基础防御力
            float baseMonsterDEF = baseMonster.GetBattleAttr(EAttr.DEF) * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(defer.lv, 10100102)/10000);
            //关卡防御力系数
            float baseMissionDEF = 0;
            //buff基础防御力
            float baseBuffDEF = 0;
            //瞬时buff基础防御力
            float baseInstantBuffDEF = 0;
            //怪物百分比防御力
            float percentMonsterDEF = baseMonster.GetBattleAttr(EAttr.DEF_Per)/10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(defer.lv, 10200101) / 10000);
            //buff百分比防御力
            float percentBuffDEF = 0;
            //瞬时buff百分比防御力
            float percentInstantBuffDEF = 0;

            float def = (baseMonsterDEF + baseMissionDEF + baseBuffDEF + baseInstantBuffDEF) * (1 + percentMonsterDEF + percentBuffDEF + percentInstantBuffDEF);
            objDEFMultiplierValue = def / (def + 9 * atker.lv + 500);
        }

        return objDEFMultiplierValue;
    }


    /// <summary>
    /// 计算技能乘区系数
    /// </summary>
    /// <param name="baseObject">进攻方</param>
    /// <param name="computeBase">进攻方快照数据 实际参与运算</param>
    /// <param name="target">防御方</param>
    /// <param name="skillCfg"></param>
    /// <param name="buffCfg"></param>
    /// <returns></returns>
    public static float ComputeBaseObjSkillMultiplier(BaseObject baseObject, BaseObject computeBase, BaseObject target, BattleSkillCfg skillCfg, BattleSkillBuffCfg buffCfg)
    {
        //技能基础数值
        float baseSkillValue = GetSkillValueMultipler(skillCfg, buffCfg,baseObject);

        //最终输出乘区
        float objSkillMultiplierValue = 0;

        //进攻方总技能易伤
        float atkAllVulnerability = 0;
        //目标总技能抗性
        float defAllResistances = 0;
        //进攻方
        if (baseObject.objType == 1)//英雄
        {
            BaseHero baseHero = (BaseHero)baseObject;
            if (baseHero == null || baseHero.bRecycle)
            {
                return 0;
            }
            //进攻方技能易伤
            float atkSkillVulnerability = GetAtkerSkillVulnerability(baseHero, computeBase, skillCfg.skilltype);
            //装备技能易伤
            float equipmentSkillVulnerability = 0;
            //法宝技能易伤
            float magicWeaponSkillVulnerability = 0;
            //buff技能易伤
            float buffSkillVulnerability = 0;
            //瞬时buff技能易伤
            float instantBuffSkillVulnerability = 0;
            //羁绊技能易伤
            float fetterSkillVulnerability = 0;

            //进攻方总易伤
            atkAllVulnerability = atkSkillVulnerability + equipmentSkillVulnerability + magicWeaponSkillVulnerability + buffSkillVulnerability + instantBuffSkillVulnerability + fetterSkillVulnerability;
        }
        else if (baseObject.objType == 2)//怪物
        {
            atkAllVulnerability = 1;
        }

        //受击方
        if (target.objType == 1)//英雄
        {
            defAllResistances = 1;
        }
        else if (target.objType == 2)//怪物
        {
            BaseMonster baseMonster = (BaseMonster)target;
            if (baseMonster == null || baseMonster.bRecycle)
            {
                return 0;
            }
            //目标技能抗性
            float targetSkillResistances = GetDefenderSkillVulnerability(baseMonster, skillCfg.skilltype);
            //目标buff技能抗性
            float targetBuffSkillResistances = 0;
            //瞬时buff技能抗性
            float targetInstantBuffSkillResistances = 0;

            defAllResistances = targetSkillResistances + targetBuffSkillResistances + targetInstantBuffSkillResistances;
        }

        objSkillMultiplierValue = baseSkillValue * (1 + atkAllVulnerability - defAllResistances);

        return objSkillMultiplierValue;
    }

    /// <summary>
    /// 获得防守方的技能抗性属性(仅怪物)
    /// </summary>
    public static float GetDefenderSkillVulnerability(BaseMonster baseMonster,int skillType)
    {
        if (baseMonster == null || baseMonster.bRecycle)
        {
            return 0;
        }
        switch (skillType)
        {
            case 1://普攻
                return baseMonster.GetBattleAttr(EAttr.TakeDmg_NormalSkill) / 10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(baseMonster.lv, 10200306) / 10000);
            case 2://战技
                return baseMonster.GetBattleAttr(EAttr.TakeDmg_BattleSkill) / 10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(baseMonster.lv, 10200307) / 10000);
            case 3://秘技
                return baseMonster.GetBattleAttr(EAttr.TakeDmg_TactickSkill) / 10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(baseMonster.lv, 10200308) / 10000);
            case 4://终结技
                return baseMonster.GetBattleAttr(EAttr.TakeDmg_UltrasSkill) / 10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(baseMonster.lv, 10200309) / 10000);
            default:
                return 0;
        }

    }

    /// <summary>
    /// 获得进攻方的技能易伤属性(仅英雄)
    /// </summary>
    public static float GetAtkerSkillVulnerability(BaseHero baseHero, BaseObject computeBasem, int skillType)
    {
        if (baseHero == null || baseHero.bRecycle)
        {
            return 0;
        }
        switch (skillType)
        {
            case 1://普攻
                return computeBasem.GetBattleAttr(EAttr.Dmg_NormalSkill)/10000;
            case 2://战技
                return computeBasem.GetBattleAttr(EAttr.Dmg_BattleSkill)/10000;
            case 3://秘技
                return computeBasem.GetBattleAttr(EAttr.Dmg_TactickSkill)/10000;
            case 4://终结技
                return computeBasem.GetBattleAttr(EAttr.Dmg_UltrasSkill)/10000;
            default:
                return 1;
        }
    }

    /// <summary>
    /// 获得基础数值
    /// </summary>
    /// <param name="buffData"></param>
    /// <param name="atker"></param>
    /// <returns></returns>
    public static float GetSkillValue(BattleSkillCfg skillcfg, BattleSkillBuffCfg buffCfg, BaseObject atker)
    {
        float buffValue = 0;
        int lv = 0;
        if (atker.objType == 1)
        {
            BaseHero baseHero = (BaseHero)atker;
            if (baseHero == null || baseHero.bRecycle)
            {
                return 0;
            }
            if (skillcfg != null)
            {
                switch (skillcfg.skilltype)
                {
                    case 1://普攻
                        lv = baseHero.baseSkillLV - 1;
                        break;
                    case 2://战技
                        lv = baseHero.skill1LV - 1;
                        break;
                    case 3://秘技
                        lv = baseHero.skill2LV - 1;
                        break;
                    case 4://终结技
                        lv = baseHero.skill3LV - 1;
                        break;
                    default:
                        break;
                }
            }
        }
        if (!string.IsNullOrEmpty(buffCfg.growvalueid))
        {
            //获取技能数值表获得技能数值
            BattleSkillValueCfg valueCfg = BattleCfgManager.Instance.GetSkillValueCfg(long.Parse(buffCfg.growvalueid));
            string[] value;//数值系数
            if (!string.IsNullOrEmpty(valueCfg.basevalue))//系数数值不为空
            {
                value = valueCfg.basevalue.Split('|');
                if (value.Length > 1)//系数数值有多个 根据等级取出值
                {
                    buffValue = float.Parse(value[lv]);
                }
                else//取第一个
                {
                    buffValue = float.Parse(value[0]);
                }
            }
        }

        return buffValue;
    }

    /// <summary>
    /// 获得数值系数
    /// </summary>
    /// <param name="buffData"></param>
    /// <param name="holder"></param>
    /// <returns></returns>
    public static float GetSkillValueMultipler(BattleSkillCfg skillCfg, BattleSkillBuffCfg buffCfg, BaseObject holder)
    {
        float buffValue = 0;

        int level = 0;
        if (holder.objType == 1)
        {
            BaseHero baseHero = (BaseHero)holder;
            if (baseHero == null || baseHero.bRecycle)
            {
                return 0;
            }
            if (skillCfg != null)
            {
                switch (skillCfg.skilltype)
                {
                    case 1:
                        level = baseHero.baseSkillLV;
                        break;
                    case 2:
                        level = baseHero.skill1LV;
                        break;
                    case 3:
                        level = baseHero.skill2LV;
                        break;
                    case 4:
                        level = baseHero.skill3LV;
                        break;
                    default:
                        break;
                }
            }
        }

        if (string.IsNullOrEmpty(buffCfg.value))
        {
            //获取技能数值表获得技能数值
            BattleSkillValueCfg valueCfg = BattleCfgManager.Instance.GetSkillValueCfg(long.Parse(buffCfg.growvalueid));
            string[] value;//数值系数
            if (!string.IsNullOrEmpty(valueCfg.value))//系数数值不为空
            {
                value = valueCfg.value.Split('|');
                if (value.Length > 1)//系数数值有多个 根据等级取出值
                {
                    buffValue = float.Parse(value[level - 1]) / 10000;
                }
                else//取第一个
                {
                    buffValue = float.Parse(value[0]) / 10000;
                }
            }
        }
        else
        {
            buffValue = float.Parse(buffCfg.value) / 10000;
        }
        return buffValue;
    }


    /// <summary>
    /// 计算对象元素伤害系数
    /// </summary>
    /// <param name="baseObject">进攻方</param>
    /// <param name="computeBase">进攻方快照数据 实际参与运算</param>
    /// <param name="target">防御方</param>
    /// <param name="element"></param>
    /// <returns></returns>
    public static float ComputeBaseObjElementDamageMultiplier(BaseObject baseObject, BaseObject computeBase, BaseObject target,int element)
    {
        //最终输出元素伤害系数
        float objElementDamageMultiplierValue = 0;

        //进攻放总元素伤害系数
        float atkerTotalElementDamageMultiplierValue = 0;
        //防御方总元素抗性系数
        float deferTotalElementResistances = 0;
        if (baseObject.objType == 1)//英雄
        {
            BaseHero baseHero = (BaseHero)baseObject;
            if (baseHero == null || baseHero.bRecycle)
            {
                return 0;
            }
            //英雄元素伤害
            float heroElementDamage = GetElementDamageByHero(baseHero, computeBase, element);
            //装备元素伤害
            float equimentElementDamage = 0;
            //法宝元素伤害
            float magicWeaponElementDamage = 0;
            //buff元素伤害
            float buffElementDamage = 0;
            //瞬时buff元素伤害
            float instantBuffElementDamage = 0;
            //羁绊元素伤害
            float fetterElementDamage = 0;
            //其他属性转换的元素伤害
            float other = 0;

            atkerTotalElementDamageMultiplierValue = heroElementDamage + equimentElementDamage + magicWeaponElementDamage + buffElementDamage + instantBuffElementDamage + fetterElementDamage + other;
        }
        else if (baseObject.objType == 2)//怪物
        {
            BaseMonster baseMonster = (BaseMonster)baseObject;
            if (baseMonster == null || baseMonster.bRecycle)
            {
                return 0;
            }
            //怪物元素伤害
            float monsterElementDamage = GetElementDamageByMonster(baseMonster, computeBase, element);
            //buff元素伤害
            float buffElementDamage = 0;
            //瞬时buff元素伤害
            float instantBuffElementDamage = 0;

            atkerTotalElementDamageMultiplierValue = monsterElementDamage + buffElementDamage + instantBuffElementDamage;
        }

        if (target.objType == 1)//英雄
        {

            BaseHero baseHero = (BaseHero)target;
            if (baseHero == null || baseHero.bRecycle)
            {
                return 0;
            }
            //英雄元素抗性
            float heroElementResistances = GetElementResistancesByHero(baseHero, element);
            //装备元素抗性
            float equimentElementResistances = 0;
            //法宝元素抗性
            float magicWeaponElementResistances = 0;
            //buff元素抗性
            float buffElementResistances = 0;
            //瞬时buff元素抗性
            float instantBuffElementResistances = 0;
            //羁绊元素抗性
            float fetterElementResistances = 0;
            //其他属性转换的元素抗性
            float other = 0;

            deferTotalElementResistances = heroElementResistances + equimentElementResistances + magicWeaponElementResistances + buffElementResistances + instantBuffElementResistances + fetterElementResistances + other;
        }
        else if (target.objType == 2)
        {
            BaseMonster baseMonster = (BaseMonster)target;
            if (baseMonster == null || baseMonster.bRecycle)
            {
                return 0;
            }
            //怪物元素抗性
            float monsterElementResistances = GetElementResistancesByMonster(baseMonster, element);
            //buff元素抗性
            float buffElementResistances = 0;
            //瞬时buff元素抗性
            float instantBuffElementResistances = 0;

            deferTotalElementResistances = monsterElementResistances + buffElementResistances + instantBuffElementResistances;
        }

        objElementDamageMultiplierValue = atkerTotalElementDamageMultiplierValue - deferTotalElementResistances;
        objElementDamageMultiplierValue = objElementDamageMultiplierValue < -1 ? -1 : objElementDamageMultiplierValue;

        return objElementDamageMultiplierValue;
    }

    /// <summary>
    /// 获得英雄元素伤害加成
    /// </summary>
    /// <param name="atker"></param>
    /// <param name="elementType"></param>
    /// <returns></returns>
    public static float GetElementDamageByHero(BaseHero atker, BaseObject computeBase,int elementType)
    {
        if (atker == null || atker.bRecycle)
        {
            return 0;
        }
        switch (elementType)
        {
            case 0://普通伤害
                return computeBase.GetBattleAttr(EAttr.Dmg_Normal)/10000;
            case 1://水
                return computeBase.GetBattleAttr(EAttr.Dmg_Water)/10000;
            case 2://火
                return computeBase.GetBattleAttr(EAttr.Dmg_Fire)/10000;
            case 3://风
                return computeBase.GetBattleAttr(EAttr.Dmg_Wind)/10000;
            case 4://雷
                return computeBase.GetBattleAttr(EAttr.Dmg_Thunder)/10000;
            default:
                return 0;
        }
    }

    /// <summary>
    /// 获得英雄元素抗性加成
    /// </summary>
    /// <param name="defer"></param>
    /// <param name="elementType"></param>
    /// <returns></returns>
    public static float GetElementResistancesByHero(BaseHero defer, int elementType)
    {
        if (defer == null || defer.bRecycle)
        {
            return 0;
        }
        switch (elementType)
        {
            case 0://普通伤害
                return defer.GetBattleAttr(EAttr.TakeDmg_Normal)/10000;
            case 1://水
                return defer.GetBattleAttr(EAttr.TakeDmg_Water)/10000;
            case 2://火
                return defer.GetBattleAttr(EAttr.TakeDmg_Fire)/10000;
            case 3://风
                return defer.GetBattleAttr(EAttr.TakeDmg_Wind)/10000;
            case 4://雷
                return defer.GetBattleAttr(EAttr.TakeDmg_Thunder)/10000;
            default:
                return 0;
        }
    }

    /// <summary>
    /// 获得怪物元素伤害加成
    /// </summary>
    /// <param name="atker"></param>
    /// <param name="elementType"></param>
    /// <returns></returns>
    public static float GetElementDamageByMonster(BaseMonster atker, BaseObject computeBase, int elementType)
    {
        if (atker == null || atker.bRecycle)
        {
            return 0;
        }
        switch (elementType)
        {
            case 0://普通伤害
                return computeBase.GetBattleAttr(EAttr.Dmg_Normal) / 10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(atker.lv, 10200401) / 10000);
            case 1://水
                return computeBase.GetBattleAttr(EAttr.Dmg_Water) / 10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(atker.lv, 10200402) / 10000);
            case 2://火
                return computeBase.GetBattleAttr(EAttr.Dmg_Fire) / 10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(atker.lv, 10200403) / 10000);
            case 3://风
                return computeBase.GetBattleAttr(EAttr.Dmg_Wind) / 10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(atker.lv, 10200404) / 10000);
            case 4://雷
                return computeBase.GetBattleAttr(EAttr.Dmg_Thunder) / 10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(atker.lv, 10200405) / 10000);
            default:
                return 0;
        }
    }

    /// <summary>
    /// 获得怪物元素抗性加成
    /// </summary>
    /// <param name="defer"></param>
    /// <param name="elementType"></param>
    /// <returns></returns>
    public static float GetElementResistancesByMonster(BaseMonster defer, int elementType)
    {
        if (defer == null || defer.bRecycle)
        {
            return 0;
        }
        switch (elementType)
        {
            case 0://普通伤害
                return defer.GetBattleAttr(EAttr.TakeDmg_Normal) / 10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(defer.lv, 10200406) / 10000);
            case 1://水
                return defer.GetBattleAttr(EAttr.TakeDmg_Water) / 10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(defer.lv, 10200407) / 10000);
            case 2://火
                return defer.GetBattleAttr(EAttr.TakeDmg_Fire) / 10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(defer.lv, 10200408) / 10000);
            case 3://风
                return defer.GetBattleAttr(EAttr.TakeDmg_Wind) / 10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(defer.lv, 10200409) / 10000);
            case 4://雷
                return defer.GetBattleAttr(EAttr.TakeDmg_Thunder) / 10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(defer.lv, 10200410) / 10000);
            default:
                return 0;
        }
    }

    /// <summary>
    /// 计算对象暴击乘区
    /// </summary>
    /// <param name="baseObject">进攻方</param>
    /// <param name="computeBase">进攻方快照数据</param>
    /// <param name="target">防御方</param>
    /// <returns></returns>
    public static float ComputeBaseObjCriticalHitMultiplier(BaseObject baseObject, BaseObject computeBase, BaseObject target)
    {
        float objCriticalHitMultiplierValue = 0;

        //进攻方总暴击
        float atkerTotalCriticalHit = 0;
        //总爆伤
        float atkerTotalCriticalHitDamega = 0;
        //防御方总抗暴
        float deferTotalCritResistance = 0;
        //防御方暴击伤害抗性
        float deferTotalCriticalStrikeDamageResistance = 0;

        if (baseObject.objType == 1)//英雄
        {
            BaseHero baseHero = (BaseHero)baseObject;
            if (baseHero == null || baseHero.bRecycle )
            {
                return 0;
            }
            //英雄暴击
            float heroCriticalHit = computeBase.GetBattleAttr(EAttr.Crit);
            //装备暴击
            float equimentCriticalHit = 0;
            //天赋暴击
            float talentCriticalHit = 0;
            //法宝暴击
            float magicWeaponCriticalHit = 0;
            //buff暴击
            float buffCriticalHit = 0;
            //瞬时buff暴击
            float baseInstantBuffCriticalHit = 0;
            //羁绊暴击
            float baseFetterCriticalHit = 0;
            //其他属性转换的暴击
            float ohterCriticalHit = 0;
            atkerTotalCriticalHit = heroCriticalHit + equimentCriticalHit + talentCriticalHit + magicWeaponCriticalHit + buffCriticalHit + baseInstantBuffCriticalHit + baseFetterCriticalHit + ohterCriticalHit;


            //英雄暴击伤害
            float heroCriticalHitDamega = computeBase.GetBattleAttr(EAttr.Crit_Dmg);
            //装备暴击伤害
            float equimentCriticalHitDamega = 0;
            //天赋暴击伤害
            float talentCriticalHitDamega = 0;
            //法宝暴击伤害
            float magicWeaponCriticalHitDamega = 0;
            //buff暴击伤害
            float buffCriticalHitDamega = 0;
            //瞬时buff暴击伤害
            float baseInstantBuffCriticalHitDamega = 0;
            //羁绊暴击伤害
            float baseFetterCriticalHitDamega = 0;
            //其他属性转换的暴击伤害
            float ohterCriticalHitDamega = 0;
            atkerTotalCriticalHitDamega = heroCriticalHitDamega + equimentCriticalHitDamega+ talentCriticalHitDamega + magicWeaponCriticalHitDamega + buffCriticalHitDamega + baseInstantBuffCriticalHitDamega + baseFetterCriticalHitDamega + ohterCriticalHitDamega;

        }
        else if (baseObject.objType == 2)//怪物
        {
            BaseMonster baseMonster = (BaseMonster)baseObject;
            if (baseMonster == null || baseMonster.bRecycle)
            {
                return 0;
            }
            //怪物暴击
            float monsterCriticalHit = computeBase.GetBattleAttr(EAttr.Crit) * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(computeBase.lv, 10200105) / 10000);
            //buff暴击
            float buffCriticalHit = 0;
            //瞬时buff暴击
            float baseInstantBuffCriticalHit = 0;

            atkerTotalCriticalHit = monsterCriticalHit + buffCriticalHit + baseInstantBuffCriticalHit;

            //怪物暴击伤害
            float monsterCriticalHitDamega = computeBase.GetBattleAttr(EAttr.Crit_Dmg) * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(computeBase.lv, 10200107) / 10000); 
            //buff暴击伤害
            float buffCriticalHitDamega = 0;
            //瞬时buff暴击
            float baseInstantBuffCriticalHitDamega = 0;

            atkerTotalCriticalHitDamega = monsterCriticalHitDamega + buffCriticalHitDamega + baseInstantBuffCriticalHitDamega;       
        }

        //受击方
        if (target.objType == 1)
        {
            BaseHero baseHero = (BaseHero)target;
            if (baseHero == null || baseHero.bRecycle)
            {
                return 0;
            }
            //英雄抗暴
            float heroCritResistance = baseHero.GetBattleAttr(EAttr.Crit_Resist); 
            //装备抗暴
            float equimentCritResistance = 0;
            //天赋抗暴
            float talentCritResistance = 0;
            //法宝抗暴
            float magicWeaponCritResistance = 0;
            //buff抗暴
            float buffCritResistance = 0;
            //瞬时buff抗暴
            float baseInstantBuffCritResistance = 0;
            //羁绊抗暴
            float baseFetterCritResistance = 0;
            //其他属性转换的抗暴
            float ohterCritResistance = 0;

            deferTotalCritResistance = heroCritResistance + equimentCritResistance + talentCritResistance + magicWeaponCritResistance + buffCritResistance + baseInstantBuffCritResistance + baseFetterCritResistance + ohterCritResistance;

            //英雄暴击伤害抗性
            float heroCriticalStrikeDamageResistance = baseHero.GetBattleAttr(EAttr.Crit_Def);
            //装备暴击伤害抗性
            float equimentCriticalStrikeDamageResistance = 0;
            //天赋暴击伤害抗性
            float talentCriticalStrikeDamageResistance = 0;
            //法宝暴击伤害抗性
            float magicWeaponCriticalStrikeDamageResistance = 0;
            //buff暴击伤害抗性
            float buffCriticalStrikeDamageResistance = 0;
            //瞬时buff暴击伤害抗性
            float baseInstantBuffCriticalStrikeDamageResistance = 0;
            //羁绊暴击伤害抗性
            float baseFetterCriticalStrikeDamageResistance = 0;
            //其他属性转换的暴击伤害抗性
            float ohterCriticalStrikeDamageResistance = 0;

            deferTotalCriticalStrikeDamageResistance = heroCriticalStrikeDamageResistance + equimentCriticalStrikeDamageResistance+ talentCriticalStrikeDamageResistance + magicWeaponCriticalStrikeDamageResistance + buffCriticalStrikeDamageResistance + baseInstantBuffCriticalStrikeDamageResistance + baseFetterCriticalStrikeDamageResistance + ohterCriticalStrikeDamageResistance;
        }
        else if (target.objType == 2)//怪物
        {
            BaseMonster baseMonster = (BaseMonster)target;
            if (baseMonster == null || baseMonster.bRecycle)
            {
                return 0;
            }
            //怪物抗暴
            float monsterCritResistance = baseMonster.GetBattleAttr(EAttr.Crit_Resist) * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(baseMonster.lv, 10200106));
            //buff抗暴
            float buffCritResistance = 0;
            //瞬时buff抗暴
            float baseInstantBuffCritResistance = 0;

            deferTotalCritResistance = monsterCritResistance + buffCritResistance + baseInstantBuffCritResistance;


            //怪物暴击伤害抗性
            float monsterCriticalStrikeDamageResistance = baseMonster.GetBattleAttr(EAttr.Crit_Def) * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(baseMonster.lv, 10200108));
            //buff暴击伤害抗性
            float buffCriticalStrikeDamageResistance = 0;
            //瞬时buff暴击伤害抗性
            float baseInstantBuffCriticalStrikeDamageResistance = 0;

            deferTotalCriticalStrikeDamageResistance = monsterCriticalStrikeDamageResistance + buffCriticalStrikeDamageResistance + baseInstantBuffCriticalStrikeDamageResistance;
        }

        objCriticalHitMultiplierValue = atkerTotalCriticalHitDamega - deferTotalCriticalStrikeDamageResistance;
        return objCriticalHitMultiplierValue;
    }

    /// <summary>
    /// 增伤乘区
    /// </summary>
    /// <param name="baseObject"></param>
    /// <param name="target"></param>
    /// <param name="buffData"></param>
    /// <returns></returns>
    public static float ComputeBaseObjDamageBoostMultiplier(BaseObject baseObject, BaseObject computeBase, BaseObject target)
    {
        //对象增伤乘区系数
        float objDamageBoostMultiplierValue = 0;

        //进攻者的总增伤系数
        float atkerTotalDamageBoostValue = 0;
        //防御者的总减伤系数
        float deferTotalDamageResistanceValue = 0;

        if (baseObject.objType == 1)//英雄
        {
            BaseHero baseHero = (BaseHero)baseObject;
            if (baseHero == null || baseHero.bRecycle)
            {
                return 0;
            }
            //英雄伤害提高
            float heroDamageBoost = computeBase.GetBattleAttr(EAttr.Dmg_Per)/10000;
            //装备伤害提高
            float equimentDamageBoost = 0;
            //天赋伤害提高
            float talentDamageBoost = 0;
            //法宝伤害提高
            float magicWeaponDamageBoost = 0;
            //buff伤害提高
            float buffDamageBoost = 0;
            //瞬时buff伤害提高
            float instantBuffDamageBoost = 0;
            //羁绊伤害提高
            float fetterDamageBoost = 0;

            float ohter = 0;

            atkerTotalDamageBoostValue = heroDamageBoost + equimentDamageBoost + talentDamageBoost + magicWeaponDamageBoost + buffDamageBoost + instantBuffDamageBoost + fetterDamageBoost + ohter;
        }
        else if (baseObject.objType == 2)//怪物
        {
            BaseMonster baseMonster = (BaseMonster)baseObject;
            if (baseMonster == null || baseMonster.bRecycle)
            {
                return 0;
            }
            //怪物伤害提高
            float monsterDamageBoost = computeBase.GetBattleAttr(EAttr.Dmg_Per)/10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(computeBase.lv, 10200109)/10000);
            //buff伤害提高
            float buffDamageBoost = 0;
            //瞬时buff伤害提高
            float instantBuffDamageBoost = 0;

            atkerTotalDamageBoostValue = monsterDamageBoost + buffDamageBoost + instantBuffDamageBoost;
        }

        if (target.objType == 1)//英雄
        {
            BaseHero baseHero = (BaseHero)target;
            if (baseHero == null || baseHero.bRecycle)
            {
                return 0;
            }
            //英雄受伤减免
            float heroDamageResistance = baseHero.GetBattleAttr(EAttr.TakeDmg_Per)/10000;
            //装备受伤减免
            float equimentDamageResistance = 0;
            //天赋受伤减免
            float talentDamageResistance = 0;
            //法宝受伤减免
            float magicWeaponDamageResistance = 0;
            //buff受伤减免
            float buffDamageResistance = 0;
            //瞬时buff受伤减免
            float instantBuffDamageResistance = 0;
            //羁绊受伤减免
            float fetterDamageResistance = 0;

            float ohter = 0;

            deferTotalDamageResistanceValue = heroDamageResistance + equimentDamageResistance + talentDamageResistance + magicWeaponDamageResistance + buffDamageResistance + instantBuffDamageResistance + fetterDamageResistance + ohter;
        }
        else if (target.objType == 2)//怪物
        {
            BaseMonster baseMonster = (BaseMonster)target;
            if (baseMonster == null || baseMonster.bRecycle)
            {
                return 0;
            }
            //怪物受伤减免
            float monsterDamageResistance = baseMonster.GetBattleAttr(EAttr.TakeDmg_Per)/10000 * (MonsterDataManager.Instance.GetMonsterLevelAttrByLvAndAttrid(baseMonster.lv, 10200110) / 10000);
            //buff受伤减免
            float buffDamageResistance = 0;
            //瞬时受伤减免
            float instantBuffDamageResistance = 0;

            deferTotalDamageResistanceValue = monsterDamageResistance + buffDamageResistance + instantBuffDamageResistance;
        }

        objDamageBoostMultiplierValue = atkerTotalDamageBoostValue - deferTotalDamageResistanceValue;

        return objDamageBoostMultiplierValue;
    }



}

