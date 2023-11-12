using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.CodeBase.PostProcessing
{
    public class MoebiusPostProcessRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader _depthShader;
        [SerializeField] private Shader _normaleShader;
        [SerializeField] private Shader _sobelShader;
        [SerializeField] private Shader _compositeShader;

        private Material _depthMaterial;
        private Material _normaleMaterial;
        private Material _sobelDepthMaterial;
        private Material _sobelNormaleMaterial;
        private Material _compositeMaterial;

        private MoebiusPostProcessPass _pass;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) => 
            renderer.EnqueuePass(_pass);

        public override void Create() {
            _depthMaterial = CoreUtils.CreateEngineMaterial(_depthShader);
            _normaleMaterial = CoreUtils.CreateEngineMaterial(_normaleShader);
            _sobelDepthMaterial = CoreUtils.CreateEngineMaterial(_sobelShader);
            _sobelNormaleMaterial = CoreUtils.CreateEngineMaterial(_sobelShader);
            _compositeMaterial = CoreUtils.CreateEngineMaterial(_compositeShader);

            _pass = new MoebiusPostProcessPass(
                _depthMaterial, _sobelDepthMaterial, _sobelNormaleMaterial, _normaleMaterial, _compositeMaterial);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData) {
            if (renderingData.cameraData.cameraType == CameraType.Game) {
                _pass.ConfigureInput(ScriptableRenderPassInput.Color);
                _pass.ConfigureInput(ScriptableRenderPassInput.Depth);
                _pass.SetTarget(renderer.cameraColorTargetHandle);
            }
        }

        protected override void Dispose(bool disposing) {
            CoreUtils.Destroy(_depthMaterial);
            CoreUtils.Destroy(_normaleMaterial);
            CoreUtils.Destroy(_sobelDepthMaterial);
            CoreUtils.Destroy(_sobelNormaleMaterial);
            CoreUtils.Destroy(_compositeMaterial);
        }
    }
}
