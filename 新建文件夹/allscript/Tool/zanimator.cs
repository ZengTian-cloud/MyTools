using UnityEngine;

public static class zanimator
{
	// 设置播放模式
	public static void setplaymode(GameObject o, int tmode)
	{
		var anicom = getcomponent(o);
		if (anicom != null)
		{
			anicom.cullingMode = (AnimatorCullingMode)tmode;
		}
	}

	// 设置动画
	public static void setcontroller(GameObject o, string abname, string assetname)
	{
		var anicom = getcomponent(o);
		if (anicom != null)
		{
			//var temasset = zxload.addsync(abname, assetname);
			//anicom.runtimeAnimatorController = temasset as RuntimeAnimatorController;
		}
	}

	// 清除动画
	public static void clearcontroller(GameObject o)
	{
		var anicom = getcomponent(o);
		if (anicom != null)
		{
			anicom.runtimeAnimatorController = null;
		}
	}

	// 通过string设置trigger动画
	public static void setstrtrigger(GameObject o, string strname, bool bplayani)
	{
		var anicom = getcomponent(o);
		if (anicom != null && anicom.gameObject.activeInHierarchy)
		{
			if (bplayani)
			{
				anicom.SetTrigger(strname);
			}
			else
			{
				anicom.ResetTrigger(strname);
			}
		}
	}

	//播放指定名字的动画
	public static void playanimation(GameObject o, string strname)
	{
		var anicom = getcomponent(o);
		if (anicom != null && anicom.gameObject.activeInHierarchy)
		{
#if UNITY_EDITOR
			int stateid = Animator.StringToHash(strname);
			bool hasAction = anicom.HasState(0, stateid);
			if (!hasAction)
			{
				Debug.LogError("错误的动画: " + anicom.name + "->" + strname, anicom.gameObject);
			}
#endif
			anicom.Play(strname, 0, 0);
		}
	}

	// 强制播放到某个动画节点
	public static void playatpoint(GameObject o, string temani, float temtime)
	{
		var anicom = getcomponent(o);
		if (anicom != null && anicom.gameObject.activeInHierarchy)
		{
			anicom.Play(temani, -1, temtime);
		}
	}

	// 获取动画速度
	public static float getspeed(GameObject o)
	{
		var anicom = getcomponent(o);
		return anicom != null ? anicom.speed : 0;
	}

	// 设置动画速度
	public static void setspeed(GameObject o, float tspeed)
	{
		var anicom = getcomponent(o);
		if (anicom != null)
		{
			anicom.speed = tspeed;
		}
	}

	// 获取动画时长
	public static int getlength(GameObject o, string aniname)
	{
		var anicom = getcomponent(o);
		if (anicom != null)
		{
			var vclip = anicom.runtimeAnimatorController.animationClips;
			for (int i = 0; i < vclip.Length; i++)
			{
				if (vclip[i].name.Equals(aniname))
				{
					return (int)(vclip[i].length * 1000);
				}
			}
		}
		return 0;
	}

	// 获取动画是否循环
	public static bool getisloop(GameObject o, string aniname)
	{
		var anicom = getcomponent(o);
		if (anicom != null)
		{
			var vclip = anicom.runtimeAnimatorController.animationClips;
			for (int i = 0; i < vclip.Length; i++)
			{
				if (vclip[i].name.Equals(aniname))
				{
					return vclip[i].isLooping;
				}
			}
		}
		return false;
	}

	// 获取所有动画名字
	//public static LuaTable getallaniname(GameObject o)
	//{
	//	var anicom = getcomponent(o);
	//	var tluatab = GameCenter.mIns.m_LuaMgr.newTable();
	//	if (anicom != null && tluatab != null)
	//	{
	//		var vclip = anicom.runtimeAnimatorController.animationClips;
	//		if (vclip != null)
	//		{
	//			for (int i = 0; i < vclip.Length; i++)
	//			{
	//				tluatab.Set<int, string>(i + 1, vclip[i].name);
	//			}
	//		}
	//	}
	//	return tluatab;
	//}

	// 设置动画控制器的更新模式
	public static void setplayupdatemode(GameObject o, int tmode)
	{
		var anicom = getcomponent(o);
		if (anicom != null)
		{
			anicom.updateMode = (AnimatorUpdateMode)tmode;
		}
	}

	// 获取当前播放时间
	public static float getcurrentplaytime(GameObject o)
	{
		float ttime = 0;
		var anicom = getcomponent(o);
		if (anicom != null)
		{
			var stateInfo = anicom.GetCurrentAnimatorStateInfo(0);
			ttime = stateInfo.length * stateInfo.normalizedTime;
		}
		return ttime;
	}

	// 获取动画控制器组件
	//private static Animator getcomponent(int temid, string tempath)
	//{
	//	_tmpobj = GameNode.GetChildGameObject(temid, tempath);
	//	return _tmpobj != null ? _tmpobj.GetComponent<Animator>() : null;
	//}

	// 获取动画控制器组件
	private static Animator getcomponent(GameObject gameObject)
	{
		_tmpobj = gameObject;
		return _tmpobj != null ? _tmpobj.GetComponent<Animator>() : null;
	}

	private static GameObject _tmpobj;
}