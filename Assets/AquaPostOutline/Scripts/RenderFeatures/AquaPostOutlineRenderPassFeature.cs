using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace AquaEffects.AquaPostOutline
{
    public class AquaPostOutlineRenderPassFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class AquaPostOutlineSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public AquaPostOutlineSettings settings = new AquaPostOutlineSettings();

        class AquaPostOutlineRenderPass : ScriptableRenderPass
        {
            AquaPostOutline AquaPostOutline;
            string profilerTag;
            Material material;
            private RenderTargetIdentifier source { get; set; }

            float _Scale;
            Color _Color;
            float _DepthThreshold;
            float _DepthNormalThreshold;
            float _DepthNormalThresholdScale;
            float _NormalThreshold;
            static class ShaderIDs
            {
                internal static readonly int MainTex = Shader.PropertyToID("_MainTex");
                internal static readonly int _Scale = Shader.PropertyToID("_Scale");
                internal static readonly int _Color = Shader.PropertyToID("_Color");
                internal static readonly int _DepthThreshold = Shader.PropertyToID("_DepthThreshold");
                internal static readonly int _DepthNormalThreshold = Shader.PropertyToID("_DepthNormalThreshold");
                internal static readonly int _DepthNormalThresholdScale = Shader.PropertyToID("_DepthNormalThresholdScale");
                internal static readonly int _NormalThreshold = Shader.PropertyToID("_NormalThreshold");
                internal static readonly int _ClipToView = Shader.PropertyToID("_ClipToView");
            }

            public AquaPostOutlineRenderPass(string profilerTag)
            {
                this.profilerTag = profilerTag;
                var stack = VolumeManager.instance.stack;
                AquaPostOutline = stack.GetComponent<AquaPostOutline>();
            }

            public void Setup(RenderTargetIdentifier source, Material material)
            {
                this.source = source;
                this.material = material;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                _Scale = AquaPostOutline.scale.value;
                _Color = AquaPostOutline.color.value;
                _DepthThreshold = AquaPostOutline.depthThreshold.value;
                _DepthNormalThreshold = AquaPostOutline.depthNormalThreshold.value;
                _DepthNormalThresholdScale = AquaPostOutline.depthNormalThresholdScale.value;
                _NormalThreshold = AquaPostOutline.normalThreshold.value;

                CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
                Render(cmd, ref renderingData);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            void Render(CommandBuffer cmd, ref RenderingData renderingData)
            {
                ref var cameraData = ref renderingData.cameraData;
                if (AquaPostOutline.IsActive() && !cameraData.isSceneViewCamera && cameraData.postProcessEnabled)
                {
                    SetupPostOutline(cmd, ref renderingData, material);
                }
            }
            public void SetupPostOutline(CommandBuffer cmd, ref RenderingData renderingData, Material material)
            {
                material.SetFloat(ShaderIDs._Scale, _Scale);
                material.SetColor(ShaderIDs._Color, _Color);
                material.SetFloat(ShaderIDs._DepthThreshold, _DepthThreshold);
                material.SetFloat(ShaderIDs._DepthNormalThreshold, _DepthNormalThreshold);
                material.SetFloat(ShaderIDs._DepthNormalThresholdScale, _DepthNormalThresholdScale);
                material.SetFloat(ShaderIDs._NormalThreshold, _NormalThreshold);

                Matrix4x4 clipToView = GL.GetGPUProjectionMatrix(renderingData.cameraData.camera.projectionMatrix, true).inverse;

                material.SetMatrix(ShaderIDs._ClipToView, clipToView);
                cmd.SetGlobalTexture("_MainTex", source);
                cmd.Blit(source, source, material);
            }
            public override void OnCameraCleanup(CommandBuffer cmd)
            {
            }
        }

        AquaPostOutlineRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new AquaPostOutlineRenderPass("AquaPostOutline");

            m_ScriptablePass.renderPassEvent = settings.renderPassEvent;

#if UNITY_EDITOR
            foreach (var guid in UnityEditor.AssetDatabase.FindAssets("AquaPostOutline t:Shader"))
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Shaders/AquaPostOutline"))
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
        }

        [SerializeField]
        Shader shader;

        Material material;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var src = renderer.cameraColorTarget;
            m_ScriptablePass.Setup(src, material);
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}