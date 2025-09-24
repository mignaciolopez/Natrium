using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Gameplay.Shared.Components
{
    [DisallowMultipleComponent]
    public class CameraFollowAuthoring : MonoBehaviour
    {
        [SerializeField] private bool overrideSceneSettings;
        [SerializeField] private float3 offset;
        
        public class Baker : Baker<CameraFollowAuthoring>
        {
            public override void Bake(CameraFollowAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(e, new CameraFollow
                {
                    OverrideSceneSettings = authoring.overrideSceneSettings,
                    Offset = authoring.offset
                });
            }
        }
    }
    
    public struct CameraFollow : IComponentData
    {
        public bool OverrideSceneSettings;
        public float3 Offset;
    }
}