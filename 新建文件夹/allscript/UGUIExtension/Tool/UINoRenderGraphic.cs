using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 不渲染 剔除渲染信息 用做空的Collider点击检测器
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class UINoRenderGraphic : MaskableGraphic
{

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }

}
