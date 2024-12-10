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
        public float speed = 5.0f;
        public MovementTypes moveType = MovementTypes.Classic;
        [Range(0.1f, 1.0f)]
        public float percentNextMove = 0.1f;
        
        public class Baker : Baker<MovementAuthoring>
        {
            public override void Bake(MovementAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(e, new Speed
                {
                    Value = authoring.speed
                });
                
                AddComponent<MovementData>(e);
                AddComponent<Reckoning>(e);
                
                AddComponent(e, new OverlapBox
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
        [GhostField] public float Value;
    }

    public struct MovementData : IComponentData
    {
        public int3 Target;
        public int3 Previous;
        public bool IsMoving;
        public bool ShouldCheckCollision;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
    public struct Reckoning : IComponentData
    {
        [GhostField] public NetworkTick Tick { get; set; }
        [GhostField] public int3 Target;
        [GhostField] public bool ShouldReckon;
    }
}
