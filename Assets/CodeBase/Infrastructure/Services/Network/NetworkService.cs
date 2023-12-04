using Unity.Netcode;

namespace Assets.CodeBase.Infrastructure.Services.Network
{
    public class NetworkService : INetworkService
    {
        private readonly NetworkManager _networkManager;

        public NetworkManager NetworkManager => _networkManager;

        public NetworkService(NetworkManager networkManager) {
            _networkManager = networkManager;
        }
    }
}
