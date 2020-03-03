using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipline
{
    public class SkyBoxRenderPass : RenderPass
    {
        public SkyBoxRenderPass(RenderPassEvent passEvent)
        {
            renderPassEvent = passEvent;
        }

        public override void Render(ScriptableRenderContext context, Camera camera, ref CullingResults cullingResults)
        {
            context.DrawSkybox(camera);
        }
    }
}
