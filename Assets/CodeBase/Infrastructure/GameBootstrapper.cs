using Assets.CodeBase.Infrastructure.GameStates;
using Unity.Netcode;
using UnityEngine;

namespace Assets.CodeBase.Infrastructure
{
    public class GameBootstrapper : MonoBehaviour, ICoroutineRunner
    {
        [SerializeField] private NetworkManager _networkManagerPrefab;

        private Game _game;

        private void Awake() {
            _game = new Game(this, Instantiate(_networkManagerPrefab));
            _game.GameStateMachine.Enter<BootstrapState>();

            DontDestroyOnLoad(gameObject);
        }
    }
}
