using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class mapeditorArewall
{
    private mapeditor mapeditor;
    private Ray ray;
    private RaycastHit hit;
    private Camera mapCamera;
    private MyBtn[] btns;
    private List<MyLine> lines = new List<MyLine>();
    private Vector3[] currLine = new Vector3[2];

    private Transform btnRoot;
    private LineRenderer lineRenderer;

    private int state = -1;
    private bool isLining = false;
    private float gridLine = 1.6f;
    public mapeditorArewall(mapeditor mapedito, Camera mapCamera)
    {
        this.mapeditor = mapedito;
        this.mapCamera = mapCamera;
        lineRenderer = mapeditor.airwall.transform.GetChild(1).GetOrAddCompoonet<LineRenderer>();
        lineRenderer.gameObject.SetActive(false);
        InitButtons();
    }

    public void InitButtons()
    {
        mapeditor.airwall.transform.GetChild(2).GetComponent<Button>().AddListenerBeforeClear(() =>
        {
            RemoveLine();
        });

        mapeditor.airwall.transform.GetChild(3).GetComponent<Button>().AddListenerBeforeClear(() =>
        {
            ClearLine();
        });

        btnRoot = mapeditor.airwall.transform.GetChild(0);
        // 0~3: 左下，左上，右上，右下
        btns = new MyBtn[btnRoot.childCount];
        for (int i = 0; i < btnRoot.childCount; i++)
        {
            MyBtn myBtn = new MyBtn(btnRoot.GetChild(i).GetComponent<Button>(), i, (index) =>
            {
                btnRoot.gameObject.SetActive(false);
                if (state == -1)
                {
                    // 选中了起点
                    state = 0;
                    isLining = true;
                    SetLineRenderer(tempGrid4p[index]);
                }
                else if (state == 0)
                {
                    // 选中了终点
                    state = -1;
                    isLining = false;
                    lineRenderer.gameObject.SetActive(false);
                    if (index == 0)
                        currLine[1] = new Vector3(currLine[1].x - gridLine / 2, currLine[1].y, currLine[1].z - gridLine / 2);
                    else if (index == 1)
                        currLine[1] = new Vector3(currLine[1].x - gridLine / 2, currLine[1].y, currLine[1].z + gridLine / 2);
                    else if (index == 2)
                        currLine[1] = new Vector3(currLine[1].x + gridLine / 2, currLine[1].y, currLine[1].z + gridLine / 2);
                    else if (index == 3)
                        currLine[1] = new Vector3(currLine[1].x + gridLine / 2, currLine[1].y, currLine[1].z - gridLine / 2);

                    CreateAirWallObs(currLine[0], currLine[1]);
                }
            });
            btns[i] = myBtn;
        }
    }

    Vector3[] tempGrid4p = new Vector3[4];
    public void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ray = mapCamera.ScreenPointToRay(Input.mousePosition);
            //Debug.LogError("ray:" + ray);
            if (Physics.Raycast(ray, out hit))
            {
                //Debug.LogError("hit.collider.name:" + hit.collider.name);
                if (hit.collider.gameObject.tag == "inputmap")
                {
                    btnRoot.gameObject.SetActive(true);
                    tempGrid4p[0] = new Vector3(hit.collider.transform.position.x - gridLine / 2, hit.collider.transform.position.y, hit.collider.transform.position.z - gridLine / 2);
                    tempGrid4p[1] = new Vector3(hit.collider.transform.position.x - gridLine / 2, hit.collider.transform.position.y, hit.collider.transform.position.z + gridLine / 2);
                    tempGrid4p[2] = new Vector3(hit.collider.transform.position.x + gridLine / 2, hit.collider.transform.position.y, hit.collider.transform.position.z + gridLine / 2);
                    tempGrid4p[3] = new Vector3(hit.collider.transform.position.x + gridLine / 2, hit.collider.transform.position.y, hit.collider.transform.position.z - gridLine / 2);
                    foreach (var btn in btns)
                    {
                        if (state == 0)
                        {
                            btn.SetText(1);
                            isLining = false;
                        }
                        else
                        {
                            btn.SetText(0);
                        }
                    }
                }
            }
        }

        if (isLining)
        {
            ray = mapCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                // Debug.LogError("ray:" + ray);
                if (hit.collider.gameObject.tag == "inputmap")
                {
                    currLine[1] = hit.collider.transform.position;
                }
            }
            SetLineRenderer(currLine[0], currLine[1]);
        }
    }

    Vector3[] tempvec3 = new Vector3[2];
    private void SetLineRenderer(Vector3 s, Vector3 e = default)
    {
        tempvec3[0] = s;
        currLine[0] = s;
        currLine[1] = e;
        if (e != default)
        {
            tempvec3[1] = e;
        }
        else
        {
            // new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z)
            tempvec3[1] = mapCamera.ScreenToWorldPoint(Input.mousePosition);
        }
        lineRenderer.SetPositions(tempvec3);
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.gameObject.SetActive(true);
    }

    private void CreateAirWallObs(Vector3 s, Vector3 e)
    {
        float dis = Vector3.Distance(s, e);
        GameObject maplist = GameObject.Find("mapeditorsence(Clone)/MapList").gameObject;
        Transform obsRoot = maplist.transform.Find("airwall");
        if (obsRoot == null)
        {
            GameObject obsObj = new GameObject("airwall");
            obsObj.transform.SetParent(maplist.transform);
            obsObj.transform.localPosition = Vector3.zero;
            obsObj.transform.localScale = Vector3.one;
            obsObj.transform.localRotation = Quaternion.identity;
            obsObj.transform.tag = "obstacle";
            obsRoot = obsObj.transform;
            obsObj.SetActive(true);
        }
        Debug.Log("~~ s:" + s + " - e:" + e + " -dot:" + Vector3.Dot(s, e) + " -cross:" + Vector3.Cross(s, e) + " - ez-sz:" + (e.z - s.z));
        GameObject airwall = new GameObject("airwall(clone)");
        airwall.transform.SetParent(obsRoot);
        airwall.transform.position = s;
        airwall.transform.localScale = Vector3.one;

        if (Mathf.Abs(s.z - e.z) <= 0.1f)
        {
            // 水平 e.x > s.x => 右 | e.x < s.x => 左 
            airwall.transform.localRotation = e.x > s.x  ? Quaternion.identity : Quaternion.Euler(0, -180, 0);
        }
        else if (Mathf.Abs(s.x - e.x) <= 0.1f)
        {
            // 水平 e.z > s.z => 上 | e.z < s.z => 下 
            airwall.transform.localRotation = e.z > s.z ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);
        }
        airwall.transform.tag = "obstacle";
        BoxCollider airwallBC = airwall.AddComponent<BoxCollider>();
        airwallBC.size = new Vector3(dis / 0.16f, 50, 0.1f);
        airwallBC.center = new Vector3(dis / 2 / 0.16f, 25, 0.0f);
        airwall.SetActive(true);
        obsRoot.gameObject.SetActive(false);
        AddLine(currLine[0], currLine[1], airwall);
    }

    private void AddLine(Vector3 s, Vector3 e, GameObject wall)
    {
        GameObject lineClone = GameObject.Instantiate(lineRenderer.gameObject).gameObject;
        lineClone.transform.parent = lineRenderer.transform.parent;
        lineClone.transform.position = Vector3.zero;
        lineClone.transform.localScale = Vector3.one;
        MyLine myLine = new MyLine(s, e, lineClone.GetComponent<LineRenderer>(), wall);
        lines.Add(myLine);
    }

    private void RemoveLine()
    {
        if (lines.Count > 0)
        {
            MyLine myLine = lines[lines.Count - 1];
            GameObject.Destroy(myLine.lineRenderer.gameObject);
            GameObject.Destroy(myLine.wall.gameObject);
            lines.RemoveAt(lines.Count - 1);
        }
    }

    private void ClearLine()
    {
        foreach (MyLine myLine in lines)
        {
            GameObject.Destroy(myLine.lineRenderer.gameObject);
            GameObject.Destroy(myLine.wall.gameObject);
        }
        lines.Clear();
    }

    private class MyLine
    {
        public Vector3 s; 
        public Vector3 e;
        public LineRenderer lineRenderer;
        public GameObject wall;
        public MyLine(Vector3 s, Vector3 e, LineRenderer lineRenderer, GameObject wall)
        {
            this.s = s;
            this.e = e;
            this.lineRenderer = lineRenderer;
            this.lineRenderer.SetPositions(new Vector3[2] { s, e });
            this.lineRenderer.startWidth = 0.1f;
            this.lineRenderer.endWidth = 0.1f;
            this.lineRenderer.gameObject.SetActive(true);
            this.wall = wall;
        }
    }

    private class MyBtn
    {
        public Button button;
        public TMP_Text text;

        public int index;
        public Action<int> callback;
        public MyBtn(Button button, int index, Action<int> callback)
        {
            this.button = button;
            text = button.GetComponentInChildren<TMP_Text>();
            this.index = index;
            this.callback = callback;
            button.AddListenerBeforeClear(() =>
            {
                callback(index);
            });
            SetText(0);
        }

        public void SetText(int sore)
        {
            switch (index)
            {
                case 0:
                    text.text = sore == 0 ? "起点(左下)" : "终点(左下)";
                    break;
                case 1:
                    text.text = sore == 0 ? "起点(左上)" : "终点(左上))";
                    break;
                case 2:
                    text.text = sore == 0 ? "起点(右上)" : "终点(右上)";
                    break;
                case 3:
                    text.text = sore == 0 ? "起点(右下)" : "终点(右下)";
                    break;
            }
        }
    }
}
