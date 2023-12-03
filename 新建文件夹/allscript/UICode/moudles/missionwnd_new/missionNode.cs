using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LitJson;
using DG.Tweening;

/// <summary>
/// 关卡节点
/// </summary>
public class missionNode
{
	public GameObject curRoot;//关卡节点

    public Button btnNode;//按钮节点
    public Image missionicon_1;//关卡图标1
    public Image tagicon;//标志图标
    public GameObject clearIcon;//探索完成的图标
    public GameObject mask;
    public Image missionicon_2;//关卡图标2
    public GameObject titleimg;
    public TextMeshProUGUI titletext;//标题
    public TextMeshProUGUI name;//名字


    public Transform grayStartGroup;//灰色星星节点
    public Transform startGroup;//星星节点

    private List<GameObject> grayStartList;
    private List<GameObject> startList;

    public GameObject teshu;

    private missionNodeData nodeData;

    private missionwnd_new missionwnd;

    public missionNode(GameObject node, missionNodeData missionNodeData,Transform parent,missionwnd_new missionwnd)
	{
		this.curRoot = node;
		this.nodeData = missionNodeData;
		this.curRoot.transform.SetParent(parent);
		this.curRoot.name = missionNodeData.missionID.ToString();
		this.curRoot.transform.localScale = Vector3.one;
        this.missionwnd = missionwnd;

        Transform root = this.curRoot.transform.Find("root");
        btnNode = root.GetComponent<Button>();
        missionicon_1 = root.Find("missionicon_1").GetComponent<Image>();
        tagicon = missionicon_1.transform.Find("tagicon").GetComponent<Image>();
        clearIcon = missionicon_1.transform.Find("clearIcon").gameObject;
        mask = root.Find("mask").gameObject;
        missionicon_2 = root.Find("mask/missionicon_2").GetComponent<Image>();
        titleimg = root.Find("mask/titleimg").gameObject;
        titletext = root.Find("mask/titleimg/titletext").GetComponent<TextMeshProUGUI>();
        name = root.Find("mask/titleimg/name").GetComponent<TextMeshProUGUI>();

        grayStartGroup = root.Find("mask/grayStartGroup");
        startGroup = root.Find("mask/startGroup");


        grayStartList = new List<GameObject>();
        for (int i = 0; i < grayStartGroup.childCount; i++)
        {
            grayStartList.Add(grayStartGroup.GetChild(i).gameObject);
        }
        startList = new List<GameObject>();
        for (int i = 0; i < startGroup.childCount; i++)
        {
            startList.Add(startGroup.GetChild(i).gameObject);
        }

        teshu = this.curRoot.transform.Find("teshu").gameObject;

        //关卡
        InitNode();

    }

    /// <summary>
    /// 刷新节点
    /// </summary>
    /// <param name="nodeData"></param>
    public void RefreshNode(missionNodeData nodeData)
    {
        this.nodeData = nodeData;
        InitNode();
    }

