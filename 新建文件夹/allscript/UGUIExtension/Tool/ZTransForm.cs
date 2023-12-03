using UnityEngine;
using UnityEngine.Events;

public static class ZTransForm
{
	// 根据路径查找子节点
	public static int findchild_check(int temid, string tempath)
	{
		return GameNode.CheckId(GameNode.GetChildGameObject(temid, tempath));
	}

	// 根据index查找子节点
	public static int getonechild_check(int temid, string tempath, int index)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		var totalnum = _tmpobj != null ? _tmpobj.transform.childCount : 0;
		_tmpobj = (index >= 1 && index <= totalnum) ? _tmpobj.transform.GetChild(index - 1).gameObject : null;
		return GameNode.CheckId(_tmpobj);
	}


	// 获取单层子节点数量
	public static int getcountoflistchild(int temid, string tempath)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		return _tmpobj != null ? _tmpobj.transform.childCount : 0;
	}

	// 获取父节点
	public static int getparent_check(int temid, string tempath)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		_tmpobj = _tmpobj != null ? _tmpobj.transform.parent.gameObject : null;
		return GameNode.CheckId(_tmpobj);
	}

	// 改变父节点
	public static void setparent(int temid, string tempath, int parentid, bool stayworldpos = false)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		var parentobj = GameNode.GetGameObject(parentid);
		if (_tmpobj != null && parentobj != null)
		{
			_tmpobj.transform.SetParent(parentobj.transform, stayworldpos);
		}
	}

	// 获取local位置
	public static void getlocalposition(int temid, string tempath, ref float temx, ref float temy, ref float temz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		var tempos = _tmpobj != null ? _tmpobj.transform.localPosition : Vector3.zero;
		temx = tempos.x;
		temy = tempos.y;
		temz = tempos.z;
	}

	// 设置local位置
	public static void setlocalposition(int temid, string tempath, float temx, float temy, float temz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			_tmpobj.transform.localPosition = new Vector3(temx, temy, temz);
		}
	}

	// 获取全局位置
	public static void getposition(int temid, string tempath, ref float temx, ref float temy, ref float temz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		var tempos = _tmpobj != null ? _tmpobj.transform.position : Vector3.zero;
		temx = tempos.x;
		temy = tempos.y;
		temz = tempos.z;
	}

	// 设置全局位置
	public static void setposition(int temid, string tempath, float temx, float temy, float temz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			_tmpobj.transform.position = new Vector3(temx, temy, temz);
		}
	}

	// 获取local缩放
	public static void getlocalscale(int temid, string tempath, ref float temx, ref float temy, ref float temz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		var temscale = _tmpobj != null ? _tmpobj.transform.localScale : Vector3.zero;
		temx = temscale.x;
		temy = temscale.y;
		temz = temscale.z;
	}

	// 设置local缩放
	public static void setlocalscale(int temid, string tempath, float temx, float temy, float temz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			_tmpobj.transform.localScale = new Vector3(temx, temy, temz);
		}
	}

	// 获取local欧拉角
	public static void getlocaleulerangles(int temid, string tempath, ref float temx, ref float temy, ref float temz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		var temeuler = _tmpobj != null ? _tmpobj.transform.localEulerAngles : Vector3.zero;
		temx = temeuler.x;
		temy = temeuler.y;
		temz = temeuler.z;
	}

	// 设置local欧拉角
	public static void setlocaleulerangles(int temid, string tempath, float temx, float temy, float temz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			_tmpobj.transform.localEulerAngles = new Vector3(temx, temy, temz);
		}
	}

	// 获取全局欧拉角
	public static void geteulerangles(int temid, string tempath, ref float temx, ref float temy, ref float temz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		var temeuler = _tmpobj != null ? _tmpobj.transform.eulerAngles : Vector3.zero;
		temx = temeuler.x;
		temy = temeuler.y;
		temz = temeuler.z;
	}

	// 设置全局欧拉角
	public static void seteulerangles(int temid, string tempath, float temx, float temy, float temz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			_tmpobj.transform.eulerAngles = new Vector3(temx, temy, temz);
		}
	}

	// 获取在父节点的序号
	public static int getsiblingindex(int temid, string tempath)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		return _tmpobj != null ? _tmpobj.transform.GetSiblingIndex() + 1 : 0;
	}

	// 设置在父节点的序号
	public static void setsiblingindex(int temid, string tempath, int index)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			_tmpobj.transform.SetSiblingIndex(index - 1);
		}
	}

	// 设置为父节点的首个子节点
	public static void setasfirstsibling(int temid, string tempath)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			_tmpobj.transform.SetAsFirstSibling();
		}
	}

	// 设置为父节点的最后一个子节点
	public static void setaslastsibling(int temid, string tempath)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			_tmpobj.transform.SetAsLastSibling();
		}
	}

	// 世界坐标转本地坐标
	public static void worldtolocalpos(int temid, string tempath, float worldx, float worldy, float worldz,
		ref float temx, ref float temy, ref float temz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		var tempos = _tmpobj != null ? _tmpobj.transform.InverseTransformPoint(worldx, worldy, worldz) : Vector3.zero;
		temx = tempos.x;
		temy = tempos.y;
		temz = tempos.z;
	}

	// 本地坐标转世界坐标
	public static void localtoworldpos(int temid, string tempath, float temx, float temy, float temz,
		ref float worldx, ref float worldy, ref float worldz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		var worldpos = _tmpobj != null ? _tmpobj.transform.TransformPoint(new Vector3(temx, temy, temz)) : Vector3.zero;
		worldx = worldpos.x;
		worldy = worldpos.y;
		worldz = worldpos.z;
	}

	// 让目标正对屏幕
	public static void faceuptoscreen(int temid, string tempath, int camid)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		var cameraobj = GameNode.GetGameObject(camid);
		if (_tmpobj != null && cameraobj != null)
		{
			var cameuler = cameraobj.transform.eulerAngles;
			_tmpobj.transform.eulerAngles = cameuler;
		}
	}

	// 立刻看向某点
	public static void lookatvector(int temid, string tempath, float temx, float temy, float temz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			_tmpobj.transform.LookAt(new Vector3(temx, temy, temz));
		}
	}

	// 逐渐看向某点
	public static void lookatslerp(int temid, string tempath, float temx, float temy, float temz, float tspeed)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			var tarqt = Quaternion.LookRotation(new Vector3(temx, temy, temz) - _tmpobj.transform.position);
			_tmpobj.transform.rotation = Quaternion.RotateTowards(_tmpobj.transform.rotation, tarqt, tspeed);
		}
	}

	// 获取自身坐标轴相对于世界坐标的向量 :x轴
	public static void getright(int temid, string tempath, ref float rx, ref float ry, ref float rz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			rx = _tmpobj.transform.right.x;
			ry = _tmpobj.transform.right.y;
			rz = _tmpobj.transform.right.z;
		}
	}

	// 获取自身坐标轴相对于世界坐标的向量 :y轴
	public static void getup(int temid, string tempath, ref float rx, ref float ry, ref float rz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			rx = _tmpobj.transform.up.x;
			ry = _tmpobj.transform.up.y;
			rz = _tmpobj.transform.up.z;
		}
	}

	// 获取自身坐标轴相对于世界坐标的向量 :z轴
	public static void getforward(int temid, string tempath, ref float rx, ref float ry, ref float rz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			rx = _tmpobj.transform.forward.x;
			ry = _tmpobj.transform.forward.y;
			rz = _tmpobj.transform.forward.z;
		}
	}

	// 看向GameObject
	public static void lookattarget(int temid, string tempath, int targetid, string targetpath)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		var _targetobj = GameNode.GetChildGameObject(targetid, targetpath);
		if (_tmpobj != null && _targetobj != null)
		{
			_tmpobj.transform.LookAt(_targetobj.transform);
		}
	}

	//物体 自己 旋转
	public static void rotatearound(int temid, string tempath, float x, float y, float z, float min, float max)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			_tmpobj.transform.localEulerAngles = new Vector3(getlimit(_tmpobj.transform.localEulerAngles.x + x, min, max), _tmpobj.transform.localEulerAngles.y + y, _tmpobj.transform.localEulerAngles.z + z);
		}
	}
	//物体 自己 旋转
	public static void rotatearound2(int temid, string tempath, float x, float y, float z, float minx, float maxx,
		float miny, float maxy, float minz, float maxz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			_tmpobj.transform.localEulerAngles = new Vector3(getlimit(_tmpobj.transform.localEulerAngles.x + x, minx, maxx), getlimit(_tmpobj.transform.localEulerAngles.y + y, miny, maxy), getlimit(_tmpobj.transform.localEulerAngles.z + z, minz, maxz));
		}
	}

	// 物体移动
	public static void translate(int temid, string tempath, float x, float y, float z, float speed, float deltatime)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			Vector3 dir = new Vector3(x, y, z);
			_tmpobj.transform.Translate(dir * speed * deltatime);
		}
	}

	public static float getlimit(float target, float min, float max)
	{
		if (target < min)
		{
			return min;
		}
		else if (target > max)
		{
			return max;
		}
		else
		{
			return target;
		}
	}
	public static void rotatearound(int temid, string tempath, float tagx, float tagy, float tagz, float speed)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			Vector3 tagpos = new Vector3(tagx, tagy, tagz);
			_tmpobj.transform.RotateAround(tagpos, Vector3.up, speed);
		}
	}
	public static void setrotate(int temid, string tempath, float tagx, float tagy, float tagz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			Vector3 tagpos = new Vector3(tagx, tagy, tagz);
			_tmpobj.transform.Rotate(tagpos);
		}
	}
	public static void setuirotation(int temid, string tempath, float tagx, float tagy, float tagz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			Vector3 tagpos = new Vector3(tagx, tagy, tagz);
			Quaternion rotation = Quaternion.LookRotation(tagpos - _tmpobj.transform.position, _tmpobj.transform.TransformDirection(Vector3.up));
			_tmpobj.transform.rotation = new Quaternion(0, 0, rotation.z, rotation.w);
		}
	}

	public static void setuirotation1(int temid, string tempath, float tagx, float tagy, float tagz)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null)
		{
			Vector3 tagpos = new Vector3(tagx, tagy, tagz);
			_tmpobj.transform.right = tagpos - _tmpobj.transform.position;
		}
	}



	//进行刚体运动(考虑碰撞)
	public static void doRigbodyMove(int temid, string tempath, float x, float y, float z)
	{
		_tmpobj = GameNode.GetChildGameObject(temid, tempath);
		if (_tmpobj != null && _tmpobj.GetComponent<Rigidbody>() != null)
		{
			_tmpobj.GetComponent<Rigidbody>().MovePosition(new Vector3(x, y, z));
		}
	}

	private static GameObject _tmpobj;
}