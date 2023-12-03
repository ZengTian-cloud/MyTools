using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using LitJson;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class showitemmiddle
{
    public override UILayerType uiLayerType => UILayerType.Pop;
    public override string uiAtlasName => "";
     protected override void OnInit()
    {

    }
    protected override void OnOpen()
    {
        base.OnOpen();

        if (openArgs !=  null)
        {
            SetItemListData(openArgs);
        }
    }
    private void SetItemListData(object[] openArgs)
    {
        List<ItemCfgData> itemCfgList = new();
        Dictionary<long, int> keyValuePairs = new();
        JsonData jsondata = (JsonData)openArgs[0];
        foreach (JsonData item in jsondata)
        {
            long pid = long.Parse(item["pid"].ToString());
            int num = int.Parse(item["num"].ToString());
            ItemCfgData cfg = GameCenter.mIns.m_CfgMgr.GetItemCfgData(pid);
            itemCfgList.Add(cfg);
            keyValuePairs.Add(pid, num);
        }

        SetContentItem(itemCfgList, keyValuePairs);
    }

    private void SetContentItem(List<ItemCfgData> itemCfgList, Dictionary<long, int> keyValuePairs)
    {
        iteminfo.transform.SetParent(bagroot.transform);
        iteminfo.transform.name = itemCfgList[0].pid.ToString();
        iteminfo.transform.Find("quality").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(WarehouseManager.Instance.GetItemParamByQuality(itemCfgList[0].quality, ItemParamByQualityType.ItemFrame));
        iteminfo.transform.Find("icon").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(itemCfgList[0].icon);
        iteminfo.transform.Find("text").GetComponent<TMP_Text>().text = keyValuePairs[itemCfgList[0].pid].ToString();
        iteminfo.GetComponent<CanvasGroup>().DOFade(1, 0.1f).SetEase(Ease.InExpo);

        OpenItenInfoWnd(openArgs);
    }

    private async void OpenItenInfoWnd(object[] openArgs)
    {
        await Task.Delay(3000);
        GameCenter.mIns.m_UIMgr.Open<showiteminfo>(openArgs[0]);
    }
}
