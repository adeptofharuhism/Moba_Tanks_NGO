using Assets.CodeBase.Infrastructure.StateMachine;

namespace Assets.CodeBase.Infrastructure.Services.Connection.States
{
    public interface IConnectionExitableState : IExitableState {
        public void StartHostIP(string playerName, string ipaddress, int port);
    }

    public interface IConnectionState : IConnectionExitableState, IState { }
}
