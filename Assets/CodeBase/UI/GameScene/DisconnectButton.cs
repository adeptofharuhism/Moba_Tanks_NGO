using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.Connection;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CodeBase.UI.GameScene
{
    public class DisconnectButton : MonoBehaviour
    {
        [SerializeField] private Button _disconnectButton;

        private IConnectionService _connectionService;

        private void Awake() {
            _connectionService = AllServices.Container.Single<IConnectionService>();
            _disconnectButton.onClick.AddListener(OnDisconnect);
        }

        private void OnDisconnect() =>
            _connectionService.RequestShutdown();
    }
}
