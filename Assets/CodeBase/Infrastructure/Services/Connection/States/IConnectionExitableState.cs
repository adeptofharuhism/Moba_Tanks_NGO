using Assets.CodeBase.Infrastructure.StateMachine;
using Unity.Netcode;

namespace Assets.CodeBase.Infrastructure.Services.Connection.States
{
    public interface IConnectionExitableState : IExitableState
    {
        void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response);

        void OnClientConnected(ulong clientId);
        void OnClientDisconnect(ulong clientId);
        void OnServerStarted();
        void OnServerStopped();
        void OnTransportFailure();
        void OnUserRequestedShutdown();

        void StartClientIP();
        void StartHostIP();
    }

    public interface IConnectionState : IConnectionExitableState, IState { }
}
