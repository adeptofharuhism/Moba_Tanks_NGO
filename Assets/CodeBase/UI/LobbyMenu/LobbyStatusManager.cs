using Assets.CodeBase.GameplayObjects;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Assets.CodeBase.UI.LobbyMenu
{
    public class LobbyStatusManager : MonoBehaviour
    {
        [SerializeField] private PersistentPlayerRuntimeCollection _persistentPlayerRuntimeCollection;
        [SerializeField] private PlayerStatusContainerLobby _playerStatusContainerLobbyPrefab;
        [SerializeField] private Transform _playersContainer;
        [SerializeField] private int _playerStatusContainerPadding;

        private List<PlayerStatusContainerLobby> _playerStatuses = new();

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
            float offset = 0;
            foreach (PersistentPlayer persistentPlayer in _persistentPlayerRuntimeCollection) {
                PlayerStatusContainerLobby playerStatus = Instantiate(_playerStatusContainerLobbyPrefab, _playersContainer);

                RectTransform playerStatusPrefabRectTransform = playerStatus.GetComponent<RectTransform>();
                float heightOffset = playerStatusPrefabRectTransform.rect.height;

                Transform playerStatusTransform = playerStatus.transform;
                playerStatusTransform.position = new Vector3(
                    playerStatusTransform.position.x,
                    playerStatusTransform.position.y + offset,
                    playerStatusTransform.position.z);
                offset -= _playerStatusContainerPadding + heightOffset;

                playerStatus.SetPlayerNameText(persistentPlayer.NetworkName.Name.Value);

                _playerStatuses.Add(playerStatus);

                Debug.Log(persistentPlayer.NetworkName.Name);
                Debug.Log(persistentPlayer.NetworkName.Name.Value);
            }
        }

        private void ClearPlayerStatuses() {
            foreach (PlayerStatusContainerLobby playerStatus in _playerStatuses)
                Destroy(playerStatus.gameObject);

            _playerStatuses.Clear();
        }
    }
}
