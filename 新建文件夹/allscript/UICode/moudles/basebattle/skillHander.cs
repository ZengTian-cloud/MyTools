using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class skillHander : MonoBehaviour,IPointerExitHandler,IPointerEnterHandler
{
    public bool onPoint = false;

    private void OnEnable()
    {
        onPoint = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPoint = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPoint = false;
    }


}
