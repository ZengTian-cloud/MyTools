using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

partial class missionwnd_new
{
    public override UILayerType uiLayerType => UILayerType.Normal;

    public override string uiAtlasName => "";

    //当前章节的数据
    public chapterData curChapterData;

    private int curAreaID;//当前区块

    public missionArea curArea;//当前区块
    public missionArea leftArea;//当前区块
    public missionArea rightArea;//当前区块
    public missionArea upArea;//当前区块
    public missionArea downArea;//当前区块

    public Dictionary<int, missionArea> dicOtherAera;//记录除了主区块的其他区块信息 k-区块id

    private GameObject missionRootprefab;//关卡节点
    public missionRoot missionRoot;
    public missionInfo missionInfo;//关卡详情界面
    public areaInfo areaInfo;//区块界面

    public float roSpeed = 200f;//翻转速度

    public List<AreaMsg> areaMsgs;
    TopResourceBar topResBar;
    //baseui-初始化
    protected override async void OnInit()
    {
        //这里需要单独用透视相机实现ui效果，所以单独分出去写
        //加载3d节点
        this.missionRootprefab = await ResourcesManager.Instance.LoadUIPrefabSync("missionRoot");
        this.missionRootprefab.transform.SetParent(this.worldRoot);

        this.missionRoot = new missionRoot(this.missionRootprefab, this);
        this.missionInfo = new missionInfo(this.missionRoot.missionInfo,this);
        this.areaInfo = new areaInfo(this.missionRoot.areaInfo, this);
        //添加相机到主相机stack下
        GameCenter.mIns.m_CamMgr.AddCameraToMainCamera(this.missionRoot.missioncamera);

        this.dicOtherAera = new Dictionary<int, missionArea>();
        this.curArea = null;
        this.missionRoot.moudleList.SetParent(this.missionRoot.showPoint);
    }

    protected override void OnRegister(bool register)
    {
        if (register)
        {
            GameEventMgr.Register(NetCfg.NPC_SAVE_INTERACT_ID.ToString(), OnSaveInteractID);
        }
        else
        {
            GameEventMgr.UnRegister(NetCfg.NPC_SAVE_INTERACT_ID.ToString(), OnSaveInteractID);
        }
    }

    /// <summary>
    /// 保存交互id时
    /// </summary>
    private void OnSaveInteractID(GEventArgs gEventArgs)
    {
        MissionHelper.Instance.SendMissionMsg(this.curChapterData.chapterId, (missionMsgs, areaMsgs) =>
        {
            //根据下发关卡获取到最近通关关卡的所在模块
            this.curAreaID = GetLastMissionOfMoudle(missionMsgs);
            this.RefreshMissionMoudles(this.curAreaID);
            this.missionRoot.chapterBtn.AddListenerBeforeClear(() =>
            {
                this.areaInfo.Display(curChapterData, areaMsgs, this.curArea.cfgData.areaid);
            });
        });
    }

    //baseui-打开时
    protected override void OnOpen()
    {
        if (this.missionRootprefab != null)
        {
            this.missionRootprefab.SetActive(true);
        }
        //加载顶部ui
        topResBar = new TopResourceBar(this.missionRoot.wndroot.gameObject, this, () =>
        {
            foreach (var item in this.dicOtherAera)
            {
                item.Value.ClearAera();
            }
            this.Close();
            return true;
        }, GameCenter.mIns.m_LanMgr.GetLan("mission_title"));

        //注册ui界面的update函数
        GameCenter.mIns.m_UIMgr.AddUpdateWin(this);

        //获得进入的章节信息
        this.curChapterData = (chapterData)openArgs[0];

        MissionHelper.Instance.SendMissionMsg(this.curChapterData.chapterId, (missionMsgs, areaMsgs) =>
        {
            //根据下发关卡获取到最近通关关卡的所在模块
            this.curAreaID = GetLastMissionOfMoudle(missionMsgs);
            this.RefreshMissionMoudles(this.curAreaID);


            this.missionRoot.chapterBtn.AddListenerBeforeClear(() =>
            {
                this.areaInfo.Display(curChapterData, areaMsgs, this.curArea.cfgData.areaid);
            });
        });
    }

    //关闭回调
    protected override void OnClose()
    {
        if (this.missionRootprefab != null)
        {
            this.missionRootprefab.SetActive(false);
        }
        if (topResBar._Root != null)
        {
            topResBar.OnDestroy();
        }
    }

