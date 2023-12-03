using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class ContentSizeFilterChild : UIBehaviour, ILayoutSelfController
{
	public enum FitMode
	{
		Unconstrained,
		MinSize,
		PreferredSize
	}

	[SerializeField] protected FitMode m_HorizontalFit = FitMode.Unconstrained;
	public FitMode horizontalFit { get { return m_HorizontalFit; } set { m_HorizontalFit = value; SetDirty(); } }

	[SerializeField] protected FitMode m_VerticalFit = FitMode.Unconstrained;
	public FitMode verticalFit { get { return m_VerticalFit; } set { m_VerticalFit = value; SetDirty(); } }

	[SerializeField] protected float m_MaxSize = 100;
	public float maxSize { get { return m_MaxSize; } set { m_MaxSize = value; SetDirty(); } }

	[System.NonSerialized] private RectTransform m_Rect;
	private RectTransform rectTransform
	{
		get
		{
			if (m_Rect == null)
				m_Rect = GetComponent<RectTransform>();
			return m_Rect;
		}
	}

	private DrivenRectTransformTracker m_Tracker;

	protected ContentSizeFilterChild() { }

	protected override void OnEnable()
	{
		base.OnEnable();
		SetDirty();
	}

	protected override void OnDisable()
	{
		m_Tracker.Clear();
		LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		base.OnDisable();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		SetDirty();
	}

	private void HandleSelfFittingAlongAxis(int axis)
	{
		FitMode fitting = (axis == 0 ? horizontalFit : verticalFit);
		if (fitting == FitMode.Unconstrained)
		{
			m_Tracker.Add(this, rectTransform, DrivenTransformProperties.None);
			return;
		}

		m_Tracker.Add(this, rectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

		if (fitting == FitMode.MinSize)
		{
			var size = Mathf.Min(LayoutUtility.GetMinSize(m_Rect, axis), m_MaxSize);
			rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, size);
		}
		else
		{
			var size = Mathf.Min(LayoutUtility.GetPreferredSize(m_Rect, axis), m_MaxSize);
			rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, size);
		}
	}

	public virtual void SetLayoutHorizontal()
	{
		m_Tracker.Clear();
		HandleSelfFittingAlongAxis(0);
	}

	public virtual void SetLayoutVertical()
	{
		m_Tracker.Clear();
		HandleSelfFittingAlongAxis(1);
	}

	protected void SetDirty()
	{
		if (!IsActive())
			return;
		LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
	}

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		SetDirty();
	}
#endif
}
