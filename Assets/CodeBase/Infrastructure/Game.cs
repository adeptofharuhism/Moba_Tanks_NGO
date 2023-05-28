using Assets.CodeBase.Infrastructure.GameStates;

namespace Assets.CodeBase.Infrastructure
{
    public class Game
    {
        private readonly GameStateMachine _gameStateMachine;

        public GameStateMachine GameStateMachine => _gameStateMachine;

        public Game(ICoroutineRunner coroutineRunner) {
            _gameStateMachine = new GameStateMachine(new SceneLoader(coroutineRunner));
        }
    }
}
