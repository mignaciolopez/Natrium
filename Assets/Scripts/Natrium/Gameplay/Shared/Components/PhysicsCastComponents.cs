using Unity.Entities;
using Unity.Mathematics;

namespace Natrium.Gameplay.Shared.Components
{
    public struct OverlapBoxTag : IComponentData  { }
    
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