using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.CodeBase.PostProcessing
{
    [VolumeComponentMenu("Custom/Moebius")]
    public class MoebiusPostProcessing : VolumeComponent, IPostProcessComponent
    {
        public NoInterpColorParameter BorderColor = new NoInterpColorParameter(Color.white);
        public ClampedFloatParameter BorderIntensity = new ClampedFloatParameter(1, 0, 3, true);
        [Header("Depth Sobel")]
        public ClampedFloatParameter SobelThresholdForDepth = new ClampedFloatParameter(0.0005f, 0, 0.002f, true);
        public FloatParameter ColorIntensityForDepth = new FloatParameter(25f, true);
        [Header("Normale Sobel")]
        public ClampedFloatParameter SobelThresholdForNormale = new ClampedFloatParameter(0.1f, 0, 1f, true);
        public FloatParameter ColorIntensityForNormale = new FloatParameter(1f, true);

        public bool IsActive() {
            return true;
        }

        public bool IsTileCompatible() {
            return false;
        }
    }

}