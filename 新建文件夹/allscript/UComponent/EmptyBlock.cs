using UnityEngine;
using UnityEngine.UI;

// ??????
public class EmptyBlock : Graphic, ICanvasRaycastFilter
{
	[HideInInspector]
	public override bool raycastTarget
	{
		get { return true; }
		set { }
	}

	[HideInInspector]
	public override Texture mainTexture
	{
		get { return null; }
	}

	[HideInInspector]
	public override Material materialForRendering
	{
		get { return null; }
	}

	public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		return true;
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
	}

	public override void SetAllDirty() { }

	public override void SetLayoutDirty() { }

	public override void SetVerticesDirty() { }

	public override void SetMaterialDirty() { }
}
