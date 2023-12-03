using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatInfoListHelper : MonoBehaviour
{
    public int viewItemLimitCount = 10;
    public float split = 10;
    private ScrollRect scrollRect;
    private RectTransform content;
    private Transform contentTran;
    private GameObject chatItemObj;
    private List<ChatViewItem> viewItems;
    private List<ChatData> allDatas;

    private Vector2 srAreaSize = Vector2.zero;
    /*
       *  WorldCorners
       * 
       *    1 ------- 2     
       *    |         |
       *    |         |
       *    0 ------- 3
       * 
       *  rectCorners
       * 
       *    - ------- 1     
       *    |         |
       *    |         |
       *    0 ------- -
       * 
   */
    private Vector3[] viewWorldConers = new Vector3[4];
    private Vector3[] rectCorners = new Vector3[2];

    public void Init(List<ChatData> allDatas)
    {
        this.allDatas = allDatas;

        // 时间间隔检测
        long prevSendTime = 0;
        for (int i = 0; i <= allDatas.Count - 1; i++)
        {
            if (prevSendTime == 0)
            {
                prevSendTime = allDatas[i].sendtime;
            }
            else
            {
                // ms 2分钟=60*2*1000
                if (Math.Abs(prevSendTime - allDatas[i].sendtime) >= 120000)
                {
                    if (i - 1 >= 0)
                    {
                        allDatas[i - 1].isDisplaySendTime = true;
                    }
                }
            }
        }

        scrollRect = transform.GetComponent<ScrollRect>();
        srAreaSize = scrollRect.GetComponent<RectTransform>().sizeDelta;

        content = scrollRect.content;
        contentTran = content.transform;
        chatItemObj = content.transform.GetChild(0).gameObject;
        chatItemObj.SetActive(false);

        Vector3[] viewWorldConers = new Vector3[4];
        Vector3[] rectCorners = new Vector3[2];
        scrollRect.GetComponent<RectTransform>().GetWorldCorners(viewWorldConers);
        rectCorners[0] = scrollRect.content.transform.InverseTransformPoint(viewWorldConers[0]);
        rectCorners[1] = scrollRect.content.transform.InverseTransformPoint(viewWorldConers[2]);
        //for (int i = 0; i < viewWorldConers.Length; i++)
        //{
        //    Debug.LogError("v i:" + viewWorldConers[i]);
        //}
        //for (int i = 0; i < rectCorners.Length; i++)
        //{
        //    Debug.LogError("r i:" + viewWorldConers[i]);
        //}

        content.sizeDelta = Vector2.zero;

        viewItems = new List<ChatViewItem>();
        for (int i = 0; i < allDatas.Count; i++)
        {
            GameObject obj = GameObject.Instantiate(chatItemObj);
            obj.name = "ci_" + i;
            obj.transform.SetParent(contentTran);
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            ChatViewItem chatViewItem = new ChatViewItem(obj, allDatas.Count > i ? (allDatas[i]) : null, i);
            obj.SetActive(true);
            StartCoroutine(WaitRefreshItem(() => {
                if (chatViewItem != null)
                {
                    chatViewItem.RefreshView();
                    AddChatItemToHead(chatViewItem);
                }
            }));
        }
    }

    public void InitList()
    {
        
    }

    public void AddNewChat(ChatData chatData)
    {
        Debug.LogWarning("~~~ allDatas:" + allDatas.Count);
        allDatas.Add(chatData);
        GameObject obj = GameObject.Instantiate(chatItemObj);
        obj.name = "ci_" + allDatas.Count;
        obj.transform.SetParent(contentTran);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        ChatViewItem chatViewItem = new ChatViewItem(obj, chatData, allDatas.Count);
        obj.SetActive(true);

        StartCoroutine(WaitRefreshItem(() => {
            if (chatViewItem != null)
            {
                chatViewItem.RefreshView();
                AddChatItemToHead(chatViewItem);
            }
        }));
    }

    private IEnumerator WaitRefreshItem(Action callback)
    {
        yield return null;
        callback?.Invoke();
    }

    public Vector2 GetContentSize()
    {
        return content.sizeDelta;
    }

    public void GetItemPos(ChatViewItem chatViewItem)
    {
        Vector3 lbpos = GetItemWorldPosCorner(chatViewItem, 0);
    }

    private void AddContentSize(Vector2 addSize)
    {
        content.sizeDelta = new Vector2(content.sizeDelta.x, content.sizeDelta.y + addSize.y + split);
    }

    private void AddChatItemToHead(ChatViewItem chatViewItem)
    {
        Vector2 vector2 = chatViewItem.GetRectSize();
        chatViewItem.SetPos(Vector2.zero);
        chatViewItem.currPosYAtView = 0;
        foreach (var vi in viewItems)
        {
            vi.currPosYAtView = vi.currPosYAtView + vector2.y + split;
            vi.SetPos(new Vector2(0, vi.currPosYAtView));
        }
        viewItems.Add(chatViewItem);
        AddContentSize(vector2);
    }


    private Vector3[] m_TempWorldConers = new Vector3[4];
    private Vector3[] m_TempRectCorners = new Vector3[2];
    private Vector3 GetItemWorldPosCorner(ChatViewItem chatViewItem, int cornerType)
    {
        chatViewItem.rt.GetWorldCorners(m_TempWorldConers);
        m_TempRectCorners[0] = content.transform.InverseTransformPoint(viewWorldConers[0]);
        m_TempRectCorners[1] = content.transform.InverseTransformPoint(viewWorldConers[2]);
        if (cornerType == 0)
        {
            return m_TempRectCorners[0];
        }
        else if (cornerType == 1)
        {
            return m_TempRectCorners[1];
        }
        return Vector2.zero;
    }

    public void Clear()
    {
        if (viewItems == null)
        {
            return;
        }
        foreach (var item in viewItems)
        {
            item.OnDestroy();
        }
        viewItems.Clear();
    }
}


