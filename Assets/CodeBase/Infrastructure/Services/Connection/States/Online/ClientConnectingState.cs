using Assets.CodeBase.Infrastructure.Services.Connection.States.Offline;
using Assets.CodeBase.Infrastructure.Services.Network;
using Assets.CodeBase.Infrastructure.Services.SessionData;
using Assets.CodeBase.Infrastructure.StateMachine;
using System;
using UnityEngine;

namespace Assets.CodeBase.Infrastructure.Services.Connection.States.Online
{
    public class ClientConnectingState : OnlineState
    {
        public ClientConnectingState(
            IStateMachine connectionStateMachine, IStateMachine gameStateMachine,
            INetworkService networkService, IConnectionService connectionService, ISessionDataService sessionData)
            : base(connectionStateMachine, gameStateMachine, networkService, connectionService, sessionData) { }

        public override void Enter() {
            Debug.Log("Starting Client State");

            StartClient();
        }

        public override void Exit() { }

        public override void OnClientConnected(ulong clientId) {
            _connectionStateMachine.Enter<ClientConnectedState>();
        }

        public override void OnClientDisconnect(ulong clientId) =>
            StartingClientFailed();

        private void StartClient() {
            try {
                SetupConnection();

                if (!_networkService.NetworkManager.StartClient())
                    StartingClientFailed();

            } catch (Exception e) {
                Debug.LogError("Error connecting client, see following exception");
                Debug.LogException(e);

                StartingClientFailed();
                throw;
            }
        }

        private void StartingClientFailed() {
            Debug.LogWarning("Starting client failed");

            _connectionStateMachine.Enter<OfflineState>();
        }
    }
}
