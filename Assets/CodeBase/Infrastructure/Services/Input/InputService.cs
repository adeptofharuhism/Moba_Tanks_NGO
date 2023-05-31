using System;
using UnityEngine;

namespace Assets.CodeBase.Infrastructure.Services.Input
{
    public class InputService : IInputService
    {
        private Controls _controls;

        public Vector2 MoveInputValue => _controls.Character.Move.ReadValue<Vector2>();

        public event IInputService.EventZeroParameters MovementStarted;
        public event IInputService.EventZeroParameters MovementCancelled;

        public InputService() {
            _controls = new Controls();
        }

        public void Initialize() {
            _controls.Character.Move.started += _ => MovementStarted?.Invoke();
            _controls.Character.Move.canceled += _ => MovementCancelled?.Invoke();
        }

        public void Enable() => 
            _controls.Enable();

        public void Disable() => 
            _controls.Disable();
    }
}
