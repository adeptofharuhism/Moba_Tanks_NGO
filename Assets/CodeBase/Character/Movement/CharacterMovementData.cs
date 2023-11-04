using System;
using UnityEngine;

namespace Assets.CodeBase.Character.Movement
{
    [CreateAssetMenu(fileName = "CharacterMovementData",menuName ="Character SO/CharacterMovementData")]
    public class CharacterMovementData : ScriptableObject
    {
        [SerializeField] private LayerMask _carCollisionLayerMask;
        
        [SerializeField] private float _maxWheelRotationDegrees = 45f;
        [SerializeField] private float _wheelRestDistance = .75f;
        [SerializeField] private float _wheelSpringStrength = 85f;
        [SerializeField] private float _wheelSpringDamper = 12f;
        [SerializeField] private float _wheelDiameter = .54f;

        [SerializeField] private float _maxSpeed;
        [SerializeField] private float _maxSpeedBackward;
        [SerializeField] private AnimationCurve _enginePower;

        public LayerMask CarCollisionLayerMask => _carCollisionLayerMask;

        public float MaxWheelRotationDegrees => _maxWheelRotationDegrees;
        public float WheelRestDistance => _wheelRestDistance;
        public float WheelSpringStrength => _wheelSpringStrength;
        public float WheelSpringDamper => _wheelSpringDamper;
        public float WheelDiameter => _wheelDiameter;

        public float MaxSpeed => _maxSpeed;
        public float MaxSpeedBackwards => _maxSpeedBackward;
        public AnimationCurve EnginePower => _enginePower;
    }
}
