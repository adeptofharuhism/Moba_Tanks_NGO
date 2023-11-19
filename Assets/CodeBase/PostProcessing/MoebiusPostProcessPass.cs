using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.CodeBase.PostProcessing
{
    public class MoebiusPostProcessPass : ScriptableRenderPass
    {
        public const string FrameDebuggerTitle = "Custom Post Process Effects";

        public const string DepthPassName = "_DepthPass";
        public const string NormalePassName = "_NormalePass";
        public const string SobelDepthPassName = "_SobelDepthPass";
        public const string SobelNormalePassName = "_SobelNormalePass";

        private readonly static int _sobelDepthPassId = Shader.PropertyToID(SobelDepthPassName);
        private readonly static int _sobelNormalePassId = Shader.PropertyToID(SobelNormalePassName);

        private readonly static int _borderIntensityPropertyId = Shader.PropertyToID("_BorderIntensity");
        private readonly static int _sobelThresholdPropertyId = Shader.PropertyToID("_SobelThreshold");
        private readonly static int _colorIntensityPropertyId = Shader.PropertyToID("_ColorIntensity");
        private readonly static int _sobelTexPropertyId = Shader.PropertyToID("_SobelTex");
        private readonly static int _postSobelDepthPropertyId = Shader.PropertyToID("_DepthSobelTex");
        private readonly static int _postSobelNormalePropertyId = Shader.PropertyToID("_NormaleSobelTex");

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

            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            InitialAllocationOfRTHandles();
            GraphicsFormatInitialization();
        }

        public void SetTarget(RTHandle cameraColorTargetHandle) =>
            _cameraColorTarget = cameraColorTargetHandle;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) =>
            _descriptor = renderingData.cameraData.cameraTargetDescriptor;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            if (_cameraColorTarget == null)
                return;

            GetMoebiusEffectVolume();

            CalculateInterpolationCoefficient();

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler(FrameDebuggerTitle))) {
                RenderTextureDescriptor descriptor = GetCompatibleDescriptor(_descriptor.width, _descriptor.height);
                PassSobelDepth(cmd, descriptor);
                PassSobelNormale(cmd, descriptor);
                PassSobelComposite(cmd);
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

        private void PassSobelDepth(CommandBuffer cmd, RenderTextureDescriptor descriptor) {
            BlitWithDestinationReAllocation(cmd, ref _depthTarget, ref _depthTarget, descriptor, _depthMaterial, 0);

            SetSobelMaterialParameters(
                _sobelDepthMaterial,
                _moebiusEffect.SobelThresholdForDepth.value,
                _moebiusEffect.ColorIntensityForDepth.value,
                _depthTarget);

            BlitWithDestinationReAllocation(cmd, ref _depthTarget, ref _sobelDepthTarget, descriptor, _sobelDepthMaterial, 0);
        }

        private void PassSobelNormale(CommandBuffer cmd, RenderTextureDescriptor descriptor) {
            BlitWithDestinationReAllocation(cmd, ref _normaleTarget, ref _normaleTarget, descriptor, _normaleMaterial, 0);

            SetSobelMaterialParameters(
                _sobeNormalelMaterial,
                _moebiusEffect.SobelThresholdForNormale.value,
                _moebiusEffect.ColorIntensityForNormale.value,
                _normaleTarget);

            BlitWithDestinationReAllocation(cmd, ref _normaleTarget, ref _sobelNormaleTarget, descriptor, _sobeNormalelMaterial, 0);
        }

        private void PassSobelComposite(CommandBuffer cmd) {
            _compositeMaterial.SetTexture(_postSobelDepthPropertyId, _sobelDepthTarget);
            _compositeMaterial.SetTexture(_postSobelNormalePropertyId, _sobelNormaleTarget);

            Blitter.BlitCameraTexture(cmd, _cameraColorTarget, _cameraColorTarget, _compositeMaterial, 0);
        }

        private void GraphicsFormatInitialization() {
            const FormatUsage usage = FormatUsage.Linear | FormatUsage.Render;
            if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, usage))
                _hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
            else
                _hdrFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
                    ? GraphicsFormat.R8G8B8A8_SRGB
                    : GraphicsFormat.R8G8B8A8_UNorm;
        }

        private void InitialAllocationOfRTHandles() {
            _depthTarget = RTHandles.Alloc(Shader.PropertyToID(DepthPassName), name: DepthPassName);
            _normaleTarget = RTHandles.Alloc(Shader.PropertyToID(NormalePassName), name: NormalePassName);
            _sobelDepthTarget = RTHandles.Alloc(_sobelDepthPassId, name: SobelDepthPassName);
            _sobelNormaleTarget = RTHandles.Alloc(_sobelNormalePassId, name: SobelNormalePassName);
        }

        private void GetMoebiusEffectVolume() {
            VolumeStack stack = VolumeManager.instance.stack;
            _moebiusEffect = stack.GetComponent<MoebiusPostProcessing>();
        }

        private void BlitWithDestinationReAllocation(
            CommandBuffer cmd,
            ref RTHandle source, ref RTHandle destination,
            RenderTextureDescriptor descriptor, Material material, int pass) {

            RenderingUtils.ReAllocateIfNeeded(
                ref destination, descriptor, FilterMode.Bilinear, TextureWrapMode.Mirror, name: destination.name);
            Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, material, pass);
        }

        private void SetSobelMaterialParameters(Material sobelMaterial, float sobelThreshold, float colorIntensity, RTHandle texture) {
            sobelMaterial.SetFloat(_borderIntensityPropertyId, _moebiusEffect.BorderIntensity.value);
            sobelMaterial.SetFloat(_sobelThresholdPropertyId, sobelThreshold);
            sobelMaterial.SetFloat(
                _colorIntensityPropertyId, InterpolateFloatParameter(colorIntensity));
            sobelMaterial.SetTexture(_sobelTexPropertyId, texture);
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
