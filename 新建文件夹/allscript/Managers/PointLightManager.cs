using System.Collections;
using System.Collections.Generic;
using Basics;
using UnityEngine;
[ExecuteInEditMode]
public class PointLightManager : SingletonOjbect
{
	static List<CustomPointLight> _pointComList = new List<CustomPointLight>();
	static Vector4[] _pointList = new Vector4[30];
	static Vector4[] _pointColorList = new Vector4[30];
	static Vector4 _defaultVec4 = new Vector4(0, 0, 0, 0);
	static int maxCount = 30;
	static int currentCount = 0;
	public Vector3 _attenuation = new Vector3(1f, 1f, 1f);

	public bool open = true;
	public bool Open
	{
		get => open;
		set
		{
			open = value;
			if (!open)
			{
				Shader.SetGlobalInt("_CustomProp_PointLightCount", 0);
			}
		}
	}
	float timer = 0.05f;
	private void Update()
	{
		if (!Open)
		{
			return;
		}
		timer -= Time.deltaTime;
		if (timer <= 0)
		{
			timer = 0.05f;
			CheckShowPoint();
			Shader.SetGlobalInt("_CustomProp_PointLightCount", currentCount);
			Shader.SetGlobalVectorArray("_CustomProp_PointLightArray", _pointList);
			Shader.SetGlobalVectorArray("_CustomProp_PointLightColor", _pointColorList);
			Shader.SetGlobalFloat("_CustomProp_fAttenuation0", _attenuation.x);
			Shader.SetGlobalFloat("_CustomProp_fAttenuation1", _attenuation.y);
			Shader.SetGlobalFloat("_CustomProp_fAttenuation2", _attenuation.z);
		}
	}

	public void SetDirty()
	{
		CheckShowPoint();
		Shader.SetGlobalInt("_CustomProp_PointLightCount", currentCount);
		Shader.SetGlobalVectorArray("_CustomProp_PointLightArray", _pointList);
		Shader.SetGlobalVectorArray("_CustomProp_PointLightColor", _pointColorList);
		Shader.SetGlobalFloat("_CustomProp_fAttenuation0", _attenuation.x);
		Shader.SetGlobalFloat("_CustomProp_fAttenuation1", _attenuation.y);
		Shader.SetGlobalFloat("_CustomProp_fAttenuation2", _attenuation.z);
	}

	static Vector4 vec4_info;
	static Vector4 vec4_col;

	public static void CheckShowPoint()
	{
		currentCount = 0;
		for (int i = 0; i < maxCount; i++)
		{
			vec4_info = _defaultVec4;
			vec4_col = _defaultVec4;
			if (_pointComList.Count >= i + 1)
			{
				var one = _pointComList[i];
				if (one._pointInfo != null && one.isActiveAndEnabled && one._intensity > 0)
				{
					currentCount++;
					vec4_info = one._pointInfo;
					vec4_col = one._color;
				}
			}
			_pointList[i] = vec4_info;
			_pointColorList[i] = vec4_col;
		}
	}

	// 添加一个点光源
	public static void AddOnePointLight(CustomPointLight one)
	{
		_pointComList.Add(one);
	}

	public static void RemoveOnePointLight(CustomPointLight one)
	{
		_pointComList.Remove(one);
	}

	private void OnDisable()
	{
		Shader.SetGlobalInt("_CustomProp_PointLightCount", 0);
	}

	private void OnDestroy()
	{
		Shader.SetGlobalInt("_CustomProp_PointLightCount", 0);
	}
}