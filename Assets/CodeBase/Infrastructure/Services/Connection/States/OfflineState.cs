using Assets.CodeBase.Infrastructure.GameStates;
using Assets.CodeBase.Infrastructure.Services.Network;
using Assets.CodeBase.Infrastructure.Services.SessionData;
using Assets.CodeBase.Infrastructure.StateMachine;
using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.CodeBase.Infrastructure.Services.Connection.States
{
    public interface IConnectionExitableState : IExitableState
    {
        void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response);

        void OnClientConnected();
        void OnClientDisconnect();
        void OnServerStarted();
        void OnServerStopped();
        void OnTransportFailure();
        void OnUserRequestedShutdown();

        void StartClientIP();
        void StartHostIP();
    }

    public interface IConnectionState : IConnectionExitableState, IState { }

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

        public virtual void OnClientConnected() { }
        public virtual void OnClientDisconnect() { }
        public virtual void OnServerStarted() { }
        public virtual void OnServerStopped() { }
        public virtual void OnTransportFailure() { }
        public virtual void OnUserRequestedShutdown() { }

        public virtual void StartClientIP() { }
        public virtual void StartHostIP() { }
    }

    public class OfflineState : ConnectionState
    {
        public OfflineState(IStateMachine connectionStateMachine, IStateMachine gameStateMachine, INetworkService networkService)
            : base(connectionStateMachine, gameStateMachine, networkService) { }

        public override void Enter() {
            Debug.Log("Offline State");

            _networkService.NetworkManager.Shutdown();

            if (SceneManager.GetActiveScene().name != Constants.SceneNames.MainMenu)
                _gameStateMachine.Enter<LoadLevelState, string>(Constants.SceneNames.MainMenu);
        }

        public override void Exit() { }

        public override void StartClientIP() =>
            _connectionStateMachine.Enter<ClientConnectingState>();

        public override void StartHostIP() =>
            _connectionStateMachine.Enter<StartingHostState>();
    }

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
            SetConnectionPayload(GetPlayerGuid(), _connectionService.PlayerName);
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

        private string GetPlayerGuid() =>
            Guid.NewGuid().ToString();
    }

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

        public override void Exit() { }

        public override void OnServerStopped() =>
            _connectionStateMachine.Enter<OfflineState>();

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

            if (_sessionData.IsDuplicateConnection(connectionPayload.PlayerId))
                return;

            ulong clientId = request.ClientNetworkId;
            _sessionData.SetupConnectingPlayerSessionData(clientId, connectionPayload.PlayerId,
                new SessionPlayerData(clientId, connectionPayload.PlayerName, true));

            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = Vector3.zero;
            response.Rotation = Quaternion.identity;
        }
    }

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

        public override void OnClientConnected() {
            _connectionStateMachine.Enter<ClientConnectedState>();
        }

        public override void OnClientDisconnect() =>
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

        public override void OnClientDisconnect() =>
            _connectionStateMachine.Enter<OfflineState>();
    }
}
