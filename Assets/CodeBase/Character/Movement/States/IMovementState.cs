using Assets.CodeBase.Infrastructure.Properties;
using Assets.CodeBase.Infrastructure.StateMachine;

namespace Assets.CodeBase.Character.Movement.MovementStates
{
    public interface IMovementExitableState : IExitableState, IUpdatable { };
    public interface IMovementState : IMovementExitableState, IState { };
    public interface IMovementPayloadedState<TPayload> : IMovementExitableState, IPayloadedState<TPayload> { };
}
