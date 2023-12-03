//using System;
//using System.Collections.Generic;
//using Object = UnityEngine.Object;

//public static class zxload
//{
//	// 从Resources加载资源
//	public static Object addresources(string assetpath)
//	{
//		var assetres = GameCenter.mIns.m_ResMgr.LoadResourceRes(assetpath);
//		if (!assetres)
//		{
//			zxlogger.logwarningformat("Resources加载资源失败: {0}", assetpath);
//		}
//		return assetres;
//	}

//	// 同步加载资源
//	public static Object addsync(string abname, string assetname, bool bsprite = false)
//	{
//		var assetres = GameCenter.mIns.m_ResMgr.LoadSyncRes(abname, assetname, bsprite);
//		if (!assetres)
//		{
//			var bloadab = GameCenter.mIns.m_ResMgr.CheckLoadAssetBundle(abname);
//			zxlogger.logwarningformat("同步加载资源失败: {0}  {1}  {2}", abname, assetname, bloadab);
//			//未找到图片资源 显示默认图片
//			assetres = GameCenter.mIns.m_ResMgr.LoadSyncRes("comatlas1", "rgba_box_empty");
//		}
//#if UNITY_EDITOR
//		if (!zxlogic.listfpasset.ContainsKey(abname))
//		{
//			zxlogic.listfpasset.Add(abname, new List<string>());
//		}
//		if (!zxlogic.listfpasset[abname].Contains(assetname))
//		{
//			zxlogic.listfpasset[abname].Add(assetname);
//		}
//#endif
//		return assetres;
//	}

//	// 异步加载资源
//	public static void addasync(string abname, string assetname, Action<Object> tcallback, bool bsprite = false)
//	{
//		GameCenter.mIns.m_ResMgr.LoadAsyncRes(abname, assetname, delegate (Object assetres)
//		{
//			if (!assetres)
//			{
//				var bloadab = GameCenter.mIns.m_ResMgr.CheckLoadAssetBundle(abname);
//				zxlogger.logwarningformat("异步加载资源失败: {0}  {1}  {2}", abname, assetname, bloadab);
//				assetres = GameCenter.mIns.m_ResMgr.LoadSyncRes("comatlas1", "rgba_box_empty");
//			}
//#if UNITY_EDITOR
//			if (!zxlogic.listfpasset.ContainsKey(abname))
//			{
//				zxlogic.listfpasset.Add(abname, new List<string>());
//			}
//			if (!zxlogic.listfpasset[abname].Contains(assetname))
//			{
//				zxlogic.listfpasset[abname].Add(assetname);
//			}
//#endif
//			tcallback?.Invoke(assetres);
//		}, bsprite);
//	}

//	// 判断资源有没有
//	public static bool check_haveasset(string abname, string assetname, bool bsprite = false)
//	{
//		var assetres = GameCenter.mIns.m_ResMgr.LoadSyncRes(abname, assetname, bsprite);
//		return assetres != null;
//	}
//}