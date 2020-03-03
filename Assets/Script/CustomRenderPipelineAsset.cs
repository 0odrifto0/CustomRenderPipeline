
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipline
{
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
}
