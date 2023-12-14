using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.SessionData;
using Unity.Netcode;
using UnityEngine;

namespace Assets.CodeBase.GameplayObjects
{
    [RequireComponent(typeof(NetworkObject))]
    public class PersistentPlayer : NetworkBehaviour
    {
        private const string PersistentPlayerString = "PersistentPlayer";

        [SerializeField]
        private PersistentPlayerRuntimeCollection _persistentPlayerRuntimeCollection;

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

            SessionPlayerData playerData = sessionPlayerData.Value;
            
        }
    }
}
