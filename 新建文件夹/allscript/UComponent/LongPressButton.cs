using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.Events;

/// <summary>
/// 长按触发button
/// </summary>
public class LongPressButton : Button
{
    // 长按
    private ButtonClickedEvent my_onLongPress = new ButtonClickedEvent();
    public ButtonClickedEvent OnLongPress
    {
        get { return my_onLongPress; }
        set { my_onLongPress = value; }
    }

    //抬起
    private ButtonClickedEvent pointerUp = new ButtonClickedEvent();
    public ButtonClickedEvent OnMouseUp
    {
        get { return pointerUp; }
        set { pointerUp = value; }
    }


    // 长按需要的变量参数
    private bool my_isStartPress = false;
    private float my_curPointDownTime = 0f;
    [SerializeField]
    [Range(0, 5)]
    private float my_longPressTime = 2f;
    private bool my_longPressTrigger = false;


    void Update()
    {
        CheckIsLongPress();
    }

    #region 长按

    /// <summary>
    /// 处理长按
    /// </summary>
    void CheckIsLongPress()
    {
        if (my_isStartPress && !my_longPressTrigger)
        {
            if (Time.time > my_curPointDownTime + my_longPressTime)
            {
                my_longPressTrigger = true;
                my_isStartPress = false;
                if (my_onLongPress != null)
                {
                    my_onLongPress.Invoke();
                }
            }
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        // 按下刷新当前时间
        base.OnPointerDown(eventData);
        my_curPointDownTime = Time.time;
        my_isStartPress = true;
        my_longPressTrigger = false;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        // 鼠标抬起，结束长按，调用抬起事件。
        base.OnPointerUp(eventData);
        my_isStartPress = false;
        if (my_longPressTrigger)
        {
            pointerUp.Invoke();
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        // 鼠标移出，结束长按、计时标志
        base.OnPointerExit(eventData);
        my_isStartPress = false;
    }

    #endregion
}