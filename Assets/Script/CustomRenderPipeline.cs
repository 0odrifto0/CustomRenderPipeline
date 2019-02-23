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

        var cmd = new CommandBuffer();
        cmd.ClearRenderTarget(true, true, Color.gray);
        renderContext.ExecuteCommandBuffer(cmd);
        cmd.Release();
        renderContext.Submit();
    }
}
