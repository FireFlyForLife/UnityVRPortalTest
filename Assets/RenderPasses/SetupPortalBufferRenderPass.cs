﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace PortalRendering
{
    public class SetupPortalBufferRenderPass : UnityEngine.Rendering.Universal.ScriptableRenderPass
    {
        private PortalRenderContext portalRenderContext;
        private String m_ProfilerTag;

        public SetupPortalBufferRenderPass(string profilerTag, PortalRenderContext portalRenderContext)
        {
            m_ProfilerTag = profilerTag;
            this.renderPassEvent = UnityEngine.Rendering.Universal.RenderPassEvent.BeforeRendering;

            this.portalRenderContext = portalRenderContext;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);

            //portalRenderContext.computeBuffer = new ComputeBuffer(255, sizeof(int));
            //portalRenderContext.computeBuffer.name = "perPortalPixelCountBuffer";
        }

        public override void Execute(ScriptableRenderContext context, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            using (new ProfilingSample(cmd, m_ProfilerTag))
            {
                cmd.SetRandomWriteTarget(1, portalRenderContext.computeBuffer);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);

            //portalRenderContext.Dispose();
        }
    }
}
