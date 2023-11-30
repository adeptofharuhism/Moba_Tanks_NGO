using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.Connection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CodeBase.UI.MainMenu
{
    public class HostLobbyPanel : LobbyPanel
    {
        [SerializeField] private Button _startHostingButton;
        [SerializeField] private TMP_InputField _hostIPAddress;
        [SerializeField] private TMP_InputField _hostPort;
        [SerializeField] private TMP_InputField _hostName;

        private IConnectionService _connectionService;

        private void Awake() {
            _connectionService = AllServices.Container.Single<IConnectionService>();
        }

        private void Start() {
            LobbyPanelInitialize();
            HostLobbyPanelInitialize();
        }

        private void HostLobbyPanelInitialize() {
            _startHostingButton.onClick.AddListener(StartHostingClick);
        }

        private void StartHostingClick() {
            _connectionService.StartHostIP(_hostName.text, _hostIPAddress.text, int.Parse(_hostPort.text));
        }
    }
}
