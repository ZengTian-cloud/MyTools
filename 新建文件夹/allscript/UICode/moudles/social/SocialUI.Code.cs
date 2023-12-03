using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UIManager;
using TMPro;

public partial class SocialUI
{
    #region 定义和生命周期
    public override UILayerType uiLayerType => UILayerType.Normal;
    public override string uiAtlasName => "socialui";

    protected override void OnInit() {
        //检测热更
        GameCenter.mIns.m_HttpMgr.SendData("POST",30,"","",(state,content,val)=> { 
            
        });
        CreateBtnList();

      
    }


    protected override void OnOpen()
    {
        InitUI();
        base.OnOpen();
        // 注册update
        GameCenter.mIns.m_UIMgr.AddUpdateWin(this);
        EnumSocialTabType socialTabType = EnumSocialTabType.Friend;
        if (openArgs != null && openArgs.Length > 0)
        {
            socialTabType = (EnumSocialTabType)openArgs[0];
        }
        Init(socialTabType);
        // ChatManager.Instance.GetLatestChat("5610406");
    }

    public override void UpdateWin()
    {
        if (socialSubModuleUIs != null)
        {
            foreach (var ui in socialSubModuleUIs)
            {
                ui.Update(Time.deltaTime);
            }
        }
    }

    protected override void OnClose()
    {
        if (socialSubModuleUIs != null)
        {
            foreach (var module in socialSubModuleUIs)
            {
                module.DestroyUIPrefab();
            }
        }
        socialSubModuleUIs.Clear();
        base.OnClose();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }


    public virtual void ReDisplay()
    {
    }
    #endregion

    #region functions
    /// <summary>
    /// register
    /// </summary>
    /// <param name="register"></param>
    protected override void OnRegister(bool register)
    {
        if (isRegister == register)
        {
            return;
        }
        base.OnRegister(register);
    }

    /// <summary>
    /// 返回
    /// </summary>
    partial void OnClickClose()
    {
        this.Close();
    }

    /// <summary>
    /// 设置按钮
    /// </summary>
    partial void OnClickSet()
    {
        GameCenter.mIns.m_UIMgr.PopMsg("未开发");
    }

    /// <summary>
    /// 初始化界面
    /// </summary>
    /// <param name="socialTabType"></param>
    private void Init(EnumSocialTabType socialTabType)
    {
        CreateBtnList();
        OnClickBtnTab(socialTabType);
    }

    // 左侧按钮列表
    private List<SocialBtnTab> socialBtnTabs = new List<SocialBtnTab>();
    // 社交界面子ui模块
    private List<SocialSubModuleUI> socialSubModuleUIs = new List<SocialSubModuleUI>();

    private EnumSocialTabType currSTT = EnumSocialTabType.Friend;
    // 创建社交按钮列表
    public void CreateBtnList()
    {
        btnItem.gameObject.SetActive(false);
        ClearBtnTabs();
        foreach (EnumSocialTabType sbType in Enum.GetValues(typeof(EnumSocialTabType)))
        {
            GameObject tabItem = GameObject.Instantiate(btnItem.gameObject);
            SocialBtnTab tab = new SocialBtnTab(sbType, tabItem, OnClickBtnTab);
            tabItem.transform.SetParent(btnItem.parent);
            tabItem.transform.localScale = Vector3.one;
            tabItem.SetActive(true);
            socialBtnTabs.Add(tab);
        }
    }

    private SocialSubModuleUI GetSocialSubModuleUI(EnumSocialTabType socialTabType)
    {
        foreach (var module in socialSubModuleUIs)
        {
            if (module.UIRoot != null && module.SocialTabType == socialTabType)
            {
                return module;
            }
        }
        return null;
    }

    private SocialSubModuleUI currSocialSubModuleUI;
    /// <summary>
    /// 点击了左侧按钮列表中的按钮
    /// </summary>
    /// <param name="socialTabType"></param>
    private void OnClickBtnTab(EnumSocialTabType socialTabType)
    {
        foreach (var tab in socialBtnTabs)
        {
            tab.SetSelectImage(socialTabType == tab.socialTabType);
        }

        bool isLoadedPrefab = false;
        foreach (var module in socialSubModuleUIs)
        {
            if (module.UIRoot != null && socialTabType == module.SocialTabType)
            {
                isLoadedPrefab = true;
            }
        }

        switch (socialTabType)
        {
            case EnumSocialTabType.Friend:
                // friend
                if (!isLoadedPrefab)
                    OnLoadFriendModule();
                else
                    GetSocialSubModuleUI(EnumSocialTabType.Friend).SetActive(true);
                SocialSubModuleUI socialSubModuleUI_mail = GetSocialSubModuleUI(EnumSocialTabType.Mail);
                if (socialSubModuleUI_mail != null)
                    socialSubModuleUI_mail.SetActive(false);
                currSocialSubModuleUI = socialSubModuleUI_mail;
                break;
            case EnumSocialTabType.Mail:
                // mail
                if (!isLoadedPrefab)
                    OnLoadMailModule();
                else
                    GetSocialSubModuleUI(EnumSocialTabType.Mail).SetActive(true);
                SocialSubModuleUI socialSubModuleUI_friend = GetSocialSubModuleUI(EnumSocialTabType.Friend);
                if (socialSubModuleUI_friend != null)
                    socialSubModuleUI_friend.SetActive(false);
                currSocialSubModuleUI = socialSubModuleUI_friend;
                break;
        }
        currSTT = socialTabType;
    }

