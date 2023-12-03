using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using LitJson;
using UnityEngine;
/// <summary>
/// 战斗配置表管理
/// </summary>
public  class BattleCfgManager:SingletonNotMono<BattleCfgManager>
{
    //技能总表配置路径
    private static string skillCfg = "t_skill";
    //技能buff配置路径
    private static string skillBuffCfg = "t_skill_buff";
    //技能子弹配置路径
    private static string skillBulletCfg = "t_skill_bullet";
    //技能条件配置路径
    private static string skillConditionCfg = "t_skill_condition";
    //技能逻辑配置路径
    private static string skillLogicCfg = "t_skill_logic";
    //技能天赋配置路径
    private static string skillTalentCfg = "t_skill_talent";
    //技能数值配置路径
    private static string skillValueCfg = "t_skill_value";
    //战斗关卡参数配置路径
    private static string missionParamCfg = "t_mission_battle";
    //战斗剧情交互配置路径
    private static string missionInteractCfg = "t_mission_interact";
    //战斗剧情交互配置路径
    private static string missionTalkCfg = "t_mission_talk";
    //战斗解密关卡配置路径
    private static string missionDecodeCfg = "t_mission_decode";

    private List<BattleSkillCfg>  battleSkillCfgs;//技能总表配置数据

    private List<BattleSkillBuffCfg>  battleSkillBuffCfgs;//技能buff配置数据

    private List<BattleSkillBulletCfg>  battleSkillBulletCfgs;//技能子弹配置数据

    private List<BattleSkillConditionCfg>  battleSkillConditionCfgs;//技能条件配置数据

    private List<BattleSkillLogicCfg>  battleSkillLogicCfgs;//技能逻辑配置数据

    private List<BattleSkillTalentCfg>  battleSkillTalentCfgs;//技能天赋配置数据

    private List<BattleSkillValueCfg>  battleSkillValueCfgs;//技能配置数据

    private List<BattleMissionParamCfg> battleMissionParamCfgs;//战斗关卡参数表

    private List<BattleInteractCfg> battleInteractCfgs;//战斗剧情交互表

    private List<BattleTalkCfg> battleTalkCfgs;//战斗剧情对话表

    private List<BattleMissionDecodeCfg> battleMissionDecodeCfgs;

    private Dictionary<long, BattleSkillCfg> dicBattleSkill;//战斗总表配置字典 key-skillid

    private Dictionary<long, BattleSkillBuffCfg> dicBattleSkillBuff;//技能buff配置数据 key-buffid

    private Dictionary<long, BattleSkillBulletCfg> dicBattleSkillBullet;//技能子弹配置数据 key-buffid

    private Dictionary<long, BattleSkillConditionCfg> dicBattleSkillCondition;//技能条件配置数据 key-conditionid

    private Dictionary<long, BattleSkillLogicCfg> dicBattleSkillLogic;//技能逻辑配置数据 key-效果id

    private Dictionary<long, BattleSkillTalentCfg> dicBattleSkillTalent;//技能天赋配置数据 key-天赋id

    private Dictionary<long, BattleSkillValueCfg> dicBattleSkillValue;//技能数值表 key-数值id

    private Dictionary<long, BattleMissionParamCfg> dicBattleMissionParam;//战斗关卡参数字典 key-关卡id

    private Dictionary<long, List<BattleInteractCfg>> dicBattleInteract;//战斗剧情交互表 key-关卡id

    private Dictionary<long, BattleTalkCfg> dicBattleTalk;//战斗剧情对话表-key-交互id

    private Dictionary<long, BattleMissionDecodeCfg> dicMissionDecode;//战斗解密关卡表


