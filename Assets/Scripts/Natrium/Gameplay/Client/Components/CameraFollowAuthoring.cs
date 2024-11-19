using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Natrium.Gameplay.Client.Components
{
    [DisallowMultipleComponent]
    public class CameraFollowAuthoring : MonoBehaviour
    {
        public float3 offset;
        
        public class Baker : Baker<CameraFollowAuthoring>
        {
            public override void Bake(CameraFollowAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(e, new CameraFollow
                {
                    Offset = authoring.offset
                });
            }
        }
    }
    
    public struct CameraFollow : IComponentData
    {
        public float3 Offset;
    }
}