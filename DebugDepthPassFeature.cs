using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DebugDepthPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material depthMaterial;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    class DebugDepthPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle cameraTarget;
        private RTHandle tempTarget;
        private string profilerTag = "DebugDepthPass";

        public DebugDepthPass(Material mat, RenderPassEvent evt)
        {
            material = mat;
            renderPassEvent = evt;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cameraTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            // Allocate intermediate target for material pass
            RenderingUtils.ReAllocateIfNeeded(
                ref tempTarget,
                renderingData.cameraData.cameraTargetDescriptor,
                name: "_DebugDepthTemp"
            );

            ConfigureInput(ScriptableRenderPassInput.Depth); // Request depth
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null) return;

            var cmd = CommandBufferPool.Get(profilerTag);

            // Render to temp RT using material
            cmd.SetRenderTarget(cameraTarget, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
            cmd.SetViewport(new Rect(0, 0, cameraTarget.rt.width, cameraTarget.rt.height));
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, 0);


            // Then copy to screen using simple blit
            Blitter.BlitCameraTexture(cmd, tempTarget, cameraTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }


        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            tempTarget?.Release();
        }
    }

    public Settings settings = new Settings();
    private DebugDepthPass depthPass;

    public override void Create()
    {
        depthPass = new DebugDepthPass(settings.depthMaterial, settings.renderPassEvent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(depthPass);
    }
}
