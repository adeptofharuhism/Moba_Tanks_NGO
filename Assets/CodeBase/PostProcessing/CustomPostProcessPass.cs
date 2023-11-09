using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.CodeBase.PostProcessing
{
    public class CustomPostProcessPass : ScriptableRenderPass
    {
        private readonly Material _bloomMaterial;
        private readonly Material _compositeMaterial;

        private RTHandle _cameraColorTarget;
        private RTHandle _cameraDepthTarget;
        private RenderTextureDescriptor _descriptor;

        const int MAX_PYRAMID_SIZE = 16;
        private int[] _bloomMipUp;
        private int[] _bloomMipDown;
        private RTHandle[] _bloomMipUpHandle;
        private RTHandle[] _bloomMipDownHandle;
        private GraphicsFormat _hdrFormat;
        private CustomBloomPostProcessing _bloomEffect;

        public CustomPostProcessPass(Material bloomMaterial, Material compositeMaterial) {
            _bloomMaterial = bloomMaterial;
            _compositeMaterial = compositeMaterial;

            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            _bloomMipUp = new int[MAX_PYRAMID_SIZE];
            _bloomMipDown = new int[MAX_PYRAMID_SIZE];
            _bloomMipUpHandle = new RTHandle[MAX_PYRAMID_SIZE];
            _bloomMipDownHandle = new RTHandle[MAX_PYRAMID_SIZE];

            for (int i = 0; i < MAX_PYRAMID_SIZE; i++) {
                _bloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
                _bloomMipDown[i] = Shader.PropertyToID("_BloomMipDown" + i);
                _bloomMipUpHandle[i] = RTHandles.Alloc(_bloomMipUp[i], name: "_BloomMipUp" + i);
                _bloomMipDownHandle[i] = RTHandles.Alloc(_bloomMipDown[i], name: "_BloomMipDown" + i);
            }

            const FormatUsage usage = FormatUsage.Linear | FormatUsage.Render;
            if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, usage))
                _hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
            else
                _hdrFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
                    ? GraphicsFormat.R8G8B8A8_SRGB
                    : GraphicsFormat.R8G8B8A8_UNorm;
        }

        public void SetTarget(RTHandle cameraColorTargetHandle, RTHandle cameraDepthTargetHandle) {
            _cameraColorTarget = cameraColorTargetHandle;
            _cameraDepthTarget = cameraDepthTargetHandle;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) => 
            _descriptor = renderingData.cameraData.cameraTargetDescriptor;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            if (_cameraColorTarget == null) {
                Debug.Log("Returned");
                return;
            }

            VolumeStack stack = VolumeManager.instance.stack;
            _bloomEffect = stack.GetComponent<CustomBloomPostProcessing>();

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler("Custom Post Process Effects"))) {
                SetupBloom(cmd, _cameraColorTarget);

                _compositeMaterial.SetFloat("_Cutoff", _bloomEffect.dotsCutoff.value);
                _compositeMaterial.SetFloat("_Density", _bloomEffect.dotsDensity.value);
                _compositeMaterial.SetVector("_Direction", _bloomEffect.scrollDirection.value);

                Blitter.BlitCameraTexture(cmd, _cameraColorTarget, _cameraColorTarget, _compositeMaterial, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        private void SetupBloom(CommandBuffer cmd, RTHandle source) {
            int downres = 1;
            int tw = _descriptor.width >> downres;
            int th = _descriptor.height >> downres;

            int maxSize = Mathf.Max(tw, th);
            int iterations = Mathf.FloorToInt(Mathf.Log(maxSize, 2f) - 1);
            int mipCount = Mathf.Clamp(iterations, 1, _bloomEffect.maxIterations.value);

            float clamp = _bloomEffect.clamp.value;
            float threshold = Mathf.GammaToLinearSpace(_bloomEffect.threshold.value);
            float thresholdKnee = threshold * .5f;

            float scatter = Mathf.Lerp(.05f, .95f, _bloomEffect.scatter.value);
            Material bloomMaterial = _bloomMaterial;

            bloomMaterial.SetVector("_Params", new Vector4(scatter, clamp, threshold, thresholdKnee));

            RenderTextureDescriptor desc = GetCompatibleDescriptor(tw, th, _hdrFormat);
            for (int i = 0; i < mipCount; i++) {
                RenderingUtils.ReAllocateIfNeeded(ref _bloomMipUpHandle[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: _bloomMipUpHandle[i].name);
                RenderingUtils.ReAllocateIfNeeded(ref _bloomMipDownHandle[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: _bloomMipDownHandle[i].name);
                desc.width = Mathf.Max(1, desc.width >> downres);
                desc.height = Mathf.Max(1, desc.height >> downres);
            }

            Blitter.BlitCameraTexture(
                cmd, source, _bloomMipDownHandle[0], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 0);

            RTHandle lastDown = _bloomMipDownHandle[0];
            for (int i = 1; i < mipCount; i++) {
                Blitter.BlitCameraTexture(
                    cmd, lastDown, _bloomMipUpHandle[i], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 1);
                Blitter.BlitCameraTexture(
                    cmd, _bloomMipUpHandle[i], _bloomMipDownHandle[i], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 2);

                lastDown = _bloomMipDownHandle[i];
            }

            for (int i = mipCount - 2; i >= 0; i--) {
                RTHandle lowMip = (i == mipCount - 2) ? _bloomMipDownHandle[i + 1] : _bloomMipUpHandle[i + 1];
                RTHandle highMip = _bloomMipDownHandle[i];
                RTHandle dst = _bloomMipUpHandle[i];

                cmd.SetGlobalTexture("_SourceTexLowMip", lowMip);
                Blitter.BlitCameraTexture(
                    cmd, highMip, dst, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 3);
            }
            
            cmd.SetGlobalTexture("_Bloom_Texture", _bloomMipUpHandle[0]);
            cmd.SetGlobalFloat("_BloomIntensity", _bloomEffect.intensity.value);
        }

        private RenderTextureDescriptor GetCompatibleDescriptor() =>
            GetCompatibleDescriptor(_descriptor.width, _descriptor.height, _descriptor.graphicsFormat);

        private RenderTextureDescriptor GetCompatibleDescriptor(
            int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None) => 
            GetCompatibleDescriptor(_descriptor, width, height, format, depthBufferBits);

        private RenderTextureDescriptor GetCompatibleDescriptor(
            RenderTextureDescriptor descriptor, int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None) {
            
            descriptor.depthBufferBits = (int)depthBufferBits;
            descriptor.msaaSamples = 1;
            descriptor.width = width;
            descriptor.height = height;
            descriptor.graphicsFormat = format;
            return descriptor;
        }
    }
}
