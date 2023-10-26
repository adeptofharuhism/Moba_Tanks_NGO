using Assets.CodeBase.Character.Movement;
using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.Input;
using UnityEngine;

namespace Assets.CodeBase.Character
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private CharacterMovementData _movementData;
        [SerializeField] private CharacterController _controller;

        private CharacterMovement _movement;

        public void Construct(IInputService inputService) {
            _movement = new CharacterMovement(inputService, _controller, _movementData.Speed);
        }

        private void Awake() {
            Construct(AllServices.Container.Single<IInputService>());
        }

        private void Update() {
            UpdateMovement();
        }

        private void UpdateMovement() {
            _movement.HandleInput();
            _movement.Update();
        }
    }
}