    /// <summary>
    /// 初始化战斗配置表
    /// </summary>
    public void InitBattleCfg()
    {
        battleSkillCfgs = GameCenter.mIns.m_CfgMgr.JsonToListClass<BattleSkillCfg>(skillCfg);
        dicBattleSkill = new Dictionary<long, BattleSkillCfg>();
        for (int i = 0; i < battleSkillCfgs.Count; i++)
        {
            dicBattleSkill.Add(battleSkillCfgs[i].skillid, battleSkillCfgs[i]);
        }

        battleSkillBuffCfgs =  GameCenter.mIns.m_CfgMgr.JsonToListClass<BattleSkillBuffCfg>(skillBuffCfg);
        dicBattleSkillBuff = new Dictionary<long, BattleSkillBuffCfg>();
        for (int i = 0; i < battleSkillBuffCfgs.Count; i++)
        {
            dicBattleSkillBuff.Add(battleSkillBuffCfgs[i].buffid, battleSkillBuffCfgs[i]);
        }

        battleSkillBulletCfgs = GameCenter.mIns.m_CfgMgr.JsonToListClass<BattleSkillBulletCfg>(skillBulletCfg);
        dicBattleSkillBullet = new Dictionary<long, BattleSkillBulletCfg>();
        for (int i = 0; i < battleSkillBulletCfgs.Count; i++)
        {
            dicBattleSkillBullet.Add(battleSkillBulletCfgs[i].bulletid, battleSkillBulletCfgs[i]);
        }

        battleSkillConditionCfgs = GameCenter.mIns.m_CfgMgr.JsonToListClass<BattleSkillConditionCfg>(skillConditionCfg);
        dicBattleSkillCondition = new Dictionary<long, BattleSkillConditionCfg>();
        for (int i = 0; i < battleSkillConditionCfgs.Count; i++)
        {
            dicBattleSkillCondition.Add(battleSkillConditionCfgs[i].conditionid, battleSkillConditionCfgs[i]);
        }

        battleSkillLogicCfgs = GameCenter.mIns.m_CfgMgr.JsonToListClass<BattleSkillLogicCfg>(skillLogicCfg);
        dicBattleSkillLogic = new Dictionary<long, BattleSkillLogicCfg>();
        for (int i = 0; i < battleSkillLogicCfgs.Count; i++)
        {
            dicBattleSkillLogic.Add(battleSkillLogicCfgs[i].effectid, battleSkillLogicCfgs[i]);
        }

        battleSkillTalentCfgs = GameCenter.mIns.m_CfgMgr.JsonToListClass<BattleSkillTalentCfg>(skillTalentCfg);
        dicBattleSkillTalent = new Dictionary<long, BattleSkillTalentCfg>();
        for (int i = 0; i < battleSkillTalentCfgs.Count; i++)
        {
            dicBattleSkillTalent.Add(battleSkillTalentCfgs[i].talentid, battleSkillTalentCfgs[i]);
        }

        battleSkillValueCfgs = GameCenter.mIns.m_CfgMgr.JsonToListClass<BattleSkillValueCfg>(skillValueCfg);
        dicBattleSkillValue = new Dictionary<long, BattleSkillValueCfg>();
        for (int i = 0; i < battleSkillValueCfgs.Count; i++)
        {
            dicBattleSkillValue.Add(battleSkillValueCfgs[i].valueid, battleSkillValueCfgs[i]);
        }

        battleMissionParamCfgs = GameCenter.mIns.m_CfgMgr.JsonToListClass<BattleMissionParamCfg>(missionParamCfg);
        dicBattleMissionParam = new Dictionary<long, BattleMissionParamCfg>();
        for (int i = 0; i < battleMissionParamCfgs.Count; i++)
        {
            dicBattleMissionParam.Add(battleMissionParamCfgs[i].missionid, battleMissionParamCfgs[i]);
        }

        battleInteractCfgs = GameCenter.mIns.m_CfgMgr.JsonToListClass<BattleInteractCfg>(missionInteractCfg);
        dicBattleInteract = new Dictionary<long, List<BattleInteractCfg>>();
        for (int i = 0; i < battleInteractCfgs.Count; i++)
        {
            if (!dicBattleInteract.ContainsKey(battleInteractCfgs[i].mission))
            {
                dicBattleInteract.Add(battleInteractCfgs[i].mission, new List<BattleInteractCfg>());
            }
            dicBattleInteract[battleInteractCfgs[i].mission].Add(battleInteractCfgs[i]);
        }

        battleTalkCfgs = GameCenter.mIns.m_CfgMgr.JsonToListClass<BattleTalkCfg>(missionTalkCfg);
        dicBattleTalk = new Dictionary<long, BattleTalkCfg>();
        for (int i = 0; i < battleTalkCfgs.Count; i++)
        {
            dicBattleTalk.Add(battleTalkCfgs[i].id, battleTalkCfgs[i]);
        }

        battleMissionDecodeCfgs = GameCenter.mIns.m_CfgMgr.JsonToListClass<BattleMissionDecodeCfg>(missionDecodeCfg);
        dicMissionDecode = new Dictionary<long, BattleMissionDecodeCfg>();
        for (int i = 0; i < battleMissionDecodeCfgs.Count; i++)
        {
            dicMissionDecode.Add(battleMissionDecodeCfgs[i].mission, battleMissionDecodeCfgs[i]);
        }

        MonsterAIManager.ins.InitAllMonsterAI();
    }