    /// <summary>
    /// 生成/刷新关卡区块
    /// </summary>
    /// <param name="curArea">当前在正面的区块或者进行翻转操作后即将处于正面的区块id</param>
    public void RefreshMissionMoudles(int curArea)
    {
        //默认区块
        if (curArea <= 0)
            curArea = 101001;

        //获得当前区块配置
        MissionAreaCfgData curAreaCfg = MissionCfgManager.Instance.GetMissionAreaCfgDataByAreaID(curArea);
        if (this.curArea == null)//第一次进入，生成主区块
        {
            ScrollRect scrollItem = GameObject.Instantiate(missionRoot.scrollItem);
            this.curArea = new missionArea(curAreaCfg, EMoudleType.MID, scrollItem, this);
        }
        else//检测翻转操作后的主区块
        {
            if (this.curArea.cfgData.areaid == curArea)//没有变化
            {
                this.curArea.RefreshMissionNode();
                return;
            }
            else//有变化
            {
                this.curArea.ClearAera();//回收上一个主界面
                if (this.dicOtherAera.ContainsKey(curArea))
                {
                    this.curArea = this.dicOtherAera[curArea];
                    this.curArea.RefreshArea(EMoudleType.MID);
                    this.dicOtherAera.Remove(curArea);
                }
                else
                {
                    Debug.LogError($"未在缓存列表中找到aeraId为{curArea}的区块，请检查");
                }
            }
        }
        this.OrientationNode(this.curArea.maxNode);

        //每一次生成主区块，回收该区块的关联区块
        if (this.dicOtherAera != null)
        {
            foreach (var item in this.dicOtherAera)
            {
                item.Value.ClearAera();
            }
            dicOtherAera.Clear();
        }

        this.leftArea = null;
        this.rightArea = null;
        this.upArea = null;
        this.downArea = null;

        //左区块
        if (!string.IsNullOrEmpty(curAreaCfg.left))
        {
            this.CreatOtherArea(int.Parse(curAreaCfg.left.Split(';')[1]), EMoudleType.LEFT);
        }
        //右区块
        if (!string.IsNullOrEmpty(curAreaCfg.right))
        {
            this.CreatOtherArea(int.Parse(curAreaCfg.right.Split(';')[1]), EMoudleType.RIGHT);
        }
        //上区块
        if (!string.IsNullOrEmpty(curAreaCfg.up))
        {
            this.CreatOtherArea(int.Parse(curAreaCfg.up.Split(';')[1]), EMoudleType.UP);
        }
        //下区块
        if (!string.IsNullOrEmpty(curAreaCfg.down))
        {
            this.CreatOtherArea(int.Parse(curAreaCfg.down.Split(';')[1]), EMoudleType.DOWN);
        }
    }

    /// <summary>
    /// 生成其他区块
    /// </summary>
    public void CreatOtherArea(int areaID,EMoudleType type)
    {
        MissionAreaCfgData areaCfgData = MissionCfgManager.Instance.GetMissionAreaCfgDataByAreaID(areaID);//获得该区块配置信息
        ScrollRect scrollItem = null;
        if (this.missionRoot.areaPoolRoot.childCount > 0)//有回收的组件
        {
            scrollItem = this.missionRoot.areaPoolRoot.GetChild(0).GetComponent<ScrollRect>();
        }
        else
        {
            scrollItem = GameObject.Instantiate(missionRoot.scrollItem);
        }
        switch (type)
        {
            case EMoudleType.LEFT:
                this.leftArea = new missionArea(areaCfgData, type, scrollItem, this);
                this.dicOtherAera.Add(areaID, this.leftArea);
                break;
            case EMoudleType.RIGHT:
                this.rightArea = new missionArea(areaCfgData, type, scrollItem, this);
                this.dicOtherAera.Add(areaID, this.rightArea);
                break;
            case EMoudleType.UP:
                this.upArea = new missionArea(areaCfgData, type, scrollItem, this);
                this.dicOtherAera.Add(areaID, this.upArea);
                break;
            case EMoudleType.DOWN:
                this.downArea = new missionArea(areaCfgData, type, scrollItem, this);
                this.dicOtherAera.Add(areaID, this.downArea);
                break;
            default:
                break;
        }
    }

