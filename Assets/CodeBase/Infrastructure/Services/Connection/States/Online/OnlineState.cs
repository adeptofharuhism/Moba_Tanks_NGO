using Assets.CodeBase.Infrastructure.Services.Connection.States.Offline;
using Assets.CodeBase.Infrastructure.Services.Network;
using Assets.CodeBase.Infrastructure.Services.SessionData;
using Assets.CodeBase.Infrastructure.StateMachine;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Assets.CodeBase.Infrastructure.Services.Connection.States.Online
{
    public abstract class OnlineState : ConnectionState
    {
        protected const string ClientGUIDKey = "client_guid";

        protected readonly IConnectionService _connectionService;
        protected readonly ISessionDataService _sessionData;

        public OnlineState(
            IStateMachine connectionStateMachine, IStateMachine gameStateMachine,
            INetworkService networkService, IConnectionService connectionService, ISessionDataService sessionData)
            : base(connectionStateMachine, gameStateMachine, networkService) {

            _connectionService = connectionService;
            _sessionData = sessionData;
        }

        public override void OnTransportFailure() {
            _connectionStateMachine.Enter<OfflineState>();
        }

        public override void OnUserRequestedShutdown() {
            _connectionStateMachine.Enter<OfflineState>();
        }

        protected void SetupConnection() {
            SetConnectionPayload(_connectionService.PlayerGuid, _connectionService.PlayerName);
            UnityTransport utp = (UnityTransport)_networkService.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(_connectionService.IPAddress, (ushort)_connectionService.Port);
        }

        private void SetConnectionPayload(string playerId, string playerName) {
            string payload = JsonUtility.ToJson(new ConnectionPayload() {
                PlayerId = playerId,
                PlayerName = playerName,
                IsDebug = Debug.isDebugBuild
            });

            byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            _networkService.NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
        }
    }
}
