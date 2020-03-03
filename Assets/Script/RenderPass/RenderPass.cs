using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipline
{
    public abstract class RenderPass
    {
        protected RenderPassEvent renderPassEvent;
        public RenderPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRendering;
        }

        public abstract void Render(ScriptableRenderContext context, Camera camera, ref CullingResults cullingResults);
    }
}
