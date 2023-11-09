using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.CodeBase.PostProcessing
{
    public class MoebiusPostProcessPass : ScriptableRenderPass
    {
        private readonly Material _depthMaterial;
        private readonly Material _sobelMaterial;

        private RTHandle _cameraColorTarget;
        private RTHandle _anus;
        private RTHandle _cameraDepthTarget;
        private RenderTextureDescriptor _descriptor;

        private MoebiusPostProcessing _moebiusEffect;
        private GraphicsFormat _hdrFormat;

        public MoebiusPostProcessPass(Material depthMaterial, Material sobelMaterial) {
            _depthMaterial = depthMaterial;
            _sobelMaterial = sobelMaterial;

            _anus = RTHandles.Alloc(Shader.PropertyToID("_SobelTex"), name: "_SobelTex");

            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

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
            _moebiusEffect = stack.GetComponent<MoebiusPostProcessing>();

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler("Custom Post Process Effects"))) {
                var descriptor = GetCompatibleDescriptor(_descriptor.width, _descriptor.height, _hdrFormat);
                RenderingUtils.ReAllocateIfNeeded(ref _anus, descriptor, FilterMode.Bilinear, TextureWrapMode.Mirror, name: _anus.name);
                Blitter.BlitCameraTexture(cmd, _cameraColorTarget, _anus, _depthMaterial, 0);

                _sobelMaterial.SetColor("_SobelColor", _moebiusEffect.borderColor.value);
                _sobelMaterial.SetTexture("_SobelTex", _anus);

                Blitter.BlitCameraTexture(cmd, _cameraColorTarget, _cameraColorTarget, _sobelMaterial, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

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
