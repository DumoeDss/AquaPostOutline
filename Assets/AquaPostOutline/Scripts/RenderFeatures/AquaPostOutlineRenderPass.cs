using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AquaPostOutline.Scripts.RenderFeatures {
    public class AquaPostOutlineRenderPass : ScriptableRenderPass
    {
        private readonly AquaEffects.AquaPostOutline.AquaPostOutline _aquaPostOutline;
        private readonly string _profilerTag;
        private Material _material;
        private RenderTargetIdentifier Source { get; set; }

        private float _scale;
        private Color _color;
        private float _depthThreshold;
        private float _depthNormalThreshold;
        private float _depthNormalThresholdScale;
        private float _normalThreshold;

        private static class ShaderIDs
        {
            internal static readonly int Scale = Shader.PropertyToID("_Scale");
            internal static readonly int Color = Shader.PropertyToID("_Color");
            internal static readonly int DepthThreshold = Shader.PropertyToID("_DepthThreshold");
            internal static readonly int DepthNormalThreshold = Shader.PropertyToID("_DepthNormalThreshold");
            internal static readonly int DepthNormalThresholdScale = Shader.PropertyToID("_DepthNormalThresholdScale");
            internal static readonly int NormalThreshold = Shader.PropertyToID("_NormalThreshold");
            internal static readonly int ClipToView = Shader.PropertyToID("_ClipToView");
        }

        public AquaPostOutlineRenderPass(string profilerTag)
        {
            this._profilerTag = profilerTag;
            var stack = VolumeManager.instance.stack;
            _aquaPostOutline = stack.GetComponent<AquaEffects.AquaPostOutline.AquaPostOutline>();
        }

        public void Setup(RenderTargetIdentifier source, Material material)
        {
            this.Source = source;
            this._material = material;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            _scale = _aquaPostOutline.scale.value;
            _color = _aquaPostOutline.color.value;
            _depthThreshold = _aquaPostOutline.depthThreshold.value;
            _depthNormalThreshold = _aquaPostOutline.depthNormalThreshold.value;
            _depthNormalThresholdScale = _aquaPostOutline.depthNormalThresholdScale.value;
            _normalThreshold = _aquaPostOutline.normalThreshold.value;

            CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);
            Render(cmd, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        private void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            if (_aquaPostOutline.IsActive() && !cameraData.isSceneViewCamera && cameraData.postProcessEnabled)
            {
                SetupPostOutline(cmd, ref renderingData, _material);
            }
        }

        private void SetupPostOutline(CommandBuffer cmd, ref RenderingData renderingData, Material material)
        {
            material.SetFloat(ShaderIDs.Scale, _scale);
            material.SetColor(ShaderIDs.Color, _color);
            material.SetFloat(ShaderIDs.DepthThreshold, _depthThreshold);
            material.SetFloat(ShaderIDs.DepthNormalThreshold, _depthNormalThreshold);
            material.SetFloat(ShaderIDs.DepthNormalThresholdScale, _depthNormalThresholdScale);
            material.SetFloat(ShaderIDs.NormalThreshold, _normalThreshold);

            Matrix4x4 clipToView = GL.GetGPUProjectionMatrix(renderingData.cameraData.camera.projectionMatrix, true).inverse;

            material.SetMatrix(ShaderIDs.ClipToView, clipToView);
            cmd.SetGlobalTexture("_MainTex", Source);
            cmd.Blit(Source, Source, material);
        }
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }
}
