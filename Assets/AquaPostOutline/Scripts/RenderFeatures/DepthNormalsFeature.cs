using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace AquaEffects.AquaPostOutline
{
    public class DepthNormalsFeature : ScriptableRendererFeature
    {
        class DepthNormalsPass : ScriptableRenderPass
        {
            int kDepthBufferBits = 32;
            private RenderTargetHandle depthAttachmentHandle { get; set; }
            internal RenderTextureDescriptor descriptor { get; private set; }

            private Material depthNormalsMaterial = null;
            private FilteringSettings m_FilteringSettings;

            string m_ProfilerTag = "Depth Normals Pre Pass";
            ShaderTagId m_ShaderTagId = new ShaderTagId("DepthOnly");

            public DepthNormalsPass(RenderQueueRange renderQueueRange, LayerMask layerMask, Material material)
            {
                m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
                depthNormalsMaterial = material;
            }

            public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle depthAttachmentHandle)
            {
                this.depthAttachmentHandle = depthAttachmentHandle;
                baseDescriptor.colorFormat = RenderTextureFormat.ARGB32;
                baseDescriptor.depthBufferBits = kDepthBufferBits;
                descriptor = baseDescriptor;
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                cmd.GetTemporaryRT(depthAttachmentHandle.id, descriptor, FilterMode.Point);
                ConfigureTarget(depthAttachmentHandle.Identifier());
                ConfigureClear(ClearFlag.All, Color.black);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

                using (new ProfilingScope(cmd, new ProfilingSampler(m_ProfilerTag)))
                {
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                    var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortFlags);
                    drawSettings.perObjectData = PerObjectData.None;

                    ref CameraData cameraData = ref renderingData.cameraData;
                    Camera camera = cameraData.camera;
                    if (cameraData.xr.enabled)
                        context.StartMultiEye(camera);

                    drawSettings.overrideMaterial = depthNormalsMaterial;

                    context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);

                    cmd.SetGlobalTexture("_CameraDepthNormalsTexture", depthAttachmentHandle.id);
                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                if (depthAttachmentHandle != RenderTargetHandle.CameraTarget)
                {
                    cmd.ReleaseTemporaryRT(depthAttachmentHandle.id);
                    depthAttachmentHandle = RenderTargetHandle.CameraTarget;
                }
            }
        }
        DepthNormalsPass depthNormalsPass;
        RenderTargetHandle depthNormalsTexture;
        Material material;

        [SerializeField]
        Shader shader;

        public override void Create()
        {
#if UNITY_EDITOR
            foreach (var guid in UnityEditor.AssetDatabase.FindAssets("DepthNormalsTexture t:Shader"))
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Shaders/DepthNormalsTexture"))
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

            depthNormalsPass = new DepthNormalsPass(RenderQueueRange.opaque, -1, material);
            depthNormalsPass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
            depthNormalsTexture.Init("_CameraDepthNormalsTexture");
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            depthNormalsPass.Setup(renderingData.cameraData.cameraTargetDescriptor, depthNormalsTexture);
            renderer.EnqueuePass(depthNormalsPass);
        }
    }
}