using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.CodeBase.PostProcessing
{
    public class CustomPostProcessRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader _bloomShader;
        [SerializeField] private Shader _compositeShader;

        private Material _bloomMaterial;
        private Material _compositeMaterial;

        private CustomPostProcessPass _pass;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            renderer.EnqueuePass(_pass);
        }

        public override void Create() {
            _bloomMaterial = CoreUtils.CreateEngineMaterial(_bloomShader);
            _compositeMaterial = CoreUtils.CreateEngineMaterial(_compositeShader);

            _pass = new CustomPostProcessPass(_bloomMaterial, _compositeMaterial);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData) {
            if (renderingData.cameraData.cameraType == CameraType.Game) {
                _pass.ConfigureInput(ScriptableRenderPassInput.Depth);
                _pass.ConfigureInput(ScriptableRenderPassInput.Color);
                _pass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
            }
        }

        protected override void Dispose(bool disposing) {
            CoreUtils.Destroy(_bloomMaterial);
            CoreUtils.Destroy(_compositeMaterial);
        }
    }
}
