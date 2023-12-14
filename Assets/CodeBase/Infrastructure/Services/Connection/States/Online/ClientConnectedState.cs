using Assets.CodeBase.Infrastructure.Services.Connection.States.Offline;
using Assets.CodeBase.Infrastructure.Services.Network;
using Assets.CodeBase.Infrastructure.Services.SessionData;
using Assets.CodeBase.Infrastructure.StateMachine;
using UnityEngine;

namespace Assets.CodeBase.Infrastructure.Services.Connection.States.Online
{
    public class ClientConnectedState : OnlineState
    {
        public ClientConnectedState(
            IStateMachine connectionStateMachine, IStateMachine gameStateMachine,
            INetworkService networkService, IConnectionService connectionService, ISessionDataService sessionData)
            : base(connectionStateMachine, gameStateMachine, networkService, connectionService, sessionData) { }

        public override void Enter() {
            Debug.Log("Client Connected State");
        }

        public override void Exit() { }

        public override void OnClientDisconnect(ulong clientId) =>
            _connectionStateMachine.Enter<OfflineState>();
    }
}
