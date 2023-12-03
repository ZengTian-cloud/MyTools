using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

// 五维属性展示
public class RadarMap : Image
{
	/// <summary>
	/// 所要展示的属性点
	/// </summary>
	public List<float> _PropsValue = new List<float>();
	/// <summary>
	/// 每个属性点最大值
	/// </summary>
	public float _MaxValue = 100;

	private int vertnum;

	public void SetPropsVlaue(params float[] _props)
	{
		_PropsValue.Clear();
		_PropsValue.AddRange(_props);
		SetVerticesDirty();
	}

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		if (_PropsValue.Count < 3)
		{
			base.OnPopulateMesh(toFill);
			return;
		}
		toFill.Clear();
		vertnum = _PropsValue.Count;
		float tw = rectTransform.rect.width;
		float th = rectTransform.rect.height;
		Vector4 uv = overrideSprite != null ? DataUtility.GetInnerUV(overrideSprite) : Vector4.zero;
		float uvCenterX = (uv.x + uv.z) * rectTransform.pivot.x;
		float uvCenterY = (uv.y + uv.w) * rectTransform.pivot.y;
		float uvScaleX = (uv.z - uv.x) / tw;
		float uvScaleY = (uv.w - uv.y) / th;
		int trianglecount = vertnum;
		float degreeconst = 1f / vertnum * 2f * Mathf.PI;
		UIVertex vert0 = new UIVertex
		{
			position = Vector2.zero,
			color = color,
			uv0 = new Vector2(uvCenterX, uvCenterY)
		};
		toFill.AddVert(vert0);
		for (int i = 0; i < vertnum; i++)
		{
			UIVertex vert = new UIVertex();
			float degree = (float)i * degreeconst;
			vert.position = new Vector2(Mathf.Sin(degree) * tw * 0.5f, Mathf.Cos(degree) * th * 0.5f) * (_PropsValue[i] / _MaxValue);
			vert.uv0 = new Vector2(vert.position.x * uvScaleX + uvCenterX, vert.position.y * uvScaleY + uvCenterY);
			vert.color = color;
			toFill.AddVert(vert);
		}
		for (int i = 0; i < trianglecount; i++)
		{
			toFill.AddTriangle(0, i, i + 1);
		}
		toFill.AddTriangle(0, trianglecount, 1);
	}
}