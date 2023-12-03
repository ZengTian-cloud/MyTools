using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyLiuGuangHelper : MonoBehaviour
{
    private RectTransform rt;
    private float b = -498f;
    private float t = 250f;
    private float s = 748f;
    private bool show = false;

    private Vector2 tempVec2 = Vector2.zero;
    void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        tempVec2.x = 0; 
        tempVec2.y = b;
        rt.anchoredPosition = tempVec2;
        show = true;
    }

    void Update()
    {
        if (!show) return;
        tempVec2.y += s * Time.deltaTime;
        if (tempVec2.y >= t)
            tempVec2.y = b;
        rt.anchoredPosition = tempVec2;
    }
}
