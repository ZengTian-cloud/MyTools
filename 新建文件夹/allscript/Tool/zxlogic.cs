using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class zxlogic
{
	// 可以清除启动界面标记
	public static bool bclearlaunch = false;
	// 启动游戏的次数标记
	public static int logincount = 1;
	// 是否需要切换账号标记
	public static bool bchangeaccount = false;
	// 切换账号参数
	public static string switchparam = "";
	// 记录的首包预制
	public static Dictionary<string, List<string>> listfpprefab = new Dictionary<string, List<string>>();
	// 记录的首包资源
	public static Dictionary<string, List<string>> listfpasset = new Dictionary<string, List<string>>();

	// 重新加载游戏
	public static void reloadgame(bool isChangeAccount, string param)
	{
		++logincount;
		bchangeaccount = isChangeAccount;
		switchparam = param;
		SceneManager.LoadScene(zxconfig.launchscene);
	}
	// 清除账号参数
	public static void removeswitchparam()
	{
		switchparam = "";
	}

	// 设置游戏正常速度
	private static float _timescale = 1;
	public static float timeratio
	{
		get { return _timescale; }
		set
		{
			if (_timescale != value)
			{
				_timescale = value;
				Time.timeScale = value;
			}
		}
	}

	// 设置游戏暂停缓动速度
	private static float _pausescale = -1;
	public static float pauseratio
	{
		get { return _pausescale; }
		set
		{
			if (_pausescale != value)
			{
				_pausescale = value;
				Time.timeScale = _pausescale < -0.01f ? _timescale : _pausescale;
			}
		}
	}

	// 游戏实际速度
	public static float timescale { get { return Time.timeScale; } }

	// 设置游戏限定帧率
	private static int _framecount = -1;
	public static int framerate
	{
		get { return _framecount; }
		set
		{
			if (_framecount != value)
			{
				_framecount = value;
				Application.targetFrameRate = value;
			}
		}
	}

	// 设置unity 播放/停止
	public static void setunityplayed(bool bplay)
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = bplay;
#endif
	}

	// 设置unity 开始/暂停
	public static void setunitypaused(bool bpause)
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPaused = bpause;
#endif
	}

	// 设置分辨率解决方案
	private static int tlastw = 0;
	private static int tlasth = 0;
	public static void setresolution(int twidth, int theight, bool fullscreen)
	{
		if (tlastw != twidth || tlasth != theight)
		{
			tlastw = twidth;
			tlasth = theight;
			Screen.SetResolution(twidth, theight, fullscreen);
		}
	}

	// 获取系统语言
	public static int getsystemlanguage()
	{
		var temLan = SystemLanguage.English;
		switch (Application.systemLanguage)
		{
			case SystemLanguage.Chinese:
				{
					temLan = SystemLanguage.ChineseSimplified;
				}
				break;
			case SystemLanguage.Unknown:
				{

				}
				break;
			default:
				{
					temLan = Application.systemLanguage;
				}
				break;
		}
		return (int)temLan;
	}

	// 释放内存
	public static void releasememory()
	{
		// GameCenter.mIns.m_LuaMgr.DoLuaGC();
		GC.Collect();
		Resources.UnloadUnusedAssets();
	}

	// // 获取首包预制
	// public static LuaTable getfirstprefab()
	// {
	// 	return getfirstres(listfpprefab);
	// }

	// // 获取首包资源
	// public static LuaTable getfirstasset()
	// {
	// 	return getfirstres(listfpasset);
	// }

	// // 获取首包加载的预制和资源
	// private static LuaTable getfirstres(Dictionary<string, List<string>> dictasset)
	// {
	// 	var tluatab = GameCenter.mIns.m_LuaMgr.newTable();
	// 	if (tluatab != null)
	// 	{
	// 		int temidx = 0;
	// 		foreach (var item in dictasset)
	// 		{
	// 			for (var i = 0; i < item.Value.Count; i++)
	// 			{
	// 				temidx++;
	// 				var tchildtab = GameCenter.mIns.m_LuaMgr.newTable();
	// 				tchildtab.Set<int, string>(1, item.Key);
	// 				tchildtab.Set<int, string>(2, item.Value[i]);
	// 				tluatab.Set<int, LuaTable>(temidx, tchildtab);
	// 			}
	// 		}
	// 	}
	// 	return tluatab;
	// }
}