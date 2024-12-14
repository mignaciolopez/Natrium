using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;

namespace Natrium.Gameplay.Shared.Components
{
    public enum MovementTypes
    {
        Classic = 0,
        Diagonal,
        Free,
    }

    [DisallowMultipleComponent]
    public class MovementAuthoring : MonoBehaviour
    {
        public float translation = 4.0f;
        public float rotation = 90.0f;
        
        public MovementTypes moveType = MovementTypes.Classic;
        
        [Range(0.1f, 1.0f)]
        public float percentNextMove = 0.1f;
        
        
        public class Baker : Baker<MovementAuthoring>
        {
            public override void Bake(MovementAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Speed
                {
                    Translation = authoring.translation,
                    Rotation = authoring.rotation
                });
                
                AddComponent(entity, new MovementData
                {
                    PercentNextMove = authoring.percentNextMove,
                });
                
                AddComponent<Reckoning>(entity);
                
                AddComponent(entity, new OverlapBox
                {
                    HalfExtends = 0.4f,
                    Offset = float3.zero,
                });
            }
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
    public struct Speed : IComponentData
    {
        [GhostField] public float Translation;
        [GhostField] public float Rotation;
    }

    public struct MovementData : IComponentData
    {
        public float3 Target;
        public float3 Previous;
        public bool IsMoving;
        public bool ShouldCheckCollision;
        public float3 Direction;
        public float PercentNextMove;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
    public struct Reckoning : IComponentData
    {
        [GhostField] public NetworkTick Tick { get; set; }
        [GhostField] public float3 Target;
        [GhostField] public bool ShouldReckon;
    }
}
