using Assets.CodeBase.Infrastructure.GameStates;
using Assets.CodeBase.Infrastructure.Services.Connection.States.Online;
using Assets.CodeBase.Infrastructure.Services.Network;
using Assets.CodeBase.Infrastructure.StateMachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.CodeBase.Infrastructure.Services.Connection.States.Offline
{
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
}
