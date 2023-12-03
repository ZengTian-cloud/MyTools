using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


class ZNode
 {
	private static GameObject _tmpobj1;
	private static GameObject _tmpobj2;

	/// <summary>
	/// 异步加载预置
	/// </summary>
	/// <param name="temid"></param>
	/// <param name="tempath"></param>
	/// <param name="abname"></param>
	/// <param name="assetname"></param>
	/// <param name="tcallback"></param>
	/// <param name="bretain"></param>
	public static void addasync_check(int temid, string tempath, string abname, string assetname,
		Action<int> tcallback, bool bretain)
	{
//		_tmpobj1 = GameNode.GetChildGameObject(temid, tempath);
//		GameCenter.mIns.m_ResMgr.LoadAsyncPrefab(_tmpobj1, abname, assetname, delegate (GameObject loadobj)
//		{
//			var loadid = GameNode.CheckId(loadobj);
//#if UNITY_EDITOR
//			if (zxconfig.bcleartext && !bretain)
//			{
//				ZUText.clearchildren(loadid, null);
//			}
//			if (!zxlogic.listfpprefab.ContainsKey(abname))
//			{
//				zxlogic.listfpprefab.Add(abname, new List<string>());
//			}
//			if (!zxlogic.listfpprefab[abname].Contains(assetname))
//			{
//				zxlogic.listfpprefab[abname].Add(assetname);
//			}
//#endif
//			tcallback?.Invoke(loadid);
//		});
	}


	/// <summary>
	/// 同步加载预置
	/// </summary>
	/// <param name="temid"></param>
	/// <param name="tempath"></param>
	/// <param name="abname"></param>
	/// <param name="assetname"></param>
	/// <param name="bretain"></param>
	/// <returns></returns>
	public static int addsync_check(int temid, string tempath, string abname, string assetname, bool bretain)
	{
//		_tmpobj1 = GameNode.GetChildGameObject(temid, tempath);
//		var loadid = GameNode.CheckId(GameCenter.mIns.m_ResMgr.LoadSyncPrefab(_tmpobj1, abname, assetname));
//#if UNITY_EDITOR
//		if (zxconfig.bcleartext && !bretain)
//		{
//			ZUText.clearchildren(loadid, null);
//		}
//		if (!zxlogic.listfpprefab.ContainsKey(abname))
//		{
//			zxlogic.listfpprefab.Add(abname, new List<string>());
//		}
//		if (!zxlogic.listfpprefab[abname].Contains(assetname))
//		{
//			zxlogic.listfpprefab[abname].Add(assetname);
//		}
//#endif
		return 0;
	}

	// 全局查找节点
	public static int findabsolute_check(string tempath)
	{
		_tmpobj1 = GameObject.Find(tempath);
		return GameNode.CheckId(_tmpobj1);
	}

	// 获取自身显隐
	public static bool activeself(int temid, string tempath)
	{
		_tmpobj1 = GameNode.GetChildGameObject(temid, tempath);
		return _tmpobj1 != null ? _tmpobj1.activeSelf : false;
	}

	// 设置自身显隐
	public static void setactive(int temid, string tempath, bool bactive)
	{
		_tmpobj1 = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj1 != null)
		{
			if (_tmpobj1.activeSelf != bactive)
				_tmpobj1.SetActive(bactive);
		}
	}

	// 设置所有子节点显隐
	public static void setlistchildactive(int temid, string tempath, bool bactive)
	{
		_tmpobj1 = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj1 != null)
		{
			for (var i = _tmpobj1.transform.childCount - 1; i >= 0; i--)
			{
				_tmpobj1.transform.GetChild(i).gameObject.SetActive(bactive);
			}
		}
	}

	// 获取节点名字
	public static string getname(int temid, string tempath)
	{
		_tmpobj1 = GameNode.GetChildGameObject(temid, tempath);
		return _tmpobj1 != null ? _tmpobj1.name : string.Empty;
	}

	// 设置节点名字
	public static void setname(int temid, string tempath, string temname)
	{
		_tmpobj1 = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj1 != null)
		{
			_tmpobj1.name = temname;
		}
	}

	// 设置层级
	public static void setlayer(int temid, string tempath, int tlayer)
	{
		_tmpobj1 = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj1 != null)
		{
			_tmpobj1.layer = tlayer;
		}
	}

	// 设置所有子节点层级
	public static void setchildrenlayer(int temid, string tempath, int tlayer)
	{
		_tmpobj1 = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj1 != null)
		{
			var vtrans = _tmpobj1.GetComponentsInChildren<Transform>();
			if (vtrans != null)
			{
				for (var i = 0; i < vtrans.Length; i++)
				{
					vtrans[i].gameObject.layer = tlayer;
				}
			}
		}
	}

	// 设置静态批处理
	public static void setstaticbatch(int temid, string tempath)
	{
		_tmpobj1 = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj1 != null)
		{
			StaticBatchingUtility.Combine(_tmpobj1);
		}
	}

	// 设置所有图片,文本置灰
	public static void setchildrengrey(int temid, string tempath, bool bgrey, bool offeffct)
	{
		ZImage.setchildrengrey(temid, tempath, bgrey);
		ZUText.setchildrengrey(temid, tempath, bgrey, offeffct);
	}
}

