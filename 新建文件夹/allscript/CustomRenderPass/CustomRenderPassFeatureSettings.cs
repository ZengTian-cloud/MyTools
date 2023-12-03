using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class CustomRenderPassFeatureSettings
{
    // Pass在渲染管线中的执行位置，这里默认设置为"BeforeRenderingPostProcessing"
    public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    // 所用Shader
    public Shader shader; 
}