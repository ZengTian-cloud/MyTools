using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UpgradesPropItem
{
    private Transform parent;
    private ExButton propBtn;
    private ExButton minBtn;
    private TextMeshProUGUI txtCount;
    public int propid;
    public int count = 0;
    public int exp = 0;
    public int index;
    private string xhStrStart;
    private string coinStrStart;

    private Func<int, int> expChange;
    private Action<bool> countChange;
    private Coroutine coroutine;
    private bool isMouseUp;
    private float longPressDuration = 0.03f;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="index"></param>
    /// <param name="expChange"></param>
    /// <param name="propidStart">道具编号的前辍</param>
    /// <param name="xhStrStart">消耗常量的前缀</param>
    /// <param name="coinStrStart">金币常量的前缀</param>
    /// <param name="countChange"></param>
    public UpgradesPropItem(Transform parent, int index, int propidStart, string xhStrStart, string coinStrStart, Func<int, int> expChange,Action<bool> countChange)
    {
        this.parent = parent;
        this.index = index;
        this.xhStrStart = xhStrStart;
        this.coinStrStart = coinStrStart;
        txtCount = parent.Find("cailiao" + index + "/countbox/txt").gameObject.GetComponent<TextMeshProUGUI>();
        propid = propidStart + index;
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
            if (!isMouseUp || !addCount(0))
            {
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
        if (isMouseUp)
        {
            isMouseUp = false;
            return;
        }
        if (addCount(0))
        {
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
        else if (count >= propcount)
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
            if (!decreaseCount())
            {
                if (c > 0)
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
        string val = GameCenter.mIns.m_CfgMgr.GetCommonCfgByName(xhStrStart + propid);
        if (val != null)
        {
            return int.Parse(val);
        }
        return 0;
    }
    public int getCoin()
    {
        string val = GameCenter.mIns.m_CfgMgr.GetCommonCfgByName(coinStrStart + propid);
        if (val != null)
        {
            return int.Parse(val);
        }
        return 0;
    }
}