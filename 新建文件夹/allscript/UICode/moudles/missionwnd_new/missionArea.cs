using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 关卡区块
/// </summary>
public class missionArea
{
    //模块ID
    public MissionAreaCfgData cfgData;
    //模块枚举
    public EMoudleType type;

    public missionwnd_new missionwnd;

    public ScrollRect areaRoot;

    public CanvasGroup areaCanvas;

    public Dictionary<long, missionNode> dicNodes = new Dictionary<long, missionNode>();//该区块的所有关卡节点 k-关卡id

    public List<missionLine> missionLines = new List<missionLine>();


    public bool bFlip;//是否处于翻转动画中

    public Vector2 lastFinger;//上一次触屏坐标 判断拖拽方向

    private int dirType;//翻转的方向 1-上 2-下 3-左 4-右

    public Transform lineList;

    public GameObject maxNode;//最大关卡节点
    public missionArea(MissionAreaCfgData areaCfgData, EMoudleType type,ScrollRect root, missionwnd_new missionwnd)
    {
        this.cfgData = areaCfgData;
        this.type = type;
        this.areaRoot = root;
        this.lineList = this.areaRoot.transform.Find("view/content/lineList");
        this.missionwnd = missionwnd;
        this.areaCanvas = areaRoot.GetComponent<CanvasGroup>();
        this.lastFinger = default;//
        this.areaRoot.transform.SetParent(this.missionwnd.missionRoot.moudleList.transform);

        this.areaRoot.transform.localScale = Vector3.one;
        this.areaRoot.name = $"{type.ToString()}_{areaCfgData.areaid}";
        InitArea();
    }

    /// <summary>
    /// 初始化区块
    /// </summary>
    private void InitArea()
    {
        bFlip = false;
        SwitchType(type);
        if (this.cfgData != null)
        {
            string[] size = this.cfgData.size.Split(';');
            this.areaRoot.content.rectTransform().sizeDelta = new Vector2(float.Parse(size[0]), float.Parse(size[1]));
            RefreshMissionNode();
        }
    }

    /// <summary>
    /// 刷新区块
    /// </summary>
    /// <param name="areaCfgData"></param>
    /// <param name="type"></param>
    public void RefreshArea(EMoudleType type)
    {
        this.type = type;
        this.areaRoot.name = $"{type.ToString()}_{this.cfgData.areaid}";
        this.areaCanvas.alpha = 1;
        RefreshMissionNode();
    }



    /// <summary>
    /// update
    /// 0
    /// </summary>
    public void OnUpDate()
    {

        //没有进入翻转阶段
        if (!bFlip)
        {
            if (lastFinger == default)
            {
                lastFinger = touchcstool.onefingerpos;
                return;
            }
            if (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Stationary))
            {

                Vector2 offect = touchcstool.onefingerpos - lastFinger;
                if (offect.x > 0)//向左
                {
                    if (MathF.Abs(offect.x) > MathF.Abs(offect.y))//偏移量不足，继续向左
                    {
                        dirType = 3;
                    }
                    else
                    {
                        if (offect.y > 0)//向下
                        {
                            dirType = 2;
                        }
                        else
                        {
                            dirType = 1;

                        }
                    }
                }
                else
                {
                    if (MathF.Abs(offect.x) > MathF.Abs(offect.y))//偏移量不足，继续向右
                    {
                        dirType = 4;
                    }
                    else
                    {
                        if (offect.y > 0)//向下
                        {
                            dirType = 2;
                        }
                        else
                        {
                            dirType = 1;
                        }
                    }
                }

                lastFinger = touchcstool.onefingerpos;

                if (dirType == 1 && areaRoot.verticalNormalizedPosition >= 1 && missionwnd.upArea != null)
                {
                    bFlip = true;
                    this.areaRoot.transform.SetParent(missionwnd.missionRoot.verticalPoint.transform);
                    foreach (var item in this.missionwnd.dicOtherAera)
                    {
                        item.Value.areaRoot.transform.SetParent(missionwnd.missionRoot.verticalPoint.transform);
                    }
                    Debug.Log("----------向上翻");
                }
                else if (dirType == 2 && areaRoot.verticalNormalizedPosition <= 0 && missionwnd.downArea != null)
                {
                    bFlip = true;
                    this.areaRoot.transform.SetParent(missionwnd.missionRoot.verticalPoint.transform);
                    foreach (var item in this.missionwnd.dicOtherAera)
                    {
                        item.Value.areaRoot.transform.SetParent(missionwnd.missionRoot.verticalPoint.transform);
                    }
                    Debug.Log("----------向下翻");
                }
                else if (dirType == 3 && areaRoot.horizontalNormalizedPosition <= 0 && missionwnd.leftArea != null)
                {
                    Debug.Log("----------向左翻");
                    bFlip = true;
                    this.areaRoot.transform.SetParent(missionwnd.missionRoot.horizontalPoint.transform);
                    foreach (var item in this.missionwnd.dicOtherAera)
                    {
                        item.Value.areaRoot.transform.SetParent(missionwnd.missionRoot.horizontalPoint.transform);
                    }
                }
                else if (dirType == 4 && areaRoot.horizontalNormalizedPosition >= 1 && missionwnd.rightArea != null)
                {
                    Debug.Log("----------向右翻");
                    bFlip = true;
                    this.areaRoot.transform.SetParent(missionwnd.missionRoot.horizontalPoint.transform);
                    foreach (var item in this.missionwnd.dicOtherAera)
                    {
                        item.Value.areaRoot.transform.SetParent(missionwnd.missionRoot.horizontalPoint.transform);
                    }
                }
            }
            else
            {
                dirType = 0;
            }
        }
        else//进入翻转
        {
            this.areaRoot.enabled = false;
            missionwnd.RotateStar(dirType);
        }

