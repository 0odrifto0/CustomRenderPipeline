using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

namespace CustomRenderPipline
{
    public class CustomRenderPipeline : RenderPipeline
    {
        private List<RenderPass> renderPasses;

        public CustomRenderPipeline()
        {
            renderPasses = new List<RenderPass>();
            renderPasses.Add(new DrawObjectsRenderPass(RenderPassEvent.BeforeRenderingOpaques, true, RenderQueueRange.opaque, SortingCriteria.CommonOpaque));
            renderPasses.Add(new SkyBoxRenderPass(RenderPassEvent.BeforeRenderingSkybox));
            renderPasses.Add(new DrawObjectsRenderPass(RenderPassEvent.BeforeRenderingTransparents, false, RenderQueueRange.transparent, SortingCriteria.CommonTransparent));
        }

        protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
        {
            BeginFrameRendering(renderContext, cameras);

            SortCameras(cameras);

            for (int index = 0; index < cameras.Length; ++index)
            {
                var camera = cameras[index];
                BeginCameraRendering(renderContext, camera);

                // clear target
                var cmd = CommandBufferPool.Get();
                cmd.ClearRenderTarget(true, false, Color.black);
                renderContext.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);

                // Setup Camera
                renderContext.SetupCameraProperties(camera);

                // Culling
                ScriptableCullingParameters cullParams;
                if (!camera.TryGetCullingParameters(out cullParams))
                    continue;

                var cullingResults = renderContext.Cull(ref cullParams);

                // render
                for (int passIndex = 0; passIndex < renderPasses.Count; ++passIndex)
                {
                    var renderPass = renderPasses[passIndex];
                    renderPass.Render(renderContext, camera, ref cullingResults);
                }

                //draw gizmos
                /*#if UNITY_EDITOR
                            if (UnityEditor.Handles.ShouldRenderGizmos())
                                renderContext.DrawGizmos(camera, GizmoSubset.PostImageEffects);
                #endif*/

                renderContext.Submit();

                EndCameraRendering(renderContext, camera);
            }

            EndFrameRendering(renderContext, cameras);
        }

        private void SortCameras(Camera[] cameras)
        {
            Array.Sort(cameras, (a, b) =>
            {
                return (int)a.depth - (int)b.depth;
            });
        }
    }
}