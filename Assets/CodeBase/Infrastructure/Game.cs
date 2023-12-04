using Assets.CodeBase.Infrastructure.GameStates;
using Assets.CodeBase.Infrastructure.Services;
using Unity.Netcode;

namespace Assets.CodeBase.Infrastructure
{
    public class Game
    {
        private readonly GameStateMachine _gameStateMachine;

        public GameStateMachine GameStateMachine => _gameStateMachine;

        public Game(ICoroutineRunner coroutineRunner, NetworkManager networkManager) {
            _gameStateMachine = new GameStateMachine(new SceneLoader(coroutineRunner), AllServices.Container, networkManager);
        }
    }
}
