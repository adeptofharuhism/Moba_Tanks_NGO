using Assets.CodeBase.Infrastructure.Services;

namespace Assets.CodeBase.Infrastructure.StateMachine
{
    public interface IStateMachine: IService
    {
        void Enter<TState>() where TState : class, IState;
        void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>;
    }
}