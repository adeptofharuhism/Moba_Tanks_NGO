using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.Connection;
using Assets.CodeBase.Infrastructure.Services.Connection.States.Offline;
using Assets.CodeBase.Infrastructure.Services.Input;
using Assets.CodeBase.Infrastructure.Services.Network;
using Assets.CodeBase.Infrastructure.Services.SessionData;
using Assets.CodeBase.Infrastructure.StateMachine;
using System;
using Unity.Netcode;

namespace Assets.CodeBase.Infrastructure.GameStates
{
    public class BootstrapState : IGameState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly NetworkManager _networkManager;
        private readonly AllServices _services;

        public BootstrapState(GameStateMachine stateMachine, SceneLoader sceneLoader, NetworkManager networkManager, AllServices services) {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _networkManager = networkManager;
            _services = services;

            RegisterServices();
        }

        public void Enter() => 
            _sceneLoader.Load(Constants.SceneNames.Initial, OnLoaded);

        public void Exit() { }

        private void OnLoaded() => 
            _stateMachine.Enter<LoadLevelState, string>(Constants.SceneNames.MainMenu);

        private void RegisterServices() {
            _services.RegisterSingle<IStateMachine>(_stateMachine);
            _services.RegisterSingle(PrepareInputService());

            _services.RegisterSingle<ISessionDataService>(new SessionDataService());
            _services.RegisterSingle(PrepareNetworkService());
            _services.RegisterSingle(PrepareConnectionService());
        }

        private IInputService PrepareInputService() {
            IInputService inputService = new InputService();
            inputService.Initialize();

            return inputService;
        }

        private INetworkService PrepareNetworkService() {
            INetworkService networkService = new NetworkService(_networkManager);

            return networkService;
        }

        private IConnectionService PrepareConnectionService() {
            ConnectionService connService = new ConnectionService(
                _services.Single<IStateMachine>(),
                _services.Single<INetworkService>(),
                _services.Single<ISessionDataService>());

            connService.Initialize();
            connService.Enter<OfflineState>();

            return connService;
        }
    }
}
