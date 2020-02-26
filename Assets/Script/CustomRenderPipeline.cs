using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("SRP/Create Custom Render Pipeline")]
    static void CreatePipelineAsset()
    {
        var inst = ScriptableObject.CreateInstance<CustomRenderPipelineAsset>();
        UnityEditor.AssetDatabase.CreateAsset(inst, "Assets/SRP/CustomRenderPipeline.asset");
    }
#endif

    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline();
    }
}

public class CustomRenderPipeline : RenderPipeline
{
    protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
    {
        BeginFrameRendering(renderContext, cameras);

        foreach (var camera in cameras)
        {
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

            // Setting
            var sortSetting = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
            var drawSetting = new DrawingSettings(new ShaderTagId("BasePass"), sortSetting);
            var filterSetting = new FilteringSettings(RenderQueueRange.opaque);

            renderContext.DrawRenderers(cullingResults, ref drawSetting, ref filterSetting);

            renderContext.Submit();

            EndCameraRendering(renderContext, camera);
        }

        EndFrameRendering(renderContext, cameras);
    }
}
