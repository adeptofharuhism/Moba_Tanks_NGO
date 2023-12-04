using Unity.Netcode;

namespace Assets.CodeBase.Infrastructure.Services.Network
{
    public interface INetworkService : IService
    {
        NetworkManager NetworkManager { get; }
    }
}
