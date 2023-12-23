using Unity.Entities;
using Unity.Mathematics;

namespace Natrium.Gameplay.Server.Components
{
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