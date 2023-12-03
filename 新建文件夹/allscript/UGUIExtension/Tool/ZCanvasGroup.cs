using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ZCanvasGroup
{
	// 检查并添加画布组组件
	public static void checkcomponent(int temid, string tempath)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			if (!_tmpobj.GetComponent<CanvasGroup>())
			{
				_tmpobj.AddComponent<CanvasGroup>();
			}
		}
	}

	// 设置透明度
	public static void setalpha(int temid, string tempath, int temalpha)
	{
		var cgcom = getcomponent(temid, tempath);
		if (cgcom != null)
		{
			cgcom.alpha = temalpha * 0.00392157f;
		}
	}

	// 设置是否有遮挡事件
	public static void setinteractable(int temid, string tempath, bool binteractable)
	{
		var cgcom = getcomponent(temid, tempath);
		if (cgcom != null)
		{
			cgcom.interactable = binteractable;
		}
	}

	// 设置是否能接受射线
	public static void setblockraycast(int temid, string tempath, bool bblockraycast)
	{
		var cgcom = getcomponent(temid, tempath);
		if (cgcom != null)
		{
			cgcom.blocksRaycasts = bblockraycast;
		}
	}

	// 获取画布组组件
	private static CanvasGroup getcomponent(int temid, string tempath)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		return _tmpobj != null ? _tmpobj.GetComponent<CanvasGroup>() : null;
	}

	private static GameObject _tmpobj;
}