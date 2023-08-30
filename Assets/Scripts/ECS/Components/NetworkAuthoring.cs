using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

namespace Natrium
{
    public class NetworkAuthoring : MonoBehaviour
    {

    }

    public class NetworkBaker : Baker<NetworkAuthoring>
    {
        public override void Bake(NetworkAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.None);
            AddComponent(e, new NetworkData
            {
                
            });
        }
    }

    public struct NetworkData : IComponentData
    {
        
    }

    public struct RpcConnect : IRpcCommand { }
    public struct RpcDisconnect : IRpcCommand { }
    public struct RpcClick : IRpcCommand
    {
        public float3 MouseWorldPosition;
    }
    public struct TouchData : IRpcCommand
    {
        public float3 Start;
        public float3 End;
        public int NetworkIDSource;
        public int NetworkIDTarget;
    }

    public struct RaycastCommand : IComponentData
    {
        public Entity ReqE;
        public float3 Start;
        public float3 End;
        public float MaxDistance;
    }

    public struct RaycastOutput : IComponentData
    {
        public Entity ReqE;
        public float3 Start;
        public float3 End;
        public Unity.Physics.RaycastHit Hit;
    }
}