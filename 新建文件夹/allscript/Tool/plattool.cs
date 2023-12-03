using UnityEngine;

// 平台工具
public static class plattool
{
	// 是否为assetbundle模式
	public static bool bassetbundle
	{
		get
		{
#if UNITY_EDITOR && !KZC_AssetBundle
			return false;
#else
			return true;
#endif
		}
	}

	// 当前平台是否支持GPU Instance
	public static bool supportsinstancing
	{
		get
		{
			return SystemInfo.supportsInstancing;
		}
	}

	// 当前平台描述
	public static string strplat
	{
		get
		{
#if UNITY_ANDROID
			return "android";
#elif (UNITY_IPHONE || UNITY_IOS)
			return "ios";
#elif UNITY_EDITOR
			return "pc";
#else
			return "unknown";
#endif
		}
	}

	// 是否为Android平台
	public static bool isandroid
	{
		get
		{
#if !UNITY_EDITOR && UNITY_ANDROID
			return true;
#else
			return false;
#endif
		}
	}

	// 是否为ios平台
	public static bool isios
	{
		get
		{
#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_IOS)
			return true;
#else
			return false;
#endif
		}
	}

	// 是否在Unity运行
	public static bool isunity
	{
		get
		{
#if UNITY_EDITOR
			return true;
#else
			return false;
#endif
		}
	}

	public static int intstrplat
	{
		get
		{
#if UNITY_ANDROID
			return 2;
#elif (UNITY_IPHONE || UNITY_IOS)
			return 1;
#elif UNITY_EDITOR
			return 3;
#else
			return 4;
#endif
		}
	}
}