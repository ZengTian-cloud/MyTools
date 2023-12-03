using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using LitJson;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class showitemlist
{
    public override UILayerType uiLayerType => UILayerType.Pop;
    public override string uiAtlasName => "";
     protected override void OnInit()
    {
        title.GetComponent<TMP_Text>().SetText(GameCenter.mIns.m_LanMgr.GetLan("get_reward"));
        confirmbtn.transform.Find("text").GetComponent<TMP_Text>().SetText(GameCenter.mIns.m_LanMgr.GetLan("common_confire"));
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
        TableView tableview = content.GetComponent<TableView>();
        tableview.onItemRender =  (GameObject item, int index) => {
            ItemCfgData onedata = itemCfgList[index - 1];
            item.transform.SetParent(content.transform);
            item.transform.name = onedata.pid.ToString();
            item.transform.Find("quality").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(WarehouseManager.Instance.GetItemParamByQuality(onedata.quality, ItemParamByQualityType.ItemFrame));
            item.transform.Find("icon").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(onedata.icon);
            item.transform.Find("text").GetComponent<TMP_Text>().text = keyValuePairs[onedata.pid].ToString();
            item.GetComponent<CanvasGroup>().DOFade(1, 0.1f).SetEase(Ease.InExpo);
        };
        tableview.SetDatas(itemCfgList.Count, false);

        OpenItenInfoWnd(openArgs);
    }

    private async void OpenItenInfoWnd(object[] openArgs)
    {
        await Task.Delay(5000);
        GameCenter.mIns.m_UIMgr.Open<showiteminfo>(openArgs[0]);
    }
}
