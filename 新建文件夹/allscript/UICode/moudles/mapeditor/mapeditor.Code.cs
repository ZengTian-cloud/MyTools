using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using Frame.Util;
using UnityEngine.UI;
using System;

public partial class mapeditor
{
    public override UILayerType uiLayerType => UILayerType.Normal;
    public override string uiAtlasName => "";
    private string mapID;
    private string mapName;

    //地图编辑器数据
    private MapEditorData mapEditorData;

    private string savePath = Application.dataPath  +"/allcfg/mapcfgs";
    private const int xvalue = 16;//x轴长度
    private const int zvalue = 11;//z轴长度

    private GameObject mapScence;//地图场景
    private Transform mapListRoot;//地块列表
    private Transform inputListRoot;//地块列表
    private GameObject temp;
    private GameObject input;

    private BoxData[,] mapList;//地块列表
    private GameObject[,] inputList;//可点击地块

    private Material normalMat;//默认地图材质 不可通行
    private Material normalMatCan;//默认地图材质 可通行

    private Material normalinputMat;//可点击地块材质  默认
    private Material selectinputMat;//可点击地块材质  选中

    private List<GameObject> selectBoxList;//选中的地块的下标集合

    private bool isShowInput;
    private Camera mapCamera;

    private mapeditorArewall mapeditorArewall;

    private List<MatCfg> matCfgs = new List<MatCfg> {
        new MatCfg{ matName = "map1001_0_ground02_00000000",state = 0},
        new MatCfg{ matName = "map1001_0_ground02_00000010",state = 0},
        new MatCfg{ matName = "map1001_0_ground02_00001010",state = 0},
        new MatCfg{ matName = "map1001_0_ground02_00100010",state = 0},
        new MatCfg{ matName = "map1001_0_ground02_00101010",state = 0},
        new MatCfg{ matName = "map1001_0_ground02_10000011",state = 0},
        new MatCfg{ matName = "map1001_0_ground02_10001011",state = 0},
        new MatCfg{ matName = "map1001_0_ground02_10001111",state = 0},
        new MatCfg{ matName = "map1001_0_ground02_10100011",state = 0},
        new MatCfg{ matName = "map1001_0_ground02_10101010",state = 0},
        new MatCfg{ matName = "map1001_0_ground02_10101011",state = 0},
        new MatCfg{ matName = "map1001_0_ground02_10101111",state = 0},
        new MatCfg{ matName = "map1001_0_ground02_10111011",state = 0},
        new MatCfg{ matName = "map1001_0_ground02_10111111",state = 0},
        new MatCfg{ matName = "map1001_0_ground02_11111111",state = 0},
        new MatCfg{ matName = "map1001_1_ground01_00000000",state = 1},
        new MatCfg{ matName = "map1001_1_ground01_00000010",state = 1},
        new MatCfg{ matName = "map1001_1_ground01_00001010",state = 1},
        new MatCfg{ matName = "map1001_1_ground01_00100010",state = 1},
        new MatCfg{ matName = "map1001_1_ground01_00101010",state = 1},
        new MatCfg{ matName = "map1001_1_ground01_10000011",state = 1},
        new MatCfg{ matName = "map1001_1_ground01_10001011",state = 1},
        new MatCfg{ matName = "map1001_1_ground01_10001111",state = 1},
        new MatCfg{ matName = "map1001_1_ground01_10100011",state = 1},
        new MatCfg{ matName = "map1001_1_ground01_10101010",state = 1},
        new MatCfg{ matName = "map1001_1_ground01_10101111",state = 1},
        new MatCfg{ matName = "map1001_1_ground01_10111011",state = 1},
        new MatCfg{ matName = "map1001_1_ground01_10111111",state = 1},
        new MatCfg{ matName = "map1001_1_ground01_11111111",state = 1},
    };

    protected override async void OnInit()
    {
        isShowInput = false;
        mapList = new BoxData[xvalue, zvalue];
        inputList = new GameObject[xvalue, zvalue];
        selectBoxList = new List<GameObject>();


        normalMat = await ResourcesManager.Instance.LoadAssetSyncByName<Material>("map1001_0_ground02_00000000.mat");
        normalMatCan = await ResourcesManager.Instance.LoadAssetSyncByName<Material>("map1001_1_ground01_00000000.mat");
        normalinputMat = await ResourcesManager.Instance.LoadAssetSync("materials/normalMat.mat") as Material;
        selectinputMat = await ResourcesManager.Instance.LoadAssetSync("materials/selectMat.mat") as Material;

        CreatMapEditorScence();
        CreatMap();
        InitMatList();

        mapeditorArewall = new mapeditorArewall(this, mapCamera);
    }

