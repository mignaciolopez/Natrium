using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Natrium.Gameplay.Client.Components
{
    [DisallowMultipleComponent]
    public class CameraAuthoring : MonoBehaviour
    {
        public float3 offset;
    }

    public class CameraBaker : Baker<CameraAuthoring>
    {
        public override void Bake(CameraAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<CameraFollow>(entity);
            AddComponent(entity, new CameraOffset
            {
                Value = authoring.offset
            });
        }
    }

    public struct CameraFollow : IComponentData
    {
    }

    public struct CameraOffset : IComponentData
    {
        public float3 Value;
    }
}