    /// <summary>
    /// 初始化节点
    /// </summary>
    /// <param name="missionCfgData">关卡配置表数据</param>
    /// <param name="missionMsg">关卡通信数据</param>
    public void InitNode()
    {
        //刷新节点坐标
        string[] pos = nodeData.cfgData.position.Split('|');
        this.curRoot.GetComponent<RectTransform>().localPosition = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), 0);
        this.curRoot.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);

        btnNode.gameObject.SetActive(this.nodeData.cfgData.core != 1);
        teshu.SetActive(this.nodeData.cfgData.core == 1);
        if (this.nodeData.cfgData.core == 1)//是否核心关卡 大关卡节点
        {
            this.InitNodeBig();
        }
        else
        {
            this.InitNodeSmall();
        }


        //是否是新解锁关卡
        bool bNew = this.CheckIsNewUnlock();
        if (!bNew)//非最新
        {
            //设置关卡图标
            this.SetMissionIcon();
            SetUnlockStyle();
            this.curRoot.SetActive(true);
        }
        else//关卡一定是解锁的
        {
            btnNode.interactable = false;
            mask.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 165);
            //图标为未解锁
            this.tagicon.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_icon_weijiesuo");
            this.missionicon_1.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_pnl_guanka_weijiesuo");
            this.curRoot.SetActive(true);
            //todo:播放特效
            //替换为正式图片

            Debug.Log($"=======>新解锁关卡:{this.nodeData.cfgData.mission}");
            missionLine curLine = this.missionwnd.curArea.GetMissionLineByMission(this.nodeData.cfgData.mission);
            if (curLine != null)
            {
                this.missionwnd.missionRoot.lineEff.transform.SetParent(curLine.root.transform);
                this.missionwnd.missionRoot.lineEff.transform.localScale = Vector3.one;
                this.missionwnd.missionRoot.lineEff.GetComponent<RectTransform>().localPosition = curLine.ep.transform.localPosition;
                this.missionwnd.missionRoot.lineEff.SetActive(true);
                this.missionwnd.missionRoot.lineEff.GetComponent<RectTransform>().DOAnchorPos(curLine.sp.transform.localPosition, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    this.missionwnd.missionRoot.lineEff.SetActive(false);
                    this.SetMissionIcon();
                    mask.GetComponent<RectTransform>().DOSizeDelta(new Vector2(250, 165), 0.5f).OnComplete(() =>
                    {
                        btnNode.interactable = true;
                    });
                });
            }
        }
    }

    /// <summary>
    /// 小型关卡节点
    /// </summary>
    private void InitNodeSmall()
    {
        titletext.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(nodeData.cfgData.name3.ToString()));//关卡编号
        name.SetTextExt(GameCenter.mIns.m_LanMgr.GetLan(nodeData.cfgData.name2.ToString()));//关卡名字
        int star = nodeData.msg == null ? 0 : nodeData.msg.star;
        grayStartGroup.gameObject.SetActive(nodeData.cfgData.type == 1 && (nodeData.msg != null && nodeData.msg.lockstate == 1 && star <= 0));//目前只有战斗关卡有星级
        startGroup.gameObject.SetActive(nodeData.cfgData.type == 1 && (nodeData.msg != null && nodeData.msg.lockstate == 1));//目前只有战斗关卡有星级
        missionicon_2.gameObject.SetActive((nodeData.msg != null && nodeData.msg.lockstate == 1) || nodeData.cfgData.unlock1 == 101);
        titleimg.gameObject.SetActive((nodeData.msg != null && nodeData.msg.lockstate == 1) || nodeData.cfgData.unlock1 == 101);
        if (nodeData.cfgData.type == 1)
        {
            for (int i = 0; i < startList.Count; i++)
            {
                startList[i].SetActive(i <= star - 1);
            }
        }
        btnNode.AddListenerBeforeClear(() =>
        {
            bool block = false;
            if (nodeData.cfgData.unlock1 == 101 || (nodeData.msg != null && nodeData.msg.lockstate == 1))//初始解锁
            {
                block = true;
            }
            else
            {
                if ((nodeData.msg == null || nodeData.msg.unlock <= 0))
                {
                    GameCenter.mIns.m_UIMgr.PopMsg("关卡未解锁!");
                    return;
                }
                else if (nodeData.msg != null)
                {
                    GameCenter.mIns.m_UIMgr.PopMsg($"关卡未解锁!解锁条件：{nodeData.msg.unlock},解锁参数：{nodeData.msg.unlockparam}");
                    return;
                }
            }

            if (block)
            {
                if (nodeData.cfgData.type == 1 || nodeData.cfgData.type == 2 || nodeData.cfgData.type == 4)
                {
                    this.missionwnd.OrientationNode(curRoot);
                    //GameEventMgr.Distribute(GEKey.CM_OpenMissionInfo, this);
                    this.missionwnd.missionInfo.Display(nodeData.cfgData, nodeData.msg);
                }
                else
                {
                    GameCenter.mIns.m_UIMgr.PopMsg("玩法开发中，暂不能进入该关卡");
                }
            }
        });



    }

    /// <summary>
    /// 大型关卡节点
    /// </summary>
    private void InitNodeBig()
    {

        teshu.transform.Find("root/titleimg/titletext").GetComponent<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan(nodeData.cfgData.name3.ToString());
        teshu.transform.Find("root/titleimg/name").GetComponent<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan(nodeData.cfgData.name2.ToString());
        teshu.transform.Find("root/icon").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(this.nodeData.cfgData.corepicture);

        int star = nodeData.msg == null ? 0 : nodeData.msg.star;
        Transform bigNodeStar = teshu.transform.Find("root/startGroup");
        teshu.transform.Find("root/grayStartGroup").gameObject.SetActive(nodeData.cfgData.type == 1 && (nodeData.msg != null && nodeData.msg.lockstate == 1 && star <= 0));//目前只有战斗关卡有星级
        bigNodeStar.gameObject.SetActive(nodeData.cfgData.type == 1 && (nodeData.msg != null && nodeData.msg.lockstate == 1));//目前只有战斗关卡有星级
        commontool.SetGary(teshu.transform.Find("root/icon").GetComponent<Image>(), this.nodeData.msg == null || this.nodeData.msg.lockstate != 1);

        //星级
        List<GameObject> startList = new List<GameObject>();
        for (int i = 0; i < bigNodeStar.childCount; i++)
        {
            startList.Add(bigNodeStar.GetChild(i).gameObject);
        }
        if (nodeData.cfgData.type == 1)
        {
            for (int i = 0; i < startList.Count; i++)
            {
                startList[i].SetActive(i <= star - 1);
            }
        }

        teshu.transform.Find("root").GetComponent<Button>().onClick.AddListener(() =>
        {
            bool block = false;
            if (nodeData.cfgData.unlock1 == 101 || (nodeData.msg != null && nodeData.msg.lockstate == 1))//初始解锁
            {
                block = true;
            }
            else
            {
                if ((nodeData.msg == null || nodeData.msg.unlock <= 0))
                {
                    GameCenter.mIns.m_UIMgr.PopMsg("关卡未解锁!");
                    return;
                }
            }

            if (block)
            {
                if (nodeData.cfgData.type == 1 || nodeData.cfgData.type == 2 || nodeData.cfgData.type == 4)
                {
                    this.missionwnd.OrientationNode(curRoot);
                    //GameEventMgr.Distribute(GEKey.CM_OpenMissionInfo, this);
                    this.missionwnd.missionInfo.Display(nodeData.cfgData, nodeData.msg);
                }
                else
                {
                    GameCenter.mIns.m_UIMgr.PopMsg("玩法开发中，暂不能进入该关卡");
                }
            }
        });
        this.curRoot.SetActive(true);
    }

    //本地存储的非最新解锁关卡
    public string unlockMissionKey = GameCenter.mIns.userInfo.Uid + "OldUnlockMission";
    /// <summary>
    /// 检测是否新解锁
    /// </summary>
    private bool CheckIsNewUnlock()
    {
        if (this.nodeData.msg!= null && this.nodeData.msg.lockstate == 1 && this.nodeData.msg.time == 0)//如果是解锁的关卡同时没有打过
        {
            string data = string.Empty;
            if (PlayerPrefs.HasKey(unlockMissionKey))
            {
                data = PlayerPrefs.GetString(unlockMissionKey);
                List<string> missions = new List<string>(data.Split(';'));
                if (missions.Contains(this.nodeData.cfgData.mission.ToString()))//已记录 不是最新
                {
                    return false;
                }
            }
            data = $"{data}{this.nodeData.cfgData.mission};";
            PlayerPrefs.SetString(unlockMissionKey,data);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 设置关卡图标
    /// </summary>
    public void SetMissionIcon()
    {
        switch (nodeData.cfgData.type)
        {
            case 1://战斗
                tagicon.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_icon_zhandou");
                missionicon_1.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_pnl_guanka_zhandou");
                missionicon_2.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_bun_guanka_zhandou");
                break;
            case 2://剧情
                tagicon.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_icon_juqing");
                missionicon_1.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_pnl_guanka_juqing");
                missionicon_2.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_bun_guanka_juqing");
                break;
            case 3://隐藏
                tagicon.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_icon_teshu");
                missionicon_1.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_pnl_guanka_teshu");
                missionicon_2.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_bun_guanka_teshu");
                break;
            case 4://解密
                tagicon.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_icon_tansuo");
                missionicon_1.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_pnl_guanka_tansuo");
                missionicon_2.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_bun_guanka_tansuo");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 设置解锁类型
    /// </summary>
    /// <param name="missionCfgData">关卡配置</param>
    /// <param name="missionMsg">关卡通信数据</param>
    public void SetUnlockStyle()
    {
        bool islocked = false;
        if (nodeData.cfgData.unlock1 == 101)//初始解锁
        {
            islocked = true;
        }
        else
        {
            if (nodeData.msg == null)
            {
                islocked = false;
            }
            else
            {
                // 关卡解锁状态 0=未解锁 1=已解锁
                islocked = nodeData.msg.lockstate == 1;
                if (nodeData.msg.lockstate == 0)
                {
                    // 打过的，lockstate == 0
                    islocked = true;
                }
                //Debug.LogError("~~~~~ .missionCommData:" + missionGraphNode.missionCommData.ToString());
                string des = "关卡未解锁";
                // 需要解锁条件解锁
                if (nodeData.msg.unlock > 0 && nodeData.msg.lockstate != 1)
                {
                    if (nodeData.msg.unlock == 201)
                    {
                        // ”与“通过a关卡b星解锁（关卡id;星数
                        des = "通关关卡201:" + nodeData.cfgData.unlock1param;
                    }
                    else if (nodeData.msg.unlock == 202)
                    {
                        // “或”通过a关卡b星解锁（关卡id;星数）
                        des = "通关关卡202:" + nodeData.cfgData.unlock1param;
                    }
                    else if (nodeData.msg.unlock == 301)
                    {
                        // 拥有a道具b数量解锁（道具id;数量|道具id;数量）
                        des = "拥有道具:" + nodeData.cfgData.unlock1param;
                    }
                    else if (nodeData.msg.unlock == 302)
                    {
                        // 消耗a道具b数量解锁（道具id;数量|道具id;数量）
                        des = "消耗道具:" + nodeData.cfgData.unlock1param;
                    }
                    else if (nodeData.msg.unlock == 501)
                    {
                        // 剧情选择解锁（剧情选项id）
                        des = "解锁条件剧情:" + nodeData.cfgData.unlock1param;
                    }
                    else if (nodeData.msg.unlock == 601)
                    {
                        // 章节探索度解锁（探索度百分比）
                        des = "章节探索度解锁:" + nodeData.cfgData.unlock1param;
                    }
                    else if (nodeData.msg.unlock == 701)
                    {
                        // 世界等级达到解锁（世界等级）
                        des = "世界等级达到解锁:" + nodeData.cfgData.unlock1param;
                    }
                    else if (nodeData.msg.unlock == 801)
                    {
                        // 达成成就解锁
                        des = "达成成就解锁:" + nodeData.cfgData.unlock1param;
                    }
                    islocked = false;
                }
                //lockRoot.transform.Find("lockdesc").GetComponent<TextMeshProUGUI>().text = des;
            }
        }
        if (!islocked)
        {
            this.tagicon.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_icon_weijiesuo");
            this.missionicon_1.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_pnl_guanka_weijiesuo");
            this.missionicon_2.sprite = SpriteManager.Instance.GetSpriteSync("ui_e_bun_guanka_weijiesuo");
        }
    }

}



