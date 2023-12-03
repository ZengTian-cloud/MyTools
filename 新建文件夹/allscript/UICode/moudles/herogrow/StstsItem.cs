using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Transactions;

public class StstsItem
{
    public Transform parent;
    public GameObject obj;
    public StatData data;
    public int index;
    public long ststsId;
    public bool isAnimations;
    public HeroData hero;
    public Image imgIcon;
    public TextMeshProUGUI count;
    public TextMeshProUGUI text;
    public Image go;
    public TextMeshProUGUI countGo;
    public bool goShow = false;//是否显示预览数据
    public float propcount = 0;

    public StstsItem() { }
    public StstsItem(Transform parent, StatData data, HeroData hero, bool ended, GameObject obj, int index,bool isAnimations)
    {
        if (obj == null)
        {
            return;
        }
        this.parent = parent;
        this.obj = obj;
        this.index = index;
        this.data = data;
        this.hero = hero;
        this.isAnimations = isAnimations;
        this.ststsId = data.statid;

        imgIcon = obj.transform.Find("itemRoot/icon").GetComponent<Image>();
        count = obj.transform.Find("itemRoot/count").GetComponent<TextMeshProUGUI>();
        text = obj.transform.Find("itemRoot/text").GetComponent<TextMeshProUGUI>();
        go = obj.transform.Find("itemRoot/go").GetComponent<Image>();
        countGo = obj.transform.Find("itemRoot/countgo").GetComponent<TextMeshProUGUI>();
        count.alignment = TextAlignmentOptions.MidlineRight;
        setCount();
        HeroStatsCfg statCfg = GameCenter.mIns.m_CfgMgr.getHeroStstsCfgByAttrId(data.statid);
        text.text = data.text;
        imgIcon.sprite = SpriteManager.Instance.GetSpriteSync(statCfg.iconid);

        if (ended)
        {
            obj.transform.Find("itemRoot/line").gameObject.SetActive(false);
        }

        countGo.text = "";

        obj.transform.SetParent(parent);
        /**/
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.SetActive(true);
        /// 左侧数值参数 X：154 313
        hideCountGo();
    }
    public void setCount()
    {
        propcount = hero.GetAttrByAttrID(data.statid);
        count.text = hero.GetAttrByAttrIdToStr(data.statid, data.vtype);
    }
    /// <summary>
    /// 显示属性预览
    /// </summary>
    public void showCountGo(float c)
    {
        goShow = true;
        goShow = c > propcount;
        count.alignment = TextAlignmentOptions.MidlineLeft;
        count.GetComponent<RectTransform>().DOAnchorPosX(154, 0.1f).OnComplete(() => {
            go.gameObject.SetActive(goShow);
            countGo.gameObject.SetActive(goShow);
            countGo.text = hero.GetAttrByValIdToStr(c, data.vtype); ;
        }); 
    }
    /// <summary>
    /// 隐藏属性预览
    /// </summary>
    public void hideCountGo()
    {
        goShow = false;
        go.gameObject.SetActive(goShow);
        countGo.gameObject.SetActive(goShow);
        count.alignment = TextAlignmentOptions.MidlineRight ;
        if(isAnimations)
            count.GetComponent<RectTransform>().DOAnchorPosX(313, 0.1f); 
        else
            count.GetComponent<RectTransform>().anchoredPosition = new Vector3(313, 0);
    }
    public void OnDestroy()
    {
        if (obj != null)
        {
            GameObject.Destroy(obj);
        }
    }
}

public class WeaponStstsItem: StstsItem
{
    public WeaponDataCfg weaponData;

    public WeaponStstsItem(Transform parent, StatData data, HeroData hero, WeaponDataCfg weaponData, bool ended, GameObject obj, int index, bool isAnimations)
    : base(parent, data, hero,ended,  obj,  index,  false)
    {
        this.weaponData = weaponData;
        setCount(data.val);
    }
    public void setCount(int val)
    {
        propcount = val;
        count.text = hero.GetWeaponAttrToStr(val, data.vtype);
    }
    
}
