using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class HeroMinItem
{
    public Transform parent;
    public GameObject obj;
    public GameObject selectObj;
    public Button clickButton;
    public Image heroicon;
    public Image herolv;


    public int index;
    public HeroData data;
    public HeroInfoCfgData heroInfo;

    private Action<HeroMinItem> clickCallback;

    /// <summary>
    /// 选中框_右下
    /// </summary>
    RectTransform selectRB;
    /// <summary>
    /// 选中框_右上
    /// </summary>
    RectTransform selectRT;
    /// <summary>
    /// 选中框_左下
    /// </summary>
    RectTransform selectLB;
    /// <summary>
    /// 选中框_左上
    /// </summary>
    RectTransform selectLT;

    public HeroMinItem() { }

    public HeroMinItem(HeroData data)
    {
        this.data = data;
        heroInfo = GameCenter.mIns.m_CfgMgr.GetHeroCfgDataByHeroID(data.heroID);
        if (heroInfo == null)
            Debug.LogError("Error: this item not has cfg! data:" + data.ToString());
    }

    public void BindObj(Transform parent, GameObject obj, int index, Action<HeroMinItem> clickCallback, bool bOpenAnim = false)
    {
        this.parent = parent;
        this.obj = obj;
        this.index = index;

        this.clickCallback = clickCallback;

        Transform tran = obj.transform;
        clickButton = tran.GetComponent<Button>();
        selectObj = tran.Find("selects").gameObject;
        heroicon = tran.Find("heroicon").GetComponent<Image>();
        herolv = tran.Find("herolv/bk").GetComponent<Image>();

        clickButton.onClick.RemoveAllListeners();
        clickButton.onClick.AddListener(OnClick);
        OnRender();

        heroicon.sprite = SpriteManager.Instance.GetSpriteSync(heroInfo.picture2);
        herolv.sprite = SpriteManager.Instance.GetSpriteSync(getIconBk(heroInfo.quality));

        selectRB = selectObj.transform.Find("select4").GetComponent<RectTransform>();
        selectRT = selectObj.transform.Find("select2").GetComponent<RectTransform>();
        selectLB = selectObj.transform.Find("select3").GetComponent<RectTransform>();
        selectLT = selectObj.transform.Find("select1").GetComponent<RectTransform>();

        //selectObj.transform.Find("select").gameObject.SetActive(false);
        if (selectObj.transform.childCount > 5)
        {
            Debug.Log("<color=#ff0000> 已经加载过特效 </color>");

            effect = selectObj.transform.GetComponentInChildren<UIParticleEffect>();

            if (effect == null)
            {
                //LoadEffect();
            }
        }
        else
        {
            //LoadEffect();
        }
    }

    private string getIconBk(int quality)
    {
        switch (quality)
        {
            case 3:
                return "ui_c_pnl_tx_lantiao";
            case 4:
                return "ui_c_pnl_tx_zitiao";
            case 5:
                return "ui_c_pnl_tx_chengtiao";
            case 6:
                return "ui_c_pnl_tx_hongtiao";

        }
        return "";
    }

    public void OnRender()
    {
        selectObj.SetActive(false);
        //txTime.text = GameCenter.mIns.m_LanMgr.GetLan(cfg.name); //data.itemCfgData.name;
        //txNumber.text = data.Number.ToString();
    }

    /// <summary>
    /// 点击
    /// </summary>
    public void OnClick()
    {
        if (selectObj.activeSelf)
            return;

        clickCallback?.Invoke(this);

    }
    public void DoOnClickEffect(bool bActive)
    {
        if (selectObj != null)
        {
            selectObj.SetActive(bActive);

            if (bActive)
            {
                //selectObj.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(WarehouseManager.Instance.GetItemParamByQuality(heroInfo.quality, ItemParamByQualityType.Select));
                
                //TODO:选中框单独做一个能复用的脚本管理 
                selectLT.DOLocalMove(selectLT.localPosition * 2, 0.25f).From().SetEase(Ease.OutQuad);
                selectLB.DOLocalMove(selectLB.localPosition * 2, 0.25f).From().SetEase(Ease.OutQuad);
                selectRT.DOLocalMove(selectRT.localPosition * 2, 0.25f).From().SetEase(Ease.OutQuad);
                selectRB.DOLocalMove(selectRB.localPosition * 2, 0.25f).From().SetEase(Ease.OutQuad);
            }
        }
    }

    UIParticleEffect effect;

    private void LoadEffect()
    {
        if (effect != null)
        {
            effect.gameObject.SetActive(true);
            effect.Play();
            return;
        }
        UIEffectManager.Instance.LoadUIEffect("effs_ui_r_01", (go) =>
        {
            effect = go.AddComponent<UIParticleEffect>();

            effect.SetUP(new UIParticleEffect.ShowInfo()
            {
                _offset = 1,

                _canvas = obj.transform.GetComponentInParent<Canvas>(),

                _rectMask= obj.transform.GetComponentInParent<RectMask2D>(),
            });
            go.transform.SetParent(selectObj.transform, false);

            go.transform.localPosition = new Vector3(-128, 0);
        });
    }

    public void OnDestroy()
    {
        if (obj != null)
        {
            GameObject.Destroy(obj);
        }
    }
}

