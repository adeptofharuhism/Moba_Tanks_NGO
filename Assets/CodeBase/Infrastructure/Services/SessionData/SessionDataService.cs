using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.CodeBase.Infrastructure.Services.SessionData
{
    public struct SessionPlayerData
    {
        public bool IsConnected;
        public ulong ClientID;

        public string PlayerName;

        public SessionPlayerData(ulong clientId, string name, bool isConnected = false) {
            IsConnected = isConnected;
            ClientID = clientId;
            PlayerName = name;
        }
    }

    public interface ISessionDataService : IService
    {
        bool IsDuplicateConnection(string playerId);
        void SetupConnectingPlayerSessionData(ulong clientId, string playerId, SessionPlayerData sessionPlayerData);
    }

    public class SessionDataService : ISessionDataService
    {
        private Dictionary<string, SessionPlayerData> _clientData;
        private Dictionary<ulong, string> _clientIdToPlayerId;

        public SessionDataService() {
            _clientData = new Dictionary<string, SessionPlayerData>();
            _clientIdToPlayerId = new Dictionary<ulong, string>();
        }

        public bool IsDuplicateConnection(string playerId) => 
            _clientData.ContainsKey(playerId) && _clientData[playerId].IsConnected;

        public void SetupConnectingPlayerSessionData(ulong clientId, string playerId, SessionPlayerData sessionPlayerData) {
            if (IsDuplicateConnection(playerId)) {
                Debug.LogError($"Player ID {playerId} already exists. This is a duplicate connection. Rejecting this session data.");
                return;
            }

            if (IsReconnection(playerId)) {
                sessionPlayerData = _clientData[playerId];
                sessionPlayerData.ClientID = clientId;
                sessionPlayerData.IsConnected = true;
            }

            _clientIdToPlayerId[clientId] = playerId;
            _clientData[playerId] = sessionPlayerData;

            Debug.Log($"Connected {sessionPlayerData.PlayerName} with ClientID {clientId} and PlayerID {playerId}");
        }

        private bool IsReconnection(string playerId) {
            bool isReconnection = false;
            if (_clientData.ContainsKey(playerId)) {
                if (!_clientData[playerId].IsConnected) {
                    isReconnection = true;
                }
            }

            return isReconnection;
        }
    }
}
