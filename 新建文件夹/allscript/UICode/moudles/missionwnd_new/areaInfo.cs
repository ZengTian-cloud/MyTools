using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LitJson;
using DG.Tweening;

public class areaInfo
{
    public missionwnd_new wnd;

    public GameObject curRoot;

    public Button title;
    public TextMeshProUGUI chapterName;
    public ScrollRect areaScroll;
    public GameObject areaItem;
    public GameObject explorationDegree;

    public Button mask;

    private Tweener tweener;


    private chapterData curChapterData;//章节信息


    private List<areaItem> areaItemList = new List<areaItem>();
    private int curArea;

    public areaInfo(GameObject obj, missionwnd_new missionwnd_New)
    {
        this.wnd = missionwnd_New;

        this.curRoot = obj;
        this.title = obj.transform.Find("bg/title").GetComponent<Button>();
        this.chapterName = this.title.transform.Find("chapterName").GetComponent<TextMeshProUGUI>();
        this.areaScroll = obj.transform.Find("bg/areaScroll").GetComponent<ScrollRect>();
        this.areaItem = this.areaScroll.content.Find("areaItem").gameObject;
        this.explorationDegree = obj.transform.Find("bg/explorationDegree").gameObject;

        this.mask = obj.transform.Find("mask").GetComponent<Button>();

        mask.gameObject.SetActive(false);


        mask.AddListenerBeforeClear(() =>
        {
            Close();
        });

        title.AddListenerBeforeClear(() =>
        {
            GameCenter.mIns.m_UIMgr.Open<chapterwnd>();
        });
    }

    public void Display(chapterData curChapterData,List<AreaMsg> areaMsgs,int curAreaID)
    {
        this.curArea = curAreaID;
        this.curChapterData = curChapterData;
        this.chapterName.text = GameCenter.mIns.m_LanMgr.GetLan(curChapterData.chapterCfgData.name);
        RefreshAreaItemList(areaMsgs);
        if (this.curRoot != null)
        {
            if (tweener != null)
            {
                tweener.Kill();
            }
            tweener = this.curRoot.transform.DOLocalMoveX(843, 0.2f);
            this.mask.gameObject.SetActive(true);
        }
    }

    private void AreaChange(int areaid)
    {
        this.curArea = areaid;
        for (int i = 0; i < areaItemList.Count; i++)
        {
            areaItemList[i].RefreshImg(areaid);
        }
        wnd.AutoRotato(areaid);
        Close();
    }


    private void RefreshAreaItemList(List<AreaMsg> areaMsgs)
    {
        if (areaItemList.Count > 0)
        {
            for (int i = 0; i < areaItemList.Count; i++)
            {
                areaItemList[i].Hide();
            }
        }


        for (int i = 0; i < areaMsgs.Count; i++)
        {
            if (i < areaItemList.Count)
            {
                areaItemList[i].Refresh(areaMsgs[i], this.curArea, (id) =>
                {
                    AreaChange(id);
                });
                
            }
            else
            {
                GameObject item = GameObject.Instantiate(this.areaItem);
                areaItem area = new areaItem(item, this.areaScroll.content);
                area.Refresh(areaMsgs[i], this.curArea, (id) =>
                {
                    AreaChange(id);
                });
                areaItemList.Add(area);
            }


        }
    }


    public void Close()
    {
        if (this.curRoot != null)
        {
            if (tweener != null)
            {
                tweener.Kill();
            }
            tweener = this.curRoot.transform.DOLocalMoveX(1700, 0.2f);
            this.mask.gameObject.SetActive(false);
        }
    }
}

public class areaItem
{
    public GameObject item;

    public Image bg;
    
    public Image tag;

    public TextMeshProUGUI title;//标题

    public Image icon;

    public TextMeshProUGUI count;//探索数量

    public MissionAreaCfgData cfgData;

    public AreaMsg areaMsg;

    public areaItem(GameObject obj,Transform parent)
    {
        this.item = obj;
        this.bg = obj.transform.Find("bg").GetComponent<Image>();
        this.tag = obj.transform.Find("bg/tag").GetComponent<Image>();
        this.title = obj.transform.Find("bg/title").GetComponent<TextMeshProUGUI>();
        this.icon = obj.transform.Find("bg/icon").GetComponent<Image>();
        this.count = obj.transform.Find("bg/count").GetComponent<TextMeshProUGUI>();

        this.item.transform.SetParent(parent);
        this.item.transform.localScale = Vector3.one;
        this.item.SetActive(true);
    }

    public void Refresh(AreaMsg areaMsg,int curArea, Action<int> cb)
    {
        this.areaMsg = areaMsg;
        cfgData = MissionCfgManager.Instance.GetMissionAreaCfgDataByAreaID(areaMsg.areaid);
        title.text = GameCenter.mIns.m_LanMgr.GetLan(cfgData.name);
        RefreshImg(curArea);
        bg.GetComponent<Button>().AddListenerBeforeClear(() =>
        {
            cb?.Invoke(this.cfgData.areaid);
        });
        this.item.SetActive(true);
    }

    public void SetBtnClick(Action<int> cb)
    {
        bg.GetComponent<Button>().AddListenerBeforeClear(() =>{
            cb?.Invoke(this.cfgData.areaid);
        });
    }

    public void Hide()
    {
        this.item.SetActive(false);
    }

    public void RefreshImg(int curAre)
    {
        if (areaMsg.lockstate == 1)
        {
            if (cfgData.areaid == curAre)
            {
                bg.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_bun_zhangjie_xuanzhong");
                tag.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_icon_linxing_hei");
            }
            else
            {
                bg.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_bun_zhangjie");
                tag.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_icon_linxing_bai");
            }
        }
        else
        {
            bg.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_bun_zhangjie_weijiesuo");
            tag.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_icon_linxing_hui");
        }
    }
}

