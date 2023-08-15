using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Natrium
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
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new CameraData
            {
                offset = authoring.offset
            });
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct CameraData : IComponentData
    {
        public float3 offset;
    }
}