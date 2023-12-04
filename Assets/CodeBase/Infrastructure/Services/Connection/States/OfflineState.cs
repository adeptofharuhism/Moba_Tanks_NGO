using Assets.CodeBase.Infrastructure.GameStates;
using Assets.CodeBase.Infrastructure.Services.Network;
using Assets.CodeBase.Infrastructure.StateMachine;
using System;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.CodeBase.Infrastructure.Services.Connection.States
{
    public interface IConnectionExitableState : IExitableState
    {
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

        public override void StartHostIP() =>
            _connectionStateMachine.Enter<StartingHostState>();
    }

    public abstract class OnlineState : ConnectionState
    {
        protected const string ClientGUIDKey = "client_guid";

        protected readonly IConnectionService _connectionService;

        public OnlineState(
            IStateMachine connectionStateMachine, IStateMachine gameStateMachine, INetworkService networkService, IConnectionService connectionService)
            : base(connectionStateMachine, gameStateMachine, networkService) {

            _connectionService = connectionService;
        }

        public override void OnTransportFailure() {
            _connectionStateMachine.Enter<OfflineState>();
        }

        public override void OnUserRequestedShutdown() {
            _connectionStateMachine.Enter<OfflineState>();
        }
    }

    public class HostingState : OnlineState
    {
        public HostingState(IStateMachine connectionStateMachine, IStateMachine gameStateMachine, INetworkService networkService, IConnectionService connectionService) : base(connectionStateMachine, gameStateMachine, networkService, connectionService) {
        }

        public override void Enter() {
            Debug.Log("Hosting State");


        }

        public override void Exit() { }
    }

    public class StartingHostState : OnlineState
    {
        public StartingHostState(
            IStateMachine connectionStateMachine, IStateMachine gameStateMachine, INetworkService networkService, IConnectionService connectionService)
            : base(connectionStateMachine, gameStateMachine, networkService, connectionService) { }

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

        private void StartHost() {
            try {
                SetupHostConnectionAsync();

                if (!_networkService.NetworkManager.StartHost())
                    StartHostFailed();

            } catch (Exception) {
                StartHostFailed();
                throw;
            }
        }

        private void SetupHostConnectionAsync() {
            SetConnectionPayload(GetPlayerGuid(), _connectionService.PlayerName);
            UnityTransport utp = (UnityTransport)_networkService.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(_connectionService.IPAddress, (ushort)_connectionService.Port);
        }

        private string GetPlayerGuid() =>
            Guid.NewGuid().ToString();

        private void SetConnectionPayload(string playerId, string playerName) {
            string payload = JsonUtility.ToJson(new ConnectionPayload() {
                PlayerId = playerId,
                PlayerName = playerName,
                IsDebug = Debug.isDebugBuild
            });

            byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            _networkService.NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
        }

        private void StartHostFailed() {
            Debug.Log("Starting Host Failed");
            _connectionStateMachine.Enter<OfflineState>();
        }
    }
}
