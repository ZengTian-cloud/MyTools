using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using LitJson;

partial class chapterwnd
{
    public override string uiAtlasName => "missionwnd";

    //章节预制体 k-章节id
    public Dictionary<long, GameObject> dicChapterObj;

    public List<chapterData> chapterDatas;
    public Dictionary<long, chapterData> dicChapterData;

    private GameObject lastItem;

    private bool bInit;//是否初始化

    TopResourceBar topResBar;

    protected override void OnInit()
    {
        dicChapterObj = new Dictionary<long, GameObject>();
        chapterDatas = new List<chapterData>();
        bInit = true;
    }

    protected override void OnOpen()
    {
        topResBar = new TopResourceBar(_Root, this, () =>
        {
            this.Close();
            return true;
        }, "章节");
        SendChpaterDataMsg();
    }
    protected override void OnClose()
    {
        if (topResBar != null)
        {
            topResBar.OnDestroy();
        }
    }

    /// <summary>
    /// 刷新界面
    /// </summary>
    /// <param name="chapterCommDatas"></param>
    private void InitView(List<ChapterCommData> chapterCommDatas)
    {
        InitChapterData(chapterCommDatas);
        //刷新列表
        if (bInit)
        {
            content.onItemRender = OnItemRender;
            content.SetDatas(chapterDatas.Count, false);
        }
    }

    /// <summary>
    /// 生成章节数据结构
    /// </summary>
    private void InitChapterData(List<ChapterCommData> chapterCommDatas)
    {
        if (bInit)
        {
            for (int i = 0; i < ChapterCfgManager.Instance.chapterCfgDatas.Count; i++)
            {
                ChapterCfgData cfg = ChapterCfgManager.Instance.chapterCfgDatas[i];
                ChapterCommData commData = chapterCommDatas.Find(data => data.id == cfg.chapter);
                chapterData chapter = new chapterData(cfg.chapter, cfg, commData);
                chapterDatas.Add(chapter);
            }
        }
        else
        {
            for (int i = 0; i < chapterDatas.Count; i++)
            {
                long chapterId = chapterDatas[i].chapterCfgData.chapter;
                ChapterCommData commData = chapterCommDatas.Find(data => data.id == chapterId);
                if (commData != null)
                {
                    chapterDatas[i].RefreshCommData(commData);
                }
            }
        }
 
    }

    private void OnItemRender(GameObject item, int index)
    {
        TMP_Text titleText = item.transform.Find("root/title/titleText").GetOrAddCompoonet<TMP_Text>();
        TMP_Text nameText = item.transform.Find("root/namebg/nameText").GetOrAddCompoonet<TMP_Text>();
        chapterData chapterData = chapterDatas[index - 1];
        if (chapterData != null)
        {
            //是否解锁
            int unlocked = 0;
            if (chapterData.chapterCfgData.unlock == 101 || (chapterData.chapterCommData!= null &&  chapterData.chapterCommData.lockstate == 1))//默认解锁或者已解锁
            {
                unlocked = 1;
            }

            titleText.SetText(chapterData.chapterCfgData.chapter.ToString());
            nameText.SetText(GameCenter.mIns.m_LanMgr.GetLan(chapterData.chapterCfgData.name));

            Button itemBtn = item.transform.Find("root/itemBtn").GetComponent<Button>();

            GameObject root = item.transform.Find("root").gameObject;
            itemBtn.onClick.RemoveAllListeners();

            itemBtn.onClick.AddListener(() =>
            {
                if (unlocked != 1)
                {
                    GameCenter.mIns.m_UIMgr.PopMsg("章节未解锁!");
                    return;
                }

                if (lastItem && lastItem != root)
                {
                    lastItem.GetComponent<RectTransform>().DOSizeDelta(new Vector2(416, 509), 0.2f).SetEase(Ease.Linear);
                }
                root.GetComponent<RectTransform>().DOSizeDelta(new Vector2(416, 915), 0.2f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    GameCenter.mIns.m_UIMgr.Open<missionwnd_new>(chapterData);
                    lastItem = root;
                });
            });
        }
    }

    /// <summary>
    /// 请求后端章节信息
    /// </summary>
    private void SendChpaterDataMsg()
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.CHAPTER_GET_ALL, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {
                    //章节数据
                    JsonData msg = jsontool.newwithstring(content);
                    if (msg.ContainsKey("chapters"))
                    {
                        //服务器关卡信息
                        List<ChapterCommData> chapterCommDatas = JsonMapper.ToObject<List<ChapterCommData>>(JsonMapper.ToJson(msg["chapters"]));
                        if (chapterCommDatas != null)
                        {
                            InitView(chapterCommDatas);
                        }
                        bInit = false;
                    }
                });
            }
        });
    }
}



