namespace Assets.CodeBase.Infrastructure.GameStates
{
    public class GameLoopState : IGameState
    {
        private readonly GameStateMachine _stateMachine;

        public GameLoopState(GameStateMachine stateMachine) {
            _stateMachine = stateMachine;
        }

        public void Enter() {
            
        }

        public void Exit() {
            
        }
    }
}