    /// <summary>
    /// 获得技能总表配置
    /// </summary>
    /// <param name="skillid"></param>
    /// <returns></returns>
    public BattleSkillCfg GetSkillCfgBySkillID(long skillid)
    {
        if (dicBattleSkill.ContainsKey(skillid))
        {
            return dicBattleSkill[skillid];
        }
        else
        {
            if (skillid != 0 && skillid % 100 <= 4) 
            {
                Debug.LogError($"未在技能表中找到id为{skillid}的技能配置！！！！");
            }
        }
        return null;
    }

    /// <summary>
    /// 获得技能buff配置
    /// </summary>
    /// <param name="buffid"></param>
    /// <returns></returns>
    public BattleSkillBuffCfg GetBuffCfg(long buffid)
    {
        if (dicBattleSkillBuff.ContainsKey(buffid))
        {
            return dicBattleSkillBuff[buffid];
        }
        return null;
    }

    /// <summary>
    /// 获得子弹配置
    /// </summary>
    /// <param name="bulletid"></param>
    /// <returns></returns>
    public BattleSkillBulletCfg GetBulletCfg(long bulletid)
    {
        if (dicBattleSkillBullet.ContainsKey(bulletid))
        {
            return dicBattleSkillBullet[bulletid];
        }
        return null;
    }

    /// <summary>
    /// 获得条件配置
    /// </summary>
    /// <param name="conditionid"></param>
    /// <returns></returns>
    public BattleSkillConditionCfg GetConditionCfg(long conditionid)
    {
        if (dicBattleSkillCondition.ContainsKey(conditionid))
        {
            return dicBattleSkillCondition[conditionid];
        }
        return null;
    }

    /// <summary>
    /// 获得技能逻辑配置
    /// </summary>
    /// <param name="effectid"></param>
    /// <returns></returns>
    public BattleSkillLogicCfg GetLogicCfg(long effectid)
    {
        if (dicBattleSkillLogic.ContainsKey(effectid))
        {
            return dicBattleSkillLogic[effectid];
        }
        return null;
    }

    /// <summary>
    /// 获得技能天赋配置
    /// </summary>
    /// <param name="talentid"></param>
    /// <returns></returns>
    public BattleSkillTalentCfg GetTalentCfg(long talentid)
    {
        if (dicBattleSkillTalent.ContainsKey(talentid))
        {
            return dicBattleSkillTalent[talentid];
        }
        return null;
    }

    /// <summary>
    /// 获得技能数值配置
    /// </summary>
    /// <param name="valueid"></param>
    /// <returns></returns>
    public BattleSkillValueCfg GetSkillValueCfg(long valueid)
    {
        if (dicBattleSkillValue.ContainsKey(valueid))
        {
            return dicBattleSkillValue[valueid];
        }
        else
        {
            Debug.LogError($"未在技能数值表中找到id为{valueid}的配置，请检查！");
        }
        return null;
    }