        if (bFlip && (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)))
        {

            switch (dirType)
            {
                case 1://上
                    areaRoot.DOVerticalNormalizedPos(0.95f, 0.2f);
                    break;
                case 2://下
                    areaRoot.DOVerticalNormalizedPos(0.05f, 0.2f);
                    break;
                case 3://左
                    areaRoot.DOHorizontalNormalizedPos(0.05f, 0.2f);
                    break;
                case 4://右
                    areaRoot.DOHorizontalNormalizedPos(0.95f, 0.2f);
                    break;
                default:
                    break;
            }

            missionwnd.RotateEnd(dirType, () => {
                lastFinger = default;
                bFlip = false;
                this.areaRoot.enabled = true;
                this.areaRoot.transform.SetParent(missionwnd.missionRoot.moudleList);
                this.missionwnd.InitRotateParam();
            });
            dirType = 0;
        }
    }

    /// <summary>
    /// 切换类型-设置对应方向的坐标
    /// </summary>
    private void SwitchType(EMoudleType type)
    {
        if (areaRoot!= null)
        {
            switch (type)
            {
                case EMoudleType.MID:
                    this.areaCanvas.alpha = 1;
                    this.areaRoot.rectTransform().localPosition = new Vector3(0, 0, 0);
                    this.areaRoot.rectTransform().localRotation = Quaternion.Euler(0, 0, 0);
                    break;
                case EMoudleType.LEFT:
                    this.areaCanvas.alpha = 0;
                    this.areaRoot.rectTransform().localPosition = new Vector3(Screen.width / 2 * -1, 0, Screen.width / 2);
                    this.areaRoot.rectTransform().localRotation = Quaternion.Euler(0, 90, 0);
                    break;
                case EMoudleType.RIGHT:
                    this.areaCanvas.alpha = 0;
                    this.areaRoot.rectTransform().localPosition = new Vector3(Screen.width / 2, 0, Screen.width / 2);
                    this.areaRoot.rectTransform().localRotation = Quaternion.Euler(0, -90, 0);
                    break;
                case EMoudleType.UP:
                    this.areaCanvas.alpha = 0;
                    this.areaRoot.rectTransform().localPosition = new Vector3(0, Screen.height / 2, Screen.height / 2);
                    this.areaRoot.rectTransform().localRotation = Quaternion.Euler(90, 0, 0);
                    break;
                case EMoudleType.DOWN:
                    this.areaCanvas.alpha = 0;
                    this.areaRoot.rectTransform().localPosition = new Vector3(0, Screen.height / 2 * -1, Screen.height / 2);
                    this.areaRoot.rectTransform().localRotation = Quaternion.Euler(-90, 0, 0);
                    break;
                default:
                    break;
            }
            this.areaRoot.gameObject.SetActive(true);
        } 
    }


    /// <summary>
    /// 刷新关卡节点
    /// </summary>
    public void RefreshMissionNode()
    {
        int count = 0;
        //获得该模块下所有关卡
        List<missionNodeData> missionNodeDatas = MissionHelper.Instance.GetMissionNodeDatasByAreaID(this.cfgData.areaid);
        if (dicNodes.Count > 0)
        {
            for (int i = 0; i < missionNodeDatas.Count; i++)
            {
                if (dicNodes.ContainsKey(missionNodeDatas[i].missionID))
                {
                    dicNodes[missionNodeDatas[i].missionID].RefreshNode(missionNodeDatas[i]);
                    if (missionNodeDatas[i].msg != null && missionNodeDatas[i].msg.lockstate == 1)
                    {
                        this.maxNode = dicNodes[missionNodeDatas[i].missionID].curRoot;
                    }
                }
            }
        }
        else
        {

            for (int i = 0; i < missionNodeDatas.Count; i++)
            {
                if (missionwnd.missionRoot.nodePoolRoot.childCount > 0)
                {
                    missionNode missionNode = new missionNode(missionwnd.missionRoot.nodePoolRoot.GetChild(0).gameObject, missionNodeDatas[i], this.areaRoot.content, this.missionwnd);
                    dicNodes.Add(missionNodeDatas[i].missionID, missionNode);
                    if (missionNodeDatas[i].msg != null && missionNodeDatas[i].msg.lockstate == 1)
                    {
                        this.maxNode = missionNode.curRoot;
                    }
                }
                else
                {
                    GameObject node = GameObject.Instantiate(missionwnd.missionRoot.missionitem);
                    missionNode missionNode = new missionNode(node, missionNodeDatas[i], this.areaRoot.content, this.missionwnd);
                    dicNodes.Add(missionNodeDatas[i].missionID, missionNode);
                    if (missionNodeDatas[i].msg != null && missionNodeDatas[i].msg.lockstate == 1)
                    {
                        this.maxNode = missionNode.curRoot;
                    }

                }
                count += 1;
            }
        }

        DrawLine(missionNodeDatas);
        //定位到最大的已通关关卡
    }

    public void DrawLine(List<missionNodeData> missionNodeDatas)
    {
        for (int i = 0; i < missionNodeDatas.Count; i++)
        {
            missionNodeData curNode = missionNodeDatas[i];
            if (curNode.cfgData.parent == "-1")//没有父节点
            {
                continue;
            }
            else
            {
                string[] missionIDs = curNode.cfgData.parent.Split('|');
                for (int m = 0; m < missionIDs.Length; m++)
                {
                    long missionid = long.Parse(missionIDs[m]);
                    missionNodeData parentNode = missionNodeDatas.Find(data => data.cfgData.mission == missionid);
                    if (parentNode != null)//检测是否是本区块的关卡节点
                    {

                        string[] starPos = curNode.cfgData.position.Split('|');
                        Vector3 star = new Vector3(float.Parse(starPos[0]), float.Parse(starPos[1]), float.Parse(starPos[2]));
                        //if (curNode.cfgData.core == 0)//普通关卡的连线偏移
                        //{
                         //   star += new Vector3(-131.15f, 16.8f, 0);
                        //}

                        string[] endPos = parentNode.cfgData.position.Split('|');
                        Vector3 end = new Vector3(float.Parse(endPos[0]), float.Parse(endPos[1]), float.Parse(endPos[2]));
                        //if (parentNode.cfgData.core == 0)
                        //{
                         //   end += new Vector3(-131.15f, 16.8f, 0);
                        //}
                        GameObject line;
                        if (missionwnd.missionRoot.linePoolRoot.childCount > 0)
                        {
                            line = missionwnd.missionRoot.linePoolRoot.GetChild(0).gameObject;
                        }
                        else
                        {
                            line = GameObject.Instantiate(missionwnd.missionRoot.missionLine);
                        }
                        line.name = $"{curNode.cfgData.mission}_{parentNode.cfgData.mission}";

                        missionLine missionLine = new missionLine(line, star, end, lineList, curNode.cfgData.mission);
                        missionLines.Add(missionLine);
                    }
                    else
                    {
                        Debug.LogError($"检测到跨区块的连接关系，请检查，节点关卡：{curNode.cfgData.mission},父节点：{missionid}");
                        continue;
                    }
                }
            }
        }
    }

    public missionLine GetMissionLineByMission(long mission)
    {
        if (missionLines != null)
        {
            missionLine line = this.missionLines.Find(line => line.mission == mission);
            if (line != null)
            {
                return line;
            }
        }
        return null;
    }

    /// <summary>
    /// 清理区块并回收
    /// </summary>
    public void ClearAera()
    {
        this.areaRoot.transform.SetParent(missionwnd.missionRoot.areaPoolRoot);//回收到区块节点下
        this.areaRoot.name = "none";
        if (dicNodes != null)
        {
            foreach (var item in dicNodes)
            {
                item.Value.curRoot.transform.SetParent(missionwnd.missionRoot.nodePoolRoot);
            }
            dicNodes.Clear();
        }
        if (missionLines != null)
        {
            for (int i = 0; i < missionLines.Count; i++)
            {
                missionLines[i].root.transform.SetParent(missionwnd.missionRoot.linePoolRoot);
            }
            missionLines.Clear();
        }
    }

}

/// <summary>
/// 模块的位置枚举
/// </summary>
public enum EMoudleType
{
	MID = 1,
    LEFT = 2,
    RIGHT = 3,
	UP = 4,
	DOWN = 5,
}

public class AreaMsg
{
    public int areaid;//区块id

    public int lockstate;//解锁状态

    public int unlock;

    public string unlockparam;

}



