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
    }

    public class MovementBaker : Baker<MovementAuthoring>
    {
        public override void Bake(MovementAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(e, new Speed
            {
                Value = authoring.speed
            });
            
            AddComponent(e, new Position
            {
                Previous = authoring.transform.position,
                Target = authoring.transform.position,
            });
            
            switch (authoring.moveType)
            {
                default:
                case MovementTypes.Classic:
                    AddComponent<MoveClassicTag>(e);
                    break;
                case MovementTypes.Diagonal:
                    AddComponent<MoveDiagonalTag>(e);
                    break;
                case MovementTypes.Free:
                    AddComponent<MoveFreeTag>(e);
                    break;
            }
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct Speed : IComponentData
    {
        [GhostField(Quantization = 100)]
        public float Value;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct Position : IComponentData
    {
        [GhostField] public float3 Previous;
        [GhostField] public float3 Target;
    }

    public struct MoveFreeTag : IComponentData { }
    public struct MoveDiagonalTag : IComponentData { }
    public struct MoveClassicTag : IComponentData { }
    public struct MoveTowardsTag : IComponentData { }
}
