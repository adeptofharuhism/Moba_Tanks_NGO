﻿using Assets.CodeBase.Infrastructure.Services.Connection.States;
using Assets.CodeBase.Infrastructure.Services.Network;
using Assets.CodeBase.Infrastructure.StateMachine;
using System.Collections.Generic;

namespace Assets.CodeBase.Infrastructure.Services.Connection
{
    public class ConnectionService : BaseStateMachine<IConnectionExitableState>, IConnectionService
    {
        public ConnectionService(IStateMachine gameStateMachine, INetworkService networkService) {
            _states = new Dictionary<System.Type, IConnectionExitableState>() {
                [typeof(OfflineState)] = new OfflineState(gameStateMachine, networkService)
            };
        }

        public void StartHostIP(string playerName, string ipaddress, int port) {
            _activeState.StartHostIP(playerName, ipaddress, port);
        }
    }

    public interface IConnectionService : IService
    {
        void StartHostIP(string playerName, string ipaddress, int port);
    }
}
