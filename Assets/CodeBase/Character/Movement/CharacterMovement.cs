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
}
