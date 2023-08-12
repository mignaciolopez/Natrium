using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Natrium
{
    public class CameraAuthoring : MonoBehaviour
    {
        public float3 offset;
    }

    public class CameraAuthoringBaker : Baker<CameraAuthoring>
    {
        public override void Bake(CameraAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new CameraData
            {
                offset = authoring.offset
            });
        }
    }

    public struct CameraData : IComponentData
    {
        public float3 offset;
    }
}