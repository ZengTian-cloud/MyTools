using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 卡牌拖拽回调
/// </summary>
public class CardInputHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    //滑动条
    public ScrollRect rootScroll;

    // 拖拽方向 -1.各个方向 0.左 1.右 2.上 3.下
    public int dradDir = -1;

    //开始拖拽回调
    public Action onBeginCB;

    //拖拽中回调
    public Action onDragCB;

    //拖拽结束回调
    public Action onEndCB;

    //按下回调
    public Action onPointDownCB;

    //抬起回调
    public Action onPointUpCB;

    private const int effectdis = 100;

    //是否锁定了拖拽操作
    private bool islock = false;

    //是否锁定当前obj操作
    private bool isSelfLock = false;

    //当前拖拽是否生效
    private bool iseffect = false;

    //是否处于点击状态
    private bool isInput = false;

    //触摸开始位置
    private Vector2 beginPos;


    public GameObject uiPrefab;

    public GameObject heroPrefab;

    #region hero icon define
    public long heroId;
    public bool isHero = false;
    public bool isHeroIconDragMove = false;
    private Vector2 beginDragRectPos = Vector2.zero;
    private int beginDragSiblingIndex = -1;
    private Transform beginDragParent = null;
    private Camera uiCamera;

    private CardInputHandler adjCardInputHandler;
    #endregion

    private void Awake()
    {
        uiCamera = GameCenter.mIns.m_UIMgr.UICamera;
    }

    private void Update()
    {
    }



    //设置当前对象拖拽是否锁定
    public void SetSelfLock(bool block)
    {
        this.isSelfLock = block;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //有未完成的拖拽
        if (iseffect)
        {
            return;
        }

        beginPos = touchcstool.onefingerpos;

        islock = isSelfLock;

        if (rootScroll != null)
        {
            rootScroll.OnBeginDrag(eventData);
        }

        if (isHero)
        {
            //adjCardInputHandler = GetAdjacentCardInputHandler();
            beginDragSiblingIndex = transform.GetSiblingIndex();
            beginDragParent = transform.parent;
            beginDragRectPos = transform.GetComponent<RectTransform>().anchoredPosition;
            //transform.parent = transform.parent.parent.parent;
            isHeroIconDragMove = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {

        if (islock)
        {
            if (rootScroll != null)
            {
                rootScroll.OnDrag(eventData);
            }
            return;
        }

        if (iseffect)
        {
            onDragCB?.Invoke();
            return;
        }
        else if (isHero)
        {
            HeroCardDrag();
            if (rootScroll != null)
                rootScroll.OnDrag(eventData);
        }
        else
        {
            iseffect = true;
            onBeginCB?.Invoke();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (rootScroll != null)
        {
            rootScroll.OnEndDrag(eventData);
        }
    }

    private void FixedUpdate()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))

#else
        if(Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary) )
#endif
        {

            if (!isInput)
            {
                isInput = true;
            }


        }
        else if (isInput)
        {
            isInput = false;
            DragOver();
        }
    }

    /// <summary>
    /// 拖拽结束
    /// </summary>
    private void DragOver()
    {
        if (iseffect)
        {
            iseffect = false;
            onEndCB?.Invoke();
        }
    }

    private void OnDestroy()
    {
        onBeginCB = null;
        onDragCB = null;
        onEndCB = null;
    }

    //按下
    public void OnPointerDown(PointerEventData eventData)
    {
        onPointDownCB?.Invoke();
    }

    //抬起
    public void OnPointerUp(PointerEventData eventData)
    {
        onPointUpCB?.Invoke();
        // Debug.LogError("~~~ isHeroIconDragMove:" + isHeroIconDragMove);
        if (isHero && isHeroIconDragMove)
        {
            StartHeroPointerUp();
        }
    }

    #region hero icon

    private void StartHeroPointerUp()
    {
        transform.SetParent(beginDragParent, false);
        //transform.parent = beginDragParent;
        transform.SetSiblingIndex(beginDragSiblingIndex);
        transform.GetComponent<RectTransform>().anchoredPosition = beginDragRectPos;
        isHeroIconDragMove = false;
    }

    private void HeroCardDrag()
    {
        Vector2 tpos = touchcstool.onefingerpos;
        float deltax = tpos.x - beginPos.x;
        float deltay = tpos.y - beginPos.y;
        if (deltay >= 100)
        {
            onBeginCB?.Invoke();
            StartHeroPointerUp();
            iseffect = true;
            isHeroIconDragMove = false;
            return;
        }
        // 不左右移动

        //CardInputHandler cardInput = adjCardInputHandler;
        //if (cardInput != null)
        //{
        //    Vector2 v2_adjacent = cardInput.GetComponent<RectTransform>().anchoredPosition;
        //    Vector2 v2_self = this.GetComponent<RectTransform>().anchoredPosition;
        //}

        Vector3 newWorldPosition = ScreenToUIWorldPos(transform as RectTransform, Input.mousePosition, uiCamera);
        newWorldPosition.x = transform.position.x;
        transform.position = newWorldPosition; //  ScreenToUIWorldPos(transform as RectTransform, Input.mousePosition, uiCamera);
    }


    /// <summary>
    /// 获取相邻的,先找前一个
    /// </summary>
    /// <returns></returns>
    private CardInputHandler GetAdjacentCardInputHandler()
    {
        Transform parent = GameObject.Find("readlyRoot/roleList/view/content").transform;
        if (parent == null)
        {
            return null;
        }
        int childCount = parent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            CardInputHandler cardInput = parent.GetChild(i).GetComponent<CardInputHandler>();
            if (cardInput != null && cardInput.heroId == heroId)
            {
                if (i > 0)
                {
                    return parent.GetChild(i - 1).GetComponent<CardInputHandler>();
                }
                else if (i < childCount - 1)
                {
                    return parent.GetChild(i + 1).GetComponent<CardInputHandler>();
                }
            }
        }
        return null;
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
        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(targetRect, mousePos, canvasCam, out worldPos);
        return worldPos;
    }
    #endregion

}
