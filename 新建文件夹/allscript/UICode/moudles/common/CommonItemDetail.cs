using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommonItemDetail : CommomCustomPrefab
{

    #region for PrefabUIBase
    public CommonItemDetail(GameObject parentNode, string resName, string abName = "", params object[] param)
    {
        Init(parentNode, resName, abName, param);
    }

    public override string PrefabType { get => "CustomPrefab"; }


    public new async void Init(GameObject parentNode, string resName, string abName = "", params object[] param)
    {
        prefabName = "commonitemdetail";
        GameObject root = await ResourcesManager.Instance.LoadUIPrefabSync(resName);
        itemPid = int.Parse(param[0].ToString());
        InitUI(root, resName, parentNode);
    }

    public new async void InitUI(GameObject root, string resName, GameObject parentNode = null, string abName = "")
    {
        prefabName = "commonitemdetail";
        if (root == null)
        {
            root = await ResourcesManager.Instance.LoadUIPrefabSync(resName);
        }
        if (parentNode != null)
        {
            root.transform.SetParent(parentNode.transform);
        }
        _Root = root;

        InitRect();
        RegisterButton();
        BindObj(_Root);
    }

    public override void InitRect()
    {
        _Root.transform.localPosition = Vector3.zero;
        _Root.transform.localScale = Vector3.one;
        RectTransform rect = _Root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(0, 0);
        rect.offsetMax = new Vector2(0, 0);

        DoAnim();
    }

    public override void DoAnim()
    {

    }

    public override void RegisterButton()
    {

    }

    public override void UnRegisterButton()
    {
    }

    public new void OnDestroy()
    {
        UnRegisterButton();
        if (_Root != null)
        {
            GameObject.Destroy(_Root);
        }
    }
    #endregion

    private long itemPid = 0;
    public GameObject obj;
    public Transform root;
    public RectTransform rect;

    public ItemCfgData cfgData;

    public Button imgMask;
    public Image imgItemDetailQuality;
    public Image imgItemDetailIcon;
    public TMP_Text txItemDetailNumberDes;
    public TMP_Text txItemDetailNumberValue;
    public TMP_Text txItemDetailName;

    public TMP_Text txBottomDes;
    public TMP_Text txBottomDes1;
    public ScrollRect getList;
    public Transform listContent;
    public GameObject getItem;

    private int hasNumber = 0;

    private List<ItemScourceItem> scourceItems = new List<ItemScourceItem>();

    public void BindObj(GameObject obj)
    {
        Debug.Log("~~ itemPid:" + itemPid);
        ItemCfgData itemCfgData = GameCenter.mIns.m_CfgMgr.GetItemCfgData(itemPid);
        Debug.Log("~~ itemCfgData:" + itemCfgData.ToString());
        if (itemCfgData != null)
        {
            ItemData data = WarehouseManager.Instance.GetItemData(itemPid);
            if (data != null)
            {
                hasNumber = data.Number;
            }
            cfgData = itemCfgData;
        }

        this.obj = obj;
        Transform tran = obj.transform;
        root = tran;
        rect = root.GetComponent<RectTransform>();

        imgMask = root.Find("imgMask").GetComponent<Button>();
        imgItemDetailQuality = root.Find("areaContent/top/imgItemDetailQuality").GetComponent<Image>();
        imgItemDetailIcon = root.Find("areaContent/top/imgItemDetailIcon").GetComponent<Image>();
        txItemDetailNumberDes = root.Find("areaContent/top/txItemDetailNumberDes").GetComponent<TMP_Text>();
        txItemDetailNumberValue = root.Find("areaContent/top/txItemDetailNumberValue").GetComponent<TMP_Text>();
        txItemDetailName = root.Find("areaContent/top/imgItemDetailName1/txItemDetailName").GetComponent<TMP_Text>();

        txBottomDes = root.Find("areaContent/bottom/txBottomDes").GetComponent<TMP_Text>();
        txBottomDes1 = root.Find("areaContent/bottom/txBottomDes1").GetComponent<TMP_Text>();
        getList = root.Find("areaContent/bottom/getList").GetComponent<ScrollRect>();
        listContent = root.Find("areaContent/bottom/getList/content");
        getItem = root.Find("areaContent/bottom/getList/content/getItem").gameObject;

        getItem.SetActive(false);

        commontool.RegisterButtonListen(imgMask, OnClickClose);

        InitData();
    }

    public void InitData()
    {
        foreach (var item in scourceItems)
        {
            item.OnDestroy();
        }
        scourceItems = new List<ItemScourceItem>();

        if (cfgData != null)
        {
            txItemDetailNumberDes.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan("commonitemdetail_1"));
            txItemDetailNumberValue.SetTextExt(hasNumber.ToString());
            txItemDetailName.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(cfgData.name));
            txBottomDes.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(cfgData.note));
            txBottomDes1.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan("commonitemdetail_2"));
 
            if (!string.IsNullOrEmpty(cfgData.source))
            {
                string[] sources = cfgData.source.Split(new char[] { ',' });
                string[] sourceids = cfgData.sourceid.Split(new char[] { ',' });
                for (int i = 0; i < sources.Length; i++)
                {
                    string sid = sourceids[i];
                    if (!string.IsNullOrEmpty(sid))
                    {
                        string[] sid_split = sid.Split(new char[] { '_' });
                        if (sid_split.Length > 0)
                        {
                            int id = int.Parse(sid_split[0]);
                            ItemScourceItem si = CreateGetSourceItem(sources[i], id, cfgData);
                            if (si != null)
                            {
                                scourceItems.Add(si);
                            }
                        }
                    }
                }
            }
        }
    }

    private ItemScourceItem CreateGetSourceItem(string sources, int sourcesId, ItemCfgData cfgData)
    {
        ItemScourceItem si = new ItemScourceItem();
        GameObject o = GameObject.Instantiate(getItem);
        o.transform.parent = getItem.transform.parent;
        o.transform.localScale = Vector3.one;
        si.BindObj(o, sources, sourcesId, cfgData);
        o.SetActive(true);
        return si;
    }

    private void OnClickClose()
    {
        GameCenter.mIns.m_UIMgr.CloseCustomPrefab("commonitemdetail");
    }
}

