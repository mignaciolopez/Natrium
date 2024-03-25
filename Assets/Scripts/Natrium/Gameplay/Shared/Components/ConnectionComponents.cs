using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Components
{
    public struct RpcConnect : IRpcCommand { }

    public struct RpcDisconnect : IRpcCommand { }

    public struct RpcPing : IRpcCommand
    {
        public long UnixTime;
    }
    public struct RpcPong : IRpcCommand
    {
        public long PingUnixTime;
        public long PongUnixTime;
    }

    public struct Latency : IComponentData
    {
        public long Value;
    }

    public struct RoundTripTime : IComponentData
    {
        public long Value;
    }
}
