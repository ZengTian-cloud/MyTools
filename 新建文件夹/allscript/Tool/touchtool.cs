using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// CS调用触摸工具
public static class touchcstool
{
	public static Vector2 onefingerpos
	{
		get
		{
#if UNITY_EDITOR
			return Input.mousePosition;
#else
			return Input.touchCount > 0 ? Input.GetTouch (0).position : Vector2.zero;
#endif
		}
	}

	public static bool istouchui(Vector2 touchpos)
	{
		return touchresult(touchpos).Count > 0;
	}

	public static List<RaycastResult> touchresult(Vector2 touchpos)
	{
		var eventDataCurrentPostion = new PointerEventData(EventSystem.current);
		eventDataCurrentPostion.position = touchpos;
		var results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPostion, results);
		return results;
	}
}

// lua调用触摸工具
public static class touchtool
{
	public static bool istouching
	{
		get
		{
#if UNITY_EDITOR
			return Input.GetMouseButtonDown(0) || Input.GetMouseButton(0);
#else
			return Input.touchCount > 0;
#endif
		}
	}

	public static void onefingerxy(ref float touchx, ref float touchy)
	{
		var tempos = touchcstool.onefingerpos;
		touchx = tempos.x;
		touchy = tempos.y;
	}

	public static bool checkonefingertargetui(string name)
	{
		var results = touchcstool.touchresult(touchcstool.onefingerpos);
		for (var i = 0; i < results.Count; i++)
		{
			if (results[i].gameObject.name.Equals(name))
			{
				return true;
			}
		}
		return false;
	}

	// ****************************** EventSystem ******************************
	//判定触碰的最上层UI名称是否一致
	public static bool CheckFirstTouchObjByName(string name)
	{
		PointerEventData eventDataCurrentPostion = new PointerEventData(EventSystem.current);
		eventDataCurrentPostion.position = ZCamera.touchPosition;
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPostion, results);
		if (results.Count > 0)
		{
			return results[0].gameObject.name.Equals(name);
		}
		return false;
	}
}