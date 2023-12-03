using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class missioneditorTrap
{
    public missioneditor missioneditor;

    private GameObject m_TrapPanel;
    private Button m_BtnAdd;
    private Button m_BtnSave;
    private Button m_BtnExit;
    private GameObject m_TrapList;
    private GameObject m_Content;
    private GameObject m_TrapItem;
    private List<MissionEditorTrapItem> m_TrapItems = new List<MissionEditorTrapItem>();

    private bool m_IsSetGriding = false;
    private bool m_Looking = false;
    private MissionEditorTrapItem m_CurrSelected;
    private MissionEditorTrapItem m_CurrLooking;
    private Action m_ClearCallback;
    private GameObject[,] m_InputList;
    private EditorMissionData m_MissionData;

    public void SetEditorMissionData(EditorMissionData missionData)
    {
        m_MissionData = missionData;
        m_ClearCallback?.Invoke();
        foreach (var item in m_TrapItems)
        {
            item.Destroy();
        }
        m_TrapItems.Clear();

        if (m_MissionData != null && m_MissionData.trapList != null)
        {
            foreach (var tl in m_MissionData.trapList)
            {
                OnAddTrapItem(tl.trapId, tl.points);
            }
        }
    }

    public void SetInputList(GameObject[,] inputList)
    {
        m_InputList = inputList;
    }

    public missioneditorTrap(missioneditor missioneditor, Action clearCallback, GameObject[,] inputList)
    {
        this.missioneditor = missioneditor;
        m_ClearCallback = clearCallback;
        m_InputList = inputList;

        m_TrapPanel = missioneditor.trapPanel;
        m_BtnAdd = m_TrapPanel.transform.Find("btnAdd").GetComponent<Button>();
        m_BtnSave = m_TrapPanel.transform.Find("btnSave").GetComponent<Button>();
        m_BtnExit = m_TrapPanel.transform.Find("btnExit").GetComponent<Button>();
        m_TrapList = m_TrapPanel.transform.Find("trapList").gameObject;
        m_Content = m_TrapList.transform.Find("content").gameObject;
        m_TrapItem = m_Content.transform.FindHideInChild("trapItem").gameObject;

        m_BtnAdd.AddListenerBeforeClear(() =>
        {
            OnAddTrapItem();
        });

        m_BtnSave.AddListenerBeforeClear(() =>
        {
            missioneditor.SaveTrapCfg(m_TrapItems);
        });

        m_BtnExit.AddListenerBeforeClear(() =>
        {
            m_ClearCallback();
            m_TrapPanel.gameObject.SetActive(false);
        });
    }

    public void ClickMapGrid(double x, double y, V3 v3)
    {
        // Debug.LogError("~ ClickMapGrid x:" + x + " -y:" + y + " - m_IsSetGriding:" + m_IsSetGriding);
        if (m_CurrSelected == null)
        {
            GameCenter.mIns.m_UIMgr.PopMsg("请先选择需要配置的机关对象!");
            return;
        }
        if (m_IsSetGriding)
        {
            bool isRemove = m_CurrSelected.SetIndex(x, y, v3);
            // Debug.LogError("~ isRemove :" + isRemove);
            if (isRemove)
            {
                missioneditor.Target.GetComponent<MeshRenderer>().material = missioneditor.NormalinputMat;
            }
            else
            {
                missioneditor.Target.GetComponent<MeshRenderer>().material = missioneditor.SelectinputMat;
            }
        }
    }

    private void OnAddTrapItem(long trapId = 0, List<EditorTrapPointData> etpds = null)
    {
        GameObject trapObj = GameObject.Instantiate(m_TrapItem);
        trapObj.transform.SetParent(m_TrapItem.transform.parent);
        trapObj.transform.localPosition = Vector3.zero;
        trapObj.transform.localScale = Vector3.one;
        long id = new Snowflake().GetId();
        trapObj.name = "trap_" + id;
        MissionEditorTrapItem missionEditorTrapItem = new MissionEditorTrapItem(id, trapObj,
            (item, state) =>
            {
                if (m_Looking)
                {
                    m_ClearCallback?.Invoke();
                    m_CurrLooking = null;
                    m_Looking = false;
                }


                // 1: 修改  0: 保存
                if (state == 1)
                {
                    if (m_IsSetGriding)
                    {
                        GameCenter.mIns.m_UIMgr.PopMsg("有机关的点位设置没有保存！请先保存这个没保存的机关点位设置!");
                        return;
                    }
                    // 修改该对象的点位
                    m_IsSetGriding = true;
                    m_CurrSelected = item;
                    // 是否已经有数据
                    if (item.indexList != null && item.indexList.Count > 0)
                    {
                        for (int i = 0; i < m_InputList.GetLength(0); i++)
                        {
                            for (int j = 0; j < m_InputList.GetLength(1); j++)
                            {
                                GameObject gridObj = m_InputList[i, j];
                                // Debug.LogError("gridObj:" + gridObj);
                                if (gridObj != null)
                                {
                                    string[] nameTab = gridObj.name.Split('_');
                                    double x = Convert.ToDouble(nameTab[1]);
                                    double y = Convert.ToDouble(nameTab[2]);
                                    foreach (var index in item.indexList)
                                    {
                                        if (x == index.v2.x && y == index.v2.y)
                                        {
                                            gridObj.GetComponent<MeshRenderer>().material = missioneditor.SelectinputMat;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // 保存该对象的点位
                    m_IsSetGriding = false;
                    m_CurrSelected = null;
                }
            },
            (item) =>
            {
                for (int i = m_TrapItems.Count - 1; i >= 0; i--)
                {
                    if (m_TrapItems[i].uid == item.uid)
                    {
                        if (m_CurrSelected != null && m_CurrSelected.uid == item.uid)
                        {
                            m_CurrSelected = null;
                            m_ClearCallback?.Invoke();
                        }
                        if (m_CurrLooking != null && m_CurrLooking.uid == item.uid)
                        {
                            m_CurrLooking = null;
                            m_ClearCallback?.Invoke();
                        }
                        m_TrapItems[i].Destroy();
                        m_TrapItems.RemoveAt(i);
                        break;
                    }
                }
            },
            (item) =>
            {
                if (m_InputList == null || m_InputList.GetLength(0) <= 0)
                {
                    GameCenter.mIns.m_UIMgr.PopMsg("格子数据为初始化，无法查看!");
                    return;
                }

                if (m_IsSetGriding)
                {
                    GameCenter.mIns.m_UIMgr.PopMsg("正在设置中，无法查看!");
                    return;
                }
                if (m_Looking)
                {
                    m_ClearCallback?.Invoke();
                }

                m_Looking = true;
                m_CurrLooking = item;
                item.btnLook.SetText("查看中");
                m_ClearCallback?.Invoke();
                // Debug.LogError("~~ m_InputList:" + m_InputList);
                for (int i = 0; i < m_InputList.GetLength(0); i++)
                {
                    for (int j = 0; j < m_InputList.GetLength(1); j++)
                    {
                        GameObject gridObj = m_InputList[i, j];
                        // Debug.LogError("gridObj:" + gridObj);
                        if (gridObj != null)
                        {
                            string[] nameTab = gridObj.name.Split('_');
                            double x = Convert.ToDouble(nameTab[1]);
                            double y = Convert.ToDouble(nameTab[2]);
                            foreach (var index in item.indexList)
                            {
                                if (x == index.v2.x && y == index.v2.y)
                                {
                                    gridObj.GetComponent<MeshRenderer>().material = missioneditor.SelectinputMat;
                                    break;
                                }
                            }
                        }
                    }
                }
            });

        if (trapId != 0)
        {
            missionEditorTrapItem.SetTrapId(trapId);
        }

        if (etpds != null && etpds.Count > 0)
        {
            missionEditorTrapItem.SetV2List(etpds);
        }

        m_TrapItems.Add(missionEditorTrapItem);
    }
}

public class MissionEditorTrapItem
{
    public long uid;
    public double x;
    public double y;
    public string indexs;
    public long trapId;

    public List<EditorTrapPointData> indexList = new List<EditorTrapPointData>();

    public GameObject obj;
    private TMP_InputField inputID;
    private TMP_Text txIndexs;

    private Button btnSet;
    public Button btnLook;
    private Button btnDelete;

    private bool setting = false;

    public MissionEditorTrapItem(long uid, GameObject obj, Action<MissionEditorTrapItem, int> setCallback, Action<MissionEditorTrapItem> delCallback, Action<MissionEditorTrapItem> lookCallback)
    {
        this.uid = uid;
        this.obj = obj;

        inputID = obj.transform.FindHideInChild("inputID").GetComponent<TMP_InputField>();
        txIndexs = obj.transform.FindHideInChild("txIndexs").GetComponent<TMP_Text>();
        btnSet = obj.transform.FindHideInChild("btnSet").GetComponent<Button>();
        btnLook = obj.transform.FindHideInChild("btnLook").GetComponent<Button>();
        btnDelete = obj.transform.FindHideInChild("btnDelete").GetComponent<Button>();

        btnSet.AddListenerBeforeClear(() =>
        {
            if (!setting)
            {
                btnSet.SetText("保存点位");
                // 1: 修改
                setCallback?.Invoke(this, 1);
            }
            else
            {
                btnSet.SetText("修改点位");
                // 0: 保存
                setCallback?.Invoke(this, 0);
            }
            setting = !setting;
        });

        btnLook.AddListenerBeforeClear(() =>
        {
            lookCallback?.Invoke(this);
        });

        btnDelete.AddListenerBeforeClear(() =>
        {
            delCallback?.Invoke(this);
        });

        txIndexs.text = "";
        obj.gameObject.SetActive(true);
    }

    public long GetTrapId()
    {
        return long.Parse(inputID.text.ToString());
    }
    public void SetTrapId(long trapId)
    {
        inputID.text = trapId.ToString();
    }

    public void SetV2List(List<EditorTrapPointData> etpds)
    {
        //foreach (EditorTrapPointData item in etpds)
        //{
        //    Debug.LogError("~~item:" + item.v2.x + " - " + item.v2.y);
        //}
        indexList = etpds;
        SetIndexsText();
    }

    public bool SetIndex(double x, double y, V3 pos)
    {
        for (int i = indexList.Count - 1; i >= 0; i--)
        {
            if (indexList[i].v2.x == x && indexList[i].v2.y == y)
            {
                indexList.RemoveAt(i);
                SetIndexsText();
                return true;
            }
        }
        V2 v2 = new V2();
        v2.x = x;
        v2.y = y;
        EditorTrapPointData editorTrapPointData = new EditorTrapPointData();
        editorTrapPointData.v2 = v2;
        editorTrapPointData.pos = pos;
        indexList.Add(editorTrapPointData);
        SetIndexsText();
        return false;
    }

    public void SetIndexsText()
    {
        int counter = 0;
        string str = "";
        foreach (var v2 in indexList)
        {
            string s = $"{v2.v2.x},{v2.v2.y}";
            str += counter == indexList.Count - 1 ? s.ToString() : s.ToString() + "|";
        }
        txIndexs.text = str;
    }

    public void Destroy()
    {
        if (obj)
        {
            GameObject.Destroy(obj);
        }
    }
}
