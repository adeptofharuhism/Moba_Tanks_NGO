using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.CodeBase.PostProcessing
{
    public class MoebiusPostProcessRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader _depthShader;
        [SerializeField] private Shader _sobelShader;

        private Material _depthMaterial;
        private Material _sobelMaterial;

        private MoebiusPostProcessPass _pass;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) => 
            renderer.EnqueuePass(_pass);

        public override void Create() {
            _depthMaterial = CoreUtils.CreateEngineMaterial(_depthShader);
            _sobelMaterial = CoreUtils.CreateEngineMaterial(_sobelShader);

            _pass = new MoebiusPostProcessPass(_depthMaterial, _sobelMaterial);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData) {
            if (renderingData.cameraData.cameraType == CameraType.Game) {
                _pass.ConfigureInput(ScriptableRenderPassInput.Color);
                _pass.ConfigureInput(ScriptableRenderPassInput.Depth);
                _pass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
            }
        }

        protected override void Dispose(bool disposing) {
            CoreUtils.Destroy(_depthMaterial);
            CoreUtils.Destroy(_sobelMaterial);
        }
    }
}
