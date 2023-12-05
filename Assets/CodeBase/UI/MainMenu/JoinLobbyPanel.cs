using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.Connection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CodeBase.UI.MainMenu
{
    public class JoinLobbyPanel : LobbyPanel
    {
        [SerializeField] private Button _startClientButton;
        [SerializeField] private TMP_InputField _serverIPAddress;
        [SerializeField] private TMP_InputField _serverPort;
        [SerializeField] private TMP_InputField _playerName;

        private IConnectionService _connectionService;

        private void Awake() => 
            _connectionService = AllServices.Container.Single<IConnectionService>();

        private void Start() {
            LobbyPanelInitialize();
            JoinLobbyPanelInitialize();
        }

        private void JoinLobbyPanelInitialize() => 
            _startClientButton.onClick.AddListener(StartClientClick);

        private void StartClientClick() => 
            _connectionService.StartClientIP(_playerName.text, _serverIPAddress.text, int.Parse(_serverPort.text));
    }
}
