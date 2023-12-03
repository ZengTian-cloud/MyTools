using System;
using System.Runtime.InteropServices;
using Basics;
using LitJson;
using UnityEngine;

namespace Managers
{
	public class SdkManager: SingletonOjbect
    {
        public Action<CallMethod, string> sdk_callback;

#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_IOS)
		[DllImport ("__Internal")]
		private static extern string _callSdk (int type, String param, String target);
#endif

#if !UNITY_EDITOR && UNITY_ANDROID
		private string JavaClassPath = "com.qile.sdk.game.CallbackCenterUnity";
#endif


        public string CallSdkInfo(int method, string param)
        {
            Debug.Log($"GIANTGAME 调用sdk方法 method:{method},param:{param}");
            string temval = null;
            try
            {
#if !UNITY_EDITOR && UNITY_ANDROID
				using (var cls_CallbackCenter = new UnityEngine.AndroidJavaClass (JavaClassPath))
				{
                    Debug.Log($"GIANTGAME 调用sdk方法 JavaClassPath:{JavaClassPath},this.gameObject.name:{this.gameObject.name}，cls_CallbackCenter：{cls_CallbackCenter}");
					temval = cls_CallbackCenter.CallStatic<string> ("callSdkInfo", method, param, this.gameObject.name);
				}
#elif !UNITY_EDITOR && (UNITY_IPHONE || UNITY_IOS)
				temval = _callSdk (method, param, this.gameObject.name);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError("SDK调用错误: " + ex.ToString());
            }
            Debug.Log ($"GIANTGAME  method:{method},param:{param} 调用返回-:{temval}" );
            return temval;
        }

        public string GetSdkInfo(int method)
        {
            string temval = null;
            try
            {
#if !UNITY_EDITOR && UNITY_ANDROID
				using (var cls_CallbackCenter = new UnityEngine.AndroidJavaClass (JavaClassPath))
				{
					temval = cls_CallbackCenter.CallStatic<string> ("getSdkInfo", method);
				}
#elif !UNITY_EDITOR && (UNITY_IPHONE || UNITY_IOS)
				temval = _callSdk (method + 100, "", this.gameObject.name);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError("SDK获取渠道号错误: " + ex.ToString());
            }
            return temval;
        }

        public void sdkCallBack(string param)
        {
            Debug.Log($"GIANTGAME  收到SDK回调 param:{param} ");

            JsonData data = jsontool.newwithstring(param);
            int type = (int)data["GameMethod"];
            sdk_callback?.Invoke((CallMethod)type, param);
        }
    }

}