    public override void UpdateWin()
    {
        if (curArea != null)
        {
           curArea.OnUpDate();
        }
    }

    /// <summary>
    /// 初始化旋转参数
    /// </summary>
    public void InitRotateParam()
    {
        lastValue = 0;
        laseRotate = 0;
        canDrag = false;
        runRotate = 0;
    }

    private float lastValue;//翻转过程中的手指坐标（x or y）
    private float laseRotate;//翻转过程中的旋转值
    private bool canDrag;//拖动的阀值
    private float runRotate = 0;
    /// <summary>
    ///  开始旋转旋转方向
    /// </summary>
    /// <param name="dirType"></param>
    public void RotateStar(int dirType)
    {
        switch (dirType)
        {
            case 1://上
                if (lastValue == 0)
                {
                    lastValue = touchcstool.onefingerpos.y;
                    return;
                }

                laseRotate = missionRoot.verticalPoint.transform.rotation.eulerAngles.x;
                Debug.Log("-------laseRotate:" + laseRotate);
                if (Math.Abs(touchcstool.onefingerpos.y - lastValue) > 10)
                {
                    canDrag = true;
                }
                if (canDrag)
                {
                    if (touchcstool.onefingerpos.y < lastValue)//手指向下滑
                    {
                        missionRoot.verticalPoint.transform.rotation = Quaternion.Euler(laseRotate + (Time.deltaTime * roSpeed) * -1, 0, 0);
                        runRotate -= Time.deltaTime * roSpeed;
                    }
                    else if (touchcstool.onefingerpos.y > lastValue)//手指向上滑
                    {
                        missionRoot.verticalPoint.transform.rotation = Quaternion.Euler(laseRotate + (Time.deltaTime * roSpeed), 0, 0);
                        runRotate += Time.deltaTime * roSpeed;
                    }

                    lastValue = touchcstool.onefingerpos.y;
                }
                this.curArea.areaCanvas.alpha = (90 - MathF.Abs(runRotate)) / 90;
                this.upArea.areaCanvas.alpha = MathF.Abs(runRotate) / 90;
                break;
            case 2://下
                if (lastValue == 0)
                {
                    lastValue = touchcstool.onefingerpos.y;
                    return;
                }

                laseRotate = missionRoot.verticalPoint.transform.rotation.eulerAngles.x;
                Debug.Log("-------laseRotate:" + laseRotate);
                if (Math.Abs(touchcstool.onefingerpos.y - lastValue) > 10)
                {
                    canDrag = true;
                }
                if (canDrag)
                {
                    if (touchcstool.onefingerpos.y < lastValue)//手指向下滑
                    {
                        missionRoot.verticalPoint.transform.rotation = Quaternion.Euler(laseRotate + (Time.deltaTime * roSpeed) *-1,0, 0);
                        runRotate -= Time.deltaTime * roSpeed;
                    }
                    else if (touchcstool.onefingerpos.y > lastValue)//手指向上滑
                    {
                        missionRoot.verticalPoint.transform.rotation = Quaternion.Euler(laseRotate + (Time.deltaTime * roSpeed),0, 0);
                        runRotate += Time.deltaTime * roSpeed;
                    }

                    lastValue = touchcstool.onefingerpos.y;
                }
                this.curArea.areaCanvas.alpha = (90 - MathF.Abs(runRotate)) / 90;
                this.downArea.areaCanvas.alpha = MathF.Abs(runRotate) / 90;
                break;
            case 3://左
                if (lastValue == 0)
                {
                    lastValue = touchcstool.onefingerpos.x;
                    return;
                }
                laseRotate = missionRoot.horizontalPoint.transform.rotation.eulerAngles.y;
                if (Math.Abs(touchcstool.onefingerpos.x - lastValue) > 10)
                {
                    canDrag = true;
                }
                if (canDrag)
                {
                    if (touchcstool.onefingerpos.x < lastValue)
                    {
                        missionRoot.horizontalPoint.transform.rotation = Quaternion.Euler(0, laseRotate + (Time.deltaTime * roSpeed), 0);
                        runRotate += Time.deltaTime * roSpeed;
                    }
                    else if (touchcstool.onefingerpos.x > lastValue)
                    {
                        missionRoot.horizontalPoint.transform.rotation = Quaternion.Euler(0, laseRotate + (Time.deltaTime * roSpeed) * -1, 0);
                        runRotate -= Time.deltaTime * roSpeed;
                    }

                    lastValue = touchcstool.onefingerpos.x;
                }
                this.curArea.areaCanvas.alpha = (90 - MathF.Abs(runRotate)) / 90;
                this.leftArea.areaCanvas.alpha = MathF.Abs(runRotate) / 90;

                break;
            case 4://右
                if (lastValue == 0)
                {
                    lastValue = touchcstool.onefingerpos.x;
                    return;
                }
                laseRotate = missionRoot.horizontalPoint.transform.rotation.eulerAngles.y;
                if (Math.Abs(touchcstool.onefingerpos.x - lastValue) > 10)
                {
                    canDrag = true;
                }
                if (canDrag)
                {
                    if (touchcstool.onefingerpos.x < lastValue)
                    {
                        missionRoot.horizontalPoint.transform.rotation = Quaternion.Euler(0, laseRotate + (Time.deltaTime * roSpeed), 0);
                        runRotate += Time.deltaTime * roSpeed;
                    }
                    else if (touchcstool.onefingerpos.x > lastValue)
                    {
                        missionRoot.horizontalPoint.transform.rotation = Quaternion.Euler(0, laseRotate + (Time.deltaTime * roSpeed) * -1, 0);
                        runRotate -= Time.deltaTime * roSpeed;
                    }

                    lastValue = touchcstool.onefingerpos.x;
                }
                this.curArea.areaCanvas.alpha = (90 - MathF.Abs(runRotate)) / 90;
                this.rightArea.areaCanvas.alpha = MathF.Abs(runRotate) / 90;

                break;
            default:
                break;
        }
    }


