using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CodeBase.UI.LobbyMenu
{
    public class PlayerStatusContainerLobby : MonoBehaviour
    {
        [SerializeField] private Image _playerColor;
        [SerializeField] private TMP_Text _playerNameText;
        [SerializeField] private GameObject _playerReadyObject;

        public void SetPlayerReady(bool isReady) =>
            _playerReadyObject.SetActive(isReady);

        public void SetPlayerNameText(string playerName) =>
            _playerNameText.text = playerName;

        public void SetPlayerColor(Color color) =>
            _playerColor.color = color;
    }
}
