using Assets.CodeBase.Character.Movement;
using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.Input;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CodeBase.Character
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private Rigidbody _characterRigidBody;
        [SerializeField] private CharacterMovementData _movementData;
        [SerializeField] private List<CharacterWheelProfile> _wheelProfiles;

        private CharacterMovement _movement;

        public void Construct(IInputService inputService) {
            _movement = new CharacterMovement(inputService, _characterRigidBody, _movementData, _wheelProfiles);
        }

        private void Awake() {
            Construct(AllServices.Container.Single<IInputService>());
        }

        private void Update() {
            UpdateMovement();
        }

        private void FixedUpdate() {
            FixedUpdateMovement();
        }

        private void UpdateMovement() {
            _movement.HandleInput();
            _movement.Update();
        }

        private void FixedUpdateMovement() {
            _movement.FixedUpdate();
        }
    }

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
