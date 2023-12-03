using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct EffectStyle
{
	public bool gradient;
	public Color32 topColor;
	public Color32 bottomColor;

	public bool outline;
	public Color32 outlineColor;
	public Vector2 outlineDistance;
	public bool outlineUseGraphicAlpha;

	public bool shadow;
	public Color32 shadowColor;
	public Vector2 shadowDistance;
	public bool shadowUseGraphicAlpha;

	public EffectStyle(bool gradient, Color32 topColor, Color32 bottomColor,
		bool outline, Color32 outlineColor, Vector2 outlineDistance, bool outlineUseGraphicAlpha,
		bool shadow, Color32 shadowColor, Vector2 shadowDistance, bool shadowUseGraphicAlpha)
	{
		this.gradient = gradient;
		this.topColor = topColor;
		this.bottomColor = bottomColor;
		this.outline = outline;
		this.outlineColor = outlineColor;
		this.outlineDistance = outlineDistance;
		this.outlineUseGraphicAlpha = outlineUseGraphicAlpha;
		this.shadow = shadow;
		this.shadowColor = shadowColor;
		this.shadowDistance = shadowDistance;
		this.shadowUseGraphicAlpha = shadowUseGraphicAlpha;
	}

	public static EffectStyle none
	{
		get
		{
			return new EffectStyle(false, Color.white, Color.black,
				false, Color.white, new Vector2(1f, -1f), true,
				false, Color.white, new Vector2(1.44f, -1.4f), true);
		}
	}
}

public class Line
{
	private int _startVertexIndex = 0;

	public int StartVertexIndex
	{
		get
		{
			return _startVertexIndex;
		}
	}

	private int _endVertexIndex = 0;
	public int EndVertexIndex
	{
		get
		{
			return _endVertexIndex;
		}
	}

	private int _vertexCount = 0;
	public int VertexCount
	{
		get
		{
			return _vertexCount;
		}
	}

	public Line(int startVertexIndex, int length)
	{
		_startVertexIndex = startVertexIndex;
		_endVertexIndex = length * 6 - 1 + startVertexIndex;
		_vertexCount = length * 6;
	}
}

[AddComponentMenu("UI/Effects/TextEffect")]
// 自定义文字效果
public class TextEffect : BaseMeshEffect
{
	[SerializeField]
	private EffectStyle effect = EffectStyle.none;

	public EffectStyle Effect
	{
		get
		{
			return this.effect;
		}
		set
		{
			this.effect = value;
			SetDirty();
		}
	}

	public void SetEffect(TextColor colortype)
	{
		switch (colortype)
		{
			case TextColor.not:
				break;
			case TextColor.A:
				Set_Effect(Color.black, new Vector2(1.44f, -1.4f));
				break;
			case TextColor.B:
				Set_Effect(Color.black, new Vector2(1.44f, -1.4f));
				break;
			case TextColor.C:
				Set_Effect(Color.black, new Vector2(1.44f, -1.4f));
				break;
			case TextColor.D:
				Set_Effect(Color.black, new Vector2(1.44f, -1.4f));
				break;
			case TextColor.E:
				Set_Effect(Color.black, new Vector2(1.44f, -1.4f));
				break;
			case TextColor.F:
				Set_Effect(Color.black, new Vector2(1.44f, -1.4f));
				break;
			case TextColor.G:
				Set_Effect(Color.black, new Vector2(1.44f, -1.4f));
				break;
			case TextColor.H:
				Set_Effect(Color.black, new Vector2(1.44f, -1.4f));
				break;
			case TextColor.J:
				Set_Effect(Color.black, new Vector2(1.44f, -1.4f));
				break;
			case TextColor.K:
				Set_Effect(Color.black, new Vector2(1.44f, -1.4f));
				break;
			case TextColor.L:
				Set_Effect(Color.black, new Vector2(1.44f, -1.4f));
				break;
			case TextColor.M:
				Set_Effect(Color.black, new Vector2(1.44f, -1.4f));
				break;
			case TextColor.Q:
				Set_Effect(Color.black, new Vector2(1.44f, -1.4f));
				break;
			case TextColor.S:
				Set_Effect(Color.black, new Vector2(1.44f, -1.4f));
				break;
			default:
				break;
		}
	}

	public void Set_Effect(Color color, Vector2 vec)
	{
		this.effect.shadowColor = new Color(color.r, color.g, color.b, 0.81f);
		this.effect.shadowDistance = vec;
	}

	public void SetEffectOutLineColor(Color _c)
	{
		this.effect.outlineColor = _c;
		SetDirty();
	}

	public void SetDirty()
	{
		if (graphic != null)
		{
			graphic.SetVerticesDirty();
		}
	}

	public override void ModifyMesh(Mesh mesh)
	{

	}

