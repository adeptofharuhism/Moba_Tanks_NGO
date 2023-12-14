namespace Assets.CodeBase.Infrastructure.Services.Connection
{
    public interface IConnectionService : IService
    {
        string PlayerName { get; }
        string IPAddress { get; }
        int Port { get; }
        int MaxConnectedPlayers { get; }
        string PlayerGuid { get; }

        void RequestShutdown();
        void StartClientIP(string playerName, string ipaddress, int port);
        void StartHostIP(string playerName, string ipaddress, int port);
    }
}
