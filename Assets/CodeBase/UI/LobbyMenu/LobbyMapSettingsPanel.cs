using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.CodeBase.UI.LobbyMenu
{
    public class LobbyMapSettingsPanel : MonoBehaviour
    {
        [SerializeField] private Button _playButton;

        private INetworkService _networkService;

        private void Awake() {
            _networkService = AllServices.Container.Single<INetworkService>();
        }

        public void Start() {
            if (_networkService.NetworkManager.IsServer) {
                _playButton.gameObject.SetActive(true);
                _playButton.onClick.AddListener(StartGame);
            }
        }

        private void StartGame() => 
            _networkService.NetworkManager.SceneManager.LoadScene(Constants.SceneNames.Main, LoadSceneMode.Single);
    }
}
