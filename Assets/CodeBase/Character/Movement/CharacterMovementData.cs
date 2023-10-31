using System;
using UnityEngine;

namespace Assets.CodeBase.Character.Movement
{
    [CreateAssetMenu(fileName = "CharacterMovementData",menuName ="Character SO")]
    public class CharacterMovementData : ScriptableObject
    {
        [SerializeField] private LayerMask _carCollisionLayerMask;

        [SerializeField] private float _maxWheelRotationDegrees = 45f;
        [SerializeField] private float _wheelRestDistance = .75f;
        [SerializeField] private float _wheelSpringStrength = 85f;
        [SerializeField] private float _wheelSpringDamper = 12f;
        [SerializeField] private float _wheelDiameter = .54f;

        [SerializeField] private AnimationCurve _frontWheelSteeringTraction;
        [SerializeField] private AnimationCurve _rearWheelSteeringTraction;

        [SerializeField] private float _maxSpeed;
        [SerializeField] private AnimationCurve _enginePower;

        public LayerMask CarCollisionLayerMask => _carCollisionLayerMask;

        public float MaxWheelRotationDegrees => _maxWheelRotationDegrees;
        public float WheelRestDistance => _wheelRestDistance;
        public float WheelSpringStrength => _wheelSpringStrength;
        public float WheelSpringDamper => _wheelSpringDamper;
        public float WheelDiameter => _wheelDiameter;

        public AnimationCurve FrontWheelSteeringTraction => _frontWheelSteeringTraction;
        public AnimationCurve RearWheelSteeringTraction => _rearWheelSteeringTraction;

        public float MaxSpeed => _maxSpeed;
        public AnimationCurve EnginePower => _enginePower;
    }
}
