using UnityEngine;

namespace Assets.CodeBase.Character.Movement
{
    [CreateAssetMenu(fileName ="CharacterWheelTractionProfile", menuName = "Character SO/CharacterWheelTractionProfile")]
    public class CharacterWheelTractionProfile : ScriptableObject
    {
        [SerializeField] private AnimationCurve _tractionProfile;

        public AnimationCurve TractionProfile => _tractionProfile;
    }
}
