using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Sprites;
using UnityEngine.UI;

// 序列帧
[ExecuteInEditMode]
public class ImageAnimation : MaskableGraphic, ISerializationCallbackReceiver, ILayoutElement, ICanvasRaycastFilter
{
	[SerializeField]
	[FormerlySerializedAs("all_m_Frame")]
	private Sprite[] sprites;
	public Sprite[] Sprites { get { return sprites; } set { sprites = value; } }

	[SerializeField]
	[FormerlySerializedAs("framerate")]
	private int framerate = 20;
	public int Framerate { get { return framerate; } set { framerate = value; } }

	[SerializeField]
	[FormerlySerializedAs("m_isLoop")]
	private bool m_isLoop = true;
	public bool mIsLoop { get { return m_isLoop; } set { m_isLoop = value; } }

	[SerializeField]
	[FormerlySerializedAs("m_autohide")]
	private bool m_autohide = true;
	public bool mAutoHide { get { return m_autohide; } set { m_autohide = value; } }

	[SerializeField]
	[FormerlySerializedAs("m_queue")]
	private bool m_queue = true;
	public bool mQueue { get { return m_queue; } set { m_queue = value; } }

	[SerializeField]
	[FormerlySerializedAs("ignore_TimeScale")]
	private bool m_ignoreTimeScale = true;
	public bool IgnoreTimeScale { get { return m_ignoreTimeScale; } set { m_ignoreTimeScale = value; } }

	[SerializeField]
	private float m_Index = 0;
	public float mIndex { get { return m_Index; } set { m_Index = value; } }

	public Action overcb;

	private bool isOver = false;
	private float mUpdate = 0f;

	private Sprite m_Sprite;
	public Sprite mSprite
	{
		get { return m_Sprite; }
		set
		{
			m_Sprite = value;
			SetAllDirty();
		}
	}

	[SerializeField]
	private bool preserveAspect = false;
	public bool PreserveAspect
	{
		get { return preserveAspect; }
		set
		{
			if (SetStruct(ref preserveAspect, value))
			{
				SetVerticesDirty();
			}
		}
	}

	// Not serialized until we support read-enabled sprites better.
	private float m_EventAlphaThreshold = 1;
	public float eventAlphaThreshold
	{
		get
		{

			return m_EventAlphaThreshold;
		}
		set
		{
			m_EventAlphaThreshold = value;
		}
	}

	protected ImageAnimation()
	{

	}

	/// <summary>
	/// Image's texture comes from the UnityEngine.Image.
	/// </summary>
	public override Texture mainTexture
	{
		get
		{
			if (mSprite != null)
			{
				return mSprite != null ? mSprite.texture :
					(sprites == null || sprites.Length == 0 ? s_WhiteTexture : sprites[0].texture);
			}
			return null;
		}
	}

	public void setColor(Color newColor)
	{
		base.color = newColor;
	}

	/// <summary>
	/// Whether the Image has a border to work with.
	/// </summary>
	public bool hasBorder
	{
		get
		{
			if (mSprite != null)
			{
				Vector4 v = mSprite.border;
				return v.sqrMagnitude > 0f;
			}
			return false;
		}
	}

	public float pixelsPerUnit
	{
		get
		{
			float spritePixelsPerUnit = 100;
			if (mSprite)
			{
				spritePixelsPerUnit = mSprite.pixelsPerUnit;
			}

			float referencePixelsPerUnit = 100;
			if (canvas)
			{
				referencePixelsPerUnit = canvas.referencePixelsPerUnit;
			}

			return spritePixelsPerUnit / referencePixelsPerUnit;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		mIndex = 0;
		isOver = false;
		mUpdate = 0;
		if (Sprites == null || Sprites.Length == 0)
		{
			if (mAutoHide)
			{
				gameObject.SetActive(false);
			}
			mSprite = null;
			isOver = true;
			overcb?.Invoke();
			return;
		}
		mSprite = Sprites[0];
	}

	void Update()
	{
		if (!isOver && framerate != 0)
		{
			float dttime = IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
			mUpdate += dttime;
			float frametime = Mathf.Abs(1f / framerate);
			if (mUpdate > frametime)
			{
				while (!isOver && mUpdate > frametime)
				{
					mUpdate -= mQueue ? mUpdate : frametime;
					m_Index = RepeatIndex(framerate > 0 ? m_Index + 1 : m_Index - 1, Sprites.Length);

					if (!mIsLoop && m_Index == Sprites.Length - 1)
					{
						if (mAutoHide)
						{
							gameObject.SetActive(false);
						}
						mUpdate = 0;
						isOver = true;
						overcb?.Invoke();
					}
				}
				mSprite = Sprites[Mathf.FloorToInt(m_Index)];
			}
		}
	}

	private float RepeatIndex(float val, float max)
	{
		if (max < 1)
		{
			return 0;
		}
		while (val < 0)
		{
			val += max;
		}
		while (val >= max)
		{
			val -= max;
		}
		return val;
	}

	public virtual void OnBeforeSerialize()
	{

	}

	public virtual void OnAfterDeserialize()
	{

	}

	/// Image's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
	private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
	{
		var padding = mSprite == null ? Vector4.zero : DataUtility.GetPadding(mSprite);
		var size = mSprite == null ? Vector2.zero : new Vector2(mSprite.rect.width, mSprite.rect.height);

		Rect r = GetPixelAdjustedRect();
		// CLog.Log(string.Format("r:{2}, size:{0}, padding:{1}", size, padding, r));

		int spriteW = Mathf.RoundToInt(size.x);
		int spriteH = Mathf.RoundToInt(size.y);

		var v = new Vector4(
			padding.x / spriteW,
			padding.y / spriteH,
			(spriteW - padding.z) / spriteW,
			(spriteH - padding.w) / spriteH
		);

		if (shouldPreserveAspect && size.sqrMagnitude > 0.0f)
		{
			var spriteRatio = size.x / size.y;
			var rectRatio = r.width / r.height;

			if (spriteRatio > rectRatio)
			{
				var oldHeight = r.height;
				r.height = r.width * (1.0f / spriteRatio);
				r.y += (oldHeight - r.height) * rectTransform.pivot.y;
			}
			else
			{
				var oldWidth = r.width;
				r.width = r.height * spriteRatio;
				r.x += (oldWidth - r.width) * rectTransform.pivot.x;
			}
		}

		v = new Vector4(
			r.x + r.width * v.x,
			r.y + r.height * v.y,
			r.x + r.width * v.z,
			r.y + r.height * v.w
		);

		return v;
	}

	public override void SetNativeSize()
	{
		if (mSprite != null)
		{
			float w = mSprite.rect.width / pixelsPerUnit;
			float h = mSprite.rect.height / pixelsPerUnit;
			rectTransform.anchorMax = rectTransform.anchorMin;
			rectTransform.sizeDelta = new Vector2(w, h);
			SetAllDirty();
		}
	}

	/// <summary>
	/// Update the UI renderer mesh.
	/// </summary>
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		if (mSprite == null)
		{
			base.OnPopulateMesh(vh);
			return;
		}
		GenerateSimpleSprite(vh, preserveAspect);
	}

