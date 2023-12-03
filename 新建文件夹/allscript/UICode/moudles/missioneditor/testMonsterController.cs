using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 怪物控制器
/// </summary>
public class testMonsterController : MonoBehaviour
{
    public MissionData curMissionData;

    public AnimationController animationController;

    public float moveSpeed;

    private Dictionary<int, int> selfMoveData;//记录自身的行走数据 int-格子point int-进入次数

    private int curPoint;//当前point

    private int nextPoint;//下一个点位的序号

    private Vector3 curPos;//当前pos

    private Vector3 nextPos;//下一个点位的pos

    private Vector3 targetPos;//目标点位

    public bool isMove = false;//是否运动

    private Vector3 selfNormalize;//方向

    public int curPath;//行走路线

    private List<PointData> curPointDatas;//当前路线点位

    public bool isEnd;//是否是终点点位

    private Rigidbody body;

    private void Awake()
    {
        selfMoveData = new Dictionary<int, int>();
        curPointDatas = new List<PointData>();
        body = this.gameObject.GetComponent<Rigidbody>();
        if (body == null)
        {
            body = this.gameObject.AddComponent<Rigidbody>();
        }
        body.useGravity = false;


        curPos = this.gameObject.transform.position;

        nextPos = Vector3.zero;
        curPoint = 0;
        isEnd = false;

        curPath = 0;
    }



    /// <summary>
    /// 开始运动
    /// </summary>
    public void StartMove()
    {
        RefreshCurPath(curPath);
        ComputeNextPoint();
        isMove = true;
        animationController.PlayAnimatorByName("move");

    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        if (isMove)
        {
            if (animationController.curName != "move")
            {
                animationController.PlayAnimatorByName("move");
            }

            targetPos = Vector3.MoveTowards(this.transform.position, nextPos, moveSpeed * Time.deltaTime);
            //转向
            var tarqt = Quaternion.LookRotation(targetPos - this.transform.position);
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, tarqt, 5);

            this.transform.position = targetPos;

            if (Vector3.Distance(this.transform.position, nextPos) <= 0.3f && !isEnd)
            {
                SetNewPoint();
            }
            if (isEnd)
            {
                isMove = false;
                selfMoveData.Clear();
                Destroy(this.gameObject);
            }
        }

    }

    /// <summary>
    /// 刷新路线点位
    /// </summary>
    /// <param name="pathIndex">路线下标</param>
    public void RefreshCurPath(int pathIndex)
    {
        if (pathIndex >= curMissionData.pathDatas.Count)
        {
            Debug.LogError($"切换路线失败，{curMissionData.missionName}没有{pathIndex}路线,请检查！");
            return;
        }
        curPointDatas = curMissionData.pathDatas[pathIndex].pointDatas;
        curPath = pathIndex;
    }


    /// <summary>
    /// 添加数据到我的记录
    /// </summary>
    private void AddToMyMoveData(int point)
    {
        if (selfMoveData.ContainsKey(point))
        {
            selfMoveData[point] += 1;
        }
        else
        {
            selfMoveData.Add(point, 1);
        }
    }


    /// <summary>
    /// 设置新的坐标和点位数据
    /// </summary>
    private void SetNewPoint()
    {
        curPoint = nextPoint;
        AddToMyMoveData(curPoint);
        ComputeNextPoint();
    }

    /// <summary>
    /// 计算下一个坐标
    /// </summary>
    private void ComputeNextPoint()
    {
        if (curPoint < curPointDatas.Count && curPointDatas[curPoint] != null)
        {
            List<NextPoint> nextPoints = curPointDatas[curPoint].nextPoints;
            if (nextPoints.Count == 1)
            {
                nextPoint = nextPoints[0].point;
                RefreshNextPoint(nextPoint);
                return;
            }
            else
            {
                for (int i = 0; i < nextPoints.Count; i++)
                {
                    if (selfMoveData.ContainsKey(curPoint))
                    {
                        if (selfMoveData[curPoint] == nextPoints[i].limit)//当前格子进入次数匹配下个格子的条件次数 
                        {
                            nextPoint = nextPoints[i].point;
                            RefreshNextPoint(nextPoint);
                            return;
                        }
                    }
                }
            }
        }
        isEnd = true;
    }

    /// <summary>
    /// 刷新下一个坐标
    /// </summary>
    /// <param name="point"></param>
    private void RefreshNextPoint(int point)
    {
        nextPos = new Vector3((float)curPointDatas[point].pos.x, 0, (float)curPointDatas[point].pos.z);
    }



}

