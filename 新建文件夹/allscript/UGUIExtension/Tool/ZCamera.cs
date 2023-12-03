using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ZCamera
{
	// 获取所有带摄像机的节点
	/*
	public static Table getchildren(int temid, string tempath)
	{
		var listcam = getcomponentsinchildren(temid, tempath);
		var tluatab = GameCenter.mIns.m_LuaMgr.newTable();
		for (var i = 0; i < listcam.Count; i++)
		{
			tluatab.Set<int, int>(i + 1, GameNode.CheckId(listcam[i].gameObject));
		}
		return tluatab;
	}
	*/

	// 设置所有子节点是否enable
	public static void setchildrenenable(int temid, string tempath, bool benable)
	{
		var listcam = getcomponentsinchildren(temid, tempath);
		for (var i = 0; i < listcam.Count; i++)
		{
			listcam[i].enabled = benable;
		}
	}

	// 设置相机是否为正交模式(borthographic ->true.正交 false.透视)
	public static void setisorthographic(int temid, string tempath, bool borthographic)
	{
		var camcom = getcomponent(temid, tempath);
		if (camcom != null)
		{
			camcom.orthographic = borthographic;
		}
	}

	// 设置相机深度
	public static void setdepth(int temid, string tempath, int tdepth)
	{
		var camcom = getcomponent(temid, tempath);
		if (camcom != null)
		{
			camcom.depth = tdepth;
		}
	}

	// 设置正交相机近裁剪
	public static void setnearclipplane(int temid, string tempath, float tnearclip)
	{
		var camcom = getcomponent(temid, tempath);
		if (camcom != null)
		{
			camcom.nearClipPlane = tnearclip;
		}
	}

	// 设置正交相机远裁剪
	public static void setfarclipplane(int temid, string tempath, float tfarclip)
	{
		var camcom = getcomponent(temid, tempath);
		if (camcom != null)
		{
			camcom.farClipPlane = tfarclip;
		}
	}

	// 获取正交相机视野
	public static float getorthographicsize(int temid, string tempath)
	{
		var camcom = getcomponent(temid, tempath);
		return camcom != null ? camcom.orthographicSize : 0;
	}

	// 设置正交相机视野
	public static void setorthographicsize(int temid, string tempath, float size)
	{
		var camcom = getcomponent(temid, tempath);
		if (camcom != null)
		{
			camcom.orthographicSize = size;
		}
	}

	// 获取透视模式相机视野
	public static float getfieldofview(int temid, string tempath)
	{
		var camcom = getcomponent(temid, tempath);
		return camcom != null ? camcom.fieldOfView : 0;
	}

	// 设置透视模式相机视野
	public static void setfieldofview(int temid, string tempath, float targetview)
	{
		var camcom = getcomponent(temid, tempath);
		if (camcom != null)
		{
			camcom.fieldOfView = targetview;
		}
	}

	// 获取摄像机aspect
	public static float getaspect(int temid, string tempath)
	{
		var camcom = getcomponent(temid, tempath);
		return camcom != null ? camcom.aspect : 0;
	}

	// 设置相机aspect
	public static void setaspect(int temid, string tempath, float temval)
	{
		var camcom = getcomponent(temid, tempath);
		if (camcom != null)
		{
			camcom.aspect = temval;
		}
	}

	// 屏幕坐标转世界坐标
	public static void screentoworldpos(int temid, string tempath, float temx, float temy, int temz,
		ref float worldx, ref float worldy, ref float worldz)
	{
		var camcom = getcomponent(temid, tempath);
		if (camcom != null)
		{
			var tempos = camcom.ScreenToWorldPoint(new Vector3(temx, temy, temz));
			worldx = tempos.x;
			worldy = tempos.y;
			worldz = tempos.z;
		}
	}

	// 世界坐标转屏幕坐标
	public static void worldtoscreenpoint(int temid, string tempath, float worldx, float worldy, float worldz,
		ref float temx, ref float temy, ref float temz)
	{
		var camcom = getcomponent(temid, tempath);
		if (camcom != null)
		{
			var tempos = camcom.WorldToScreenPoint(new Vector3(worldx, worldy, worldz));
			temx = tempos.x;
			temy = tempos.y;
			temz = tempos.z;
		}
	}

	// 触摸点位对应摄像机射线
	public static Vector3 shoottouchray_check(Camera camera, int tmask,Action<GameObject> func = null)
	{
		var touchpos = touchcstool.onefingerpos;
		return shootray_check(camera, tmask, touchpos, func);
	}

	// 摄像机发射射线
	public static Vector3 shootray_check(Camera camera, int tmask, Vector2 v2, Action<GameObject> func = null)
	{
		if (camera != null)
		{
			RaycastHit rhit;
			Ray temray = camera.ScreenPointToRay(v2);
			Debug.DrawRay(camera.transform.position, temray.direction, Color.red);
            if (Physics.Raycast(temray, out rhit, camera.farClipPlane, tmask))
			{
                if (func != null)
                {
                    func(rhit.collider.gameObject);
                }

                return rhit.point;
            }
        }
		return Vector3.zero;
	}

    //摄像机检测obj
    public static GameObject shootray_checkobj(Camera camera, int tmask, Action<GameObject> func = null)
    {
        if (camera != null)
        {
			Vector2 v2 = touchcstool.onefingerpos;
            RaycastHit rhit;
            Ray temray = camera.ScreenPointToRay(v2);
            Debug.DrawRay(camera.transform.position, temray.direction, Color.yellow);
            if (Physics.Raycast(temray, out rhit, camera.farClipPlane, tmask))
            {
                if (func != null)
                {
                    func(rhit.collider.gameObject);
                }

                return rhit.collider.gameObject;
            }
        }
        return null;
    }


    // 设置是否开启HDR
    public static void setopenhdr(int temid, string tempath, bool bopen)
	{
		var camcom = getcomponent(temid, tempath);
		if (camcom != null)
		{
			camcom.allowHDR = bopen;
		}
	}

	// 设置是否开启抗锯齿
	public static void setopenmsaa(int temid, string tempath, bool bopen)
	{
		var camcom = getcomponent(temid, tempath);
		if (camcom != null)
		{
			camcom.allowMSAA = bopen;
		}
	}

	// 获取摄像机组件
	private static Camera getcomponent(int temid, string tempath)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		return _tmpobj != null ? _tmpobj.GetComponent<Camera>() : null;
	}

	// 获取所有子节点摄像机组件
	private static List<Camera> getcomponentsinchildren(int temid, string tempath)
	{
		var listcom = new List<Camera>();
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			var vcam = _tmpobj.GetComponentsInChildren<Camera>();
			if (vcam != null)
			{
				listcom.AddRange(vcam);
			}
		}
		return listcom;
	}

	private static GameObject _tmpobj;

	// 触摸点
	public static Vector2 touchPosition
	{
		get
		{
#if UNITY_EDITOR
			return Input.mousePosition;
#else
			if (Input.touchCount > 0) {
				return Input.GetTouch (0).position;
			} else {
				return Vector2.zero;
			}
#endif
		}
	}
	/// 获取触摸位置
	public static void GetTouchXY(ref float touchx, ref float touchy)
	{
		var temtouchpos = touchPosition;
		touchx = temtouchpos.x;
		touchy = temtouchpos.y;
	}

	/// 将GameObject坐标设置到触摸对应的camera坐标
	public static void AttachToTouch(GameObject obj,Camera camera,Vector2 v)
	{
		Vector3 revec3;
		
		RectTransform temrt = obj.GetComponent<RectTransform>();
		if (temrt != null && camera != null)
		{
			RectTransformUtility.ScreenPointToWorldPointInRectangle(temrt, v, camera, out revec3);
            obj.transform.position = revec3;
		}
	}

	// 获取摄像机组件
	private static RectTransform getrectcomponent(int temid)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, null);
		return _tmpobj != null ? _tmpobj.GetComponent<RectTransform>() : null;
	}
}