    protected override void OnOpen()
    {
        //注册update
        GameCenter.mIns.m_UIMgr.AddUpdateWin(this);
        mapScence.SetActive(true);
        //CreatMapEditorScence();
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

    private void CreatMap()
    {
        for (int z = 0; z < zvalue; z++)
        {
            for (int x = 0; x < xvalue; x++)
            {
                if (z > 0 && z < zvalue -1 && x > 0 && x < xvalue - 1)
                {
                    GameObject input = GameObject.Instantiate(this.input, inputListRoot);
                    input.transform.localPosition = new Vector3(x * 10, 2, z * 10);
                    input.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    input.tag = "inputmap";
                    input.GetComponent<MeshRenderer>().material = normalinputMat;
                    input.SetActive(true);
                    input.name = $"input_{x}_{z}";
                    inputList[x, z] = input;
                }
                GameObject map = GameObject.Instantiate(temp, mapListRoot);
                map.transform.localPosition = new Vector3(x * 10, 0, z * 10);
                map.transform.localScale = Vector3.one;
                map.GetComponentInChildren<MeshRenderer>().material = normalMat;
                map.SetActive(true);
                map.name = $"map_{x}_{z}";
                mapList[x, z] = new BoxData {
                    state = 0,
                    mat = "map1001_0_ground02_00000000",
                    index = new int[] { x, z },
                    worldPos = map.transform.position,
                    rot = map.transform.rotation.eulerAngles,
                    prefab = map
                };
            }
        }
    }


    private Ray ray;
    private RaycastHit hit;
    private string[] nameTab;
    private int selectX;
    private int selectZ;
    private BoxData selceBoxData;
    public override void UpdateWin()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ray = mapCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray,out hit))
            {
                if (hit.collider.gameObject.tag == "inputmap")
                {
                    nameTab = hit.collider.gameObject.name.Split('_');
                    selectX = int.Parse(nameTab[1]);
                    selectZ = int.Parse(nameTab[2]);
                    selceBoxData = mapList[selectX, selectZ];
                    if (selceBoxData.state == 0)
                    {
                        inputList[selectX, selectZ].GetComponentInChildren<MeshRenderer>().material = selectinputMat;
                        selceBoxData.state = 1;
                        selectBoxList.Add(selceBoxData.prefab);
                    }
                    else
                    {
                        inputList[selectX, selectZ].GetComponentInChildren<MeshRenderer>().material = normalinputMat;
                        selceBoxData.state = 0;
                        selectBoxList.Remove(selceBoxData.prefab);
                    }

                }
            }
        }

        if (mapeditorArewall != null)
        {
            mapeditorArewall.Update();
        }
    }


    //保存配置为txt文本
    private void SaveCfg()
    {
        mapEditorData = new MapEditorData();
        mapEditorData.cfgName = $"map{mapID}";
        mapEditorData.boxList = mapList;
        string content = GetContent();
        FileOperate.FileWrite($"{savePath}/{mapEditorData.cfgName}.txt", content);
    }

    //数据转文本
    private string GetContent()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("{");
        builder.AppendLine($"{GetTab(1)}\"cfgname\":\"{mapEditorData.cfgName}\",");
        builder.AppendLine($"{GetTab(1)}\"maplist\":[");
        for (int x = 0; x <xvalue; x++)
        {
            for (int z = 0; z < zvalue; z++)
            {
                BoxData oneData = mapEditorData.boxList[x,z];
                builder.AppendLine(GetTab(2));
                builder.Append($"{GetTab(3)}{{ \"state\":{oneData.state},");
                builder.Append($"\"mat\":\"{oneData.mat}\",");
                builder.Append($"\"index\" : {{ \"x\":{oneData.index[0]},\"y\":{oneData.index[1]} }},");
                builder.Append($"\"pos\" : {{ \"x\":{oneData.worldPos.x},\"y\":{oneData.worldPos.y},\"z\":{oneData.worldPos.z} }},");
                builder.Append($"\"rot\" : {{ \"x\":{oneData.rot.x},\"y\":{oneData.rot.y},\"z\":{oneData.rot.z} }}}}");
                if (x != xvalue - 1 || z != zvalue - 1)
                {
                    builder.Append(",");
                }

            }     
        }
        builder.AppendLine($"{GetTab(1)}]");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string GetTab(int num)
    {
        string s = "";
        for (int i = 0; i < num; i++)
            s += "\t";
        return s;
    }

    //刷新地图列表的格子状态
    private void RefreshMapListState(GameObject obj,int state)
    {
        for (int z = 0; z < zvalue; z++)
        {
            for (int x = 0; x < xvalue; x++)
            {
                if (mapList[x,z].prefab == obj)
                {
                    mapList[x, z].state = state;
                }
            }
        }
    }

    //刷新地图列表的格子材质
    private void RefreshMapListMat(GameObject obj, string mat)
    {
        for (int z = 0; z < zvalue; z++)
        {
            for (int x = 0; x < xvalue; x++)
            {
                if (mapList[x, z].prefab == obj)
                {
                    mapList[x, z].mat = mat;
                }
            }
        }
    }

    //刷新地图列表的格子角度
    private void RefreshMapListRotate(GameObject obj,Vector3 v)
    {
        for (int z = 0; z < zvalue; z++)
        {
            for (int x = 0; x < xvalue; x++)
            {
                if (mapList[x, z].prefab == obj)
                {
                    mapList[x, z].rot = v;
                }
            }
        }
    }

    //初始化可点击方块列表
    private void InitInputList()
    {
        for (int z = 0; z < zvalue; z++)
        {
            for (int x = 0; x < xvalue; x++)
            {
                if (inputList[x, z])
                {
                    inputList[x, z].GetComponent<MeshRenderer>().material = normalinputMat;
                }         
            }
        }
    }

    private MatCfg GetMatByID(string id,int state)
    {
        string name = state == 0 ? $"map1001_0_ground02_{id}" : $"map1001_1_ground01_{id}";
        for (int i = 0; i < matCfgs.Count; i++)
        {
            if (matCfgs[i].matName == name && matCfgs[i].state == state)
            {
                return matCfgs[i];
            }
        }
        return null;
    }


    //刷新地图材质球 根据周围八个格子进行计算
    private async void RefreshMapMat()
    {
        BoxData oneBoxData;
        BoxData BoxData1;
        BoxData BoxData2;
        BoxData BoxData3;
        BoxData BoxData4;
        BoxData BoxData5;
        BoxData BoxData6;
        BoxData BoxData7;
        BoxData BoxData8;

        int num1;
        int num2;
        int num3;
        int num4;
        int num5;
        int num6;
        int num7;
        int num8;

        string id1;
        string id2;
        string id3;
        string id4;

        int rotate = 0;
        MatCfg matCfg = new MatCfg();
        for (int x = 0; x < xvalue; x++)
        {
            for (int z = 0; z < zvalue ; z++) 
            {

                oneBoxData = mapList[x, z];
                BoxData1 = (x == 0 || z + 1 > zvalue - 1) ? new BoxData { state = 0 } : mapList[x - 1, z + 1];
                BoxData2 = z + 1 > zvalue - 1 ? new BoxData { state = 0 } : mapList[x, z + 1];
                BoxData3 = (x + 1 > xvalue - 1 || z + 1 > zvalue - 1) ? new BoxData { state = 0 } : mapList[x + 1, z + 1];
                BoxData4 = x + 1 > xvalue - 1 ? new BoxData { state = 0 } : mapList[x + 1, z];
                BoxData5 = (z == 0 || x + 1 > xvalue - 1) ? new BoxData { state = 0 } : mapList[x + 1, z - 1];
                BoxData6 = z == 0 ? new BoxData { state = 0 } : mapList[x, z - 1];
                BoxData7 = (z == 0 || x == 0) ? new BoxData { state = 0 } : mapList[x - 1, z - 1];
                BoxData8 = x == 0 ? new BoxData { state = 0 } : mapList[x - 1, z];

                if (oneBoxData.state == 0)
                {
                    num1 = BoxData1.state == oneBoxData.state ? 0 : 1;
                    num2 = BoxData2.state == oneBoxData.state ? 0 : 1;
                    num3 = BoxData3.state == oneBoxData.state ? 0 : 1;
                    num4 = BoxData4.state == oneBoxData.state ? 0 : 1;
                    num5 = BoxData5.state == oneBoxData.state ? 0 : 1;
                    num6 = BoxData6.state == oneBoxData.state ? 0 : 1;
                    num7 = BoxData7.state == oneBoxData.state ? 0 : 1;
                    num8 = BoxData8.state == oneBoxData.state ? 0 : 1;

                    num1 = Mathf.Max(num1, num2, num8);
                    num3 = Mathf.Max(num2, num3, num4);
                    num5 = Mathf.Max(num4, num5, num6);
                    num7 = Mathf.Max(num6, num7, num8);

                    id1 = $"{num1}{num2}{num3}{num4}{num5}{num6}{num7}{num8}";
                    id2 = $"{num7}{num8}{num1}{num2}{num3}{num4}{num5}{num6}";
                    id3 = $"{num5}{num6}{num7}{num8}{num1}{num2}{num3}{num4}";
                    id4 = $"{num3}{num4}{num5}{num6}{num7}{num8}{num1}{num2}";

                    if (GetMatByID(id1, oneBoxData.state) != null)
                    {
                        rotate = 0;
                        matCfg = GetMatByID(id1, oneBoxData.state);
                    }
                   
                    else if (GetMatByID(id4, oneBoxData.state) != null)
                    {
                        rotate = 90;
                        matCfg = GetMatByID(id4, oneBoxData.state);
                    }
                    else if (GetMatByID(id3, oneBoxData.state) != null)
                    {
                        rotate = 180;
                        matCfg = GetMatByID(id3, oneBoxData.state);
                    }
                    else if (GetMatByID(id2, oneBoxData.state) != null)
                    {
                        rotate = 270;
                        matCfg = GetMatByID(id2, oneBoxData.state);
                    }


                    Material mat = await ResourcesManager.Instance.LoadAssetSyncByName<Material>(matCfg.matName+".mat");
                    mapList[x, z].mat = matCfg.matName;
                    mapList[x, z].rot = new Vector3(0, rotate + 180 , 0);
                    oneBoxData.prefab.transform.localEulerAngles = new Vector3(0, rotate + 180 , 0);
                    oneBoxData.prefab.GetComponentInChildren<MeshRenderer>().material = mat;
                }
                else
                {
                    oneBoxData.prefab.GetComponentInChildren<MeshRenderer>().material = normalMatCan;
                    mapList[x, z].mat = "map1001_1_ground01_00000000";
                }

            }
        }
    }

    //加载配置表
    private async void LoadCfg(string mapID)
    {
        string path = $"{savePath}/map{mapID}.txt";
        string allBytes =  File.ReadAllText(path);
        MapData mapData =  maptool.MapJsonToClassData(allBytes);
        for (int i = 0; i < mapData.maplist.Count; i++)
        {
            CellData cell = mapData.maplist[i];
            BoxData boxData = mapList[(int)cell.index.x, (int)cell.index.y];
            boxData.state = cell.state;
          
            boxData.prefab.GetComponentInChildren<MeshRenderer>().material = await ResourcesManager.Instance.LoadAssetSyncByName<Material>(cell.mat+".mat");
            boxData.mat = cell.mat;
            boxData.prefab.transform.localEulerAngles = new Vector3((float)cell.rot.x, (float)cell.rot.y, (float)cell.rot.z);
            boxData.rot = new Vector3((float)cell.rot.x, (float)cell.rot.y, (float)cell.rot.z);
        }
        Debug.Log($"加载配置表{path}完成");
    }

    //初始化材质列表
    private void InitMatList()
    {
        content.SetDatas(matCfgs.Count, false);
        content.onItemRender = SetItemRender;
    }

    private async void SetItemRender(GameObject item , int index)
    {
        item.GetComponent<RawImage>().texture = await ResourcesManager.Instance.LoadAssetSync(matCfgs[index - 1].matName) as Texture;
        item.GetComponent<Button>().onClick.RemoveAllListeners();
        item.GetComponent<Button>().onClick.AddListener(() => { ChangeSelectBoxMat(index); });
    }

    //替换选中的格子的mat
    private async void ChangeSelectBoxMat(int index)
    {
        for (int i = 0; i < selectBoxList.Count; i++)
        {
            selectBoxList[i].GetComponentInChildren<MeshRenderer>().material = await ResourcesManager.Instance.LoadAssetSync(matCfgs[index - 1].matName) as Material;
            RefreshMapListMat(selectBoxList[i], matCfgs[index - 1].matName);
        }
    }

    //旋转选中的格子
    private void RotateSelectBox()
    {
        for (int i = 0; i < selectBoxList.Count; i++)
        {
            Vector3 v = selectBoxList[i].transform.localEulerAngles;
            selectBoxList[i].transform.localEulerAngles = new Vector3(v.x, v.y + 90, v.z);
            RefreshMapListRotate(selectBoxList[i], new Vector3(v.x, v.y + 90, v.z));
        }
    }

    //保存
    partial void btn_save_Click()
    {
        mapID = input_id.text;
        if (!string.IsNullOrEmpty(mapID))
        {
            SaveCfg();
        }
        else
        {
            Debug.LogError("地图id为空，先输入地图id再进行保存！");
        }
    }
    //返回
    partial void btn_back_Click()
    {
        this.Close();
        mapScence.SetActive(false);
        //GameCenter.mIns.m_CamMgr.RemoveCameraInMainCamera(mapCamera);
        //GameObject.Destroy(mapScence);
    }

    //手动替换图片
    partial void btn_change_Click()
    {
        matlist.SetActive(true);
    }

    //导入
    partial void btn_get_Click()
    {
        mapID = input_id.text;
        if (!string.IsNullOrEmpty(mapID))
        {
            LoadCfg(mapID);
        }
        else
        {
            Debug.LogError("地图id为空，先输入地图id再进行导入操作！");
        }
    }

    //关闭选择地块窗口
    partial void btn_close_Click()
    {
        matlist.SetActive(false);
        InitInputList();
        selectBoxList.Clear();
    }

    //设为不可通行
    partial void btn_setcannot_Click()
    {
        for (int i = 0; i < selectBoxList.Count; i++)
        {
            selectBoxList[i].GetComponentInChildren<MeshRenderer>().material = normalMat;
            RefreshMapListState(selectBoxList[i], 0);
            RefreshMapListMat(selectBoxList[i], "map1001_0_ground02_00000000");
        }
        InitInputList();
        selectBoxList.Clear();
        RefreshMapMat();
    }

    //设为可通行
    partial void btn_setcan_Click()
    {
        for (int i = 0; i < selectBoxList.Count; i++)
        {
            selectBoxList[i].GetComponentInChildren<MeshRenderer>().material = normalMatCan;
            RefreshMapListState(selectBoxList[i], 1);
            RefreshMapListMat(selectBoxList[i], "map1001_1_ground01_00000000");
        }
        InitInputList();
        selectBoxList.Clear();
        RefreshMapMat();
    }

    //切换编辑面板
    partial void btn_switch_Click()
    {
        isShowInput = !isShowInput;
        inputListRoot.gameObject.SetActive(isShowInput);
    }

    //初始化
    partial void btn_init_Click()
    {
        for (int x = 0; x < xvalue; x++)
        {
            for (int z = 0; z < zvalue; z++)
            {
                mapList[x, z].prefab.GetComponentInChildren<MeshRenderer>().material = normalMat;
                mapList[x, z].mat = "map1001_0_ground02_00000000";
                mapList[x, z].state = 0;             
            }
        }
        InitInputList();
        selectBoxList.Clear();
    }

    partial void btn_ro_Click()
    {
        RotateSelectBox();
    }

}

//地图编辑器数据
public class MapEditorData
{
    public string cfgName;

    public BoxData[,] boxList;
}

//格子数据
public class BoxData
{
    public int state;//格子状态 0-不可通行 1-可通行
    public string mat;//材质球
    public int[] index;//在所有地图列表的二维数组中的下标
    public Vector3 worldPos;//世界坐标
    public Vector3 rot;//角度
    public GameObject prefab;
}

public class MatCfg
{
    public string matName;//材质名字
    public int state;//1-可通行 0-不可通行
}


