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
        private const AnimationCurve STEERING_TRACTION = null;

        private readonly IInputService _inputService;
        private readonly Transform _characterTransform;

        private float _maxWheelRotationDegrees = 0;
        private float _wheelAccelerationInput = 0;
        private float _wheelRotationDegree = 0;

        private CharacterWheel[] _characterWheelsUnrotated;
        private CharacterWheel[] _characterWheelsRotatedStraight;
        private CharacterWheel[] _characterWheelsRotatedBackwards;

        public CharacterMovement(
            IInputService inputService, 
            Transform characterTransform, Rigidbody characterRigidBody, CharacterWheelTransforms wheelTransforms, 
            CharacterMovementData movementData) {

            _inputService = inputService;
            _characterTransform = characterTransform;

            _maxWheelRotationDegrees = movementData.MaxWheelRotationDegrees;

            CreateAllWheelArrays(characterRigidBody, wheelTransforms, movementData);
        }

        public void HandleInput() {
            Vector2 movementInput = _inputService.MoveInputValue;

            _wheelAccelerationInput = movementInput.y;

            SetWheelRotationInput(movementInput);
        }

        public void Update() {
            RotateWheels();
        }

        public void FixedUpdate() {
            ApplyForcesToAllWheelGroups();
        }

        private void ApplyForcesToAllWheelGroups() {
            ApplyForcesToWheelGroup(_characterWheelsUnrotated);
            ApplyForcesToWheelGroup(_characterWheelsRotatedStraight);
            ApplyForcesToWheelGroup(_characterWheelsRotatedBackwards);
        }

        private void ApplyForcesToWheelGroup(CharacterWheel[] characterWheels) {
            foreach (CharacterWheel wheel in characterWheels) {
                wheel.ApplyForce(_wheelAccelerationInput);
            }
        }

        private void CreateAllWheelArrays(Rigidbody characterRigidBody, CharacterWheelTransforms wheelTransforms, CharacterMovementData movementData) {
            _characterWheelsUnrotated = CreateWheelArray(
                            wheelTransforms.CharacterWheelTransformsUnrotated, characterRigidBody, movementData);
            _characterWheelsRotatedStraight = CreateWheelArray(
                            wheelTransforms.CharacterWheelTransformsRotatedStraight, characterRigidBody, movementData);
            _characterWheelsRotatedBackwards = CreateWheelArray(
                            wheelTransforms.CharacterWheelTransformsRotatedBackwards, characterRigidBody, movementData);
        }

        private CharacterWheel[] CreateWheelArray(List<Transform> wheelTransforms, Rigidbody characterRigidbody, CharacterMovementData movementData) {
            CharacterWheel[] wheels = new CharacterWheel[wheelTransforms.Count];

            for (int i = 0; i < wheelTransforms.Count; i++) {
                wheels[i] =
                    new CharacterWheel(
                        wheelTransforms[i],
                        movementData.CarCollisionLayerMask,
                        characterRigidbody,
                        movementData.WheelRestDistance,
                        movementData.WheelSpringStrength,
                        movementData.WheelSpringDamper,
                        movementData.WheelDiameter,
                        STEERING_TRACTION,
                        movementData.MaxSpeed,
                        movementData.EnginePower);

                wheels[i].TryGetWheelChild();
            }

            return wheels;
        }

        private void RotateWheels() {
            _characterTransform.Rotate(new Vector3(0, _wheelRotationDegree * Time.deltaTime, 0));
        }

        private void SetWheelRotationInput(Vector2 movementInput) =>
            _wheelRotationDegree = _maxWheelRotationDegrees * movementInput.x;
    }

    public class CharacterWheel
    {
        private readonly Transform _wheelTransform;
        private readonly LayerMask _characterCollisionLayerMask;
        private readonly Rigidbody _characterRigidbody;
        private readonly float _springRestDistance;
        private readonly float _springStrength;
        private readonly float _springDamper;
        private readonly float _diameter;
        private readonly AnimationCurve _steeringTraction;
        private readonly float _maxSpeed;
        private readonly AnimationCurve _enginePower;

        private bool _hasChild = false;
        private Transform _wheelChild;

        private Vector3 _springAxis;
        private Vector3 _steeringAxis;
        private Vector3 _accelerationAxis;
        private Vector3 _wheelVelocityInWorld;

        public CharacterWheel(
            Transform wheelTransform, LayerMask characterCollisionLayerMask,
            Rigidbody characterRigidbody,
            float springRestDistance, float springStrength, float springDamper, float diameter,
            AnimationCurve steeringTraction,
            float maxSpeed, AnimationCurve enginePower) {

            _wheelTransform = wheelTransform;
            _characterCollisionLayerMask = characterCollisionLayerMask;
            _characterRigidbody = characterRigidbody;
            _springRestDistance = springRestDistance;
            _springStrength = springStrength;
            _springDamper = springDamper;
            _diameter = diameter;
            _steeringTraction = steeringTraction;
            _enginePower = enginePower;
            _maxSpeed = maxSpeed;
        }

        public void TryGetWheelChild() {
            if (_wheelTransform.childCount > 0) {
                _hasChild = true;
                _wheelChild = _wheelTransform.GetChild(0);
            }
        }

        public void ApplyForce(float wheelAccelerationInput) {
            RaycastHit hitInfo;
            bool rayDidHit;
            CastRayToSurface(out hitInfo, out rayDidHit);

            if (rayDidHit) {
                VelocityAndAxisPreparation();

                Vector3 springForce = CalculateSpringAxisForce(hitInfo);
                Vector3 steeringForce = CalculateSteeringAxisForce();
                Vector3 accelerationForce = CalculateAccelerationAxisForce(wheelAccelerationInput);

                Vector3 summaryForce = springForce + steeringForce + accelerationForce;
                _characterRigidbody.AddForceAtPosition(summaryForce, _wheelTransform.position);

                SetChildPosition(hitInfo.distance);

                ShowForcesInEditor(springForce, steeringForce, accelerationForce);
            } else {
                SetChildPosition(_springRestDistance);
            }
        }

        private void CastRayToSurface(out RaycastHit hitInfo, out bool rayDidHit) {
            rayDidHit = Physics.Raycast(
                    _wheelTransform.position,
                    -_wheelTransform.up,
                    out hitInfo,
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
            float offsetFromRestY = _springRestDistance - hitInfo.distance;
            float velocityY = Vector3.Dot(_springAxis, _wheelVelocityInWorld);
            float forceY = (offsetFromRestY * _springStrength) - (velocityY * _springDamper);
            Vector3 springForce = _springAxis * forceY;
            return springForce;
        }

        private Vector3 CalculateSteeringAxisForce() {
            float velocity = Vector3.Dot(_steeringAxis, _wheelVelocityInWorld);
            float force = velocity * 1f / Time.fixedDeltaTime;
            Vector3 steeringForce = -_steeringAxis * force;
            return steeringForce;
        }

        private Vector3 CalculateAccelerationAxisForce(float wheelAccelerationInput) {
            float velocityZ = Vector3.Dot(_accelerationAxis, _wheelVelocityInWorld);
            float normalizedVelocityZ = Mathf.Clamp01(Mathf.Abs(velocityZ) / _maxSpeed);
            float forceZ = _enginePower.Evaluate(normalizedVelocityZ) * wheelAccelerationInput;
            Vector3 accelerationForce = _accelerationAxis * forceZ;
            return accelerationForce;
        }

        private void ShowForcesInEditor(Vector3 springForce, Vector3 steeringForce, Vector3 accelerationForce) {
            Debug.DrawLine(_wheelTransform.position, _wheelTransform.position + (springForce), Color.green, Time.deltaTime);
            Debug.DrawLine(_wheelTransform.position, _wheelTransform.position + (steeringForce), Color.red, Time.deltaTime);
            Debug.DrawLine(_wheelTransform.position, _wheelTransform.position + (accelerationForce), Color.blue, Time.deltaTime);
        }

        private void SetChildPosition(float distanceToChild) {
            if (_hasChild)
                _wheelChild.position = _wheelTransform.position - _wheelTransform.up * (distanceToChild - _diameter);
        }
    }
}