public class ItemScourceItem
{
    public GameObject o;

    public TMP_Text txContent;
    public Button btnGoGet;
    public ItemCfgData cfgData;

    public void BindObj(GameObject o, string sources, int sourcesId, ItemCfgData cfgData)
    {
        this.o = o;
        this.cfgData = cfgData;
        txContent = o.transform.Find("txContent").GetComponent<TMP_Text>();
        btnGoGet = o.transform.Find("btnGoGet").GetComponent<Button>();
        /*
             来源途径方式
             -1=无来源途径，固定文本：限时获取
             0=一键扫荡
             1=跳转
             2=关卡
             3=商店
             4=合成获得
             5=纯文本“分解幻灵碎片获得”
             6=纯文本“日常任务获得”
             7=纯文本“邀约幻灵获得”

             来源ID
             如果前面是
             -1则填-1
             1则填跳转ID
             2则填关卡ID
             3则填商城配置表内对应字段的数字shop_bigpage_smallpage
             4则填合成结果物品
          */

        switch (cfgData.source)
        {
            case "-1":
            default:
                break;
            case "0":
                txContent.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan("commonitemdetail_3"));
                break;
            case "1":
                txContent.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan("commonitemdetail_4"));
                break;
            case "2":
                txContent.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan("commonitemdetail_5"));
                break;
            case "3":
                txContent.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan("commonitemdetail_6"));
                break;
            case "4":
                txContent.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan("commonitemdetail_7"));
                break;
            case "5":
                txContent.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan("commonitemdetail_8"));
                break;
            case "6":
                txContent.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan("commonitemdetail_9"));
                break;
            case "7":
                txContent.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan("commonitemdetail_10"));
                break;
        }
        btnGoGet.AddListenerBeforeClear(()=> {

        });
    }

    public void OnDestroy()
    {
        if (o != null)
        {
            GameObject.Destroy(o);
        }
    }
}
