using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class achievementsecondmainslot:MonoBehaviour
{
    bool haveGot=false;//是否已经点击过领取防止重复领取
    List<DropCfgData> dropData;
    public void Init(achievementwnd.AchieveStruct data,achievementwnd mywnd)
    {
        haveGot = false;
        /*需要补充的内容
         * 1.
         * transform.Find("icon").GetComponent<Image>()=
         * 2.奖励内容显示
         *
         * 
         * 
         */
        dropData = DropManager.Instance.GetDropListByDropID(data.dropid);
        foreach (var item in dropData)
        {
         //   Debug.Log(item.dropid);
        }
        transform.Find("slottitle").GetComponent<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan(data.name);
        transform.Find("slotinfotxt").GetComponent<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan(data.note);
        //是否显示进度条
        if (data.progressbar == 1&&data.total>0)
        {
            //进度数字设置
            Transform father = transform.Find("finishprocessarchor");
            father.gameObject.SetActive(true);
            transform.Find("finishprocessarchor/totaltxt").GetComponent<TextMeshProUGUI>().text = data.total.ToString();
            transform.Find("finishprocessarchor/finishedtxt").GetComponent<TextMeshProUGUI>().text = data.count.ToString();
            //刷新大小
            LayoutRebuilder.ForceRebuildLayoutImmediate(father.transform.GetChild(0).GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(father.transform.GetChild(2).GetComponent<RectTransform>());
            float wide = 0;
            wide -= father.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;
            father.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(wide, father.GetChild(1).GetComponent<RectTransform>().anchoredPosition.y);
            wide -= father.GetChild(1).GetComponent<RectTransform>().sizeDelta.x;
            father.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(wide, father.GetChild(2).GetComponent<RectTransform>().anchoredPosition.y);

            //刷新slider
            transform.Find("processSlider").gameObject.SetActive(true);
            transform.Find("processSlider/Image").GetComponent<Image>().fillAmount = data.count / (float)data.total;
        }
        else        
        {
            transform.Find("processSlider").gameObject.SetActive(false);
            transform.Find("finishprocessarchor").gameObject.SetActive(false);            
        }
        transform.Find("processingImage").gameObject.SetActive(false);
        transform.Find("getbonusbtn").gameObject.SetActive(false);
        transform.Find("finishtime").gameObject.SetActive(false);

        //任务当前状态
        switch (data.state)
        {
            case 0://未完成
                transform.Find("processingImage").gameObject.SetActive(true);
                break;
            case 2://已领取
                
                //显示完成时间
                if (data.completetime != null)
                {
                    DateTime time = data.completetime;
                    transform.Find("finishtime").gameObject.SetActive(true);
                    transform.Find("finishtime").GetComponent<TextMeshProUGUI>().text = (time.Year.ToString() + "年" + time.Month.ToString() + "月" + time.Day.ToString() + "日");
                    transform.Find("getbonusbtn").gameObject.SetActive(false);
                }                
                break;
            case 1://已完成未领取
                {
                    transform.Find("getbonusbtn").gameObject.SetActive(true);
                    
                    //领取奖励button添加监听
                    transform.Find("getbonusbtn").GetComponent<ExButton>().onClick.AddListener(() =>
                    {
                         if(haveGot)
                            return;
                        haveGot = true;                        
                        //领取奖励
                        TaskMsgManager.Instance.SendReceiveReward((msg) => {
                           
                            //领取奖励回调
                            if (msg.ContainsKey("taskid"))
                            {
                                mywnd.FinishAAchieve(data.id,data.floor);
                            }
                        }                            
                        , data.id,data.floor);


                    });


                }
                break;

        }
        
    }
}