    /// <summary>
    /// 获得关卡参数配置
    /// </summary>
    /// <returns></returns>
    public BattleMissionParamCfg GetMissionParamCfg(long missionId)
    {
        if (dicBattleMissionParam.ContainsKey(missionId))
        {
            return dicBattleMissionParam[missionId];
        }
        return null;
    }

    /// <summary>
    /// 获得技能配置表中的具体数值
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public string GetSkillCfgNoteValue(BattleSkillCfg skillCfg,int skillLv)
    {
        string note = GameCenter.mIns.m_LanMgr.GetLan(skillCfg.note);
        //正则获取所有括号内容
        Match[] matches = Regex.Matches(note, @"(?<={).*?(?=})").ToArray();
        for (int i = 0; i < matches.Length; i++)
        {
            string[] computes = matches[i].Value.Split('.');
            BattleSkillValueCfg valueCfg = BattleCfgManager.Instance.GetSkillValueCfg(long.Parse(computes[0]));
            string value = null;
            switch (computes[1])
            {
                case "basevalue":
                    string[] basevalues = valueCfg.basevalue.Split('|');
                    if (basevalues.Length >= skillLv)
                    {
                        value = basevalues[skillLv - 1];
                    }
                    else
                    {
                        Debug.Log($"技能{skillCfg.skillid}等级超出技能数值表长度，请检查");
                    }
                    break;
                case "value":
                    string[] showvalue = valueCfg.showvalue.Split('|');
                    if (showvalue.Length >= skillLv)
                    {
                        value = showvalue[skillLv - 1];

                        value = $"{long.Parse(value) / 100f}%";
                    }
                    else
                    {
                        Debug.Log($"技能{skillCfg.skillid}等级超出技能数值表长度，请检查");
                    }
                    break;
            }
            bool b = string.Equals("{" + matches[i].Value + "}", "{1010010101.showvalue}");
            note = note.Replace("{" + matches[i].Value + "}", value);
        }
        return note;
    }


    /// <summary>
    /// 获得关卡的剧情交互配置
    /// </summary>
    /// <param name="interact"></param>
    /// <returns></returns>
    public BattleInteractCfg GetBattleInteractByInteract(long interact)
    {
        return battleInteractCfgs.Find(x => x.interact == interact);
    }

    /// <summary>
    /// 获得关卡的剧情交互配置
    /// </summary>
    /// <param name="mission">关卡id</param>
    /// <param name="param">触发类型</param>
    public BattleInteractCfg GetBattleInteractByMissionAndTimer(long mission ,int param = -1)
    {
        if (dicBattleInteract.ContainsKey(mission))
        {
            return dicBattleInteract[mission].Find(i => i.param == param);
        }
        //Debug.LogError($"未在关卡剧情交互表中找到关卡为{mission}，时机为{param}的配置请检查");
        return null;
    }

    /// <summary>
    /// 获得关卡的对话配置
    /// </summary>
    /// <param name="talkid"></param>
    public BattleTalkCfg GetBattleTalkCfgByID(long talkid)
    {
        if (dicBattleTalk.ContainsKey(talkid))
        {
            return dicBattleTalk[talkid];
        }
        Debug.LogError($"未在关卡对话表中找到id为{talkid}的配置请检查");
        return null;
    }

    /// <summary>
    /// 获得关卡解密配置
    /// </summary>
    /// <returns></returns>
    public BattleMissionDecodeCfg GetDecodeCfgByMission(long mission)
    {
        if (dicMissionDecode.ContainsKey(mission))
        {
            return dicMissionDecode[mission];
        }
        Debug.LogError($"未在关卡解密表中找到关卡id为{mission}的配置，请检查！");
        return null;
    }
}