	public override void ModifyMesh(VertexHelper mesh)
	{
		if (!IsActive())
		{
			return;
		}

		Text text = GetComponent<Text>();
		if (text == null)
		{
			Debug.Log("Missing Text component");
			return;
		}

		try
		{
			List<UIVertex> vertexs = new List<UIVertex>();
			mesh.GetUIVertexStream(vertexs);
			int indexCount = mesh.currentIndexCount;

			string[] lineTexts = text.text.Split('\n');

			Line[] lines = new Line[lineTexts.Length];

			for (int i = 0; i < lines.Length; i++)
			{
				if (i == 0)
				{
					lines[i] = new Line(0, lineTexts[i].Length + 1);
				}
				else if (i > 0 && i < lines.Length - 1)
				{
					lines[i] = new Line(lines[i - 1].EndVertexIndex + 1, lineTexts[i].Length + 1);
				}
				else
				{
					lines[i] = new Line(lines[i - 1].EndVertexIndex + 1, lineTexts[i].Length);
				}
			}

			UIVertex vt;

			for (int i = 0; i < lines.Length; i++)
			{
				for (int j = lines[i].StartVertexIndex + 6; j <= lines[i].EndVertexIndex; j++)
				{
					if (j < 0 || j >= vertexs.Count)
					{
						continue;
					}
					vt = vertexs[j];
					vertexs[j] = vt;
					if (j % 6 <= 2)
					{
						mesh.SetUIVertex(vt, (j / 6) * 4 + j % 6);
					}
					if (j % 6 == 4)
					{
						mesh.SetUIVertex(vt, (j / 6) * 4 + j % 6 - 1);
					}
				}
			}

			List<UIVertex> vertexList = new List<UIVertex>();
			mesh.GetUIVertexStream(vertexList);
			applyGradient(vertexList);
			applyOutline(vertexList);
			applyShadow(vertexList);
			mesh.Clear();
			mesh.AddUIVertexTriangleStream(vertexList);
		}
		catch (Exception)
		{

		}
	}

	private void applyShadow(List<UIVertex> vertexList)
	{
		if (effect.shadow)
		{
			ApplyShadow(vertexList, effect.shadowColor, 0, vertexList.Count, effect.shadowDistance.x, effect.shadowDistance.y, effect.shadowUseGraphicAlpha);
		}
	}

	private void applyOutline(List<UIVertex> vertexList)
	{
		if (effect.outline)
		{
			var start = 0;
			var end = vertexList.Count;
			ApplyShadow(vertexList, effect.outlineColor, start, vertexList.Count, effect.outlineDistance.x, effect.outlineDistance.y, effect.outlineUseGraphicAlpha);

			start = end;
			end = vertexList.Count;
			ApplyShadow(vertexList, effect.outlineColor, start, vertexList.Count, effect.outlineDistance.x, -effect.outlineDistance.y, effect.outlineUseGraphicAlpha);

			start = end;
			end = vertexList.Count;
			ApplyShadow(vertexList, effect.outlineColor, start, vertexList.Count, -effect.outlineDistance.x, effect.outlineDistance.y, effect.outlineUseGraphicAlpha);

			start = end;
			end = vertexList.Count;
			ApplyShadow(vertexList, effect.outlineColor, start, vertexList.Count, -effect.outlineDistance.x, -effect.outlineDistance.y, effect.outlineUseGraphicAlpha);
		}
	}

	private void applyGradient(List<UIVertex> vertexList)
	{
		if (effect.gradient && vertexList.Count > 0)
		{
			int count = vertexList.Count;
			float bottomY = vertexList[0].position.y;
			float topY = vertexList[0].position.y;

			for (int i = 1; i < count; i++)
			{
				float y = vertexList[i].position.y;
				if (y > topY)
				{
					topY = y;
				}
				else if (y < bottomY)
				{
					bottomY = y;
				}
			}

			float uiElementHeight = topY - bottomY;

			for (int i = 0; i < count; i++)
			{
				UIVertex uiVertex = vertexList[i];
				uiVertex.color = Color32.Lerp(effect.bottomColor, effect.topColor, (uiVertex.position.y - bottomY) / uiElementHeight);
				vertexList[i] = uiVertex;
			}
		}
	}

	private void ApplyShadow(List<UIVertex> verts, Color32 color, int start, int end, float x, float y, bool useGraphicAlpha)
	{
		UIVertex vt;

		var neededCpacity = verts.Count * 2;
		if (verts.Capacity < neededCpacity)
		{
			verts.Capacity = neededCpacity;
		}

		for (int i = start; i < end; ++i)
		{
			vt = verts[i];
			verts.Add(vt);

			Vector3 v = vt.position;
			v.x += x;
			v.y += y;
			vt.position = v;
			var newColor = color;
			if (useGraphicAlpha)
			{
				newColor.a = (byte)((newColor.a * verts[i].color.a) / 255);
			}
			vt.color = newColor;
			verts[i] = vt;
		}
	}
}
