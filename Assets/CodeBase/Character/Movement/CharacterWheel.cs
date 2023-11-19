using System;
using UnityEngine;

namespace Assets.CodeBase.Character.Movement
{
    public class CharacterWheel
    {
        public const float Epsilon = 1E-06f;
        public const int HardBrakingEnginePower = 0;

        private readonly Transform _wheelTransform;
        private readonly Rigidbody _characterRigidbody;

        private readonly LayerMask _characterCollisionLayerMask;
        private readonly float _maxWheelRotationDegrees;
        private readonly float _springRestDistance;
        private readonly float _springStrength;
        private readonly float _springDamper;
        private readonly float _diameter;
        private readonly AnimationCurve _steeringTraction;
        private readonly float _maxSpeed;
        private readonly float _maxSpeedBackwards;
        private readonly AnimationCurve _enginePower;
        private readonly float _brakingForce;
        private readonly float _hardBrakingCoefficient;

        private Transform _wheelModelTransform;

        private bool _hasContactWithGround;
        private RaycastHit _groundRaycastHit;

        private Vector3 _springAxis;
        private Vector3 _steeringAxis;
        private Vector3 _accelerationAxis;
        private Vector3 _wheelVelocityInWorld;
        private float _currentVelocityZ;

        public CharacterWheel(
            Transform wheelTransform, Rigidbody characterRigidbody, AnimationCurve steeringTraction,
            CharacterMovementData movementData,
            Transform wheelModelTransform) {

            _wheelTransform = wheelTransform;
            _characterRigidbody = characterRigidbody;
            _steeringTraction = steeringTraction;

            _characterCollisionLayerMask = movementData.CarCollisionLayerMask;
            _maxWheelRotationDegrees = movementData.MaxWheelRotationDegrees;
            _springRestDistance = movementData.WheelRestDistance;
            _springStrength = movementData.WheelSpringStrength;
            _springDamper = movementData.WheelSpringDamper;
            _diameter = movementData.WheelDiameter;
            _maxSpeed = movementData.MaxSpeed;
            _maxSpeedBackwards = movementData.MaxSpeedBackwards;
            _enginePower = movementData.EnginePower;
            _brakingForce = movementData.BrakingForce;
            _hardBrakingCoefficient = movementData.HardBrakingCoefficient;

            _wheelModelTransform = wheelModelTransform;
        }

        public void FindContactWithGround() {
            CastRayToGround();

            VelocityAndAxisPreparation();
        }

        public void ApplyPassiveForce() {
            if (!_hasContactWithGround) {
                UpdateModelPosition(_springRestDistance);
                return;
            }

            Vector3 summaryPassiveForce =
                CalculateSpringAxisForce(_groundRaycastHit) +
                CalculateSteeringAxisForce();

            _characterRigidbody.AddForceAtPosition(summaryPassiveForce, _wheelTransform.position);

            UpdateModelPosition(_groundRaycastHit.distance);
        }

        public void ApplyAccelerationForce(float wheelAccelerationInput) {
            if (!_hasContactWithGround)
                return;

            Vector3 accelerationForce = CalculateAccelerationAxisForce(wheelAccelerationInput);

            _characterRigidbody.AddForceAtPosition(accelerationForce, _wheelTransform.position);
        }

        public void SetWheelRotation(float wheelRotationInput) =>
            _wheelTransform.localRotation = Quaternion.Euler(0, wheelRotationInput * _maxWheelRotationDegrees, 0);

        private void CastRayToGround() =>
            _hasContactWithGround =
                Physics.Raycast(
                    _wheelTransform.position,
                    -_wheelTransform.up,
                    out _groundRaycastHit,
                    _springRestDistance,
                    _characterCollisionLayerMask);

        private void VelocityAndAxisPreparation() {
            _springAxis = _wheelTransform.up;
            _steeringAxis = _wheelTransform.right;
            _accelerationAxis = _wheelTransform.forward;
            _wheelVelocityInWorld = _characterRigidbody.GetPointVelocity(_wheelTransform.position);
        }

        private Vector3 CalculateSpringAxisForce(RaycastHit hitInfo) {
            float offsetFromRest = _springRestDistance - hitInfo.distance;
            float velocityY = Vector3.Dot(_springAxis, _wheelVelocityInWorld);
            float forceY = (offsetFromRest * _springStrength) - (velocityY * _springDamper);

            return _springAxis * forceY;
        }

        private Vector3 CalculateSteeringAxisForce() {
            float velocityX = Vector3.Dot(_steeringAxis, _wheelVelocityInWorld);
            _currentVelocityZ = Vector3.Dot(_accelerationAxis, _wheelVelocityInWorld);

            Vector3 worldSpaceHorizontalSpeed = new Vector3(velocityX, 0, _currentVelocityZ);
            Vector3 localSpaceHorizontalSpeed = _wheelTransform.rotation * worldSpaceHorizontalSpeed;

            float tractionCoefficient = _steeringTraction.Evaluate(velocityX / (Mathf.Abs(localSpaceHorizontalSpeed.magnitude) + Epsilon));

            float forceX = velocityX * tractionCoefficient / Time.fixedDeltaTime;

            return -_steeringAxis * forceX;
        }

        private Vector3 CalculateAccelerationAxisForce(float wheelAccelerationInput) {
            float velocityZ = _currentVelocityZ;
            float forceZ;

            if (wheelAccelerationInput > Epsilon) {
                if (velocityZ > _maxSpeed)
                    return Vector3.zero;
                forceZ = CalculateForceZ(wheelAccelerationInput, velocityZ, _maxSpeed);

            } else if (wheelAccelerationInput < -Epsilon) {
                if (velocityZ < _maxSpeedBackwards)
                    return Vector3.zero;
                forceZ = CalculateForceZ(wheelAccelerationInput, velocityZ, _maxSpeedBackwards);

            } else
                forceZ = CalculateBraking(velocityZ);

            forceZ /= Time.fixedDeltaTime;

            return _accelerationAxis * forceZ;
        }

        private float CalculateForceZ(float wheelAccelerationInput, float velocityZ, float maxSpeed) {
            float forceZ;
            if (velocityZ * wheelAccelerationInput > 0)
                forceZ = CalculateEngineForce(
                    wheelAccelerationInput,
                    NormalizeVelocityZ(velocityZ, maxSpeed));
            else
                forceZ =
                    CalculateEngineForce(wheelAccelerationInput, HardBrakingEnginePower) +
                    CalculateHardBraking(velocityZ);

            return forceZ;
        }

        private float NormalizeVelocityZ(float velocityZ, float maxSpeed) =>
            Mathf.Clamp01(Mathf.Abs(velocityZ) / maxSpeed);

        private float CalculateEngineForce(float wheelAccelerationInput, float normalizedVelocityZ) =>
            _enginePower.Evaluate(normalizedVelocityZ) * wheelAccelerationInput;

        private float CalculateHardBraking(float velocityZ) =>
            -velocityZ * _hardBrakingCoefficient;

        private float CalculateBraking(float velocityZ) {
            if (velocityZ > _brakingForce)
                return -_brakingForce;

            if (velocityZ < -_brakingForce)
                return _brakingForce;

            return -velocityZ;
        }

        private void UpdateModelPosition(float distanceToChild) => 
            _wheelModelTransform.position = _wheelTransform.position - _wheelTransform.up * (distanceToChild - _diameter);
    }
}
