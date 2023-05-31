using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.Input;
using Assets.CodeBase.Infrastructure.StateMachine;

namespace Assets.CodeBase.Infrastructure.GameStates
{
    public class BootstrapState : IGameState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly AllServices _services;

        public BootstrapState(GameStateMachine stateMachine, SceneLoader sceneLoader, AllServices services) {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _services = services;

            RegisterServices();
        }

        public void Enter() {
            _sceneLoader.Load(Constants.SceneNames.Initial, OnLoaded);
        }

        public void Exit() {

        }

        private void OnLoaded() {
            _stateMachine.Enter<LoadLevelState, string>(Constants.SceneNames.Main);
        }

        private void RegisterServices() {
            _services.RegisterSingle<IStateMachine>(_stateMachine);
            _services.RegisterSingle(PrepareInputService());
        }

        private IInputService PrepareInputService() {
            IInputService inputService = new InputService();
            inputService.Initialize();

            return inputService;
        }
    }
}
