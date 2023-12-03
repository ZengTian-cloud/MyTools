using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LitJson;
using JetBrains.Annotations;
using System.Threading;
using System.Collections;

public class HeroUpgradesProp
{
    private Transform obj;
    private List<UpgradesPropItem> items = new List<UpgradesPropItem>();

    public int maxExp = 0;//当前升到满级需要的最大经验值 
    public int totalExp = 0 ;//总消耗的EXP
    public int totalCoin = 0;//总消耗的金币
    public int tolevel = 0;//升到的等级
    public int overflowExp = 0;
    private HeroLevelData levelData;
    private HeroData data;
    private Action<int,int,float,int,bool> levelChange;

    public HeroUpgradesProp(Transform obj, HeroLevelData levelData, HeroData data,Action<int,int, float,int,bool> levelChange)
    {
        this.obj = obj;
        this.levelChange = levelChange;
        this.levelData = levelData;
        this.data = data;
        if (items.Count == 0) { 
            for (int i = 1; i <= 3; i++)
            {
                UpgradesPropItem item =new UpgradesPropItem(obj, i,10010, "addexp_", "exptomoney_", (exp)=> {
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
            data["book" + item.index]=item.count;
        }
        data["tolevel"] = tolevel;
        data["coin"] = totalCoin;
    }
    public int onExpChange(int exp)
    {
        return maxExp - totalExp - data.lvexp - exp;
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
            tolevel = data.level;
            levelChange?.Invoke(data.level, data.lvexp, 0, 0, isChangeStatsInfo);
            return;
        }
        else if (totalExp > maxExp - data.lvexp)
        {
            overflowExp = totalExp - maxExp - data.lvexp;
            totalExp = maxExp - data.lvexp;
        }

        onExp(totalExp, isChangeStatsInfo);
    }
    public void onExp(int exp,bool isChangeStatsInfo)
    {
        HeroInfoCfgData heroInfo = GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(data.heroID);
        HeroBreakData breakData = heroInfo.getCurrBreakData(data.state);
        //计算传过来的经验能增加到多少级

        int oldExp = data.lvexp;//英雄当前已有的EXP值 
        int wLevel = data.level;//能达到的等级
        int xExp = 0;//已消耗的EXP
        int kExp = exp;//可消耗的EXP
        int wExp = 0;//当前英雄已有经验 ，计算后的
        while (kExp>0&& wLevel < breakData.herolevel)
        {
            HeroLevelData levelData = heroInfo.getLevelData(wLevel);
            if (levelData == null)
            {
                //取不到等级数据了，跳出
                break;
            }
            if (kExp < levelData.exp - oldExp)
            {
                //如果可消耗的EXP
                wLevel = levelData.herolevel;
                wExp = kExp + oldExp;
                xExp += kExp;
                kExp = 0;
                oldExp = 0;
                
                break;
            }
            else {
                xExp += (levelData.exp - oldExp);
                kExp -= (levelData.exp - oldExp);
                oldExp = 0;
                wLevel += 1;
                wExp = 0;
            }
        }
        if (xExp > 0)
        {
            HeroLevelData levelData = heroInfo.getCurrLevelData(wLevel, data.state);
            float scale = float.Parse(wExp + "") / float.Parse(levelData.exp+"");

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
        int[] props = new int[3] { 0,0,0};
        int onexp = maxExp-data.lvexp;

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
            if (propcount3 - props[2] <1 && propcount2 - props[1] <1 && propcount1 - props[0] <1)
            {
                break;
            }
            else if (onexp >= exp3&& propcount3 - props[2]>0)
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
            else if (onexp >= exp2&& propcount2 - props[1] > 0)
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
        for (int i=0;i<props.Length;i++)
        {
            items[i].setCount(props[i]);
        }
        onCountChange(true);

    }
    public void reset()
    {
        //计算当前能升级需要的最大经验值
        HeroInfoCfgData heroInfo = GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(data.heroID);
        HeroBreakData breakData = heroInfo.getCurrBreakData(data.state);
        maxExp = 0;
        totalExp = 0;
        int ix = data.level;
        while (ix <= breakData.herolevel)
        {
            HeroLevelData curLevData = heroInfo.getCurrLevelData(ix, data.state);
            maxExp += (curLevData.exp > 0 ? curLevData.exp : 0);
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

    private class HeroUpgradesPropItem
    {
        private Transform parent;
        private ExButton propBtn;
        private ExButton minBtn;
        private TextMeshProUGUI txtCount;
        public int propid;
        public int count = 0;
        public int exp = 0;
        public int index;

        private Func<int,int> expChange;
        private Action<bool> countChange;
        private Coroutine coroutine;
        private bool isMouseUp;
        private float longPressDuration = 0.05f;
        public HeroUpgradesPropItem(Transform parent,int index, Func<int,int> expChange,Action<bool> countChange)
        {
            this.parent = parent;
            this.index  = index;
            txtCount = parent.Find("cailiao" + index + "/countbox/txt").gameObject.GetComponent<TextMeshProUGUI>();
            propid = 10010 + index;
            exp = getExp();

            propBtn = parent.Find("cailiao" + index + "/whItem").gameObject.GetComponent<ExButton>();
            minBtn = parent.Find("cailiao" + index + "/btn").gameObject.GetComponent<ExButton>();

            propBtn.onClick.RemoveAllListeners();
            propBtn.onClick.AddListener(propBtnClick);
            propBtn.OnLongPress.RemoveAllListeners();
            propBtn.OnLongPress.AddListener(propBtnLongPress);
            propBtn.OnMouseUp.RemoveAllListeners();
            propBtn.OnMouseUp.AddListener(propMouseUp);
            minBtn.onClick.RemoveAllListeners();
            minBtn.onClick.AddListener(minBtnClick);
            minBtn.OnLongPress.RemoveAllListeners();
            minBtn.OnLongPress.AddListener(minBtnLongPress);
            minBtn.OnMouseUp.RemoveAllListeners();
            minBtn.OnMouseUp.AddListener(propMouseUp);
            minBtn.gameObject.SetActive(false);
            this.expChange = expChange;
            this.countChange = countChange;
            
        }

        public void propMouseUp()
        {
            
        }

        public void propBtnLongPress()
        {
            isMouseUp = true;
            int c = 0;
            coroutine = MonoCoroutineTool.LoopInvokeByTime(-1, longPressDuration, () => {
                if (!isMouseUp|| !addCount(0)) {
                    if (c > 0)
                    {
                        onCountChange(true);
                    }
                    return false;
                }
                c++;
                onCountChange(false);
                return true;
            });
        }
        public void propBtnClick()
        {
            if (isMouseUp) {
                isMouseUp = false;
                return;
            }
            if (addCount(0)) {
                onCountChange(true);
            }
        }
        /// <summary>
        /// 增加道具数量 
        /// </summary>
        /// <returns></returns>
        public bool addCount(int addexp)
        {
            int propcount = GameCenter.mIns.userInfo.getPropCount(propid);
            if (onExpChange(addexp) <= 0)
            {
                GameCenter.mIns.m_UIMgr.PopMsg("经验已经满了");
                return false;
            }
            else if(count >= propcount)
            {
                GameCenter.mIns.m_UIMgr.PopMsg("没有可使用的道具");
                return false;
            }
            else
            {
                count++;
                txtCount.text = HeroGrowUtils.parsePropCountStr(count, propcount);
                return true;
            }
        }

        public void minBtnLongPress()
        {
            isMouseUp = true;
            int c = 0;
            coroutine = MonoCoroutineTool.LoopInvokeByTime(-1, longPressDuration, () => {
                if (!decreaseCount()) {
                    if(c>0)
                        onCountChange(true);
                    return false;
                }
                c++;
                onCountChange(false);
                return true;
            });
        }
        public void minBtnClick()
        {
            if (isMouseUp)
            {
                isMouseUp = false;
                return;
            }
            if (decreaseCount())
                onCountChange(true);
        }
        /// <summary>
        /// 减少道具数据
        /// </summary>
        /// <returns></returns>
        public bool decreaseCount()
        {
            int propcount = GameCenter.mIns.userInfo.getPropCount(propid);
            if (count > 0)
            {
                count--;
                txtCount.text = HeroGrowUtils.parsePropCountStr(count, propcount);  
                return true;
            }
            return false;

        }
        public void setCount(int val)
        {
            count = val;
            int propcount = GameCenter.mIns.userInfo.getPropCount(propid);
            txtCount.text = HeroGrowUtils.parsePropCountStr(count, propcount);
            minBtn.gameObject.SetActive(count > 0);
        }
        public void reset()
        {
            setCount(0);
        }
        public int onExpChange(int exp)
        {  
            return expChange.Invoke(exp);
        }

        /// <summary>
        /// 道具数量发生变化 ，
        /// </summary>
        /// <param name="isChangeStatsInfo">是否拉取参数预览数据</param>
        private void onCountChange(bool isChangeStatsInfo)
        {
            minBtn.gameObject.SetActive(count > 0);
            this.countChange?.Invoke(isChangeStatsInfo);
        }

        public int getExp()
        {
            string val = GameCenter.mIns.m_CfgMgr.GetCommonCfgByName("addexp_"+propid);
            if (val != null)
            { 
                return int.Parse(val);
            }
            //以下是临时使用的固定值 ，配置表改好以后要删除这段代码
            switch (propid)
            {
                case 10011:
                    return 1000;
                case 10012:
                    return 5000;
                case 10013:
                    return 20000;
            }
            return 0;
        }
        public int getCoin()
        {
            string val = GameCenter.mIns.m_CfgMgr.GetCommonCfgByName("exptomoney_" + propid);
            if (val != null)
            {
                return int.Parse(val);
            }
            //以下是临时使用的固定值 ，配置表改好以后要删除这段代码
            switch (propid)
            {
                case 10011:
                    return 200;
                case 10012:
                    return 1000;
                case 10013:
                    return 4000;
            }
            return 0;
        }
    }

}

