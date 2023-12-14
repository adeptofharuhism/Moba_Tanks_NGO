using Assets.CodeBase.Infrastructure.Services.Connection.States.Offline;
using Assets.CodeBase.Infrastructure.Services.Network;
using Assets.CodeBase.Infrastructure.Services.SessionData;
using Assets.CodeBase.Infrastructure.StateMachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.CodeBase.Infrastructure.Services.Connection.States.Online
{
    public class HostingState : OnlineState
    {
        private const int MaxConnectPayload = 1024;

        public HostingState(
            IStateMachine connectionStateMachine, IStateMachine gameStateMachine,
            INetworkService networkService, IConnectionService connectionService, ISessionDataService sessionData)
            : base(connectionStateMachine, gameStateMachine, networkService, connectionService, sessionData) { }

        public override void Enter() {
            Debug.Log("Hosting State");

            _networkService.NetworkManager.SceneManager.LoadScene(Constants.SceneNames.LobbyMenu, LoadSceneMode.Single);
        }

        public override void Exit() {
            _sessionData.OnServerEnded();
        }

        public override void OnClientDisconnect(ulong clientId) {
            if (clientId == _networkService.NetworkManager.LocalClientId)
                return;

            _sessionData.DisconnectClient(clientId);
        }

        public override void OnServerStopped() =>
            _connectionStateMachine.Enter<OfflineState>();

        public override void OnUserRequestedShutdown() {
            DisconnectAllClients();

            base.OnUserRequestedShutdown();
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
            response.Approved = false;

            byte[] connectionData = request.Payload;
            if (connectionData.Length > MaxConnectPayload)
                return;

            if (_networkService.NetworkManager.ConnectedClientsIds.Count >= _connectionService.MaxConnectedPlayers)
                return;

            string payload = System.Text.Encoding.UTF8.GetString(connectionData);
            ConnectionPayload connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

            if (connectionPayload.IsDebug != Debug.isDebugBuild)
                return;

            ulong clientId = request.ClientNetworkId;
            _sessionData.SetupConnectingPlayerSessionData(clientId, connectionPayload.PlayerId,
                new SessionPlayerData(clientId, connectionPayload.PlayerName, true));

            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = Vector3.zero;
            response.Rotation = Quaternion.identity;
        }

        private void DisconnectAllClients() {
            for (int i = _networkService.NetworkManager.ConnectedClientsIds.Count - 1; i >= 0; i--) {
                ulong id = _networkService.NetworkManager.ConnectedClientsIds[i];
                if (id != _networkService.NetworkManager.LocalClientId)
                    _networkService.NetworkManager.DisconnectClient(id);
            }
        }
    }
}