    /// <summary>
    /// 清空左侧按钮列表
    /// </summary>
    private void ClearBtnTabs()
    {
        if (socialBtnTabs != null)
        {
            foreach (var item in socialBtnTabs)
            {
                item.OnDestroy();
            }
            socialBtnTabs = new List<SocialBtnTab>();
        }
    }

    /// <summary>
    /// 加载好友模块ui
    /// </summary>
    private void OnLoadFriendModule()
    {
        Debug.Log("OnLoadFriendModule!");
        FriendUI friendUI = new FriendUI();
        friendUI.LoadUIPrefab(uiRoot.transform);
        socialSubModuleUIs.Add(friendUI);
        currSocialSubModuleUI = friendUI;
    }

    /// <summary>
    /// 加载邮件模块ui
    /// </summary>
    private void OnLoadMailModule()
    {
        Debug.Log("OnLoadMailModule!");
        MailListUI mailListUI = new MailListUI();
        mailListUI.LoadUIPrefab(uiRoot.transform);
        socialSubModuleUIs.Add(mailListUI);
        currSocialSubModuleUI = mailListUI;
    }
    #endregion

    /// <summary>
    /// 内部构建左侧按钮类
    /// </summary>
    private class SocialBtnTab
    {
        // 按钮类型
        public EnumSocialTabType socialTabType;
        // 按钮对象
        public GameObject o;
        // 点击回调
        public Action<EnumSocialTabType> clickCallback;

        private GameObject selectObj;
        private Image icon;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socialTabType"></param>
        /// <param name="o"></param>
        /// <param name="clickCallback"></param>
        public SocialBtnTab(EnumSocialTabType socialTabType, GameObject o, Action<EnumSocialTabType> clickCallback)
        {
            this.socialTabType = socialTabType;
            this.o = o;
            this.clickCallback = clickCallback;
            selectObj = o.transform.GetChild(0).gameObject;
            icon = o.transform.GetChild(1).GetComponent<Image>();

            o.GetComponent<Button>().onClick.AddListener(() => {
                this.clickCallback?.Invoke(socialTabType);
            });
            SetBtnIcon(false);
        }

        /// <summary>
        /// 设置选中效果
        /// </summary>
        /// <param name="b"></param>
        public void SetSelectImage(bool b)
        {
            if (selectObj == null)
                selectObj = o.transform.GetChild(0).gameObject;
            selectObj.SetActive(b);

            SetBtnIcon(b);
        }

        public void SetBtnIcon(bool b)
        {
            string iconName = "";
            switch (socialTabType)
            {
                case EnumSocialTabType.Friend:
                default:
                    // friend
                    iconName = b ? "ui_c_icon_haoyou_xz" : "ui_c_icon_haoyou_wxz";
                    break;
                case EnumSocialTabType.Mail:
                    // mail
                    iconName = b ? "ui_c_icon_mail_xz" : "ui_c_icon_mail_wxz";
                    break;
            }

            if (icon == null)
                icon = o.transform.GetChild(1).GetComponent<Image>();
            if (!string.IsNullOrEmpty(iconName))
            {
                icon.sprite = SpriteManager.Instance.GetSpriteSync(iconName);
            }
            icon.SetNativeSize();
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void OnDestroy()
        {
            if (o!= null)
            {
                GameObject.Destroy(o);
            }
        }
    }
}

/// <summary>
/// 社交包含模块对象类型
/// </summary>
public enum EnumSocialTabType
{
    Friend = 0,
    Mail = 1,
}

/// <summary>
/// 社交子UI模块基类
/// </summary>
public abstract class SocialSubModuleUI
{
    public abstract EnumSocialTabType SocialTabType { get; }

    public abstract GameObject UIRoot { get; }

    public abstract void LoadUIPrefab(Transform parent);

    public abstract void InitUI();

    public abstract void SetActive(bool b);

    public abstract void DestroyUIPrefab();

    public virtual void Display() { 
    
    }

    public virtual void Update(float dt)
    {

    }
}