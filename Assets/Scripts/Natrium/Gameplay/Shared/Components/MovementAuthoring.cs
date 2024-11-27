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
        
        public class Baker : Baker<MovementAuthoring>
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
            
                AddComponent<MoveClassicTag>(e);
                AddComponent<MoveDiagonalTag>(e);
                AddComponent<MoveFreeTag>(e);
                
                switch (authoring.moveType)
                {
                    default:
                    case MovementTypes.Classic:
                        SetComponentEnabled<MoveClassicTag>(e, true);
                        SetComponentEnabled<MoveDiagonalTag>(e, false);
                        SetComponentEnabled<MoveFreeTag>(e, false);
                        break;
                    case MovementTypes.Diagonal:
                        SetComponentEnabled<MoveClassicTag>(e, false);
                        SetComponentEnabled<MoveDiagonalTag>(e, true);
                        SetComponentEnabled<MoveFreeTag>(e, false);
                        break;
                    case MovementTypes.Free:
                        SetComponentEnabled<MoveClassicTag>(e, false);
                        SetComponentEnabled<MoveDiagonalTag>(e, false);
                        SetComponentEnabled<MoveFreeTag>(e, true);
                        break;
                }
            
                AddComponent<OverlapBox>(e);
                SetComponentEnabled<OverlapBox>(e, false);
                
                AddComponent<MoveTowardsTag>(e);
                SetComponentEnabled<MoveTowardsTag>(e, false);
            }
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
    public struct Speed : IComponentData
    {
        [GhostField]
        public float Value;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
    public struct Position : IComponentData
    {
        [GhostField] public float3 Previous;
        [GhostField] public float3 Target;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
    [GhostEnabledBit]
    public struct MoveFreeTag : IComponentData, IEnableableComponent { }
    
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
    [GhostEnabledBit]
    public struct MoveDiagonalTag : IComponentData, IEnableableComponent { }
    
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
    [GhostEnabledBit]
    public struct MoveClassicTag : IComponentData, IEnableableComponent { }
    
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
    [GhostEnabledBit]
    public struct MoveTowardsTag : IComponentData, IEnableableComponent { }
}
