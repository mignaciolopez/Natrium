using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Natrium.Gameplay.Components
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
            AddComponent(entity, new CameraData
            {
                Offset = authoring.offset
            });
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct CameraData : IComponentData
    {
        public float3 Offset;
    }
}