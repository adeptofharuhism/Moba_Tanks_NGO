using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.Connection;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CodeBase.UI.LobbyMenu
{
    public class LobbyMenuPanel: MonoBehaviour
    {
        [SerializeField] private Button _exitButton;

        private IConnectionService _connectionService;

        private void Awake() {
            _connectionService = AllServices.Container.Single<IConnectionService>();
            _exitButton.onClick.AddListener(OnExitButtonClick);
        }

        private void OnExitButtonClick() => 
            _connectionService.RequestShutdown();
    }
}
