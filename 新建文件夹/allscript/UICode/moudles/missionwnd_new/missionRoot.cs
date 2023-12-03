using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// 关卡总节点
/// </summary>
public class missionRoot
{
    public missionwnd_new missionWnd;//ui脚本

    public GameObject root;//预制体节点
    public Transform wndroot;//ui节点
    public Camera missioncamera;//关卡相机

    public Transform showPoint;//表现节点
    public Transform areaPoolRoot;//区块缓存节点
    public Transform nodePoolRoot;//关卡缓存节点
    public Transform moudleList;//模块节点
    public Transform linePoolRoot;//连线缓存节点

    public GameObject missionitem;//关卡节点
    public ScrollRect scrollItem;
    public GameObject reward;//奖励（探索度）
    public Button chapterBtn;//章节按钮
    public GameObject missionInfo;//关卡信息界面
    public GameObject areaInfo;//区块信息界面

    public GameObject missionLine;//连线预制体

    public GameObject verticalPoint;//纵向旋转节点
    public GameObject horizontalPoint;//横向旋转节点

    public GameObject lineEff;

    public GameObject mask;
    public missionRoot(GameObject _Root,missionwnd_new missionWnd)
	{
        this.missionWnd = missionWnd;
        this.root = _Root;

        missioncamera = this.root.transform.Find("missionCamera").GetComponent<Camera>();

        this.wndroot = this.root.transform.Find("missionwnd_new");
        showPoint = wndroot.Find("showPoint");
        areaPoolRoot = wndroot.Find("areaPoolRoot");
        nodePoolRoot = wndroot.Find("nodePoolRoot");
        moudleList = wndroot.Find("moudleList");
        linePoolRoot = wndroot.Find("linePoolRoot");

        missionitem = wndroot.Find("moudleList/missionitem").gameObject;
        scrollItem = wndroot.Find("scrollItem").GetComponent<ScrollRect>();
        reward = wndroot.Find("reward").gameObject;
        chapterBtn = wndroot.Find("chapterBtn").GetComponent<Button>();
        missionInfo = wndroot.Find("missionInfo").gameObject;
        areaInfo = wndroot.Find("areaInfo").gameObject;


        missionLine = wndroot.Find("missionLine").gameObject;

		verticalPoint = wndroot.Find("verticalPoint").gameObject;
		horizontalPoint = wndroot.Find("horizontalPoint").gameObject;

        mask = wndroot.Find("mask").gameObject;

        lineEff = wndroot.Find("lineEff").gameObject;

        scrollItem.GetComponent<RectTransform>().sizeDelta = root.GetComponent<RectTransform>().sizeDelta;
    }


}

