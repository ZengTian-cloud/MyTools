using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 关卡连线
/// </summary>
public class missionLine
{
    public GameObject root;

	public GameObject lineObj;
    public GameObject sp;
    public GameObject ep;
    public long mission;

    public missionLine(GameObject obj,Vector3 start,Vector3 end,Transform parent,long mission)
    {
        this.root = obj;
        this.lineObj = this.root.transform.Find("line").gameObject;
        this.sp = this.root.transform.Find("sp").gameObject;
        this.ep = this.root.transform.Find("ep").gameObject;
        this.mission = mission;

        this.root.transform.SetParent(parent);
        this.root.transform.localScale = Vector3.one;
        this.root.transform.localPosition = Vector3.zero;
        this.root.transform.localRotation = Quaternion.Euler(Vector3.zero);

        Refresh(start, end);
        this.root.SetActive(true);

    }

    public void Refresh(Vector3 start, Vector3 end)
    {
        this.sp.GetComponent<RectTransform>().localPosition = start;
        this.ep.GetComponent<RectTransform>().localPosition = end;
        DrawLine();
    }


    private void DrawLine()
    {
        Vector3 pointAPosition = sp.GetComponent<RectTransform>().localPosition;
        Vector3 pointBPosition = ep.GetComponent<RectTransform>().localPosition;
        Vector3 centerPosition = (pointAPosition + pointBPosition) / 2f;

        
        RectTransform connectionLine = lineObj.GetComponent<RectTransform>();

        //计算长度
        float distance = Vector3.Distance(pointAPosition, pointBPosition);
        connectionLine.sizeDelta = new Vector2(distance - 5, connectionLine.sizeDelta.y);

        //计算角度
        Vector3 direction = (pointAPosition - pointBPosition).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        connectionLine.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);

        connectionLine.localPosition = centerPosition;
    }

}

