using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 血条管理器
/// </summary>
public class HpSliderManager 
{
    private static HpSliderManager Ins;
    public static HpSliderManager ins
    {
        get
        {
            if (Ins == null)
            {
                Ins = new HpSliderManager();
            }
            return Ins;
        }
    }

    public Dictionary<long, GameObject> dicSliderList = new Dictionary<long, GameObject>();//血条管理器 k-唯一id， v-slider



    /// <summary>
    /// 生成一个血条
    /// </summary>
    /// <param name="baseMonster"></param>
    public void CreatOneSliderByMonster(BaseObject baseobj)
    {
        if (baseobj == null)
        {
            return;
        };

        if (dicSliderList.ContainsKey(baseobj.GUID))
        {
            Debug.LogError($"对象{baseobj.objID}_{baseobj.GUID}重复生成血条，请检查");
            return;
        }
        GameObject slider = BattlePoolManager.Instance.OutPool(ERootType.HPui);
        if (slider == null)
        {
            slider = GameObject.Instantiate(GameCenter.mIns.m_BattleMgr.hpSliderTemp);
        }

        slider.transform.SetParent(GameCenter.mIns.m_BattleMgr.hpCanvas.transform);
        dicSliderList.Add(baseobj.GUID, slider);
        hpSliderController hpController = slider.gameObject.GetOrAddCompoonet<hpSliderController>();
        hpController.targetObj = baseobj.pointHelper.GetBone(FBXBoneType.point_headtop).gameObject;
        hpController.OnInit();

        slider.gameObject.SetActive(true);
    }

    /// <summary>
    /// 当一个血条对象消失
    /// </summary>
    public void OnOneObjDisappear(BaseObject baseObj)
    {
        if (dicSliderList.ContainsKey(baseObj.GUID))
        {
            GameObject.Destroy(dicSliderList[baseObj.GUID].gameObject.GetComponent<hpSliderController>());
            dicSliderList[baseObj.GUID].gameObject.SetActive(false);
            BattlePoolManager.Instance.InPool(ERootType.HPui, dicSliderList[baseObj.GUID].gameObject);
            dicSliderList.Remove(baseObj.GUID);
        }    
    }

    /// <summary>
    /// 刷新血条数字
    /// </summary>
    /// <param name="guid"></param>
    public void RefreshHpSliderValue(long guid,float value)
    {
        if (dicSliderList.ContainsKey(guid))
        {
            {
                HpSliderDomove(dicSliderList[guid], value);
            }
        }
    }

    /// <summary>
    /// 刷新护盾显示
    /// </summary>
    /// <param name="bActive"></param>
    public void RefreshShieldSliderActive(long guid,bool bActive)
    {
        if (dicSliderList.ContainsKey(guid))
        {
            dicSliderList[guid].transform.Find("shieldSlider").gameObject.SetActive(bActive);
        }
    }

    /// <summary>
    /// 刷新护盾
    /// </summary>
    public void RefreshShieldSldierValue(long guid, float value)
    {
        if (dicSliderList.ContainsKey(guid))
        {
            {
                ShieldSliderDomove(dicSliderList[guid], value);
            }
        }
    }

    /// <summary>
    /// 血条动画
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public void HpSliderDomove(GameObject obj,float value)
    {
        Image hp = obj.transform.Find("hp").GetComponent<Image>();
        Image hpeff = obj.transform.Find("hpEff").GetComponent<Image>();
        hp.DOFillAmount(value, 0.3f).SetEase(Ease.Linear).OnComplete(() => {
            hpeff.DOFillAmount(value, 0.1f).SetEase(Ease.Linear);
        });
    }

    /// <summary>
    /// 护盾条动画
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public void ShieldSliderDomove(GameObject obj, float value)
    {
        Image hp = obj.transform.Find("shieldSlider/hp").GetComponent<Image>();
        Image hpeff = obj.transform.Find("shieldSlider/hpEff").GetComponent<Image>();
        hp.DOFillAmount(value, 0.3f).SetEase(Ease.Linear).OnComplete(() => {
            hpeff.DOFillAmount(value, 0.1f).SetEase(Ease.Linear);
        });
    }
}
