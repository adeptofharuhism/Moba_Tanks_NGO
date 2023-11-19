using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.CodeBase.PostProcessing
{
    public class MoebiusPostProcessPass : ScriptableRenderPass
    {
        private const string _depthPassName = "_DepthPass";
        private const string _normalePassName = "_NormalePass";
        private const string _sobelDepthPassName = "_SobelDepthPass";
        private const string _sobelNormalePassName = "_SobelNormalePass";

        private static int _sobelDepthPassId = Shader.PropertyToID(_sobelDepthPassName);
        private static int _sobelNormalePassId = Shader.PropertyToID(_sobelNormalePassName);

        private static int _borderIntensityPropertyId = Shader.PropertyToID("_BorderIntensity");
        private static int _sobelThresholdPropertyId = Shader.PropertyToID("_SobelThreshold");
        private static int _colorIntensityPropertyId = Shader.PropertyToID("_ColorIntensity");
        private static int _sobelTexPropertyId = Shader.PropertyToID("_SobelTex");
        private static int _postSobelDepthPropertyId = Shader.PropertyToID("_DepthSobelTex");
        private static int _postSobelNormalePropertyId = Shader.PropertyToID("_NormaleSobelTex");

        private readonly Material _depthMaterial;
        private readonly Material _normaleMaterial;
        private readonly Material _sobelDepthMaterial;
        private readonly Material _compositeMaterial;
        private readonly Material _sobeNormalelMaterial;

        private RTHandle _cameraColorTarget;
        private RTHandle _depthTarget;
        private RTHandle _normaleTarget;
        private RTHandle _sobelDepthTarget;
        private RTHandle _sobelNormaleTarget;
        private RenderTextureDescriptor _descriptor;

        private MoebiusPostProcessing _moebiusEffect;
        private GraphicsFormat _hdrFormat;

        private float _interpolationCoefficientForParameters;

        public MoebiusPostProcessPass
            (Material depthMaterial, Material sobelDepthMaterial, Material sobelNormaleMaterial, Material normaleMaterial, Material compositeMaterial) {

            _depthMaterial = depthMaterial;
            _sobelDepthMaterial = sobelDepthMaterial;
            _normaleMaterial = normaleMaterial;
            _compositeMaterial = compositeMaterial;
            _sobeNormalelMaterial = sobelNormaleMaterial;

            _depthTarget = RTHandles.Alloc(Shader.PropertyToID(_depthPassName), name: _depthPassName);
            _normaleTarget = RTHandles.Alloc(Shader.PropertyToID(_normalePassName), name: _normalePassName);
            _sobelDepthTarget = RTHandles.Alloc(_sobelDepthPassId, name: _sobelDepthPassName);
            _sobelNormaleTarget = RTHandles.Alloc(_sobelNormalePassId, name: _sobelNormalePassName);

            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            const FormatUsage usage = FormatUsage.Linear | FormatUsage.Render;
            if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, usage))
                _hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
            else
                _hdrFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
                    ? GraphicsFormat.R8G8B8A8_SRGB
                    : GraphicsFormat.R8G8B8A8_UNorm;
        }

        public void SetTarget(RTHandle cameraColorTargetHandle) =>
            _cameraColorTarget = cameraColorTargetHandle;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) =>
            _descriptor = renderingData.cameraData.cameraTargetDescriptor;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            if (_cameraColorTarget == null)
                return;

            VolumeStack stack = VolumeManager.instance.stack;
            _moebiusEffect = stack.GetComponent<MoebiusPostProcessing>();

            CalculateInterpolationCoefficient();

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler("Custom Post Process Effects"))) {
                RenderTextureDescriptor descriptor = GetCompatibleDescriptor(_descriptor.width, _descriptor.height);

                RenderingUtils.ReAllocateIfNeeded(
                    ref _depthTarget, descriptor, FilterMode.Bilinear, TextureWrapMode.Mirror, name: _depthTarget.name);
                Blitter.BlitCameraTexture(cmd, _depthTarget, _depthTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, _depthMaterial, 0);

                _sobelDepthMaterial.SetFloat(_borderIntensityPropertyId, _moebiusEffect.BorderIntensity.value);
                _sobelDepthMaterial.SetFloat(_sobelThresholdPropertyId, _moebiusEffect.SobelThresholdForDepth.value);
                _sobelDepthMaterial.SetFloat(
                    _colorIntensityPropertyId, InterpolateFloatParameter(_moebiusEffect.ColorIntensityForDepth.value));
                _sobelDepthMaterial.SetTexture(_sobelTexPropertyId, _depthTarget);

                RenderingUtils.ReAllocateIfNeeded(
                    ref _sobelDepthTarget, descriptor, FilterMode.Bilinear, TextureWrapMode.Mirror, name: _sobelDepthTarget.name);
                Blitter.BlitCameraTexture(cmd, _depthTarget, _sobelDepthTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, _sobelDepthMaterial, 0);

                RenderingUtils.ReAllocateIfNeeded(
                    ref _normaleTarget, descriptor, FilterMode.Bilinear, TextureWrapMode.Mirror, name: _normaleTarget.name);
                Blitter.BlitCameraTexture(cmd, _normaleTarget, _normaleTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, _normaleMaterial, 0);

                _sobeNormalelMaterial.SetFloat(_borderIntensityPropertyId, _moebiusEffect.BorderIntensity.value);
                _sobeNormalelMaterial.SetFloat(_sobelThresholdPropertyId, _moebiusEffect.SobelThresholdForNormale.value);
                _sobeNormalelMaterial.SetFloat(
                    _colorIntensityPropertyId, InterpolateFloatParameter(_moebiusEffect.ColorIntensityForNormale.value));
                _sobeNormalelMaterial.SetTexture(_sobelTexPropertyId, _normaleTarget);

                RenderingUtils.ReAllocateIfNeeded(
                    ref _sobelNormaleTarget, descriptor, FilterMode.Bilinear, TextureWrapMode.Mirror, name: _sobelNormaleTarget.name);
                Blitter.BlitCameraTexture(cmd, _normaleTarget, _sobelNormaleTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, _sobeNormalelMaterial, 0);

                _compositeMaterial.SetTexture(_postSobelDepthPropertyId, _sobelDepthTarget);
                _compositeMaterial.SetTexture(_postSobelNormalePropertyId, _sobelNormaleTarget);

                Blitter.BlitCameraTexture(cmd, _cameraColorTarget, _cameraColorTarget, _compositeMaterial, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        private void CalculateInterpolationCoefficient() {
            int currentResolution = (_descriptor.width > _descriptor.height) ? _descriptor.width : _descriptor.height;
            _interpolationCoefficientForParameters =
                (currentResolution - _moebiusEffect.MinimalResolution.value) /
                (_moebiusEffect.TargetResolution.value - _moebiusEffect.MinimalResolution.value);
        }

        private float InterpolateFloatParameter(float parameter) =>
            parameter + parameter * _interpolationCoefficientForParameters;

        private RenderTextureDescriptor GetCompatibleDescriptor(
            int width, int height, DepthBits depthBufferBits = DepthBits.None) =>
            GetCompatibleDescriptor(_descriptor, width, height, depthBufferBits);

        private RenderTextureDescriptor GetCompatibleDescriptor(
            RenderTextureDescriptor descriptor, int width, int height, DepthBits depthBufferBits = DepthBits.None) {

            descriptor.depthBufferBits = (int)depthBufferBits;
            descriptor.msaaSamples = 1;
            descriptor.width = width;
            descriptor.height = height;
            descriptor.graphicsFormat = _hdrFormat;
            return descriptor;
        }
    }
}
