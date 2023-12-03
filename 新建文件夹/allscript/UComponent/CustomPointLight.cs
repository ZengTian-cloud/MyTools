using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CustomPointLight : MonoBehaviour
{
	// Start is called before the first frame update
	public Vector4 _pointInfo;
	public Color _color = Color.white;
	public float _intensity = 1f;
	private void Awake()
	{
		_pointInfo = new Vector4(pos.x, pos.y, pos.z, _intensity);
	}
	void OnEnable()
	{
		PointLightManager.AddOnePointLight(this);
	}
	Vector3 pos;
	private void Update()
	{
		pos = this.gameObject.transform.position;
		_pointInfo.x = pos.x;
		_pointInfo.y = pos.y;
		_pointInfo.z = pos.z;
		_pointInfo.w = _intensity;
	}

	private void OnDisable()
	{
		PointLightManager.RemoveOnePointLight(this);
	}

	private void OnDestroy()
	{
		PointLightManager.RemoveOnePointLight(this);
	}
}
