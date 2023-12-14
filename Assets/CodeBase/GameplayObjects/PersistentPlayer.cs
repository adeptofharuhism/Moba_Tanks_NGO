using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.SessionData;
using Assets.CodeBase.Networking;
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

        private ISessionDataService _sessionData;

        private void Awake() {
            _sessionData = AllServices.Container.Single<ISessionDataService>();
        }

        public override void OnNetworkSpawn() {
            gameObject.name = PersistentPlayerString + OwnerClientId;

            _persistentPlayerRuntimeCollection.Add(this);

            if (IsServer)
                OnNetworkSpawnOnServer();
        }

        private void OnNetworkSpawnOnServer() {
            SessionPlayerData? sessionPlayerData = _sessionData.GetPlayerData(OwnerClientId);
            if (sessionPlayerData == null)
                return;

            _networkName.Name.Value = sessionPlayerData.Value.PlayerName;
        }
    }
}
