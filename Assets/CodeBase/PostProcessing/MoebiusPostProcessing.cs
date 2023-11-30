using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.CodeBase.PostProcessing
{
    [VolumeComponentMenu("Custom/Moebius")]
    public class MoebiusPostProcessing : VolumeComponent, IPostProcessComponent
    {
        [Header("Resolution scaling")]
        public IntParameter MinimalResolution = new(600, true);
        public IntParameter TargetResolution = new(1920, true);
        [Header("Border")]
        public ClampedFloatParameter BorderIntensity = new(1, 0, 3, true);
        [Header("Depth Sobel")]
        public ClampedFloatParameter SobelThresholdForDepth = new(0.0005f, 0, 0.1f, true);
        public FloatParameter ColorIntensityForDepth = new(250f, true);
        [Header("Normale Sobel")]
        public ClampedFloatParameter SobelThresholdForNormale = new(0.1f, 0, 1f, true);
        public FloatParameter ColorIntensityForNormale = new(2.2f, true);

        public bool IsActive() => true;

        public bool IsTileCompatible() => false;
    }

}