using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LitJson;
using JetBrains.Annotations;
using System.Threading;
using System.Collections;

public class WeaponUpgradesProp
{
    private Transform obj;
    private List<UpgradesPropItem> items = new List<UpgradesPropItem>();

    public int maxExp = 0;//当前升到满级需要的最大经验值 
    public int totalExp = 0;//总消耗的EXP
    public int totalCoin = 0;//总消耗的金币
    public int tolevel = 0;//升到的等级
    public int overflowExp = 0;
    private HeroData data;
    private Action<int, int, float, int, bool> levelChange;

    public WeaponUpgradesProp(Transform obj, WeaponDataCfg weaponData, HeroData data, Action<int, int, float, int, bool> levelChange)
    {
        this.obj = obj;
        this.levelChange = levelChange;
        this.data = data;
        if (items.Count == 0)
        {
            for (int i = 1; i <= 3; i++)
            {
                UpgradesPropItem item = new UpgradesPropItem(obj, i,10020, "addweaponexp_", "weaponexptomoney_", (exp) => {
                    return onExpChange(exp);
                }, (bool isChangeStatsInfo) => {
                    onCountChange(isChangeStatsInfo);
                });
                items.Add(item);
            }
        }
    }
    public void getPropCounts(JsonData data)
    {
        foreach (var item in items)
        {
            data["wp" + item.index] = item.count;
        }
        data["tolevel"] = tolevel;
        data["coin"] = totalCoin;
    }
    public int onExpChange(int exp)
    {
        return maxExp - totalExp - data.weaponexp - exp;
    }
    public void onCountChange(bool isChangeStatsInfo)
    {
        totalExp = 0;
        totalCoin = 0;
        foreach (var item in items)
        {
            totalExp += item.count * item.exp;
            totalCoin += item.count * item.getCoin();
        }
        if (totalExp <= 0)
        {
            tolevel = data.weaponLevel;
            levelChange?.Invoke(data.weaponLevel, data.weaponexp, 0, 0, isChangeStatsInfo);
            return;
        }
        else if (totalExp > maxExp - data.weaponexp)
        {
            overflowExp = totalExp - maxExp - data.weaponexp;
            totalExp = maxExp - data.weaponexp;
        }

        onExp(totalExp, isChangeStatsInfo);
    }
    public void onExp(int exp, bool isChangeStatsInfo)
    {
        HeroInfoCfgData heroInfo = GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(data.heroID);
        WeaponBreakCfg breakData = heroInfo.getCurrWeaponBreakData(data.weaponstate);
        //计算传过来的经验能增加到多少级
        int oldExp = data.weaponexp;//英雄当前已有的EXP值 
        int wLevel = data.weaponLevel;//能达到的等级
        int xExp = 0;//已消耗的EXP
        int kExp = exp;//可消耗的EXP
        int wExp = 0;//当前英雄已有经验 ，计算后的
        while (kExp > 0 && wLevel < breakData.level)
        {
            WeaponUpCostCfg levelData = heroInfo.getWeaponLevelData(wLevel);

            if (levelData == null)
            {
                //取不到等级数据了，跳出
                break;
            }
            if (kExp < levelData.weaponexp - oldExp)
            {
                //如果可消耗的EXP
                wLevel = levelData.level;
                wExp = kExp + oldExp;
                xExp += kExp;
                kExp = 0;
                oldExp = 0;

                break;
            }
            else
            {
                xExp += (levelData.weaponexp - oldExp);
                kExp -= (levelData.weaponexp - oldExp);
                oldExp = 0;
                wLevel += 1;
                wExp = 0;
            }
        }
        if (xExp > 0)
        {
            WeaponUpCostCfg levelData = heroInfo.getCurrWeaponLevelData(wLevel,data.weaponstate);
            float scale = float.Parse(wExp + "") / float.Parse(levelData.weaponexp + "");

            tolevel = wLevel;
            levelChange?.Invoke(wLevel, wExp, ((float)Math.Round(scale, 2)), totalExp, isChangeStatsInfo);
        }
    }
    /// <summary>
    /// 清空选择的道具
    /// </summary>
    public void clearProp()
    {
        for (int i = 0; i < 3; i++)
        {
            items[i].setCount(0);
        }
        onCountChange(true);
    }
    /// <summary>
    /// 自动置入
    /// </summary>
    public void onAI()
    {
        int[] props = new int[3] { 0, 0, 0 };
        int onexp = maxExp - data.weaponexp;

        int exp1 = items[0].getExp();
        int propcount1 = GameCenter.mIns.userInfo.getPropCount(items[0].propid);

        int exp2 = items[1].getExp();
        int propcount2 = GameCenter.mIns.userInfo.getPropCount(items[1].propid);

        int exp3 = items[2].getExp();
        int propcount3 = GameCenter.mIns.userInfo.getPropCount(items[2].propid);
        if (propcount1 < 1 && propcount2 < 1 && propcount3 < 1)
        {
            GameCenter.mIns.m_UIMgr.PopMsg("没有可使用的道具");
            return;
        }
        while (onexp > 0)
        {
            if (propcount3 - props[2] < 1 && propcount2 - props[1] < 1 && propcount1 - props[0] < 1)
            {
                break;
            }
            else if (onexp >= exp3 && propcount3 - props[2] > 0)
            {
                int c = onexp / exp3;
                c = propcount3 >= c ? c : propcount3;
                props[2] += c;
                onexp -= props[2] * items[2].getExp();
                continue;
            }
            else if (propcount1 < 1 && propcount2 < 1 && propcount3 - props[2] > 0)
            {
                props[2] += 1;
                onexp -= items[2].getExp();
                continue;
            }
            else if (onexp >= exp2 && propcount2 - props[1] > 0)
            {
                int c = onexp / exp2;
                c = propcount2 >= c ? c : propcount2;
                props[1] += c;
                onexp -= props[1] * items[1].getExp();
                continue;
            }
            else if (propcount1 < 1 && propcount2 - props[1] > 0)
            {
                props[1] += 1;
                onexp -= items[1].getExp();
                continue;
            }
            else if (onexp >= exp1 && propcount1 - props[0] > 0)
            {
                int c = onexp / exp1;
                c = propcount1 >= c ? c : propcount1;
                props[0] += c;
                onexp -= props[0] * items[0].getExp();
                continue;
            }
            else if (onexp < exp1 && propcount1 - props[0] > 0)
            {
                props[0] += 1;
                onexp -= items[0].getExp();
                continue;
            }
        }

        //计算完毕，开始更新UI
        for (int i = 0; i < props.Length; i++)
        {
            items[i].setCount(props[i]);
        }
        onCountChange(true);

    }
    public void reset()
    {
        //计算当前能升级需要的最大经验值
        HeroInfoCfgData heroInfo = GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(data.heroID);
        
        WeaponBreakCfg breakData = heroInfo.getCurrWeaponBreakData(data.weaponstate);
        maxExp = 0;
        totalExp = 0;
        int ix = data.weaponLevel;
        while (ix <= breakData.level)
        {
            WeaponUpCostCfg curLevData = heroInfo.getCurrWeaponLevelData(ix, data.weaponstate);
            maxExp += (curLevData.weaponexp > 0 ? curLevData.weaponexp : 0);
            ix++;
        }
        foreach (var item in items)
        {
            item.reset();
        }
    }
    public void OnDestroy()
    {
    }
}

