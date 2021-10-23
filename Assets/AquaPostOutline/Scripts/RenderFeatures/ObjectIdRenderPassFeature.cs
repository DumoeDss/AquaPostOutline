using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace AquaEffects.AquaPostOutline
{
    public class ObjectIdRenderPassFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Setting
        {
            public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingOpaques;
            public LayerMask layermask;
        }
        public Setting setting = new Setting();
        class CustomRenderPass : ScriptableRenderPass
        {
            public int soildColorID = 0;
            public ShaderTagId shaderTag = new ShaderTagId("UniversalForward");
            public Setting setting;

            FilteringSettings filtering;
            Material material;

            public CustomRenderPass(Setting setting, Material material)
            {
                this.setting = setting;
                this.material = material;

                RenderQueueRange queue = new RenderQueueRange();
                queue.lowerBound = 2000;
                queue.upperBound = 3000;
                
                filtering = new FilteringSettings(queue, setting.layermask);
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                int temp = Shader.PropertyToID("_ObjectIdTex");
                RenderTextureDescriptor desc = cameraTextureDescriptor;
                cmd.GetTemporaryRT(temp, desc);
                soildColorID = temp;
                ConfigureTarget(temp);
                ConfigureClear(ClearFlag.All, Color.black);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var draw = CreateDrawingSettings(shaderTag, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
                draw.overrideMaterial = material;
                draw.overrideMaterialPassIndex = 0;
                context.DrawRenderers(renderingData.cullResults, ref draw, ref filtering);
            }

        }

        CustomRenderPass m_ScriptablePass;

        [SerializeField]
        Shader shader;

        Material material;

        public override void Create()
        {
#if UNITY_EDITOR
            foreach (var guid in UnityEditor.AssetDatabase.FindAssets("ObjectID t:Shader"))
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Shaders/ObjectID"))
                {
                    shader = UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(path);
                    break;
                }
            }
#endif
            if (material == null)
            {
                material = CoreUtils.CreateEngineMaterial(shader);
            }
            if (material == null)
                Debug.LogError("Error,material is Null!");

            m_ScriptablePass = new CustomRenderPass(setting, material);

            m_ScriptablePass.renderPassEvent = setting.passEvent;
        }


        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}