    /// <summary>
    /// 旋转结束
    /// </summary>
    public void RotateEnd(int type, Action action)
    {
        int dirType = type;
        switch (dirType)
        {
            case 1:
                if (runRotate > -45)//还原
                {
                    Debug.Log("--------上翻-还原");
                    missionRoot.verticalPoint.transform.DOLocalRotate(Vector3.zero, 0.5f).OnComplete(() =>
                    {
                        action?.Invoke();
                        foreach (var item in this.dicOtherAera)
                        {
                            item.Value.areaRoot.transform.SetParent(this.missionRoot.moudleList);
                        }
                    });
                    this.curArea.areaCanvas.DOFade(1, 0.5f);
                    this.upArea.areaCanvas.DOFade(0, 0.5f);
                }
                else
                {
                    Debug.Log("--------上翻-成功");
                    missionRoot.verticalPoint.transform.DOLocalRotate(new Vector3(-90, 0, 0), 0.5f).OnComplete(() =>
                    {
                        action?.Invoke();
                        foreach (var item in this.dicOtherAera)
                        {
                            item.Value.areaRoot.transform.SetParent(this.missionRoot.moudleList);
                        }
                        missionRoot.verticalPoint.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        RefreshMissionMoudles(this.upArea.cfgData.areaid);
                    });
                    this.curArea.areaCanvas.DOFade(0, 0.5f);
                    this.upArea.areaCanvas.DOFade(1, 0.5f);
                }
                break;
            case 2:
                if (runRotate < 45)//还原
                {
                    Debug.Log("--------下翻-还原");
                    missionRoot.verticalPoint.transform.DOLocalRotate(Vector3.zero, 0.5f).OnComplete(() =>
                    {
                        action?.Invoke();
                        foreach (var item in this.dicOtherAera)
                        {
                            item.Value.areaRoot.transform.SetParent(this.missionRoot.moudleList);
                        }
                    });
                    this.curArea.areaCanvas.DOFade(1, 0.5f);
                    this.downArea.areaCanvas.DOFade(0, 0.5f);
                }
                else
                {
                    Debug.Log("--------下翻-成功");
                    missionRoot.verticalPoint.transform.DOLocalRotate(new Vector3(90, 0, 0), 0.5f).OnComplete(() =>
                    {
                        action?.Invoke();
                        foreach (var item in this.dicOtherAera)
                        {
                            item.Value.areaRoot.transform.SetParent(this.missionRoot.moudleList);
                        }
                        missionRoot.verticalPoint.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        RefreshMissionMoudles(this.downArea.cfgData.areaid);
                    });
                    this.curArea.areaCanvas.DOFade(0, 0.5f);
                    this.downArea.areaCanvas.DOFade(1, 0.5f);
                }
                break;
            case 3:
                if (runRotate > -45)//还原
                {
                    Debug.Log("--------左翻-还原");
                    missionRoot.horizontalPoint.transform.DOLocalRotate(Vector3.zero, 0.5f).OnComplete(() =>
                    {
                        action?.Invoke();
                        foreach (var item in this.dicOtherAera)
                        {
                            item.Value.areaRoot.transform.SetParent(this.missionRoot.moudleList);
                        }
                    });
                    this.curArea.areaCanvas.DOFade(1, 0.5f);
                    this.leftArea.areaCanvas.DOFade(0, 0.5f);

                }
                else
                {
                    Debug.Log("--------左翻-成功");
                    missionRoot.horizontalPoint.transform.DOLocalRotate(new Vector3(0, -90, 0), 0.5f).OnComplete(() =>
                    {
                        action?.Invoke();
                        foreach (var item in this.dicOtherAera)
                        {
                            item.Value.areaRoot.transform.SetParent(this.missionRoot.moudleList);
                        }
                        missionRoot.horizontalPoint.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        RefreshMissionMoudles(this.leftArea.cfgData.areaid);
                    });
                    this.curArea.areaCanvas.DOFade(0, 0.5f);
                    this.leftArea.areaCanvas.DOFade(1, 0.5f);
                }
                break;
            case 4:
                if (runRotate < 45)//还原
                {
                    Debug.Log("--------右翻-还原");
                    missionRoot.horizontalPoint.transform.DOLocalRotate(Vector3.zero, 0.5f).OnComplete(() =>
                    {
                        action?.Invoke();
                        foreach (var item in this.dicOtherAera)
                        {
                            item.Value.areaRoot.transform.SetParent(this.missionRoot.moudleList);
                        }
                    });
                    this.curArea.areaCanvas.DOFade(1, 0.5f);
                    this.rightArea.areaCanvas.DOFade(0, 0.5f);

                }
                else
                {
                    Debug.Log("--------右翻-成功");
                    missionRoot.horizontalPoint.transform.DOLocalRotate(new Vector3(0, 90, 0), 0.5f).OnComplete(() =>
                    {
                        action?.Invoke();
                        foreach (var item in this.dicOtherAera)
                        {
                            item.Value.areaRoot.transform.SetParent(this.missionRoot.moudleList);
                        }
                        missionRoot.horizontalPoint.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        RefreshMissionMoudles(this.rightArea.cfgData.areaid);
                    });
                    this.curArea.areaCanvas.DOFade(0, 0.5f);
                    this.rightArea.areaCanvas.DOFade(1, 0.5f);
                }
                break;   
            default:
                action?.Invoke();
                break;
        }
        runRotate = 0;
    }






