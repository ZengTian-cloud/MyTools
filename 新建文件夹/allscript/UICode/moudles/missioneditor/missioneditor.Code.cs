using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Frame.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class missioneditor
{
    public override UILayerType uiLayerType => UILayerType.Normal;
    public override string uiAtlasName => "missionwnd";

    private GameObject mapScence;
    private Transform mapListRoot;
    private Transform inputListRoot;
    private GameObject temp;
    private GameObject input;
    private Camera mapCamera;
    private const int xvalue = 16;//x轴长度
    private const int zvalue = 11;//z轴长度

    private bool isCreat = false;

    private string mapCfgPath = Application.dataPath + "/allcfg/mapcfgs";//地图配置表路径
    private string missionCfgPath = Application.dataPath + "/allcfg/missioncfgs";//关卡配置表路径
    private string missionEditorCfgPath = Application.dataPath + "/allcfg/missioneditorcfgs";//关卡编辑器的配置缓存，仅开发使用，不参与打包

    private Material normalinputMat;//可点击地块材质  默认
    public Material NormalinputMat { get { return normalinputMat; } }
    private Material selectinputMat;//可点击地块材质  选中
    public Material SelectinputMat { get { return selectinputMat; } }

    private GameObject[,] inputList;//可点击地块列表
    private bool isInputShow = false;

    private EditorMissionData missionData;//关卡数据

    private int pathCount;//路线数量
    private int curBranchCount;//当前路线下的分支数量
    private int curPointCount;//当前分支下的点位数量

    private int curPath = 0;
    private int curBranch = 0;
    private int curPoint = 0;

    private GameObject[,] mapList;//地图列表

    private GameObject curBranchListObj;//当前分支列表预支体
    private List<GameObject> allPointObj;//所有点位ui预支体
    private GameObject curSelectPointUI;//当前选中的点位ui
    private GameObject curPathTagUI;
    private GameObject curBranchUI;

    private List<GameObject> allPathTagList;//所有路线tag
    private Dictionary<int, GameObject> dicAllPathPrefab;//路线对应的分支列表预支体 int-路线 obj-分支列表预支体


    private Dictionary<int, List<GameObject>> allBranchList;//所有分支列表 int-路线 list-单个分支

    private int editorState;//编辑器状态 1-编辑路线 2-编辑可放置角色格子

    private bool monsterPanelSwitch;

    private List<GameObject> MonsterPanelData = new List<GameObject>();
    private List<GameObject> gameobjectIsSwitch = new List<GameObject>();
    // 机关
    private missioneditorTrap missioneditorTrap = null;
    protected override async void OnInit()
    {
        editorState = 1;
        pathCount = 0;
        missionData = new EditorMissionData();
        allPathTagList = new List<GameObject>();
        dicAllPathPrefab = new Dictionary<int, GameObject>();
        allBranchList = new Dictionary<int, List<GameObject>>();
        allPointObj = new List<GameObject>();
        CreatMapEditorScence();
        inputList = new GameObject[xvalue, zvalue];
        normalinputMat = await ResourcesManager.Instance.LoadAssetSync("materials/normalMat.mat") as Material;
        selectinputMat = await ResourcesManager.Instance.LoadAssetSync("materials/selectMat.mat") as Material;
        inputListRoot.gameObject.SetActive(isInputShow);

        GameCenter.mIns.m_UIMgr.AddUpdateWin(this);

        missioneditorTrap = new missioneditorTrap(this, InitInputList, inputList);
    }

    private void RemoveAllObj()
    {
        for (int i = 0; i < allPathTagList.Count; i++)
        {
            GameObject.Destroy(allPathTagList[i]);
        }
        foreach (var item in dicAllPathPrefab)
        {
            GameObject.Destroy(item.Value);
        }

        foreach (var item in allBranchList)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                GameObject.Destroy(item.Value[i]);
            }
        }
    }

    protected override void OnOpen()
    {
        mapScence.SetActive(true);
    }

    private Ray ray;
    private RaycastHit hit;
    private GameObject target;
    public GameObject Target { get { return target; } }
    private string count;
    private string[] nameTab;
    public override void UpdateWin()
    {

        if (Input.GetMouseButtonDown(0))
        {
            ray = mapCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.tag == "inputmap")
                {
                    target = hit.collider.gameObject;
                    nameTab = target.name.Split('_');
                    if (editorState == 1)
                    {

                        count = target.GetComponentInChildren<TextMesh>().text;
                        int i = 0;
                        if (string.IsNullOrEmpty(count))
                        {
                            target.GetComponentInChildren<TextMesh>().text = "1";
                            target.GetComponent<MeshRenderer>().material = selectinputMat;
                        }
                        else
                        {

                            try
                            {
                                i = Convert.ToInt32(count);
                            }
                            catch
                            {
                            }
                            target.GetComponentInChildren<TextMesh>().text = (i + 1).ToString();
                            target.GetComponent<MeshRenderer>().material = selectinputMat;
                        }
                        CreatPoint(nameTab[1], nameTab[2], target.transform.position, i + 1);
                    }
                    else if (editorState == 2)
                    {
                        RolePointData data = FindRolePointByIndex(Convert.ToDouble(nameTab[1]), Convert.ToDouble(nameTab[2]));
                        if (data != null)
                        {
                            missionData.rolePointList.Remove(data);
                            target.GetComponent<MeshRenderer>().material = normalinputMat;
                        }
                        else
                        {
                            missionData.rolePointList.Add(new RolePointData
                            {
                                index = new V2
                                {
                                    x = Convert.ToDouble(nameTab[1]),
                                    y = Convert.ToDouble(nameTab[2])
                                },
                                pos = new V3
                                {
                                    x = Convert.ToDouble(target.transform.position.x),
                                    y = Convert.ToDouble(target.transform.position.y),
                                    z = Convert.ToDouble(target.transform.position.z),
                                }
                            });
                            target.GetComponent<MeshRenderer>().material = selectinputMat; ;
                        }
                    }
                    else if (editorState == 3 && missioneditorTrap != null)
                    {
                        V3 pos = new V3();
                        pos.x = target.transform.position.x;
                        pos.y = target.transform.position.y;
                        pos.z = target.transform.rotation.z;
                        missioneditorTrap.ClickMapGrid(Convert.ToDouble(nameTab[1]), Convert.ToDouble(nameTab[2]), pos);

                    }
                }
            }
        }
    }


    private RolePointData FindRolePointByIndex(double _x, double _y)
    {
        if (missionData.rolePointList != null)
        {
            for (int i = 0; i < missionData.rolePointList.Count; i++)
            {
                if (missionData.rolePointList[i].index.x == _x && missionData.rolePointList[i].index.y == _y)
                {
                    return missionData.rolePointList[i];
                }
            }
        }
        else
        {
            missionData.rolePointList = new List<RolePointData>();
        }
        return null;
    }

    //初始化可点击方块
    private void InitInputList()
    {
        for (int x = 0; x < xvalue; x++)
        {
            for (int z = 0; z < zvalue; z++)
            {
                if (inputList[x, z] != null)
                {
                    inputList[x, z].GetComponent<MeshRenderer>().material = normalinputMat;
                    inputList[x, z].GetComponentInChildren<TextMesh>().text = "";
                }
            }

        }
    }

    //生成点位
    private void CreatPoint(string _x, string _y, Vector3 v, int inputCout)
    {
        GameObject curbranch = allBranchList[curPath][curBranch];
        GameObject point = GameObject.Instantiate(PointItem, curbranch.GetComponentInChildren<ScrollRect>().content);
        point.GetComponentInChildren<Text>().text = $"{_x}_{_y}";
        point.SetActive(true);
        curbranch.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 0;
        allPointObj.Add(point);
        int pointcount = missionData.pathDatas[curPath].branchDatas[curBranch].pointDatas.Count;
        pointcount += 1;
        missionData.pathDatas[curPath].branchDatas[curBranch].pointDatas.Add(new EditorPointData
        {
            point = 0,
            index = new V2 { x = Convert.ToDouble(_x), y = Convert.ToDouble(_y) },
            pos = new V3 { x = v.x, y = v.y, z = v.z },
            inputCount = inputCout
        });
        point.GetComponent<Button>().onClick.AddListener(() =>
        {
            SelectPointUI(point, pointcount - 1);
        });
        SelectPointUI(point, pointcount - 1);
    }

    //选中一个点位ui
    private void SelectPointUI(GameObject obj, int index)
    {
        for (int i = 0; i < allPointObj.Count; i++)
        {
            allPointObj[i].transform.Find("select").gameObject.SetActive(obj == allPointObj[i]);
        }
        curSelectPointUI = obj;
        curPoint = index;
    }



    //生成地图编辑器场景
    private async void CreatMapEditorScence()
    {
        mapScence = await ResourcesManager.Instance.LoadUIPrefabSync("mapeditorsence", worldRoot);
        mapScence.transform.localPosition = Vector3.zero;
        mapListRoot = mapScence.transform.Find("MapList");
        inputListRoot = mapScence.transform.Find("InputList");
        temp = mapScence.transform.Find("plane").gameObject;
        input = mapScence.transform.Find("input").gameObject;
        mapCamera = mapScence.transform.Find("Camera").GetComponent<Camera>();
        GameCenter.mIns.m_CamMgr.AddCameraToMainCamera(mapCamera);
    }

    //生成地图列表
    private async void CreatMapList(MapData mapData)
    {
        mapList = new GameObject[xvalue, zvalue];
        CellData oneCell;
        GameObject obj;
        for (int i = 0; i < mapData.maplist.Count; i++)
        {
            oneCell = mapData.maplist[i];
            obj = GameObject.Instantiate(temp, mapListRoot);
            obj.name = $"map_{oneCell.index.x}_{oneCell.index.y}";
            obj.transform.position = new Vector3((float)oneCell.pos.x, (float)oneCell.pos.y, (float)oneCell.pos.z);
            obj.transform.localEulerAngles = new Vector3((float)oneCell.rot.x, (float)oneCell.rot.y, (float)oneCell.rot.z);
            obj.transform.localScale = Vector3.one;
            obj.GetComponentInChildren<MeshRenderer>().material = await ResourcesManager.Instance.LoadAssetSyncByName<Material>($"{oneCell.mat}.mat");
            obj.SetActive(true);
            if (oneCell.index.x > 0 && oneCell.index.x < xvalue - 1 && oneCell.index.y > 0 && oneCell.index.y < zvalue - 1)
            {
                GameObject input = GameObject.Instantiate(this.input, inputListRoot);
                input.name = $"input_{oneCell.index.x}_{oneCell.index.y}";
                input.tag = "inputmap";
                input.transform.position = new Vector3((float)oneCell.pos.x, 0.32f, (float)oneCell.pos.z);
                input.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                input.GetComponent<MeshRenderer>().material = normalinputMat;
                input.SetActive(true);
                input.transform.GetChild(0).gameObject.SetActive(true);
                inputList[(int)oneCell.index.x, (int)oneCell.index.y] = input;
            }
            mapList[(int)oneCell.index.x, (int)oneCell.index.y] = obj;
        }
    }

    //保存配置表
    private void SaveCfg()
    {
        if (!string.IsNullOrEmpty(input_id.text))
        {
            missionData.missionName = $"mission{input_id.text}";
            string code = missiontool.EditorDataToJson(missionData);
            FileOperate.FileWrite($"{missionCfgPath}/{missionData.missionName}.txt", code);

            string content = missiontool.EditorDataToTestJson(missionData);
            FileOperate.FileWrite($"{missionEditorCfgPath}/missioneditor{input_id.text}.txt", content);
        }
        else
        {
            Debug.Log("关卡id为空，请输入关卡id后保存！");
        }

    }

    //保存机关配置表
    public void SaveTrapCfg(List<MissionEditorTrapItem> m_TrapItems)
    {
        if (m_TrapItems == null)
        {
            m_TrapItems = new List<MissionEditorTrapItem>();
        }
        if (!string.IsNullOrEmpty(input_id.text))
        {
            List<EditorTrapData> trapDatas = new List<EditorTrapData>();
            for (int i = 0; i < m_TrapItems.Count; i++)
            {
                EditorTrapData trapData = new EditorTrapData();
                trapData.points = m_TrapItems[i].indexList;
                trapData.trapId = m_TrapItems[i].GetTrapId();
                if (trapData.trapId > 0 && trapData.points.Count > 0)
                {
                    trapDatas.Add(trapData);
                }
            }
            //if (trapDatas.Count <= 0)
            //{
                //return;
            //}
            missionData.trapList = trapDatas;
            missionData.missionName = $"mission{input_id.text}";
            string code = missiontool.EditorDataToJson(missionData);
            FileOperate.FileWrite($"{missionCfgPath}/{missionData.missionName}.txt", code);

            string content = missiontool.EditorDataToTestJson(missionData);
            FileOperate.FileWrite($"{missionEditorCfgPath}/missioneditor{input_id.text}.txt", content);
        }
        else
        {
            Debug.Log("关卡id为空，请输入关卡id后保存！");
        }

    }

    //导入配置表
    private void LoadCfg()
    {

        if (!string.IsNullOrEmpty(input_id.text))
        {
            string allText = File.ReadAllText($"{mapCfgPath}/map{input_id.text}.txt");
            MapData mapData = maptool.MapJsonToClassData(allText);
            // if (!isCreat)
            //{
            if (mapList != null)
            {
                for (int i = 0; i < xvalue; i++)
                {
                    for (int j = 0; j < zvalue; j++)
                    {
                        if (mapList[i, j] != null)
                        {
                            GameObject.Destroy(mapList[i, j]);
                        }
                        if (inputList[i, j] != null)
                        {
                            GameObject.Destroy(inputList[i, j]);
                        }

                    }
                }
            }
            CreatMapList(mapData);
            //   isCreat = true;
            // }
            string path = $"{missionCfgPath}/mission{input_id.text}.txt";
            string editorCfgPath = $"{missionEditorCfgPath}/missioneditor{input_id.text}.txt";
            RestarAll();
            if (File.Exists(path))
            {
                missionData = missiontool.TestJsonDataToMissionData(editorCfgPath);
                LoadCfgPanel();
            }
            else
            {
                missionData = new EditorMissionData();
                CreatDefult();
            }
            if (missioneditorTrap != null)
            {
                missioneditorTrap.SetEditorMissionData(missionData);
            }
        }
        else
        {
            Debug.LogError("关卡id为空，先输入关卡id再进行导出操作！");
        }
    }

    //加载已有配置表生成面板
    private void LoadCfgPanel()
    {
        for (int i = 0; i < missionData.pathDatas.Count; i++)
        {
            //路线tag
            int _i = i;
            pathCount = pathCount + 1;
            GameObject path = GameObject.Instantiate(pathTag, pathList.content);
            path.GetComponentInChildren<Text>().text = $"路线{i + 1}";
            path.SetActive(true);
            path.GetComponent<Button>().onClick.AddListener(() =>
            {
                SetSelectPathTag(_i);
            });
            allPathTagList.Add(path);

            //路线对应的分支列表
            GameObject onePath = GameObject.Instantiate(branchList.gameObject, pathroot.transform);
            if (dicAllPathPrefab.ContainsKey(i))
            {
                dicAllPathPrefab[i] = onePath;
            }
            else
            {
                dicAllPathPrefab.Add(i, onePath);
            }
            SetSelectPathTag(i);//下标
            EditorPathData onePathData = missionData.pathDatas[i];
            for (int b = 0; b < onePathData.branchDatas.Count; b++)
            {
                int _b = b;
                GameObject branch = GameObject.Instantiate(PointList, curBranchListObj.GetComponent<ScrollRect>().content);
                branch.transform.Find("btn_branch").GetComponentInChildren<Text>().text = $"分支{b + 1}";
                branch.transform.Find("btn_branch").GetComponent<Button>().onClick.AddListener(() =>
                {
                    SetSelectBranch(_b);
                });
                branch.SetActive(true);
                if (!allBranchList.ContainsKey(curPath))
                {
                    allBranchList.Add(curPath, new List<GameObject>());
                }
                allBranchList[curPath].Add(branch);
                SetSelectBranch(b);

                EditorBranchData oneBranch = onePathData.branchDatas[b];
                for (int p = 0; p < oneBranch.pointDatas.Count; p++)
                {
                    EditorPointData editorPointData = oneBranch.pointDatas[p];

                    GameObject curbranch = allBranchList[curPath][curBranch];
                    GameObject point = GameObject.Instantiate(PointItem, curbranch.GetComponentInChildren<ScrollRect>().content);
                    point.GetComponentInChildren<Text>().text = $"{editorPointData.index.x}_{editorPointData.index.y}";
                    point.SetActive(true);
                    allPointObj.Add(point);
                    int _p = p;
                    point.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SelectPointUI(point, _p);
                    });
                }
            }
        }
    }

    //重置所有 导入新配置的时候调用一次
    private void RestarAll()
    {
        for (int i = 0; i < pathroot.transform.childCount; i++)
        {
            GameObject.Destroy(pathroot.transform.GetChild(i).gameObject);
        }
        for (int i = 1; i < pathList.content.childCount; i++)
        {
            GameObject.Destroy(pathList.content.GetChild(i).gameObject);
        }
        pathCount = 0;
        curBranchCount = 0;
        curPointCount = 0;
        curPath = -1;
        curBranch = -1;
        curPath = -1;
        allPathTagList.Clear();
        dicAllPathPrefab.Clear();
        allBranchList.Clear();
        allPointObj.Clear();
    }

    //生成默认路线和分支
    private void CreatDefult()
    {
        CreatPath();
    }

    //创建分支
    private void CreatBranch()
    {
        curBranchCount = missionData.pathDatas[curPath].branchDatas.Count;
        curBranchCount = curBranchCount + 1;
        int curBranch = curBranchCount - 1;
        GameObject branch = GameObject.Instantiate(PointList, curBranchListObj.GetComponent<ScrollRect>().content);
        branch.transform.Find("btn_branch").GetComponentInChildren<Text>().text = $"分支{curBranchCount}";
        branch.transform.Find("btn_branch").GetComponent<Button>().onClick.AddListener(() =>
        {
            SetSelectBranch(curBranch);
        });
        branch.SetActive(true);
        missionData.pathDatas[curPath].branchDatas.Add(new EditorBranchData
        {
            branchIndex = curBranchCount,
            pointDatas = new List<EditorPointData>()
        });
        if (!allBranchList.ContainsKey(curPath))
        {
            allBranchList.Add(curPath, new List<GameObject>());
        }
        allBranchList[curPath].Add(branch);
        SetSelectBranch(curBranchCount - 1);
    }

    //删除分支
    private void Deletbranch()
    {
        GameObject.Destroy(curBranchUI);
        missionData.pathDatas[curPath].branchDatas.RemoveAt(curBranch);
        allBranchList[curPath].RemoveAt(curBranch);
    }

    //创建路线
    private void CreatPath()
    {
        //路线tag
        pathCount = pathCount + 1;
        int curIndex = pathCount - 1;
        GameObject path = GameObject.Instantiate(pathTag, pathList.content);
        path.GetComponentInChildren<Text>().text = $"路线{pathCount}";
        path.SetActive(true);
        path.GetComponent<Button>().onClick.AddListener(() =>
        {
            SetSelectPathTag(curIndex);
        });
        allPathTagList.Add(path);
        if (missionData.pathDatas == null)
        {
            missionData.pathDatas = new List<EditorPathData>();
        }
        missionData.pathDatas.Add(new EditorPathData
        {
            pathIndex = pathCount,
            branchDatas = new List<EditorBranchData>()
        });



        //路线对应的分支列表
        GameObject onePath = GameObject.Instantiate(branchList.gameObject, pathroot.transform);
        if (dicAllPathPrefab.ContainsKey(pathCount - 1))
        {
            dicAllPathPrefab[pathCount - 1] = onePath;
        }
        else
        {
            dicAllPathPrefab.Add(pathCount - 1, onePath);
        }
        SetSelectPathTag(pathCount - 1);//下标

        CreatBranch();
    }

    //设置选中的路线(按钮表现)
    private void SetSelectPathTag(int index)
    {
        if (curPath == index)
        {
            return;
        }
        InitInputList();
        for (int i = 0; i < allPathTagList.Count; i++)
        {
            allPathTagList[i].transform.Find("normal").gameObject.SetActive(index != i);
            allPathTagList[i].transform.Find("select").gameObject.SetActive(index == i);
            if (index == i)
            {
                curPathTagUI = allPathTagList[i];
            }
        }

        foreach (var item in dicAllPathPrefab)
        {
            item.Value.SetActive(item.Key == index);
            if (item.Key == index)
            {
                curBranchListObj = item.Value;
            }
        }
        curPath = index;
    }


    //设置选中的分支(按钮表现)
    private void SetSelectBranch(int index)
    {

        List<GameObject> curbranch = allBranchList[curPath];
        for (int i = 0; i < curbranch.Count; i++)
        {
            curbranch[i].transform.Find("btn_branch/normal").gameObject.SetActive(index != i);
            curbranch[i].transform.Find("btn_branch/select").gameObject.SetActive(index == i);
            if (index == i)
            {
                curBranchUI = curbranch[i];
            }
        }
        curBranch = index;
        RefreshInputList();
    }

    private void RefreshInputList()
    {
        if (curPath == -1 || curBranch == -1)
        {
            return;
        }
        InitInputList();
        List<EditorPointData> editorPointDatas = missionData.pathDatas[curPath].branchDatas[curBranch].pointDatas;
        for (int i = 0; i < editorPointDatas.Count; i++)
        {
            GameObject input = inputList[(int)editorPointDatas[i].index.x, (int)editorPointDatas[i].index.y];
            input.GetComponent<MeshRenderer>().material = selectinputMat;
            string count = input.GetComponentInChildren<TextMesh>().text;
            if (!string.IsNullOrEmpty(count))
            {
                input.GetComponentInChildren<TextMesh>().text = (Convert.ToInt32(count) + 1).ToString();
            }
            else
            {
                input.GetComponentInChildren<TextMesh>().text = "1";
            }
        }
    }

    //删除路线
    private void DeletPath()
    {
        GameObject.Destroy(dicAllPathPrefab[curPath]);
        GameObject.Destroy(curPathTagUI);
        missionData.pathDatas.RemoveAt(curPath);
        allPathTagList.Remove(curPathTagUI);
        dicAllPathPrefab.Remove(curPath);
    }

    //删除点位
    private void DeletPoint()
    {
        EditorPointData editorPointData = missionData.pathDatas[curPath].branchDatas[curBranch].pointDatas[curPoint];
        GameObject input = inputList[(int)editorPointData.index.x, (int)editorPointData.index.y];
        string count = input.GetComponentInChildren<TextMesh>().text;
        int sub = int.Parse(count) - 1;
        if (sub > 0)
        {
            input.GetComponentInChildren<TextMesh>().text = sub.ToString();
        }
        else
        {
            input.GetComponentInChildren<TextMesh>().text = "";
            input.GetComponent<MeshRenderer>().material = normalinputMat;
        }
        missionData.pathDatas[curPath].branchDatas[curBranch].pointDatas.Remove(editorPointData);
        allPointObj.Remove(curSelectPointUI);
        GameObject.Destroy(curSelectPointUI);
    }

    //保存
    partial void btn_save_Click()
    {
        MonsterPanelSave_Click();
        SaveCfg();
    }
    //导出
    partial void btn_get_Click()
    {
        LoadCfg();
        ReflushCfg();
    }

    //创建分支
    partial void btn_creatBranch_Click()
    {
        CreatBranch();
    }
    //删除分支
    partial void btn_delBranch_Click()
    {
        Deletbranch();
    }
    //创建路线
    partial void btn_creatPath_Click()
    {
        CreatPath();
    }
    //删除路线
    partial void btn_delPath_Click()
    {
        DeletPath();
    }
    //删除点位
    partial void btn_delPoint_Click()
    {
        DeletPoint();
    }
    //返回
    partial void btn_back_Click()
    {
        this.Close();
        mapScence.SetActive(false);
    }
    //切换编辑视图
    partial void btn_switch_Click()
    {
        isInputShow = !isInputShow;
        inputListRoot.gameObject.SetActive(isInputShow);
    }
    //进入设置可放置角色格子的模式
    partial void btn_rolepoint_Click()
    {
        InitInputList();
        editorState = 2;
        rolePoint.SetActive(true);
        for (int i = 0; i < missionData.rolePointList.Count; i++)
        {
            inputList[(int)missionData.rolePointList[i].index.x, (int)missionData.rolePointList[i].index.y].GetComponent<MeshRenderer>().material = selectinputMat;
        }
    }
    //进入设置机关模式
    partial void btn_trap_Click()
    {
        InitInputList();
        editorState = 3;
        trapPanel.SetActive(true);
    }
    partial void btn_monster_panel_Click()
    {
        monsterPanelSwitch = !monsterPanelSwitch;
        monsterPanel.SetActive(monsterPanelSwitch);
    }
    //退出设置可放置角色格子的模式
    partial void btn_outRolePoint_Click()
    {
        InitInputList();
        editorState = 1;
        rolePoint.SetActive(false);
    }
    //怪物界面复制按钮
    partial void MonsterPanelCopy_Click()
    {
        if (MonsterPanelToggleIsOn().Count > 0)
        {
            foreach (var item in MonsterPanelToggleIsOn())
            {
                MonsterPanelData.Add(GameObject.Instantiate(item, MonsterPanelContents.transform.GetChild(0).Find("Viewport/Content").transform));
            }
        }
    }
    //怪物界面删除按钮
    partial void MonsterPanelDelete_Click()
    {
        List<GameObject> object_delete = new List<GameObject>();
        object_delete = MonsterPanelToggleIsOn();
        int a = object_delete.Count;
        if (a > 0)
        {
            for (int i = 0; i < a; i++)
            {
                GameObject go = object_delete[i];
                MonsterPanelData.Remove(go);
                GameObject.Destroy(go);
            }
        }
        gameobjectIsSwitch.Clear();
    }
    //怪物界面新建按钮
    partial void MonsterPanelNew_Click()
    {

        GameObject monsterInput = GameObject.Instantiate(MonsterInput, MonsterPanelContents.transform.GetChild(0).Find("Viewport/Content").transform);
        monsterInput.transform.Find("EndInput").gameObject.AddComponent<MonsterPanelCompute>();
        monsterInput.SetActive(true);
        MonsterPanelData.Add(monsterInput);
    }
    //怪物界面上移按钮
    partial void MonsterPanelUp_Click()
    {
        if (MonsterPanelToggleIsOn().Count > 0)
        {
            foreach (var item in MonsterPanelToggleIsOn())
            {
                if (item.transform.GetSiblingIndex() > 0)
                {
                    item.transform.SetSiblingIndex(item.transform.GetSiblingIndex() - 1);
                }
            }
        }
    }
    //怪物界面下移按钮
    partial void MonsterPanelDown_Click()
    {
        if (MonsterPanelToggleIsOn().Count > 0)
        {
            foreach (var item in MonsterPanelToggleIsOn())
            {
                if (item.transform.GetSiblingIndex() < item.transform.parent.childCount - 1)
                {
                    item.transform.SetSiblingIndex(item.transform.GetSiblingIndex() + 1);
                }
            }
        }
    }
    //怪物界面上一波按钮
    partial void MonsterPanelLast_Click()
    {
        if (MonsterPanelContents.transform.childCount > 0)
        {
            if (MonsterPanelContents.transform.GetChild(0).Find("MonsterPanelIndex").GetComponent<Text>().text != "1")
            {
                //第一个元素置于最后一个
                MonsterPanelContents.transform.GetChild(0).SetAsLastSibling();
                MonsterPanelOneIsShow();
            }
        }
    }
    //怪物界面下一波按钮
    partial void MonsterPanelNext_Click()
    {
        if (MonsterPanelContents.transform.GetChild(0).Find("MonsterPanelIndex").GetComponent<Text>().text != (MonsterPanelContents.transform.childCount).ToString())
        {
            if (MonsterPanelContents.transform.childCount > 0)
            {
                //最后一个元素置于第一个
                MonsterPanelContents.transform.GetChild(MonsterPanelContents.transform.childCount - 1).SetAsFirstSibling();
                MonsterPanelOneIsShow();
            }
        }
    }
    //怪物界面新建一页按钮
    partial void MonsterPanelNewOne_Click()
    {

        GameObject MonsterPanelDemo = GameObject.Instantiate(monsterPanel.transform.Find("MonsterPanelContentDemo").gameObject, MonsterPanelContents.transform);

        MonsterPanelDemo.SetActive(true);
        MonsterPanelDemo.transform.SetAsFirstSibling();
        MonsterPanelOneIsShow();
        AdjustAddIndex(MonsterPanelDemo);
    }
    //怪物界面删除一页按钮
    partial void MonsterPanelDeleteOne_Click()
    {
        List<GameObject> object_delete = new List<GameObject>();
        object_delete = MonsterPanelToggleIsOn();
        if (MonsterPanelContents.transform.childCount > 0)
        {
            int SubIndex = int.Parse(MonsterPanelContents.transform.GetChild(0).transform.Find("MonsterPanelIndex").GetComponent<Text>().text);
            for (int i = 0; i < MonsterPanelContents.transform.GetChild(0).Find("Viewport/Content").childCount; i++)
            {
                MonsterPanelData.Remove(MonsterPanelContents.transform.GetChild(0).Find("Viewport/Content").GetChild(i).gameObject);
            }

            GameObject.Destroy(MonsterPanelContents.transform.GetChild(0).gameObject);
            MonsterPanelOneIsShow_Delete();
            AdjustSubIndex(SubIndex);
        }

    }

    //怪物界面复制新建按钮
    partial void MonsterPanelCopyNew_Click()
    {
        GameObject MonsterPanelDemoNew = GameObject.Instantiate(MonsterPanelContents.transform.GetChild(0).gameObject, MonsterPanelContents.transform);
        MonsterPanelDemoNew.transform.SetAsFirstSibling();
        MonsterPanelOneIsShow();
        AdjustCopyIndex(MonsterPanelDemoNew);
    }
    //怪物界面保存按钮
    partial void MonsterPanelSave_Click()
    {
        // 先清理本地数据
        for (int pi = 0; pi < missionData.pathDatas.Count; pi++)
        {
            missionData.pathDatas[pi].monsterDatas = new List<MonsterData>();
        }

        // 波数
        int wareCount = MonsterPanelContents.transform.childCount;
        if (wareCount <= 0) return;

        for (int wareIndex = 0; wareIndex < wareCount; wareIndex++)
        {
            int monsterGroupCount = MonsterPanelContents.transform.GetChild(wareIndex).Find("Viewport/Content").childCount;
            for (int mgIndex = 0; mgIndex < monsterGroupCount; mgIndex++)
            {
                // 一波中一组怪物配置的对象
                Transform oneMonsterGroupItem = MonsterPanelContents.transform.GetChild(wareIndex).Find("Viewport/Content").GetChild(mgIndex);
                // 该组怪物所在的路径id
                int pathId = int.Parse(oneMonsterGroupItem.Find("RoadIDInput").GetComponent<InputField>().text);
                // 该路径是否在配置的路线中
                bool isInPathCfg = false;
                for (int y = 0; y < missionData.pathDatas.Count; y++)
                {
                    if (missionData.pathDatas[y].pathIndex == pathId)
                        isInPathCfg = true;
                }

                // 是一条新的路径，着生成一条新路径
                if (!isInPathCfg)
                {
                    EditorPathData editorPathData = new EditorPathData();
                    editorPathData.pathIndex = pathId;
                    editorPathData.branchDatas = new List<EditorBranchData>();
                    editorPathData.monsterDatas = new List<MonsterData>();
                    missionData.pathDatas.Add(editorPathData);
                }

                for (int pi = 0; pi < missionData.pathDatas.Count; pi++)
                {
                    if (missionData.pathDatas[pi].pathIndex == pathId)
                    {
                        if (missionData.pathDatas[pi].monsterDatas.Count <= wareIndex)
                        {
                            MonsterData monsterData = new MonsterData();
                            monsterData.waveDatas = new List<waveData>();
                            missionData.pathDatas[pi].monsterDatas.Add(monsterData);
                        }

                        missionData.pathDatas[pi].monsterDatas[wareIndex].waveIndex = int.Parse(MonsterPanelContents.transform.GetChild(wareIndex).Find("MonsterPanelIndex").GetComponent<Text>().text);
                        waveData newWD = new waveData();
                        //Debug.LogError("~~ curPath:" + curPath + " - wareIndex:" + wareIndex + " -  missionData.pathDatas:" + missionData.pathDatas.Count);
                        // int wavecount = missionData.pathDatas[curPath].monsterDatas[wareIndex].waveDatas.Count;
                        newWD.RoadID = oneMonsterGroupItem.Find("RoadIDInput").GetComponent<InputField>().text;
                        newWD.MonsterID = oneMonsterGroupItem.Find("MonsterIDInput").GetComponent<InputField>().text;
                        newWD.spawn = oneMonsterGroupItem.Find("SpawnInput").GetComponent<InputField>().text;
                        newWD.Start = oneMonsterGroupItem.Find("StartInput").GetComponent<InputField>().text;
                        newWD.End = oneMonsterGroupItem.Find("EndInput").GetComponent<Text>().text;
                        newWD.Interval = oneMonsterGroupItem.Find("IntervalInput").GetComponent<InputField>().text;
                        newWD.Count = oneMonsterGroupItem.Find("CountInput").GetComponent<InputField>().text;
                        newWD.Event = oneMonsterGroupItem.Find("eventInput").GetComponent<InputField>().text;
                        missionData.pathDatas[pi].monsterDatas[wareIndex].waveDatas.Add(newWD);

                    }
                }
            }
        }
        SaveCfg();
    }
    //加载配置表时初始化怪物界面
    private GameObject MonsterPanelNewOne_Init()
    {
        GameObject MonsterPanelDemo = GameObject.Instantiate(monsterPanel.transform.Find("MonsterPanelContentDemo").gameObject, MonsterPanelContents.transform);
        MonsterPanelOneIsShow();
        AdjustAddIndex(MonsterPanelDemo);
        return MonsterPanelDemo;
    }
    //加载配置表时初始化每个怪物数据
    private GameObject MonsterPanelNew_Init()
    {
        GameObject monsterInput = GameObject.Instantiate(MonsterInput, MonsterPanelContents.transform.GetChild(MonsterPanelContents.transform.childCount - 1).Find("Viewport/Content").transform);
        monsterInput.SetActive(true);
        monsterInput.transform.Find("EndInput").gameObject.AddComponent<MonsterPanelCompute>();
        MonsterPanelData.Add(monsterInput);
        return monsterInput;
    }
    //加载刷新配置表
    private void ReflushCfg()
    {
        if (!string.IsNullOrEmpty(input_id.text))
        {

            DeleteAlldata();
            string path = $"{missionCfgPath}/mission{input_id.text}.txt";
            string editorCfgPath = $"{missionEditorCfgPath}/missioneditor{input_id.text}.txt";
            if (File.Exists(path) && missionData.missionName.Replace("mission", "") == input_id.text)
            {
                missionData = missiontool.TestJsonDataToMissionData(editorCfgPath);
                if (missionData.pathDatas[curPath].monsterDatas.Count != 0)
                {
                    foreach (var item in missionData.pathDatas[curPath].monsterDatas)
                    {
                        if (item.waveIndex > 0 && item.waveDatas.Count != 0)
                        {
                            GameObject MonsterPanelDemo = GameObject.Instantiate(monsterPanel.transform.Find("MonsterPanelContentDemo").gameObject, MonsterPanelContents.transform);
                            MonsterPanelDemo.transform.Find("MonsterPanelIndex").GetComponent<Text>().text = item.waveIndex.ToString();
                            MonsterPanelOneIsShow();
                            foreach (var it in item.waveDatas)
                            {
                                GameObject monsterInput = MonsterPanelNew_Init();
                                monsterInput.transform.Find("RoadIDInput").GetComponent<InputField>().text = it.RoadID;
                                monsterInput.transform.Find("MonsterIDInput").GetComponent<InputField>().text = it.MonsterID;
                                monsterInput.transform.Find("SpawnInput").GetComponent<InputField>().text = it.spawn;
                                monsterInput.transform.Find("StartInput").GetComponent<InputField>().text = it.Start;
                                monsterInput.transform.Find("EndInput").GetComponent<Text>().text = it.End;
                                monsterInput.transform.Find("IntervalInput").GetComponent<InputField>().text = it.Interval;
                                monsterInput.transform.Find("CountInput").GetComponent<InputField>().text = it.Count;
                                monsterInput.transform.Find("eventInput").GetComponent<InputField>().text = it.Event;

                            }
                        }
                    }

                }
                else
                {
                    GameObject MonsterPanelDemo = GameObject.Instantiate(monsterPanel.transform.Find("MonsterPanelContentDemo").gameObject, MonsterPanelContents.transform);
                    MonsterPanelDemo.transform.Find("MonsterPanelIndex").GetComponent<Text>().text = "1";
                }
            }
        }
        else
        {
            Debug.LogError("关卡id为空，先输入关卡id再进行导出操作！");
        }
    }

    private void DeleteAlldata()
    {
        MonsterPanelData.Clear();
        int a = MonsterPanelContents.transform.childCount;
        for (int i = 0; i < a; i++)
        {
            GameObject.DestroyImmediate(MonsterPanelContents.transform.GetChild(0).gameObject);
        }

    }

    //调整添加顺序
    private void AdjustAddIndex(GameObject go)
    {
        if (MonsterPanelContents.transform.childCount < 1)
        {
            go.transform.Find("MonsterPanelIndex").GetComponent<Text>().text = "1";
        }
        else
        {
            for (int i = 1; i < MonsterPanelContents.transform.childCount; i++)
            {
                go.transform.Find("MonsterPanelIndex").GetComponent<Text>().text = ((int.Parse(MonsterPanelContents.transform.GetChild(1).Find("MonsterPanelIndex").GetComponent<Text>().text) + 1)).ToString();
                if (int.Parse(MonsterPanelContents.transform.GetChild(i).Find("MonsterPanelIndex").GetComponent<Text>().text) >= int.Parse(go.transform.Find("MonsterPanelIndex").GetComponent<Text>().text))
                {
                    MonsterPanelContents.transform.GetChild(i).Find("MonsterPanelIndex").GetComponent<Text>().text = (int.Parse(MonsterPanelContents.transform.GetChild(i).Find("MonsterPanelIndex").GetComponent<Text>().text) + 1).ToString();
                }
            }
        }
    }

    //调整删除顺序
    private void AdjustSubIndex(int SubIndex)
    {
        if (MonsterPanelContents.transform.childCount > 0)
        {
            for (int i = 1; i < MonsterPanelContents.transform.childCount; i++)
            {
                if (int.Parse(MonsterPanelContents.transform.GetChild(i).Find("MonsterPanelIndex").GetComponent<Text>().text) >= SubIndex)
                {
                    MonsterPanelContents.transform.GetChild(i).Find("MonsterPanelIndex").GetComponent<Text>().text = (int.Parse(MonsterPanelContents.transform.GetChild(i).Find("MonsterPanelIndex").GetComponent<Text>().text) - 1).ToString();
                }
            }
        }
    }
    //调整复制新建顺序
    private void AdjustCopyIndex(GameObject go)
    {
        if (MonsterPanelContents.transform.childCount < 1)
        {
            go.transform.Find("MonsterPanelIndex").GetComponent<Text>().text = "1";
        }
        else
        {
            for (int i = 1; i < MonsterPanelContents.transform.childCount; i++)
            {
                go.transform.Find("MonsterPanelIndex").GetComponent<Text>().text = ((int.Parse(MonsterPanelContents.transform.GetChild(1).Find("MonsterPanelIndex").GetComponent<Text>().text) + 1)).ToString();
                if (int.Parse(MonsterPanelContents.transform.GetChild(i).Find("MonsterPanelIndex").GetComponent<Text>().text) >= int.Parse(go.transform.Find("MonsterPanelIndex").GetComponent<Text>().text))
                {
                    MonsterPanelContents.transform.GetChild(i).Find("MonsterPanelIndex").GetComponent<Text>().text = (int.Parse(MonsterPanelContents.transform.GetChild(i).Find("MonsterPanelIndex").GetComponent<Text>().text) + 1).ToString();
                }
            }
        }

    }
    //MonsterPanelContents下第一个物体显示,剩下的隐藏
    private void MonsterPanelOneIsShow()
    {
        int a = MonsterPanelContents.transform.childCount;

        if (a > 0)
        {
            for (int i = 0; i < a; i++)
            {
                MonsterPanelContents.transform.GetChild(i).gameObject.SetActive(false);
                int b = MonsterPanelContents.transform.GetChild(i).Find("Viewport/Content").childCount;
                for (int x = 0; x < b; x++)
                {
                    MonsterPanelContents.transform.GetChild(i).Find("Viewport/Content").GetChild(x).Find("SwitchToggle").GetComponent<Toggle>().isOn = false;
                }
            }
            MonsterPanelContents.transform.GetChild(0).gameObject.SetActive(true);
        }
    }
    //MonsterPanelContents下第一个物体显示,剩下的隐藏,删除时候用
    private void MonsterPanelOneIsShow_Delete()
    {
        int a = MonsterPanelContents.transform.childCount;
        if (a > 1)
        {
            for (int i = 0; i < a; i++)
            {
                MonsterPanelContents.transform.GetChild(i).gameObject.SetActive(false);
            }
            MonsterPanelContents.transform.GetChild(1).gameObject.SetActive(true);

        }
    }
    //返回选中的选中行
    private List<GameObject> MonsterPanelToggleIsOn()
    {
        gameobjectIsSwitch.Clear();
        foreach (var item in MonsterPanelData)
        {
            if (item.transform.Find("SwitchToggle").GetComponent<Toggle>().isOn)
            {
                gameobjectIsSwitch.Add(item);
            }
        }

        return gameobjectIsSwitch;
    }


    private bool bMoving = false;
    /// <summary>
    /// 出怪预览
    /// </summary>
    partial void btn_monsterMove_Click()
    {
        if (bMoving)
        {
            Debug.LogError("正在预览！！！");
            return;
        }
        if (!string.IsNullOrEmpty(input_id.text))
        {
            bMoving = true;
            string missionName = $"mission{input_id.text}.txt";
            MissionData data = missiontool.JsonDataToMissionData(Path.Combine(missionCfgPath, missionName));
            CreatMonsterByCfg(data);
        }
    }

    private void CreatMonsterByCfg(MissionData missionData)
    {
        //获得关卡路线配置
        List<PathData> pathDatas = missionData.pathDatas;
        PathData onePath;//单跳路线 
        MonsterData monsterData;//单个波次的怪物数据

        int waveDelay = 0;//当前波次的延时
        for (int i = 0; i < pathDatas.Count; i++)
        {
            onePath = pathDatas[i];
            if (onePath.monsterDatas != null)
            {
                for (int m = 0; m < onePath.monsterDatas.Count; m++)
                {
                    monsterData = onePath.monsterDatas[m];
                    if (m > 0)
                    {
                        //从第二波开始，延时取上一波的结束时间
                        waveDelay = int.Parse(monsterData.waveDatas[monsterData.waveDatas.Count - 1].End);
                    }
                    for (int w = 0; w < monsterData.waveDatas.Count; w++)
                    {
                        waveData waveData = monsterData.waveDatas[w];
                        int delay = int.Parse(waveData.Start);
                        V3 startPos = onePath.pointDatas[0].pos;
                        int pathIndex = i;
                        GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(waveDelay + delay, () => { CreatMonsterByWavedata(waveData, startPos, missionData, pathIndex); });
                        if (w == monsterData.waveDatas.Count - 1 && m == onePath.monsterDatas.Count - 1)
                        {
                            GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(waveDelay + delay, () => { bMoving = false; });
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 根据波次数据生成怪物
    /// </summary>
    public void CreatMonsterByWavedata(waveData waveData, V3 startPos, MissionData missionData, int pathindex)
    {
        int count = int.Parse(waveData.Count);//怪物数量
        long monsterID = long.Parse(waveData.MonsterID);
        int interval = int.Parse(waveData.Interval);//间隔时间
        for (int i = 0; i < count; i++)
        {
            int time = i * interval;
            GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(time, () => { CreatOneMonster(monsterID, startPos, missionData, pathindex); });

        }
    }

    /// <summary>
    /// 生成一只怪物
    /// </summary>
    public async void CreatOneMonster(long monsterID, V3 startPos, MissionData missionData, int pathindex)
    {
        GameObject root = null;
        GameObject oneMonster = null;
        MonsterDataCfg cfg = MonsterDataManager.Instance.GetMonsterCfgByMonsterID(monsterID);

        //加载怪物对象节点
        root = await ResourcesManager.Instance.LoadUIPrefabSync("battleObjRoot");
        oneMonster = await LoadMonsterModelByHeroID(root, cfg.modelid);
        oneMonster.transform.SetParent(root.transform);
        oneMonster.transform.localPosition = Vector3.zero;

        root.transform.position = new Vector3((float)startPos.x, 0, (float)startPos.z);
        testMonsterController controller = root.GetOrAddCompoonet<testMonsterController>();
        controller.curPath = pathindex;
        controller.animationController = root.GetOrAddCompoonet<AnimationController>();
        controller.animationController.PlayAnimatorByName("");
        controller.curMissionData = missionData;
        controller.moveSpeed = MonsterDataManager.Instance.GetMonsterAttrByMonsterIDAndAttrID(monsterID, 10100113) / 100f;
        controller.StartMove();
    }

    /// <summary>
    /// 加载怪物模型
    /// </summary>
    /// <param name="heroid"></param>
    /// <returns></returns>
    public async UniTask<GameObject> LoadMonsterModelByHeroID(GameObject parent, long monsterid)
    {
        //加载英雄ab包
        GameObject heroPrefab = await ResourcesManager.Instance.LoadPrefabSync("role", "role_" + monsterid.ToString(), parent.transform);
        return heroPrefab;
    }
}

//关卡数据
public class EditorMissionData
{
    public string missionName;
    public List<EditorPathData> pathDatas;
    public List<RolePointData> rolePointList;//可放置角色格子列表
    public List<EditorTrapData> trapList;//机关列表
}

//路线数据
public class EditorPathData
{
    public int pathIndex;//路线序号
    public List<EditorBranchData> branchDatas;
    public List<MonsterData> monsterDatas;
    //放怪物的数据
}

//分支数据
public class EditorBranchData
{
    public int branchIndex;//分支序号
    public List<EditorPointData> pointDatas;
}

//点位数据
public class EditorPointData
{
    public int point;//点位
    public V3 pos;
    public V2 index;
    public int inputCount;
}

//机关数据
public class EditorTrapData
{
    public List<EditorTrapPointData> points;//点位
    public long trapId;//机关id
    public EditorTrapData Clone()
    {
        EditorTrapData editorTrapData = new EditorTrapData();
        editorTrapData.points = new List<EditorTrapPointData>();
        foreach (var p in points)
            editorTrapData.points.Add(p.Clone());
        editorTrapData.trapId = trapId;
        return editorTrapData;
    }
}

public class EditorTrapPointData
{
    public V2 v2;
    public V3 pos;

    public EditorTrapPointData Clone()
    {
        EditorTrapPointData e = new EditorTrapPointData();
        e.v2 = new V2();
        e.pos = new V3();
        e.v2.x = v2.x; e.v2.y = v2.y;
        e.pos.x = pos.x; e.pos.y = pos.y; e.pos.z = pos.z;
        return e;
    }
}

public class RolePointData
{
    public V2 index;
    public V3 pos;
}

public class TrapPointData
{
    public V2 index;
    public V3 pos;
    public long trapId;
}

public class MonsterData
{
    public int waveIndex;
    public List<waveData> waveDatas;
}

public class waveData
{
    public string RoadID;
    public string MonsterID;
    public string spawn;
    public string Start;
    public string End;
    public string Interval;
    public string Count;
    public string Event;
}





