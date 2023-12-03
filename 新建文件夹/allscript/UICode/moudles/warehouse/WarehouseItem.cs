using System;
using UnityEngine;
using UnityEngine.UI;

public class WareHouseItem
{
    public Transform parent;
    public GameObject obj;
    public int index;
    public ItemData data;
    public ItemCfgData cfg;

    public Button clickButton;
    public GameObject selectObj;
    public Image icon;
    public Image iconFrame;
    public Text txTime;
    public Text txNumber;

    private Action<WareHouseItem> clickCallback;

    /// <summary>
    /// 构造函数
    /// </summary>
    public WareHouseItem() { }

    public WareHouseItem(ItemData data)
    {
        this.data = data;
        cfg = GameCenter.mIns.m_CfgMgr.GetItemCfgData(data.Pid);
        if (cfg == null)
        {
            Debug.LogError("Error: this item not has cfg! data:" + data.ToString());
        }
    }

    public void BindObj(Transform parent, GameObject obj, int index, Action<WareHouseItem> clickCallback, bool bOpenAnim = false)
    {
        this.parent = parent;
        this.obj = obj;
        this.index = index;

        this.clickCallback = clickCallback;
        Transform tran = obj.transform;
        clickButton = tran.GetComponent<Button>();
        selectObj = tran.Find("root").FindHideInChild("imgSelect").gameObject;
        icon = tran.Find("root/imgIcon").GetComponent<Image>();
        iconFrame = tran.Find("root/imgFrame").GetComponent<Image>();
        txTime = tran.Find("root/txTime").GetComponent<Text>();
        txNumber = tran.Find("root/txNumber").GetComponent<Text>();

        clickButton.onClick.RemoveAllListeners();
        clickButton.onClick.AddListener(OnClick);
        OnRender();

        if (bOpenAnim)
        {
            obj.SetActive(false);
        }

        // --test
        icon.sprite = SpriteManager.Instance.GetSpriteSync("Icon_pack_libao_blue");
        iconFrame.sprite = SpriteManager.Instance.GetSpriteSync(WarehouseManager.Instance.GetItemParamByQuality(cfg.quality, ItemParamByQualityType.ItemFrame));
    }

    /// <summary>
    /// 当item对象倍渲染, 刷新数据
    /// </summary>
    public void OnRender()
    {
        selectObj.SetActive(false);
        if (txTime != null)
            txTime.text = GameCenter.mIns.m_LanMgr.GetLan(cfg.name); //data.itemCfgData.name;
        if (txNumber != null)
            txNumber.text = data.Number.ToString();
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
        data.Number = newNumber;
        txNumber.text = data.Number.ToString();
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