    /// <summary>
    /// 判断上一次最近通关关卡的所属模块
    /// </summary>
    public int GetLastMissionOfMoudle(List<MissionMsg> missionMsgs)
    {
        DateTime temp = default;
        DateTime newtemp = default;
        long targetMissionID = 0;
        for (int i = 0; i < missionMsgs.Count; i++)
        {
            if (missionMsgs[i].time > 0)
            {
                newtemp = commontool.TimestampToDataTime(missionMsgs[i].time);
                if (DateTime.Compare(newtemp, temp) > 0)
                {
                    temp = newtemp;
                    targetMissionID = missionMsgs[i].id;
                }
            }
        }
        if (targetMissionID == 0)//一关没打
        {
            targetMissionID = 1010010101;//默认第一关
        }
        MissionCfgData missionCfgData = MissionCfgManager.Instance.GetMissionCfgByMissionID(targetMissionID);
        if (missionCfgData != null) 
        {
            return missionCfgData.areaid;
        }
        return 0;
    }



    /// <summary>
    /// 定位到点击的格子
    /// </summary>
    public void OrientationNode(GameObject node)
    {
        if (node!= null)
        {
            Vector2 size = this.curArea.areaRoot.content.GetComponent<RectTransform>().sizeDelta;
            float maxX = Math.Abs((size.x - Screen.width) / 2f);
            float maxY = Math.Abs((size.y - Screen.height) / 2f);

            float x = node.GetComponent<RectTransform>().anchoredPosition.x;
            float y = node.GetComponent<RectTransform>().anchoredPosition.y;
            x = Math.Clamp(x * -1 - 500, -maxX, maxX);
            y = Math.Clamp(y * -1, -maxY, maxY);

            this.curArea.areaRoot.content.GetComponent<RectTransform>().DOAnchorPos(new Vector2(x, y), 0.2f);
        }
    }


