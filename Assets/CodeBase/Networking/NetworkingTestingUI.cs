using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CodeBase.Networking
{
    public class NetworkingTestingUI : MonoBehaviour
    {
        [SerializeField] private Button _startHostButton;
        [SerializeField] private Button _startClientButton;
        [SerializeField] private Button _startServerButton;
        [SerializeField] private TMP_InputField _ipAddress;

        private void Awake() {
            _startHostButton.onClick.AddListener(() => {
                NetworkManager.Singleton.StartHost();
            });

            _startClientButton.onClick.AddListener(() => {
                NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = _ipAddress.text;
                NetworkManager.Singleton.StartClient();
            });

            _startServerButton.onClick.AddListener(() => {
                NetworkManager.Singleton.StartServer();
            });
        }
    }
}