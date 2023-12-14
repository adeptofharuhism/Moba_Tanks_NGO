using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Assets.CodeBase.Networking
{
    public class NetworkName : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariable<FixedPlayerName> Name = new NetworkVariable<FixedPlayerName>();
    }

    public struct FixedPlayerName : INetworkSerializable
    {
        private FixedString32Bytes _name;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
            serializer.SerializeValue(ref _name);

        public override string ToString() =>
            _name.Value.ToString();

        public static implicit operator string(FixedPlayerName s) => s.ToString();
        public static implicit operator FixedPlayerName(string s) => new FixedPlayerName() { _name = new FixedString32Bytes(s) };
    }
}