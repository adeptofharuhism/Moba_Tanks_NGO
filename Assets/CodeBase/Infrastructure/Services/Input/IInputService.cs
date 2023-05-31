using UnityEngine;

namespace Assets.CodeBase.Infrastructure.Services.Input
{
    public interface IInputService: IService
    {
        Vector2 MoveInputValue { get; }

        void Initialize();
        void Enable();
        void Disable();

        delegate void EventZeroParameters();
        event EventZeroParameters MovementStarted;
        event EventZeroParameters MovementCancelled;
    }
}
