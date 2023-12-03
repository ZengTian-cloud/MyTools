using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ReplaceComponent(typeof(Button))]
public class ExButton : Button
{
    public enum SoundEffectType
    {
        None = 0,

        Normal,

        Open,

        Close,
    }
    public SoundEffectType soundEffect;

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        switch (soundEffect)
        {
            case SoundEffectType.None:

                return;

            case SoundEffectType.Normal:

                AudioManager.Instance.PlayMultipleSound("button_click");

                break;
            case SoundEffectType.Open:

                AudioManager.Instance.PlayMultipleSound("close_window2");

                break;
            case SoundEffectType.Close:

                AudioManager.Instance.PlayMultipleSound("close_window1");

                break;
            default:
                break;
        }
    }

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
    private float my_longPressTime = 0.6f;
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
