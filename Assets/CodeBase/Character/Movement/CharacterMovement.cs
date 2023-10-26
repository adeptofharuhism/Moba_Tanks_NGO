using Assets.CodeBase.Infrastructure.Properties;
using Assets.CodeBase.Infrastructure.Services.Input;
using System;
using UnityEngine;

namespace Assets.CodeBase.Character.Movement
{
    [Serializable]
    public class CharacterMovement : IUpdatable
    {
        private readonly IInputService _inputService;
        private readonly CharacterController _characterController;
        private readonly float _movementSpeed;

        private Vector3 _movementDirection;

        public CharacterMovement(IInputService inputService, CharacterController characterController, float movementSpeed) {
            _inputService = inputService;
            _characterController = characterController;
            _movementSpeed = movementSpeed;
        }

        public void FixedUpdate() {

        }

        public void HandleInput() {
            Vector3 movementInput = _inputService.MoveInputValue;
            _movementDirection = new Vector3(movementInput.x, 0, movementInput.y);
        }

        public void Update() {
            _characterController.Move(_movementDirection * Time.deltaTime * _movementSpeed);
        }
    }
}
