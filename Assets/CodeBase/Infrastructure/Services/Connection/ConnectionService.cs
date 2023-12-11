using Assets.CodeBase.Infrastructure.Services.Connection.States;
using Assets.CodeBase.Infrastructure.Services.Network;
using Assets.CodeBase.Infrastructure.Services.SessionData;
using Assets.CodeBase.Infrastructure.StateMachine;
using System;
using System.Collections.Generic;

namespace Assets.CodeBase.Infrastructure.Services.Connection
{
    [Serializable]
    public class ConnectionPayload
    {
        public string PlayerId;
        public string PlayerName;
        public bool IsDebug;
    }

    public class ConnectionService : BaseStateMachine<IConnectionExitableState>, IConnectionService
    {
        public const int MaxConnectedPlayersConst = 4;

        private readonly INetworkService _networkService;

        private string _playerName;
        private string _ipaddress;
        private int _port;

        public string PlayerName => _playerName;
        public string IPAddress => _ipaddress;
        public int Port => _port;
        public int MaxConnectedPlayers => MaxConnectedPlayersConst;

        public ConnectionService(IStateMachine gameStateMachine, INetworkService networkService, ISessionDataService sessionData) {
            _networkService = networkService;

            _states = new Dictionary<Type, IConnectionExitableState>() {
                [typeof(OfflineState)] = new OfflineState(this, gameStateMachine, networkService),
                [typeof(StartingHostState)] = new StartingHostState(this, gameStateMachine, networkService, this, sessionData),
                [typeof(HostingState)] = new HostingState(this, gameStateMachine, networkService, this, sessionData),
                [typeof(ClientConnectingState)] = new ClientConnectingState(this, gameStateMachine, networkService, this, sessionData),
                [typeof(ClientConnectedState)] = new ClientConnectedState(this, gameStateMachine, networkService, this, sessionData)
            };

            _ready = true;
        }

        public void Initialize() {
            _networkService.NetworkManager.ConnectionApprovalCallback += 
                (request, response) => _activeState.ApprovalCheck(request, response);

            _networkService.NetworkManager.OnServerStarted += () => _activeState.OnServerStarted();
            _networkService.NetworkManager.OnTransportFailure += () => _activeState.OnTransportFailure();

            _networkService.NetworkManager.OnServerStopped += _ => _activeState.OnServerStopped();
            _networkService.NetworkManager.OnClientConnectedCallback += clientId => _activeState.OnClientConnected(clientId);
            _networkService.NetworkManager.OnClientDisconnectCallback += clientId => _activeState.OnClientDisconnect(clientId);
        }

        public void StartHostIP(string playerName, string ipaddress, int port) {
            _playerName = playerName;
            _ipaddress = ipaddress;
            _port = port;

            _activeState.StartHostIP();
        }

        public void StartClientIP(string playerName, string ipaddress, int port) {
            _playerName = playerName;
            _ipaddress = ipaddress;
            _port = port;

            _activeState.StartClientIP();
        }

        public void RequestShutdown() {
            _activeState.OnUserRequestedShutdown();
        }
    }

    public interface IConnectionService : IService
    {
        string PlayerName { get; }
        string IPAddress { get; }
        int Port { get; }
        int MaxConnectedPlayers { get; }

        void RequestShutdown();
        void StartClientIP(string playerName, string ipaddress, int port);
        void StartHostIP(string playerName, string ipaddress, int port);
    }
}
