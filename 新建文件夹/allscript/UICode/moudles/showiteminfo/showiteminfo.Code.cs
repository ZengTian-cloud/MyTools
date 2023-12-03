using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public partial class showiteminfo
{
    public override UILayerType uiLayerType => UILayerType.Pop;

    public override string uiAtlasName => "";
    protected override void OnInit()
    {

    }
    protected override void OnOpen()
    {
        base.OnOpen();
        Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~");

        if (openArgs !=  null)
        {
            // curInteract = (BattleInteractCfg)openArgs[0];
            // bSend = curInteract.param == 3;//不进入战斗的剧情节点，在对话完成后需要发起战斗结算
            // if (curInteract.param == 3)
            // {
            //     mission = curInteract.mission;
            //     randse = (JsonData)openArgs[1];
            // }
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
        //SetContentItem2(itemCfgList, keyValuePairs);
    }

    private void SetContentItem(List<ItemCfgData> itemCfgList, Dictionary<long, int> keyValuePairs)
    {
        TableView tableview = content.GetComponent<TableView>();
        tableview.onItemRender =  (GameObject item, int index) => {
            ItemCfgData onedata = itemCfgList[index - 1];
            Debug.Log("onedata.name = " + onedata.name);
            Debug.Log("keyValuePairs[onedata.pid] = " + keyValuePairs[onedata.pid]);
            item.GetComponent<CanvasGroup>().DOFade(1, 0.1f).SetEase(Ease.InExpo);
            // new Vector3(590, -534, 0) new Vector3(item.transform.localPosition.x - 60f, item.transform.localPosition.y - 100f, 0)
            item.transform.DOLocalMove(new Vector3(item.transform.localPosition.x - 60f, item.transform.localPosition.y - 200f, 0), 0.3f).From().OnComplete(() => {
                item.GetComponent<CanvasGroup>().DOFade(0, 0.3f).SetEase(Ease.InExpo).SetDelay(2.3f + index * 0.3f);
                item.transform.DOLocalMove(new Vector3(item.transform.localPosition.x, item.transform.localPosition.y + ((index - 1) * 82f), 0), 0.5f).SetDelay(3f + index * 0.3f).OnComplete(() => {
                    item.SetActive(false);
                });
            })
;            // item.GetComponent<CanvasGroup>().DOFade(1, 5f).SetEase(Ease.InExpo).OnComplete(() => {
            //     Debug.Log("11111111111111");
            //     Debug.Log("onedata.name = " + onedata.name);
            //     Debug.Log("keyValuePairs[onedata.pid] = " + keyValuePairs[onedata.pid]);
            //     item.transform.DOLocalMove(new Vector3(590, -534, 0), 5f).From().OnComplete(() => {
            //         Debug.Log("22222222222222");
            //         item.GetComponent<CanvasGroup>().DOFade(0, 5f).SetEase(Ease.InExpo).SetDelay(5).OnComplete(() => {
            //             Debug.Log("3333333333333");
            //             item.SetActive(false);
            //         });
            //     });
            // });
            //item.transform.SetParent(content.transform);
            item.transform.name = onedata.pid.ToString();
            item.transform.Find("quality").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(WarehouseManager.Instance.GetItemParamByQuality(onedata.quality, ItemParamByQualityType.LongDetailBg));
            item.transform.Find("icon").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(onedata.icon);
            item.transform.Find("text").GetComponent<TMP_Text>().text = GameCenter.mIns.m_LanMgr.GetLan(onedata.name) +  "  x  " + keyValuePairs[onedata.pid].ToString();
            // if (itemCfgList.Count >= 7)
            // {
            //     tableview.ScrollToIndex(itemCfgList.Count, 0.5f, 1);
            // }
        };
        tableview.SetDatas(itemCfgList.Count, false);

        CloseItenInfoWnd();
    }
    private void SetContentItem2(List<ItemCfgData> itemCfgList, Dictionary<long, int> keyValuePairs)
    {
        foreach (var onedata in itemCfgList)
        {
            GameObject item = GameObject.Instantiate(iteminfo);
            item.transform.DOLocalMove(new Vector3(item.transform.localPosition.x, -534, 0), 0.3f).From();
            item.SetActive(true);
            item.transform.SetParent(content.transform);
            item.transform.name = onedata.pid.ToString();
            item.transform.Find("quality").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(WarehouseManager.Instance.GetItemParamByQuality(onedata.quality, ItemParamByQualityType.LongDetailBg));
            item.transform.Find("icon").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(onedata.icon);
            item.transform.Find("text").GetComponent<TMP_Text>().text = GameCenter.mIns.m_LanMgr.GetLan(onedata.name) +  "  x  " + keyValuePairs[onedata.pid].ToString();
            //item.GetComponent<CanvasGroup>().DOFade(1, 0.1f).SetEase(Ease.InExpo);
            //item.transform.DOLocalMove(new Vector3(item.transform.localPosition.x, -534, 0), 0.3f).From().OnComplete(() => {
                // item.GetComponent<CanvasGroup>().DOFade(0, 1f).SetEase(Ease.InExpo).SetDelay(3f);
                // item.transform.DOLocalMove(new Vector3(item.transform.localPosition.x, item.transform.localPosition.y + 82f, 0), 0.3f).SetDelay(3f).OnComplete(() => {
                //     item.SetActive(false);
                // });
            //});
        }

        //CloseItenInfoWnd();
    }
    private async void CloseItenInfoWnd()
    {
        await Task.Delay(5000);
        Close();
    }
}
