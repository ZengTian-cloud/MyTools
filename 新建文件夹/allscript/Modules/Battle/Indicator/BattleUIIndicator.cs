using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleUIIndicator : MonoBehaviour
{
    private Image m_BgImage;

    private Vector2 m_UguiPos;

    private Image m_IndicatorImage;
    public RectTransform indicatorTran;
    public Camera uiCamera;

    private Vector2 m_FingerPos = Vector2.zero;
    private CanvasScalerEx m_CanvasScalerEx = null;

    private Vector2 m_Resolution = Vector2.zero;
    private Vector2 m_AreaSize;
    private float m_AreaPreX = 0f;
    private float m_AreaPreY = 0f;

    private bool m_Begin = false;
    private void OnEnable()
    {
        m_BgImage = GetComponent<Image>();
        m_IndicatorImage = transform.Find("imgIndicator").GetComponent<Image>();
        indicatorTran = m_IndicatorImage.GetComponent<RectTransform>();
        uiCamera = GameCenter.mIns.m_UIMgr.UICamera;
    }

    private void Update()
    {
        int intCheckCancel = CheckCancel();
        if (intCheckCancel == -1)
        {
            BattleSelectedHelper.Instance.DestroySingleUIIndicator();
        }

        if (!m_Begin) return;

        m_FingerPos = touchcstool.onefingerpos;
        if (m_FingerPos == Vector2.zero) return;

        //RectTransformUtility.ScreenPointToLocalPointInRectangle(indicatorTran, m_FingerPos, uiCamera, out m_UguiPos);
        //indicatorTran.anchoredPosition = indicatorTran.anchoredPosition + m_UguiPos;

        indicatorTran.position = ScreenToUIWorldPos(indicatorTran as RectTransform, Input.mousePosition, uiCamera);
    }

    /// <summary>
    /// 屏幕坐标转换成 UI 坐标
    /// </summary>
    /// <param name="targetParentRect"> 目标 UI 父物体的 RectTransform </param>
    /// <param name="mousePos"> 鼠标位置 </param>
    /// <param name="canvasCam"> 如果Canvas的渲染模式为: Screen Space - Overlay, Camera 设置为 null;
    /// Screen Space-Camera or World Space, Camera 设置为 Camera.main></param>
    /// <returns>UI 的局部坐标</returns>
    private Vector2 ScreenToUILocalPos(RectTransform targetParentRect, Vector2 mousePos, Camera canvasCam = null)
    {
        //UI 的局部坐标
        Vector2 uiLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(targetParentRect, mousePos, canvasCam, out uiLocalPos);
        return uiLocalPos;
    }


    /// <summary>
    /// 屏幕坐标转换成 UI 坐标
    /// </summary>
    /// <param name="targetRect"> 目标 UI 物体的 RectTransform </param>
    /// <param name="mousePos"> 鼠标位置 </param>
    /// <param name="canvasCam"> 如果Canvas的渲染模式为: Screen Space - Overlay, Camera 设置为 null;
    /// Screen Space-Camera or World Space, Camera 设置为 Camera.main></param>
    /// <returns> UI 的坐标 </returns>
    private Vector3 ScreenToUIWorldPos(RectTransform targetRect, Vector2 mousePos, Camera canvasCam = null)
    {
        //UI 的局部坐标
        Vector3 worldPos = default;
        if (targetRect != null)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(targetRect, mousePos, canvasCam, out worldPos);
        }
        return worldPos;
    }


    public void Begin()
    {
        m_CanvasScalerEx = GetComponentInParent<CanvasScalerEx>();
        m_FingerPos = touchcstool.onefingerpos;
        m_Begin = true;
    }

    public void Drag()
    {
    }

    public void End()
    {
        m_Begin = false;
    }

    private int CheckCancel()
    {

#if UNITY_EDITOR
        if (!Input.GetMouseButton(0)) return -1;
#else
        Debug.LogError("Input.touchCount:" + Input.touchCount);
		if (Input.touchCount > 1 || Input.touchCount <= 0) return -1;
#endif
        m_Resolution = new Vector2(2436, 1125);
        if (m_CanvasScalerEx != null)
            m_Resolution = m_CanvasScalerEx.referenceResolution;

        m_AreaPreX = m_AreaSize.x / m_Resolution.x;
        m_AreaPreY = m_AreaSize.y / m_Resolution.y;
        float lx = m_Resolution.x - (m_AreaPreX * m_Resolution.x);
        float ly = m_Resolution.y - (m_AreaPreY * m_Resolution.y);

        if (lx <= m_FingerPos.x && ly <= m_FingerPos.y)
        {
            return 0;
        }
        return 1;
    }
}
