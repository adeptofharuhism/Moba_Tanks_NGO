using Assets.CodeBase.Infrastructure.GameStates;
using Assets.CodeBase.Infrastructure.StateMachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.CodeBase.Infrastructure.Services.Connection.States
{
    public abstract class ConnectionState : IConnectionState
    {
        public abstract void Enter();

        public abstract void Exit();

        public virtual void StartHostIP(string playerName, string ipaddress, int port) { }
    }


    public class OfflineState : ConnectionState
    {
        private readonly IStateMachine _gameStateMachine;

        public OfflineState(IStateMachine gameStateMachine) {
            _gameStateMachine = gameStateMachine;
        }

        public override void Enter() {


            if (SceneManager.GetActiveScene().name != Constants.SceneNames.MainMenu)
                _gameStateMachine.Enter<LoadLevelState, string>(Constants.SceneNames.MainMenu);
        }

        public override void Exit() {
            throw new System.NotImplementedException();
        }
    }

    public class OnlineState : ConnectionState
    {
        public override void Enter() {
            throw new System.NotImplementedException();
        }

        public override void Exit() {
            throw new System.NotImplementedException();
        }
    }
}
