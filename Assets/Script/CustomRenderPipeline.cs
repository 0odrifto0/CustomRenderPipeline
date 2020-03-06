using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using Unity.Collections;

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

                SetupMainLightConstants(renderContext, ref cullingResults);

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

        private void SetupMainLightConstants(ScriptableRenderContext renderContext, ref CullingResults cullingResults)
        {
            int index = GetMainLightIndex(cullingResults.visibleLights);
            if (index == -1)
                return;

            VisibleLight lightData = cullingResults.visibleLights[index];
            Vector3 lightPos;
            Color lightColor;

            if (lightData.lightType == LightType.Directional)
            {
                Vector4 dir = -lightData.localToWorldMatrix.GetColumn(2);
                lightPos = new Vector4(dir.x, dir.y, dir.z, 0.0f);
            }
            else
            {
                Vector4 pos = lightData.localToWorldMatrix.GetColumn(3);
                lightPos = new Vector4(pos.x, pos.y, pos.z, 1.0f);
            }

            // VisibleLight.finalColor already returns color in active color space
            lightColor = lightData.finalColor;

            var cmd = CommandBufferPool.Get();
            cmd.SetGlobalVector("_MainLightPosition", lightPos);
            cmd.SetGlobalVector("_MainLightColor", lightColor);
            renderContext.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

        }

        private int GetMainLightIndex(NativeArray<VisibleLight> visibleLights)
        {
            int totalVisibleLights = visibleLights.Length;

            if (totalVisibleLights == 0)
                return -1;

            Light sunLight = RenderSettings.sun;
            int brightestDirectionalLightIndex = -1;
            float brightestLightIntensity = 0.0f;
            for (int i = 0; i < totalVisibleLights; ++i)
            {
                VisibleLight currVisibleLight = visibleLights[i];
                Light currLight = currVisibleLight.light;

                // Particle system lights have the light property as null. We sort lights so all particles lights
                // come last. Therefore, if first light is particle light then all lights are particle lights.
                // In this case we either have no main light or already found it.
                if (currLight == null)
                    break;

                if (currLight == sunLight)
                    return i;

                // In case no shadow light is present we will return the brightest directional light
                if (currVisibleLight.lightType == LightType.Directional && currLight.intensity > brightestLightIntensity)
                {
                    brightestLightIntensity = currLight.intensity;
                    brightestDirectionalLightIndex = i;
                }
            }

            return brightestDirectionalLightIndex;
        }
    }
}