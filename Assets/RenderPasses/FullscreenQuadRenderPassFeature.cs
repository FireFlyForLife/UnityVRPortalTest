using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FullscreenQuadRenderPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public bool RestoreCamera = true;
        public Material material;
        public RenderPassEvent renderPassEvent;
    }

    public Settings settings = new Settings();

    private FullscreenQuadRenderPass renderPass;

    public override void Create()
    {
        renderPass = new FullscreenQuadRenderPass(settings, settings.renderPassEvent, name);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }
}
