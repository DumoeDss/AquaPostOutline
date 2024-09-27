using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AquaPostOutline.Scripts.RenderFeatures
{
    public class DepthNormalsFeature : ScriptableRendererFeature
    {
        private class DepthNormalsPass : ScriptableRenderPass
        {
            private const int KDepthBufferBits = 32;
            private RenderTargetHandle DepthAttachmentHandle { get; set; }
            private RenderTextureDescriptor Descriptor { get; set; }

            private readonly Material _depthNormalsMaterial = null;
            private FilteringSettings _mFilteringSettings;

            private const string MProfilerTag = "Depth Normals Pre Pass";
            private readonly ShaderTagId _mShaderTagId = new ShaderTagId("DepthOnly");

            public DepthNormalsPass(RenderQueueRange renderQueueRange, LayerMask layerMask, Material material)
            {
                _mFilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
                _depthNormalsMaterial = material;
            }

            public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle depthAttachmentHandle)
            {
                this.DepthAttachmentHandle = depthAttachmentHandle;
                baseDescriptor.colorFormat = RenderTextureFormat.ARGB32;
                baseDescriptor.depthBufferBits = KDepthBufferBits;
                Descriptor = baseDescriptor;
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                cmd.GetTemporaryRT(DepthAttachmentHandle.id, Descriptor, FilterMode.Point);
                ConfigureTarget(DepthAttachmentHandle.Identifier());
                ConfigureClear(ClearFlag.All, Color.black);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(MProfilerTag);

                using (new ProfilingScope(cmd, new ProfilingSampler(MProfilerTag)))
                {
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                    var drawSettings = CreateDrawingSettings(_mShaderTagId, ref renderingData, sortFlags);
                    drawSettings.perObjectData = PerObjectData.None;

                    ref CameraData cameraData = ref renderingData.cameraData;
                    Camera camera = cameraData.camera;
                    if (cameraData.xr.enabled)
                        context.StartMultiEye(camera);

                    drawSettings.overrideMaterial = _depthNormalsMaterial;

                    context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref _mFilteringSettings);

                    cmd.SetGlobalTexture("_CameraDepthNormalsTexture", DepthAttachmentHandle.id);
                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                if (DepthAttachmentHandle != RenderTargetHandle.CameraTarget)
                {
                    cmd.ReleaseTemporaryRT(DepthAttachmentHandle.id);
                    DepthAttachmentHandle = RenderTargetHandle.CameraTarget;
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