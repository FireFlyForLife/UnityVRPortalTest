using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;


namespace PortalRendering
{
    public class PortalRenderContext : IDisposable
    {
        public ComputeBuffer computeBuffer;

        public void Dispose()
        {
            computeBuffer?.Dispose();
            computeBuffer = null;
        }
    }

    public class PortalRenderPassFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class RenderObjectsSettings
        {
            public string passTag = "RenderObjectsFeature";
            public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

            public FilterSettings filterSettings = new FilterSettings();

            public Material overrideMaterial = null;
            public int overrideMaterialPassIndex = 0;

            public bool overrideDepthState = false;
            public CompareFunction depthCompareFunction = CompareFunction.LessEqual;
            public bool enableWrite = true;

            public StencilStateData stencilSettings = new StencilStateData();

            public CustomCameraSettings cameraSettings = new CustomCameraSettings();
        }

        [System.Serializable]
        public class FilterSettings
        {
            // TODO: expose opaque, transparent, all ranges as drop down
            public RenderQueueType RenderQueueType;
            public LayerMask LayerMask;
            public string[] PassNames;

            public FilterSettings()
            {
                RenderQueueType = RenderQueueType.Opaque;
                LayerMask = 0;
            }
        }

        [System.Serializable]
        public class CustomCameraSettings
        {
            public bool overrideCamera = false;
            public bool restoreCamera = true;
            public Vector4 offset;
            public float cameraFieldOfView = 60.0f;
        }

        public RenderObjectsSettings settings = new RenderObjectsSettings();

        PortalRenderContext portalRenderContext;
        SetupPortalBufferRenderPass setupPortalBufferRenderPass;
        RenderPortalPass renderPortalPass;

        public override void Create()
        {
            portalRenderContext?.Dispose();
            portalRenderContext = new PortalRenderContext();

            FilterSettings filter = settings.filterSettings;
            renderPortalPass = new RenderPortalPass(settings.passTag, settings.Event, filter.PassNames,
                filter.RenderQueueType, filter.LayerMask, settings.cameraSettings, portalRenderContext);

            renderPortalPass.overrideMaterial = settings.overrideMaterial;
            renderPortalPass.overrideMaterialPassIndex = settings.overrideMaterialPassIndex;

            if (settings.overrideDepthState)
                renderPortalPass.SetDetphState(settings.enableWrite, settings.depthCompareFunction);

            if (settings.stencilSettings.overrideStencilState)
                renderPortalPass.SetStencilState(settings.stencilSettings.stencilReference,
                    settings.stencilSettings.stencilCompareFunction, settings.stencilSettings.passOperation,
                    settings.stencilSettings.failOperation, settings.stencilSettings.zFailOperation);

            setupPortalBufferRenderPass = new SetupPortalBufferRenderPass("SetupPortalBuffer", portalRenderContext);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(renderPortalPass);
            renderer.EnqueuePass(setupPortalBufferRenderPass);
        }
    }
}
