using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
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
    protected override IRenderPipeline InternalCreatePipeline()
    {
        return new CustomRenderPipeline();
    }
}

public class CustomRenderPipeline : RenderPipeline
{
    public override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
    {
        base.Render(renderContext, cameras);

        foreach (var camera in cameras)
        {
            // Culling
            ScriptableCullingParameters cullParams;
            if (!CullResults.GetCullingParameters(camera, out cullParams))
                continue;

            var cmd = CommandBufferPool.Get();
            cmd.ClearRenderTarget(true, false, Color.black);
            renderContext.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            CullResults cullResults = new CullResults();
            CullResults.Cull(ref cullParams, renderContext, ref cullResults);

            // Setup Camera
            renderContext.SetupCameraProperties(camera);

            // Filter 
            var opaqueRange = new FilterRenderersSettings(true) { renderQueueRange = RenderQueueRange.opaque };

            // Render setting
            var drs = new DrawRendererSettings(camera, new ShaderPassName("BasePass"));
            drs.sorting.flags = SortFlags.CommonOpaque;
            renderContext.DrawRenderers(cullResults.visibleRenderers, ref drs, opaqueRange);

            renderContext.Submit();
        }
    }
}