public class ChatViewItem
{
    public int index;
    public Action<ChatData> clickCallback;
    public GameObject o;
    public RectTransform rt;
    public ChatData chatData;

    private GameObject timeObjNode;
    private GameObject commonObjNode;
    private RectTransform commonObjRect;
    private GameObject seqImgObjNode;

    private TMP_Text timeText;

    private RectTransform timeText_rect;

    public float currPosYAtView;

    private GameObject common_L;
    private GameObject common_R;
    private CNode cNode;

    public ChatViewItem(GameObject o, ChatData chatData, int index)
    {
        this.chatData = chatData;
        // Debug.LogWarning("chatData:" + chatData.ToString());
        this.o = o;
        this.index = index;
        rt = o.GetComponent<RectTransform>();

        timeObjNode = o.transform.FindHideInChild("txTime").gameObject;
        timeText = timeObjNode.GetComponent<TMP_Text>();
        timeText_rect = timeText.GetComponent<RectTransform>();
        timeObjNode.SetActive(false);

        common_L = o.transform.FindHideInChild("common_l").gameObject;
        common_R = o.transform.FindHideInChild("common_r").gameObject;
        common_L.SetActive(false);
        common_R.SetActive(false);
        if (chatData.roleid == GameCenter.mIns.userInfo.RoleId)
            { cNode = new CNode(common_R); common_R.SetActive(true); }
        else
            { cNode = new CNode(common_L); common_L.SetActive(false); }

        SetChatData(chatData);
    }

    public void RefreshView()
    {
        if (cNode != null)
            cNode.RefreshView(rt);
    }

    public Vector2 GetRectSize()
    {
        return rt.sizeDelta;
    }

    public void SetPos(Vector2 pos)
    {
        rt.anchoredPosition = pos;
    }

