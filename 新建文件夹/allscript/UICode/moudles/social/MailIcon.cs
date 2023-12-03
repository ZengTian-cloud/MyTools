using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MailIcon
{
    public GameObject obj;
    public Transform root;
    public RectTransform rect;

    public MailData data;
 
    public Button btn;
    public Image imgBg;
    public Image imgIcon;
    private Action<MailIcon> clickCallback;
    /// <summary>
    /// 构造函数
    /// </summary>
    public MailIcon() { }

    public MailIcon(MailData data, GameObject obj, Action<MailIcon> clickCallback)
    {
        this.data = data;
        BindObj(obj, clickCallback);
    }

    public void BindObj(GameObject obj, Action<MailIcon> clickCallback)
    {
        this.obj = obj;
        this.clickCallback = clickCallback;

        Transform tran = obj.transform;
        root = tran;
        btn = obj.GetComponent<Button>();
        imgBg = root.Find("imgBg").GetComponent<Image>();
        imgIcon = root.Find("imgIcon").GetComponent<Image>();

        commontool.RegisterButtonListen(btn, OnClick);
        SetIcon();
    }

    private void OnClick()
    {

    }

    public void SetIcon(int newState = -1)
    {
        if (data == null || imgIcon == null)
        {
            return;
        }
        // ui_m_icon_libao_dakai
        // ui_m_icon_libao
        // ui_m_icon_mail_wd
        // ui_m_icon_mail_yd

        if (newState != -1)
        {
            data.SetState(newState);
        }
        List<string> test = new List<string>() { "ui_m_icon_mail_wd", "ui_m_icon_mail_yd", "ui_m_icon_libao_dakai", "ui_m_icon_libao" };
        imgIcon.sprite = SpriteManager.Instance.GetSpriteSync(test[data.state]);
    }

    public void UpdateMd(MailData data)
    {
        this.data = data;
        SetIcon();
    }
}
