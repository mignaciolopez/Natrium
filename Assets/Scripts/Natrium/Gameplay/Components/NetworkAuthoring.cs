using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Natrium.Gameplay.Components
{
    public struct RpcConnect : IRpcCommand { }
    public struct RpcDisconnect : IRpcCommand { }
    public struct RpcClick : IRpcCommand
    {
        public float3 MouseWorldPosition;
    }
    public struct RpcHit : IRpcCommand
    {
        public float3 Start;
        public float3 End;
        public int NetworkIdSource;
        public int NetworkIdTarget;
    }

    public struct Hit : IComponentData
    {
        public float3 End;
        public int NetworkIdTarget;
    }

    public struct RaycastCommand : IComponentData
    {
        public float3 Start;
        public float3 End;
    }

    public struct RaycastOutput : IComponentData
    {
        public float3 Start;
        public float3 End;
        public Unity.Physics.RaycastHit Hit;
    }
}