using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipline
{

    public class DrawObjectsRenderPass : RenderPass
    {
        FilteringSettings filteringSettings;
        SortingCriteria sortingCriteria;
        bool isOpaque;
        ShaderTagId shaderTagId;
        public DrawObjectsRenderPass(RenderPassEvent passEvent, bool opaque, RenderQueueRange range, SortingCriteria criteria)
        {
            renderPassEvent = passEvent;
            isOpaque = opaque;
            sortingCriteria = criteria;
            filteringSettings = new FilteringSettings(range);
            shaderTagId = ShaderTags.ForwardBase;
        }

        public override void Render(ScriptableRenderContext context, Camera camera, ref CullingResults cullingResults)
        {
            var sortingSettings = new SortingSettings(camera) { criteria = sortingCriteria };
            var drawSettings = new DrawingSettings(shaderTagId, sortingSettings);

            context.DrawRenderers(cullingResults, ref drawSettings, ref filteringSettings);
        }
    }
}
