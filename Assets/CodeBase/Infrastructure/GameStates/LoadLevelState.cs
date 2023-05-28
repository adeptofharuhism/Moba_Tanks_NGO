using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.CodeBase.Infrastructure.GameStates
{
    public class LoadLevelState : IGamePayloadedState<string>
    {
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;

        public LoadLevelState(GameStateMachine stateMachine, SceneLoader sceneLoader) {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
        }

        public void Enter(string sceneName) {
            _sceneLoader.Load(sceneName, OnLoaded);
        }

        public void Exit() {

        }

        private void OnLoaded() {
            _stateMachine.Enter<GameLoopState>();
        }
    }
}
