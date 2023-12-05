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
        private readonly INetworkService _networkService;

        private string _playerName;
        private string _ipaddress;
        private int _port;

        public string PlayerName => _playerName;
        public string IPAddress => _ipaddress;
        public int Port => _port;

        public ConnectionService(IStateMachine gameStateMachine, INetworkService networkService, ISessionDataService sessionData) {
            _networkService = networkService;

            _states = new Dictionary<Type, IConnectionExitableState>() {
                [typeof(OfflineState)] = new OfflineState(this, gameStateMachine, networkService),
                [typeof(StartingHostState)] = new StartingHostState(this, gameStateMachine, networkService, this, sessionData),
                [typeof(HostingState)] = new HostingState(this, gameStateMachine, networkService, this, sessionData)
            };
        }

        public void Initialize() {
            _networkService.NetworkManager.ConnectionApprovalCallback += 
                (request, response) => _activeState.ApprovalCheck(request, response);
            _networkService.NetworkManager.OnServerStarted += () => _activeState.OnServerStarted();
            _networkService.NetworkManager.OnServerStopped += _ => _activeState.OnServerStopped();
            _networkService.NetworkManager.OnTransportFailure += () => _activeState.OnTransportFailure();
        }

        public void StartHostIP(string playerName, string ipaddress, int port) {
            _playerName = playerName;
            _ipaddress = ipaddress;
            _port = port;

            _activeState.StartHostIP();
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

        void RequestShutdown();
        void StartHostIP(string playerName, string ipaddress, int port);
    }
}
