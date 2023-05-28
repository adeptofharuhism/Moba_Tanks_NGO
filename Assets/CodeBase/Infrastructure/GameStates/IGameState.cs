using Assets.CodeBase.Infrastructure.StateMachine;

namespace Assets.CodeBase.Infrastructure.GameStates
{
    public interface IGameExitableState : IExitableState { }
    public interface IGameState : IGameExitableState, IState { }
    public interface IGamePayloadedState<TPayload> : IGameExitableState, IPayloadedState<TPayload> { }
}