	#region Various fill functions
	/// <summary>
	/// Generate vertices for a simple Image.
	/// </summary>
	void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect)
	{
		Vector4 v = GetDrawingDimensions(lPreserveAspect);
		var uv = (mSprite != null) ? DataUtility.GetOuterUV(mSprite) : Vector4.zero;

		var color32 = color;
		vh.Clear();
		vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
		vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
		vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
		vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));

		vh.AddTriangle(0, 1, 2);
		vh.AddTriangle(2, 3, 0);
	}

	Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
	{
		for (int axis = 0; axis <= 1; axis++)
		{
			// If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
			// In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
			float combinedBorders = border[axis] + border[axis + 2];
			if (rect.size[axis] < combinedBorders && combinedBorders != 0)
			{
				float borderScaleRatio = rect.size[axis] / combinedBorders;
				border[axis] *= borderScaleRatio;
				border[axis + 2] *= borderScaleRatio;
			}
		}
		return border;
	}
	#endregion

	public virtual void CalculateLayoutInputHorizontal()
	{

	}
	public virtual void CalculateLayoutInputVertical()
	{

	}

	public virtual float minWidth
	{
		get { return 0; }
	}

	public virtual float preferredWidth
	{
		get
		{
			if (mSprite == null)
			{
				return 0;
			}
			return mSprite.rect.size.x / pixelsPerUnit;
		}
	}

	public virtual float flexibleWidth
	{
		get { return -1; }
	}

	public virtual float minHeight
	{
		get { return 0; }
	}

	public virtual float preferredHeight
	{
		get
		{
			if (mSprite == null)
			{
				return 0;
			}
			return mSprite.rect.size.y / pixelsPerUnit;
		}
	}

	public virtual float flexibleHeight
	{
		get { return -1; }
	}

	public virtual int layoutPriority
	{
		get { return 0; }
	}

	public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		if (m_EventAlphaThreshold >= 1)
		{
			return true;
		}

		if (mSprite == null)
		{
			return true;
		}

		Vector2 local;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out local);

		Rect rect = GetPixelAdjustedRect();

		// Convert to have lower left corner as reference point.
		local.x += rectTransform.pivot.x * rect.width;
		local.y += rectTransform.pivot.y * rect.height;

		local = MapCoordinate(local, rect);

		// Normalize local coordinates.
		Rect spriteRect = mSprite.textureRect;
		Vector2 normalized = new Vector2(local.x / spriteRect.width, local.y / spriteRect.height);

		// Convert to texture space.
		float x = Mathf.Lerp(spriteRect.x, spriteRect.xMax, normalized.x) / mSprite.texture.width;
		float y = Mathf.Lerp(spriteRect.y, spriteRect.yMax, normalized.y) / mSprite.texture.height;

		try
		{
			return mSprite.texture.GetPixelBilinear(x, y).a >= m_EventAlphaThreshold;
		}
		catch (UnityException e)
		{
			Debug.LogError("Using clickAlphaThreshold lower than 1 on Image whose sprite texture cannot be read. " + e.Message + " Also make sure to disable sprite packing for this sprite.", this);
			return true;
		}
	}

	private Vector2 MapCoordinate(Vector2 local, Rect rect)
	{
		Rect spriteRect = mSprite.rect;
		return new Vector2(local.x * spriteRect.width / rect.width, local.y * spriteRect.height / rect.height);
	}

	public static bool SetColor(ref Color currentValue, Color newValue)
	{
		if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a)
		{
			return false;
		}

		currentValue = newValue;
		return true;
	}

	public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
	{
		if (currentValue.Equals(newValue))
		{
			return false;
		}

		currentValue = newValue;
		return true;
	}

	public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
	{
		if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
		{
			return false;
		}

		currentValue = newValue;
		return true;
	}
}