using UnityEngine;

public static class zxconfig
{
	public const string resmanifest = "temp_ab";
	public const string unmanifest = "temp_un";
	public const string encrydatafolder = "n7b82c2d74cf";
	public const string encryresfolder = "r81f0c7eb18cf";
	//public const string luafolder = "allzclua";
	public const string launchscene = "launchnormal";
	public const string recordfile = "a6f00b5c2804cf9f5a727a5367e2f3de";
	public const string logoname = "bg_logo";
	public const string channelpath = "channelcfg.txt";
	public static string[] vassetfolder = new string[] {
		"Assets/afirsts",
		"Assets/allbase",
		"Assets/allmodel",
		"Assets/allres",
		"Assets/allshader",
	};

	// 渠道信息
	public static string channelstr = string.Empty;
	// 是否清除预制文本
	public static bool bcleartext = true;
	// 是否为横屏
	public static bool blandscape { get; private set; } = true;
	// 设计分辨率宽度
	public static int designwidth { get; private set; } = blandscape ? 2436 : 1125;
	// 设计分辨率高度
	public static int designheight { get; private set; } = blandscape ? 1125 : 2436;
	// 适配分辨率宽度
	public static int adaptwidth = designwidth;
	// 适配分辨率高度
	public static int adaptheight = designheight;
	// 可用分辨率宽度
	public static int usablewidth = designwidth;
	// 可用分辨率高度
	public static int usableheight = designheight;
	// 手机留边的适配总距离
	public static int maxunusedlen = 110;
	// 初始屏幕宽度
	public static int initwidth = Screen.width;
	// 初始屏幕高度
	public static int initheight = Screen.height;
	// 屏幕分辨率宽度
	public static int screenwidth { get { return Screen.width; } }
	// 屏幕分辨率高度
	public static int screenheight { get { return Screen.height; } }

	// 屏幕分辨率坐标 转 设计分辨率坐标
	public static void screentodesignpos(float sx, float sy, ref float tx, ref float ty)
	{
		var temval = (float)adaptwidth / Screen.width * Screen.height;
		tx = (float)sx * adaptwidth / Screen.width;
		ty = (float)sy * temval / Screen.height;
	}

	// 设计分辨率坐标 转 屏幕分辨率坐标
	public static void designtoscreenpos(float sx, float sy, ref float tx, ref float ty)
	{
		var temval = (float)Screen.width / designwidth * designheight;
		tx = (float)sx * Screen.width / designwidth;
		ty = (float)sy * temval / designheight;
	}

	static zxconfig()
	{
#if UNITY_ANDROID
		int safewidth = (int)Screen.safeArea.width;
		int safeheight = (int)Screen.safeArea.height;
		if (blandscape && Screen.width - safewidth > 1)
		{
			var leftdis = (int)Screen.safeArea.x;
			var rightdis = Screen.width - safewidth - leftdis;
			maxunusedlen = Mathf.Max(leftdis, rightdis) * 2 * zxconfig.designheight / Screen.height;
		}
		else if (!blandscape && Screen.height - safeheight > 1)
		{
			var bottomdis = (int)Screen.safeArea.y;
			var topdis = Screen.height - safeheight - bottomdis;
			maxunusedlen = Mathf.Max(bottomdis, topdis) * 2 * zxconfig.designwidth / Screen.width;
		}
#endif
	}
}