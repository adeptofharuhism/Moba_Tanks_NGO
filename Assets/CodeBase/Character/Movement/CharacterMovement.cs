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
        private readonly Transform _characterTransform;

        private float _wheelRotationInput = 0;
        private float _wheelAccelerationInput = 0;
        private float _wheelRotationDegree = 0;

        private CharacterWheel[] _characterWheelsAccelerated;
        private CharacterWheel[] _characterWheelsRotatedStraight;
        private CharacterWheel[] _characterWheelsRotatedBackwards;

        public CharacterMovement(
            IInputService inputService,
            Transform characterTransform, Rigidbody characterRigidBody, CharacterWheelGroups wheelGroups,
            CharacterMovementData movementData) {

            _inputService = inputService;
            _characterTransform = characterTransform;

            CreateAllWheelArrays(characterRigidBody, wheelGroups, movementData);
        }

        public void HandleInput() {
            Vector2 movementInput = _inputService.MoveInputValue;

            _wheelAccelerationInput = movementInput.y;

            _wheelRotationInput = movementInput.x;
        }

        public void Update() {
            RotateWheels();
        }

        public void FixedUpdate() {
            ApplyRotationToWheelGroup(_wheelRotationInput, _characterWheelsRotatedStraight);
            ApplyRotationToWheelGroup(-_wheelRotationInput, _characterWheelsRotatedBackwards);

            ApplyPassiveForceToAllWheelGroups();

            ApplyAccelerationForceToWheelGroup();
        }

        private void ApplyAccelerationForceToWheelGroup() {
            foreach (CharacterWheel wheel in _characterWheelsAccelerated)
                wheel.ApplyAccelerationForce(_wheelAccelerationInput);
        }

        private void ApplyPassiveForceToAllWheelGroups() {
            ApplyPassiveForceToWheelGroup(_characterWheelsAccelerated);
            ApplyPassiveForceToWheelGroup(_characterWheelsRotatedStraight);
            ApplyPassiveForceToWheelGroup(_characterWheelsRotatedBackwards);
        }

        private void ApplyPassiveForceToWheelGroup(CharacterWheel[] characterWheels) {
            foreach (CharacterWheel wheel in characterWheels)
                wheel.FindContactWithGround();

            foreach (CharacterWheel wheel in characterWheels)
                wheel.ApplyPassiveForce();
        }

        private void ApplyRotationToWheelGroup(float wheelRotationInput, CharacterWheel[] characterWheelsRotated) {
            foreach (CharacterWheel wheel in characterWheelsRotated)
                wheel.SetWheelRotation(wheelRotationInput);
        }

        private void CreateAllWheelArrays(Rigidbody characterRigidBody, CharacterWheelGroups wheelTransforms, CharacterMovementData movementData) {
            _characterWheelsAccelerated = CreateWheelArray(
                            wheelTransforms.CharacterWheelTransformsAccelerated, characterRigidBody, movementData);
            _characterWheelsRotatedStraight = CreateWheelArray(
                            wheelTransforms.CharacterWheelTransformsRotatedStraight, characterRigidBody, movementData);
            _characterWheelsRotatedBackwards = CreateWheelArray(
                            wheelTransforms.CharacterWheelTransformsRotatedBackwards, characterRigidBody, movementData);
        }

        private CharacterWheel[] CreateWheelArray(
            List<CharacterWheelProfile> wheelTransforms, Rigidbody characterRigidbody, CharacterMovementData movementData) {

            CharacterWheel[] wheels = new CharacterWheel[wheelTransforms.Count];

            for (int i = 0; i < wheelTransforms.Count; i++) {
                wheels[i] =
                    new CharacterWheel(
                        wheelTransforms[i].WheelTransform,
                        movementData.CarCollisionLayerMask,
                        characterRigidbody,
                        movementData.MaxWheelRotationDegrees,
                        movementData.WheelRestDistance,
                        movementData.WheelSpringStrength,
                        movementData.WheelSpringDamper,
                        movementData.WheelDiameter,
                        wheelTransforms[i].WheelTractionProfile.TractionProfile,
                        movementData.MaxSpeed,
                        movementData.MaxSpeedBackwards,
                        movementData.EnginePower);

                wheels[i].TryGetWheelChild();
            }

            return wheels;
        }

        private void RotateWheels() {
            _characterTransform.Rotate(new Vector3(0, _wheelRotationDegree * Time.deltaTime, 0));
        }
    }

    public class CharacterWheel
    {
        private const float EPSILON = 1E-06f;
        private const float BRAKING_COEFFICIENT = .14f;
        private const float HARD_BRAKING_COEFFICIENT = .42f;

        private readonly Transform _wheelTransform;
        private readonly LayerMask _characterCollisionLayerMask;
        private readonly Rigidbody _characterRigidbody;
        private readonly float _maxWheelRotationDegrees;
        private readonly float _springRestDistance;
        private readonly float _springStrength;
        private readonly float _springDamper;
        private readonly float _diameter;
        private readonly AnimationCurve _steeringTraction;
        private readonly float _maxSpeed;
        private readonly float _maxSpeedBackwards;
        private readonly AnimationCurve _enginePower;

        private bool _hasChild = false;
        private Transform _wheelChild;

        private bool _hasContactWithGround;
        private RaycastHit _groundRaycastHit;

        private Vector3 _springAxis;
        private Vector3 _steeringAxis;
        private Vector3 _accelerationAxis;
        private Vector3 _wheelVelocityInWorld;
        private float _currentVelocityZ;

        public CharacterWheel(
            Transform wheelTransform, LayerMask characterCollisionLayerMask,
            Rigidbody characterRigidbody,
            float maxWheelRotationDegrees, float springRestDistance, float springStrength, float springDamper, float diameter,
            AnimationCurve steeringTraction,
            float maxSpeed, float maxSpeedBackwards, AnimationCurve enginePower) {

            _wheelTransform = wheelTransform;
            _characterCollisionLayerMask = characterCollisionLayerMask;
            _characterRigidbody = characterRigidbody;
            _maxWheelRotationDegrees = maxWheelRotationDegrees;
            _springRestDistance = springRestDistance;
            _springStrength = springStrength;
            _springDamper = springDamper;
            _diameter = diameter;
            _steeringTraction = steeringTraction;
            _maxSpeed = maxSpeed;
            _maxSpeedBackwards = maxSpeedBackwards;
            _enginePower = enginePower;
        }

        public void TryGetWheelChild() {
            if (_wheelTransform.childCount == 0)
                return;

            _hasChild = true;
            _wheelChild = _wheelTransform.GetChild(0);
        }

        public void FindContactWithGround() {
            CastRayToGround();

            VelocityAndAxisPreparation();
        }

        public void ApplyPassiveForce() {
            if (!_hasContactWithGround) {
                SetChildPosition(_springRestDistance);
                return;
            }

            Vector3 summaryPassiveForce =
                CalculateSpringAxisForce(_groundRaycastHit) +
                CalculateSteeringAxisForce();

            _characterRigidbody.AddForceAtPosition(summaryPassiveForce, _wheelTransform.position);

            SetChildPosition(_groundRaycastHit.distance);
        }

        public void ApplyAccelerationForce(float wheelAccelerationInput) {
            if (!_hasContactWithGround)
                return;

            Vector3 accelerationForce = CalculateAccelerationAxisForce(wheelAccelerationInput);

            _characterRigidbody.AddForceAtPosition(accelerationForce, _wheelTransform.position);
        }

        public void SetWheelRotation(float wheelRotationInput) =>
            _wheelTransform.localRotation = Quaternion.Euler(0, wheelRotationInput * _maxWheelRotationDegrees, 0);

        private void CastRayToGround() {
            _hasContactWithGround =
                Physics.Raycast(
                    _wheelTransform.position,
                    -_wheelTransform.up,
                    out _groundRaycastHit,
                    _springRestDistance,
                    _characterCollisionLayerMask);
        }

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
            float normalizedVelocityZ;
            float forceZ;

            if (wheelAccelerationInput > EPSILON) {
                if (velocityZ > _maxSpeed)
                    return Vector3.zero;

                if (velocityZ > 0) {
                    normalizedVelocityZ = Mathf.Clamp01(velocityZ / _maxSpeed);
                    forceZ = _enginePower.Evaluate(normalizedVelocityZ) * wheelAccelerationInput;
                } else {
                    forceZ = _enginePower.Evaluate(0) * wheelAccelerationInput;
                    forceZ = forceZ - velocityZ * HARD_BRAKING_COEFFICIENT;
                }
            } else if (wheelAccelerationInput < -EPSILON) {
                if (velocityZ < _maxSpeedBackwards)
                    return Vector3.zero;

                if (velocityZ < 0) {
                    normalizedVelocityZ = Mathf.Clamp01(Mathf.Abs(velocityZ) / _maxSpeedBackwards);
                    forceZ = _enginePower.Evaluate(normalizedVelocityZ) * wheelAccelerationInput;
                } else {
                    forceZ = _enginePower.Evaluate(0) * wheelAccelerationInput;
                    forceZ = forceZ - velocityZ * HARD_BRAKING_COEFFICIENT;
                }
            } else
                forceZ = -velocityZ * BRAKING_COEFFICIENT;

            forceZ /= Time.fixedDeltaTime;

            return _accelerationAxis * forceZ;
        }

        private void SetChildPosition(float distanceToChild) {
            if (_hasChild)
                _wheelChild.position = _wheelTransform.position - _wheelTransform.up * (distanceToChild - _diameter);
        }
    }
}
