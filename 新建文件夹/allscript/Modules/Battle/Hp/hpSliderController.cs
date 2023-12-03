using System.Collections;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1.X509;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//血条跟随3d对象组件
public class hpSliderController : MonoBehaviour
{
    public GameObject targetObj;//跟随目标
    public Vector3 targetPos;
    public RectTransform rect;
    public Camera lookCamera;
    public Canvas canvas;

    private bool bEnable;

    private void Awake()
    {
        
    }

    public void OnInit()
    {
        rect = this.gameObject.GetComponent<RectTransform>();
        lookCamera = GameCenter.mIns.m_BattleMgr.hpCamera;
        canvas = GameCenter.mIns.m_BattleMgr.hpCanvas;

        bEnable = true;
        SetHpSliderValue(1);

        targetPos = WorldPointToUILocalPoint(targetObj.transform.position);
        rect.localPosition = targetPos;
    }

    private void FixedUpdate()
    {
        if (targetObj != null && lookCamera != null && bEnable)
        {
            targetPos = WorldPointToUILocalPoint(targetObj.transform.position);
            rect.localPosition = targetPos;
        }
    }

    /// <summary>
    /// 设置slider的value值
    /// </summary>
    /// <param name="value"></param>
    public void SetHpSliderValue(float value)
    {
        Image hp = this.transform.Find("hp").GetComponent<Image>();
        Image hpeff = this.transform.Find("hpEff").GetComponent<Image>();
        hp.DOFillAmount(value, 0.3f).SetEase(Ease.Linear).OnComplete(() => {
            hpeff.DOFillAmount(value, 0.1f).SetEase(Ease.Linear);
        });
    }

    /// <summary>
    /// 设置sldier的显隐
    /// </summary>
    /// <param name="isActive"></param>
    public void SetHpSliderActive(bool isActive)
    {
        this.gameObject.SetActive(isActive);
    }

    /// <summary>
    /// 世界坐标转换为ui坐标
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private Vector3 WorldPointToUILocalPoint(Vector3 point)
    {
        Vector3 screenPoint = GameCenter.mIns.m_BattleMgr.battleCamer.WorldToScreenPoint(point);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, screenPoint, lookCamera, out localPoint);
        return localPoint;
    }

    /// <summary>
    /// 设置血条的激活状态
    /// </summary>
    private void SetHpSliderEnable(bool isEnabale)
    {
        bEnable = isEnabale;
    }
}
