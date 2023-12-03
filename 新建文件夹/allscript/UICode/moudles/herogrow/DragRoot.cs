using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragRoot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //开始拖拽回调
    public Action onBeginCB;

    //拖拽中回调
    public Action onDragCB;

    //拖拽结束回调
    public Action onEndCB;

    public void OnBeginDrag(PointerEventData eventData)
    {
        onBeginCB?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        onDragCB?.Invoke();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        onEndCB?.Invoke();
    }
}

