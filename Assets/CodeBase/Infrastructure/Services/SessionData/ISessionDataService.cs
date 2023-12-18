namespace Assets.CodeBase.Infrastructure.Services.SessionData
{
    public interface ISessionDataService : IService
    {
        bool IsDuplicateConnection(string playerId);
        void SetupConnectingPlayerSessionData(ulong clientId, string playerId, SessionPlayerData sessionPlayerData);
        SessionPlayerData? GetPlayerData(ulong clientId);
        SessionPlayerData? GetPlayerData(string playerId);
        string GetPlayerId(ulong clientId);
        void DisconnectClient(ulong clientId);
        void OnServerEnded();
        void OnSessionStarted();
        void OnSessionEnded();
        void SetPlayerData(ulong ownerClientId, SessionPlayerData playerData);
    }
}
