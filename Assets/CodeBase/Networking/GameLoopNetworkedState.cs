using Assets.CodeBase.GameplayObjects;
using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.Network;
using Assets.CodeBase.Infrastructure.Services.SessionData;
using Unity.Netcode;
using UnityEngine;

namespace Assets.CodeBase.Networking
{
    public class GameLoopNetworkedState : NetworkBehaviour
    {
        [SerializeField] private PersistentPlayerRuntimeCollection _persistentPlayers;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private NetworkObject _playerPrefab;

        private ISessionDataService _sessionData;
        private INetworkService _networkService;

        private void Awake() {
            _sessionData = AllServices.Container.Single<ISessionDataService>();
            _networkService = AllServices.Container.Single<INetworkService>();

            _sessionData.OnSessionStarted();
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();

            if (IsServer) {
                int playerIndex = 0;
                foreach (PersistentPlayer player in _persistentPlayers) {
                    NetworkObject playerNetworkObject = Instantiate(_playerPrefab);

                    playerNetworkObject.transform.position = _spawnPoints[playerIndex].position;
                    playerIndex++;

                    playerNetworkObject.SpawnAsPlayerObject(player.OwnerClientId);
                }
            }
        }
    }
}