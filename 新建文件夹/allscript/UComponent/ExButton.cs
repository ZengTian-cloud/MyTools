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

    // ����
    private ButtonClickedEvent my_onLongPress = new ButtonClickedEvent();
    public ButtonClickedEvent OnLongPress
    {
        get { return my_onLongPress; }
        set { my_onLongPress = value; }
    }

    //̧��
    private ButtonClickedEvent pointerUp = new ButtonClickedEvent();
    public ButtonClickedEvent OnMouseUp
    {
        get { return pointerUp; }
        set { pointerUp = value; }
    }


    // ������Ҫ�ı�������
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

    #region ����

    /// <summary>
    /// ������
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
        // ����ˢ�µ�ǰʱ��
        base.OnPointerDown(eventData);
        my_curPointDownTime = Time.time;
        my_isStartPress = true;
        my_longPressTrigger = false;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        // ���̧�𣬽�������������̧���¼���
        base.OnPointerUp(eventData);
        my_isStartPress = false;
        if (my_longPressTrigger)
        {
            pointerUp.Invoke();
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        // ����Ƴ���������������ʱ��־
        base.OnPointerExit(eventData);
        my_isStartPress = false;
    }

    #endregion

    
}
