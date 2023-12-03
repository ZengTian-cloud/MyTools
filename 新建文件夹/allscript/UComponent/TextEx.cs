using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public enum TextColor
{
	not = 0,
	A = 1,
	B = 2,
	C = 3,
	D = 4,
	E = 5,
	F = 6,
	G = 7,
	H = 8,
	J = 9,
	K = 10,
	L = 11,
	M = 12,
	N = 13,
	O = 14,
	P = 15,
	Q = 16,
	R = 17,
	S = 18,
	T = 19,
}

[RequireComponent(typeof(TextEffect))]
// 自定义文本
public class TextEx : Text
{
	[SerializeField, HideInInspector]
	public TextColor textColor;
	public int VisibleLines = 1;
	public int realFontSize = 1;

	private readonly UIVertex[] _tmpVerts = new UIVertex[4];

	protected override void OnEnable()
	{
		base.OnEnable();
		GetEffect();
	}

	public void GetEffect()
	{
		this.GetComponent<TextEffect>().SetEffect(textColor);
	}

	public void SetCommonColor(int idx)
	{
		textColor = (TextColor)idx;
		SwithcColor();
	}

	public void SwithcColor()
	{

		switch (textColor)
		{
			case TextColor.not:
				break;
			case TextColor.A:
				color = GetColor("#6F594B");
				GetEffect();
				break;
			case TextColor.B:
				color = GetColor("#834829");
				GetEffect();
				break;
			case TextColor.C:
				color = GetColor("#616483");
				GetEffect();
				break;
			case TextColor.D:
				color = GetColor("#434166");
				GetEffect();
				break;
			case TextColor.E:
				color = GetColor("#f3f0e4");
				GetEffect();
				break;
			case TextColor.F:
				color = GetColor("#f3f0e4");
				GetEffect();
				break;
			case TextColor.G:
				color = GetColor("#f3f0e4");
				GetEffect();
				break;
			case TextColor.H:
				color = GetColor("#f3f0e4");
				GetEffect();
				break;
			case TextColor.J:
				color = GetColor("#f3f0e4");
				GetEffect();
				break;
			case TextColor.K:
				color = GetColor("#f3f0e4");
				GetEffect();
				break;
			case TextColor.L:
				color = GetColor("#f3f0e4");
				GetEffect();
				break;
			case TextColor.M:
				color = GetColor("#2da847");
				GetEffect();
				break;
			case TextColor.Q:
				color = GetColor("#646464");
				GetEffect();
				break;
			case TextColor.S:
				color = GetColor("#f3f0e4");
				GetEffect();
				break;
			case TextColor.T:
				color = GetColor("#ff5353");
				GetEffect();
				break;
		}
	}

	public Color GetColor(string colorstr)
	{
		Color nowColor;
		ColorUtility.TryParseHtmlString(colorstr, out nowColor);
		return nowColor;
	}

	public string getfixtext()
	{
		var sb = new StringBuilder();
		int curlen = text.Length;
		string linestr = string.Empty;
		var vlineinfo = cachedTextGenerator.GetLinesArray();
		for (var i = vlineinfo.Length - 1; i >= 0; i--)
		{
			for (var j = vlineinfo[i].startCharIdx; j < curlen; j++)
			{
				linestr = linestr + text[j];
			}
			curlen = vlineinfo[i].startCharIdx;
			if (sb.Length > 0)
			{
				sb.Insert(0, "\n");
			}
			sb.Insert(0, linestr);
			linestr = string.Empty;
		}
		return sb.ToString();
	}

	protected void _UseFitSettings(string temtext, bool bcheck)
	{
		TextGenerationSettings texsettings = GetGenerationSettings(rectTransform.rect.size);
		texsettings.resizeTextForBestFit = false;

		if (resizeTextForBestFit)
		{
			int minSize = resizeTextMinSize;
			int txtLen = temtext.Length;
			for (int i = resizeTextMaxSize; i >= minSize; --i)
			{
				texsettings.fontSize = i;
				cachedTextGenerator.Populate(temtext, texsettings);
				if (cachedTextGenerator.characterCountVisible == txtLen)
				{
					break;
				}
			}
		}
		else
		{
			cachedTextGenerator.Populate(temtext, texsettings);
		}
		realFontSize = texsettings.fontSize;

#if UNITY_EDITOR
		if (bcheck &&
			cachedTextGenerator.characterCountVisible < temtext.Length &&
			(!GetComponent<ContentSizeFitter>() && !GetComponent<ContentSizeFilterChild>()))
		{
			var sb = new System.Text.StringBuilder();
			var ttrans = transform;
			while (ttrans != null)
			{
				sb.Insert(0, "/" + ttrans.name);
				ttrans = ttrans.parent;
			}
			sb.Append("\n");
			sb.Append(temtext);
			zxlogger.logerror("文本展示不全: " + sb.ToString());
		}
#endif
	}

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		if (null == font)
		{
			return;
		}

		m_DisableFontTextureRebuiltCallback = true;
		_UseFitSettings(text, true);
		Rect trect = rectTransform.rect;

		var temAnchorPivot = GetTextAnchorPivot(alignment);
		var zerovec = Vector2.zero;
		zerovec.x = Mathf.Lerp(trect.xMin, trect.xMax, temAnchorPivot.x);
		zerovec.y = Mathf.Lerp(trect.yMin, trect.yMax, temAnchorPivot.y);
		var vecadjust = PixelAdjustPoint(zerovec) - zerovec;
		var listvert = cachedTextGenerator.verts;
		float num1 = 1f / pixelsPerUnit;
		int num2 = listvert.Count;
		toFill.Clear();
		if (vecadjust != Vector2.zero)
		{
			for (int index1 = 0; index1 < num2; ++index1)
			{
				int index2 = index1 & 3;
				_tmpVerts[index2] = listvert[index1];
				_tmpVerts[index2].position *= num1;
				_tmpVerts[index2].position.x += vecadjust.x;
				_tmpVerts[index2].position.y += vecadjust.y;
				if (index2 == 3)
				{
					toFill.AddUIVertexQuad(this._tmpVerts);
				}
			}
		}
		else
		{
			for (int index1 = 0; index1 < num2; ++index1)
			{
				int index2 = index1 & 3;
				_tmpVerts[index2] = listvert[index1];
				_tmpVerts[index2].position *= num1;
				if (index2 == 3)
				{
					toFill.AddUIVertexQuad(_tmpVerts);
				}
			}
		}
		m_DisableFontTextureRebuiltCallback = false;
		VisibleLines = cachedTextGenerator.lineCount;
	}
}
