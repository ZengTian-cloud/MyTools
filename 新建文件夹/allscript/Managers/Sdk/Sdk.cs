using Basics;
using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum SdkMethod
{
    dologin=1,//登陆
    dologout = 2, //登出
    doexit = 3, //退出
    dopay = 4, //支付
    docreaterole = 5, //创建游戏
    doentergame = 6, //登陆完成后进入游戏
    dolvup = 7, //角色升级
    dogetagreement = 8, //获取权限
    doupload = 9, //上报
    advinit = 10,//广告初始化
    advshow = 11, //播放广告
}

public enum CallMethod
{
    intisucc  = 1,
    loginsucc =2,
    logoutsucc =3,
    exitsucc =4,
    switchsucc =5,
    dogetagreementsucc=6
}

public enum SdkInfo {
    channelid = 1, // 获取渠道号
    platid = 2, // 获取平台号
}

public static class Sdk
{

    private static Action<string> loginSuccessCallback = null;

    public static void init() {
        GameCenter.mIns.m_SdkMgr.sdk_callback = (type, param) =>
        {
            switch (type)
            {
                case CallMethod.loginsucc:
                    loginSuccessCallback?.Invoke(param);
                    break;
            }
        };
    }

    public static string GetSdkChannelId()
    {
        return GameCenter.mIns.m_SdkMgr.GetSdkInfo((int)SdkInfo.channelid);
    }
    public static void GetSdkPlatId()
    {
        GameCenter.mIns.m_SdkMgr.GetSdkInfo((int)SdkInfo.platid);
    }
    public static void Login(Action<string> callback)
    {
        loginSuccessCallback = callback;
        GameCenter.mIns.m_SdkMgr.CallSdkInfo((int)SdkMethod.dologin, "");
    }
    public static void Logout()
    {
        GameCenter.mIns.m_SdkMgr.CallSdkInfo((int)SdkMethod.dologout, "");
    }

}

