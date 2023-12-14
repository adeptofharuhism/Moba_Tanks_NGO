using Assets.CodeBase.Infrastructure.Services.Connection.States.Offline;
using Assets.CodeBase.Infrastructure.Services.Network;
using Assets.CodeBase.Infrastructure.Services.SessionData;
using Assets.CodeBase.Infrastructure.StateMachine;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Assets.CodeBase.Infrastructure.Services.Connection.States.Online
{
    public class StartingHostState : OnlineState
    {
        public StartingHostState(
            IStateMachine connectionStateMachine, IStateMachine gameStateMachine,
            INetworkService networkService, IConnectionService connectionService, ISessionDataService sessionData)
            : base(connectionStateMachine, gameStateMachine, networkService, connectionService, sessionData) { }

        public override void Enter() {
            Debug.Log("Starting Host State");

            StartHost();
        }

        public override void Exit() { }

        public override void OnServerStarted() {
            Debug.Log("Successfuly connected");

            _connectionStateMachine.Enter<HostingState>();
        }

        public override void OnServerStopped() =>
            StartHostFailed();

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
            ulong clientId = request.ClientNetworkId;

            if (clientId != _networkService.NetworkManager.LocalClientId)
                return;

            byte[] connectionData = request.Payload;
            string payload = System.Text.Encoding.UTF8.GetString(connectionData);
            ConnectionPayload connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

            _sessionData.SetupConnectingPlayerSessionData(clientId, connectionPayload.PlayerId,
                new SessionPlayerData(clientId, connectionPayload.PlayerName, true));

            response.Approved = true;
            response.CreatePlayerObject = true;
        }

        private void StartHost() {
            try {
                SetupConnection();

                if (!_networkService.NetworkManager.StartHost())
                    StartHostFailed();

            } catch (Exception) {
                StartHostFailed();
                throw;
            }
        }

        private void StartHostFailed() {
            Debug.Log("Starting Host Failed");

            _connectionStateMachine.Enter<OfflineState>();
        }
    }
}
