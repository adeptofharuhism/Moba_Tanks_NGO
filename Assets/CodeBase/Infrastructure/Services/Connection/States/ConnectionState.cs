using Assets.CodeBase.Infrastructure.Services.Network;
using Assets.CodeBase.Infrastructure.StateMachine;
using Unity.Netcode;

namespace Assets.CodeBase.Infrastructure.Services.Connection.States
{
    public abstract class ConnectionState : IConnectionState
    {
        protected readonly IStateMachine _connectionStateMachine;
        protected readonly IStateMachine _gameStateMachine;
        protected readonly INetworkService _networkService;

        public ConnectionState(IStateMachine connectionStateMachine, IStateMachine gameStateMachine, INetworkService networkService) {
            _connectionStateMachine = connectionStateMachine;
            _gameStateMachine = gameStateMachine;
            _networkService = networkService;
        }

        public abstract void Enter();
        public abstract void Exit();

        public virtual void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) { }

        public virtual void OnClientConnected(ulong clientId) { }
        public virtual void OnClientDisconnect(ulong clientId) { }
        public virtual void OnServerStarted() { }
        public virtual void OnServerStopped() { }
        public virtual void OnTransportFailure() { }
        public virtual void OnUserRequestedShutdown() { }

        public virtual void StartClientIP() { }
        public virtual void StartHostIP() { }
    }
}
