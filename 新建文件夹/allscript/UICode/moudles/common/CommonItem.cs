using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommonItem
{
    public GameObject obj;
    public Transform root;
    public RectTransform rect;
    public int index;
    public ItemData data;
    public ItemCfgData cfg;

    public Image imgFrame;
    public Button clickButton;
    public GameObject selectObj;
    public Image icon;
    public TMP_Text txTime;
    public TMP_Text txNumber;

    public GameObject rewardInfo;
    public TMP_Text txRewardInfo;
    public GameObject mask;
    private Action<CommonItem> clickCallback;
    private bool isShowSelectFrame;
    /// <summary>
    /// 构造函数
    /// </summary>
    public CommonItem() { }

    public CommonItem(ItemData data, GameObject obj, Action<CommonItem> clickCallback, bool isShowSelectFrame = false, int index = -1)
    {
        this.data = data;
        cfg = GameCenter.mIns.m_CfgMgr.GetItemCfgData(data.Pid);
        if (cfg == null)
        {
            Debug.LogError("Error: this item not has cfg! data:" + data.ToString());
        }
        BindObj(obj, clickCallback, isShowSelectFrame, index);
    }

    public CommonItem(ItemCfgData cfg, GameObject obj, Action<CommonItem> clickCallback, bool isShowSelectFrame = false, int index = -1)
    {
        data = null;
        this.cfg = cfg;
        if (cfg == null)
        {
            Debug.LogError("Error: this item not has cfg! cfg id:" + cfg.pid.ToString());
        }
        BindObj(obj, clickCallback, isShowSelectFrame, index);
    }

    public void BindObj(GameObject obj, Action<CommonItem> clickCallback, bool isShowSelectFrame = false, int index = -1)
    {
        this.obj = obj;
        this.index = index;
        this.isShowSelectFrame = isShowSelectFrame;
        this.clickCallback = clickCallback;

        Transform tran = obj.transform;
        clickButton = tran.GetComponent<Button>();
        root = tran.Find("root");
        rect = obj.GetComponent<RectTransform>();
        imgFrame = root.FindHideInChild("imgFrame").GetComponent<Image>();
        selectObj = root.FindHideInChild("imgSelect").gameObject;
        icon = root.FindHideInChild("imgIcon").GetComponent<Image>();
        txTime = root.FindHideInChild("txTime").GetComponent<TMP_Text>();
        txNumber = root.FindHideInChild("txNumber").GetComponent<TMP_Text>();
//        mask = root.FindHideInChild("mask").gameObject;

        rewardInfo = root.FindHideInChild("rewardInfo").gameObject;
        txRewardInfo = rewardInfo.transform.FindHideInChild("txRewardInfo").GetComponent<TMP_Text>();

        commontool.RegisterButtonListen(clickButton, OnClick);
        OnRender();

        // --test
        icon.sprite = SpriteManager.Instance.GetSpriteSync("Icon_pack_libao_blue");
        if (!isShowSelectFrame)
        {
            selectObj.SetActive(false);
        }
    }

    /// <summary>
    /// 当item对象倍渲染, 刷新数据
    /// </summary>
    public void OnRender()
    {
        selectObj.SetActive(false);
        txTime.text = GameCenter.mIns.m_LanMgr.GetLan(cfg.name); //data.itemCfgData.name;
        txNumber.text = data == null ? "0" : data.Number.ToString();
    }

    /// <summary>
    /// 刷新数据，外部
    /// </summary>
    public void UpdateData(ItemData itemData)
    {
        data = itemData;
        OnRender();
    }

    /// <summary>
    /// 刷新数量
    /// </summary>
    /// <param name="newNumber">新数量</param>
    public void UpdateNumber(int newNumber)
    {
        if (data != null)
        {
            data.Number = newNumber;
            txNumber.text = data.Number.ToString();
        }
    }

    /// <summary>
    /// 点击
    /// </summary>
    public void OnClick()
    {
        clickCallback?.Invoke(this);
    }

    /// <summary>
    /// 处理点击效果
    /// </summary>
    /// <param name="bActive"></param>
    public void DoOnClickEffect(bool bActive)
    {
        if (!isShowSelectFrame)
        {
            return;
        }

        if (selectObj != null)
        {
            selectObj.SetActive(bActive);
            if (bActive)
            {
                selectObj.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(WarehouseManager.Instance.GetItemParamByQuality(cfg.quality, ItemParamByQualityType.Select));
            }
        }
    }

    /// <summary>
    /// Item被移除,数量==0，或被丢弃
    /// </summary>
    public void OnRemove()
    {
        clickButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 界面打开动画
    /// </summary>
    public void DoAnim()
    {
        if (obj != null)
        {
            obj.SetActive(true);
            // todo
        }
    }

    public void SetSize(Vector3 size)
    {
        root.transform.localScale = size;
        //rect.sizeDelta = new Vector2(rect.sizeDelta.x * size.x, rect.sizeDelta.y * size.y);
    }
    public void SetName(string name)
    {
        root.transform.name = name;
    }
    public void SetLocalPosition(float x, float y, float z)
    {
        root.transform.localPosition = new Vector3(x, y, z);
    }
    public void ChangeType(bool ischange)
    {
        if(ischange)
        {
            txNumber.gameObject.SetActive(false);
            txTime.gameObject.SetActive(false);
        }
    }

    public void SetRewardInfoActive(bool b)
    {
        if (rewardInfo != null)
        {
            rewardInfo.SetActive(b);
        }
    }
    public void SetMaskNumber(int num)
    {
        if (mask != null)
        {
            mask.SetActive(true);
            mask.transform.Find("text").GetComponent<TMP_Text>().SetText(num.ToString());
        }
    }

    /// <summary>
    /// 销毁
    /// </summary>
    public void OnDestroy()
    {
        if (obj != null)
        {
            GameObject.Destroy(obj);
        }
    }
}
