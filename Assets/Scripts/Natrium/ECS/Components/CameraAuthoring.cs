using System.ComponentModel;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace Natrium.Ecs.Components
{
    public class CameraOffsetAuthoring : MonoBehaviour
    {
        public float3 value;
    }

    public class CameraOffsetBaker : Baker<CameraOffsetAuthoring>
    {
        public override void Bake(CameraOffsetAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new CameraOffset
            {
                Value = authoring.value
            });
        }
    }

    public struct CameraOffset : IComponentData
    {
        public float3 Value;
    }

    public struct CameraFollow : IComponentData
    {
    }
}