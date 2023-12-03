using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeroPropItem
{
    private TextMeshProUGUI txtCount;
    private GameObject obj;
    private int propcount = 0;
    private int count = 0;

    public HeroPropItem(Transform parent, GameObject obj, CostData cost)
    {
        this.obj = obj;
        this.count = cost.count;
        txtCount = obj.transform.Find("countbox/txt").gameObject.GetComponent<TextMeshProUGUI>();
        propcount = GameCenter.mIns.userInfo.getPropCount(cost.propid);
        txtCount.text = HeroGrowUtils.parsePropCountStr(cost.count, propcount);
        obj.transform.SetParent(parent);
        /**/
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        obj.SetActive(true);
    }

    public bool propIsCount()
    {
        return count < propcount;
    }

    public void OnDestroy()
    {
        if (obj != null)
        {
            GameObject.Destroy(obj);
        }
    }
}
