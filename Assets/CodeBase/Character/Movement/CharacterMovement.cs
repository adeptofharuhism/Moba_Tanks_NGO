using Assets.CodeBase.Infrastructure.Properties;
using Assets.CodeBase.Infrastructure.Services.Input;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CodeBase.Character.Movement
{
    [Serializable]
    public class CharacterMovement : IUpdatable
    {
        private readonly IInputService _inputService;

        private float _wheelRotationInput = 0;
        private float _wheelAccelerationInput = 0;

        private List<CharacterWheel> _characterWheels = new List<CharacterWheel>();
        private List<CharacterWheel> _characterWheelsAccelerated = new List<CharacterWheel>();
        private List<CharacterWheel> _characterWheelsRotatedStraight = new List<CharacterWheel>();
        private List<CharacterWheel> _characterWheelsRotatedBackwards = new List<CharacterWheel>();

        public CharacterMovement(
            IInputService inputService,
            Rigidbody characterRigidBody,
            CharacterMovementData movementData, List<CharacterWheelProfile> wheelProfiles) {

            _inputService = inputService;

            CreateWheelArrays(characterRigidBody, movementData, wheelProfiles);
        }

        public void HandleInput() {
            Vector2 movementInput = _inputService.MoveInputValue;

            _wheelAccelerationInput = movementInput.y;

            _wheelRotationInput = movementInput.x;
        }

        public void Update() { }

        public void FixedUpdate() {
            ApplyRotationToWheelGroup(_wheelRotationInput, _characterWheelsRotatedStraight);
            ApplyRotationToWheelGroup(-_wheelRotationInput, _characterWheelsRotatedBackwards);

            ApplyPassiveForceToWheels();

            ApplyAccelerationForceToWheelGroup();
        }

        private void ApplyPassiveForceToWheels() {
            foreach (CharacterWheel wheel in _characterWheels)
                wheel.FindContactWithGround();

            foreach (CharacterWheel wheel in _characterWheels)
                wheel.ApplyPassiveForce();
        }

        private void ApplyRotationToWheelGroup(float wheelRotationInput, List<CharacterWheel> characterWheelsRotated) {
            foreach (CharacterWheel wheel in characterWheelsRotated)
                wheel.SetWheelRotation(wheelRotationInput);
        }

        private void ApplyAccelerationForceToWheelGroup() {
            foreach (CharacterWheel wheel in _characterWheelsAccelerated)
                wheel.ApplyAccelerationForce(_wheelAccelerationInput);
        }

        private void CreateWheelArrays(Rigidbody characterRigidBody, CharacterMovementData movementData, List<CharacterWheelProfile> wheelProfiles) {
            foreach (CharacterWheelProfile profile in wheelProfiles) {
                CharacterWheel newWheel =
                    new CharacterWheel(
                        profile.WheelTransform,
                        characterRigidBody,
                        profile.WheelTractionProfile.TractionProfile,
                        movementData,
                        profile.WheelModelTransform);

                _characterWheels.Add(newWheel);

                if (profile.IsAccelerated)
                    _characterWheelsAccelerated.Add(newWheel);

                switch (profile.RotationType) {
                    case RotationType.Straight:
                        _characterWheelsRotatedStraight.Add(newWheel);
                        break;
                    case RotationType.Backward:
                        _characterWheelsRotatedBackwards.Add(newWheel);
                        break;
                }
            }
        }
    }

    public class CharacterWheel
    {
        private const float EPSILON = 1E-06f;
        private const int HARD_BRAKING_ENGINE_POWER = 0;

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
        private readonly float _brakingCoefficient;
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
            _brakingCoefficient = movementData.BrakingCoefficient;
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

            float tractionCoefficient = _steeringTraction.Evaluate(velocityX / (Mathf.Abs(localSpaceHorizontalSpeed.magnitude) + EPSILON));

            float forceX = velocityX * tractionCoefficient / Time.fixedDeltaTime;

            return -_steeringAxis * forceX;
        }

        private Vector3 CalculateAccelerationAxisForce(float wheelAccelerationInput) {
            float velocityZ = _currentVelocityZ;
            float forceZ;

            if (wheelAccelerationInput > EPSILON) {
                if (velocityZ > _maxSpeed)
                    return Vector3.zero;
                forceZ = CalculateForceZ(wheelAccelerationInput, velocityZ, _maxSpeed);

            } else if (wheelAccelerationInput < -EPSILON) {
                if (velocityZ < _maxSpeedBackwards)
                    return Vector3.zero;
                forceZ = CalculateForceZ(wheelAccelerationInput, velocityZ, _maxSpeedBackwards);

            } else
                forceZ = CalculateBraking(velocityZ, _brakingCoefficient);

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
                    CalculateEngineForce(wheelAccelerationInput, HARD_BRAKING_ENGINE_POWER) +
                    CalculateBraking(velocityZ, _hardBrakingCoefficient);

            return forceZ;
        }

        private float NormalizeVelocityZ(float velocityZ, float maxSpeed) =>
            Mathf.Clamp01(Mathf.Abs(velocityZ) / maxSpeed);

        private float CalculateEngineForce(float wheelAccelerationInput, float normalizedVelocityZ) =>
            _enginePower.Evaluate(normalizedVelocityZ) * wheelAccelerationInput;

        private float CalculateBraking(float velocityZ, float brakingCoefficient) =>
            -velocityZ * brakingCoefficient;

        private void UpdateModelPosition(float distanceToChild) => 
            _wheelModelTransform.position = _wheelTransform.position - _wheelTransform.up * (distanceToChild - _diameter);
    }
}
