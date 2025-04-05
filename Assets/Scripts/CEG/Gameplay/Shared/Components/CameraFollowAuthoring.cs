using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace CEG.Gameplay.Shared.Components
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
    
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToOwner)]
    public struct CameraFollow : IComponentData
    {
        [GhostField] public float3 Offset;
    }
}