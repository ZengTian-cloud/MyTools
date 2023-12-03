using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using Object = UnityEngine.Object;
using UnityEngine;
using Cysharp.Threading.Tasks;

public static class missiontool
{

    public static async UniTask<MissionData> JsonDataToMissionDataByAB(string path)
    {
        Object cfg = await ResourcesManager.Instance.LoadAssetSync(path);
        if (cfg != null)
        {
            TextAsset textAsset = (TextAsset)cfg;
            string allText = textAsset.ToString();
            MissionData mapData = JsonMapper.ToObject<MissionData>(allText);
            return mapData;
        }
        return null;
    }

    public static MissionData JsonDataToMissionData(string path)
    {
        string allText = File.ReadAllText(path);
        MissionData mapData = JsonMapper.ToObject<MissionData>(allText);
        return mapData;
    }

    public static EditorMissionData TestJsonDataToMissionData(string path)
    {
        string allText = File.ReadAllText(path);
        EditorMissionData mapData = JsonMapper.ToObject<EditorMissionData>(allText);
        return mapData;
    }

    public static string EditorDataToJson(EditorMissionData editorMissionData)
    {
        MissionData missionData = EditorDataToMissionData(editorMissionData);
        string json = JsonMapper.ToJson(missionData);
        return json;
    }

    public static string EditorDataToTestJson(EditorMissionData editorMissionData)
    {
        return JsonMapper.ToJson(editorMissionData);
    }



    private static string GetTab(int num)
    {
        string s = "";
        for (int i = 0; i < num; i++)
            s += "\t";
        return s;
    }

    //编辑器数据转换为关卡数据
    private static MissionData EditorDataToMissionData(EditorMissionData editorMissionData)
    {
        // cloneTrapList
        List<EditorTrapData> cloneTrapList = new List<EditorTrapData>();
        if (editorMissionData.trapList != null)
        {
            foreach (var item in editorMissionData.trapList)
                cloneTrapList.Add(item.Clone());
        }

        MissionData missionData = new MissionData
        {
            missionName = editorMissionData.missionName,
            pathDatas = new List<PathData>(),
            trapPointList = new List<TrapPointData>()
        };

        for (int i = 0; i < editorMissionData.pathDatas.Count; i++)
        {
            int newCount = -1;
            EditorPathData oneEditorPathData = editorMissionData.pathDatas[i];

            //一条路线的数据
            PathData onePathData = new PathData
            {
                pathIndex = i,
                pointDatas = new List<PointData>(),
                monsterDatas = oneEditorPathData.monsterDatas,
            };
            //先处理主分支
            EditorBranchData mianBranchData = oneEditorPathData.branchDatas[0];
            for (int p = 0; p < mianBranchData.pointDatas.Count; p++)
            {
                EditorPointData oneEditorPoint = mianBranchData.pointDatas[p];//当前格子
                EditorPointData nexrEditorPoint = p + 1 >= mianBranchData.pointDatas.Count ? null : mianBranchData.pointDatas[p + 1];//下个格子
                int curPoint = IsSamePoint(onePathData, oneEditorPoint);
                int nextPoint = IsSamePoint(onePathData, nexrEditorPoint);
                if (curPoint >= 0)//如果本个格子已存在
                {
                    if (nextPoint >= 0)//如果下个格子已存在
                    {
                        onePathData.pointDatas[curPoint].nextPoints.Add(new NextPoint
                        {
                            point = nextPoint,
                            limit = oneEditorPoint.inputCount,
                            branchindex = 0
                        });
                    }
                    else//如果下个格子不存在
                    {
                        if (nexrEditorPoint != null)
                        {
                            onePathData.pointDatas[curPoint].nextPoints.Add(new NextPoint
                            {
                                point = newCount + 1,
                                limit = oneEditorPoint.inputCount,
                                branchindex = 0
                            });
                        }
                    }
                }
                else//如果本个格子不存在
                {
                    newCount = newCount + 1;
                    if (nextPoint >= 0)//如果下个格子已存在
                    {
                        onePathData.pointDatas.Add(new PointData
                        {
                            type = 1,
                            point = newCount,
                            pos = oneEditorPoint.pos,
                            index = oneEditorPoint.index,
                            trapId = GetTarpId(cloneTrapList, oneEditorPoint.index),
                            nextPoints = new List<NextPoint> {
                                new NextPoint{ point = nextPoint,limit = oneEditorPoint.inputCount,branchindex = 0},
                            }
                        });

                    }
                    else//如果下个格子不存在
                    {
                        onePathData.pointDatas.Add(new PointData
                        {
                            type = 1,
                            point = newCount,
                            pos = oneEditorPoint.pos,
                            index = oneEditorPoint.index,
                            trapId = GetTarpId(cloneTrapList, oneEditorPoint.index),
                            nextPoints = new List<NextPoint>(),
                        });
                        if (nexrEditorPoint != null)
                        {
                            onePathData.pointDatas[newCount].nextPoints.Add(new NextPoint
                            {
                                point = newCount + 1,
                                limit = oneEditorPoint.inputCount,
                                branchindex = 0
                            });
                        }
                    }
                }
            }
            //处理其他分支
            for (int other = 1; other < oneEditorPathData.branchDatas.Count; other++)
            {
                EditorBranchData otherBranch = oneEditorPathData.branchDatas[other];//其他分支
                for (int ch = 0; ch < otherBranch.pointDatas.Count; ch++)
                {
                    EditorPointData oneEditorPoint = otherBranch.pointDatas[ch];//当前格子
                    EditorPointData nexrEditorPoint = ch + 1 >= otherBranch.pointDatas.Count ? null : otherBranch.pointDatas[ch + 1];//下个格子
                    int curPoint = IsSamePoint(onePathData, oneEditorPoint);
                    int nextPoint = IsSamePoint(onePathData, nexrEditorPoint);

                    if (curPoint >= 0)//如果本个格子已存在---分支节点
                    {
                        onePathData.pointDatas[curPoint].type = 2;
                        if (nextPoint >= 0)//如果下个格子已存在
                        {
                            onePathData.pointDatas[curPoint].nextPoints.Add(new NextPoint
                            {
                                point = nextPoint,
                                limit = oneEditorPoint.inputCount,
                                branchindex = other
                            });
                        }
                        else//如果下个格子不存在
                        {
                            if (nexrEditorPoint != null)
                            {
                                onePathData.pointDatas[curPoint].nextPoints.Add(new NextPoint
                                {
                                    point = newCount + 1,
                                    limit = oneEditorPoint.inputCount,
                                    branchindex = other
                                });
                            }
                        }
                    }
                    else//如果本个格子不存在
                    {
                        newCount = newCount + 1;
                        if (nextPoint >= 0)//如果下个格子已存在
                        {
                            onePathData.pointDatas.Add(new PointData
                            {
                                type = 1,
                                point = newCount,
                                pos = oneEditorPoint.pos,
                                index = oneEditorPoint.index,
                                trapId = GetTarpId(cloneTrapList, oneEditorPoint.index),
                                nextPoints = new List<NextPoint> {
                                new NextPoint{ point = nextPoint,limit = oneEditorPoint.inputCount,branchindex = other},
                            }
                            });

                        }
                        else//如果下个格子不存在
                        {
                            onePathData.pointDatas.Add(new PointData
                            {
                                type = 1,
                                point = newCount,
                                pos = oneEditorPoint.pos,
                                index = oneEditorPoint.index,
                                trapId = GetTarpId(cloneTrapList, oneEditorPoint.index),
                                nextPoints = new List<NextPoint>(),
                            });
                            if (nexrEditorPoint != null)
                            {
                                onePathData.pointDatas[newCount].nextPoints.Add(new NextPoint
                                {
                                    point = newCount + 1,
                                    limit = oneEditorPoint.inputCount,
                                    branchindex = other
                                });
                            }
                        }
                    }
                }

            }
            missionData.pathDatas.Add(onePathData);
        }
        missionData.rolePointList = editorMissionData.rolePointList;

        if (cloneTrapList != null && cloneTrapList.Count > 0)
        {
            // 还有不在路径点上的机关, 加入到 missionData.trapPointList 中
            //missionData.trapPointList
            foreach (var tp in cloneTrapList)
            {
                bool hasSame = false;
                // 如果已经添加在路径，忽略
                foreach (PathData pd in missionData.pathDatas)
                {
                    foreach (PointData pintdata in pd.pointDatas)
                    {
                        if (pintdata.trapId == tp.trapId)
                        {
                            // Debug.LogWarning("has same ：" + tp.trapId);
                            hasSame = true;
                            break;
                        }
                    }
                }

                if (!hasSame)
                {
                    foreach (EditorTrapPointData p in tp.points)
                    {
                        TrapPointData tpd = new TrapPointData();
                        tpd.trapId = tp.trapId;
                        tpd.index = p.v2;
                        tpd.pos = p.pos;
                        missionData.trapPointList.Add(tpd);
                    }
                }
            }
        }
        return missionData;
    }