    public void SetChatData(ChatData chatData)
    {
        this.chatData = chatData;
        if (chatData == null)
        {
            return;
        }
        if (chatData.chatTextType == EnumChatTextType.Common)
        {
            cNode.SetChatData(chatData, rt, true, timeText);
            //commonObjNode.SetActive(true);
            //commonText.text = chatData.chattext;
            //LayoutRebuilder.ForceRebuildLayoutImmediate(commonText_rect);
            //LayoutRebuilder.ForceRebuildLayoutImmediate(chatRect);
            //commonObjRect.sizeDelta = new Vector2(commonObjRect.sizeDelta.x, chatRect.sizeDelta.y);
            //rt.sizeDelta = new Vector2(rt.sizeDelta.x, commonObjRect.sizeDelta.y);
            //LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
        else if (chatData.chatTextType == EnumChatTextType.Time)
        {
            timeObjNode.SetActive(true);
            timeText.text = chatData.chattext;
            LayoutRebuilder.ForceRebuildLayoutImmediate(timeText_rect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
        else if (chatData.chatTextType == EnumChatTextType.SeqImage)
        {
            seqImgObjNode.SetActive(true);
        }
    }

    public void OnDestroy()
    {
        if (o != null) { GameObject.Destroy(o); }
    }

    public override string ToString()
    {
        return chatData.chattext.ToString();
    }

    public void Cover(ChatData chatData)
    {
        this.chatData = chatData;

    }

    private class CNode
    {
        public ChatData chatData;
        private GameObject commonObjNode;
        private RectTransform commonObjRect;
        private GameObject seqImgObjNode;
        private TMP_Text commonText;
        private Image avatar;
        private RectTransform chatRect;
        private RectTransform commonText_rect;
        private ContentSizeFitter commonText_csf;

        private TMP_Text timeText = null;
        public CNode(GameObject o)
        {
            commonObjNode = o;
            seqImgObjNode = o.transform.FindHideInChild("seqImg").gameObject;
            commonObjRect = commonObjNode.GetComponent<RectTransform>();
            avatar = commonObjNode.transform.Find("imgAvatar").GetComponent<Image>();
            chatRect = commonObjNode.transform.Find("imgChat").GetComponent<RectTransform>();
            commonText = chatRect.transform.FindHideInChild("tx").GetComponent<TMP_Text>();
            commonText_rect = commonText.GetComponent<RectTransform>();
            commonText_csf = commonText.GetComponent<ContentSizeFitter>();

            commonObjNode.SetActive(false);
            seqImgObjNode.SetActive(false);
        }

        public void SetChatData(ChatData chatData, RectTransform rt, bool isClcTextWidth = true, TMP_Text timeText = null)
        {
            this.chatData = chatData;
            if (timeText != null)
            {
                this.timeText = timeText;
            }
            if (chatData == null)
            {
                return;
            }
            if (chatData.chatTextType == EnumChatTextType.Common)
            {
                commonObjNode.SetActive(true);
                commonText.text = chatData.chattext;

                if (commonText_csf != null)
                {
                    if (commonText.preferredWidth > 400)
                    {
                        commonText_csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        commonText_rect.sizeDelta = new Vector2(400, commonText_rect.sizeDelta.y);
                    }
                    else
                    {
                        commonText_csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    }
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(commonText_rect);
                LayoutRebuilder.ForceRebuildLayoutImmediate(chatRect);


                float x = rt.sizeDelta.x;
                if (isClcTextWidth)
                {
                    x = commontool.AdvanceGetTextWidth(commonText.text, commonText.font.name, (int)commonText.fontSize);
                    //Debug.LogError("`` s: " + commonText.text + " - len: " + x);
                    x = x + 30 >= 400 ? 400 : x + 30;
                    chatRect.sizeDelta = new Vector2(x, chatRect.sizeDelta.y);
                }
                float y = chatRect.sizeDelta.y <= 120 ? 120 : chatRect.sizeDelta.y;
          
                commonObjRect.sizeDelta = new Vector2(690, y + 30);

                float rty = chatData.isDisplaySendTime ? commonObjRect.sizeDelta.y + 30 : commonObjRect.sizeDelta.y;
                rt.sizeDelta = new Vector2(commonObjRect.sizeDelta.x, rty);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

                if (chatData.isDisplaySendTime)
                {
                    if (this.timeText != null)
                    {
                        this.timeText.SetTextExt(datetool.TimeStampToFormatDateString(chatData.sendtime, "H:Mi:S", false));
                        this.timeText.gameObject.SetActive(chatData.isDisplaySendTime);
                    }
                }
                //Debug.LogError("preferredWidth:" + commonText.preferredWidth + " - text:" + commonText.text);
            }
            else if (chatData.chatTextType == EnumChatTextType.SeqImage)
            {
                seqImgObjNode.SetActive(true);
            }
        }

        public void RefreshView(RectTransform rt)
        {
            if (chatData != null)
                SetChatData(chatData, rt, false);
        }
    }
}
