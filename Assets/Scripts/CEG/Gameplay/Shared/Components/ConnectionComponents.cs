using Unity.Entities;
using Unity.NetCode;

namespace CEG.Gameplay.Shared.Components
{
    public struct RpcStartStreaming : IRpcCommand { }

    public struct RpcDisconnect : IRpcCommand { }

    public struct PingRequest : IComponentData { }
    
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
