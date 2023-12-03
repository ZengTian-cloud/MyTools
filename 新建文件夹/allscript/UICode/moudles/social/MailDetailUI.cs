using LitJson;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MailDetailUI : SocialSubModuleUI
{
    public override EnumSocialTabType SocialTabType { get => EnumSocialTabType.Mail; }
    private GameObject uiRoot;
    private Transform uiRootTran;
    private RectTransform uiRootRect;
    public override GameObject UIRoot { get => uiRoot; }

    private GameObject mailIconObj;
    private MailIcon mailIcon;
    private TMP_Text txTitle;
    private TMP_Text txSender;
    private TMP_Text txTime;
    private ScrollRect txContentSR;
    private GameObject txContentSRContent;
    private TMP_Text txContent;
    private TMP_Text txExpireTime;
    private ScrollRect awardSR;
    private GameObject awardSRContent;
    private GameObject awardItem;
    private Button btnReceived;

    private MailData mailData;
    private List<CommonItem> rewards = new List<CommonItem>();

    public override async void LoadUIPrefab(Transform parent)
    {
        GameObject loadObj = await ResourcesManager.Instance.LoadUIPrefabSync("maildetail");
        if (loadObj != null)
        {
            uiRoot = loadObj;
            uiRootTran = uiRoot.transform;
            uiRootRect = uiRoot.GetComponent<RectTransform>();

            uiRootTran.parent = parent;
            uiRootTran.localScale = Vector3.one;
            uiRootRect.anchorMax = new Vector2(1, 1);
            uiRootRect.anchorMin = new Vector2(0, 0);
            uiRootRect.pivot = new Vector2(0.5f, 0.5f);
            uiRootRect.anchoredPosition = Vector2.zero;
            uiRootRect.offsetMin = new Vector2(0, 0);
            uiRootRect.offsetMax = new Vector2(0, 0);

            InitUI();
        }
    }

    public override void InitUI()
    {
        mailIconObj = uiRootTran.Find("root/mailicon").gameObject;
        mailIcon = new MailIcon(mailData, mailIconObj, (_mailIcon) => { });

        txTitle = uiRootTran.Find("root/txTitle").GetComponent<TMP_Text>();
        txSender = uiRootTran.Find("root/txSender").GetComponent<TMP_Text>();
        txTime = uiRootTran.Find("root/txTime").GetComponent<TMP_Text>();

        txContentSR = uiRootTran.Find("root/txContentSR").GetComponent<ScrollRect>();
        txContentSRContent = uiRootTran.Find("root/txContentSR/srcontent").gameObject;
        txContent = txContentSRContent.transform.FindHideInChild("txContent").GetComponent<TMP_Text>();

        txExpireTime = uiRootTran.Find("root/txExpireTime").GetComponent<TMP_Text>();

        awardSR = uiRootTran.Find("root/awardSR").GetComponent<ScrollRect>();
        awardSRContent = uiRootTran.Find("root/awardSR/srcontent").gameObject;
        awardItem = awardSRContent.transform.FindHideInChild("commonitem").gameObject;
        awardItem.gameObject.SetActive(false);

        btnReceived = uiRootTran.Find("root/btnReceived").GetComponent<Button>();

        commontool.RegisterButtonListen(btnReceived, OnClickReceived);
        SetReceivedBtnStlye();
        SetAwardItemStlye();
    }

    public override void SetActive(bool b)
    {
        if (uiRoot != null)
        {
            uiRoot.gameObject.SetActive(b);
        }
    }

    public override void DestroyUIPrefab()
    {
        if (uiRoot != null)
        {
            GameObject.Destroy(uiRoot);
        }
    }


    public void SetUIData(MailData mailData)
    {
        if (mailData == null)
        {
            return;
        }
        this.mailData = mailData;
        txTitle.SetTextWithEllipsis(mailData.title, 27);
        txSender.SetTextExt("来自  " + mailData.sender);
        txTime.text = datetool.TimeStampToFormatDateString(mailData.sendtime, "Y-Mo-D H:Mi:S", false);
        txContent.SetTextExt(mailData.content);
 
        txExpireTime.text = mailData.expiretime > 0 ? datetool.TimeStampToFormatDateString(mailData.expiretime, "Y-Mo-D H:Mi:S", false) + "过期" : "";//"过期";
        if (!string.IsNullOrEmpty(mailData.reward))
        {
            // reward
        }

        CreateRewardItems();
        SetReceivedBtnStlye();
        if (mailIcon != null)
        {
            mailIcon.UpdateMd(mailData);
        }
        btnReceived.gameObject.SetActive(!(mailData.state == (int)EnumMailState.Readed || mailData.state == (int)EnumMailState.UnRead));
    }

    private void OnClickReceived()
    {
        if (mailData.state == (int)EnumMailState.AlreadyReceived)
        {
            MailManager.Instance.OnDeleteMail(mailData.id);
            return;
        }
        SetReceivedBtnStlye();

        MailManager.Instance.OnReceivedMail(mailData.id);
    }

    public void OnReceiveReceivedAwardInfo()
    {
        mailData.SetState((int)EnumMailState.AlreadyReceived);
        SetReceivedBtnStlye();
        SetAwardItemStlye();
    }

    private void SetReceivedBtnStlye()
    {
        if (mailData != null)
        {
            btnReceived.transform.GetChild(0).gameObject.SetActive(mailData.state == (int)EnumMailState.AlreadyReceived);
            btnReceived.GetComponentInChildren<TMP_Text>().text = mailData.state == (int)EnumMailState.AlreadyReceived
                ? "删除邮件" : "领取物品";
        }
    }
    private void SetAwardItemStlye()
    {
        if (mailData != null)
        {
            foreach (var item in rewards)
            {
                item.SetRewardInfoActive(mailData.state == (int)EnumMailState.AlreadyReceived);
            }
        }
    }

    private void CreateRewardItems()
    {
        foreach(var item in rewards) {
            item.OnDestroy();
        }
        rewards = new List<CommonItem>();
        if (mailData != null && !string.IsNullOrEmpty(mailData.reward) && !"{}".Equals(mailData.reward))
        {
            JsonData jd = jsontool.newwithstring(mailData.reward);
            if (jd != null)
            {
                List<string> keys = new List<string>(jd.Keys);
                foreach (var k in keys)
                {
                    int iid = int.Parse(k);
                    int num = int.Parse(jd[k].ToString());
                    ItemData itemData = new ItemData(iid, iid, num);
                    GameObject itemObj = GameObject.Instantiate(awardItem);
                    itemObj.transform.SetParent(awardItem.transform.parent);
                    itemObj.transform.localScale = Vector3.one;

                    CommonItem commonItem = new CommonItem(itemData, itemObj, (_item) =>
                    {
                        // 点击了
                        Debug.Log("点击了 _item:" + _item.data.ToString());
                        GameCenter.mIns.m_UIMgr.PopCustomPrefab("commonitemdetail", "", _item.data.Pid);
                    });
                    commonItem.SetSize(Vector3.one * 0.73f);
                    itemObj.SetActive(true);
                    commonItem.SetRewardInfoActive(mailData.state == (int)EnumMailState.AlreadyReceived);
                    rewards.Add(commonItem);
                }
            }
        }
    }

    /// <summary>
    /// 标记为已读
    /// </summary>
    public void MarkReaded(MailData md)
    {
        if (md != null && md.id == mailData.id)
        {
            // todo
            mailIcon.SetIcon(md.state);
        }
    }

    /// <summary>
    /// 标记为已领取
    /// </summary>
    public void MarkReceived(MailData md)
    {
        if (md != null && md.id == mailData.id)
        {
            // todo
            mailIcon.SetIcon(md.state);
            OnReceiveReceivedAwardInfo();
        }
    }
}
