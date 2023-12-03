using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RD_ParticleClip : UIBehaviour, IClippable
{
    [SerializeField]
    private Renderer[] renderers; // 支持多个粒子特效同时设置
    static Vector3[] wcs = new Vector3[2];
    private static MaterialPropertyBlock block;
    private RectMask2D m_ParentMask = null;

    　　// 由于美术在特效中使用的Transform, 故取父结点的RectTransform
    public RectTransform rectTransform => transform.parent as RectTransform;

    protected override void Awake()
    {
        ParticleSystem[] particleSystems = this.GetComponentsInChildren<ParticleSystem>(true);
        if (particleSystems.Length > 0)
        {
            renderers = new Renderer[particleSystems.Length];
            for (int i = 0; i < particleSystems.Length; ++i)
            {
                renderers[i] = particleSystems[i].GetComponent<Renderer>();
            }
        }

        if (block == null)
            block = new MaterialPropertyBlock();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (rectTransform != null)
            UpdateClipParent();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (rectTransform != null)
            UpdateClipParent();
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();

        if (!isActiveAndEnabled)
            return;

        if (rectTransform != null)
            UpdateClipParent();
    }

    // 代码来自UISystem/MaskableGraphic
    private void UpdateClipParent()
    {
        var newParent = IsActive() ? MaskUtilities.GetRectMaskForClippable(this) : null;

        // if the new parent is different OR is now inactive
        if (m_ParentMask != null && (newParent != m_ParentMask || !newParent.IsActive()))
            m_ParentMask.RemoveClippable(this);

        // don't re-add it if the newparent is inactive
        if (newParent != null && newParent.IsActive())
            newParent.AddClippable(this);

        m_ParentMask = newParent;
    }

    public void RecalculateClipping()
    {
        //Debug.Log("RecalculateClipping:" + gameObject.name);
    }

    public void Cull(Rect clipRect, bool validRect)
    {
        //Debug.Log("Cull:" + gameObject.name);
    }

    public void SetClipRect(Rect value, bool validRect)
    {
        wcs[0] = new Vector3(value.xMin, value.yMin, 0);
        wcs[1] = new Vector3(value.xMax, value.yMax, 0);

        var rootCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
        if (rootCanvas != null)
        {　　　　　　　// 坐标系转换
            Matrix4x4 mat = rootCanvas.transform.localToWorldMatrix;
            for (int i = 0; i < 2; ++i)
                wcs[i] = mat.MultiplyPoint(wcs[i]);

            for (int i = 0; i < renderers.Length; ++i)
            {
                Renderer render = renderers[i];
                render.GetPropertyBlock(block);
                block.SetVector("_ClipRect", new Vector4(wcs[0].x, wcs[0].y, wcs[1].x, wcs[1].y));
                block.SetFloat("_UseClipRect", 1);
                render.SetPropertyBlock(block);
            }
        }
    }

    public void SetClipSoftness(Vector2 clipSoftness)
    {
        throw new System.NotImplementedException();
    }
}