    private static long GetTarpId(List<EditorTrapData> etdlist, V2 v2)
    {
        if (etdlist == null)
        {
            return 0;
        }

        for (int i = etdlist.Count - 1; i >= 0; i--)
        {

            for (int j = etdlist[i].points.Count - 1; j >= 0; j--)
            {
                EditorTrapPointData p = etdlist[i].points[j];
                if (p.v2.x == v2.x && p.v2.y == v2.y)
                {
                    long trapId = etdlist[i].trapId;
                    //etdlist[i].points.RemoveAt(j);

                    //if (etdlist[i].points.Count <= 0)
                    //{
                    //    etdlist.RemoveAt(i);
                    //}

                    return trapId;
                }

            }
        }
        return 0;
    }

    //判断是否已添加相同的点位
    private static int IsSamePoint(PathData pathData, EditorPointData editorPointData)
    {
        if (pathData == null || editorPointData == null)
        {
            return -1;
        }
        for (int i = 0; i < pathData.pointDatas.Count; i++)
        {
            V2 inlist = pathData.pointDatas[i].index;
            V2 tolist = editorPointData.index;
            if (inlist.x == tolist.x && inlist.y == tolist.y)
            {
                return i;
            }
        }
        return -1;
    }


}



public class MissionData
{
    public string missionName;//关卡名字
    public List<PathData> pathDatas;
    public List<RolePointData> rolePointList;//可放置角色的格子列表
    public List<TrapPointData> trapPointList;//放置了机关的格子列表（路径外的）
}

public class PathData
{
    public int pathIndex;
    public List<PointData> pointDatas;
    public List<MonsterData> monsterDatas;
}

public class PointData
{
    public int type;//1-普通道路 2-分支节点 
    public int point;
    public V3 pos;
    public V2 index;
    public List<NextPoint> nextPoints;
    public int rolePoint;//是否可放置角色点位 0-fasle 1-true
    public long trapId;//机关id
}

public class NextPoint
{
    public int point;
    public int limit;//进入条件（上个格子的进入次数）
    public int branchindex;//所属分支 
}


