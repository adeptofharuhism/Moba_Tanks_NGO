using System;
using System.Collections.Generic;
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

        public void Reinitalize() { }
    }

    public class SessionDataService : ISessionDataService
    {
        private readonly Dictionary<string, SessionPlayerData> _clientData;
        private readonly Dictionary<ulong, string> _clientIdToPlayerId;

        private bool _sessionHasStarted;

        public SessionDataService() {
            _clientData = new Dictionary<string, SessionPlayerData>();
            _clientIdToPlayerId = new Dictionary<ulong, string>();
        }

        public void DisconnectClient(ulong clientId) {
            if (!_clientIdToPlayerId.TryGetValue(clientId, out string playerId))
                return;

            if (!_sessionHasStarted)
                _clientIdToPlayerId.Remove(clientId);

            SessionPlayerData? playerData = GetPlayerData(playerId);
            if (playerData == null)
                return;

            if (clientId != playerData.Value.ClientID)
                return;

            if (_sessionHasStarted)
                MarkClientDataAsDisconnected(playerId);
            else
                _clientData.Remove(playerId);
        }

        public void SetPlayerData(ulong clientId, SessionPlayerData playerData) {

        }

        public SessionPlayerData? GetPlayerData(ulong clientId) {
            string playerId = GetPlayerId(clientId);
            if (playerId != null)
                return GetPlayerData(playerId);

            return null;
        }

        public SessionPlayerData? GetPlayerData(string playerId) {
            if (_clientData.TryGetValue(playerId, out SessionPlayerData playerData))
                return playerData;

            return null;
        }

        public string GetPlayerId(ulong clientId) {
            if (_clientIdToPlayerId.TryGetValue(clientId, out string playerId))
                return playerId;

            return null;
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

        public void OnSessionStarted() =>
            _sessionHasStarted = true;

        public void OnSessionEnded() {
            ClearDisconnectedPlayersData();
            ReinitializePlayersData();

            _sessionHasStarted = false;
        }

        private void ClearDisconnectedPlayersData() {
            List<ulong> idsToClear = new();
            foreach(ulong id in _clientIdToPlayerId.Keys) {
                SessionPlayerData? data = GetPlayerData(id);
                if (data is { IsConnected: false })
                    idsToClear.Add(id);
            }

            foreach (ulong id in idsToClear) {
                string playerId = _clientIdToPlayerId[id];
                var playerData = GetPlayerData(playerId);
                if (playerData != null && playerData.Value.ClientID == id)
                    _clientData.Remove(playerId);

                _clientIdToPlayerId.Remove(id);
            }
        }

        private void ReinitializePlayersData() {
            foreach (ulong id in _clientIdToPlayerId.Keys) {
                string playerId = _clientIdToPlayerId[id];
                SessionPlayerData sessionPlayerData = _clientData[playerId];
                sessionPlayerData.Reinitalize();
                _clientData[playerId] = sessionPlayerData;
            }
        }

        public void OnServerEnded() {
            _clientData.Clear();
            _clientIdToPlayerId.Clear();

            _sessionHasStarted = false;
        }

        private void MarkClientDataAsDisconnected(string playerId) {
            SessionPlayerData clientData = _clientData[playerId];
            clientData.IsConnected = false;
            _clientData[playerId] = clientData;
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
