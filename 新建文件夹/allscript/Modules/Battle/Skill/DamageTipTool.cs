using System;
using UnityEngine;
using TMPro;

/// <summary>
/// 伤害跳字工具
/// </summary>
public class DamageTipTool
{
    private static DamageTipTool Ins;
    public static DamageTipTool ins
    {
        get
        {
            if (Ins == null)
            {
                Ins = new DamageTipTool();
            }
            return Ins;
        }
    }
    private GameObject DamageTip;//伤害文本预支体

	/// <summary>
	/// 一次伤害跳字
	/// </summary>
	/// <param name="parent"></param>
	/// <param name="value"></param>
	/// <param name="color"></param>
	public async void ShowDamageTip(Transform parent,float value,int type,int size)
	{
        int element = 0;
        switch (type)
        {
            case 100://普通
                element = 0;
                break;
            case 101://水
                element = 8;
                break;
            case 102://火
                element = 4;
                break;
            case 103://风
                element = 2;
                break;
            case 104://雷
                element = 6;
                break;
            default:
                break;
        }
        if (DamageTip == null)
        {
            DamageTip = await ResourcesManager.Instance.LoadUIPrefabSync("DamageTMP");
        }
        GameObject tip = BattlePoolManager.Instance.OutPool(ERootType.DamageTMP);
        if (tip == null)
        {
            tip = GameObject.Instantiate(DamageTip);
        }
        TextMeshPro textMesh = tip.GetComponent<TextMeshPro>();
        textMesh.text = "";
        textMesh.fontSize = size;
        tip.transform.SetParent(GameCenter.mIns.m_BattleMgr.battleRoot.transform);
        tip.transform.position = parent.transform.position;
        tip.GetOrAddCompoonet<DamageTipCompent>().ShowTip(value, element);
    }
}