    /////////////////////////////////////////////////////在区块信息界面手动选择目标区块 计算旋转方向路径进行动画演出 需实时刷新关卡界面数据///////////////////////////////
    /// <summary>
    /// 自动旋转 在区块信息界面手动选择目标区块 计算旋转方向路径进行动画演出 需实时刷新关卡界面数据
    /// </summary>
    public void AutoRotato(int tagetArea)
    {
        this.missionRoot.mask.SetActive(true);//自动翻转期间屏蔽玩家操作
        List<int> roPath = ComputeRotatPath(tagetArea);
        int index = 0;
        DoRotate(index, roPath);
    }
    private float durtion = 0.25f;
    public void DoRotate(int index, List<int> roPath)
    {
        int ro = roPath[index];
        switch (ro)
        {
            case 1://上
                foreach (var item in this.dicOtherAera)
                {
                    item.Value.areaRoot.transform.SetParent(this.missionRoot.verticalPoint.transform);
                }
                this.curArea.areaRoot.transform.SetParent(this.missionRoot.verticalPoint.transform);
                this.curArea.areaCanvas.DOFade(0, durtion);
                this.upArea.areaCanvas.DOFade(1, durtion);
                this.missionRoot.verticalPoint.transform.DOLocalRotate(new Vector3(-90, 0, 0), durtion).SetEase(Ease.Linear).OnComplete(() => {

                    foreach (var item in this.dicOtherAera)
                    {
                        item.Value.areaRoot.transform.SetParent(this.missionRoot.moudleList);
                    }
                    missionRoot.verticalPoint.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    RefreshMissionMoudles(this.upArea.cfgData.areaid);

                    GameCenter.mIns.m_CoroutineMgr.DelayNextFrame(() =>
                    {
                        index += 1;
                        if (index < roPath.Count)
                        {
                            DoRotate(index, roPath);
                        }
                        else
                        {
                            this.missionRoot.mask.SetActive(false);
                        }
                    });
                   
                });
                break;
            case 2://下
                foreach (var item in this.dicOtherAera)
                {
                    item.Value.areaRoot.transform.SetParent(this.missionRoot.verticalPoint.transform);
                }
                this.curArea.areaRoot.transform.SetParent(this.missionRoot.verticalPoint.transform);
                this.curArea.areaCanvas.DOFade(0, durtion);
                this.downArea.areaCanvas.DOFade(1, durtion);
                this.missionRoot.verticalPoint.transform.DOLocalRotate(new Vector3(90, 0, 0), durtion).SetEase(Ease.Linear).OnComplete(() => {

                    foreach (var item in this.dicOtherAera)
                    {
                        item.Value.areaRoot.transform.SetParent(this.missionRoot.moudleList);
                    }
                    missionRoot.verticalPoint.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    RefreshMissionMoudles(this.downArea.cfgData.areaid);

                    GameCenter.mIns.m_CoroutineMgr.DelayNextFrame(() =>
                    {
                        index += 1;
                        if (index < roPath.Count)
                        {
                            DoRotate(index, roPath);
                        }
                        else
                        {
                            this.missionRoot.mask.SetActive(false);
                        }
                    });

                });
                break;
            case 3://左
                foreach (var item in this.dicOtherAera)
                {
                    item.Value.areaRoot.transform.SetParent(this.missionRoot.horizontalPoint.transform);
                }
                this.curArea.areaRoot.transform.SetParent(this.missionRoot.horizontalPoint.transform);
                this.curArea.areaCanvas.DOFade(0, durtion);
                this.leftArea.areaCanvas.DOFade(1, durtion);
                this.missionRoot.horizontalPoint.transform.DOLocalRotate(new Vector3(0, -90, 0), durtion).SetEase(Ease.Linear).OnComplete(() => {

                    foreach (var item in this.dicOtherAera)
                    {
                        item.Value.areaRoot.transform.SetParent(this.missionRoot.moudleList);
                    }
                    missionRoot.horizontalPoint.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    RefreshMissionMoudles(this.leftArea.cfgData.areaid);

                    GameCenter.mIns.m_CoroutineMgr.DelayNextFrame(() =>
                    {
                        index += 1;
                        if (index < roPath.Count)
                        {
                            DoRotate(index, roPath);
                        }
                        else
                        {
                            this.missionRoot.mask.SetActive(false);
                        }
                    });

                });


                break;
            case 4://右
                foreach (var item in this.dicOtherAera)
                {
                    item.Value.areaRoot.transform.SetParent(this.missionRoot.horizontalPoint.transform);
                }
                this.curArea.areaRoot.transform.SetParent(this.missionRoot.horizontalPoint.transform);
                this.curArea.areaCanvas.DOFade(0, durtion);
                this.rightArea.areaCanvas.DOFade(1, durtion);
                this.missionRoot.horizontalPoint.transform.DOLocalRotate(new Vector3(0, 90, 0), durtion).SetEase(Ease.Linear).OnComplete(() => {

                    foreach (var item in this.dicOtherAera)
                    {
                        item.Value.areaRoot.transform.SetParent(this.missionRoot.moudleList);
                    }
                    missionRoot.horizontalPoint.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    RefreshMissionMoudles(this.rightArea.cfgData.areaid);

                    GameCenter.mIns.m_CoroutineMgr.DelayNextFrame(() =>
                    {
                        index += 1;
                        if (index < roPath.Count)
                        {
                            DoRotate(index, roPath);
                        }
                        else
                        {
                            this.missionRoot.mask.SetActive(false);
                        }
                    });

                });


                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 计算旋转路径
    /// </summary>
    /// <param name="tagetArea"></param>
    public List<int> ComputeRotatPath(int tagetArea)
    {
        List<int> roPath = new List<int>();//记录旋转方向 1-上 2-下 3-左 4-右
        int curIndex = this.curArea.cfgData.list;//当前列表序号
        int tagetAreaIndex = MissionCfgManager.Instance.GetMissionAreaCfgDataByAreaID(tagetArea).list;//目标列表序号
        if (curIndex < tagetAreaIndex)//正序遍历
        {
            for (int i = curIndex; i <= tagetAreaIndex; i++)
            {
                if (i + 1 <= tagetAreaIndex)
                {
                    roPath.Add(GetRotate(i, i + 1));
                }
            }
        }
        else if (curIndex > tagetAreaIndex)//反序遍历
        {
            for (int i = curIndex; i >= tagetAreaIndex; i--)
            {
                if (i - 1 >= tagetAreaIndex)
                {
                    roPath.Add(GetRotate(i, i - 1));
                }
            }
        }
        return roPath;
    }

    /// <summary>
    /// 获得转向目标区块的方向
    /// </summary>
    /// <param name="curIndex"></param>
    /// <param name="nextIndex"></param>
    public int GetRotate(int curIndex,int nextIndex)
    {
        if (Math.Abs(curIndex- nextIndex) != 1)
        {
            Debug.LogError($"计算到非相邻区块，curIndex:{curIndex},nextIndex:{nextIndex},请检查！");
            return 0;
        }
        MissionAreaCfgData curCfg = MissionCfgManager.Instance.GetMissionAreaCfgDataByIndex(curIndex);
        MissionAreaCfgData nextCfg = MissionCfgManager.Instance.GetMissionAreaCfgDataByIndex(nextIndex);
        if (!string.IsNullOrEmpty(nextCfg.left))//是左侧端点 向右翻
        {
            if (curCfg.areaid == int.Parse(nextCfg.left.Split(';')[1]))
            {
                return 4;
            } 
        }
        if (!string.IsNullOrEmpty(nextCfg.right))//是右侧端点 向左翻
        {
            if (curCfg.areaid == int.Parse(nextCfg.right.Split(';')[1]))
            {
                return 3;
            }
        }
        if (!string.IsNullOrEmpty(nextCfg.up))//是上测端点 向下翻
        {
            if (curCfg.areaid == int.Parse(nextCfg.up.Split(';')[1]))
            {
                return 2;
            }
        }
        if (!string.IsNullOrEmpty(nextCfg.down))//是下测端点 向上翻
        {
            if (curCfg.areaid == int.Parse(nextCfg.down.Split(';')[1]))
            {
                return 1;
            }
        }
        return 0;
    }
}



