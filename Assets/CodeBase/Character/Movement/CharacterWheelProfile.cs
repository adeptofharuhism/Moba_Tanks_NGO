using System;
using UnityEngine;

namespace Assets.CodeBase.Character.Movement
{
    [Serializable]
    public class CharacterWheelProfile
    {
        public Transform WheelTransform;
        public CharacterWheelTractionProfile WheelTractionProfile;
        public bool IsAccelerated;
        public RotationType RotationType;
        public Transform WheelModelTransform;
    }

    public enum RotationType
    {
        Static,
        Straight,
        Backward
    }
}
