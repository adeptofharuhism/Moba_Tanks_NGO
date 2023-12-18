using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.SessionData;
using Assets.CodeBase.Networking;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Assets.CodeBase.GameplayObjects
{
    [RequireComponent(typeof(NetworkObject))]
    public class PersistentPlayer : NetworkBehaviour
    {
        private const string PersistentPlayerString = "PersistentPlayer";

        [SerializeField] private PersistentPlayerRuntimeCollection _persistentPlayerRuntimeCollection;
        [SerializeField] private NetworkName _networkName;

        public NetworkName NetworkName => _networkName;

        private ISessionDataService _sessionData;

        private void Awake() {
            _sessionData = AllServices.Container.Single<ISessionDataService>();
        }

        public override void OnNetworkSpawn() {
            gameObject.name = PersistentPlayerString + OwnerClientId;

            if (IsServer)
                OnNetworkSpawnOnServer();

            _persistentPlayerRuntimeCollection.Add(this);
        }

        public override void OnNetworkDespawn() {
            RemovePersistentPlayer();
        }

        public override void OnDestroy() {
            base.OnDestroy();
            RemovePersistentPlayer();
        }

        private void RemovePersistentPlayer() {
            _persistentPlayerRuntimeCollection.Remove(this);

            if (IsServer)
                OnNetworkDespawnOnServer();
        }

        private void OnNetworkSpawnOnServer() {
            SessionPlayerData? sessionPlayerData = _sessionData.GetPlayerData(OwnerClientId);
            if (sessionPlayerData == null)
                return;

            _networkName.Name.Value = sessionPlayerData.Value.PlayerName;
        }

        private void OnNetworkDespawnOnServer() {
            SessionPlayerData? sessionPlayerData = _sessionData.GetPlayerData(OwnerClientId);
            if (sessionPlayerData == null)
                return;

            SessionPlayerData playerData = sessionPlayerData.Value;
            playerData.PlayerName = _networkName.Name.Value;
            _sessionData.SetPlayerData(OwnerClientId, playerData);
        }
    }
}
