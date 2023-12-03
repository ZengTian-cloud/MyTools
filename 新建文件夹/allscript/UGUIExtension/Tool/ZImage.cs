using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ZImage
{
	// 设置组件开关
	public static void setenable(int temid, string tempath, bool benable)
	{
		var imgcom = getcomponent(temid, tempath);
		if (imgcom != null)
		{
			imgcom.enabled = benable;
		}
	}

	// 同步设置图片
	public static void setsyncsprite_real(int temid, string tempath, string abname, string assetname)
	{
		//var imgcom = getcomponent(temid, tempath);
		//setspriteasset(imgcom, zxload.addsync(abname, assetname, true));
	}

	// 异步设置图片
	public static void setasyncsprite_real(int temid, string tempath, string abname, string assetname, System.Action callback)
	{
		//var imgcom = getcomponent(temid, tempath);
		//zxload.addasync(abname, assetname, delegate (Object tasset)
		//{
		//	setspriteasset(imgcom, tasset);
		//	callback?.Invoke();
		//}, true);
	}

	// 设置图片颜色
	public static void setcolor(int temid, string tempath, int temred, int temgreen, int temblue, int temalpha)
	{
		var imgcom = getcomponent(temid, tempath);
		if (imgcom != null)
		{
			imgcom.color = NewColor(temred, temgreen, temblue, temalpha);
		}
	}

	// 设置所有子节点图片颜色
	public static void setchildrencolor(int temid, string tempath, int temred, int temgreen, int temblue, int temalpha)
	{
		var listimg = getcomponentsinchildren(temid, tempath);
		for (var i = 0; i < listimg.Count; i++)
		{
			listimg[i].color = NewColor(temred, temgreen, temblue, temalpha); ;
		}
	}

	/// 设置图片颜色
	public static void setimagecolor(int temid, string tempath, long color)
	{
		var imgcom = getcomponent(temid, tempath);
		if (imgcom != null)
		{
			float r = (color >> 24) * 0.00392157f;
			float g = ((color >> 16) & 255) * 0.00392157f;
			float b = ((color >> 8) & 255) * 0.00392157f;
			float a = (color & 255) * 0.00392157f;
			imgcom.color = new Color(r, g, b, a);
		}
	}
	/// 设置所有图片颜色
	public static void setallimagecolor(int temid, string tempath, long color)
	{
		var listimg = getcomponentsinchildren(temid, tempath);
		float r = (color >> 24) * 0.00392157f;
		float g = ((color >> 16) & 255) * 0.00392157f;
		float b = ((color >> 8) & 255) * 0.00392157f;
		float a = (color & 255) * 0.00392157f;
		for (var i = 0; i < listimg.Count; i++)
		{
			listimg[i].color = new Color(r, g, b, a);
		}
	}

	// 设置透明度
	public static void setalpha(int temid, string tempath, int temalpha)
	{
		var imgcom = getcomponent(temid, tempath);
		if (imgcom != null)
		{
			var tcolor = imgcom.color;
			tcolor.a = temalpha * 0.00392157f;
			imgcom.color = tcolor;
		}
	}

	// 设置透明度低于一定值取消射线检测
	public static void sethitalpha(int temid, string tempath, int temalpha)
	{
		var imgcom = getcomponent(temid, tempath);
		if (imgcom != null)
		{
			imgcom.alphaHitTestMinimumThreshold = temalpha * 0.00392157f;
		}
	}

	// 设置是否接收射线检测
	public static void setraycast(int temid, string tempath, bool braycast)
	{
		var imgcom = getcomponent(temid, tempath);
		if (imgcom != null)
		{
			imgcom.raycastTarget = braycast;
		}
	}

	// 设置自适应
	public static void setnativesize(int temid, string tempath)
	{
		var imgcom = getcomponent(temid, tempath);
		if (imgcom != null)
		{
			imgcom.SetNativeSize();
		}
	}

	// 设置填充值
	public static void setfillamount(int temid, string tempath, float temval)
	{
		var imgcom = getcomponent(temid, tempath);
		if (imgcom != null)
		{
			imgcom.fillAmount = temval;
		}
	}
	// 获取填充值
	public static float getfillamount(int temid, string tempath)
	{
		var imgcom = getcomponent(temid, tempath);
		if (imgcom != null)
		{
			return imgcom.fillAmount;
		}
		return 0;
	}

	//设置材质(AB包)
	public static void setabmaterial(int temid, string tempath, string abnamee, string assetname, bool isnotuse)
	{
		//var imgcom = getcomponent(temid, tempath);
		//if (imgcom != null)
		//{
		//	Material mat = zxload.addsync(abnamee, assetname) as Material;
		//	imgcom.material = !isnotuse ? mat : null;
		//}
	}

	// 设置材质颜色
	public static void setmaterialcolor(int temid, string tempath, string tname, int temred, int temgreen, int temblue, int temalpha)
	{
		var imgcom = getcomponent(temid, tempath);
		if (imgcom != null && imgcom.material != null)
		{
			var temcolor = NewColor(temred, temgreen, temblue, temalpha);
			imgcom.material.SetColor(tname, temcolor);
		}
	}
	// 设置材质参数vertor
	public static void setmaterialvertor(int temid, string tempath, string tname, float v1, float v2, float v3, float v4)
	{
		var imgcom = getcomponent(temid, tempath);
		if (imgcom != null)
		{
			var vec = new Vector4(v1, v2, v3, v4);
			if (imgcom.material)
				imgcom.material.SetVector(tname, vec);
		}
	}

	// -------------------------------------------- 特殊处理(start) --------------------------------------------
	// 设置图片置灰
	public static void setgrey(int temid, string tempath, bool bgrey)
	{
		//var imgcom = getcomponent(temid, tempath);
		//if (imgcom != null)
		//{
		//	imgcom.material = bgrey ? zxload.addsync("uishaders", "uigreymat") as Material : null;
		//}
	}

	// 设置所有子节点图片置灰
	public static void setchildrengrey(int temid, string tempath, bool bgrey)
	{
		//var listimg = getcomponentsinchildren(temid, tempath);
		//for (var i = 0; i < listimg.Count; i++)
		//{
		//	listimg[i].material = bgrey ? zxload.addsync("uishaders", "uigreymat") as Material : null;
		//}
	}
	// 设置文本置灰
	public static void setblack(int temid, string tempath, bool bblack)
	{
		//var imgcom = getcomponent(temid, tempath);
		//if (imgcom != null)
		//{
		//	imgcom.material = bblack ? zxload.addsync("uishaders", "uiblack") as Material : null;
		//}
	}

	// 设置所有子节点文本置灰
	public static void setchildrenblack(int temid, string tempath, bool bblack)
	{
		//var listimg = getcomponentsinchildren(temid, tempath);
		//for (var i = 0; i < listimg.Count; i++)
		//{
		//	listimg[i].material = bblack ? zxload.addsync("uishaders", "uiblack") as Material : null;
		//}
	}

	// 设置图片灰度
	public static void setgreyval(int temid, string tempath, float greyval)
	{
		//var imgcom = getcomponent(temid, tempath);
		//if (imgcom != null && imgcom.material != null)
		//{
		//	greyval = greyval > 1 ? 1 : (greyval < 0 ? 0 : greyval);
		//	if (!imgcom.material)
		//	{
		//		imgcom.material = zxload.addsync("uishaders", "uigreymat") as Material;
		//	}
		//	imgcom.material.SetFloat("_GreyFactor", greyval);
		//}
	}

	// 设置芒星图
	public static void setradarmap(int temid, string tempath, params float[] _props)
	{
		var rmcom = getradarcomponent(temid, tempath);
		if (rmcom != null)
		{
			rmcom.SetPropsVlaue(_props);
		}
	}

	// -------------------------------------------- 特殊处理(end) --------------------------------------------

	// 获取Image组件
	private static Image getcomponent(int temid, string tempath)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		return _tmpobj != null ? _tmpobj.GetComponent<Image>() : null;
	}

	// 获取所有子节点Image组件
	private static List<Image> getcomponentsinchildren(int temid, string tempath)
	{
		var listcom = new List<Image>();
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			var vcam = _tmpobj.GetComponentsInChildren<Image>(true);
			if (vcam != null)
			{
				listcom.AddRange(vcam);
			}
		}
		return listcom;
	}

	// 获取芒星图案组件
	private static RadarMap getradarcomponent(int temid, string tempath)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		return _tmpobj != null ? _tmpobj.GetComponent<RadarMap>() : null;
	}

	// 设置图片
	private static void setspriteasset(Image imgcom, Object tasset)
	{
		if (imgcom != null)
		{
			var tspr = tasset as Sprite;
			if (tspr != null)
			{
				imgcom.sprite = tspr;
				return;
			}
			var t2d = tasset as Texture2D;
			if (t2d != null)
			{
				imgcom.sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
				return;
			}
			zxlogger.logerrorobject("加载Image图片失败: " + imgcom.name, imgcom);
			imgcom.sprite = null;
		}
	}

	// 创建颜色类
	private static Color NewColor(int temred, int temgreen, int temblue, int temalpha)
	{
		return new Color(temred * 0.00392157f, temgreen * 0.00392157f, temblue * 0.00392157f, temalpha * 0.00392157f);
	}

	private static GameObject _tmpobj;
}