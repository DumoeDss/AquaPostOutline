using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AquaPostOutline.Scripts.RenderFeatures {
    public class AquaPostOutlineRenderPassFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class AquaPostOutlineSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public AquaPostOutlineSettings settings = new AquaPostOutlineSettings();

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
            renderer.EnqueuePass(m_ScriptablePass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            m_ScriptablePass.Setup(renderer.cameraColorTargetHandle, material);
        }
    
    }
}