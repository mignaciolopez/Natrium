using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Components
{
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
    public struct OverlapBox : IComponentData
    {
        [GhostField] public float HalfExtends;
        [GhostField] public float3 Offset;
    }
    
    public struct RayCast : IComponentData
    {
        public float3 Start;
        public float3 End;
    }
    public struct RayCastOutput : IComponentData
    {
        public bool IsHit;
        public float3 Start;
        public float3 End;
        public Unity.Physics.RaycastHit Hit;
    }

    public struct BoxCast : IComponentData
    {
        public float3 Center;
        public quaternion Orientation;
        public float3 HalfExtents;
        public float3 Direction;
        public float MaxDistance;
    }

    public struct BoxCastOutput : IComponentData
    {
        public bool IsHit;
        public Unity.Physics.ColliderCastHit Hit;
    }
}