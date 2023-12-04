using Assets.CodeBase.Infrastructure.GameStates;
using Assets.CodeBase.Infrastructure.Services.Network;
using Assets.CodeBase.Infrastructure.StateMachine;
using UnityEngine.SceneManagement;

namespace Assets.CodeBase.Infrastructure.Services.Connection.States
{
    public abstract class ConnectionState : IConnectionState
    {
        protected readonly IStateMachine _gameStateMachine;
        protected readonly INetworkService _networkService;

        public ConnectionState(IStateMachine gameStateMachine, INetworkService networkService) {
            _gameStateMachine = gameStateMachine;
            _networkService = networkService;
        }

        public abstract void Enter();

        public abstract void Exit();

        public virtual void StartHostIP(string playerName, string ipaddress, int port) { }
    }


    public class OfflineState : ConnectionState
    {
        public OfflineState(IStateMachine gameStateMachine, INetworkService networkService)
            : base(gameStateMachine, networkService) { }

        public override void Enter() {
            _networkService.NetworkManager.Shutdown();

            if (SceneManager.GetActiveScene().name != Constants.SceneNames.MainMenu)
                _gameStateMachine.Enter<LoadLevelState, string>(Constants.SceneNames.MainMenu);
        }

        public override void Exit() {
            throw new System.NotImplementedException();
        }
    }

    public class OnlineState : ConnectionState
    {
        public OnlineState(IStateMachine gameStateMachine, INetworkService networkService)
            : base(gameStateMachine, networkService) { }

        public override void Enter() {
            throw new System.NotImplementedException();
        }

        public override void Exit() {
            throw new System.NotImplementedException();
        }
    }
}
