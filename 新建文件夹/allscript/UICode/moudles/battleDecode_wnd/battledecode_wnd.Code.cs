using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗解密关卡界面/战前战后交互场景
/// </summary>
public partial class battledecode_wnd
{
    public override UILayerType uiLayerType => UILayerType.Float;

    public override string uiAtlasName => "";

    public BattleDecodeManager battleDecodeManager;//解密管理器

    private bool bDecode;//是否是战斗解密关卡
    private BattleMissionDecodeCfg decodeCfg;
    private List<BattleDecodeCondition> battleDecodeConditions;//完成解密关卡的条件列表

    protected override void OnOpen()
    {
        battleDecodeManager = (BattleDecodeManager)this.openArgs[0];
        bDecode = (bool)this.openArgs[1];

        if (bDecode)
        {
            decodeCfg = BattleCfgManager.Instance.GetDecodeCfgByMission(battleDecodeManager.curMission);
            //生成解密关卡通关条件的数据列表
            battleDecodeConditions = new List<BattleDecodeCondition>();
            if (!string.IsNullOrEmpty(decodeCfg.passtype1) && !string.IsNullOrEmpty(decodeCfg.passcond1) && decodeCfg.passtype1 != "-1" && decodeCfg.passcond1 != "-1")
                battleDecodeConditions.Add(new BattleDecodeCondition(decodeCfg.passtype1, decodeCfg.passcond1));
            if (!string.IsNullOrEmpty(decodeCfg.passtype2) && !string.IsNullOrEmpty(decodeCfg.passcond2) && decodeCfg.passtype2 != "-1" && decodeCfg.passcond2 != "-1")
                battleDecodeConditions.Add(new BattleDecodeCondition(decodeCfg.passtype2, decodeCfg.passcond2));
            if (!string.IsNullOrEmpty(decodeCfg.passtype3) && !string.IsNullOrEmpty(decodeCfg.passcond3) && decodeCfg.passtype3 != "-1" && decodeCfg.passcond3 != "-1")
                battleDecodeConditions.Add(new BattleDecodeCondition(decodeCfg.passtype3, decodeCfg.passcond3));
            GameEventMgr.Register(GEKey.NpcInteracton_OnInteractionEcd, OnInteractionEndByDecode);
        }
        else {
            GameEventMgr.Register(GEKey.NpcInteracton_OnInteractionEcd, OnInteractionEndByNotDecode);
        }
    }

    /// <summary>
    /// 解密关卡交互监听
    /// </summary>
    /// <param name="args"></param>
    private void OnInteractionEndByDecode(GEventArgs args)
    {
        if (decodeCfg != null)
        {
            //当前完成的交互ID
            if (args == null) return;
                InteractionConfig interaction = args.args[0] as InteractionConfig;

            RefreshConditionByInteraction(interaction.id);
            CheckBattleDecodeCondition();
        }
    }

    /// <summary>
    /// 交互结束刷新条件列表
    /// </summary>
    /// <param name="interID">本次监听到的id</param>
    private void RefreshConditionByInteraction(long interID)
    {
        for (int i = 0; i < battleDecodeConditions.Count; i++)
        {
            if (battleDecodeConditions[i].conditionType == 1)//只检测交互id完成类型
            {
                List<ConditionParam> @params = battleDecodeConditions[i].conditionList;
                for (int p = 0; p < @params.Count; p++)
                {
                    if (@params[p].param == interID.ToString())
                    {
                        @params[p].bCheck = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检测条件是否满足通关
    /// </summary>
    private void CheckBattleDecodeCondition()
    {
        bool bCheck = true;
        for (int i = 0; i < battleDecodeConditions.Count; i++)
        {
            if (!battleDecodeConditions[i].GetBool())
            {
                bCheck = false;
            }
        }

        if (bCheck)
        {

            BattleMsgManager.Instance.SendResultMsg(decodeCfg.mission, 3, BattleMsgManager.Instance.randomSed, "", () => {
                battleDecodeManager.QuitBattleDecode();
                this.Close();
            });
        }
    }

    /// <summary>
    /// 战前战斗交互监听
    /// </summary>
    /// <param name="args"></param>
    private void OnInteractionEndByNotDecode(GEventArgs args)
    {
        if (args == null) return;
            InteractionConfig interaction = args.args[0] as InteractionConfig;
        if (battleDecodeManager.CheckInteract(interaction.id) && battleDecodeManager.curTimeing == 1)
        {

            GameCenter.mIns.m_BattleMgr.RequstBattle(battleDecodeManager.curMission,1,() => {
                this.Close();
            });
            battleDecodeManager.QuitBattleDecode();


        }
        else if (battleDecodeManager.CheckInteract(interaction.id) && battleDecodeManager.curTimeing == 2)
        {
            battleDecodeManager.QuitBattleDecode();
            this.Close();
        }
    }

    protected override void OnClose()
    {
        if (bDecode)
        {

        }
        else
        {
            GameEventMgr.UnRegister(GEKey.NpcInteracton_OnInteractionEcd, OnInteractionEndByNotDecode);
        }

    }

    /// <summary>
    /// 返回按钮
    /// </summary>
    partial void btn_back_Click()
    {
        this.Close();
        if (battleDecodeManager!= null)
        {
            battleDecodeManager.QuitBattleDecode();
        }
    }



    partial void btnResetLens_Click()
    {
    }

    partial void btn_setting_Click()
    {
    }

    partial void btn_Switch_Click()
    {
    }

}

public class BattleDecodeCondition
{
    public int conditionType;//条件类型 1-交互id完成

    public int checkType;//检测类型 1-与 2-或

    public List<ConditionParam> conditionList;

    public BattleDecodeCondition(string passTypem,string passCond)
    {
        string[] passes = passTypem.Split(';');
        conditionType = int.Parse(passes[0]);
        checkType = int.Parse(passes[1]);

        conditionList = new List<ConditionParam>();
        string[] passConds = passCond.Split(';');
        for (int i = 0; i < passConds.Length; i++)
        {
            conditionList.Add(new ConditionParam(passConds[i]));
        }
    }

    public bool GetBool()
    {
        if (checkType == 1)//
        {
            bool bAll = true;
            for (int i = 0; i < conditionList.Count; i++)
            {
                if (!conditionList[i].bCheck)
                {
                    bAll = false;
                }
            }
            return bAll;
        }
        else//或
        {
            bool bOne = false;
            for (int i = 0; i < conditionList.Count; i++)
            {
                if (conditionList[i].bCheck)
                {
                    bOne = true;
                }
            }
            return bOne;
        }
    }
}

public class ConditionParam
{
    public bool bCheck;//是否完成

    public string param;//检测参数

    public ConditionParam(string param)
    {
        this.param = param;
        bCheck = false;
    }
}

