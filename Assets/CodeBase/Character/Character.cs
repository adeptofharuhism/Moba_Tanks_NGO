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
        //[SerializeField] private CharacterMovementData _movementData;

        [SerializeField] private Rigidbody _characterRigidBody;
        [SerializeField] private CharacterWheelTransforms _wheelTransforms;
        [SerializeField] private CharacterMovementData _movementData;

        private CharacterMovement _movement;
        
        public void Construct(IInputService inputService) {
            _movement = new CharacterMovement(inputService, transform, _characterRigidBody, _wheelTransforms, _movementData);
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
    public class CharacterWheelTransforms
    {
        public List<Transform> CharacterWheelTransformsUnrotated;
        public List<Transform> CharacterWheelTransformsRotatedStraight;
        public List<Transform> CharacterWheelTransformsRotatedBackwards;
    }
}
