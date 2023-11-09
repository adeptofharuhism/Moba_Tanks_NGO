using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.CodeBase.PostProcessing
{
    [VolumeComponentMenu("Custom/Moebius")]
    public class MoebiusPostProcessing : VolumeComponent, IPostProcessComponent
    {
        public NoInterpColorParameter borderColor = new NoInterpColorParameter(Color.white);

        public bool IsActive() {
            return true;
        }

        public bool IsTileCompatible() {
            return false;
        }
    }

}