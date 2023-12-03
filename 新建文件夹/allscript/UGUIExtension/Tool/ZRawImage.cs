using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class ZRawImage
{
	// 重新创建材质
	public static void recreatemat(int temid, string tempath)
	{
		var ricom = getcomponent(temid, tempath);
		if (ricom != null)
		{
			ricom.material = new Material(ricom.material);
		}
	}

	// 同步设置图片
	public static void setsyncsprite_real(int temid, string tempath, string abname, string assetname)
	{
		//var ricom = getcomponent(temid, tempath);
		//setspriteasset(ricom, zxload.addsync(abname, assetname));
	}

	//  异步设置图片
	public static void setasyncsprite_real(int temid, string tempath, string abname, string assetname, Action<Object> tcallback)
	{
		//var ricom = getcomponent(temid, tempath);
		//zxload.addasync(abname, assetname, delegate (Object tasset)
		//{
		//	setspriteasset(ricom, tasset);
		//});
	}

	// 设置自适应
	public static void setnativesize(int temid, string tempath)
	{
		var ricom = getcomponent(temid, tempath);
		if (ricom != null)
		{
			ricom.SetNativeSize();
		}
	}

	// 设置图片颜色
	public static void setcolor(int temid, string tempath, int temred, int temgreen, int temblue, int temalpha)
	{
		var ricom = getcomponent(temid, tempath);
		if (ricom != null)
		{
			ricom.color = NewColor(temred, temgreen, temblue, temalpha);
		}
	}

	// 设置直接截屏
	public static void capturescreen(int temid, string tempath, Action tcallback)
	{
		//GameCenter.mIns.m_ResMgr.DoCaptureScreen(delegate (Texture2D screenshot)
		//{
		//	setspriteasset(getcomponent(temid, tempath), screenshot);
		//	tcallback?.Invoke();
		//});
	}

	// 设置从相机截屏
	public static void capturecamera(int temid, string tempath, string savepath, Action tcallback, params int[] vcamtag)
	{
		//GameCenter.mIns.m_ResMgr.DoCaptureCamera(delegate (Texture2D screenshot)
		//{
		//	setspriteasset(getcomponent(temid, tempath), screenshot);
		//	tcallback?.Invoke();
		//}, savepath, vcamtag);
	}

	// 获取RawImage组件
	private static RawImage getcomponent(int temid, string tempath)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		return _tmpobj != null ? _tmpobj.GetComponent<RawImage>() : null;
	}

	// 设置图片
	private static void setspriteasset(RawImage ricom, Object tasset)
	{
		var bgcom = ricom as AdaptBG;
		if (ricom != null)
		{
			var tspr = tasset as Sprite;
			if (tspr != null)
			{
				ricom.texture = tspr.texture;
				if (bgcom != null)
				{
					bgcom.DoRefresh();
				}
				return;
			}
			var t2d = tasset as Texture2D;
			if (t2d != null)
			{
				ricom.texture = t2d;
				if (bgcom != null)
				{
					bgcom.DoRefresh();
				}
				return;
			}
			zxlogger.logerrorobject("加载RawImage图片失败: " + ricom.name, ricom);
			ricom.texture = null;
		}
	}

	// 创建颜色类
	private static Color NewColor(int temred, int temgreen, int temblue, int temalpha)
	{
		return new Color(temred * 0.00392157f, temgreen * 0.00392157f, temblue * 0.00392157f, temalpha * 0.00392157f);
	}

	private static GameObject _tmpobj;
}