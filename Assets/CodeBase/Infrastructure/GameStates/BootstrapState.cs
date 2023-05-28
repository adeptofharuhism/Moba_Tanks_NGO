using System;
using UnityEngine;

namespace Assets.CodeBase.Infrastructure.GameStates
{
    public class BootstrapState : IGameState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;

        public BootstrapState(GameStateMachine stateMachine, SceneLoader sceneLoader) {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;

            RegisterServices();
        }

        public void Enter() {
            _sceneLoader.Load(Constants.SceneNames.Initial, OnLoaded);
        }

        public void Exit() {

        }

        private void OnLoaded() {
            Debug.Log("Initialized");
            _stateMachine.Enter<LoadLevelState, string>(Constants.SceneNames.Main);
        }

        private void RegisterServices() {

        }
    }
}
