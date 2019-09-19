using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullscreenQuadRenderPass : ScriptableRenderPass
{
    string profilerTag;
    FullscreenQuadRenderPassFeature.Settings settings;

    public FullscreenQuadRenderPass(FullscreenQuadRenderPassFeature.Settings settings, RenderPassEvent renderPassEvent, string profilerTag)
    {
        this.settings = settings;
        this.renderPassEvent = renderPassEvent;
        this.profilerTag = profilerTag;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmdBuffer = CommandBufferPool.Get(profilerTag);

        Camera camera = renderingData.cameraData.camera;
    
        cmdBuffer.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
        Mesh fullscreenQuad = RenderingUtils.fullscreenMesh;
        cmdBuffer.DrawMesh(fullscreenQuad, Matrix4x4.identity, settings.material);
        cmdBuffer.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
        
        context.ExecuteCommandBuffer(cmdBuffer);
        CommandBufferPool.Release(cmdBuffer);
    }
}
