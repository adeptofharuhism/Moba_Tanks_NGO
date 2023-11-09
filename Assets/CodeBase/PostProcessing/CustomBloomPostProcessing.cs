using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.CodeBase.PostProcessing
{
    [VolumeComponentMenu("Custom/Custom Bloom")]
    public class CustomBloomPostProcessing : VolumeComponent, IPostProcessComponent
    {
        [Header("Bloom settings")]
        public FloatParameter threshold = new FloatParameter(0.9f, true);
        public FloatParameter intensity = new FloatParameter(1, true);
        public ClampedFloatParameter scatter = new ClampedFloatParameter(.7f, 0, 1, true);
        public IntParameter clamp = new IntParameter(65472, true);
        public ClampedIntParameter maxIterations = new ClampedIntParameter(6, 0, 10);
        public NoInterpColorParameter tint = new NoInterpColorParameter(Color.white);

        [Header("Custom bloom settings")]
        public IntParameter dotsDensity = new IntParameter(10, true);
        public ClampedFloatParameter dotsCutoff = new ClampedFloatParameter(.4f, 0, 1, true);
        public Vector2Parameter scrollDirection = new Vector2Parameter(new Vector2());

        public bool IsActive() {
            return true;
        }

        public bool IsTileCompatible() {
            return false;
        }
    }

}