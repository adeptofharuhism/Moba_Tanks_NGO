using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.Input;
using Assets.CodeBase.Infrastructure.StateMachine;
using System.Collections.Generic;
using Unity.Netcode;

namespace Assets.CodeBase.Infrastructure.GameStates
{
    public class GameStateMachine: BaseStateMachine<IGameExitableState>
    {
        public GameStateMachine(SceneLoader sceneLoader, AllServices services, NetworkManager networkManager) {
            _states = new Dictionary<System.Type, IGameExitableState>() {
                [typeof(BootstrapState)] = new BootstrapState(this, sceneLoader, networkManager ,services),
                [typeof(LoadLevelState)] = new LoadLevelState(this, sceneLoader),
                [typeof(GameLoopState)] = new GameLoopState(this, services.Single<IInputService>()),
            };
        }
    }
}