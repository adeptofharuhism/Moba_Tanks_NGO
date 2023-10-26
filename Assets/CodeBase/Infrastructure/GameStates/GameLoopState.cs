using Assets.CodeBase.Infrastructure.Services.Input;

namespace Assets.CodeBase.Infrastructure.GameStates
{
    public class GameLoopState : IGameState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IInputService _inputService;

        public GameLoopState(GameStateMachine stateMachine, IInputService inputService) {
            _stateMachine = stateMachine;
            _inputService = inputService;
        }

        public void Enter() {
            _inputService.Enable();
        }

        public void Exit() {
            _inputService.Disable();
        }
    }
}