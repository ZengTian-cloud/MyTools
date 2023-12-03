using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRenderPassFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //// 这里处理pass逻辑
            /*
                // 从对象池中获取cmd
                var cmd = CommandBufferPool.Get("Pass Name");
                // 执行简单地blit操作
                cmd.Blit(null, cameraColorTarget);
                // 调用context 执行我们的cmd
                context.ExecuteCommandBuffer(cmd);
                // 释放cmd
                CommandBufferPool.Release(cmd);
            */
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        // 自定义渲染通道基本参数
        private CustomRenderPassFeatureSettings settings;
        // pass所使用的材质(可动态生成)
        private Material passMaterial;
        // 当前摄像机的渲染对象
        public RenderTargetIdentifier cameraColorTarget; 
        // 构造函数
        public CustomRenderPass(CustomRenderPassFeatureSettings settings)
        {
            this.settings = settings;
            // 设置Pass的渲染时机
            this.renderPassEvent = settings.passEvent;
            if (passMaterial == null && settings.shader != null)
            {
                // 通过此方法创建所需材质(动态)
                passMaterial = CoreUtils.CreateEngineMaterial(settings.shader);
            }
        }
    }

    CustomRenderPass m_ScriptablePass;
    [SerializeField]
    private CustomRenderPassFeatureSettings settings = new CustomRenderPassFeatureSettings();

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(settings);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // 指定渲染器的当前摄像机渲染对象(缓存)
        m_ScriptablePass.cameraColorTarget = renderer.cameraColorTarget;
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


