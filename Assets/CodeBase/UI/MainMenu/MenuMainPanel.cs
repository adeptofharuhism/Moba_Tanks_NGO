using UnityEngine;
using UnityEngine.UI;

namespace Assets.CodeBase.UI.MainMenu
{
    public class MenuMainPanel : MonoBehaviour
    {
        [SerializeField] private Button _hostLobbyButton;
        [SerializeField] private Button _joinLobbyButton;
        [SerializeField] private Button _exitButton;

        [SerializeField] private GameObject _hostLobbyPanel;
        [SerializeField] private GameObject _joinLobbyPanel;

        private void Start() {
            _hostLobbyButton.onClick.AddListener(HostLobbyClick);
            _joinLobbyButton.onClick.AddListener(JoinLobbyClick);
            _exitButton.onClick.AddListener(ExitClick);
        }

        private void HostLobbyClick() {
            _hostLobbyPanel.SetActive(true);
            _joinLobbyPanel.SetActive(false);
        }

        private void JoinLobbyClick() {
            _hostLobbyPanel.SetActive(false);
            _joinLobbyPanel.SetActive(true);
        }

        private void ExitClick() {
            Application.Quit();
        }
    }
}
