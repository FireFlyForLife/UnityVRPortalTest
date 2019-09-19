using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace PortalRendering
{
    using RenderQueueType = UnityEngine.Experimental.Rendering.Universal.RenderQueueType;


    public class RenderPortalPass : UnityEngine.Rendering.Universal.ScriptableRenderPass
    {
        RenderQueueType renderQueueType;
        FilteringSettings m_FilteringSettings;
        PortalRenderPassFeature.CustomCameraSettings m_CameraSettings;
        string m_ProfilerTag;
        private PortalRenderContext portalRenderContext;

        public Material overrideMaterial { get; set; }
        public int overrideMaterialPassIndex { get; set; }

        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

        public void SetDetphState(bool writeEnabled, CompareFunction function = CompareFunction.Less)
        {
            m_RenderStateBlock.mask |= RenderStateMask.Depth;
            m_RenderStateBlock.depthState = new DepthState(writeEnabled, function);
        }

        public void SetStencilState(int reference, CompareFunction compareFunction, StencilOp passOp, StencilOp failOp,
            StencilOp zFailOp)
        {
            StencilState stencilState = StencilState.defaultValue;
            stencilState.enabled = true;
            stencilState.SetCompareFunction(compareFunction);
            stencilState.SetPassOperation(passOp);
            stencilState.SetFailOperation(failOp);
            stencilState.SetZFailOperation(zFailOp);

            m_RenderStateBlock.mask |= RenderStateMask.Stencil;
            m_RenderStateBlock.stencilReference = reference;
            m_RenderStateBlock.stencilState = stencilState;
        }

        RenderStateBlock m_RenderStateBlock;

        public RenderPortalPass(string profilerTag, UnityEngine.Rendering.Universal.RenderPassEvent renderPassEvent, string[] shaderTags,
            RenderQueueType renderQueueType, int layerMask, PortalRenderPassFeature.CustomCameraSettings cameraSettings, PortalRenderContext portalContext)
        {

            m_ProfilerTag = profilerTag;
            this.renderPassEvent = renderPassEvent;
            this.renderQueueType = renderQueueType;
            this.overrideMaterial = null;
            this.overrideMaterialPassIndex = 0;
            RenderQueueRange renderQueueRange = (renderQueueType == RenderQueueType.Transparent)
                ? RenderQueueRange.transparent
                : RenderQueueRange.opaque;
            m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
            portalRenderContext = portalContext;

            if (shaderTags != null && shaderTags.Length > 0)
            {
                foreach (var passName in shaderTags)
                    m_ShaderTagIdList.Add(new ShaderTagId(passName));
            }
            else
            {
                m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
                m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
                m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
            }

            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            m_CameraSettings = cameraSettings;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
        }

        private Matrix4x4 clippedProjMat(Matrix4x4 viewMat, Matrix4x4 projMat, Vector3 d_position, Quaternion d_orientation) 
        {

            float dist = d_position.magnitude;
            Vector3 clipPlaneNorm = d_orientation * new Vector3(0.0f, 0.0f, -1.0f);
            Vector4 clipPlane = new Vector4(clipPlaneNorm.x, clipPlaneNorm.y, clipPlaneNorm.z, dist);
	        clipPlane = viewMat.transpose.inverse * clipPlane;

	        if (clipPlane.w > 0.0f)
		        return projMat;

	        Vector4 q = projMat.inverse * new Vector4(
                Mathf.Sign(clipPlane.x),
                Mathf.Sign(clipPlane.y),
                1.0f,
                1.0f
            );
            
            Vector4 c = clipPlane * (2.0f / (Vector4.Dot(clipPlane, q)));

            Matrix4x4 newProj = projMat;
            // third row = clip plane - fourth row
            newProj.SetRow(2, c - newProj.GetRow(3));

	        return newProj;
        }

    public override void Execute(ScriptableRenderContext context, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
            SortingCriteria sortingCriteria = (renderQueueType == RenderQueueType.Transparent)
                ? SortingCriteria.CommonTransparent
                : renderingData.cameraData.defaultOpaqueSortFlags;

            DrawingSettings drawingSettings =
                CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = overrideMaterial;
            drawingSettings.overrideMaterialPassIndex = overrideMaterialPassIndex;

            GraphicsFence portalsRenderedFence;

            Camera camera = renderingData.cameraData.camera;
            float cameraAspect = (float) camera.pixelWidth / (float) camera.pixelHeight;
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            using (new ProfilingSample(cmd, m_ProfilerTag))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                cmd.SetRandomWriteTarget(1, portalRenderContext.computeBuffer);


                if (m_CameraSettings.overrideCamera)
                {
                    //Matrix4x4 projectionMatrix = Matrix4x4.Perspective(m_CameraSettings.cameraFieldOfView, cameraAspect,
                    //    camera.nearClipPlane, camera.farClipPlane);

                    //Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
                    //Vector4 cameraTranslation = viewMatrix.GetColumn(3);
                    //viewMatrix.SetColumn(3, cameraTranslation + m_CameraSettings.offset);

                    //cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                    portalRenderContext.exitPortal = GameObject.FindGameObjectWithTag("ExitPortal");
                    Camera exitCam = portalRenderContext.exitPortal.GetComponentInChildren<Camera>();
                    GameObject entryGO = GameObject.FindGameObjectWithTag("EntryPortal");

                    //Vector3 normal = exitCam.transform.forward;
                    //Vector3 pos = exitCam.transform.position;

                    //Vector4 clipPlaneWorldSpace = new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(normal, pos));
                    //Vector4 clipPlaneCameraSpace = exitCam.worldToCameraMatrix * clipPlaneWorldSpace;

                    Matrix4x4 projectionMatrix = camera.projectionMatrix;//exitCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
                    
                    Matrix4x4 destView = camera.worldToCameraMatrix * entryGO.transform.localToWorldMatrix
                        * Matrix4x4.Rotate(entryGO.transform.rotation)
                        * exitCam.transform.localToWorldMatrix.inverse;
                    //* glm::rotate(glm::mat4(1.0f), 180.0f, glm::vec3(0.0f, 1.0f, 0.0f) * portal.orientation())
                    //* glm::inverse(portal.destination()->modelMat());

                    projectionMatrix = clippedProjMat(destView, projectionMatrix, entryGO.transform.position, entryGO.transform.rotation);


                    //cmd.Clear();
                    cmd.SetViewProjectionMatrices(destView, projectionMatrix);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                }



                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings,
                    ref m_RenderStateBlock);

                //ScriptableCullingParameters param;
                
                //context.Cull()

                if (m_CameraSettings.overrideCamera && m_CameraSettings.restoreCamera)
                {
                    Matrix4x4 projectionMatrix = Matrix4x4.Perspective(camera.fieldOfView, cameraAspect,
                        camera.nearClipPlane, camera.farClipPlane);

                    cmd.Clear();
                    cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, projectionMatrix);

                }
                //portalsRenderedFence = cmd.CreateGraphicsFence(GraphicsFenceType.CPUSynchronisation,
                //    SynchronisationStageFlags.PixelProcessing);
                //if (PortalDebugRenderOverlay.Instance?.renderTarget)
                //{
                //    PortalDebugRenderOverlay.Instance?.renderTarget = new RenderTexture(depthAttachment)
                //}
                //PortalDebugRenderOverlay.Instance?.fullscreenOverlay?.mainTexture = this.depthAttachment;
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            context.Submit();

            //Debug.Log("Fence support:"+ SystemInfo.supportsGraphicsFence);
            //while (!portalsRenderedFence.passed) ;

            //Int32[] perPortalPixelCount = new Int32[255];
            //portalRenderContext.computeBuffer.GetData(perPortalPixelCount, 0, 0, 255);
            //for (int i = 0; i < 5; i++)
            //{
            //    Debug.Log(i + "Portal count: " + perPortalPixelCount[i]);
            //}
            //portalRenderContext.computeBuffer.SetData(new Int32[255]);

            
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
        }
    }
}
