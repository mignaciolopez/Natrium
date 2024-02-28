using Unity.Mathematics;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Components
{
    public struct RpcConnect : IRpcCommand { }
    public struct RpcDisconnect : IRpcCommand { }
    public struct RpcAim : IRpcCommand
    {
        public float3 MouseWorldPosition;
    }
    public struct RpcMeele : IRpcCommand { }

    public struct RpcAttack : IRpcCommand
    {
        public float3 Start;
        public float3 End;
        public int NetworkIdSource;
        public int NetworkIdTarget;
        public float DP;
    }
}
