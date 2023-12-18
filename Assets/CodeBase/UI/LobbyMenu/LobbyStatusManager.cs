using Assets.CodeBase.GameplayObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CodeBase.UI.LobbyMenu
{
    public class LobbyStatusManager : MonoBehaviour
    {
        [SerializeField] private PersistentPlayerRuntimeCollection _persistentPlayerRuntimeCollection;
        [SerializeField] private PlayerStatusContainerLobby[] _preparedPlayerStatusContainerLobbyArray;
        [SerializeField] private Transform _playersContainer;

        private void Awake() {
            ResetPlayerList();
        }

        private void OnEnable() {
            _persistentPlayerRuntimeCollection.ItemAdded += OnPlayerAdded;
            _persistentPlayerRuntimeCollection.ItemRemoved += OnPlayerRemoved;
        }

        private void OnDisable() {
            _persistentPlayerRuntimeCollection.ItemAdded -= OnPlayerAdded;
            _persistentPlayerRuntimeCollection.ItemRemoved -= OnPlayerRemoved;
        }

        private void OnPlayerAdded(PersistentPlayer persistentPlayer) {
            ResetPlayerList();
        }

        private void OnPlayerRemoved(PersistentPlayer persistentPlayer) {
            ResetPlayerList();
        }

        private void ResetPlayerList() {
            ClearPlayerStatuses();
            AddPlayerStatuses();
        }

        private void AddPlayerStatuses() {
            int playerStatusesShown = 0;
            foreach (PersistentPlayer persistentPlayer in _persistentPlayerRuntimeCollection) {
                PlayerStatusContainerLobby playerStatus = _preparedPlayerStatusContainerLobbyArray[playerStatusesShown];

                playerStatus.SetPlayerNameText(persistentPlayer.NetworkName.Name.Value);

                playerStatus.gameObject.SetActive(true);

                playerStatusesShown++;
            }
        }

        private void ClearPlayerStatuses() {
            foreach (PlayerStatusContainerLobby playerStatus in _preparedPlayerStatusContainerLobbyArray)
                playerStatus.gameObject.SetActive(false);
        }
    }
}
